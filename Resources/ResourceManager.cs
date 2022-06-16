using UnityEngine;
using System;
using System.Threading.Tasks;
using BJSYGameCore.UI;
using System.CodeDom;
using System.Collections.Generic;
namespace BJSYGameCore
{
    public partial class ResourceManager : Manager, IDisposable, IResourceManager
    {
        #region 公有方法
        /// <summary>
        /// 同步的加载一个资源
        /// </summary>
        /// <typeparam name="T">资源类型</typeparam>
        /// <param name="info">资源信息</param>
        /// <returns>加载的资源</returns>
        public T load<T>(ResourceInfo info) where T : UnityEngine.Object
        {
            if (typeof(T) == typeof(ResourcesInfo))
            {
                if (resourcesInfo.resourceList.Contains(info))
                    return resourcesInfo as T;
                else
                {
                    Debug.LogError($"ResourceManager::resoucesInfo里面没有{info.path}");
                    return null;
                }
            }
            else if (typeof(T) == typeof(AssetBundleManifest))
            {
                return loadAssetBundleManifest(info) as T;
            }
            else
            {
                switch (info.type)
                {
                    case ResourceType.Assetbundle:
                        if (typeof(T) == typeof(AssetBundle))
                        {
                            return loadAssetBundle(info) as T;
                        }
                        else return loadFromAssetBundle(info.path) as T;
                    case ResourceType.Resources:
                        return loadFromResources(info.path) as T;
                    case ResourceType.File:
                        //using (UnityWebRequest req = UnityWebRequest.Get(Application.streamingAssetsPath + info.path)) {
                        //    req.SendWebRequest();
                        //    while (!req.isDone) {Debug.Log("loading"); }
                        //    req.downloadHandler.data;
                        //}
                        //todo  : 不知道该如何处理，先放着.....
                        return null;
                }
                return null;
            }
        }
        /// <summary>
        /// 异步的加载一个资源。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
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
                res = (T)operation.resource;
            }
            else
            {
                throw new InvalidOperationException("无法加载资源" + path);
            }
            saveToCache(path, res);
            return res;
        }
        #endregion
        #region 私有方法
        protected void addLoadOperation(LoadResourceOperationBase operation)
        {
            if (!_loadOpDict.ContainsKey(operation.path))
                _loadOpDict[operation.path] = operation;
            else
            {
                List<LoadResourceOperationBase> list = _loadOpDict[operation.path] as List<LoadResourceOperationBase>;
                if (list == null)
                {
                    list = new List<LoadResourceOperationBase>();
                    list.Add(_loadOpDict[operation.path] as LoadResourceOperationBase);
                    _loadOpDict[operation.path] = list;
                }
                list.Add(operation);
            }
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
        #endregion

        #region 废弃方法，不知道还用不用得着，先留着
        /// <summary>
        /// 同步的加载一个资源。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <returns></returns>
        public T load<T>(string path, string dir = null)
        {
            T res;
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("路径不能为空", nameof(path));
            else if (loadFromCache<T>(path, out var cachedRes))//尝试从缓存中加载
            {
                return cachedRes;
            }
            else if (path.StartsWith("res:"))//尝试从资源中加载
            {
                var uRes = Resources.Load(path.Substring(4, path.Length - 4));
                if (uRes == null)
                    res = default;
                else if (uRes is T t)
                    res = t;
                else
                    throw new InvalidCastException("资源\"" + path + "\"" + uRes + "不是" + typeof(T).Name);
            }
            else if (path.StartsWith("ab:") && resourcesInfo != null)
            {
                res = loadFromBundle<T>(resourcesInfo, path.Substring(3, path.Length - 3));
            }
            else
                throw new InvalidOperationException("无法加载类型为" + typeof(T).Name + "的资源" + path);
            saveToCache(path, res);
            return res;
        }
        [Obsolete]
        public T loadFromBundle<T>(ResourcesInfo abInfo, string path)
        {
            if (loadFromAssetBundle(abInfo, path) is T t)
                return t;
            else
                return default;
        }
        #endregion

        public void Dispose()
        {
            cacheDic.Clear();
#if UNITY_EDITOR
            if (!Application.isPlaying)
                DestroyImmediate(gameObject);
            else
                Destroy(gameObject);
#else
            Destroy(gameObject);
#endif
        }
        #region 字段
        /// <summary>
        /// 正在加载资源的LoadResourceOperation字典，值可能是LoadResouceOperation，也可能是一个List。
        /// 之所以只用object是因为大部分情况下同一个路径下不会有多个有不同类型的资源，在这种情况下不使用List。
        /// </summary>
        Dictionary<string, object> _loadOpDict = new Dictionary<string, object>();
        [SerializeField]
        ResourcesInfo _resourcesInfo;
        public ResourcesInfo resourcesInfo
        {
            get { return _resourcesInfo; }
            set { _resourcesInfo = value; }
        }
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
        }
        public override string path => _path;
        public override Type type => _type;
        public override Task task => _tcs.Task;
        public override object resource => _request.asset;
        string _path;
        Type _type;
        ResourceRequest _request;
        TaskCompletionSource<object> _tcs;
    }
    public abstract class LoadResourceOperationBase
    {
        public abstract string path { get; }
        public abstract Type type { get; }
        public abstract Task task { get; }
        public abstract object resource { get; }
    }
}
