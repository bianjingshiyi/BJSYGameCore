using System.IO;
using UnityEngine;
using UnityEditor;
using UObject = UnityEngine.Object;
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
                //if (Directory.Exists(path))
                //    buildAssetBundle(path);
            }
        }
        public static AssetBundleManifest buildAssetBundle(UObject obj, string bundleName, string bundleVariant, string outputDir)
        {
            AssetImporter importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(obj));
            importer.assetBundleName = bundleName;
            importer.assetBundleVariant = bundleVariant;
            importer.SaveAndReimport();
            AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(outputDir,
                BuildAssetBundleOptions.StrictMode | BuildAssetBundleOptions.ChunkBasedCompression,
                EditorUserBuildSettings.activeBuildTarget);
            importer.assetBundleName = null;
            importer.SaveAndReimport();
            return manifest;
        }
        public static void buildAssetBundle(AssetBundleInfo info)
        {
            foreach (var bundleName in AssetDatabase.GetAllAssetBundleNames())
            {
                if (bundleName == AssetDatabase.GetAssetPath(info))
                    continue;//跳过自己
                AssetBundleInfoItem item = info.bundleList.Find(i => i.name == bundleName);
            }
        }
    }
}