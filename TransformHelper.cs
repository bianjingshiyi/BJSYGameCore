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
            return rootGameObject.transform.findByPath(path).gameObject;
        }
        public static Transform findByPath(this Transform transform, string path)
        {
            if (path == "./")
                return transform;
            string[] names = path.Split('/');
            for (int i = 1; i < names.Length; i++)
            {
                transform = transform.Find(names[i]);
            }
            return transform;
        }
    }
}