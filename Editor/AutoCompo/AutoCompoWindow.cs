using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System;
using UnityEngine.UI;
using System.Collections.Generic;
using Object = UnityEngine.Object;
using System.Diagnostics.CodeAnalysis;
// ReSharper disable InconsistentNaming
#if UNITY_2019
#endif

namespace BJSYGameCore.AutoCompo
{
    public partial class AutoCompoWindow : EditorWindow
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
#if UNITY_2017
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
#else
        [MenuItem("Assets/Create/AutoCompo/GetPrefabInfo", priority = PRIOR_PREFAB_GETPREFABINFO)]
        [MenuItem("GameObject/AutoCompo/GetPrefabInfo", priority = PRIOR_SCENE_GETPREFABINFO)]
        public static void onMenuItemGetPrefab()
        {
            if (Selection.gameObjects.Length != 1)
                return;
            GameObject gameObject = Selection.gameObjects[0];
            getSourcePrefab42019(gameObject, out bool isSceneObject, out GameObject sourceRoot, out string childPath);
            if (isSceneObject)
            {
                Debug.Assert(sourceRoot == null);
                Debug.Assert(string.IsNullOrEmpty(childPath));
                Debug.Log(gameObject + "是场景物体", gameObject);
            }
            else
            {
                string assetPath = AssetDatabase.GetAssetPath(sourceRoot);
                string guid = AssetDatabase.AssetPathToGUID(assetPath);
                Debug.Log(gameObject + "不是场景物体，Prefab为" + sourceRoot + "，相对路径：" + childPath + "，Asset路径：" + assetPath + "，GUID：" + guid, sourceRoot);
            }
        }
#endif
        [MenuItem("CONTEXT/MonoBehaviour/Generate", true)]
        public static bool onValidateMenuItemGenerate(MenuCommand command)
        {
            MonoBehaviour compo = command.context as MonoBehaviour;
            if (compo == null)
                return false;
            MonoScript script = MonoScript.FromMonoBehaviour(compo);
            if (script == null)
                return false;
            Type type = script.GetClass();
            if (type == null)
                return false;
            return true;
        }
        [MenuItem("CONTEXT/MonoBehaviour/Generate", false)]
        public static void onMenuItemGenerate(MenuCommand command)
        {
            MonoBehaviour compo = command.context as MonoBehaviour;
            if (compo == null)
                return;
            MonoScript script = MonoScript.FromMonoBehaviour(compo);
            if (script == null)
                return;
            Type type = script.GetClass();
            if (type == null)
                return;
            GetWindow<AutoCompoWindow>(typeof(AutoCompoWindow).Name, true).init(compo.gameObject, script, type);
        }
        #endregion
        #region 私有成员
        #region 生命周期
        [SuppressMessage("Style", "IDE0018:内联变量声明", Justification = "<挂起>")]
        [SuppressMessage("Style", "IDE0059:不需要赋值", Justification = "<挂起>")]
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
            {
                //初始化
                _generator = createGenerator();
                if (_serializedObject == null)
                    _serializedObject = new SerializedObject(this);
                loadEditorSettings();
                _objGenDict = new Dictionary<Object, AutoBindFieldInfo>();
                if (script == null)
                {
                    bool willBeOverride;
                    MonoScript script;
                    if (tryFindExistScript(_gameObject, out script, out willBeOverride))
                    {
                        if (willBeOverride)
                        {
                            if (EditorUtility.DisplayDialog("已存在脚本", "AutoCompo发现" + _gameObject.name + "上已经有一个同名的脚本，是否覆盖该脚本？",
                                "是", "否"))
                            {
                                this.script = script;
                                _type = this.script.GetClass();
                                _autoScripts = tryFindAllAutoScript(this.script);
                                resetGenDictByType(_type);
                            }
                        }
                        else
                        {
                            this.script = script;
                            _type = this.script.GetClass();
                            _autoScripts = tryFindAllAutoScript(this.script);
                            resetGenDictByType(_type);
                        }
                    }
                }
                else
                {
                    _type = script.GetClass();
                    _autoScripts = tryFindAllAutoScript(script);
                    resetGenDictByType(_type);
                }
                onInitByCtrlType();
            }
            onGUISetting();
            onGUICtrlSettting();
            _onGUIGameObjectScrollPos = EditorGUILayout.BeginScrollView(_onGUIGameObjectScrollPos);
            //onGUIGameObject(_gameObject);
            onGUIGenFields();
            EditorGUILayout.EndScrollView();
            Rect dragArea = GUILayoutUtility.GetLastRect();
            if (dragArea.Contains(Event.current.mousePosition))
            {
                if (Event.current.type == EventType.MouseDrag || Event.current.type == EventType.DragUpdated)
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
                }
                else if (Event.current.type == EventType.DragPerform || Event.current.type == EventType.DragExited)
                {
                    if (DragAndDrop.objectReferences.Length > 0)
                    {
                        foreach (Object obj in DragAndDrop.objectReferences)
                        {
                            if (obj is GameObject)
                                addGenField(obj as GameObject);
                            else if (obj is Component)
                                addGenField(obj as Component);
                        }
                        DragAndDrop.AcceptDrag();
                    }
                }
            }
            if (_removeObject != null)
            {
                _objGenDict.Remove(_removeObject);
                _removeObject = null;
            }
            bool confirmGen = false;
            string savePath = null;
            if (GUILayout.Button("保存脚本"))
            {
                if (checkGenInput())
                    confirmGen = true;
            }
            if (GUILayout.Button("另存为脚本"))
            {
                if (checkGenInput())
                {
                    savePath = EditorUtility.SaveFilePanel("另存为脚本",
                        Path.GetDirectoryName(getSaveFilePath(_gameObject.name)),
                        _type != null ? _type.Name : _gameObject.name, "cs");
                    if (!string.IsNullOrEmpty(savePath) && Directory.Exists(Path.GetDirectoryName(savePath)))
                        confirmGen = true;
                }
            }
            if (confirmGen)
                onGenerate(savePath);
        }
        protected void OnDisable()
        {
            if (_serializedObject != null)
            {
                _serializedObject.ApplyModifiedProperties();
                _serializedObject.Dispose();
                _serializedObject = null;
            }
            saveEditorSettings();
        }
        #endregion
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
        /// 绘制关于游戏物体，保存路径，以及设定的GUI。
        /// </summary>
        protected virtual void onGUISetting()
        {
            if (_serializedObject == null)
                _serializedObject = new SerializedObject(this);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("要生成脚本的游戏物体", _gameObject, typeof(GameObject), true);
            EditorGUI.EndDisabledGroup();
            MonoScript newScript = EditorGUILayout.ObjectField("脚本对象", script, typeof(MonoScript), false) as MonoScript;
            if (newScript != script)
            {
                script = newScript;
                _serializedObject.FindProperty(NAME_OF_SCRIPT).objectReferenceValue = script;
                if (script != null)
                {
                    _type = script.GetClass();
                    resetGenDictByType(_type);
                }
                else
                    _type = null;
            }
            if (_autoScripts != null && _autoScripts.Length > 0)
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.LabelField("已自动生成脚本：");
                foreach (MonoScript autoScript in _autoScripts)
                {
                    EditorGUILayout.ObjectField(autoScript, typeof(MonoScript), false);
                }
                EditorGUI.EndDisabledGroup();
            }
            if (_type == null)
                EditorGUILayout.PropertyField(_serializedObject.FindProperty(NAME_OF_SETTING), new GUIContent("设置"), true);
            //if (_ctrlTypes == null || _ctrlTypes.Length < 1)
            //    _ctrlTypes = new string[] { "none" }.Concat(_generator.ctrlTypes).ToArray();
            //SerializedProperty ctrlTypeProp = _serializedObject.FindProperty(NAME_OF_CTRL_TYPE);
            //int index = Array.IndexOf(_ctrlTypes, ctrlTypeProp.stringValue);
            //if (index < 0)
            //    index = 0;
            //index = EditorGUILayout.Popup("类型", index, _ctrlTypes);
            //ctrlTypeProp.stringValue = _ctrlTypes[index];
            _serializedObject.ApplyModifiedProperties();
        }
        void onGUIGameObject(GameObject gameObject)
        {
            EditorGUILayout.BeginHorizontal();
            AutoBindFieldInfo field = getInfoFromExists(gameObject);
            //if (field != null && !_objGenDict.ContainsKey(gameObject))
            //    _objGenDict.Add(gameObject, field);
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
        protected virtual void onGUIGenFields()
        {
            EditorGUILayout.LabelField("要自动生成的字段：");
            if (_objGenDict.Count > 0)
            {
                foreach (var pair in _objGenDict)
                {
                    onGUIGenField(pair.Key, pair.Value);
                }
            }
            else
            {
                GUILayout.Box("将GameObject或者Component拖拽到这里来添加自动生成字段", GUILayout.Height(128));
            }
        }
        protected virtual void onGUIGenField(Object obj, AutoBindFieldInfo field)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField(obj, typeof(Object), false);
            EditorGUI.EndDisabledGroup();
            setSepecifiedFieldName(obj, EditorGUILayout.TextField(getSpecifiedFieldName(obj)));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            if (obj is RectTransform)
                onGUIGenRectTransform(obj as RectTransform, field);
            EditorGUILayout.Space();
            if (GUILayout.Button("-", GUILayout.Width(25)))
                _removeObject = obj;
            EditorGUILayout.EndHorizontal();
        }
        void onGUIGenRectTransform(RectTransform transform, AutoBindFieldInfo field)
        {
            //如果是RectTransform，那么可以当列表。
            bool isList = field.getValueOrDefault<bool>("isList");
            if (EditorGUILayoutHelper.toggle("列表", isList, out isList))
            {
                field.propDict["isList"] = isList;
            }
            if (isList)
            {
                GameObject template = field.getValueOrDefault<GameObject>("template");
                if (EditorGUILayoutHelper.objectField("列表模板", template, out template, true))
                {
                    field.propDict["template"] = template;
                }
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
            //if (field != null && !_objGenDict.ContainsKey(component))
            //    _objGenDict.Add(component, field);
            EditorGUI.BeginDisabledGroup(field != null && !field.isGenerated);
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
        void onCheckGenObj(Object obj)
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
        protected virtual void onGUICtrlSettting()
        {
            if (_controllerType == AutoCompoGenerator.CTRL_TYPE_LIST || _controllerType == AutoCompoGenerator.CTRL_TYPE_BUTTON_LIST)
            {
                _listItemTypeScript = EditorGUILayout.ObjectField("列表项类型", _listItemTypeScript, typeof(MonoScript), false) as MonoScript;
            }
        }
        void reset()
        {
            if (_objGenDict != null)
                _objGenDict.Clear();
            if (_objFoldDict != null)
                _objFoldDict.Clear();
            if (_serializedObject != null)
                _serializedObject.Dispose();
            _serializedObject = null;
        }
        Vector2 _onGUIGameObjectScrollPos;
        readonly Dictionary<Object, bool> _objFoldDict = new Dictionary<Object, bool>();
        Object _removeObject = null;
        protected const int PRIOR_SCENE_GENERATE = 15;
        protected const int PRIOR_SCENE_GETPREFABINFO = 16;
        protected const int PRIOR_PREFAB_GENERATE = 81;
        protected const int PRIOR_PREFAB_GETPREFABINFO = 82;
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