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
            //初始化
            if (_serializedObject == null)
                _serializedObject = new SerializedObject(this);//序列化对象
            if (_generator == null)
            {
                _generator = createGenerator();//生成器
                loadEditorSettings();//编辑器设置
                _objGenDict = new Dictionary<Object, AutoBindFieldInfo>();
                if (script == null)//已存在类型，脚本，自动生成脚本
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
                initCtrl();//初始化控制器相关
                onInitByCtrlType();
            }
            onGUISetting();
            _serializedObject.ApplyModifiedProperties();
            onGUICtrlSettting();
            _onGUIGameObjectScrollPos = EditorGUILayout.BeginScrollView(_onGUIGameObjectScrollPos);
            //onGUIGameObject(_gameObject);
            onGUIGenFields();
            EditorGUILayout.EndScrollView();
            GUILayout.Box("将GameObject或者Component拖拽到这里来添加自动生成字段", GUILayout.Height(64));
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
            if (_removeField != null)
            {
                Object key = _objGenDict.Where(p => p.Value == _removeField).FirstOrDefault().Key;
                if (key != null)
                    _objGenDict.Remove(key);
                else
                    _missingObjGenList.Remove(_removeField);
                _removeField = null;
            }
            if (_redirObjField.Key != null)
            {
                _missingObjGenList.Remove(_redirObjField.Value);
                _objGenDict.Add(_redirObjField.Key, _redirObjField.Value);
                _redirObjField = default(KeyValuePair<Object, AutoBindFieldInfo>);
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
                    string typeName = _type != null ? _type.Name : _gameObject.name;
                    savePath = EditorUtility.SaveFilePanel("另存为脚本",
                        Path.GetDirectoryName(getSaveFilePath(typeName)), typeName, "cs");
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
            else
            {
                //主控制器类型
                if (_mainCtrlTypeNames.Length > 0)
                {
                    _selectedMainCtrlTypeIndex = EditorGUILayout.Popup(_selectedMainCtrlTypeIndex, _mainCtrlTypeNames);
                }
            }
            //if (_ctrlTypes == null || _ctrlTypes.Length < 1)
            //    _ctrlTypes = new string[] { "none" }.Concat(_generator.ctrlTypes).ToArray();
            //SerializedProperty ctrlTypeProp = _serializedObject.FindProperty(NAME_OF_CTRL_TYPE);
            //int index = Array.IndexOf(_ctrlTypes, ctrlTypeProp.stringValue);
            //if (index < 0)
            //    index = 0;
            //index = EditorGUILayout.Popup("类型", index, _ctrlTypes);
            //ctrlTypeProp.stringValue = _ctrlTypes[index];
        }
        protected virtual void onGUIGenFields()
        {
            EditorGUILayout.LabelField("要自动生成的字段：");
            if (_objGenDict.Count > 0 || _missingObjGenList.Count > 0)
            {
                foreach (var pair in _objGenDict)
                {
                    onGUIGenField(pair.Key, pair.Value);
                }
                foreach (var field in _missingObjGenList)
                {
                    onGUIGenField(null, field);
                }
            }
        }
        protected virtual void onGUIGenField(Object obj, AutoBindFieldInfo field)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(!field.isGenerated);
            if (obj != null)
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.ObjectField(obj, typeof(Object), true);
                EditorGUI.EndDisabledGroup();
            }
            else
            {
                Object redirObj = EditorGUILayout.ObjectField("丢失对象", null, field.targetType, true);
                if (redirObj != null)
                    setRedirObjField(field, redirObj);
            }
            if (obj != null)
                setSepecifiedFieldName(obj, EditorGUILayout.TextField(getSpecifiedFieldName(obj)));
            else
                EditorGUILayout.LabelField(field.fieldName);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            if (obj is RectTransform)
                onGUIGenRectTransform(obj as RectTransform, field);
            EditorGUILayout.Space();
            if (GUILayout.Button("-", GUILayout.Width(25)))
                _removeField = field;
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
        }

        private void setRedirObjField(AutoBindFieldInfo field, Object redirObj)
        {
            GameObject redirGameObject = redirObj is GameObject ? (GameObject)redirObj : ((Component)redirObj).gameObject;
            GameObject prefabGameObject = getTargetGameObject(redirGameObject);
            if (prefabGameObject == null)
            {
                Debug.LogError(redirGameObject + "不是" + _gameObject + "的一部分", redirGameObject);
                return;
            }
            redirObj = redirObj is GameObject ? (Object)prefabGameObject : prefabGameObject.GetComponent(redirObj.GetType());
            field.path = _gameObject.transform.getChildPath(prefabGameObject.transform);
            _redirObjField = new KeyValuePair<Object, AutoBindFieldInfo>(redirObj, field);
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
            if (_missingObjGenList != null)
                _missingObjGenList.Clear();
            if (_serializedObject != null)
                _serializedObject.Dispose();
            _serializedObject = null;
        }
        string[] _mainCtrlTypeNames;
        int _selectedMainCtrlTypeIndex;
        Vector2 _onGUIGameObjectScrollPos;
        AutoBindFieldInfo _removeField = null;
        KeyValuePair<Object, AutoBindFieldInfo> _redirObjField = default(KeyValuePair<Object, AutoBindFieldInfo>);
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