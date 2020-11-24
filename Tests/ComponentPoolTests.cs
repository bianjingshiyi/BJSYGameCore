using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace BJSYGameCore.Tests
{
    public class ComponentPoolTests
    {
        [UnityTest]
        public IEnumerator addAndGetTest()
        {
            Transform root = new GameObject("Root").transform;
            Transform origin = new GameObject("Origin").transform;
            ComponentPool<Transform> list = new ComponentPool<Transform>(root, origin);
            list.add(Object.Instantiate(origin));
            Transform nullObj = Object.Instantiate(origin);
            list.add(nullObj);
            Object.Destroy(nullObj.gameObject);
            yield return new WaitForEndOfFrame();
            Assert.AreEqual(1, list.count);
            Assert.AreEqual(1, root.transform.childCount);
        }
        [UnityTest]
        public IEnumerator removeTest()
        {
            Transform root = new GameObject("Root").transform;
            Transform origin = new GameObject("Origin").transform;
            ComponentPool<Transform> list = new ComponentPool<Transform>(root, origin);
            Transform component = Object.Instantiate(origin);
            list.add(component);
            Assert.True(list.remove(component));
            yield return new WaitForEndOfFrame();
        }
        [UnityTest]
        public IEnumerator createTest()
        {
            Transform root = new GameObject("Root").transform;
            Transform origin = new GameObject("Origin").transform;
            ComponentPool<Transform> list = new ComponentPool<Transform>(root, origin);
            Transform clone = list.create();
            Assert.NotNull(clone);
            yield return new WaitForEndOfFrame();
            Assert.False(origin == clone);
            list.remove(clone);
            Assert.AreEqual(0, list.count);
            Transform pooled = list.create();
            Assert.AreEqual(clone, pooled);
        }
        [Test]
        public void setCountTest()
        {
            Transform root = new GameObject("Root").transform;
            Transform origin = new GameObject("Origin").transform;
            ComponentPool<Transform> list = new ComponentPool<Transform>(root, origin);
            list.setCount(20);
            Assert.AreEqual(20, root.childCount);
        }
    }
}