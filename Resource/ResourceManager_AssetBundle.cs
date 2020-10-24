using UnityEngine;
using System;
using System.Collections.Generic;
using UObject = UnityEngine.Object;
namespace BJSYGameCore
{
    public partial class ResourceManager
    {
        #region 公共成员
        /// <summary>
        /// 从Resources中加载资源。
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public UObject loadFromResources(ResourceInfo info)
        {
            return Resources.Load(info.path);
        }
        /// <summary>
        /// 从AssetBundle中加载资源。
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public UObject loadFromAssetBundle(ResourceInfo info)
        {
            throw new NotImplementedException();
        }
#if UNITY_EDITOR
        /// <summary>
        /// 从AssetDatabase中加载资源。
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public UObject loadFromAssetDatabase(ResourceInfo info)
        {
            return UnityEditor.AssetDatabase.LoadAssetAtPath<UObject>(info.path);
        }
#endif
        /// <summary>
        /// 从AssetBundle中加载资源。
        /// </summary>
        /// <param name="abInfo">AssetBundle相关信息</param>
        /// <param name="path">资源路径，格式为包名/资源相对路径</param>
        /// <returns></returns>
        public UObject loadFromAssetBundle(ResourcesInfo abInfo, string path)
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
                    UObject asset = bundle.LoadAsset(assetInfo.assetPath);
                    saveToCache(path, asset);
                    return asset;
                }
            }
            return null;
        }
        #endregion
        #region 私有成员
        ResourceInfo getResourceInfo(string path)
        {
            throw new NotImplementedException();
        }
        #endregion
        AssetBundle loadBundle(ResourcesInfo bundleInfo, AssetBundleInfoItem itemInfo)
        {
            //尝试从缓存中加载
            if (loadBundleFromCache(itemInfo.bundleName, out var bundle))
            {
                return bundle;
            }
            //没有缓存，先获取Bundle依赖性
            if (!loadBundleFromCache(bundleInfo.manifest.bundleName, out var manifestBundle))
            {
                manifestBundle = loadBundleFromFileOrWeb(bundleInfo.manifest);
                if (manifestBundle == null)
                {
                    Debug.LogError("加载AssetBundleManifest失败");
                    return null;
                }
                saveBundleToCache(bundleInfo.manifest.bundleName, manifestBundle);
            }
            AssetBundleManifest manifest = manifestBundle.LoadAsset<AssetBundleManifest>(bundleInfo.manifest.assetList[0].assetPath);
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
        AssetBundle loadBundleFromFileOrWeb(AssetBundleInfoItem info)
        {
            //TODO:AssetBundle加载的Web实现。
            return AssetBundle.LoadFromFile(info.path);
        }
        bool loadBundleFromCache(string name, out AssetBundle bundle)
        {
            if (bundleCacheDic.TryGetValue(name, out var item))
            {
                bundle = item.bundle;
                return true;
            }
            bundle = null;
            return false;
        }
        BundleCacheItem saveBundleToCache(string name, AssetBundle bundle)
        {
            BundleCacheItem cacheItem = new BundleCacheItem() { bundle = bundle };
            bundleCacheDic[name] = cacheItem;
            return cacheItem;
        }
        Dictionary<string, BundleCacheItem> bundleCacheDic { get; } = new Dictionary<string, BundleCacheItem>();
        [Serializable]
        class BundleCacheItem
        {
            public AssetBundle bundle = null;
            public List<string> dependenceList = null;
        }
    }
}
