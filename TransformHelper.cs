using System;
using UnityEngine;
namespace BJSYGameCore.AutoCompo
{
    public static class TransformHelper
    {
        public static bool isPartOfPath(GameObject gameObject, GameObject rootGameObject, string path)
        {
            if (path == "./")
                return rootGameObject == gameObject;
            string[] names = path.Split('/');
            Transform transform = rootGameObject.transform;
            for (int i = 1; i < names.Length; i++)
            {
                transform = transform.Find(names[i]);
                if (transform == null)
                    return false;
                if (transform.gameObject == gameObject)
                    return true;
            }
            return false;
        }
        public static bool isPathMatch(string path, GameObject gameObject, GameObject rootGameObject)
        {
            if (path == "./")
                return rootGameObject == gameObject;
            string[] names = path.Split('/');
            Transform transform = rootGameObject.transform;
            for (int i = 1; i < names.Length; i++)
            {
                transform = transform.Find(names[i]);
                if (transform == null)
                    return false;
            }
            return transform.gameObject == gameObject;
        }
        public static GameObject findGameObjectByPath(GameObject rootGameObject, string path)
        {
            return rootGameObject.transform.Find(path, true).gameObject;
        }

        public static GameObject find(this GameObject gameObject, string path)
        {
            if (gameObject == null)
                throw new ArgumentNullException("gameObject");
            Transform transform = gameObject.transform.Find(path, true);
            return transform != null ? transform.gameObject : null;
        }
        /// <summary>
        /// 选择是否检查以./开头
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="path"></param>
        /// <param name="checkHead"></param>
        /// <returns></returns>
        public static Transform Find(this Transform transform, string path, bool checkHead)
        {
            if (checkHead && path.StartsWith("./"))
            {
                if (path == "./")
                    return transform;
                path = path.Substring(2, path.Length - 2);
            }
            return transform.Find(path);
        }
        [Obsolete("使用Find作为替代")]
        public static Transform getChildAt(this Transform transform, string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                int index = path.LastIndexOf('/');
                if (0 <= index && index < path.Length)
                {
                    string prevPath = path.Substring(0, index);
                    string name = path.Substring(index + 1, path.Length - index - 1);
                    Transform parent = transform.getChildAt(prevPath);
                    if (parent != null)
                        return parent.getChildAt(name);
                    else
                        return null;
                }
                else
                {
                    for (int i = 0; i < transform.childCount; i++)
                    {
                        if (transform.GetChild(i).name == path)
                            return transform.GetChild(i);
                    }
                    return null;
                }
            }
            else
                return transform;
        }
    }
}