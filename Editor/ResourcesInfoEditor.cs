using System.IO;
using UnityEngine;
using System.Linq;
using UnityEditor;
using UObject = UnityEngine.Object;
using System.Collections.Generic;
using System;
using System.Text.RegularExpressions;

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
                    build(target as ResourcesInfo, path);
            }
        }

        static void BuildInfoOfResourcesAndFile(ResourcesInfo info, params ResourceInfo[] assetsInfo) {
            if(assetsInfo == null || assetsInfo.Length <= 0) {
                foreach (string path in AssetDatabase.GetAllAssetPaths().Where(p => Regex.IsMatch(p, @".\.{1}\w+"))) {
                    string[] strs = path.Split('/');
                    if (strs.Any(s => s == "Assets")) {
                        if (strs.Any(s => s == "Resources") && strs.All(s => s != "Editor")) {
                            info.resourceList.Add(new ResourceInfo {
                                type = ResourceType.Resources,
                                path = Regex.Match(path, @"/Resources/{1}\w+").ToString().removeHead("/Resources/"),
                                version = info.version
                            });
                        }
                        else if (strs.Any(s => s == "StreamingAssets")) {
                            info.resourceList.Add(new ResourceInfo {
                                type = ResourceType.File,
                                path = path,
                                version = info.version
                            });
                        }
                    }
                }
            }
            else {
                foreach (ResourceInfo assetInfo in assetsInfo)
                    info.resourceList.Add(assetInfo);
            }
        }

        public static bool build(ResourcesInfo info, string outputDir, params ResourceInfo[] assetsInfo) {
            BuildInfoOfResourcesAndFile(info,assetsInfo);
            if (info == null) { throw new ArgumentNullException(nameof(info)); }
            DirectoryInfo dirInfo = new DirectoryInfo(outputDir);
            if (!dirInfo.Exists) { dirInfo.Create(); }
            AssetImporter importer;
            Dictionary<string, string> nameCacheDic = null;
            Dictionary<string, string> variantCacheDic = null;
            if (assetsInfo != null && assetsInfo.Length > 0) {
                //先缓存并清空所有其他AssetBundle的信息
                nameCacheDic = new Dictionary<string, string>();
                variantCacheDic = new Dictionary<string, string>();
                string[] allBundlNames = AssetDatabase.GetAllAssetBundleNames();
                foreach (var otherBundleName in allBundlNames) {
                    foreach (var otherAssetPath in AssetDatabase.GetAssetPathsFromAssetBundle(otherBundleName)) {
                        if (assetsInfo.Any(a=>a.path == otherAssetPath))//是目标之一，跳过
                            continue;
                        importer = AssetImporter.GetAtPath(otherAssetPath);
                        nameCacheDic.Add(otherAssetPath, importer.assetBundleName);
                        variantCacheDic.Add(otherAssetPath, importer.assetBundleVariant);
                        importer.assetBundleName = null;
                        importer.SaveAndReimport();
                    }
                }
            }
            foreach (var assetInfo in assetsInfo.Where(a=>a.type==ResourceType.Assetbundle)) {
                importer = AssetImporter.GetAtPath(assetInfo.path);
                if(assetInfo.bundleName != null)
                    importer.assetBundleName = assetInfo.bundleName;
                importer.SaveAndReimport();
            }
            try {
                AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(outputDir,
                BuildAssetBundleOptions.StrictMode | BuildAssetBundleOptions.ChunkBasedCompression,
                EditorUserBuildSettings.activeBuildTarget);
                if (manifest != null) {
                    foreach (var bundleName in manifest.GetAllAssetBundles()) {
                        string bundlePath = Path.Combine(outputDir, bundleName);
                        AssetBundle bundle = AssetBundle.LoadFromFile(bundlePath);
                        foreach (var path in bundle.GetAllAssetNames()) {
                            ResourceInfo assetInfo = info.resourceList.Find(a => a.path == path);
                            if (assetInfo == null) { 
                                if(assetsInfo.Length>0) 
                                    assetInfo = assetsInfo.FirstOrDefault(a => a.path == path);
                                if (assetInfo == null) {
                                    assetInfo = new ResourceInfo {
                                        path = path,
                                        type = ResourceType.Assetbundle,
                                        bundleName = bundle.name,
                                        version = info.version
                                    };
                                }
                                info.resourceList.Add(assetInfo);
                            }
                                
                        }
                        bundle.Unload(true);
                    }
                }
                return true;
            }
            finally {
                if (nameCacheDic != null) {
                    //打包完毕，重置其他AssetBundle的信息。
                    foreach (var pair in nameCacheDic) {
                        importer = AssetImporter.GetAtPath(pair.Key);
                        importer.assetBundleName = pair.Value;
                        importer.assetBundleVariant = variantCacheDic[pair.Key];
                        importer.SaveAndReimport();
                    }
                }
            }
        }

        [Obsolete("这个函数可以扔进历史垃圾桶了")]
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