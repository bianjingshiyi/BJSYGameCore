using System;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace BJSYGameCore
{
    public class UnloadSceneOperation
    {
        public string scenePath { get; }
        public AsyncOperation operation { get; }
        public UnloadSceneOperation(string scenePath)
        {
            this.scenePath = scenePath;
            operation = SceneManager.UnloadSceneAsync(scenePath, UnloadSceneOptions.None);
            operation.completed += Operation_completed;
        }
        private void Operation_completed(AsyncOperation obj)
        {
            onSceneUnloaded?.Invoke(this);
        }
        public event Action<UnloadSceneOperation> onSceneUnloaded;
    }
}