using System;
using UnityEngine;
using UnityEditor;

namespace BJSYGameCore.UI
{
    class AddComponentWhenCompiled : MonoBehaviour
    {
        [SerializeField]
        string _path;
        public string path
        {
            get { return _path; }
            set { _path = value; }
        }
        [InitializeOnLoadMethod]
        public static void replaceComponentWhenCompiled()
        {
            foreach (var obj in FindObjectsOfType<AddComponentWhenCompiled>())
            {
                MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(obj.path);
                Type type = script.GetClass();
                if (type != null)
                {
                    if (obj.gameObject.GetComponent(type) == null)
                    {
                        obj.gameObject.AddComponent(type);
                        Debug.Log("AddComponent:" + type.Name, obj.gameObject);
                    }
                }
                DestroyImmediate(obj);
            }
        }
    }
}