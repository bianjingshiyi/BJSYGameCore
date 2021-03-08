using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Reflection;
using Object = UnityEngine.Object;
using System.Diagnostics.CodeAnalysis;
// ReSharper disable InconsistentNaming
#if UNITY_2019
using UnityEditor.Experimental.SceneManagement;
#endif

namespace BJSYGameCore.AutoCompo
{
    partial class AutoCompoWindow
    {
        [SuppressMessage("Style", "IDE0018:内联变量声明", Justification = "<挂起>")]
        public void checkGameObject(GameObject gameObject, bool forceReselect = false)
        {
            if (gameObject == null || (!forceReselect && _gameObject == gameObject))
                return;
            reset();
            if (_serializedObject == null)
                _serializedObject = new SerializedObject(this);
            _gameObject = getTargetGameObject(gameObject);
            _serializedObject.FindProperty(NAME_OF_GAMEOBJECT).objectReferenceValue = _gameObject;
            //bool willBeOverride;
            //if (tryFindExistScript(_gameObject, out _script, out willBeOverride))
            //    _type = _script.GetClass();
            //_serializedObject.FindProperty(NAME_OF_SCRIPT).objectReferenceValue = _script;
            string path = getSavePath4GO(_gameObject);
            _savePath = Path.GetDirectoryName(path);
            _serializedObject.FindProperty(NAME_OF_SAVEPATH).stringValue = _savePath;
            _saveFileName = Path.GetFileNameWithoutExtension(path);
            _serializedObject.FindProperty(NAME_OF_SAVEFILENAME).stringValue = _saveFileName;
            _serializedObject.ApplyModifiedProperties();
            if (string.IsNullOrEmpty(_savePath))
            {
                Close();
                throw new DirectoryNotFoundException();
            }
        }
        public void init(GameObject gameObject, MonoScript script, Type type)
        {
            if (gameObject == null)
            {
                Close();
                throw new ArgumentNullException("gameObject");
            }
            if (script == null)
            {
                Close();
                throw new ArgumentNullException("script");
            }
            reset();
            if (_serializedObject == null)
                _serializedObject = new SerializedObject(this);
            _gameObject = getTargetGameObject(gameObject);
            _serializedObject.FindProperty(NAME_OF_GAMEOBJECT).objectReferenceValue = _gameObject;
            _script = script;
            _serializedObject.FindProperty(NAME_OF_SCRIPT).objectReferenceValue = _script;
            _type = type;
            _serializedObject.ApplyModifiedProperties();
        }
        void loadEditorSettings()
        {
            if (_setting == null)
            {
                _setting = new AutoCompoGenSetting();
                _setting.loadFromPrefs(PlayerSettings.productName);
            }
        }
        protected virtual AutoCompoGenerator createGenerator()
        {
            AutoCompoGenerator generator = new AutoCompoGenerator();
            generator.rootGameObject = _gameObject;
            return generator;
        }
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
            return getSourcePrefab42019(gameObject, out bool isSceneObject, out GameObject sourceObject, out string relativePath);
#endif
        }
#if !UNITY_2017
        protected static GameObject getSourcePrefab42019(GameObject gameObject, out bool isSceneObject, out GameObject sourceRoot, out string childPath)
        {
            GameObject source;
            PrefabAssetType assetType = PrefabUtility.GetPrefabAssetType(gameObject);
            PrefabInstanceStatus status = PrefabUtility.GetPrefabInstanceStatus(gameObject);
            PrefabStage stage = PrefabStageUtility.GetCurrentPrefabStage();
            if (stage != null && stage.IsPartOfPrefabContents(gameObject))
            {
                //编辑场景
                isSceneObject = false;
                childPath = stage.prefabContentsRoot.transform.getChildPath(gameObject.transform);
                sourceRoot = AssetDatabase.LoadAssetAtPath<GameObject>(stage.prefabAssetPath);
                source = sourceRoot.transform.Find(childPath).gameObject;
            }
            else if (string.IsNullOrEmpty(gameObject.scene.path))
            {
                //Assets
                isSceneObject = false;
                source = gameObject;
                sourceRoot = source.transform.root.gameObject;
                childPath = sourceRoot.transform.getChildPath(source.transform);
                //switch (assetType)
                //{
                //    case PrefabAssetType.Regular:
                //    case PrefabAssetType.Variant:
                //    case PrefabAssetType.Model:
                //    case PrefabAssetType.MissingAsset:
                //    case PrefabAssetType.NotAPrefab:
                //    default:
                //        break;
                //}
            }
            else
            {
                //场景物体
                switch (status)
                {
                    case PrefabInstanceStatus.Connected:
                        switch (assetType)
                        {
                            case PrefabAssetType.Regular:
                            case PrefabAssetType.Model:
                                isSceneObject = false;
                                source = PrefabUtility.GetCorrespondingObjectFromSource(gameObject);
                                sourceRoot = source.transform.root.gameObject;
                                childPath = sourceRoot.transform.getChildPath(source.transform);
                                break;
                            case PrefabAssetType.Variant:
                                isSceneObject = false;
                                source = PrefabUtility.GetCorrespondingObjectFromSource(gameObject);
                                sourceRoot = source.transform.root.gameObject;
                                childPath = sourceRoot.transform.getChildPath(source.transform);
                                break;
                            case PrefabAssetType.MissingAsset:
                            case PrefabAssetType.NotAPrefab:
                            default:
                                isSceneObject = true;
                                source = gameObject;
                                sourceRoot = null;
                                childPath = null;
                                break;
                        }
                        break;
                    case PrefabInstanceStatus.NotAPrefab:
                    case PrefabInstanceStatus.MissingAsset:
                    case PrefabInstanceStatus.Disconnected:
                    default:
                        isSceneObject = true;
                        source = gameObject;
                        sourceRoot = null;
                        childPath = null;
                        break;
                }
            }
            return source;
        }
#endif
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
                    s.text.Contains("<auto-generated>"))
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
        protected MonoScript[] tryFindAllAutoScript(MonoScript script)
        {
            if (script == null)
                return new MonoScript[0];
            return EditorHelper.tryFindScripts(s => s.text.Contains("<auto-generated>") && s.text.Contains("partial class " + script.GetClass().Name));
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
        void resetGenDictByType(Type type)
        {
            _objGenDict.Clear();
            if (type == null)
                return;
            foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic))
            {
                var pair = getGenInfoFromFieldInfo(field);
                if (pair.Key != null)
                    _objGenDict.Add(pair.Key, pair.Value);
            }
        }
        /// <summary>
        /// 如果字段有AutoCompo特性，则认为它是自动生成的，从特性中读取元数据生成字段信息。
        /// 如果已经存在组件，字段在自动生成的文件里声明，并且有对应的属性可以来获取相关信息，则认为它是自动生成的。
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        [SuppressMessage("Style", "IDE0034:简化 \"default\" 表达式", Justification = "<挂起>")]
        protected virtual KeyValuePair<Object, AutoBindFieldInfo> getGenInfoFromFieldInfo(FieldInfo field)
        {
            AutoCompoAttribute autoCompoAttr = field.getAttribute<AutoCompoAttribute>();
            if (autoCompoAttr != null)
            {
                //先用InstanceID找
                GameObject gameObject = findGameObjectByInstanceID(_gameObject, autoCompoAttr.instanceID);
                if (gameObject != null)
                {
                    if (field.FieldType != typeof(GameObject))
                    {
                        Component component = gameObject.GetComponent(field.FieldType);
                        if (component != null)
                            return new KeyValuePair<Object, AutoBindFieldInfo>(component, new AutoBindFieldInfo(autoCompoAttr.instanceID, autoCompoAttr.path, field.FieldType, null, field.Name));
                        return default(KeyValuePair<Object, AutoBindFieldInfo>);
                    }
                    return new KeyValuePair<Object, AutoBindFieldInfo>(gameObject, new AutoBindFieldInfo(autoCompoAttr.instanceID, autoCompoAttr.path, field.FieldType, null, field.Name));
                }
                //再用路径找
                if (!string.IsNullOrEmpty(autoCompoAttr.path))
                {
                    Transform transform = _gameObject.transform.Find(autoCompoAttr.path);
                    if (transform != null)
                    {
                        gameObject = transform.gameObject;
                        if (field.FieldType != typeof(GameObject))
                        {
                            Component component = gameObject.GetComponent(field.FieldType);
                            if (component != null)
                                return new KeyValuePair<Object, AutoBindFieldInfo>(component, new AutoBindFieldInfo(autoCompoAttr.instanceID, autoCompoAttr.path, field.FieldType, null, field.Name));
                            return default(KeyValuePair<Object, AutoBindFieldInfo>);
                        }
                        return new KeyValuePair<Object, AutoBindFieldInfo>(gameObject, new AutoBindFieldInfo(autoCompoAttr.instanceID, autoCompoAttr.path, field.FieldType, null, field.Name));
                    }
                }
            }
            else
            {
                Component autoComponent = _gameObject.GetComponent(_type);
                if (autoComponent == null)
                    return default(KeyValuePair<Object, AutoBindFieldInfo>);
                foreach (MonoScript autoScript in _autoScripts)
                {
                    if (autoScript.text.Contains(field.FieldType.Name + " " + field.Name))
                    {
                        //查找对应的属性
                        foreach (var property in _type.GetProperties())
                        {
                            //类型与名字相同则认为是对应属性
                            if (property.PropertyType != field.FieldType || field.Name != "_" + property.Name)
                                continue;
                            Object obj = property.GetValue(autoComponent, new object[0]) as Object;
                            if (obj is GameObject)
                            {
                                GameObject gameObject = obj as GameObject;
                                return new KeyValuePair<Object, AutoBindFieldInfo>(gameObject,
                                    new AutoBindFieldInfo(true, _gameObject.transform.getChildPath(gameObject.transform), typeof(GameObject), null, field.Name));
                            }
                            else if (obj is Component)
                            {
                                Component component = obj as Component;
                                return new KeyValuePair<Object, AutoBindFieldInfo>(component,
                                    new AutoBindFieldInfo(true, _gameObject.transform.getChildPath(component.transform), field.FieldType, null, field.Name));
                            }
                        }
                    }
                }
            }
            return default(KeyValuePair<Object, AutoBindFieldInfo>);
        }
        GameObject findGameObjectByInstanceID(GameObject gameObject, int instanceID)
        {
            if (gameObject == null)
                return null;
            if (gameObject.GetInstanceID() == instanceID)
                return gameObject;
            for (int i = 0; i < gameObject.transform.childCount; i++)
            {
                GameObject childGameObject = findGameObjectByInstanceID(gameObject.transform.GetChild(i).gameObject, instanceID);
                if (childGameObject != null)
                    return childGameObject;
            }
            return null;
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
        protected void addGenField(GameObject gameObject)
        {
            gameObject = getTargetGameObject(gameObject);
            if (gameObject != _gameObject && !gameObject.transform.isChildOf(_gameObject.transform))
                throw new ArgumentException(gameObject + "不是" + _gameObject + "的一部分");
            if (_objGenDict.ContainsKey(gameObject))
                return;
            _objGenDict.Add(gameObject, new AutoBindFieldInfo(true, _gameObject.transform.getChildPath(gameObject.transform), typeof(GameObject), null, _generator.genFieldName4GO(gameObject)));
        }
        protected void addGenField(Component component)
        {
            component = getTargetGameObject(component.gameObject).GetComponent(component.GetType());
            if (component.gameObject != _gameObject && !component.transform.isChildOf(_gameObject.transform))
                throw new ArgumentException(component + "不是" + _gameObject + "的一部分");
            if (_objGenDict.ContainsKey(component))
                return;
            _objGenDict.Add(component, new AutoBindFieldInfo(true, _gameObject.transform.getChildPath(component.transform), component.GetType(), null, _generator.genFieldName4Compo(component)));
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
            return _script != null ? AssetDatabase.GetAssetPath(_script) : path;
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
            return new AutoBindFieldInfo(
                obj.GetInstanceID(),
                getPath(obj is GameObject ? obj as GameObject : (obj as Component).gameObject),
                obj.GetType(),
                null,
                null);
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
            return _objGenDict.ContainsKey(obj) ? _objGenDict[obj] : null;
        }
        protected virtual bool isPartOfAnyPath(GameObject gameObject)
        {
            if (_objGenDict != null)
            {
                foreach (var pair in _objGenDict)
                {
                    if (string.IsNullOrEmpty(pair.Value.path))
                        continue;
                    if (gameObject == _gameObject || TransformHelper.isPartOfPath(gameObject, _gameObject, pair.Value.path))
                        return true;
                }
            }
            return false;
        }
        protected bool hasProp(Object obj, string propName)
        {
            return _objGenDict.ContainsKey(obj) && _objGenDict[obj].propDict.ContainsKey(propName);
        }
        protected T getProp<T>(Object obj, string propName)
        {
            if (!_objGenDict.ContainsKey(obj))
                throw new KeyNotFoundException("不存在" + obj + "的生成信息");
            if (!_objGenDict[obj].propDict.ContainsKey(propName))
                throw new KeyNotFoundException(obj + "不存在属性" + propName);
            else if (_objGenDict[obj].propDict[propName] is T)
                return (T)_objGenDict[obj].propDict[propName];
            else
                throw new InvalidCastException(obj + "的属性" + propName + "的值" + _objGenDict[obj].propDict[propName] + "无法被转换为" + typeof(T).Name);
        }
        protected T getPropOrDefault<T>(Object obj, string propName)
        {
            if (!_objGenDict.ContainsKey(obj))
                throw new KeyNotFoundException("不存在" + obj + "的生成信息");
            if (!_objGenDict[obj].propDict.ContainsKey(propName))
                return default(T);
            else if (_objGenDict[obj].propDict[propName] is T)
                return (T)_objGenDict[obj].propDict[propName];
            else
                return default(T);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="propName"></param>
        /// <param name="value"></param>
        /// <returns>如果属性发生改变，那么返回true</returns>
        protected bool setProp(Object obj, string propName, object value)
        {
            if (!_objGenDict.ContainsKey(obj))
                throw new KeyNotFoundException("不存在" + obj + "的生成信息");
            if (!_objGenDict[obj].propDict.ContainsKey(propName))
            {
                _objGenDict[obj].propDict[propName] = value;
                return true;
            }
            else
            {
                if (_objGenDict[obj].propDict[propName] == value)
                    return false;
                else
                {
                    _objGenDict[obj].propDict[propName] = value;
                    return true;
                }
            }
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
            var unit = _type != null ? _generator.genScript4GO(_gameObject, _type.Namespace) : _generator.genScript4GO(_gameObject, _setting);
            FileInfo fileInfo = new FileInfo(getSaveFilePath(unit.Namespaces[0].Types[0].Name));
            CodeDOMHelper.writeUnitToFile(fileInfo, unit);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            _autoAddList.Add(new AutoAddCompoInfo(_gameObject, _setting.Namespace + "." + _generator.typeName));
        }
        string getDefaultTypeName()
        {
            if (!string.IsNullOrEmpty(_saveFileName))
                return _saveFileName;
            if (_type != null)
                return _type.Name;
            return _generator.genTypeName4GO(_gameObject);
        }
        /// <summary>
        /// 默认路径是文件夹/类名.cs
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        protected virtual string getSaveFilePath(string typeName)
        {
            if (string.IsNullOrEmpty(_savePath) && _script != null)
            {
                return Path.GetDirectoryName(AssetDatabase.GetAssetPath(_script)) + "/" + typeName + "_AutoGen.cs";
            }
            return _savePath + "/" + typeName + ".cs";
        }
        void saveEditorSettings()
        {
            if (_setting != null)
                _setting.saveToPrefs(PlayerSettings.productName);
        }
        [SerializeField]
        protected AutoCompoGenSetting _setting = null;
        [SerializeField]
        protected GameObject _gameObject;
        [SerializeField]
        protected MonoScript _script;
        [SerializeField]
        protected MonoScript[] _autoScripts;
        protected Type _type;
        protected AutoCompoGenerator _generator;
        protected Dictionary<Object, AutoBindFieldInfo> _objGenDict;
        string[] _ctrlTypes;
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
        [SerializeField]
        protected string _savePath;
        /// <summary>
        /// 保存文件名，同时也被当做默认的类名来使用。
        /// </summary>
        [SerializeField]
        protected string _saveFileName;
        protected SerializedObject _serializedObject;
        [SerializeField]
        protected List<AutoAddCompoInfo> _autoAddList = new List<AutoAddCompoInfo>();
        const string NAME_OF_GAMEOBJECT = "_gameObject";
        const string NAME_OF_SCRIPT = "_script";
        const string NAME_OF_SAVEPATH = "_savePath";
        const string NAME_OF_SAVEFILENAME = "_saveFileName";
        const string NAME_OF_SETTING = "_setting";
        protected const string NAME_OF_CTRL_TYPE = "_controllerType";
    }
}