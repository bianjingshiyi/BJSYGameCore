﻿using System;

using UnityEngine;
using UnityEngine.SceneManagement;

using BJSYGameCore;

namespace BJSYGameCore
{
    /// <summary>
    /// 管理器
    /// </summary>
    public class Manager : MonoBehaviour
    {
        /// <summary>
        /// 本地管理器
        /// </summary>
        public LocalManager local
        {
            get
            {
                if (_local == null)
                {
                    _local = this.findInstance<LocalManager>();
                    if (_local == null)
                    {
                        _local = new GameObject("LocalManager").AddComponent<LocalManager>();
                        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(_local.gameObject, gameObject.scene);
                    }
                    _local.registerManager(this);
                }
                return _local;
            }
            internal set
            {
                _local = value;
            }
        }
        LocalManager _local;
        protected void Awake()
        {
            onAwake();
        }
        protected virtual void onAwake()
        {
        }
        internal protected virtual void onSceneLoad(string scenePath)
        {
        }
        internal protected virtual void onSceneLoaded(string scenePath)
        {
        }
        internal protected virtual void onSceneUnload(string scenePath)
        {
        }
        internal protected virtual void onSceneUnloaded(string scenePath)
        {
        }
        public T getManager<T>() where T : Manager
        {
            return local.getManager<T>();
        }
        public Manager getManager(Type type)
        {
            return local.getManager(type);
        }
        public LoadSceneOperation loadSceneAsync(string scenePath, LoadSceneMode loadMode, Action callback = null)
        {
            return local.global.loadSceneAsync(scenePath, loadMode, callback);
        }
        public UnloadSceneOperation unloadSceneAsync(string scenePath, Action callback = null)
        {
            return local.global.unloadSceneAsync(scenePath, callback);
        }
    }
    public interface IManager<T>
    {
        public virtual void Initialize(T gameManager)
        {
            this.gameManager = gameManager;
        }
        public T gameManager { get; set; }
    }
}