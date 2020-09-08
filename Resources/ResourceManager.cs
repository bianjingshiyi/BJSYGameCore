using UnityEngine;
using System;
using System.Collections.Generic;
namespace BJSYGameCore
{
    public class ResourceManager : Manager
    {
        #region 公开接口
        public T load<T>(string path)
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
            else if (path.StartsWith("ab:"))
            {
                res = loadFromBundle<T>(path.Substring(3, path.Length - 3));
            }
            else
                throw new InvalidOperationException("无法加载类型为" + typeof(T).Name + "的资源" + path);
            saveToCache(path, res);
            return res;
        }
        #endregion
        #region AssetBundle
        T loadFromBundle<T>(string path)
        {
            //如果有，需要做一些关于热更新的处理，但是现在没有。
            AssetBundle bundle;
            if (!loadBundleFromCache(path, out bundle))
            {

            }
            else
            {

            }
            return default;
        }
        bool loadBundleFromCache(string path, out AssetBundle bundle)
        {
            if (bundleCacheDic.TryGetValue(path, out var item))
            {
                bundle = item.bundle;
                return true;
            }
            bundle = null;
            return false;
        }
        Dictionary<string, BundleCacheItem> bundleCacheDic { get; } = new Dictionary<string, BundleCacheItem>();
        class BundleCacheItem
        {
            public AssetBundle bundle;
        }
        #endregion
        #region 缓存
        bool loadFromCache<T>(string path, out T res)
        {
            if (cacheDic.TryGetValue(path, out var item))
            {
                if (item.wref.IsAlive)
                {
                    if (item.wref.Target is T t)
                    {
                        res = t;
                        return true;
                    }
                    else
                        throw new InvalidCastException("资源\"" + path + "\"" + item.wref.Target + "不是" + typeof(T).Name);
                }
                else
                {
                    cacheDic.Remove(path);
                    res = default;
                    return false;
                }
            }
            else
            {
                res = default;
                return false;
            }
        }
        void saveToCache(string path, object res)
        {
            if (cacheDic.TryGetValue(path, out var item) && ReferenceEquals(item.wref.Target, res))
            {
                //Debug.LogWarning("路径" + path + "已经缓存了同一个资源" + item.wref.Target + "，取消缓存" + res);
                return;
            }
            cacheDic[path] = new CacheItem() { wref = new WeakReference(res) };
        }
        Dictionary<string, CacheItem> cacheDic { get; } = new Dictionary<string, CacheItem>();
        class CacheItem
        {
            /// <summary>
            /// 对资源的弱引用。
            /// </summary>
            /// <remarks>
            /// 这么做是为了不影响GC和Unity自动卸载没有被引用的资源。
            /// </remarks>
            public WeakReference wref;
        }
        #endregion
    }
}
