using System.Collections.Generic;
using System.Resources;
using NUnit.Framework;
using UnityEngine;
using BJSYGameCore;
using ResourceManager = BJSYGameCore.ResourceManager;
using UnityEngine.AI;
using UnityEngine.TestTools;
using System.Collections;
using System.Threading.Tasks;
using System.IO;

namespace BJSYGameCore.Tests
{
    public class ResourceManagerTests
    {
        const string PATH_TEST_RESOURCE = "ResourceToLoad";
        /// <summary>
        /// 从Resources中加载资源
        /// </summary>
        [Test]
        public void loadTest_Resources()
        {
            ResourceManager manager = createManager();
            var res = manager.loadFromResources<GameObject>(PATH_TEST_RESOURCE);
            Assert.NotNull(res);
        }
        [UnityTest]
        public IEnumerator loadTest_ResoucesAsync()
        {
            ResourceManager manager = createManager();
            Task<GameObject> task = manager.loadFromResourcesAsync<GameObject>(PATH_TEST_RESOURCE);
            yield return task.wait();
            GameObject res = task.Result;
            Assert.NotNull(res);
        }
        /// <summary>
        /// 从缓存中加载资源
        /// </summary>
        [Test]
        public void loadTest_Cache()
        {
            ResourceManager manager = createManager();
            manager.loadFromCache(PATH_TEST_RESOURCE, out GameObject res);
            Assert.Null(res);
            res = manager.loadFromResources<GameObject>(PATH_TEST_RESOURCE);
            Assert.NotNull(res);
            manager.loadFromCache(PATH_TEST_RESOURCE, out res);
            Assert.NotNull(res);
        }
        /// <summary>
        /// 从AssetBundle中加载资源，地址与AssetDatabase中地址一致
        /// </summary>
        [UnityTest]
        public IEnumerator loadTest_AssetBundle()
        {
            ResourceManager manager = createManager();
            manager.resourcesInfo = ScriptableObject.CreateInstance<ResourcesInfo>();
            manager.resourcesInfo.bundleOutputPath = "AssetBundles";
            //manager.resourcesInfo.resourceList.Add(new ResourceInfo()
            //{
            //    bundleName = "test.variant",
            //    path = "Assets/Plugins/BJSYGameCore/Tests/AssetToPack.prefab"
            //});
            Task<GameObject> task = manager.loadFromAssetBundleAsync<GameObject>("Assets/Plugins/BJSYGameCore/Tests/AssetToPack.prefab");
            yield return task.wait();
            GameObject prefab = task.Result;
            Assert.NotNull(prefab);
        }
        public static ResourceManager createManager()
        {
            if (gameManager == null)
            {
                gameManager = new GameManager();
            }
            return gameManager.resourceManager;
        }
        static GameManager gameManager;
    }
}
