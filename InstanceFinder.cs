using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace BJSYGameCore
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
            return default;
        }
        public static Component findInstance(this Scene scene, Type type)
        {
            GameObject[] objs = scene.GetRootGameObjects();
            for (int i = 0; i < objs.Length; i++)
            {
                Component instance = objs[i].GetComponentInChildren(type, true);
                if (instance != null)
                    return instance;
            }
            return null;
        }
        public static T[] findInstances<T>(this Scene scene)
        {
            GameObject[] objs = scene.GetRootGameObjects();
            List<T> instances = new List<T>();
            for (int i = 0; i < objs.Length; i++)
            {
                instances.AddRange(objs[i].GetComponentsInChildren<T>(true));
            }
            return instances.ToArray();
        }
        public static Component findInstance(this Component component, Type type)
        {
            return component.gameObject.scene.findInstance(type);
        }
        public static T findInstance<T>(this Component component)
        {
            return component.gameObject.scene.findInstance<T>();
        }
        public static T[] findInstances<T>(this Component component)
        {
            return component.gameObject.scene.findInstances<T>();
        }
        public static T findInstanceAllScene<T>()
        {
            T instance = default;
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                instance = SceneManager.GetSceneAt(i).findInstance<T>();
                if (instance != null)
                    break;
            }
            return instance;
        }
        public static T[] findInstancesAllScene<T>()
        {
            List<T> instanceList = new List<T>();
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                GameObject[] roots = SceneManager.GetSceneAt(i).GetRootGameObjects();
                for (int j = 0; j < roots.Length; j++)
                {
                    instanceList.AddRange(roots[j].GetComponentsInChildren<T>(true));
                }
            }
            return instanceList.ToArray();
        }
    }
}