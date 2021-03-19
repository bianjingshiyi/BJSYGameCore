using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;

namespace BJSYGameCore
{
    /// <summary>
    /// Task管理系统，主要的用途是控制Task完成的线程同步。
    /// </summary>
    public class TaskManager
    {
        /// <summary>
        /// 注册一个事件
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public Task regTask(Task task, CancellationToken? cancelToken = null)
        {
            TaskRegistration reg = new TaskRegistration(task, cancelToken);
            _taskRegDict.Add(task, reg);
            return reg.task;
        }
        public Task<T> regTask<T>(Task<T> task, CancellationToken? cancelToken = null)
        {
            TaskRegistration<T> reg = new TaskRegistration<T>(task, cancelToken);
            _taskRegDict.Add(task, reg);
            return reg.task;
        }
        public void onUpdate()
        {
            List<Task> completedTaskList = new List<Task>();
            foreach (var pair in _taskRegDict)
            {
                if (pair.Value.cancelToken != null && pair.Value.cancelToken.Value.IsCancellationRequested)
                {
                    completedTaskList.Add(pair.Key);

                }
                if (pair.Value.isCompleted())
                {
                    completedTaskList.Add(pair.Key);
                    pair.Value.setResult();
                }
            }
            foreach (var task in completedTaskList)
            {
                _taskRegDict.Remove(task);
            }
        }
        readonly Dictionary<Task, TaskRegistrationBase> _taskRegDict = new Dictionary<Task, TaskRegistrationBase>();
        abstract class TaskRegistrationBase
        {
            public TaskRegistrationBase(CancellationToken? cancelToken)
            {
                this.cancelToken = cancelToken;
            }
            public CancellationToken? cancelToken { get; }
            public abstract bool isCompleted();
            public abstract void setResult();
            public abstract void setCanceled();
        }
        class TaskRegistration : TaskRegistrationBase
        {
            public TaskRegistration(Task task, CancellationToken? cancelToken) : base(cancelToken)
            {
                _originTask = task;
                _tcs = new TaskCompletionSource<object>();
            }
            public override bool isCompleted()
            {
                return _originTask.IsCompleted;
            }
            public override void setResult()
            {
                _tcs.SetResult(null);
            }
            public override void setCanceled()
            {
                _tcs.SetCanceled();
            }
            public Task task => _tcs.Task;
            Task _originTask;
            TaskCompletionSource<object> _tcs;
        }
        class TaskRegistration<T> : TaskRegistrationBase
        {
            public TaskRegistration(Task<T> task, CancellationToken? cancelToken) : base(cancelToken)
            {
                _originTask = task;
            }
            public override bool isCompleted()
            {
                return _originTask.IsCompleted;
            }
            public override void setResult()
            {
                _tcs.SetResult(_originTask.Result);
            }
            public override void setCanceled()
            {
                _tcs.SetCanceled();
            }
            public Task<T> task => _tcs.Task;
            Task<T> _originTask;
            TaskCompletionSource<T> _tcs;
        }
    }
}
