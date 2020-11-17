using UnityEngine;
using UnityEditor;
using System.IO;
using System.Reflection;
namespace BJSYGameCore.AutoCompo
{
    public class AutoCompoWindow : EditorWindow
    {
        #region 公共成员
        [MenuItem("Assets/Create/AutoCompo/Generate", true, 81)]
        [MenuItem("GameObject/AutoCompo/Generate", true, 15)]
        public static bool onValidateMenuItemGenerate()
        {
            if (Selection.gameObjects.Length != 1)
                return false;
            return true;
        }
        [MenuItem("Assets/Create/AutoCompo/Generate", false, 81)]
        [MenuItem("GameObject/AutoCompo/Generate", false, 15)]
        public static void onMenuItemGenerate()
        {
            GameObject gameObject = Selection.gameObjects[0];
            string path = gameObject.scene.path;
            if (string.IsNullOrEmpty(path))
                path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(gameObject);
            AutoCompoWindow window = GetWindow<AutoCompoWindow>(nameof(AutoCompo), true);
            window._gameObject = gameObject;
            if (window.tryFindAutoCompo(gameObject.GetInstanceID(), out string existPath))
                window._saveDir = Path.GetDirectoryName(existPath);
            else
                window._saveDir = Path.GetDirectoryName(path);
        }
        #endregion
        #region 私有成员
        protected void OnGUI()
        {
            if (_serializedWindow == null)
                _serializedWindow = new SerializedObject(this);
            if (_serializedSetting == null)
                _serializedSetting = new SerializedObject(AutoCompoGenSettingContainer.getDefault());
            GUI.enabled = false;
            EditorGUILayout.ObjectField("要生成脚本的游戏物体", _gameObject, typeof(GameObject), true);
            GUI.enabled = true;
            EditorGUILayout.LabelField("保存路径", _saveDir);
            EditorGUILayout.PropertyField(_serializedSetting.FindProperty(nameof(AutoCompoGenSettingContainer.setting)), new GUIContent("设置"), true);
            AutoCompoGenerator generator = null;
            if (GUILayout.Button("保存脚本"))
                generator = new AutoCompoGenerator();
            if (GUILayout.Button("另存为脚本"))
            {
                _saveDir = EditorUtility.SaveFolderPanel("另存为脚本", _saveDir, string.Empty);
                if (Directory.Exists(_saveDir))
                    generator = new AutoCompoGenerator();
            }
            _serializedWindow.ApplyModifiedProperties();
            _serializedSetting.ApplyModifiedProperties();
            if (generator != null)
            {
                var unit = generator.genScript4GO(_gameObject, AutoCompoGenSettingContainer.getDefault().setting);
                FileInfo fileInfo = new FileInfo(_saveDir + "/" + unit.Namespaces[0].Types[0].Name + ".cs");
                CodeDOMHelper.writeUnitToFile(fileInfo, unit);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }
        protected void OnDisable()
        {
            _serializedWindow.ApplyModifiedProperties();
            _serializedWindow.Dispose();
            _serializedWindow = null;
            _serializedSetting.ApplyModifiedProperties();
            _serializedSetting.Dispose();
            _serializedSetting = null;
        }
        bool tryFindAutoCompo(int instanceID, out string path)
        {
            foreach (var guid in AssetDatabase.FindAssets("t:MonoScript"))
            {
                path = AssetDatabase.GUIDToAssetPath(guid);
                var script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                var type = script.GetClass();
                if (type != null
                    && type.GetCustomAttribute<AutoCompoAttribute>() is AutoCompoAttribute att
                    && att.instanceID == instanceID)
                {
                    return true;
                }
            }
            path = string.Empty;
            return false;
        }
        #endregion
        [SerializeField]
        GameObject _gameObject;
        [SerializeField]
        string _saveDir;
        SerializedObject _serializedWindow;
        SerializedObject _serializedSetting;
    }
}