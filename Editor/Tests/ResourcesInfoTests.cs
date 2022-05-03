using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using BJSYGameCore;
using System.IO;
using UnityEditor;
using BJSYGameCore.Tests;
using System;
using Object = UnityEngine.Object;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine.Rendering;
using System.Threading.Tasks;

namespace Tests
{
    public class ResourcesInfoTests
    {
        const string PATH_RESOURCE_TO_LOAD = "ResourceToLoad";
        const string PATH_RESOURCE_NOT_TO_LOAD = "ResourceNotToLoad";
        const string PATH_ASSET_TO_PACK = "Assets/Plugins/BJSYGameCore/Tests/AssetToPack.prefab";
        const string PATH_DEPENDENT_ASSET = "Assets/Plugins/BJSYGameCore/Tests/DependentAsset.prefab";
        const string PATH_FILE_TO_READ = "Assets/StreamingAssets/FileToRead.txt";
        const string PATH_BUILD_OUTPUT = "Assets/StreamingAssets/AssetBundles";
        const string PATH_RESOURCESINFO = "Assets/Plugins/BJSYGameCore/Tests/ResourcesInfo.asset";
        const string BUNDLENAME_RESOURCESINFO = "resourcesinfo";
        const string ASSETNAME_MANIFEST = "assetbundlemanifest";
        /// <summary>
        /// 打包所有类型的资源并生成它们的信息。
        /// 包括一个“需要加载的资源”，类型是Resource，path是Resources文件夹下相对路径。
        /// 不包括“不需要加载的资源”，因为它位于Editor文件夹下。
        /// 不包括MonoScript。
        /// 包括所有设置了AssetBundle信息的Asset。
        /// 包括StreamingAssets下的所有文件信息。
        /// </summary>
        [Test]
        public void buildAndCheckInfoTest()
        {
            using (ResourcesInfo info = ScriptableObject.CreateInstance<ResourcesInfo>())
            {
                usingTempFile(() =>
                {
                    ResourcesInfoEditor.build(info, PATH_BUILD_OUTPUT);

                    Assert.AreEqual(PATH_BUILD_OUTPUT, info.bundleOutputPath);

                    var resourceToLoad = info.resourceList.Find(r => r.type == ResourceType.Resources && r.path == PATH_RESOURCE_TO_LOAD);
                    Assert.NotNull(resourceToLoad);

                    var resourceNotToLoad = info.resourceList.Find(r => r.path == PATH_RESOURCE_NOT_TO_LOAD);
                    Assert.Null(resourceNotToLoad);

                    Assert.False(info.resourceList.Exists(r => r.type == ResourceType.Resources && Path.GetFileNameWithoutExtension(r.path) == nameof(ResourceManager)));

                    var fileToRead = info.resourceList.Find(r => r.type == ResourceType.File && r.path == PATH_FILE_TO_READ);
                    Assert.NotNull(fileToRead);

                    var assetToPack = info.resourceList.Find(r => r.type == ResourceType.Assetbundle && r.path == PATH_ASSET_TO_PACK.ToLower());
                    Assert.NotNull(assetToPack);
                });
            }
        }
        /// <summary>
        /// build可以在打包的时候指定资源打包进的Bundle和路径，
        /// 资源加载可以通过"ab:包名/指定路径"来进行加载。
        /// </summary>
        [Test]
        public void buildSelectedAndLoadTest()
        {
            ResourceManager manager = ResourceManagerTests.createManager();
            using (ResourcesInfo info = ScriptableObject.CreateInstance<ResourcesInfo>())
            {
                usingTempFile(() =>
                {
                    ResourcesInfoEditor.build(info, PATH_BUILD_OUTPUT, new ResourceInfo()
                    {
                        type = ResourceType.Resources,
                        path = PATH_RESOURCE_TO_LOAD
                    },
                    new ResourceInfo()
                    {
                        type = ResourceType.Assetbundle,
                        path = PATH_ASSET_TO_PACK,
                    },
                    new ResourceInfo()
                    {
                        type = ResourceType.File,
                        path = PATH_FILE_TO_READ
                    });
                    manager.resourcesInfo = info;
                    loadAllKindResAssert(manager);
                });
            }
        }
        /// <summary>
        /// build如果不指定需要打包的bundle或者资源路径，就会打包所有Bundle和资源。
        /// </summary>
        [Test]
        public void buildAndLoadTest()
        {
            createManagerBuildAndAssert(loadAllKindResAssert);
        }
        private void createManagerBuildAndAssert(Action<ResourceManager> onAssert, bool reCreate = true)
        {
            if (!reCreate && manager != null)
            {
                usingTempFile(() =>
                {
                    onAssert?.Invoke(manager);
                });
                return;
            }
            manager = ResourceManagerTests.createManager();
            using (ResourcesInfo info = ScriptableObject.CreateInstance<ResourcesInfo>())
            {
                usingTempFile(() =>
                {
                    ResourcesInfoEditor.build(info, PATH_BUILD_OUTPUT);
                    manager.resourcesInfo = info;
                    onAssert?.Invoke(manager);
                });
            }
            manager = null;
        }
        ResourceManager manager;
        private static void loadAllKindResAssert(ResourceManager manager)
        {
            //Resources
            Object asset = Resources.Load(PATH_RESOURCE_TO_LOAD);
            Object resource = manager.loadFromResources("res:" + PATH_RESOURCE_TO_LOAD);
            Assert.AreEqual(asset, resource);
            //AssetBundle
            asset = AssetDatabase.LoadAssetAtPath<GameObject>(PATH_ASSET_TO_PACK);
            resource = manager.loadFromAssetBundle("ab:" + PATH_ASSET_TO_PACK);
            Assert.IsInstanceOf<GameObject>(resource);
            Assert.AreEqual(asset.name, resource.name);
            //File
            Assert.True(File.Exists(PATH_FILE_TO_READ));
            //文件一般都是直接通过System.IO或者WebRequest来读取文件流，所以这里不做读取测试。
            //其实是我懒得写。
        }
        /// <summary>
        /// 将ResourcesInfo打包到AssetBundle中再输出到StreamingAssets中，
        /// ResourcesInfo中应该存在ResourcesInfo的AssetBundle信息和File信息，
        /// 这么做是为了让ResourcesInfo可以从外部读取，为热更新做准备。
        /// </summary>
        [Test]
        public void loadResInfoTest()
        {
            createManagerBuildAndAssert(manager =>
            {
                string resInfoFilePath = PATH_BUILD_OUTPUT + "/" + BUNDLENAME_RESOURCESINFO;
                ResourceInfo resInfo = manager.resourcesInfo.getInfoByPath(resInfoFilePath);
                Assert.AreEqual(ResourceType.File, resInfo.type);
                Assert.AreEqual(resInfoFilePath, resInfo.path);
                string resInfoABPath = "ab:" + PATH_RESOURCESINFO;
                resInfo = manager.resourcesInfo.getInfoByPath(resInfoABPath);
                Assert.AreEqual(ResourceType.Assetbundle, resInfo.type);
                Assert.AreEqual(BUNDLENAME_RESOURCESINFO, resInfo.bundleName);
                Assert.AreEqual(PATH_RESOURCESINFO.ToLower(), resInfo.path);
                manager.resourcesInfo = manager.load<ResourcesInfo>(resInfo);
                Assert.NotNull(manager.resourcesInfo);
            });
        }
        /// <summary>
        /// 在BuildAssetBundles之后会生成AssetBundleManifest，
        /// ResourcesInfo中应该存在它的文件资源信息和AB资源信息，
        /// 同样是出于热更的考虑，它必须要能够从外部读取。
        /// </summary>
        [Test]
        public void loadManifestTest()
        {
            createManagerBuildAndAssert(manager =>
            {
                string manifestFilePath = PATH_BUILD_OUTPUT + "/" + Path.GetFileNameWithoutExtension(PATH_BUILD_OUTPUT);
                ResourceInfo resInfo = manager.resourcesInfo.getInfoByPath(manifestFilePath);
                Assert.AreEqual(ResourceType.File, resInfo.type);
                Assert.AreEqual(manifestFilePath, resInfo.path);

                string manifestABPath = "ab:" + ASSETNAME_MANIFEST;
                resInfo = manager.resourcesInfo.getInfoByPath(manifestABPath);
                Assert.AreEqual(ResourceType.Assetbundle, resInfo.type);
                Assert.True(!string.IsNullOrEmpty(resInfo.bundleName));
                Assert.AreEqual(ASSETNAME_MANIFEST, resInfo.path);

                AssetBundleManifest manifest = manager.load<AssetBundleManifest>(resInfo);
                Assert.NotNull(manifest);
            });
        }
        /// <summary>
        /// 加载一个资源的正确思路应该是，先加载ResourcesInfo，因为它是所有资源的元数据
        /// 然后加载AssetBundleManifest，因为它是所有AssetBundle的元数据
        /// 然后检查这个要加载的资源是不是位于AssetBundle中，如果是则去找AssetBundle对应的文件信息
        /// 再根据文件信息加载出AssetBundle，再从AssetBundle中加载出所需要的资源。
        /// </summary>
        [Test]
        public void loadAssetTest()
        {
            createManagerBuildAndAssert(manager =>
            {
                Assert.NotNull(manager.resourcesInfo);
                manager.load<AssetBundleManifest>(manager.resourcesInfo.getInfoByPath("ab:" + ASSETNAME_MANIFEST));

                ResourceInfo resInfo = manager.resourcesInfo.getInfoByPath("ab:" + PATH_ASSET_TO_PACK);
                Assert.AreEqual(ResourceType.Assetbundle, resInfo.type);
                Assert.AreEqual(PATH_ASSET_TO_PACK.ToLower(), resInfo.path);

                resInfo = manager.resourcesInfo.getAssetBundleInfoByName(resInfo.bundleName);
                AssetBundle bundle = manager.loadAssetBundle(resInfo);
                Assert.True(manager.loadAssetBundleFromCache(bundle.name, out bundle));

                Object asset = bundle.LoadAsset(PATH_ASSET_TO_PACK);
                Assert.AreEqual(Path.GetFileNameWithoutExtension(PATH_ASSET_TO_PACK), asset.name);
            });
        }
        /// <summary>
        /// 已经被加载过的资源可以直接从缓存中读取。
        /// </summary>
        [Test]
        public void loadFromCacheTest()
        {
            createManagerBuildAndAssert(manager =>
            {
                GameObject res = manager.load<GameObject>("res:" + PATH_RESOURCE_TO_LOAD);
                Assert.NotNull(res);
                Assert.True(manager.loadFromCache("res:" + PATH_RESOURCE_TO_LOAD, out GameObject cachedRes));
                Assert.AreEqual(res, cachedRes);
            }, false);
        }
        /// <summary>
        /// 已经被加载过的资源包可以直接从缓存中读取。
        /// </summary>
        [Test]
        public void loadAssetBundleFromCacheTest()
        {
            createManagerBuildAndAssert(manager =>
            {
                AssetBundle bundle = manager.loadAssetBundle(
                    manager.resourcesInfo.getAssetBundleInfoByName(
                        manager.resourcesInfo.getInfoByPath("ab:" + PATH_ASSET_TO_PACK).bundleName));
                Assert.NotNull(bundle);
                Assert.True(manager.loadAssetBundleFromCache(
                    manager.resourcesInfo.getInfoByPath("ab:" + PATH_ASSET_TO_PACK).bundleName, out AssetBundle cachedBundle));
                Assert.AreEqual(bundle, cachedBundle);
            }, false);
        }
        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator loadAllKindResAsyncTest()
        {
            yield return createManagerBuildAndAssertAsync(loadAllKindResAsyncAssert, false);
        }
        IEnumerator loadAllKindResAsyncAssert(ResourceManager manager)
        {
            //Resources
            Object asset = Resources.Load(PATH_RESOURCE_TO_LOAD);
            var objTask = manager.loadFromResourcesAsync<Object>("res:" + PATH_RESOURCE_TO_LOAD);
            yield return objTask.wait();
            Object resource = objTask.Result;
            Assert.AreEqual(asset, resource);
            //AssetBundle
            asset = AssetDatabase.LoadAssetAtPath<GameObject>(PATH_ASSET_TO_PACK);
            var goTask = manager.loadFromAssetBundleAsync<GameObject>("ab:" + PATH_ASSET_TO_PACK);
            yield return goTask.wait();
            resource = goTask.Result;
            Assert.IsInstanceOf<GameObject>(resource);
            Assert.AreEqual(asset.name, resource.name);
            //File
            Assert.True(File.Exists(PATH_FILE_TO_READ));
            //文件一般都是直接通过System.IO或者WebRequest来读取文件流，所以这里不做读取测试。
            //其实是我懒得写。
        }
        IEnumerator createManagerBuildAndAssertAsync(Func<ResourceManager, IEnumerator> onAssert, bool reCreate = false)
        {
            if (!reCreate && manager != null)
            {
                bool isFileTemp = false;
                if (!Directory.Exists(Path.GetDirectoryName(PATH_FILE_TO_READ)))
                    Directory.CreateDirectory(Path.GetDirectoryName(PATH_FILE_TO_READ));
                if (!File.Exists(PATH_FILE_TO_READ))
                {
                    isFileTemp = true;
                    File.Create(PATH_FILE_TO_READ).Close();
                }
                yield return onAssert?.Invoke(manager);
                if (isFileTemp)
                    File.Delete(PATH_FILE_TO_READ);
                yield break;
            }
            manager = ResourceManagerTests.createManager();
            using (ResourcesInfo info = ScriptableObject.CreateInstance<ResourcesInfo>())
            {
                bool isFileTemp = false;
                if (!Directory.Exists(Path.GetDirectoryName(PATH_FILE_TO_READ)))
                    Directory.CreateDirectory(Path.GetDirectoryName(PATH_FILE_TO_READ));
                if (!File.Exists(PATH_FILE_TO_READ))
                {
                    isFileTemp = true;
                    File.Create(PATH_FILE_TO_READ).Close();
                }
                ResourcesInfoEditor.build(info, PATH_BUILD_OUTPUT);
                manager.resourcesInfo = info;
                yield return onAssert?.Invoke(manager);
                if (isFileTemp)
                    File.Delete(PATH_FILE_TO_READ);
            }
            manager = null;
        }
        /// <summary>
        /// 同时异步加载多个AssetBundle应当只存在一个AssetBundle加载线程，
        /// 当这个AssetBundle加载完成之后，其他的Task也会完成。
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator loadBundleAsyncSameTimeTest()
        {
            yield return createManagerBuildAndAssertAsync(loadBundleAsyncSameTimeTest, false);
        }
        IEnumerator loadBundleAsyncSameTimeTest(ResourceManager manager)
        {
            var task1 = manager.loadAssetBundleAsync(
                manager.resourcesInfo.getAssetBundleInfoByName(
                    manager.resourcesInfo.getInfoByPath("ab:" + PATH_ASSET_TO_PACK).bundleName));
            var task2 = manager.loadAssetBundleAsync(
                manager.resourcesInfo.getAssetBundleInfoByName(
                    manager.resourcesInfo.getInfoByPath("ab:" + PATH_ASSET_TO_PACK).bundleName));
            yield return task1.wait();
            Assert.AreEqual(task1.IsCompleted, task2.IsCompleted);
        }
        /// <summary>
        /// 在加载AssetBundle的时候会先加载所有依赖包，然后再加载本体。
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator loadDenpendentBundleTest() 
        {
            yield return createManagerBuildAndAssertAsync(loadDenpendentBundleAssert, false);
        }
        IEnumerator loadDenpendentBundleAssert(ResourceManager manager)
        {
            yield return manager.loadAssetBundleAsync(
               manager.resourcesInfo.getAssetBundleInfoByName(
                   manager.resourcesInfo.getInfoByPath("ab:" + PATH_DEPENDENT_ASSET).bundleName)).wait();
            Assert.True(manager.loadAssetBundleFromCache(manager.resourcesInfo.getInfoByPath("ab:" + PATH_ASSET_TO_PACK).bundleName, out _));
            Assert.True(manager.loadAssetBundleFromCache(manager.resourcesInfo.getInfoByPath("ab:" + PATH_ASSET_TO_PACK).bundleName, out _));
        }
        void usingTempFile(Action action)
        {
            //文件
            bool isFileTemp = false;
            if (!Directory.Exists(Path.GetDirectoryName(PATH_FILE_TO_READ)))
                Directory.CreateDirectory(Path.GetDirectoryName(PATH_FILE_TO_READ));
            if (!File.Exists(PATH_FILE_TO_READ))
            {
                isFileTemp = true;
                File.Create(PATH_FILE_TO_READ).Close();
            }
            action?.Invoke();
            if (isFileTemp)
                File.Delete(PATH_FILE_TO_READ);
        }
    }
}