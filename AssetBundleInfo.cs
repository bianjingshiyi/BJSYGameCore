using UnityEngine;
using System;
using System.Collections.Generic;

namespace BJSYGameCore
{
    [CreateAssetMenu(fileName = nameof(AssetBundleInfo), menuName = nameof(BJSYGameCore) + "/" + nameof(AssetBundleInfo))]
    public class AssetBundleInfo : ScriptableObject, IDisposable
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
        /// Bundle列表
        /// </summary>
        public List<AssetBundleInfoItem> bundleList = new List<AssetBundleInfoItem>();

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
        public List<AssetInfoItem> assetList = new List<AssetInfoItem>();
        public AssetBundleInfoItem()
        {
        }
        public AssetBundleInfoItem(string name, string variant)
        {
            this.name = name;
            this.variant = variant;
        }
        public AssetBundleInfoItem(string name, string variant, params AssetInfoItem[] assets)
        {
            this.name = name;
            this.variant = variant;
            assetList.AddRange(assets);
        }
    }
    [Serializable]
    public class AssetInfoItem
    {
        public string path;
        public string assetPath;
        public AssetInfoItem()
        {
        }
        public AssetInfoItem(string assetPath)
        {
            path = assetPath;
            this.assetPath = assetPath;
        }
        public AssetInfoItem(string path, string assetPath)
        {
            this.path = path;
            this.assetPath = assetPath;
        }
    }
}