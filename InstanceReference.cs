using System;
using System.Linq;

using UnityEngine;
using UnityEngine.SceneManagement;

using TBSGameCore;

namespace TBSGameCore
{
    [Serializable]
    public class InstanceReference
    {
        public int id;
        public string path;
        public InstanceReference(Component component)
        {
            if (component != null)
            {
                path = component.gameObject.name;
                for (Transform parent = component.gameObject.transform.parent; parent != null; parent = parent.parent)
                {
                    SavableInstance instance = parent.GetComponent<SavableInstance>();
                    if (instance == null)
                        path = parent.gameObject.name + '/' + path;
                    else
                    {
                        id = instance.id;
                        break;
                    }
                }
            }
            else
            {
                id = 0;
                path = "";
            }
        }
        public InstanceReference(GameObject go)
        {
            path = go.name;
            for (Transform parent = go.transform.parent; parent != null; parent = parent.parent)
            {
                SavableInstance instance = parent.GetComponent<SavableInstance>();
                if (instance == null)
                    path = parent.gameObject.name + '/' + path;
                else
                {
                    id = instance.id;
                    break;
                }
            }
        }
        public InstanceReference(int id, string path = "")
        {
            this.id = id;
            this.path = path;
        }
        public bool isNull
        {
            get { return id <= 0 && string.IsNullOrEmpty(path); }
        }
        public T findInstanceIn<T>(InstanceManager manager)
        {
            if (id > 0)
            {
                SavableInstance instance = manager.getInstanceById(id);
                if (string.IsNullOrEmpty(path))
                {
                    return instance.GetComponent<T>();
                }
                else
                {
                    string[] names = path.Split('/');
                    Transform child = instance.transform;
                    for (int i = 0; i < names.Length; i++)
                    {
                        child = child.Find(names[i]);
                    }
                    if (child != null)
                        return child.GetComponent<T>();
                    else
                        return default(T);
                }
            }
            else if (!string.IsNullOrEmpty(path))
            {
                string[] names = path.Split('/');
                GameObject root = manager.gameObject.scene.GetRootGameObjects().FirstOrDefault(e => { return e.name == names[0]; });
                if (root == null)
                    return default(T);
                Transform parent = root.transform;
                for (int i = 1; i < names.Length; i++)
                {
                    parent = parent.Find(names[i]);
                }
                if (parent != null)
                    return parent.GetComponent<T>();
                else
                    return default(T);
            }
            return default(T);
        }
        public T findInstanceIn<T>(Scene scene)
        {
            if (id > 0)
            {
                SavableInstance instance = scene.findInstance<InstanceManager>().getInstanceById(id);
                if (string.IsNullOrEmpty(path))
                {
                    return instance.GetComponent<T>();
                }
                else
                {
                    string[] names = path.Split('/');
                    Transform child = instance.transform;
                    for (int i = 0; i < names.Length; i++)
                    {
                        child = child.Find(names[i]);
                    }
                    if (child != null)
                        return child.GetComponent<T>();
                    else
                        return default(T);
                }
            }
            else if (!string.IsNullOrEmpty(path))
            {
                string[] names = path.Split('/');
                GameObject root = scene.GetRootGameObjects().FirstOrDefault(e => { return e.name == names[0]; });
                if (root == null)
                    return default(T);
                Transform parent = root.transform;
                for (int i = 1; i < names.Length; i++)
                {
                    parent = parent.Find(names[i]);
                }
                if (parent != null)
                    return parent.GetComponent<T>();
                else
                    return default(T);
            }
            return default(T);
        }
        public Component findInstanceIn(Scene scene, Type type)
        {
            if (id > 0)
            {
                SavableInstance instance = scene.findInstance<InstanceManager>().getInstanceById(id);
                if (string.IsNullOrEmpty(path))
                {
                    return instance.GetComponent(type);
                }
                else
                {
                    string[] names = path.Split('/');
                    Transform child = instance.transform;
                    for (int i = 0; i < names.Length; i++)
                    {
                        child = child.Find(names[i]);
                    }
                    if (child != null)
                        return child.GetComponent(type);
                    else
                        return null;
                }
            }
            else if (!string.IsNullOrEmpty(path))
            {
                string[] names = path.Split('/');
                GameObject root = scene.GetRootGameObjects().FirstOrDefault(e => { return e.name == names[0]; });
                if (root == null)
                    return null;
                Transform parent = root.transform;
                for (int i = 1; i < names.Length; i++)
                {
                    parent = parent.Find(names[i]);
                }
                if (parent != null)
                    return parent.GetComponent(type);
                else
                    return null;
            }
            return null;
        }
        public override string ToString()
        {
            return id.ToString() + ':' + path;
        }
    }
}