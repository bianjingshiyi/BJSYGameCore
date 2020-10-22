using NUnit.Framework;
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
    }
}