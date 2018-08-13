using System;
using System.Linq;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace TBSGameCore
{
    [Serializable]
    public abstract class SavableEntityData : ILoadableData
    {
        [SerializeField]
        public int id;
        [SerializeField]
        public string path;
        public abstract ISavable load(Scene scene);
        protected GameObject createGameObjectByPath(string path, Scene scene)
        {
            string[] names = path.Split('/');
            if (names.Length > 1)
            {
                GameObject root = scene.GetRootGameObjects().FirstOrDefault(e => { return e.name == names[0]; });
                if (root != null)
                {
                    Transform parent = root.transform;
                    for (int i = 1; i < names.Length - 1; i++)
                    {
                        if (parent != null)
                            parent = parent.Find(names[i]);
                        else
                            return null;
                    }
                    if (parent != null)
                    {
                        GameObject obj = new GameObject(names[names.Length - 1]);
                        obj.transform.parent = parent;
                        return obj;
                    }
                    else
                        return null;
                }
                else
                    return null;
            }
            else if (names.Length == 1)
            {
                return new GameObject(names[0]);
            }
            else
            {
                return null;
            }
        }
    }
}