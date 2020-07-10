using UnityEngine;
using UnityEditor;
using System;
using BJSYGameCore.UI;
namespace BJSYGameCore
{
    class UncompiledComponentHandler
    {
        [InitializeOnLoadMethod]
        public static void replaceComponentWhenCompiled()
        {
            foreach (var obj in UnityEngine.Object.FindObjectsOfType<UncompiledComponent>())
            {
                if (PrefabUtility.IsPartOfNonAssetPrefabInstance(obj))
                    continue;
                MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(obj.path);
                Type type = script.GetClass();
                if (type != null)
                {
                    if (obj.gameObject.GetComponent(type) is Component component)
                    {
                        component.GetType().GetMethod("autoBind").Invoke(component, new object[] { });
                    }
                    else
                    {
                        component = obj.gameObject.AddComponent(type);
                        component.GetType().GetMethod("autoBind").Invoke(component, new object[] { });
                        Debug.Log("AddComponent:" + type.Name, obj.gameObject);
                    }
                }
                UnityEngine.Object.DestroyImmediate(obj);
            }
            UGUIAutoScriptPref pref = UGUIAutoScriptPref.getDefaultPref();
            foreach (GameObject prefab in pref.updateList)
            {
                string prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(prefab);
                GameObject gameObject = PrefabUtility.LoadPrefabContents(prefabPath);

                UncompiledComponent obj = gameObject.GetComponent<UncompiledComponent>();
                if (obj != null)
                {
                    MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(obj.path);
                    Type type = script.GetClass();
                    if (type != null)
                    {
                        if (obj.gameObject.GetComponent(type) is Component component)
                        {
                            component.GetType().GetMethod("autoBind").Invoke(component, new object[] { });
                        }
                        else
                        {
                            component = obj.gameObject.AddComponent(type);
                            component.GetType().GetMethod("autoBind").Invoke(component, new object[] { });
                            Debug.Log("AddComponent:" + type.Name, obj.gameObject);
                        }
                    }
                    UnityEngine.Object.DestroyImmediate(obj);
                }

                PrefabUtility.SaveAsPrefabAsset(gameObject, prefabPath);
                PrefabUtility.UnloadPrefabContents(gameObject);
            }
            pref.updateList.Clear();
            EditorUtility.SetDirty(pref);
        }
    }
}