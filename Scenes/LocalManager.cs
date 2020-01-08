using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using UnityEngine;

using BJSYGameCore;

namespace BJSYGameCore
{
    /// <summary>
    /// 本地管理器，用于管理一整个场景里的Manager。
    /// </summary>
    public class LocalManager : MonoBehaviour
    {
        [SerializeField]
        GlobalManager _global;
        /// <summary>
        /// 应该是全局只存在一个单例的全局管理器。
        /// </summary>
        public GlobalManager global
        {
            get
            {
                if (_global == null)
                {
                    _global = InstanceFinder.findInstanceAllScene<GlobalManager>();
                    if (_global == null)
                        _global = new GameObject("GlobalManager").AddComponent<GlobalManager>();
                    _global.registerLocal(this);
                }
                return _global;
            }
            internal set
            {
                _global = value;
            }
        }
        /// <summary>
        /// 这个LocalManager管理的所有Manager。
        /// </summary>
        public Manager[] managers
        {
            get { return dicTypeManager.Values.ToArray(); }
        }
        Dictionary<Type, Manager> dicTypeManager { get; } = new Dictionary<Type, Manager>();
        /// <summary>
        /// 注册管理器，只有被注册的管理器才可以被getManager方法获取到。
        /// </summary>
        /// <param name="manager">管理器</param>
        public void registerManager(Manager manager)
        {
            if (!dicTypeManager.ContainsKey(manager.GetType()))
            {
                dicTypeManager.Add(manager.GetType(), manager);
                manager.local = this;
            }
            else
                Debug.LogWarning(this + "中已经存在对" + manager + "的引用！", this);
        }
        /// <summary>
        /// 获取指定类型的管理器
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public Manager getManager(Type type)
        {
            if (dicTypeManager.ContainsKey(type))
                return dicTypeManager[type];
            else
            {
                Manager manager = this.findInstance(type) as Manager;
                if (manager != null)
                {
                    registerManager(manager);
                    return manager;
                }
                else
                    return default;
            }
        }
        /// <summary>
        /// 获取指定类型的管理器
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T getManager<T>() where T : Manager
        {
            T manager = getManager(typeof(T)) as T;
            if (manager != null)
                return manager;
            if (this != global.root)
                return global.getManager<T>();
            return default;
        }
        public override string ToString()
        {
            return "LocalManager(" + gameObject.scene.name + ")";
        }
    }
}