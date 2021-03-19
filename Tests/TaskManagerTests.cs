using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace BJSYGameCore.Tests
{
    public class TaskManagerTests
    {
        [UnityTest]
        public IEnumerator regTaskTest()
        {
            TaskManager taskManager = new TaskManager();
            mainThread = Thread.CurrentThread.ManagedThreadId;
            Debug.Log("主线程开始：" + Thread.CurrentThread.ManagedThreadId);
            threadA = 0;
            threadB = 0;
            Task.Run(taskA);
            Task.Run(async () => await taskB(taskManager));
            yield return new WaitForSeconds(3.1f);
            Debug.Log("主线程等待后：" + Thread.CurrentThread.ManagedThreadId);
            taskManager.onUpdate();
            Debug.Log("主线程Update后：" + Thread.CurrentThread.ManagedThreadId);
            Assert.AreEqual(mainThread, Thread.CurrentThread.ManagedThreadId);
            Assert.AreNotEqual(mainThread, threadA);
            Assert.AreEqual(mainThread, threadB);
        }
        async Task taskA()
        {
            Debug.Log("TaskA线程等待前：" + Thread.CurrentThread.ManagedThreadId);
            Assert.AreEqual(mainThread, Thread.CurrentThread.ManagedThreadId);
            await Task.Run(async () => await Task.Delay(3000));
            Debug.Log("TaskA线程等待后：" + Thread.CurrentThread.ManagedThreadId);
            threadA = Thread.CurrentThread.ManagedThreadId;
        }
        async Task taskB(TaskManager taskManager)
        {
            Debug.Log("TaskB线程等待前：" + Thread.CurrentThread.ManagedThreadId);
            Assert.AreEqual(mainThread, Thread.CurrentThread.ManagedThreadId);
            await taskManager.regTask(Task.Run(async () => await Task.Delay(3000)));
            Debug.Log("TaskB线程等待后：" + Thread.CurrentThread.ManagedThreadId);
            threadB = Thread.CurrentThread.ManagedThreadId;
        }
        int mainThread;
        int threadA;
        int threadB;
    }
}