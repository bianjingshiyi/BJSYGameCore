using System.IO;
using UnityEngine;
using System.Linq;
using UnityEditor;
using UObject = UnityEngine.Object;
using System.Collections.Generic;
using System;

namespace BJSYGameCore
{
    [CustomEditor(typeof(AssetBundleInfo), true)]
    public class AssetBundleInfoEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("打包AssetBundle"))
            {
                string path = EditorUtility.SaveFolderPanel("保存AssetBundle", Application.streamingAssetsPath, "AssetBundles");
                if (Directory.Exists(path))
                    build(target as AssetBundleInfo, path);
            }
        }
        public static bool build(AssetBundleInfo info, string outputDir, params AssetBundleInfoItem[] bundlesInfo)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));
            DirectoryInfo dirInfo = new DirectoryInfo(outputDir);
            if (!dirInfo.Exists)
                dirInfo.Create();
            AssetImporter importer;
            Dictionary<string, string> nameCacheDic = null;
            Dictionary<string, string> variantCacheDic = null;
            if (bundlesInfo != null && bundlesInfo.Length > 0)
            {
                //先缓存并清空所有其他AssetBundle的信息
                nameCacheDic = new Dictionary<string, string>();
                variantCacheDic = new Dictionary<string, string>();
                string[] allBundlNames = AssetDatabase.GetAllAssetBundleNames();
                foreach (var otherBundleName in allBundlNames)
                {
                    foreach (var otherAssetPath in AssetDatabase.GetAssetPathsFromAssetBundle(otherBundleName))
                    {
                        if (bundlesInfo.Any(b => b.assetList.Any(a => a.assetPath == otherAssetPath)))//是目标之一，跳过
                            continue;
                        importer = AssetImporter.GetAtPath(otherAssetPath);
                        nameCacheDic.Add(otherAssetPath, importer.assetBundleName);
                        variantCacheDic.Add(otherAssetPath, importer.assetBundleVariant);
                        importer.assetBundleName = null;
                        importer.SaveAndReimport();
                    }
                }
            }
            //暂时清空了其他物体的AssetBundle信息，可以开始设置并打包AssetBundle了
            foreach (var bundleInfo in bundlesInfo)
            {
                foreach (var assetInfo in bundleInfo.assetList)
                {
                    importer = AssetImporter.GetAtPath(assetInfo.assetPath);
                    importer.assetBundleName = bundleInfo.name;
                    if (!string.IsNullOrEmpty(bundleInfo.variant))
                        importer.assetBundleVariant = bundleInfo.variant;
                    importer.SaveAndReimport();
                }
            }
            try
            {
                AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(outputDir,
                BuildAssetBundleOptions.StrictMode | BuildAssetBundleOptions.ChunkBasedCompression,
                EditorUserBuildSettings.activeBuildTarget);
                //打包完毕，处理AssetBundleInfo，首先处理生成的Manifest
                if (manifest != null)
                {
                    foreach (var bundleName in manifest.GetAllAssetBundles().Append(dirInfo.Name))
                    {
                        AssetBundle bundle = AssetBundle.LoadFromFile(outputDir + "/" + bundleName);
                        AssetBundleInfoItem item = info.bundleList.Find(b => b.name == bundle.name);
                        if (item == null)
                        {
                            item = new AssetBundleInfoItem(bundleName, null);
                            info.bundleList.Add(item);
                        }
                        item.assetList.Clear();
                        item.assetList.AddRange(bundle.GetAllAssetNames().Select(p =>
                            new AssetInfoItem(bundlesInfo.First(b =>
                                b.name == bundle.name).assetList.First(a =>
                                    a.assetPath == p).path, p)));
                        bundle.Unload(true);
                    }
                    return true;
                }
                else
                    return false;
            }
            finally
            {
                if (nameCacheDic != null)
                {
                    //打包完毕，重置其他AssetBundle的信息。
                    foreach (var pair in nameCacheDic)
                    {
                        importer = AssetImporter.GetAtPath(pair.Key);
                        importer.assetBundleName = pair.Value;
                        importer.assetBundleVariant = variantCacheDic[pair.Key];
                        importer.SaveAndReimport();
                    }
                }
            }
        }
    }
}