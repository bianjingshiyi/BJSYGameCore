using System.IO;
using UnityEngine;
using System.Linq;
using UnityEditor;
using UObject = UnityEngine.Object;
using System.Collections.Generic;
using System;

namespace BJSYGameCore
{
    [CustomEditor(typeof(ResourcesInfo), true)]
    public class ResourcesInfoEditor : Editor
    {
        const string RESOURCES_BUNDLENAME = "Resources";
        const string STREAMINGASSETS_BUNDLENAME = "StreamingAssets";

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("打包AssetBundle"))
            {
                string path = EditorUtility.SaveFolderPanel("保存AssetBundle", Application.streamingAssetsPath, "AssetBundles");
                if (Directory.Exists(path))
                    Build(target as ResourcesInfo, path);
            }
        }

        static void GetAllFileNamesInPath(string path, ref List<string> allFileNames) {
            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            foreach(FileInfo info in directoryInfo.GetFiles()) {
                if(info.Extension!=".meta")
                    allFileNames.Add(info.Name);
            }
            foreach(DirectoryInfo info in directoryInfo.GetDirectories()) {
                GetAllFileNamesInPath(info.FullName, ref allFileNames);
            }
        }

        public static bool Build(ResourcesInfo info, string outputDir, params ResourceInfo[] assetsInfo) {
            if (info == null) { throw new ArgumentNullException(nameof(info)); }
            DirectoryInfo dirInfo = new DirectoryInfo(outputDir);
            if (!dirInfo.Exists) { dirInfo.Create(); }
            if (assetsInfo == null || assetsInfo.Length <= 0) {
                string resourcesPath = Application.streamingAssetsPath.ToLower().Replace("streamingassets", "resources");
                List<string> fileNamesInResources = new List<string>();
                GetAllFileNamesInPath(resourcesPath,ref fileNamesInResources);
                foreach (string fileName in fileNamesInResources) {
                    info.resourceList.Add(new ResourceInfo {
                        type = ResourceType.Resources,
                        path = fileName.Split('.')[0].ToLower(),
                        version = info.version
                    });
                }
                List<string> fileNamesInStreamingAssets = new List<string>();
                GetAllFileNamesInPath(Application.streamingAssetsPath, ref fileNamesInStreamingAssets);
                foreach (string fileName in fileNamesInStreamingAssets) {
                    info.resourceList.Add(new ResourceInfo {
                        type = ResourceType.File,
                        path = ("Assets/StreamingAssets/"+fileName).ToLower(),
                        version = info.version
                    });
                }
                AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(outputDir,
                BuildAssetBundleOptions.StrictMode | BuildAssetBundleOptions.ChunkBasedCompression,
                EditorUserBuildSettings.activeBuildTarget);
                foreach(var bundleName in manifest.GetAllAssetBundles()) {
                    string bundlePath = Path.Combine(outputDir, bundleName);
                    AssetBundle bundle = AssetBundle.LoadFromFile(bundlePath);
                    foreach(var path in bundle.GetAllAssetNames()) {
                        info.resourceList.Add(new ResourceInfo {
                            path = path,
                            type = ResourceType.Assetbundle,
                            bundleName = bundle.name,
                            version = info.version
                        });
                    }
                }
                return true;
            }
            else {
                // todo ： assetsInfo存在时，增量打包功能
                return false;
            }
        }

        public static bool build(ResourcesInfo info, string outputDir, AssetBundleInfoItem[] bundlesInfo)
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
                    importer.assetBundleName = bundleInfo.bundleName;
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
                    string manifestBundleName = dirInfo.Name;
                    AssetBundle bundle = AssetBundle.LoadFromFile(outputDir + "/" + manifestBundleName);
                    info.manifest = new AssetBundleInfoItem(manifestBundleName, outputDir + "/" + manifestBundleName);
                    info.manifest.assetList.AddRange(bundle.GetAllAssetNames().Select(p => new ResourceInfo(bundle.name, p)));
                    bundle.Unload(true);
                    foreach (var bundleName in manifest.GetAllAssetBundles())
                    {
                        bundle = AssetBundle.LoadFromFile(outputDir + "/" + bundleName);
                        AssetBundleInfoItem item = info.bundleList.Find(b => b.bundleName == bundle.name);
                        if (item == null)
                        {
                            item = new AssetBundleInfoItem(bundleName, outputDir + "/" + bundleName);
                            info.bundleList.Add(item);
                        }
                        item.assetList.Clear();
                        foreach (var assetName in bundle.GetAllAssetNames())
                        {
                            AssetBundleInfoItem bundleInfo = bundlesInfo.First(b => (string.IsNullOrEmpty(b.variant) ? b.bundleName.ToLower() : (b.bundleName + "." + b.variant).ToLower()) == bundle.name);
                            ResourceInfo assetInfo = bundleInfo.assetList.Find(a => a.assetPath == assetName);
                            item.assetList.Add(new ResourceInfo(assetInfo.path, assetName));
                        }
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