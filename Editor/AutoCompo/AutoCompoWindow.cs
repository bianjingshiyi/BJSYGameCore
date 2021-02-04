using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Reflection;
using Object = UnityEngine.Object;
#if UNITY_2019
using UnityEditor.Experimental.SceneManagement;
#endif

namespace BJSYGameCore.AutoCompo
{
    public class AutoCompoWindow : EditorWindow
    {
        #region 公共成员
        [MenuItem("Assets/Create/AutoCompo/Generate", true, PRIOR_PREFAB_GENERATE)]
        [MenuItem("GameObject/AutoCompo/Generate", true, PRIOR_SCENE_GENERATE)]
        public static bool onValidateMenuItemGenerate()
        {
            if (Selection.gameObjects.Length != 1)
                return false;
            return true;
        }
        [MenuItem("Assets/Create/AutoCompo/Generate", false, PRIOR_PREFAB_GENERATE)]
        [MenuItem("GameObject/AutoCompo/Generate", false, PRIOR_SCENE_GENERATE)]
        public static void onMenuItemGenerate()
        {
            GetWindow<AutoCompoWindow>(typeof(AutoCompoWindow).Name, true).checkGameObject(Selection.gameObjects.Length > 0 ? Selection.gameObjects[0] : null, true);
        }
        [MenuItem("Assets/Create/AutoCompo/GetPrefab")]
        [MenuItem("GameObject/AutoCompo/GetPrefab")]
        public static void onMenuItemGetPrefab()
        {
            if (Selection.gameObjects.Length != 1)
                return;
            GameObject gameObject = Selection.gameObjects[0];
            PrefabType prefabType = PrefabUtility.GetPrefabType(gameObject);
            switch (prefabType)
            {
                case PrefabType.PrefabInstance:
                    Object source = PrefabUtility.FindPrefabRoot(PrefabUtility.GetPrefabParent(gameObject) as GameObject);
                    Debug.Log("Prefab：" + source);
                    Selection.activeObject = source;
                    break;
                default:
                    Debug.Log(prefabType);
                    break;
            }
        }
        public void checkGameObject(GameObject gameObject, bool forceReselect = false)
        {
            if (gameObject == null || (!forceReselect && _gameObject == gameObject))
                return;
            reset();
            if (_serializedObject == null)
                _serializedObject = new SerializedObject(this);
            GameObject sourceObject = getTargetGameObject(gameObject);
            _serializedObject.FindProperty(NAME_OF_GAMEOBJECT).objectReferenceValue = sourceObject;
            string path = getSavePath4GO(sourceObject);
            _serializedObject.FindProperty(NAME_OF_SAVEPATH).stringValue = Path.GetDirectoryName(path);
            _serializedObject.FindProperty(NAME_OF_SAVEFILENAME).stringValue = Path.GetFileNameWithoutExtension(path);
            _serializedObject.ApplyModifiedProperties();
            if (string.IsNullOrEmpty(_savePath))
            {
                Close();
                throw new DirectoryNotFoundException();
            }
        }
        #endregion
        #region 私有成员
        #region 生命周期
        protected void Awake()
        {
            if (_setting == null)
                _setting = loadSetting();
        }
        protected void OnGUI()
        {
            if (this == null)
                return;
            if (onCompilingWarning())
                return;
            //自动添加组件
            if (!EditorApplication.isCompiling)
            {
                if (onAutoAddComponent())
                {
                    Close();
                    return;
                }
            }
            if (_gameObject == null)
            {
                EditorGUILayout.LabelField("目标GameObject可能未被加载或者已被销毁");
                return;
            }
            if (_generator == null)
                _generator = createGenerator();
            if (_fieldList == null)
            {
                _fieldList = new List<AutoBindFieldInfo>();
                if (_type == null && tryGetExistsType(out _type, out _script))
                    _fieldList.AddRange(getFieldInfosFromType(_type));
            }
            if (_objGenDict == null)
            {
                _objGenDict = new Dictionary<Object, AutoBindFieldInfo>();
                onInitByCtrlType();
            }
            onGUISetting();
            onGUICtrlSettting();
            _onGUIGameObjectScrollPos = EditorGUILayout.BeginScrollView(_onGUIGameObjectScrollPos);
            onGUIGameObject(_gameObject);
            EditorGUILayout.EndScrollView();
            bool confirmGen = false;
            if (GUILayout.Button("保存脚本"))
            {
                if (checkGenInput())
                    confirmGen = true;
            }
            if (GUILayout.Button("另存为脚本"))
            {
                if (checkGenInput())
                {
                    string savePath = EditorUtility.SaveFilePanel("另存为脚本", _savePath, getDefaultTypeName(), "cs");
                    if (!string.IsNullOrEmpty(savePath))
                    {
                        _savePath = Path.GetDirectoryName(savePath);
                        _saveFileName = Path.GetFileNameWithoutExtension(savePath);
                        if (Directory.Exists(_savePath))
                            confirmGen = true;
                    }
                }
            }
            if (confirmGen)
                onGenerate();
        }
        protected void OnDisable()
        {
            if (_serializedObject != null)
            {
                _serializedObject.ApplyModifiedProperties();
                _serializedObject.Dispose();
                _serializedObject = null;
            }
            if (_setting != null)
                _setting.saveToPrefs(getSettingName());
        }
        #endregion
        /// <summary>
        /// 已经存在的类型是通过实例ID唯一定位的。
        /// </summary>
        /// <returns></returns>
        protected virtual bool tryGetExistsType(out Type type, out MonoScript script)
        {
            var scripts = AssetDatabase.FindAssets("t:MonoScript")
                                       .Select(g => AssetDatabase.GUIDToAssetPath(g))
                                       .Select(p => AssetDatabase.LoadAssetAtPath<MonoScript>(p))
                                       .Where(s => s != null);
            foreach (var s in scripts)
            {
                type = s.GetClass();
                if (type == null)
                    continue;
                AutoCompoAttribute autoCompo = type.getAttribute<AutoCompoAttribute>();
                if (autoCompo == null)
                    continue;
                int instanceID = _gameObject.GetInstanceID();
                if (autoCompo.instanceID == instanceID)
                {
                    script = s;
                    return true;
                }
            }
            type = null;
            script = null;
            return false;
        }
        protected virtual void onInitByCtrlType()
        {
            if (_controllerType == AutoCompoGenerator.CTRL_TYPE_BUTTON)
            {
                if (_type != null)
                {
                    foreach (var fieldInfo in _type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
                    {
                        var autoCompo = fieldInfo.getAttribute<AutoCompoAttribute>();
                        if (autoCompo == null || !autoCompo.tags.Contains("mainButton"))
                            continue;
                        GameObject buttonGO = TransformHelper.findGameObjectByPath(_gameObject, autoCompo.path);
                        if (buttonGO == null)
                            continue;
                        _buttonMain = buttonGO.GetComponent<Button>();
                        break;
                    }
                }
            }
        }
        private AutoBindFieldInfo[] getFieldInfosFromType(Type type)
        {
            if (type == null)
                return new AutoBindFieldInfo[0];
            List<AutoBindFieldInfo> infoList = new List<AutoBindFieldInfo>();
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic))
            {
                AutoBindFieldInfo info = getGenInfoFromFieldInfo(field);
                if (info != null)
                {
                    infoList.Add(info);
                }
            }
            return infoList.ToArray();
        }
        /// <summary>
        /// 如果字段有AutoCompo特性，则认为它是自动生成的，从特性中读取元数据生成字段信息。
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        protected virtual AutoBindFieldInfo getGenInfoFromFieldInfo(FieldInfo field)
        {
            AutoCompoAttribute autoCompo = field.getAttribute<AutoCompoAttribute>();
            if (autoCompo != null)
                return new AutoBindFieldInfo(autoCompo.instanceID, autoCompo.path, field.FieldType, null, field.Name);
            return null;
        }

