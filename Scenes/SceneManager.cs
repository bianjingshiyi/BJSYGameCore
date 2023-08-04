using BJSYGameCore.SaveSystem;
using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace BJSYGameCore
{
    /// <summary>
    /// 场景管理器类
    /// </summary>
    public class SceneManager
    {
        public void Initialize(GlobalManager gameManager)
        {
            _gameManager = gameManager;
        }
        // 加载场景方法
        public LoadSceneOperationBase LoadScene(string scenePath, LoadSceneMode loadMode)
        {
            // 判断是否已经在加载
            if (_loadingOperations.ContainsKey(scenePath))
            {
                return _loadingOperations[scenePath];
            }

            // 使用资源管理器加载场景，创建新的异步操作对象
            LoadSceneOperationBase operation = GameManager.ResourceManager.loadSceneAsync(scenePath, loadMode);

            // 在完成事件中，找到SceneController并设置引用
            operation.onCompleted += scene =>
            {
                // 在回调函数中，找到SceneController并设置引用
                ISceneController controller = FindSceneController(scene);
                controller.Initialize(GameManager, this); // 假设GameManager是一个单例类

                // 设置场景控制器
                _sceneControllers[scenePath] = controller;

                // 从加载中的操作移除
                _loadingOperations.Remove(scenePath);
            };

            // 将异步操作对象添加到字典中
            _loadingOperations[scenePath] = operation;

            return operation;
        }

        // 卸载场景方法
        public void UnloadScene(string scenePath)
        {
            GameManager.ResourceManager.unload(scenePath);
        }

        // 获取场景控制器方法
        public bool TryGetSceneController<T>(string scenePath, out T controller) where T : class, ISceneController
        {
            if (_sceneControllers.ContainsKey(scenePath) && _sceneControllers[scenePath] is T sceneController)
            {
                controller = sceneController;
                return true;
            }
            else
            {
                controller = null;
                return false;
            }
        }
        private ISceneController FindSceneController(Scene scene)
        {
            return scene.findInstance<ISceneController>();
        }
        public GlobalManager GameManager => _gameManager;
        // 其他的方法，例如FindSceneController等
        private GlobalManager _gameManager;
        private Dictionary<string, LoadSceneOperationBase> _loadingOperations = new Dictionary<string, LoadSceneOperationBase>();
        private Dictionary<string, LoadSceneOperationBase> _unloadingOperations = new Dictionary<string, LoadSceneOperationBase>();
        private Dictionary<string, ISceneController> _sceneControllers = new Dictionary<string, ISceneController>();
    }
    /// <summary>
    /// 场景控制器接口
    /// </summary>
    public interface ISceneController
    {
        GlobalManager GameManager { get; }
        SceneManager SceneManager { get; }

        void Initialize(GlobalManager gameManager, SceneManager sceneManager);
    }
}