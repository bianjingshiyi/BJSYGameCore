using System;
using UnityEngine;
using UnityEditor;

namespace BJSYGameCore.UI
{
    class AddComponentWhenCompiled
    {
        [InitializeOnLoadMethod]
        public static void replaceComponentWhenCompiled()
        {
            foreach (var obj in UnityEngine.Object.FindObjectsOfType<AddComponentWhenCompiledComponent>())
            {
                MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(obj.path);
                Type type = script.GetClass();
                if (type != null)
                {
                    if (obj.gameObject.GetComponent(type) == null)
                    {
                        Component component = obj.gameObject.AddComponent(type);
                        component.GetType().GetMethod("autoBind").Invoke(component, new object[] { });
                        Debug.Log("AddComponent:" + type.Name, obj.gameObject);
                    }
                }
                UnityEngine.Object.DestroyImmediate(obj);
            }
        }
    }
}