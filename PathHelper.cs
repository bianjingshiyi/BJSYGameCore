using System.Linq;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace TBSGameCore
{
    public static class PathHelper
    {
        public static string getPath(this Component component)
        {
            string path = component.gameObject.name;
            for (Transform parent = component.transform.parent; parent != null; parent = parent.parent)
            {
                path = parent.gameObject.name + path;
            }
            return path;
        }
        public static string getPath(this GameObject gameObject)
        {
            string path = gameObject.name;
            for (Transform parent = gameObject.transform.parent; parent != null; parent = parent.parent)
            {
                path = parent.gameObject.name + path;
            }
            return path;
        }
        public static Transform getTransformAtPath(this Scene scene, string path)
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
                            else
                                return null;
                        }
                        return child;
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
        public static Transform getTransformAtPath(this Component component, string path)
        {
            return component.gameObject.scene.getTransformAtPath(path);
        }
        public static Transform getTransformAtPath(this GameObject gameObject, string path)
        {
            return gameObject.scene.getTransformAtPath(path);
        }
        public static T instantiateAtPath<T>(this Scene scene, T prefab, string path) where T : Component
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
                            instance.name = names[names.Length - 1];
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
        public static GameObject createGameObjectAtPath(this Scene scene, string path)
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