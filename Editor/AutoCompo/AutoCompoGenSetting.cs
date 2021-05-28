using System;
using UnityEditor;

namespace BJSYGameCore.AutoCompo
{
    [Serializable]
    public class AutoCompoGenSetting
    {
        public void loadFromPrefs(string name)
        {
            if (!EditorPrefs.HasKey(name + ":" + USINGS_LENGTH))
                return;
            usings = new string[EditorPrefs.GetInt(name + ":" + USINGS_LENGTH)];
            for (int i = 0; i < usings.Length; i++)
            {
                usings[i] = EditorPrefs.GetString(name + ":" + USINGS + i);
            }
            Namespace = EditorPrefs.GetString(name + ":" + NAMESPACE);
            baseTypes = new string[EditorPrefs.GetInt(name + ":" + BASETYPES_LENGTH)];
            for (int i = 0; i < baseTypes.Length; i++)
            {
                baseTypes[i] = EditorPrefs.GetString(name + ":" + BASETYPES + i);
            }
            fieldAttributes = new string[EditorPrefs.GetInt(name + ":" + FIELDATTRIBUTES_LENGTH)];
            for (int i = 0; i < fieldAttributes.Length; i++)
            {
                fieldAttributes[i] = EditorPrefs.GetString(name + ":" + FIELDATTRIBUTES + i);
            }
            ctrlNamespace = EditorPrefs.GetString(name + ":" + CTRLNAMESPACE);
        }
        public void saveToPrefs(string name)
        {
            EditorPrefs.SetInt(name + ":" + USINGS_LENGTH, usings.Length);
            for (int i = 0; i < usings.Length; i++)
            {
                EditorPrefs.SetString(name + ":" + USINGS + i, usings[i]);
            }
            EditorPrefs.SetString(name + ":" + NAMESPACE, Namespace);
            EditorPrefs.SetInt(name + ":" + BASETYPES_LENGTH, baseTypes.Length);
            for (int i = 0; i < baseTypes.Length; i++)
            {
                EditorPrefs.SetString(name + ":" + BASETYPES + i, baseTypes[i]);
            }
            EditorPrefs.SetInt(name + ":" + FIELDATTRIBUTES_LENGTH, fieldAttributes.Length);
            for (int i = 0; i < fieldAttributes.Length; i++)
            {
                EditorPrefs.SetString(name + ":" + FIELDATTRIBUTES + i, fieldAttributes[i]);
            }
            EditorPrefs.SetString(name + ":" + CTRLNAMESPACE, ctrlNamespace);
        }
        public string[] usings = new string[0];
        public string Namespace = "UI";
        public string[] baseTypes = new string[0];
        public string[] fieldAttributes = new string[0];
        public string ctrlNamespace = "Controllers";
        const string USINGS = "AutoCompoGenSetting.usings";
        const string USINGS_LENGTH = "AutoCompoGenSetting.usings.Length";
        const string NAMESPACE = "AutoCompoGenSetting.Namespace";
        const string BASETYPES = "AutoCompoGenSetting.baseTypes";
        const string BASETYPES_LENGTH = "AutoCompoGenSetting.baseTypes.Length";
        const string FIELDATTRIBUTES = "AutoCompoGenSetting.fieldAttributes";
        const string FIELDATTRIBUTES_LENGTH = "AutoCompoGenSetting.fieldAttributes.Length";
        const string CTRLNAMESPACE = "AutoCompoGenSetting.ctrlNamespace";
    }
}