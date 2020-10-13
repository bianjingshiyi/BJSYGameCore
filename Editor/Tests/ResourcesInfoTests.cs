using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using BJSYGameCore;
using System.IO;
using UnityEditor;
using BJSYGameCore.Tests;

namespace Tests
{
    public class ResourcesInfoTests
    {
        const string PATH_RESOURCE_TO_LOAD = "ResourceToLoad";
        const string PATH_RESOURCE_NOT_TO_LOAD = "ResourceNotToLoad";
        const string PATH_BUILD_OUTPUT = "Tests/AssetBundles";
        const string PATH_TEST_MATB = "Materials/MatB";
        const string TEST_BUNDLE_NAME = "Tests";
        const string TEST_BUNDLE_VARIANT = "Variant";
        /// <summary>
        /// 打包所有类型的资源并生成它们的信息。
        /// </summary>
        [Test]
        public void buildAndCheckInfoTest()
        {
            using (ResourcesInfo info = ScriptableObject.CreateInstance<ResourcesInfo>())
            {
                ResourcesInfoEditor.build(info, PATH_BUILD_OUTPUT);
                var resourceToLoad = info.resourceList.Find(r => r.path == PATH_RESOURCE_TO_LOAD);
                Assert.NotNull(resourceToLoad);
                var resourceNotToLoad = info.resourceList.Find(r => r.path == PATH_RESOURCE_NOT_TO_LOAD);
                Assert.Null(resourceNotToLoad);
            }
        }
        /// <summary>
        /// 只打指定目标的AssetBundle。
        /// </summary>
        [Test]
        public void buildSelectedTest()
        {
            Object asset = Resources.Load(PATH_TEST_MATB);
            using (ResourcesInfo info = ScriptableObject.CreateInstance<ResourcesInfo>())
            {
                ResourcesInfoEditor.build(info, PATH_BUILD_OUTPUT, new AssetBundleInfoItem(TEST_BUNDLE_NAME, TEST_BUNDLE_VARIANT,
                    new ResourceInfo(AssetDatabase.GetAssetPath(asset))));
                Assert.AreEqual(1, info.bundleList.Count);
                Assert.AreEqual(TEST_BUNDLE_NAME.ToLower() + "." + TEST_BUNDLE_VARIANT.ToLower(), info.bundleList[0].name);

                AssetBundle bundle = AssetBundle.LoadFromFile(PATH_BUILD_OUTPUT + "/" + info.bundleList[0].name);
                Assert.NotNull(bundle);
                Assert.AreEqual(1, bundle.GetAllAssetNames().Length);
                Object loadedAsset = bundle.LoadAsset(AssetDatabase.GetAssetPath(asset).ToLower());
                Assert.AreEqual(asset.name, loadedAsset.name);

                bundle = AssetBundle.LoadFromFile(PATH_BUILD_OUTPUT + "/" + new DirectoryInfo(PATH_BUILD_OUTPUT).Name);
                Assert.NotNull(bundle);
                foreach (var assetName in bundle.GetAllAssetNames())
                {
                    AssetBundleManifest manifest = bundle.LoadAsset<AssetBundleManifest>(assetName);
                    foreach (var assetNameInManifest in manifest.GetAllAssetBundles())
                    {
                        Assert.AreEqual((TEST_BUNDLE_NAME + "." + TEST_BUNDLE_VARIANT).ToLower(), assetNameInManifest);
                    }
                }
            }
        }
        /// <summary>
        /// build可以在打包的时候指定资源打包进的Bundle和路径，
        /// 资源加载可以通过"ab:包名/指定路径"来进行加载。
        /// </summary>
        [Test]
        public void buildAndLoadTest()
        {
            using (ResourceManager manager = ResourceManagerTests.createManager())
            {
                Object asset = Resources.Load(PATH_TEST_MATB);
                using (ResourcesInfo info = ScriptableObject.CreateInstance<ResourcesInfo>())
                {
                    ResourcesInfoEditor.build(info, PATH_BUILD_OUTPUT, new AssetBundleInfoItem(TEST_BUNDLE_NAME, TEST_BUNDLE_VARIANT,
                        new ResourceInfo(PATH_TEST_MATB, AssetDatabase.GetAssetPath(asset))));

                    var loadedAsset = manager.loadFromAssetBundle(info, TEST_BUNDLE_NAME + "." + TEST_BUNDLE_VARIANT + "/" + PATH_TEST_MATB);
                    Assert.NotNull(loadedAsset);
                    Assert.AreEqual(asset.name, loadedAsset.name);
                }
            }
        }
    }
}
