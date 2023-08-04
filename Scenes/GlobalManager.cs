using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace BJSYGameCore
{
    /// <summary>
    /// 全局管理器，用于管理场景的加载以及对场景中LocalManager的管理。
    /// </summary>
    public class GlobalManager : MonoBehaviour
    {
        protected void Awake()
        {
            if (_resourceManager == null)
                _resourceManager = this.findInstance<ResourceManager>();
            _resourceManager.Initialize(this);
        }

        Dictionary<string, LocalManager> localDic { get; } = new Dictionary<string, LocalManager>();
        /// <summary>
        /// 根场景管理器
        /// </summary>
        public LocalManager root
        {
            get
            {
                if (!localDic.ContainsKey(gameObject.scene.path))
                {
                    LocalManager local = this.findInstance<LocalManager>();
                    if (local == null)
                    {
                        local = new GameObject("LocalManager").AddComponent<LocalManager>();
                        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(local.gameObject, gameObject.scene);
                    }
                    registerLocal(local);
                }
                return localDic[gameObject.scene.path];
            }
        }
        /// <summary>
        /// 获取根场景中的管理器
        /// </summary>
        /// <typeparam name="T">管理器类型</typeparam>
        /// <returns>管理器</returns>
        public T getManager<T>() where T : Manager
        {
            return root.getManager<T>();
        }
        /// <summary>
        /// 获取根场景中的管理器
        /// </summary>
        /// <param name="t">管理器类型</param>
        /// <returns>管理器</returns>
        public Manager getManager(Type t)
        {
            return root.getManager(t);
        }
        /// <summary>
        /// 已注册的局部管理器。
        /// </summary>
        public LocalManager[] locals
        {
            get { return localDic.Values.ToArray(); }
        }
        /// <summary>
        /// 注册局部管理器。
        /// </summary>
        /// <param name="local"></param>
        public void registerLocal(LocalManager local)
        {
            if (!localDic.ContainsKey(local.gameObject.scene.path))
            {
                localDic.Add(local.gameObject.scene.path, local);
                local.global = this;
            }
        }
        public void unregisterLocal(LocalManager local)
        {
            string scenePath = null;
            foreach (var p in localDic)
            {
                if (p.Value == local)
                {
                    scenePath = p.Key;
                    break;
                }
            }
            if (scenePath != null)
            {
                localDic.Remove(scenePath);
                local.global = null;
            }
        }
        /// <summary>
        /// 清除已经被销毁的local
        /// </summary>
        public void checkLocals()
        {
            List<string> removeList = new List<string>();
            foreach (var p in localDic)
            {
                if (p.Value == null)
                    removeList.Add(p.Key);
            }
            foreach (string key in removeList)
            {
                localDic.Remove(key);
            }
        }
        /// <summary>
        /// 异步加载场景。
        /// </summary>
        /// <param name="scenePath"></param>
        /// <param name="loadMode"></param>
        /// <returns></returns>
        public LoadSceneOperation loadSceneAsync(string scenePath, LoadSceneMode loadMode, Action callback = null)
        {
            //触发加载场景事件
            foreach (LocalManager local in locals)
            {
                foreach (Manager manager in local.managers)
                {
                    manager.onSceneLoad(scenePath);
                }
            }
            //加载场景
            LoadSceneOperation operation = new LoadSceneOperation(scenePath, loadMode);
            operation.onSceneLoaded += Operation_onSceneLoaded;
            operation.onSceneLoaded += o =>
            {
                callback?.Invoke();
            };
            return operation;
        }
        private void Operation_onSceneLoaded(LoadSceneOperation operation)
        {
            //按道理来讲场景里的东西应该都Awake过了，以防万一还是检查一下场景里的LocalManager吧。
            Scene scene = UnityEngine.SceneManagement.SceneManager.GetSceneByPath(operation.scenePath);
            LocalManager currentLocal = scene.findInstance<LocalManager>();
            if (currentLocal == null)
            {
                GameObject gameObject = new GameObject("LocalManager");
                UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(gameObject, scene);
                currentLocal = gameObject.AddComponent<LocalManager>();
            }
            registerLocal(currentLocal);
            //触发场景加载完毕事件
            foreach (LocalManager local in locals)
            {
                foreach (Manager manager in local.managers)
                {
                    manager.onSceneLoaded(operation.scenePath);
                }
            }
        }
        public UnloadSceneOperation unloadSceneAsync(string scenePath, Action callback = null)
        {
            if (localDic.ContainsKey(scenePath))
            {
                unregisterLocal(localDic[scenePath]);
            }
            else
                return null;
            foreach (LocalManager local in locals)
            {
                foreach (Manager manager in local.managers)
                {
                    manager.onSceneUnload(scenePath);
                }
            }
            UnloadSceneOperation operation = new UnloadSceneOperation(scenePath);
            operation.onSceneUnloaded += Operation_onSceneUnloaded;
            operation.onSceneUnloaded += o =>
            {
                callback?.Invoke();
            };
            return operation;
        }
        private void Operation_onSceneUnloaded(UnloadSceneOperation operation)
        {
            checkLocals();
            foreach (LocalManager local in locals)
            {
                foreach (Manager manager in local.managers)
                {
                    manager.onSceneUnloaded(operation.scenePath);
                }
            }
        }
        /// <summary>
        /// 获取局部管理器
        /// </summary>
        /// <param name="path">局部管理器所在场景路径</param>
        /// <returns>局部管理器</returns>
        public LocalManager getLocal(string path)
        {
            return localDic[path];
        }
        public ResourceManager ResourceManager => _resourceManager;
        private ResourceManager _resourceManager;
    }
}