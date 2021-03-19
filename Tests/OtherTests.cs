using NUnit.Framework;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using UnityEngine.TestTools;
using UnityEngine;
using System.Threading;

namespace BJSYGameCore.Tests
{
    public class OtherTests
    {
        [Test]
        public void getDirTest()
        {
            string path = "A/B/C";
            Assert.AreEqual("C", new DirectoryInfo(path).Name);
        }
        [UnityTest]
        public IEnumerator awaitTest()
        {
            Debug.Log(Time.time + "主线程：" + Thread.CurrentThread.ManagedThreadId);
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            bool flag = false;
            Task.Run(asyncMethod).continueOnMainThread();
            Assert.False(flag);
            yield return new WaitForSeconds(.1f);
            tcs.SetResult(null);
            Debug.Log(Time.time + "主线程SetResult：" + Thread.CurrentThread.ManagedThreadId);
            Assert.True(flag);
            //async void syncMethod(Task task)
            //{
            //    await task;
            //    Debug.Log(Time.time + "异步线程：" + Thread.CurrentThread.ManagedThreadId);
            //}
            async Task asyncMethod()
            {
                await tcs.Task;
                flag = true;
            }
        }
    }
}
