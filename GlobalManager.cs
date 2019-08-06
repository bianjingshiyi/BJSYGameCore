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
                        SceneManager.MoveGameObjectToScene(local.gameObject, gameObject.scene);
                    }
                    registerLocal(local);
                }
                return localDic[gameObject.scene.path];
            }
        }
        public T getManager<T>() where T : Manager
        {
            return root.getManager<T>();
        }
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
            registerLocal(SceneManager.GetSceneByPath(operation.scenePath).findInstance<LocalManager>());
            //触发场景加载完毕事件
            foreach (LocalManager local in locals)
            {
                foreach (Manager manager in local.managers)
                {
                    manager.onSceneLoaded(operation.scenePath);
                }
            }
        }
        public LocalManager getLocal(string path)
        {
            return localDic[path];
        }
    }
}