        protected virtual bool checkGenInput()
        {
            if (string.IsNullOrEmpty(_setting.Namespace))
            {
                Debug.LogError("命名空间不能为空");
                return false;
            }
            return true;
        }
        /// <summary>
        /// 默认路径是文件夹/类名.cs
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        protected virtual string getSaveFilePath(string typeName)
        {
            return _savePath + "/" + typeName + ".cs";
        }

        /// <summary>
        /// 绘制关于游戏物体，保存路径，以及设定的GUI。
        /// </summary>
        protected virtual void onGUISetting()
        {
            if (_serializedObject == null)
                _serializedObject = new SerializedObject(this);
            GUI.enabled = false;
            EditorGUILayout.ObjectField("要生成脚本的游戏物体", _gameObject, typeof(GameObject), true);
            GUI.enabled = true;
            MonoScript newScript = EditorGUILayout.ObjectField("脚本对象", _script, typeof(MonoScript), false) as MonoScript;
            if (newScript != _script)
            {
                _script = newScript;
                _fieldList.Clear();
                if (_script != null)
                {
                    _type = _script.GetClass();
                    _fieldList.AddRange(getFieldInfosFromType(_type));
                    string path = AssetDatabase.GetAssetPath(_script);
                    _savePath = Path.GetDirectoryName(path);
                    _saveFileName = _type.Name;
                }
                else
                    _type = null;
            }
            EditorGUILayout.LabelField("保存路径", string.IsNullOrEmpty(_saveFileName) ? _savePath : _savePath + "/" + _saveFileName);
            EditorGUILayout.PropertyField(_serializedObject.FindProperty(NAME_OF_SETTING), new GUIContent("设置"), true);
            if (_ctrlTypes == null || _ctrlTypes.Length < 1)
                _ctrlTypes = new string[] { "none" }.Concat(_generator.ctrlTypes).ToArray();
            SerializedProperty ctrlTypeProp = _serializedObject.FindProperty(NAME_OF_CTRL_TYPE);
            int index = Array.IndexOf(_ctrlTypes, ctrlTypeProp.stringValue);
            if (index < 0)
                index = 0;
            index = EditorGUILayout.Popup("类型", index, _ctrlTypes);
            ctrlTypeProp.stringValue = _ctrlTypes[index];
            _serializedObject.ApplyModifiedProperties();
        }
        void onGUIGameObject(GameObject gameObject)
        {
            EditorGUILayout.BeginHorizontal();
            AutoBindFieldInfo field = getInfoFromExists(gameObject);
            if (field != null && !_objGenDict.ContainsKey(gameObject))
                _objGenDict.Add(gameObject, field);
            onGUIGameObjectField(gameObject, field);
            EditorGUILayout.EndHorizontal();
            if (_objFoldDict[gameObject])
            {
                EditorGUI.indentLevel++;
                foreach (var component in gameObject.GetComponents<Component>())
                {
                    onGUIComponent(component);
                }
                for (int i = 0; i < gameObject.transform.childCount; i++)
                {
                    onGUIGameObject(gameObject.transform.GetChild(i).gameObject);
                }
                EditorGUI.indentLevel--;
            }
        }
        protected virtual void onGUIGameObjectField(GameObject gameObject, AutoBindFieldInfo field)
        {
            EditorGUILayout.BeginHorizontal(GUILayout.Width(WIDTH_OF_FIELD));
            if (canEditGameObjectName(gameObject))
            {
                _objFoldDict[gameObject] = EditorGUILayout.Foldout(_objFoldDict.ContainsKey(gameObject) ? _objFoldDict[gameObject] : isPartOfAnyPath(gameObject), string.Empty, true);
                setSepecifiedFieldName(gameObject, EditorGUILayout.TextField(hasSpecifiedFieldName(gameObject) ?
                    getSpecifiedFieldName(gameObject) :
                    _generator.genFieldName4GO(gameObject)));
            }
            else
                _objFoldDict[gameObject] = EditorGUILayout.Foldout(_objFoldDict.ContainsKey(gameObject) ? _objFoldDict[gameObject] : isPartOfAnyPath(gameObject), gameObject.name, true);
            EditorGUILayout.EndHorizontal();
            if (gameObject != _gameObject)
            {
                bool isOn = isObjectGen(gameObject);
                if (EditorGUILayout.Toggle(isOn) != isOn)
                {
                    isOn = !isOn;
                    if (isOn)
                        _objGenDict[gameObject] = getOrGenField(gameObject);
                    else
                        _objGenDict[gameObject] = null;
                }
                onGUIGameObjectCtrl(gameObject);
            }
        }

