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
        [SerializeField]
        List<LocalManager> _locals;
        /// <summary>
        /// 已注册的局部管理器。
        /// </summary>
        public LocalManager[] locals
        {
            get { return _locals.ToArray(); }
        }
        /// <summary>
        /// 注册局部管理器。
        /// </summary>
        /// <param name="local"></param>
        public void registerLocal(LocalManager local)
        {
            if (!_locals.Contains(local))
            {
                _locals.Add(local);
                local.global = this;
            }
        }
        /// <summary>
        /// 异步加载场景。
        /// </summary>
        /// <param name="scenePath"></param>
        /// <param name="loadMode"></param>
        /// <returns></returns>
        public LoadSceneOperation loadSceneAsync(string scenePath, LoadSceneMode loadMode)
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
    }
}