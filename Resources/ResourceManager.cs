using UnityEngine;
using System;
using System.Threading.Tasks;
using BJSYGameCore.UI;
using System.CodeDom;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
namespace BJSYGameCore
{
    public partial class ResourceManager : Manager, IDisposable, IResourceManager
    {
        #region 公有方法
        public void Initialize(GlobalManager gameManager)
        {
            _gameManager = gameManager;
        }
        /// <summary>
        /// 异步的加载一个资源。
        /// </summary>
        /// <typeparam name="T">资源类型</typeparam>
        /// <param name="path">资源路径</param>
        /// <returns></returns>
        public virtual async Task<T> loadAsync<T>(string path, string dir)
        {
            T res;
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("路径不能为空", nameof(path));
            if (loadFromCache<T>(path, out var cachedRes))
            {
                //有缓存资源，直接返回缓存资源
                return cachedRes;
            }
            else if (tryGetLoadOperation(path, typeof(T), out LoadResourceOperationBase operation))
            {
                //正在加载这个资源，等待加载过程完成并返回，避免重复加载
                await operation.task;
                res = (T)operation.resource;
            }
            else if (path.StartsWith(PATH_RES_PREFIX))
            {
                //从资源中加载
                ResourceRequest request = Resources.LoadAsync(path.Substring(PATH_RES_PREFIX.Length, path.Length - PATH_RES_PREFIX.Length), typeof(T));
                operation = new LoadResourceOperation(path, typeof(T), request);
                addLoadOperation(operation);
                await operation.task;
                removeLoadOperation(operation);
                res = (T)operation.resource;
            }
            else
            {
                res = await loadImp<T>(path, dir);
            }
            saveToCache(path, res);
            return res;
        }
        public virtual void loadAsync<T>(string path, Action<T> callback)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("路径不能为空", nameof(path));
            if (loadFromCache(path, out T cachedRes))
            {
                //有缓存资源，直接返回缓存资源
                callback?.Invoke(cachedRes);
            }
            else if (tryGetLoadOperation(path, typeof(T), out var operation))
            {
                //正在加载这个资源，等待加载过程完成并返回，避免重复加载
                operation.onCompleted += (res) => callback?.Invoke((T)res);
            }
            else if (path.StartsWith(PATH_RES_PREFIX))
            {
                //从资源中加载
                ResourceRequest request = Resources.LoadAsync(path.Substring(PATH_RES_PREFIX.Length, path.Length - PATH_RES_PREFIX.Length), typeof(T));
                operation = new LoadResourceOperation(path, typeof(T), request);
                addLoadOperation(operation);
                operation.onCompleted += (res) =>
                {
                    removeLoadOperation(operation);
                    saveToCache(path, res);
                    callback?.Invoke((T)res);
                };
            }
            else
            {
                loadImp<T>(path, res =>
                {
                    saveToCache(path, res);
                    callback?.Invoke(res);
                });
            }
        }
        public virtual LoadResourceOperationBase loadAsync<T>(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("路径不能为空", nameof(path));
            if (loadFromCache(path, out object cachedRes))
            {
                //有缓存资源，直接返回缓存资源
                return new LoadCompletedResourceOperation(path, typeof(T), cachedRes);
            }
            else if (tryGetLoadOperation(path, typeof(T), out LoadResourceOperationBase operation))
            {
                //正在加载这个资源，等待加载过程完成并返回，避免重复加载
                return operation;
            }
            else if (path.StartsWith(PATH_RES_PREFIX))
            {
                //从资源中加载
                ResourceRequest request = Resources.LoadAsync(path.Substring(PATH_RES_PREFIX.Length, path.Length - PATH_RES_PREFIX.Length), typeof(T));
                operation = new LoadResourceOperation(path, typeof(T), request);
                addLoadOperation(operation);
                operation.onCompleted += res =>
                {
                    removeLoadOperation(operation);
                    saveToCache(path, res);
                };
                return operation;
            }
            else
            {
                operation = loadImp<T>(path);
                operation.onCompleted += res =>
                {
                    saveToCache(path, res);
                };
                return operation;
            }
        }
        public void unload(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("路径不能为空", nameof(path));
            removeFromCache(path);
            unloadImp(path);
        }
        public LoadSceneOperationBase loadSceneAsync(string path, LoadSceneMode loadMode)
        {
            Scene scene = UnityEngine.SceneManagement.SceneManager.GetSceneByPath(path);
            if (scene.isLoaded)
            {
                return null;
            }
            else if (_loadSceneOpDict.ContainsKey(path))
            {
                return _loadSceneOpDict[path];
            }
            else if (IsBuiltinScene(path))
            {
                LoadBuiltinSceneOperation operation = new LoadBuiltinSceneOperation(path, loadMode);
                addLoadSceneOperation(operation);
                operation.onComplete += (s) =>
                {
                    removeLoadSceneOperation(operation);
                };
                return operation;
            }
            else
            {
                return loadSceneImp(path, loadMode);
            }
        }
        public new void unloadSceneAsync(string path, Action onComplete)
        {
            Scene scene = UnityEngine.SceneManagement.SceneManager.GetSceneByPath(path);
            if (!scene.isLoaded)
            {
                onComplete?.Invoke();
                return;
            }
            else if (_unloadSceneOpDict.ContainsKey(path))
            {
                _unloadSceneOpDict[path].onComplete += _ => onComplete?.Invoke();
            }
            else if (IsBuiltinScene(path))
            {
                UnloadBuiltinSceneOperation operation = new UnloadBuiltinSceneOperation(path);
                addUnloadSceneOperation(operation);
                operation.onComplete += _ =>
                {
                    removeUnloadSceneOperation(operation);
                    onComplete?.Invoke();
                };
            }
            else
            {
                unloadSceneImp(path, onComplete);
            }
        }
        #endregion
        #region 私有方法
        protected void addLoadOperation(LoadResourceOperationBase operation)
        {
            if (!_loadOpDict.ContainsKey(operation.path))
            {
                _loadOpDict[operation.path] = operation;
            }
            else
            {
                if (_loadOpDict[operation.path] is not List<LoadResourceOperationBase> list)
                {
                    list = new List<LoadResourceOperationBase>
                    {
                        _loadOpDict[operation.path] as LoadResourceOperationBase
                    };
                    _loadOpDict[operation.path] = list;
                }
                list.Add(operation);
            }
        }
        protected void addLoadSceneOperation(LoadSceneOperationBase operation)
        {
            _loadSceneOpDict[operation.path] = operation;
        }
        protected void addUnloadSceneOperation(LoadSceneOperationBase operation)
        {
            _unloadSceneOpDict[operation.path] = operation;
        }
        protected bool tryGetLoadOperation(string path, Type type, out LoadResourceOperationBase operation)
        {
            if (!_loadOpDict.ContainsKey(path))
            {
                operation = null;
                return false;
            }
            if (_loadOpDict[path] is LoadResourceOperationBase)
            {
                operation = _loadOpDict[path] as LoadResourceOperationBase;
                return true;
            }
            else
            {
                List<LoadResourceOperationBase> list = _loadOpDict[path] as List<LoadResourceOperationBase>;
                operation = list.Find(o => type.IsAssignableFrom(o.type));
                return operation != null;
            }
        }
        protected bool tryGetLoadSceneOperation(string path, out LoadSceneOperationBase operation)
        {
            return _loadSceneOpDict.TryGetValue(path, out operation);
        }
        protected bool tryGetUnloadSceneOperation(string path, out LoadSceneOperationBase operation)
        {
            return _unloadSceneOpDict.TryGetValue(path, out operation);
        }
        protected bool removeLoadOperation(LoadResourceOperationBase operation)
        {
            if (_loadOpDict.TryGetValue(operation.path, out object obj))
            {
                if (obj is List<LoadResourceOperationBase> list)
                    return list.Remove(operation);
                else
                    return _loadOpDict.Remove(operation.path);
            }
            else
                return false;
        }
        protected bool removeLoadSceneOperation(LoadSceneOperationBase operation)
        {
            return _loadSceneOpDict.Remove(operation.path);
        }
        protected bool removeUnloadSceneOperation(LoadSceneOperationBase operation)
        {
            return _unloadSceneOpDict.Remove(operation.path);
        }
        private bool IsBuiltinScene(string scenePath)
        {
            int sceneCount = UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings;
            for (int i = 0; i < sceneCount; i++)
            {
                if (SceneUtility.GetScenePathByBuildIndex(i) == scenePath)
                    return true;
            }
            return false;
        }
        #endregion

