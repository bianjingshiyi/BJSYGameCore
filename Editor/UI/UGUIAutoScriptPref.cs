using UnityEngine;
using UnityEditor;

namespace BJSYGameCore.UI
{
    class UGUIAutoScriptPref : ScriptableObject
    {
        [SerializeField]
        string _lastDir = string.Empty;
        public string lastDir
        {
            get { return _lastDir; }
            set { _lastDir = value; }
        }
        [SerializeField]
        string _namespace = "UI";
#pragma warning disable IDE1006 // 命名样式
        public string Namespace
#pragma warning restore IDE1006 // 命名样式
        {
            get { return _namespace; }
        }
        public static UGUIAutoScriptPref getDefaultPref()
        {
            UGUIAutoScriptPref pref = AssetDatabase.LoadAssetAtPath<UGUIAutoScriptPref>("Assets/Editor/UIScriptGeneratorPrefs.asset");
            if (pref == null)
            {
                pref = ScriptableObject.CreateInstance<UGUIAutoScriptPref>();
                AssetDatabase.CreateAsset(pref, "Assets/Editor/UIScriptGeneratorPrefs.asset");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            return pref;
        }
    }
}