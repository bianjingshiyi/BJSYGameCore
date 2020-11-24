using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

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
        #endregion
        #region 私有成员
        protected void Awake()
        {
            if (_setting == null)
                _setting = loadSetting();
        }
        public void checkGameObject(GameObject gameObject, bool forceReselect = false)
        {
            if (gameObject == null || (!forceReselect && _gameObject == gameObject))
                return;
            _gameObject = gameObject;
            _savePath = getSavePath4GO(gameObject);
            if (string.IsNullOrEmpty(_savePath))
            {
                Close();
                throw new DirectoryNotFoundException();
            }
        }
        protected void OnGUI()
        {
            onGUISetting();
            AutoCompoGenerator generator = null;
            if (GUILayout.Button("保存脚本"))
                generator = createGenerator();
            if (GUILayout.Button("另存为脚本"))
            {
                _savePath = EditorUtility.SaveFolderPanel("另存为脚本", _savePath, string.Empty);
                if (Directory.Exists(_savePath))
                    generator = createGenerator();
            }
            if (generator != null)
            {
                var unit = generator.genScript4GO(_gameObject, _setting);
                FileInfo fileInfo = new FileInfo(getSaveFilePath(unit.Namespaces[0].Types[0].Name));
                CodeDOMHelper.writeUnitToFile(fileInfo, unit);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
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
            if (_gameObject == null)
            {
                Close();
                return;
            }
            if (_serializedObject == null)
                _serializedObject = new SerializedObject(this);
            GUI.enabled = false;
            EditorGUILayout.ObjectField("要生成脚本的游戏物体", _gameObject, typeof(GameObject), true);
            GUI.enabled = true;
            EditorGUILayout.LabelField("保存路径", _savePath);
            EditorGUILayout.PropertyField(_serializedObject.FindProperty(NAME_OF_SETTING), new GUIContent("设置"), true);
            _serializedObject.ApplyModifiedProperties();
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
            string path = gameObject.scene.path;
            if (string.IsNullOrEmpty(path))
            {
#if UNITY_2017
                path = AssetDatabase.GetAssetPath(gameObject);
#else
                path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(gameObject);
#endif
            }
            string existPath;
            if (tryFindAutoCompo(gameObject.GetInstanceID(), out existPath))
                return Path.GetDirectoryName(existPath);
            else
                return Path.GetDirectoryName(path);
        }
        protected virtual AutoCompoGenerator createGenerator()
        {
            return new AutoCompoGenerator();
        }
        bool tryFindAutoCompo(int instanceID, out string path)
        {
            foreach (var guid in AssetDatabase.FindAssets("t:MonoScript"))
            {
                path = AssetDatabase.GUIDToAssetPath(guid);
                var script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                var type = script.GetClass();
                if (type != null)
                {
                    AutoCompoAttribute att = type.GetCustomAttributes(typeof(AutoCompoAttribute), true).OfType<AutoCompoAttribute>().FirstOrDefault();
                    if (att != null && att.instanceID == instanceID)
                        return true;
                }
            }
            path = string.Empty;
            return false;
        }
        [SerializeField]
        protected GameObject _gameObject;
        [SerializeField]
        protected string _savePath;
        [SerializeField]
        protected AutoCompoGenSetting _setting = null;
        SerializedObject _serializedObject;
        protected const int PRIOR_SCENE_GENERATE = 15;
        protected const int PRIOR_PREFAB_GENERATE = 81;
        const string NAME_OF_SETTING = "_setting";
        #endregion
    }
}