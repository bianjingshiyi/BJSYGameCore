using UnityEngine;
using System;
using System.Collections.Generic;

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
        /// ManifestBundle信息
        /// </summary>
        public AssetBundleInfoItem manifest = null;
        /// <summary>
        /// Bundle列表
        /// </summary>
        public List<AssetBundleInfoItem> bundleList = new List<AssetBundleInfoItem>();
        /// <summary>
        /// 资源信息列表
        /// </summary>
        public List<ResourceInfo> resourceList = new List<ResourceInfo>();
        public ResourceInfo getInfoByPath(string path)
        {
            throw new NotImplementedException();
        }
        public void Dispose()
        {
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
    [Serializable]
    public class AssetBundleInfoItem
    {
        /// <summary>
        /// Bundle名
        /// </summary>
        public string name = null;
        /// <summary>
        /// Bundle.Variant，可以当成后缀来用
        /// </summary>
        public string variant = null;
        /// <summary>
        /// Bundle打包之后的相对路径
        /// </summary>
        public string path = null;
        /// <summary>
        /// Bundle版本号
        /// </summary>
        public int version = 1;
        /// <summary>
        /// AssetBundle的Hash
        /// </summary>
        public string hash = null;
        /// <summary>
        /// CRC校验码
        /// </summary>
        public uint crc = 0;
        /// <summary>
        /// bundle大小
        /// </summary>
        public long size = 0;
        /// <summary>
        /// Bundle包含的Asset
        /// </summary>
        public List<ResourceInfo> assetList = new List<ResourceInfo>();
        public AssetBundleInfoItem()
        {
        }
        public AssetBundleInfoItem(string name, string path)
        {
            this.name = name;
            this.path = path;
        }
        public AssetBundleInfoItem(string name, string variant, string path)
        {
            this.name = name;
            this.variant = variant;
            this.path = path;
        }
        public AssetBundleInfoItem(string name, string variant, params ResourceInfo[] assets)
        {
            this.name = name;
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
        [SerializeField]
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
        public ResourceInfo(string assetPath)
        {
            path = assetPath;
            this.assetPath = assetPath;
        }
        public ResourceInfo(string path, string assetPath)
        {
            this.path = path;
            this.assetPath = assetPath;
        }
    }
    public enum ResourceType
    {
        resource,
        assetbundle,
        file
    }
}