using UnityEngine;
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

namespace BJSYGameCore
{
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
            //先尝试从缓存中获取
            if (loadFromCache(path, out Object t))
            {
                return t;
            }
            //缓存中不存在
            t = Resources.Load(path);
            if (t == null)
            {
                Debug.LogError($"从Resources同步加载{path}失败");
            }
            else
            {
                saveToCache(path, t);
            }
            return t;
        }
        public T loadFromResources<T>(string path) where T : Object
        {
            //先尝试从缓存中获取
            if (loadFromCache(path, out T t))
            {
                return t;
            }
            //缓存中不存在
            t = Resources.Load<T>(path);
            if (t == null)
            {
                Debug.LogError($"从Resources同步加载{path}失败");
            }
            else
            {
                saveToCache(path, t);
            }
            return t;
        }
        /// <summary>
        /// 异步方法，从Resources中加载资源。
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns></returns>
        public Task<T> loadFromResourcesAsync<T>(string path) where T : Object
        {
            //先尝试从缓存中获取
            if (loadFromCache(path, out T t))
            {
                return Task.FromResult(t);
            }
            //加载资源
            TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();
            ResourceRequest req = Resources.LoadAsync(path);
            req.completed += op =>
            {
                Object obj = (op as ResourceRequest).asset;
                if (obj is T t)
                {
                    saveToCache(path, t);
                    tcs.SetResult(t);
                }
                else
                {
                    if (obj == null)
                        tcs.SetException(new NullReferenceException("加载资源" + path + "为空"));
                    else
                        tcs.SetException(new InvalidCastException("加载资源" + path + "的结果" + obj + "无法转化为" + typeof(T).Name));
                }
            };
            return tcs.Task;
        }
        /// <summary>
        /// 异步方法，从Resources中加载资源。
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns></returns>
        public Task<Object> loadFromResourcesAsync(string path)
        {
            //先尝试从缓存中获取
            if (loadFromCache(path, out Object obj))
            {
                return Task.FromResult(obj);
            }
            //加载资源
            TaskCompletionSource<Object> tcs = new TaskCompletionSource<Object>();
            ResourceRequest req = Resources.LoadAsync(path);
            req.completed += op =>
            {
                Object obj = (op as ResourceRequest).asset;
                if (obj != null)
                {
                    saveToCache(path, obj);
                    tcs.SetResult(obj);
                }
                else
                    tcs.SetException(new NullReferenceException("加载资源" + path + "为空"));
            };
            return tcs.Task;
        }
        #endregion

        #region Assetbundle加载资源
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
        public T loadFromAssetBundle<T>(string path) where T : Object
        {
            //先尝试从缓存中获取
            if (loadFromCache(path, out T t))
            {
                return t;
            }
            //不知道这个资源在哪个AssetBundle里面，通过ResourceInfo来获取
            ResourceInfo info = resourcesInfo.getInfoByPath(path);
            if (info == null)
                throw new NullReferenceException("加载资源" + path + "失败，未找到相关资源信息");
            AssetBundle assetBundle = loadAssetBundle(info);
            if (assetBundle == null)
                throw new NullReferenceException("加载资源" + path + "失败，未能加载AssetBundle");
            t = assetBundle.LoadAsset<T>(path);
            if (t == null)
                throw new NullReferenceException("加载资源" + path + "失败，资源为空");
            saveToCache(path, t);
            return t;
        }
        /// <summary>
        /// 异步方法，从AssetBundle中加载资源。
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public async Task<T> loadFromAssetBundleAsync<T>(string path) where T : Object
        {
            //先实现一个比较暴力的，穷举AssetBundle来寻找资源
            string assetBundlePath = Path.Combine(
                resourcesInfo.bundleOutputPath,
                new DirectoryInfo(resourcesInfo.bundleOutputPath).Name);
            byte[] bytes;
            if (!loadAssetBundleFromCache(assetBundlePath, out AssetBundle assetBundle))
            {
                bytes = await game.fileManager.readStreamingBinaryFile(assetBundlePath);
                assetBundle = await loadAssetBundleFromBytes(bytes);
                saveBundleToCache(assetBundlePath, assetBundle);
            }
            if (assetBundle == null)
                throw new NullReferenceException("无法加载资源" + path + "，由于Manifest加载失败");
            AssetBundleManifest manifest = assetBundle.LoadAsset<AssetBundleManifest>(nameof(AssetBundleManifest));
            foreach (string assetBundleName in manifest.GetAllAssetBundles())
            {
                assetBundlePath = Path.Combine(
                    resourcesInfo.bundleOutputPath,
                    assetBundleName);
                if (!loadAssetBundleFromCache(assetBundlePath, out assetBundle))
                {
                    bytes = await game.fileManager.readStreamingBinaryFile(assetBundlePath);
                    assetBundle = await loadAssetBundleFromBytes(bytes);
                    saveBundleToCache(assetBundlePath, assetBundle);
                }
                T t = await loadFromAssetBundleAsync<T>(assetBundle, path);
                if (t != null)
                    return t;
                //else
                //    assetBundle.Unload(false);
            }
            return null;
            ////上一步找不到，就加载AB包并Load出UObject
            //AssetBundle infoBundle = await loadAssetBundleAsync(info);
            //if (infoBundle == null) { return default; }
            ////加载资源
            //T newObj = await loadObjFromBundleAsync<T>(infoBundle, info.path);
            //if (newObj == null)
            //{
            //    Debug.LogError($"从AB包异步加载{info.path}失败");
            //}
            //else { saveToCache(info.path, newObj); }
            //return newObj;
        }

        /// <summary>
        /// 同步方法，根据ResourceInfo加载AB包
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public AssetBundle loadAssetBundle(ResourceInfo info)
        {
            //尝试从缓存中查找AssetBundle
            if (loadAssetBundleFromCache(info.bundleName, out AssetBundle assetBundle))
            {
                return assetBundle;
            }
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
            assetBundle = loadBundleFromFile(info.bundleName);
            if (assetBundle == null)
            {
                Debug.LogError($"同步加载AB包{info.bundleName}失败");
                return null;
            }
            else
            {
                var cacheItem = saveBundleToCache(info.bundleName, assetBundle);
                cacheItem.dependenceList = dependenceList;
            }
            return assetBundle;
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
        #endregion
        #region 私有方法
        private Task<AssetBundle> loadAssetBundleFromBytes(byte[] bytes)
        {
            TaskCompletionSource<AssetBundle> tcs = new TaskCompletionSource<AssetBundle>();
            AssetBundleCreateRequest request = AssetBundle.LoadFromMemoryAsync(bytes);
            request.completed += op =>
            {
                AssetBundle assetBundle = (op as AssetBundleCreateRequest).assetBundle;
                if (assetBundle != null)
                    tcs.SetResult(assetBundle);
                else
                    tcs.SetResult(null);
            };
            return tcs.Task;
        }
        private Task<T> loadFromAssetBundleAsync<T>(AssetBundle assetBundle, string path) where T : Object
        {
            TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();
            AssetBundleRequest request = assetBundle.LoadAssetAsync<T>(path);
            request.completed += op =>
            {
                Object obj = (op as AssetBundleRequest).asset;
                if (obj is T t)
                    tcs.SetResult(t);
                else
                    tcs.SetResult(default);
            };
            return tcs.Task;
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


    }
}
