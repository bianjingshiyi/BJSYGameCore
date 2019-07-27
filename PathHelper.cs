using System.Linq;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace BJSYGameCore
{
    public static class PathHelper
    {
        public static string getPath(this Component component)
        {
            string path = component.gameObject.name;
            for (Transform parent = component.transform.parent; parent != null; parent = parent.parent)
            {
                path = parent.gameObject.name + "/" + path;
            }
            return path;
        }
        public static string getPath(this GameObject gameObject)
        {
            string path = gameObject.name;
            for (Transform parent = gameObject.transform.parent; parent != null; parent = parent.parent)
            {
                path = parent.gameObject.name + "/" + path;
            }
            return path;
        }
        public static string getChildPath(this Transform transform, Transform child)
        {
            if (transform != child)
            {
                string path = child.gameObject.name;
                for (Transform parent = child.parent; parent != null && parent != transform; parent = parent.parent)
                {
                    path = parent.gameObject.name + "/" + path;
                }
                return path;
            }
            else
                return string.Empty;
        }
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
        public static T findInstanceAt<T>(this Scene scene, string path) where T : Component
        {
            if (!string.IsNullOrEmpty(path))
            {
                string[] names = path.Split('/');
                if (names.Length > 0)
                {
                    GameObject root = scene.GetRootGameObjects().FirstOrDefault(e => { return e.name == names[0]; });
                    if (root != null)
                    {
                        Transform child = root.transform;
                        for (int i = 1; i < names.Length; i++)
                        {
                            if (child != null)
                                child = child.Find(names[i]);
                        }
                        if (child != null)
                            return child.GetComponent<T>();
                        else
                            return null;
                    }
                    else
                        return null;
                }
                else
                    return null;
            }
            else
                return null;
        }
        public static GameObject findGameObjectAt(this Scene scene, string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                string[] names = path.Split('/');
                if (names.Length > 0)
                {
                    GameObject root = scene.GetRootGameObjects().FirstOrDefault(e => { return e.name == names[0]; });
                    if (root != null)
                    {
                        Transform child = root.transform;
                        for (int i = 1; i < names.Length; i++)
                        {
                            if (child != null)
                                child = child.Find(names[i]);
                        }
                        if (child != null)
                            return child.gameObject;
                        else
                            return null;
                    }
                    else
                        return null;
                }
                else
                    return null;
            }
            else
                return null;
        }
        public static T instantiateAt<T>(this Scene scene, T prefab, string path) where T : Component
        {
            if (!string.IsNullOrEmpty(path))
            {
                string[] names = path.Split('/');
                if (names.Length > 1)
                {
                    GameObject root = scene.GetRootGameObjects().FirstOrDefault(e => { return e.name == names[0]; });
                    if (root != null)
                    {
                        Transform child = root.transform;
                        for (int i = 1; i < names.Length - 1; i++)
                        {
                            if (child != null)
                                child = child.Find(names[i]);
                            else
                                return null;
                        }
                        if (child != null)
                        {
                            T instance = Component.Instantiate(prefab, child);
                            instance.gameObject.name = names[names.Length - 1];
                            return instance;
                        }
                        else
                            return null;
                    }
                    else
                        return null;
                }
                else
                {
                    T instance = Component.Instantiate(prefab);
                    instance.gameObject.name = names[0];
                    return instance;
                }
            }
            else
                return null;
        }
        public static GameObject newGameObjectAt(this Scene scene, string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                string[] names = path.Split('/');
                if (names.Length > 1)
                {
                    GameObject root = scene.GetRootGameObjects().FirstOrDefault(e => { return e.name == names[0]; });
                    if (root != null)
                    {
                        Transform child = root.transform;
                        for (int i = 1; i < names.Length - 1; i++)
                        {
                            if (child != null)
                                child = child.Find(names[i]);
                            else
                                return null;
                        }
                        if (child != null)
                        {
                            GameObject go = new GameObject(names[names.Length - 1]);
                            go.transform.parent = child;
                            return go;
                        }
                        else
                            return null;
                    }
                    else
                        return null;
                }
                else
                {
                    GameObject go = new GameObject(names[0]);
                    return go;
                }
            }
            else
                return null;
        }
    }
}