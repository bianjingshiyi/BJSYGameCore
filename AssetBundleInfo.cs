using UnityEngine;
using System;
using System.Collections.Generic;

namespace BJSYGameCore
{
    [CreateAssetMenu(fileName = nameof(AssetBundleInfo), menuName = nameof(BJSYGameCore) + "/" + nameof(AssetBundleInfo))]
    public class AssetBundleInfo : ScriptableObject
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
    }
    [Serializable]
    public class AssetBundleInfoItem
    {
        /// <summary>
        /// Bundle名
        /// </summary>
        public string name = null;
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
        public List<string> assetList = new List<string>();
    }
}