        public void Dispose()
        {
            cacheDic.Clear();
            _loadOpDict.Clear();
            _loadSceneOpDict.Clear();
#if UNITY_EDITOR
            if (!Application.isPlaying)
                DestroyImmediate(gameObject);
            else
                Destroy(gameObject);
#else
            Destroy(gameObject);
#endif
        }
        #region 属性字段
        public GlobalManager GameManager => _gameManager;
        private GlobalManager _gameManager;
        /// <summary>
        /// 正在加载资源的LoadResourceOperation字典，值可能是LoadResouceOperation，也可能是一个List。
        /// 之所以只用object是因为大部分情况下同一个路径下不会有多个有不同类型的资源，在这种情况下不使用List。
        /// </summary>
        private Dictionary<string, object> _loadOpDict = new Dictionary<string, object>();
        private Dictionary<string, object> _unloadOpDict = new Dictionary<string, object>();
        private Dictionary<string, LoadSceneOperationBase> _loadSceneOpDict = new Dictionary<string, LoadSceneOperationBase>();
        private Dictionary<string, LoadSceneOperationBase> _unloadSceneOpDict = new Dictionary<string, LoadSceneOperationBase>();
        protected const string PATH_RES_PREFIX = "res:";
        #endregion
    }
    public class LoadResourceOperation : LoadResourceOperationBase
    {
        public LoadResourceOperation(string path, Type type, ResourceRequest request)
        {
            _path = path;
            _type = type;
            _request = request;
            _tcs = new TaskCompletionSource<object>();
            _request.completed += onComplete;
        }
        private void onComplete(AsyncOperation op)
        {
            _tcs.SetResult(null);
            Complete();
        }
        public override string path => _path;
        public override Type type => _type;
        public override Task task => _tcs.Task;
        public override float progress => _request.progress;
        public override object resource => _request.asset;
        string _path;
        Type _type;
        ResourceRequest _request;
        TaskCompletionSource<object> _tcs;
    }
    public class LoadCompletedResourceOperation : LoadResourceOperationBase
    {
        public LoadCompletedResourceOperation(string path, Type type, object resource)
        {
            this.path = path;
            this.type = type;
            this.resource = resource;
        }

        public override string path { get; }

        public override Type type { get; }

        public override Task task => Task.FromResult(resource);

        public override float progress => 1;

        public override object resource { get; }

        public override event Action<object> onCompleted
        {
            add
            {
                value?.Invoke(resource);
            }
            remove
            {
                // do nothing
            }
        }
    }
    public abstract class LoadResourceOperationBase : IAsyncOperation
    {
        protected void Complete()
        {
            onCompleted?.Invoke(resource);
        }

        public abstract string path { get; }
        public abstract Type type { get; }
        public abstract Task task { get; }
        public abstract float progress { get; }
        public abstract object resource { get; }
        public virtual event Action<object> onCompleted;
        event Action<object> IAsyncOperation.onComplete
        {
            add
            {
                onCompleted += value;
            }
            remove
            {
                onCompleted -= value;
            }
        }
    }
    public abstract class LoadSceneOperationBase : IAsyncOperation
    {
        protected void Complete(Scene scene)
        {
            onComplete?.Invoke(scene);
        }

        public abstract string path { get; }
        public abstract Task task { get; }

        public abstract float progress { get; }

        public event Action<object> onComplete;
    }
    public class LoadBuiltinSceneOperation : LoadSceneOperationBase
    {
        public LoadBuiltinSceneOperation(string path, LoadSceneMode loadMode)
        {
            this.path = path;
            _op = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(path, loadMode);
            _tcs = new TaskCompletionSource<Scene>();
            _op.completed += OnComplete;
        }

        private void OnComplete(AsyncOperation obj)
        {
            Scene scene = UnityEngine.SceneManagement.SceneManager.GetSceneByPath(path);
            _tcs.SetResult(scene);
            Complete(scene);
        }

        public override string path { get; }

        public override Task task => _tcs.Task;

        public override float progress => _op.progress;

        AsyncOperation _op;

        TaskCompletionSource<Scene> _tcs;
    }
    public class UnloadBuiltinSceneOperation : LoadSceneOperationBase
    {
        public UnloadBuiltinSceneOperation(string path)
        {
            this.path = path;
            _op = UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(path);
            _tcs = new TaskCompletionSource<object>();
            _op.completed += OnComplete;
        }

        private void OnComplete(AsyncOperation obj)
        {
            _tcs.SetResult(null);
            Complete(default);
        }

        public override string path { get; }

        public override Task task => _tcs.Task;

        public override float progress => _op.progress;
        AsyncOperation _op;
        TaskCompletionSource<object> _tcs;
    }
    public interface IAsyncOperation
    {
        float progress { get; }
        event Action<object> onComplete;
    }
}
