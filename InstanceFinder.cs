using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace TBSGameCore
{
    public static class InstanceFinder
    {
        public static T findInstance<T>(this Scene scene)
        {
            GameObject[] objs = scene.GetRootGameObjects();
            for (int i = 0; i < objs.Length; i++)
            {
                T t = objs[i].GetComponentInChildren<T>(true);
                if (t != null)
                    return t;
            }
            return default(T);
        }
        public static T[] findInstances<T>(this Scene scene)
        {
            GameObject[] objs = scene.GetRootGameObjects();
            List<T> instances = new List<T>();
            for (int i = 0; i < objs.Length; i++)
            {
                instances.AddRange(objs[i].GetComponentsInChildren<T>());
            }
            return instances.ToArray();
        }
        public static T findInstance<T>(this Component component)
        {
            return component.gameObject.scene.findInstance<T>();
        }
        public static T[] findInstances<T>(this Component component)
        {
            return component.gameObject.scene.findInstances<T>();
        }
    }
}