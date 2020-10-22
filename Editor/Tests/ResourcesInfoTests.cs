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

namespace Tests
{
    public class ResourcesInfoTests
    {
        const string PATH_RESOURCE_TO_LOAD = "ResourceToLoad";
        const string PATH_RESOURCE_NOT_TO_LOAD = "ResourceNotToLoad";
        const string PATH_ASSET_TO_PACK = "Assets/Plugins/BJSYGameCore/Tests/AssetToPack.prefab";
        const string PATH_FILE_TO_READ = "Assets/StreamingAssets/FileToRead.txt";
        const string PATH_BUILD_OUTPUT = "Tests/AssetBundles";
        /// <summary>
        /// 打包所有类型的资源并生成它们的信息。
        /// 包括一个“需要加载的资源”，类型是Resource，path是Resources文件夹下相对路径。
        /// 不包括“不需要加载的资源”，因为它位于Editor文件夹下。
        /// </summary>
        [Test]
        public void buildAndCheckInfoTest()
        {
            using (ResourcesInfo info = ScriptableObject.CreateInstance<ResourcesInfo>())
            {
                usingTempFile(() =>
                {
                    ResourcesInfoEditor.build(info, PATH_BUILD_OUTPUT);
                    var resourceToLoad = info.resourceList.Find(r => r.type == ResourceType.resource && r.path == PATH_RESOURCE_TO_LOAD);
                    Assert.NotNull(resourceToLoad);
                    var resourceNotToLoad = info.resourceList.Find(r => r.path == PATH_RESOURCE_NOT_TO_LOAD);
                    Assert.Null(resourceNotToLoad);
                    var assetToPack = info.resourceList.Find(r => r.type == ResourceType.assetbundle && r.path == PATH_ASSET_TO_PACK);
                    Assert.NotNull(assetToPack);
                    var fileToRead = info.resourceList.Find(r => r.type == ResourceType.file && r.path == PATH_FILE_TO_READ);
                    Assert.NotNull(fileToRead);
                });
            }
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
                File.Create(PATH_FILE_TO_READ);
            }
            action?.Invoke();
            if (isFileTemp)
                File.Delete(PATH_FILE_TO_READ);
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
                using (ResourcesInfo info = ScriptableObject.CreateInstance<ResourcesInfo>())
                {
                    usingTempFile(() =>
                    {
                        ResourcesInfoEditor.build(info, PATH_BUILD_OUTPUT, new ResourceInfo()
                        {
                            type = ResourceType.resource,
                            path = PATH_RESOURCE_TO_LOAD
                        },
                        new ResourceInfo()
                        {
                            type = ResourceType.assetbundle,
                            path = PATH_ASSET_TO_PACK
                        },
                        new ResourceInfo()
                        {
                            type = ResourceType.file,
                            path = PATH_FILE_TO_READ
                        });
                        //Resources
                        Object asset = Resources.Load(PATH_RESOURCE_TO_LOAD);
                        Object resource = manager.loadFromResources(info.getInfoByPath("res:" + PATH_RESOURCE_TO_LOAD));
                        Assert.AreEqual(asset, resource);
                        //AssetBundle
                        asset = AssetDatabase.LoadAssetAtPath<GameObject>(PATH_ASSET_TO_PACK);
                        resource = manager.loadFromAssetBundle(info.getInfoByPath("ab:" + PATH_ASSET_TO_PACK));
                        Assert.IsInstanceOf<GameObject>(resource);
                        Assert.AreEqual(asset.name, resource.name);
                        //File
                        Assert.True(File.Exists(PATH_FILE_TO_READ));
                        //文件一般都是直接通过System.IO或者WebRequest来读取文件流，所以这里不做读取测试。
                        //其实是我懒得写。
                    });
                }
            }
        }
    }
}