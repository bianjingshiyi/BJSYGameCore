using UnityEngine;
using System;
using System.Collections.Generic;
using System.Runtime.Versioning;

namespace BJSYGameCore
{
    [CreateAssetMenu(fileName = nameof(ResourcesInfo), menuName = nameof(BJSYGameCore) + "/" + nameof(ResourcesInfo))]
    public class ResourcesInfo : ScriptableObject, IDisposable
    {
        /// <summary>
        /// AssetBundleInfo自身的版本号
        /// </summary>
        public int version = 1;
        /// <summary>
        /// AssetBundle的代码版本，这个只能重新下游戏才能更新。
        /// </summary>
        public int codeVersion = 1;
        /// <summary>
        /// 资源信息列表
        /// </summary>
        public List<ResourceInfo> resourceList = new List<ResourceInfo>();
        public ResourceInfo getInfoByPath(string path)
        {
            return resourceList.Find(r => r.path == path);
        }

        /// <summary>
        /// ManifestBundle信息
        /// </summary>
        [Obsolete("这些变量要扔进历史垃圾堆")]
        public AssetBundleInfoItem manifest = null;
        /// <summary>
        /// Bundle列表
        /// </summary>
        [Obsolete("这些变量要扔进历史垃圾堆")]
        public List<AssetBundleInfoItem> bundleList = new List<AssetBundleInfoItem>();
        public void Dispose()
        {
            resourceList.Clear();
            bundleList.Clear();
#if UNITY_EDITOR
            if (!Application.isPlaying)
                DestroyImmediate(this);
            else
                Destroy(this);
#else
            Destroy(this);
#endif
        }
#if UNITY_EDITOR
        protected void OnValidate()
        {
            //TODO:检查代码版本变更。
        }
#endif
    }
    [Serializable,Obsolete("AssetBundleInfoItem即将要废弃")]
    public class AssetBundleInfoItem
    {
        [Obsolete("这些变量要扔进历史垃圾堆")]
        /// <summary>
        /// Bundle名
        /// </summary>
        public string bundleName = null;
        [Obsolete("这些变量要扔进历史垃圾堆")]
        /// <summary>
        /// Bundle.Variant，可以当成后缀来用
        /// </summary>
        public string variant = null;
        [Obsolete("这些变量要扔进历史垃圾堆")]
        /// <summary>
        /// Bundle打包之后的相对路径
        /// </summary>
        public string path = null;
        [Obsolete("这些变量要扔进历史垃圾堆")]
        /// <summary>
        /// Bundle版本号
        /// </summary>
        public int version = 1;
        [Obsolete("这些变量要扔进历史垃圾堆")]
        /// <summary>
        /// AssetBundle的Hash
        /// </summary>
        public string hash = null;
        [Obsolete("这些变量要扔进历史垃圾堆")]
        /// <summary>
        /// CRC校验码
        /// </summary>
        public uint crc = 0;
        [Obsolete("这些变量要扔进历史垃圾堆")]
        /// <summary>
        /// bundle大小
        /// </summary>
        public long size = 0;
        [Obsolete("这些变量要扔进历史垃圾堆")]
        /// <summary>
        /// Bundle包含的Asset
        /// </summary>
        public List<ResourceInfo> assetList = new List<ResourceInfo>();
        public AssetBundleInfoItem()
        {
        }
        public AssetBundleInfoItem(string bundleName, string path)
        {
            this.bundleName = bundleName;
            this.path = path;
        }
        public AssetBundleInfoItem(string bundleName, string variant, string path)
        {
            this.bundleName = bundleName;
            this.variant = variant;
            this.path = path;
        }
        public AssetBundleInfoItem(string bundleName, string variant, params ResourceInfo[] assets)
        {
            this.bundleName = bundleName;
            this.variant = variant;
            assetList.AddRange(assets);
        }
    }
    [Serializable]
    public class ResourceInfo
    {
        /// <summary>
        /// 版本
        /// </summary>
        public int version;
        /// <summary>
        /// 资源类型
        /// </summary>
        public ResourceType type;
        /// <summary>
        /// 资源路径
        /// </summary>
        public string path;
        /// <summary>
        /// 包名
        /// </summary>
        public string bundleName;


        [SerializeField,Obsolete("这些变量要扔进历史垃圾堆")]
        string _assetPath;
        public string assetPath
        {
            get { return _assetPath; }
            set
            {
                _assetPath = value.ToLower();
            }
        }
        public ResourceInfo()
        {
        }
        [Obsolete("这些函数要扔进历史垃圾堆")]
        public ResourceInfo(string assetPath)
        {
            path = assetPath;
            this.assetPath = assetPath;
        }
        [Obsolete("这些函数要扔进历史垃圾堆")]
        public ResourceInfo(string path, string assetPath)
        {
            this.path = path;
            this.assetPath = assetPath;
        }
    }
    public enum ResourceType
    {
        Resources,
        Assetbundle,
        File
    }
}