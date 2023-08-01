using System.Collections.Generic;
using System.Resources;
using NUnit.Framework;
using UnityEngine;
using BJSYGameCore;
using ResourceManager = BJSYGameCore.ResourceManager;
using UnityEngine.AI;
namespace BJSYGameCore.Tests
{
    public class ResourceManagerTests
    {
        const string PATH_TEST_RESOURCE = "res:TestResource";
#if !ADDRESSABLE_ASSETS
        /// <summary>
        /// 从Resources中加载资源
        /// </summary>
        [Test]
        public void loadTest_Resources()
        {
            using (ResourceManager manager = createManager())
            {
                var res = manager.load<GameObject>(PATH_TEST_RESOURCE);
                Assert.NotNull(res);
            }
        }
        /// <summary>
        /// 从缓存中加载资源
        /// </summary>
        [Test]
        public void loadTest_Cache()
        {
            using (ResourceManager manager = createManager())
            {
                manager.loadFromCache<GameObject>(PATH_TEST_RESOURCE, out var res);
                Assert.Null(res);
                res = manager.load<GameObject>(PATH_TEST_RESOURCE);
                Assert.NotNull(res);
                manager.loadFromCache(PATH_TEST_RESOURCE, out res);
                Assert.NotNull(res);
            }
        }
        /// <summary>
        /// 从AssetBundle中加载资源，格式为“ab:AssetBundle名/相对资源路径”
        /// </summary>
        [Test]
        public void loadTest_AssetBundle()
        {
            using (ResourceManager manager = createManager())
            {
            }
        }
        public static ResourceManager createManager()
        {
            return new GameObject(nameof(ResourceManager)).AddComponent<ResourceManager>();
        }
#endif
    }
}
