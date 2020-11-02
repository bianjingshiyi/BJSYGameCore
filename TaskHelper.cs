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
    }
}