using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

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
        [SerializeField]
        List<GameObject> _updateList = new List<GameObject>();
        public List<GameObject> updateList
        {
            get { return _updateList; }
        }
        public static UGUIAutoScriptPref getDefaultPref()
        {
            UGUIAutoScriptPref pref = AssetDatabase.LoadAssetAtPath<UGUIAutoScriptPref>("Assets/Editor/UIScriptGeneratorPrefs.asset");
            if (pref == null)
            {
                pref = CreateInstance<UGUIAutoScriptPref>();
                if (!Directory.Exists("Assets/Editor"))
                    Directory.CreateDirectory("Assets/Editor");
                AssetDatabase.CreateAsset(pref, "Assets/Editor/UIScriptGeneratorPrefs.asset");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            return pref;
        }
    }
}