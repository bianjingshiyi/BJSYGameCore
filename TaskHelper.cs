using System.Threading.Tasks;
using UnityEngine;

namespace BJSYGameCore
{
    public static class TaskHelper
    {
        public static WaitUntil wait(this Task task)
        {
            return new WaitUntil(() => task.IsCompleted || task.IsCanceled || task.IsFaulted);
        }
        /// <summary>
        /// 在async方法中对task进行await，以便其可以被UnitySyncCtx捕获，在主线程上继续。
        /// </summary>
        /// <param name="task">要捕获的Task</param>
        public static async void continueOnMainThread(this Task task)
        {
            await task;
        }
    }
}