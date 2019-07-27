using System;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace BJSYGameCore
{
    public class LoadSceneOperation
    {
        public string scenePath { get; }
        public AsyncOperation operation { get; }
        public LoadSceneOperation(string scenePath, LoadSceneMode loadMode)
        {
            this.scenePath = scenePath;
            operation = SceneManager.LoadSceneAsync(scenePath, loadMode);
            operation.completed += Operation_completed;
        }
        private void Operation_completed(AsyncOperation obj)
        {
            onSceneLoaded?.Invoke(this);
        }
        public event Action<LoadSceneOperation> onSceneLoaded;
    }
}