        protected virtual bool canEditGameObjectName(GameObject gameObject)
        {
            return isObjectGen(gameObject);
        }
        protected virtual void onGUIGameObjectCtrl(GameObject gameObject)
        {
            if (_controllerType == AutoCompoGenerator.CTRL_TYPE_LIST || _controllerType == AutoCompoGenerator.CTRL_TYPE_BUTTON_LIST)
            {
                if (_listOrigin == null)
                {
                    if (gameObject.transform.parent == _gameObject.transform)
                    {
                        if (EditorGUILayout.Toggle("选为列表项", false))
                        {
                            _listOrigin = gameObject;
                            _objGenDict[gameObject] = getOrGenField(gameObject);
                        }
                    }
                }
                else
                {
                    if (_listOrigin == gameObject)
                    {
                        if (!EditorGUILayout.Toggle("取消列表项", true))
                            _listOrigin = null;
                    }
                }
            }
        }
        void onGUIComponent(Component component)
        {
            EditorGUILayout.BeginHorizontal();
            AutoBindFieldInfo field = getInfoFromExists(component);
            if (field != null && !_objGenDict.ContainsKey(component))
                _objGenDict.Add(component, field);
            EditorGUI.BeginDisabledGroup(field != null && field.instanceId == 0);
            EditorGUILayout.BeginHorizontal(GUILayout.Width(WIDTH_OF_FIELD));
            if (isObjectGen(component))
            {
                setSepecifiedFieldName(component, EditorGUILayout.TextField(hasSpecifiedFieldName(component) ?
                    getSpecifiedFieldName(component) :
                    _generator.genFieldName4Compo(component)));
            }
            else
                EditorGUILayout.LabelField(component.GetType().Name, GUILayout.Width(WIDTH_OF_FIELD));
            EditorGUILayout.EndHorizontal();
            //勾选是否生成组件字段
            onCheckGenObj(component);
            if (_controllerType == AutoCompoGenerator.CTRL_TYPE_BUTTON)
            {
                if (_buttonMain == null)
                {
                    if (component is Button)
                    {
                        if (EditorGUILayout.Toggle("选为主按钮", false))
                        {
                            _buttonMain = component as Button;
                            _objGenDict[component] = getOrGenField(component);
                        }
                    }
                }
                else
                {
                    if (_buttonMain == component)
                    {
                        if (!EditorGUILayout.Toggle("取消主按钮", true))
                            _buttonMain = null;
                    }
                }
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
        }
        private void onCheckGenObj(Object obj)
        {
            bool isGen = _objGenDict.ContainsKey(obj);
            isGen = EditorGUILayout.Toggle(isGen);
            if (isGen)
            {
                if (!_objGenDict.ContainsKey(obj))
                    _objGenDict[obj] = getOrGenField(obj);
            }
            else
            {
                if (_objGenDict.ContainsKey(obj))
                    _objGenDict.Remove(obj);
            }
        }
        protected virtual bool onCompilingWarning()
        {
            if (EditorApplication.isCompiling && _autoAddList.Count > 0)
            {
                Color color = GUI.color;
                GUI.color = Color.red;
                EditorGUILayout.LabelField("脚本将会在编译完成后自动添加，请不要关闭窗口或销毁GameObject");
                GUI.color = color;
                return true;
            }
            else
                return false;
        }
        protected virtual bool onAutoAddComponent()
        {
            bool isAdded = false;
            if (_autoAddList.Count > 0)
            {
                foreach (var info in _autoAddList)
                {
                    if (info.rootGameObject == null)
                    {
                        Debug.LogWarning(info.typeFullName + "要自动添加的物体已经被销毁，自动添加失败");
                        continue;
                    }
                    MonoScript script;
                    if (tryFindScriptByFullName(info.typeFullName, out script))
                    {
                        Type type = script.GetClass();
                        if (info.rootGameObject.GetComponent(type) == null)
                        {
                            info.rootGameObject.AddComponent(type);
                            isAdded = true;
                        }
                    }
                    else
                    {
                        Debug.LogError("未找到要为" + info.rootGameObject + "添加的脚本" + info.typeFullName, info.rootGameObject);
                        continue;
                    }
                }
                _autoAddList.Clear();
            }
            return isAdded;
        }
        bool isObjectGen(Object obj)
        {
            return _objGenDict.ContainsKey(obj) && _objGenDict[obj] != null;
        }
        bool hasSpecifiedFieldName(Object obj)
        {
            return _objGenDict.ContainsKey(obj) &&
                    _objGenDict[obj] != null &&
                    !string.IsNullOrEmpty(_objGenDict[obj].fieldName);
        }
        string getSpecifiedFieldName(Object obj)
        {
            return _objGenDict[obj].fieldName;
        }
        void setSepecifiedFieldName(Object obj, string fieldName)
        {
            if (string.IsNullOrEmpty(fieldName))
                return;
            if (obj is GameObject && (obj as GameObject).name == fieldName)
                return;
            if (obj is Component && (obj as Component).gameObject.name == fieldName)
                return;
            _objGenDict[obj].fieldName = fieldName;
        }
        protected AutoBindFieldInfo getOrGenField(Object obj)
        {
            if (_objGenDict.ContainsKey(obj) && _objGenDict[obj] != null)
                return _objGenDict[obj];
            AutoBindFieldInfo fieldInfo = new AutoBindFieldInfo(
                obj.GetInstanceID(),
                getPath(obj is GameObject ? obj as GameObject : (obj as Component).gameObject),
                obj.GetType(),
                null,
                null);
            return fieldInfo;
        }
        string getPath(GameObject gameObject)
        {
            string path = string.Empty;
            Transform transform = gameObject.transform;
            while (transform.gameObject != _gameObject)
            {
                if (transform == null)
                    return null;
                path = "/" + transform.gameObject.name + path;
                transform = transform.parent;
            }
            if (string.IsNullOrEmpty(path))
                path = "/";
            path = "." + path;
            return path;
        }
        AutoBindFieldInfo getInfoFromExists(Object obj)
        {
            if (obj == null)
                return null;
            //if (_objGenDict.ContainsKey(obj) && _objGenDict[obj] != null)
            //    return _objGenDict[obj];
            if (_fieldList == null)
                return null;
            foreach (var field in _fieldList.Where(f => f.targetType == obj.GetType()))
            {
                if (string.IsNullOrEmpty(field.path))
                    continue;
                GameObject gameObject = obj is GameObject ? obj as GameObject : (obj as Component).gameObject;
                if (TransformHelper.isPathMatch(field.path, gameObject, _gameObject))
                    return field;
            }
            return null;
        }
        protected virtual bool isPartOfAnyPath(GameObject gameObject)
        {
            if (_fieldList != null)
            {
                foreach (var field in _fieldList)
                {
                    if (string.IsNullOrEmpty(field.path))
                        continue;
                    if (TransformHelper.isPartOfPath(gameObject, _gameObject, field.path))
                        return true;
                }
            }
            return false;
        }
        AutoCompoGenSetting loadSetting()
        {
            AutoCompoGenSetting setting = new AutoCompoGenSetting();
            setting.loadFromPrefs(getSettingName());
            return setting;
        }
        protected virtual string getSettingName()
        {
            return "Default";
        }
        /// <summary>
        /// 默认保存到gameObject所在的预制件或者场景的同级目录下。
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        protected virtual string getSavePath4GO(GameObject gameObject)
        {
            string path;
#if UNITY_2017
            path = AssetDatabase.GetAssetPath(gameObject);
#else
            if (PrefabUtility.IsPartOfNonAssetPrefabInstance(gameObject))
            {
                GameObject prefabInstanceRoot = PrefabUtility.GetOutermostPrefabInstanceRoot(gameObject);
                GameObject prefabRoot = PrefabUtility.GetCorrespondingObjectFromSource(prefabInstanceRoot);
                path = AssetDatabase.GetAssetPath(prefabRoot);
            }
            else
            {
                path = gameObject.scene.path;
                if (string.IsNullOrEmpty(path))
                {
                    path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(gameObject);
                }
            }
#endif
            bool willBeOverride;
            if (tryFindExistScript(gameObject, out _script, out willBeOverride))
            {
                if (willBeOverride)
                {
                    if (EditorUtility.DisplayDialog("已存在脚本", "AutoCompo发现" + gameObject.name + "上已经有一个同名的脚本，是否覆盖该脚本？", "是", "否"))
                    {
                        _type = _script.GetClass();
                        return AssetDatabase.GetAssetPath(_script);
                    }
                    else
                        return path;
                }
                else
                {
                    _type = _script.GetClass();
                    return AssetDatabase.GetAssetPath(_script);
                }
            }
            else
                return path;
        }
        protected virtual void onGUICtrlSettting()
        {
            if (_controllerType == AutoCompoGenerator.CTRL_TYPE_LIST || _controllerType == AutoCompoGenerator.CTRL_TYPE_BUTTON_LIST)
            {
                _listItemTypeScript = EditorGUILayout.ObjectField("列表项类型", _listItemTypeScript, typeof(MonoScript), false) as MonoScript;
            }
        }
        protected virtual AutoCompoGenerator createGenerator()
        {
            AutoCompoGenerator generator = new AutoCompoGenerator();
            generator.rootGameObject = _gameObject;
            return generator;
        }
        /// <summary>
        /// 如果已经存在类型，那么重新生成该类型，如果不存在则生成和GameObject同名的脚本。
        /// </summary>
        protected virtual void onGenerate()
        {
            _generator.typeName = getDefaultTypeName();
            _generator.objFieldDict = _objGenDict;
            _generator.controllerType = _controllerType;
            _generator.buttonMain = _buttonMain;
            _generator.listOrigin = _listOrigin;
            if (_listItemTypeScript != null)
                _generator.listItemTypeName = _listItemTypeScript.GetClass().Name;
            var unit = _generator.genScript4GO(_gameObject, _setting);
            FileInfo fileInfo = new FileInfo(getSaveFilePath(unit.Namespaces[0].Types[0].Name));
            CodeDOMHelper.writeUnitToFile(fileInfo, unit);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            _autoAddList.Add(new AutoAddCompoInfo(_gameObject, _setting.Namespace + "." + _generator.typeName));
        }
        private string getDefaultTypeName()
        {
            if (!string.IsNullOrEmpty(_saveFileName))
                return _saveFileName;
            if (_type != null)
                return _type.Name;
            return _generator.genTypeName4GO(_gameObject);
        }
        #region 逻辑层
        /// <summary>
        /// 获取目标GameObject，如果目标是Prefab或者Prefab的实例，则返回PrefabAsset。
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        protected virtual GameObject getTargetGameObject(GameObject gameObject)
        {
#if UNITY_2017
            PrefabType prefabType = PrefabUtility.GetPrefabType(gameObject);
            switch (prefabType)
            {
                case PrefabType.PrefabInstance:
                case PrefabType.DisconnectedPrefabInstance:
                    return PrefabUtility.GetPrefabParent(gameObject) as GameObject;
                case PrefabType.Prefab:
                    return gameObject;
                default:
                    Debug.Log("未处理的Prefab类型" + prefabType);
                    throw new NotImplementedException();
            }
#else
            GameObject source;
            var assetType = PrefabUtility.GetPrefabAssetType(gameObject);
            var status = PrefabUtility.GetPrefabInstanceStatus(gameObject);
            if (string.IsNullOrEmpty(gameObject.scene.path))
            {
                //Prefab，Assets或者编辑场景
                Debug.Log(gameObject + "目标资源类型：" + assetType + "，目标状态：" + status, gameObject);
                if (assetType == PrefabAssetType.NotAPrefab && status == PrefabInstanceStatus.NotAPrefab)
                {
                    //PrefabStage
                    PrefabStage stage = PrefabStageUtility.GetCurrentPrefabStage();
                    string path = stage.prefabContentsRoot.transform.getChildPath(gameObject.transform);
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(stage.prefabAssetPath);
                    source = prefab.transform.getChildAt(path).gameObject;
                }
                else if (assetType == PrefabAssetType.Regular)
                    source = gameObject;
                else
                    source = PrefabUtility.GetCorrespondingObjectFromSource(gameObject);
            }
            else
            {
                //场景物体
                if (assetType == PrefabAssetType.MissingAsset || assetType == PrefabAssetType.NotAPrefab)
                    source = gameObject;
                else
                    source = PrefabUtility.GetCorrespondingObjectFromSource(gameObject);
            }
            if (source != null)
            {
                assetType = PrefabUtility.GetPrefabAssetType(gameObject);
                status = PrefabUtility.GetPrefabInstanceStatus(gameObject);
                Debug.Log(source + "来源资源类型：" + assetType + "，来源状态：" + status, source);
            }
            else
                Debug.LogError("找不到源物体");
            return source;
#endif
        }
        protected virtual bool tryFindExistScript(GameObject gameObject, out MonoScript script, out bool willBeOverride)
        {
            var scripts = AssetDatabase.FindAssets("t:MonoScript")
                           .Select(g => AssetDatabase.GUIDToAssetPath(g))
                           .Select(p => AssetDatabase.LoadAssetAtPath<MonoScript>(p))
                           .Where(s => s != null);
            foreach (var s in scripts)
            {
                Type type = s.GetClass();
                if (type == null)
                    continue;
                AutoCompoAttribute autoCompo = type.getAttribute<AutoCompoAttribute>();
                int instanceID = gameObject.GetInstanceID();
                if (autoCompo != null && autoCompo.instanceID == instanceID)
                {
                    script = s;
                    willBeOverride = false;
                    return true;
                }
                if ((type.IsSubclassOf(typeof(MonoBehaviour)) || type.IsSubclassOf(typeof(Component))) &&
                    gameObject.GetComponent(type) != null &&
                    s.text.Contains("// <auto-generated>"))
                {
                    script = s;
                    willBeOverride = true;
                    return true;
                }
            }
            script = null;
            willBeOverride = false;
            return false;
        }
        protected bool tryFindScriptByFullName(string typeFullName, out MonoScript script)
        {
            int index = typeFullName.LastIndexOf('.');
            string typeName;
            if (index < 0)
                typeName = typeFullName;
            else
                typeName = typeFullName.Substring(index + 1, typeFullName.Length - index - 1);
            var scripts = AssetDatabase.FindAssets(typeName + " t:" + typeof(MonoScript).Name)
                .Select(g => AssetDatabase.GUIDToAssetPath(g))
                .Select(p => AssetDatabase.LoadAssetAtPath<MonoScript>(p))
                .Where(s => s != null);
            foreach (var s in scripts)
            {
                Type type = s.GetClass();
                if (type != null && type.FullName == typeFullName)
                {
                    script = s;
                    return true;
                }
            }
            script = null;
            return false;
        }
        #endregion
        void reset()
        {
            _objGenDict = null;
            _objFoldDict.Clear();
            if (_serializedObject != null)
                _serializedObject.Dispose();
            _serializedObject = null;
        }
        protected Type _type = null;
        [SerializeField] protected MonoScript _script = null;
        [Obsolete("应该只有一个生成字段信息")]
        List<AutoBindFieldInfo> _fieldList = null;
        protected AutoCompoGenerator _generator = null;
        string[] _ctrlTypes = null;
        Vector2 _onGUIGameObjectScrollPos;
        Dictionary<Object, bool> _objFoldDict = new Dictionary<Object, bool>();
        protected Dictionary<Object, AutoBindFieldInfo> _objGenDict;
        [SerializeField]
        protected GameObject _gameObject;
        [SerializeField]
        protected string _savePath;
        /// <summary>
        /// 保存文件名，同时也被当做默认的类名来使用。
        /// </summary>
        [SerializeField]
        protected string _saveFileName;
        [SerializeField]
        protected AutoCompoGenSetting _setting = null;
        [SerializeField]
        protected string _controllerType;
        //按钮
        [SerializeField]
        protected Button _buttonMain;
        //列表
        [SerializeField]
        protected GameObject _listOrigin;
        [SerializeField]
        protected MonoScript _listItemTypeScript;
        SerializedObject _serializedObject;
        [SerializeField]
        protected List<AutoAddCompoInfo> _autoAddList = new List<AutoAddCompoInfo>();
        protected const int PRIOR_SCENE_GENERATE = 15;
        protected const int PRIOR_PREFAB_GENERATE = 81;
        const string NAME_OF_GAMEOBJECT = "_gameObject";
        const string NAME_OF_SAVEPATH = "_savePath";
        const string NAME_OF_SAVEFILENAME = "_saveFileName";
        const string NAME_OF_SETTING = "_setting";
        const string NAME_OF_CTRL_TYPE = "_controllerType";
        const int WIDTH_OF_FIELD = 200;
        const int WIDTH_OF_TOGGLE = 25;
        #endregion
    }
    [Serializable]
    public class AutoAddCompoInfo
    {
        public GameObject rootGameObject;
        public string typeFullName;
        public AutoAddCompoInfo(GameObject rootGameObject, string typeFullName)
        {
            this.rootGameObject = rootGameObject;
            this.typeFullName = typeFullName;
        }
    }
}