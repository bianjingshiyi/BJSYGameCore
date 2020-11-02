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
        const string BUNDLE_NAME_TEST_VARIANT = "test.varaint";
        const string BUNDLE_NAME_DEPENDENT = "dependent";
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
        [Test]
        public void dependentTest()
        {
            BuildPipeline.BuildAssetBundles(PATH_BUILD_OUTPUT, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);
            AssetBundle bundle = AssetBundle.LoadFromFile(PATH_BUILD_OUTPUT + "/" + Path.GetFileNameWithoutExtension(PATH_BUILD_OUTPUT));
            AssetBundleManifest manifest = bundle.LoadAsset<AssetBundleManifest>(bundle.GetAllAssetNames()[0]);
            Assert.True(manifest.GetAllDependencies(BUNDLE_NAME_DEPENDENT).Contains(BUNDLE_NAME_TEST_VARIANT));
        }
        const string PATH_AUTOUI = "Assets/Plugins/BJSYGameCore/Tests/Editor/AutoUI.RectTransform,Animator.prefab";
        [Test]
        public void getPrefabByIdTest()
        {
            Object obj = AssetDatabase.LoadAssetAtPath<GameObject>(PATH_AUTOUI);
            int id = obj.GetInstanceID();
            Debug.Log(id);
            Assert.AreEqual(obj, EditorUtility.InstanceIDToObject(id));
        }
    }
}