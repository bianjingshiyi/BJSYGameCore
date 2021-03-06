﻿
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BJSYGameCore.SaveSystem
{
    [ExecuteInEditMode]
    public class SavableInstance : MonoBehaviour
    {
        [SerializeField]
        int _id = 0;
        public int id
        {
            get { return _id; }
            internal set
            {
                _id = value;
            }
        }
        public string path
        {
            get
            {
                if (transform.parent == null)
                    return gameObject.name;
                else
                {
                    string path = gameObject.name;
                    for (Transform parent = transform.parent; parent != null; parent = parent.parent)
                    {
                        path = parent.gameObject.name + "/" + path;
                    }
                    return path;
                }
            }
        }
        public InstanceReference reference
        {
            get { return new InstanceReference(id, path); }
        }
        public static SavableInstance create(InstanceManager manager, string name)
        {
            SavableInstance instance = new GameObject(name).AddComponent<SavableInstance>();
            instance.id = manager.allocate(instance);
            return instance;
        }
        public static SavableInstance create(Scene scene, string path, int id)
        {
            SavableInstance instance = scene.newGameObjectAt(path).AddComponent<SavableInstance>();
            instance._id = id;
            return instance;
        }
        protected void Awake()
        {
            manager = this.findInstance<InstanceManager>();
        }
        InstanceManager manager { get; set; }
        public GameObject findChild(string path)
        {
            if (string.IsNullOrEmpty(path))
                return gameObject;
            string[] names = path.Split('/');
            if (names.Length > 0)
            {
                Transform child = transform.Find(names[0]);
                for (int i = 1; i < names.Length; i++)
                {
                    if (child != null)
                        child = child.Find(names[i]);
                    else
                        break;
                }
                if (child != null)
                    return child.gameObject;
                else
                    return null;
            }
            else
                return null;
        }
        bool _checked = false;
        protected void Update()
        {
            if (manager != null)
            {
                if (id <= 0)
                {
                    //没有注册，注册ID。
                    id = manager.allocate(this);
                    _checked = true;
                }
                else if (!_checked)
                {
                    //已经注册，没有检查，检查是否实际上丢失了注册。
                    SavableInstance other = manager.getInstanceById(id);
                    if (other == null)
                    {
                        //有ID但是丢失引用，重新分配引用
                        manager.reallocate(id, this);
                    }
                    else if (other != this)
                    {
                        //引用被别人占据了，重新注册
                        id = manager.allocate(this);
                    }
                }
            }
        }
        protected void OnDestroy()
        {
            if (manager != null)
                manager.dellocate(this);
        }
    }
}