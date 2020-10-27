﻿using NUnit.Framework;
using UnityEngine;
using System.IO;
using UnityEditor;
using System.Linq;

namespace Tests
{
    public class OtherEditorTests
    {
        const string PATH_BUILD_OUTPUT = "Tests/AssetBundles";
        const string PATH_ASSET_TO_PACK = "Assets/Plugins/BJSYGameCore/Tests/AssetToPack.prefab";
        [Test]
        public void buildTest()
        {
            BuildPipeline.BuildAssetBundles(PATH_BUILD_OUTPUT, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);
            AssetImporter importer = AssetImporter.GetAtPath(PATH_ASSET_TO_PACK);
            string path = PATH_BUILD_OUTPUT + "/" + importer.assetBundleName + "." + importer.assetBundleVariant;
            Assert.True(File.Exists(path));
            AssetBundle bundle = AssetBundle.LoadFromFile(path);
            Assert.AreEqual(importer.assetBundleName + "." + importer.assetBundleVariant, bundle.name);
            Assert.True(bundle.GetAllAssetNames().Contains(PATH_ASSET_TO_PACK.ToLower()));
        }
        [Test]
        public void manifestTest()
        {
            AssetImporter importer = AssetImporter.GetAtPath(PATH_ASSET_TO_PACK);
            string bundleName = importer.assetBundleName;
            string bundleVariant = importer.assetBundleVariant;
            BuildPipeline.BuildAssetBundles(PATH_BUILD_OUTPUT, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);
            string manifestPath = PATH_BUILD_OUTPUT + "/" + Path.GetFileNameWithoutExtension(PATH_BUILD_OUTPUT);
            Assert.True(File.Exists(manifestPath));
            AssetBundle bundle = AssetBundle.LoadFromFile(manifestPath);
            Debug.Log(bundle.name);
            Assert.NotNull(bundle);
            string manifestName = bundle.GetAllAssetNames()[0];
            Debug.Log(manifestName);
            AssetBundleManifest manifest = bundle.LoadAsset<AssetBundleManifest>(manifestName);
            Assert.NotNull(manifest);
            string bundlePath = PATH_BUILD_OUTPUT + "/" + bundleName + "." + bundleVariant;
            Assert.True(File.Exists(bundlePath));
            bundle = AssetBundle.LoadFromFile(bundlePath);
            Assert.NotNull(bundle);
            Assert.True(bundle.GetAllAssetNames().Contains(PATH_ASSET_TO_PACK.ToLower()));
            Object asset = bundle.LoadAsset(PATH_ASSET_TO_PACK.ToLower());
            Assert.AreEqual(Path.GetFileNameWithoutExtension(PATH_ASSET_TO_PACK), asset.name);
        }
    }
}