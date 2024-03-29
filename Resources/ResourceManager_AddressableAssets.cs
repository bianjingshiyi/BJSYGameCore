using System.Threading.Tasks;
using System;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using UnityEngine.ResourceManagement.ResourceProviders;
using System.Collections.Generic;

namespace BJSYGameCore
{
#if ADDRESSABLE_ASSETS
    public partial class ResourceManager
    {
        #region 公有方法
        protected void loadImp<T>(string path, Action<T> onComplete)
        {
            LoadAddressableAssetOperation<T> operation = new LoadAddressableAssetOperation<T>(path, typeof(T));
            addLoadOperation(operation);
            operation.onCompleted += obj =>
            {
                _pathHandleDict[path] = operation.handle;
                removeLoadOperation(operation);
                onComplete?.Invoke((T)obj);
            };
        }
        protected LoadResourceOperationBase loadImp<T>(string path)
        {
            LoadAddressableAssetOperation<T> operation = new LoadAddressableAssetOperation<T>(path, typeof(T));
            addLoadOperation(operation);
            operation.onCompleted += obj =>
            {
                _pathHandleDict[path] = operation.handle;
                removeLoadOperation(operation);
            };
            return operation;
        }
        protected async Task<T> loadImp<T>(string path, string dir)
        {
            LoadAddressableAssetOperation<T> operation = new LoadAddressableAssetOperation<T>(path, typeof(T));
            addLoadOperation(operation);
            await operation.task;
            _pathHandleDict[path] = operation.handle;
            removeLoadOperation(operation);
            return (T)operation.resource;
        }
        protected void unloadImp(string path)
        {
            Addressables.Release(_pathHandleDict[path]);
            _pathHandleDict.Remove(path);
        }
        protected LoadSceneOperationBase loadSceneImp(string path, LoadSceneMode loadSceneMode = LoadSceneMode.Additive, bool activeOnLoad = true, int priority = 100)
        {
            var operation = new LoadAddressableSceneOperation(path, loadSceneMode, activeOnLoad, priority);
            addLoadSceneOperation(operation);
            operation.onComplete += scene =>
            {
                _pathSceneInstanceDict[path] = operation.sceneInstance;
                removeLoadSceneOperation(operation);
            };
            return operation;
        }
        protected void unloadSceneImp(string path, Action onComplete)
        {
            var operation = new UnloadAddressableSceneOperation(path, _pathSceneInstanceDict[path]);
            _pathSceneInstanceDict.Remove(path);
            addUnloadSceneOperation(operation);
            operation.onComplete += scene =>
            {
                removeUnloadSceneOperation(operation);
                onComplete?.Invoke();
            };
        }
        #endregion
        #region 属性字段
        private Dictionary<string, AsyncOperationHandle> _pathHandleDict = new Dictionary<string, AsyncOperationHandle>();
        private Dictionary<string, SceneInstance> _pathSceneInstanceDict = new Dictionary<string, SceneInstance>();
        #endregion
        #region 内部类
        class UnloadAddressableSceneOperation : LoadSceneOperationBase
        {
            public UnloadAddressableSceneOperation(string path, SceneInstance sceneInstance)
            {
                this.path = path;
                _handle = Addressables.UnloadSceneAsync(sceneInstance, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
                _handle.Completed += OnComplete;
            }

            private void OnComplete(AsyncOperationHandle<SceneInstance> handle)
            {
                Complete(handle.Result.Scene);
            }

            public override string path { get; }

            public override Task task => _handle.Task;
            public override float progress => _handle.PercentComplete;
            AsyncOperationHandle<SceneInstance> _handle;
        }
        class LoadAddressableSceneOperation : LoadSceneOperationBase
        {
            public LoadAddressableSceneOperation(string path, LoadSceneMode loadMode, bool activeOnLoad, int priority)
            {
                this.path = path;
                _handle = Addressables.LoadSceneAsync(path, loadMode, activeOnLoad, priority);
                _handle.Completed += OnComplete;
            }

            private void OnComplete(AsyncOperationHandle<SceneInstance> handle)
            {
                Complete(handle.Result.Scene);
            }

            public override string path { get; }

            public override Task task => _handle.Task;

            public override float progress => _handle.PercentComplete;

            public SceneInstance sceneInstance => _handle.Result;

            AsyncOperationHandle<SceneInstance> _handle;
        }
        class LoadAddressableAssetOperation<T> : LoadResourceOperationBase
        {
            public LoadAddressableAssetOperation(string path, Type type)
            {
                this.path = path;
                this.type = type;
                handle = Addressables.LoadAssetAsync<T>(path);
                handle.Completed += OnComplete;
            }

            private void OnComplete(AsyncOperationHandle<T> handle)
            {
                Complete();
            }

            public override string path { get; }

            public override Type type { get; }

            public override Task task => handle.Task;

            public override float progress => handle.PercentComplete;

            public override object resource => handle.Result;

            public AsyncOperationHandle<T> handle;
        }
        #endregion
    }
#endif
}