﻿using UnityEngine;
using System;
using System.Collections.Generic;
using Object = UnityEngine.Object;
using JetBrains.Annotations;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System.Collections;
using UnityEngine.Networking;
using System.Net.Http;
using System.Data;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace BJSYGameCore
{
#if !ADDRESSABLE_ASSETS
    public partial class ResourceManager
    {

        #region AssetDatabase加载资源
#if UNITY_EDITOR
        /// <summary>
        /// 从AssetDatabase中加载资源。
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public Object loadFromAssetDatabase(ResourceInfo info)
        {
            Object obj = null;
            if (loadFromCache<Object>(info.path, out obj)) { return obj; }
            obj = UnityEditor.AssetDatabase.LoadAssetAtPath<Object>(info.path);
            if (obj == null) { Debug.LogError($"从AssetDatabase加载{info.path}失败"); }
            saveToCache(info.path, obj);
            return obj;
        }
#endif
    #endregion

        #region Resources加载资源
        /// <summary>
        /// 同步方法，从Resources中加载资源。
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public Object loadFromResources(string path)
        {
            //获取path对应的ResourceInfo
            ResourceInfo info = resourcesInfo.getInfoByPath(path);
            if (info == null) { return null; }
            //先尝试从缓存中获取
            if (!loadFromCache(info.path, out Object obj))
            {
                obj = Resources.Load(info.path);
                if (obj == null)
                {
                    Debug.LogError($"从Resources同步加载{info.path}失败");
                }
                else { saveToCache(info.path, obj); }
            }
            return obj;
        }

        /// <summary>
        /// 异步方法，从Resources中加载资源。
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns></returns>
        public Task<T> loadFromResourcesAsync<T>(string path) where T : UnityEngine.Object
        {
            TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();
            //获取path对应的ResourceInfo
            ResourceInfo info = resourcesInfo.getInfoByPath(path);
            if (info == null) { tcs.SetCanceled(); }
            //先尝试从缓存中获取
            else if (!loadFromCache(info.path, out T obj))
            {
                var req = Resources.LoadAsync(info.path);
                req.completed += op =>
                {
                    T newObj = (op as ResourceRequest).asset as T;
                    if (newObj == null)
                    {
                        Debug.LogError($"从Resources异步加载{info.path}失败");
                    }
                    else { saveToCache(info.path, newObj); }
                    tcs.SetResult(newObj);
                };
            }
            else { tcs.SetResult(obj); }
            return tcs.Task;
        }
        #endregion

        #region Assetbundle加载资源
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
        #endregion
        [Obsolete("ResourceInfo是针对AssetBundle的，在使用其他资源加载方案的时候不应该使用这个方法。")]
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
        [Obsolete]
        public T loadFromBundle<T>(ResourcesInfo abInfo, string path)
        {
            if (loadFromAssetBundle(abInfo, path) is T t)
                return t;
            else
                return default;
        }
        /// <summary>
        /// 同步方法，从AssetBundle中加载资源。
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public Object loadFromAssetBundle(string path)
        {
            //获取path对应的ResourceInfo
            ResourceInfo info = resourcesInfo.getInfoByPath(path);
            if (info == null) { return null; }
            //先尝试从缓存中获取
            if (!loadFromCache<Object>(info.path, out var obj))
            {
                //上一步找不到，就加载AB包并Load出UObject
                AssetBundle infoBundle = loadAssetBundle(info);
                if (infoBundle == null) { return null; }
                //加载资源
                obj = infoBundle.LoadAsset(info.path);
                if (obj == null)
                {
                    Debug.LogError($"从AB包同步加载{info.path}失败");
                }
                else { saveToCache(info.path, obj); }
            }
            return obj;
        }

        /// <summary>
        /// 异步方法，从AssetBundle中加载资源。
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public async Task<T> loadFromAssetBundleAsync<T>(string path) where T : UnityEngine.Object
        {
            //获取path对应的ResourceInfo
            ResourceInfo info = resourcesInfo.getInfoByPath(path);
            if (info == null) { return default; }
            //先尝试从缓存中获取
            if (!loadFromCache<T>(info.path, out var obj))
            {
                //上一步找不到，就加载AB包并Load出UObject
                AssetBundle infoBundle = await loadAssetBundleAsync(info);
                if (infoBundle == null) { return default; }
                //加载资源
                T newObj = await loadObjFromBundleAsync<T>(infoBundle, info.path);
                if (newObj == null)
                {
                    Debug.LogError($"从AB包异步加载{info.path}失败");
                }
                else { saveToCache(info.path, newObj); }
                return newObj;
            }
            else { return obj; }
        }

        /// <summary>
        /// 同步方法，根据ResourceInfo加载AB包
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public AssetBundle loadAssetBundle(ResourceInfo info)
        {
            //尝试在AB包Cache去获取infoBundle
            if (!loadAssetBundleFromCache(info.bundleName, out var infoBundle))
            {
                //上一步找不到，就去加载info的AB包，
                //在这之前要加载manifest
                ResourceInfo manifestInfo = resourcesInfo.getInfoByPath("ab:assetbundlemanifest");
                AssetBundleManifest manifest = loadAssetBundleManifest(manifestInfo);
                if (manifest == null) { return null; }
                //然后根据manifest加载依赖项
                var dependencies = manifest.GetDirectDependencies(info.bundleName);
                List<string> dependenceList = null;
                if (dependencies != null && dependencies.Length > 0)
                {
                    foreach (var dependence in dependencies)
                    {
                        AssetBundle dependencedBundle = loadBundleFromFile(dependence);
                        if (dependencedBundle == null)
                        {
                            Debug.LogError("加载" + info.bundleName + "的依赖项" + dependence + "失败");
                            return null;
                        }
                        else
                        {
                            //记录依赖
                            if (dependenceList == null) { dependenceList = new List<string>(); }
                            dependenceList.Add(dependence);
                            saveBundleToCache(dependence, dependencedBundle);
                        }
                    }
                }
                //加载完依赖项之后，可以正式加载AB包了
                infoBundle = loadBundleFromFile(info.bundleName);
                if (infoBundle == null)
                {
                    Debug.LogError($"同步加载AB包{info.bundleName}失败");
                    return null;
                }
                else
                {
                    var cacheItem = saveBundleToCache(info.bundleName, infoBundle);
                    cacheItem.dependenceList = dependenceList;
                }
            }
            return infoBundle;
        }

        /// <summary>
        /// 异步方法，根据ResourceInfo加载AB包
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public async Task<AssetBundle> loadAssetBundleAsync(ResourceInfo info)
        {
            //尝试在AB包Cache去获取infoBundle
            if (!loadAssetBundleFromCache(info.bundleName, out var infoBundle))
            {
                //上一步找不到，就去加载info的AB包，
                //在这之前要加载manifest
                ResourceInfo manifestInfo = resourcesInfo.getInfoByPath("ab:assetbundlemanifest");
                AssetBundleManifest manifest = await loadAssetBundleManifestAsync(manifestInfo);
                if (manifest == null) { return null; }
                //然后根据manifest加载依赖项
                var dependencies = manifest.GetDirectDependencies(info.bundleName);
                List<string> dependenceList = null;
                if (dependencies != null && dependencies.Length > 0)
                {
                    foreach (var dependence in dependencies)
                    {
                        AssetBundle dependencedBundle = await loadBundleFromWeb(dependence);
                        if (dependencedBundle == null)
                        {
                            Debug.LogError("异步加载" + info.bundleName + "的依赖项" + dependence + "失败");
                            return null;
                        }
                        else
                        {
                            //记录依赖
                            if (dependenceList == null)
                                dependenceList = new List<string>();
                            dependenceList.Add(dependence);
                            saveBundleToCache(dependence, dependencedBundle);
                        }
                    }
                }
                //加载完依赖项之后，可以正式加载AB包了
                var newInfoBundle = await loadBundleFromWeb(info.bundleName);
                if (newInfoBundle == null)
                {
                    Debug.LogError($"异步加载AB包{info.bundleName}失败");
                    return null;
                }
                else
                {
                    var cacheItem = saveBundleToCache(info.bundleName, newInfoBundle);
                    cacheItem.dependenceList = dependenceList;
                }
                return newInfoBundle;
            }
            else { return infoBundle; }
        }
        public bool loadAssetBundleFromCache(string name, out AssetBundle bundle)
        {
            if (bundleCacheDic.TryGetValue(name, out var item))
            {
                bundle = item.bundle;
                return true;
            }
            bundle = null;
            return false;
        }
        #endregion

        #region 和AssetBundle加载相关的工具方法
        // 同步方法，加载manifest
        AssetBundleManifest loadAssetBundleManifest(ResourceInfo manifestInfo)
        {
            //尝试在UObject缓存找manifest
            if (!loadFromCache<AssetBundleManifest>(manifestInfo.path, out var manifest))
            {
                //上一步找不到，就在AB包缓存去找manifest的AB包并Load出manifest
                if (!loadAssetBundleFromCache(manifestInfo.bundleName, out var manifestBundle))
                {
                    //同步加载manifest的AB包
                    manifestBundle = loadBundleFromFile(manifestInfo.bundleName);
                    if (manifestBundle == null)
                    {
                        Debug.LogError("同步加载ManifestAB包失败，Manifest无法加载");
                        return null;
                    }
                    else { saveBundleToCache(manifestInfo.path, manifestBundle); }
                }
                //从AB包Load出manifest
                manifest = manifestBundle.LoadAsset<AssetBundleManifest>(manifestInfo.path);
                if (manifest == null)
                {
                    Debug.LogError("同步加载Manifest失败");
                    return null;
                }
                else { saveToCache(manifestInfo.path, manifest); }
            }
            return manifest;
        }

        //异步方法，加载manifest
        async Task<AssetBundleManifest> loadAssetBundleManifestAsync(ResourceInfo manifestInfo)
        {
            //尝试在UObject缓存找manifest
            if (!loadFromCache<AssetBundleManifest>(manifestInfo.path, out var manifest))
            {
                //上一步找不到，就在AB包缓存去找manifest的AB包并Load出manifest
                if (!loadAssetBundleFromCache(manifestInfo.bundleName, out var manifestBundle))
                {
                    //异步加载manifest的AB包
                    var newManifestBundle = await loadBundleFromWeb(manifestInfo.bundleName);
                    if (newManifestBundle == null)
                    {
                        Debug.LogError("异步加载AssetBundleManifest失败");
                        return null;
                    }
                    else { saveBundleToCache(manifestInfo.bundleName, newManifestBundle); }
                    //从AB包异步加载manifest
                    var newManifest = await loadObjFromBundleAsync<AssetBundleManifest>(newManifestBundle, manifestInfo.path);
                    if (newManifest == null)
                    {
                        Debug.LogError("异步加载Manifest失败");
                        return null;
                    }
                    else { saveToCache(manifestInfo.path, newManifest); }
                    return newManifest;
                }
                else
                {
                    //找到了manifest的AB包，可以直接异步加载manifest
                    var newManifest = await loadObjFromBundleAsync<AssetBundleManifest>(manifestBundle, manifestInfo.path);
                    if (newManifest == null)
                    {
                        Debug.LogError("异步加载Manifest失败");
                        return null;
                    }
                    else { saveToCache(manifestInfo.path, newManifest); }
                    return newManifest;
                }
            }
            else { return manifest; }
        }

        //同步方法，UnityAPI加载AB包
        AssetBundle loadBundleFromFile(string bundleName)
        {
            return AssetBundle.LoadFromFile(resourcesInfo.bundleOutputPath + "/" + bundleName);
        }

        //异步方法，UnityAPI加载AB包
        Task<AssetBundle> loadBundleFromWeb(string bundleName)
        {
            if (startLoadBundleOperation(bundleName, out var lbo))
            {
                TaskCompletionSource<AssetBundle> tcs = lbo.tcs;
                string uri = Application.streamingAssetsPath + "/" +
                Path.GetFileNameWithoutExtension(resourcesInfo.bundleOutputPath) + "/" + bundleName;
                //尝试从缓存获取AB包
                if (!loadAssetBundleFromCache(bundleName, out var resBundle))
                {
                    //缓存拿不到就加载AB包
                    UnityWebRequestAssetBundle.GetAssetBundle(uri).SendWebRequest().completed += op =>
                    {
                        var uop = op as UnityWebRequestAsyncOperation;
                        //抛出加载异常
                        if (uop.webRequest.isNetworkError)
                        {
                            tcs.SetException(new HttpRequestException(uop.webRequest.error));
                            return;
                        }
                        if (uop.webRequest.isHttpError)
                        {
                            if (uop.webRequest.responseCode == 404)
                            {
                                tcs.SetException(new FileNotFoundException());
                            }
                            else
                            {
                                tcs.SetException(new HttpRequestException(uop.webRequest.error));
                            }
                            return;
                        }
                        //拿到AB包
                        tcs.SetResult(DownloadHandlerAssetBundle.GetContent(uop.webRequest));
                        completeLoadBundleOperation(bundleName);
                    };
                }
                else
                {
                    tcs.SetResult(resBundle);
                    completeLoadBundleOperation(bundleName);
                }
            }
            return lbo.tcs.Task;
        }

        //异步方法，从AB包里面加载Asset(资源)
        Task<T> loadObjFromBundleAsync<T>(AssetBundle bundle, string assetPath) where T : UnityEngine.Object
        {
            TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();
            bundle.LoadAssetAsync<T>(assetPath).completed += op =>
            {
                T obj = (op as AssetBundleRequest).asset as T;
                if (obj == null)
                    Debug.LogError($"从AB包加载{assetPath}失败");
                tcs.SetResult(obj);
            };
            return tcs.Task;
        }


        //缓存加载过的AB包
        BundleCacheItem saveBundleToCache(string name, AssetBundle bundle)
        {
            BundleCacheItem cacheItem = new BundleCacheItem() { bundle = bundle };
            bundleCacheDic[name] = cacheItem;
            return cacheItem;
        }
        //缓存AB包的数据结构
        Dictionary<string, BundleCacheItem> bundleCacheDic { get; } = new Dictionary<string, BundleCacheItem>();
        [Serializable]
        class BundleCacheItem
        {
            public AssetBundle bundle = null;
            public List<string> dependenceList = null;
        }

        //记录异步加载AB包任务的数据结构
        class LoadBundleOperation
        {
            public TaskCompletionSource<AssetBundle> tcs = new TaskCompletionSource<AssetBundle>();
        }
        Dictionary<string, LoadBundleOperation> loadBundleOperationDict = new Dictionary<string, LoadBundleOperation>();
        //开始一个加载AB包异步任务的操作
        bool startLoadBundleOperation(string bundleName, out LoadBundleOperation lbo)
        {
            if (!loadBundleOperationDict.ContainsKey(bundleName))
            {
                lbo = new LoadBundleOperation();
                loadBundleOperationDict[bundleName] = lbo;
                return true;
            }
            else
            {
                lbo = loadBundleOperationDict[bundleName];
                return false;
            }
        }
        //结束一个加载AB包异步任务的操作
        void completeLoadBundleOperation(string bundleName)
        {
            if (loadBundleOperationDict.ContainsKey(bundleName))
                loadBundleOperationDict.Remove(bundleName);
        }

        #endregion

        //暂时不知道有什么卵用.....
        ResourceInfo getResourceInfo(string path)
        {
            throw new NotImplementedException();
        }


        #region 废弃方法，不知道还用不用得着，先留着
        [Obsolete]
        void checkPathIsValid(string path)
        {
            //路径格式校验
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("资源地址不能为空", nameof(path));
            int index = path.Replace('\\', '/').IndexOf('/');
            if (index < 0)
                throw new ArgumentException(path + "不是有效的资源地址格式", nameof(path));
        }
        [Obsolete]
        public Object loadFromAssetBundle(ResourceInfo info)
        {
            checkPathIsValid(info.path);
            //直接去UObject缓存找UObject
            if (!loadFromCache<Object>(info.path, out var obj))
            {
                //上一步找不到，就加载AB包并Load出UObject
                AssetBundle infoBundle = loadAssetBundle(info);
                obj = infoBundle.LoadAsset(info.path);
                if (obj == null)
                {
                    Debug.LogError($"从AB包加载{info.path}失败");
                    return null;
                }
                else { saveToCache(info.path, obj); }
            }
            return obj;
        }
        [Obsolete]
        public Object loadFromResources(ResourceInfo info)
        {
            checkPathIsValid(info.path);
            Object obj;
            if (loadFromCache(info.path, out obj))
                return obj;
            obj = Resources.Load(info.path);
            if (obj == null)
                Debug.LogError($"从Resources加载{info.path}失败");
            saveToCache(info.path, obj);
            return obj;
        }
        /// <summary>
        /// 从AssetBundle中加载资源。
        /// </summary>
        /// <param name="abInfo">AssetBundle相关信息</param>
        /// <param name="path">资源路径，格式为包名/资源相对路径</param>
        /// <returns></returns>
        [Obsolete]
        public Object loadFromAssetBundle(ResourcesInfo abInfo, string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("资源地址不能为空", nameof(path));
            int index = path.Replace('\\', '/').IndexOf('/');
            if (index < 0)
                throw new ArgumentException(path + "不是有效的资源地址格式", nameof(path));
            string bundleName = path.Substring(0, index).ToLower();
            AssetBundleInfoItem bundleInfo = abInfo.bundleList.Find(b => b.bundleName == bundleName);
            if (bundleInfo != null)
            {
                string assetPath = path.Substring(index + 1, path.Length - index - 1);
                ResourceInfo assetInfo = bundleInfo.assetList.Find(a => a.path == assetPath);
                if (assetInfo != null)
                {
                    AssetBundle bundle = loadBundle(abInfo, bundleInfo);
                    Object asset = bundle.LoadAsset(assetInfo.assetPath);
                    saveToCache(path, asset);
                    return asset;
                }
            }
            return null;
        }
        [Obsolete]
        AssetBundle loadBundle(ResourcesInfo bundleInfo, AssetBundleInfoItem itemInfo)
        {
            //尝试从缓存中加载
            if (loadAssetBundleFromCache(itemInfo.bundleName, out var bundle))
            {
                return bundle;
            }
            //没有缓存，先获取Bundle依赖性
            if (!loadAssetBundleFromCache(bundleInfo.manifest_old.bundleName, out var manifestBundle))
            {
                manifestBundle = loadBundleFromFileOrWeb(bundleInfo.manifest_old);
                if (manifestBundle == null)
                {
                    Debug.LogError("加载AssetBundleManifest失败");
                    return null;
                }
                saveBundleToCache(bundleInfo.manifest_old.bundleName, manifestBundle);
            }
            AssetBundleManifest manifest = manifestBundle.LoadAsset<AssetBundleManifest>(bundleInfo.manifest_old.assetList[0].assetPath);
            //加载依赖Bundle
            var dependencies = manifest.GetDirectDependencies(itemInfo.bundleName);
            List<string> dependenceList = null;
            if (dependencies != null && dependencies.Length > 0)
            {
                foreach (var dependence in dependencies)
                {
                    AssetBundle dependencedBundle = loadBundle(bundleInfo, bundleInfo.bundleList.Find(b => b.bundleName == dependence));
                    if (dependencedBundle == null)
                    {
                        Debug.LogError("加载" + itemInfo.bundleName + "的依赖项" + dependence + "失败");
                        return null;
                    }
                    else
                    {
                        //记录依赖
                        if (dependenceList == null)
                            dependenceList = new List<string>();
                        dependenceList.Add(dependence);
                    }
                }
            }
            bundle = loadBundleFromFileOrWeb(itemInfo);
            if (bundle != null)
            {
                var cacheItem = saveBundleToCache(itemInfo.bundleName, bundle);
                cacheItem.dependenceList = dependenceList;
                return bundle;
            }
            else
                return null;
        }
        [Obsolete]
        AssetBundle loadBundleFromFileOrWeb(AssetBundleInfoItem info)
        {
            //TODO:AssetBundle加载的Web实现。
            return AssetBundle.LoadFromFile(info.path);
        }
        #endregion

        [SerializeField]
        ResourcesInfo _resourcesInfo;
        public ResourcesInfo resourcesInfo
        {
            get { return _resourcesInfo; }
            set { _resourcesInfo = value; }
        }
    }
#endif
}
