#if UNITY_EDITOR
using UnityEngine;
using BJSYGameCore.UI;
using UnityEditor;
using System;
namespace BJSYGameCore
{
    /// <summary>
    /// 用来添加还没有被编译的脚本，在编译之后，这个脚本会被自动炸掉。
    /// </summary>
    /// <remarks>这个组件不能放在Editor里面，因为所有Editor里面的组件都不能添加。</remarks>
    [ExecuteInEditMode]
    public class UncompiledComponent : MonoBehaviour
    {
        [SerializeField]
        string _path;
        public string path
        {
            get { return _path; }
            set { _path = value; }
        }
        protected void Update()
        {
            if (PrefabUtility.IsPartOfNonAssetPrefabInstance(this))
                return;
            MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
            Type type = script.GetClass();
            if (type != null)
            {
                if (gameObject.GetComponent(type) == null)
                {
                    gameObject.AddComponent(type);
                }
            }
            DestroyImmediate(this);
        }
        public static UncompiledComponent add(GameObject gameObject, string scriptAssetPath)
        {
            UncompiledComponent component = gameObject.AddComponent<UncompiledComponent>();
            component.path = scriptAssetPath;
            return component;
        }
    }
}
#endif