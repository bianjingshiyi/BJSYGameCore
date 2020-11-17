using UnityEngine;
using UnityEditor;
using System.IO;

namespace BJSYGameCore.AutoCompo
{
    public class AutoCompoGenSettingContainer : ScriptableObject
    {
        public AutoCompoGenSetting setting;
        public static AutoCompoGenSettingContainer getDefault()
        {
            AutoCompoGenSettingContainer pref = AssetDatabase.LoadAssetAtPath<AutoCompoGenSettingContainer>("Assets/Editor/" + nameof(AutoCompoGenSettingContainer) + ".asset");
            if (pref == null)
            {
                pref = CreateInstance<AutoCompoGenSettingContainer>();
                if (!Directory.Exists("Assets/Editor"))
                    Directory.CreateDirectory("Assets/Editor");
                AssetDatabase.CreateAsset(pref, "Assets/Editor/" + nameof(AutoCompoGenSettingContainer) + ".asset");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            return pref;
        }
    }
}