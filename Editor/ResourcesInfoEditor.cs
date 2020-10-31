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

        static void BuildInfoOfResourcesAndFile(ResourcesInfo info,params ResourceInfo[] assetsInfo)
        {
            if (assetsInfo == null || assetsInfo.Length <= 0)
            {
                //取得有文件后缀的路径
                foreach (string path in AssetDatabase.GetAllAssetPaths().Where(p => File.Exists(p)))
                {
                    string[] strs = path.Split('/');
                    if (strs.Any(s => s == "Assets"))
                    {
                        //取得Resources下面的文件，注意：过滤Editor下的Resources，过滤脚本
                        if (strs.Any(s => s == "Resources") && strs.All(s => s != "Editor") && !strs.Last().EndsWith(".cs"))
                        {
                            info.resourceList.Add(new ResourceInfo
                            {
                                type = ResourceType.Resources,
                                path = Regex.Match(path, @"/Resources/{1}\w+").ToString().removeHead("/Resources/"),
                                version = info.version
                            });
                            //Debug.Log($"{Regex.Match(path, @"/Resources/{1}\w+").ToString().removeHead("/Resources/")}");
                        }
                        //取得StreamingAssets下面的文件，注意:过滤xxx.mainfest
                        else if (strs.Any(s => s == "StreamingAssets") && !strs.Last().EndsWith(".manifest"))
                        {
                            if (path.Contains(info.bundleOutputPath)) {
                                var tempInfo = info.resourceList.Find(r => Path.GetFileName(path) == r.bundleName);
                                if (tempInfo != null)
                                    info.resourceList.Add(new ResourceInfo {
                                        type = ResourceType.File,
                                        path = path,
                                        bundleName = tempInfo.bundleName,
                                        version = info.version
                                    });
                            }
                            else{
                                info.resourceList.Add(new ResourceInfo {
                                    type = ResourceType.File,
                                    path = path,
                                    version = info.version
                                });
                            }
                        }
                    }
                }
            }
            else
            {
                foreach (ResourceInfo assetInfo in assetsInfo.Where(
                    a => a.type == ResourceType.Resources || a.type == ResourceType.File))
                    info.resourceList.Add(assetInfo);
                string manifestBundleName = Path.GetFileNameWithoutExtension(info.bundleOutputPath);
                if (info.resourceList.Find(r=>r.bundleName == manifestBundleName)!=null) {
                    info.resourceList.Add(new ResourceInfo {
                        path = info.bundleOutputPath+"/"+manifestBundleName,
                        type = ResourceType.File,
                        bundleName = manifestBundleName,
                        version = info.version
                    });
                }

            }
        }

        public static bool build(ResourcesInfo info, string outputDir, params ResourceInfo[] assetsInfo)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));
            DirectoryInfo dirInfo = new DirectoryInfo(outputDir);
            if (!dirInfo.Exists)
                dirInfo.Create();
            info.bundleOutputPath = outputDir;
            AssetImporter importer;
            Dictionary<string, string> nameCacheDic = null;
            Dictionary<string, string> variantCacheDic = null;
            if (assetsInfo != null && assetsInfo.Length > 0)
            {
                //先缓存并清空所有其他AssetBundle的信息
                nameCacheDic = new Dictionary<string, string>();
                variantCacheDic = new Dictionary<string, string>();
                string[] allBundlNames = AssetDatabase.GetAllAssetBundleNames();
                foreach (var otherBundleName in allBundlNames)
                {
                    foreach (var otherAssetPath in AssetDatabase.GetAssetPathsFromAssetBundle(otherBundleName))
                    {
                        if (assetsInfo.Any(a => a.path == otherAssetPath))//是目标之一，跳过
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
            foreach (var assetInfo in assetsInfo.Where(a => a.type == ResourceType.Assetbundle))
            {
                importer = AssetImporter.GetAtPath(assetInfo.path);
                if (assetInfo.bundleName != null)
                    importer.assetBundleName = assetInfo.bundleName;
                importer.SaveAndReimport();
            }
            try
            {
                AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(outputDir,
                BuildAssetBundleOptions.StrictMode | BuildAssetBundleOptions.ChunkBasedCompression,
                EditorUserBuildSettings.activeBuildTarget);
                //打包完毕，处理AssetBundleInfo，首先处理生成的Manifest
                if (manifest != null)
                {
                    info.resourceList.Add(new ResourceInfo {
                        path = "assetbundlemanifest",
                        type = ResourceType.Assetbundle,
                        bundleName = dirInfo.Name,
                        version = info.version
                    });
                    //处理所有的AB包
                    foreach (var bundleName in manifest.GetAllAssetBundles())
                    {
                        string bundlePath = Path.Combine(outputDir, bundleName);
                        AssetBundle bundle = AssetBundle.LoadFromFile(bundlePath);
                        foreach (var path in bundle.GetAllAssetNames())
                        {
                            ResourceInfo assetInfo = info.resourceList.Find(a => a.path == path);
                            if (assetInfo == null)
                            {
                                if (assetsInfo.Length > 0)
                                    assetInfo = assetsInfo.FirstOrDefault(a => a.path == path);
                                if (assetInfo == null)
                                {
                                    assetInfo = new ResourceInfo
                                    {
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
                BuildInfoOfResourcesAndFile(info,assetsInfo);
                return true;
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