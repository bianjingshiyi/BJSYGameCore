using System;
using System.Collections.Generic;
using UnityEngine;
namespace BJSYGameCore
{
    public partial class ResourceManager
    {
        #region 公共成员
        public bool loadFromCache<T>(string path, out T res)
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
        #region 私有成员
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
