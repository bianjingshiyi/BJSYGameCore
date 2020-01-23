using System.Threading.Tasks;
namespace BJSYGameCore
{
    public class AsyncCallback<T>
    {
        public delegate Task Delegate(T arg);
        public delegate Task Delegate1(T arg, object[] vars);
        object[] vars { get; set; } = new object[0];
        Delegate _callback = null;
        Delegate callback
        {
            get { return _callback; }
            set
            {
                _callback = value;
                _callback1 = null;
            }
        }
        Delegate1 _callback1 = null;
        Delegate1 callback1
        {
            get { return _callback1; }
            set
            {
                _callback1 = value;
                _callback = null;
            }
        }
        public AsyncCallback()
        {
        }
        public AsyncCallback(Delegate callback)
        {
            this.callback = callback;
        }
        public AsyncCallback(Delegate1 callback, params object[] vars)
        {
            callback1 = callback;
            this.vars = vars;
        }
        public void setCallback(Delegate callback)
        {
            this.callback = callback;
        }
        public void setCallback(Delegate1 callback, params object[] vars)
        {
            this.vars = vars;
            callback1 = callback;
        }
        public Task call(T arg)
        {
            if (callback != null)
                return callback.Invoke(arg);
            if (callback1 != null)
                return callback1.Invoke(arg, vars);
            return Task.CompletedTask;
        }
    }
    public class AsyncCallback<T1, T2>
    {
        public delegate Task Delegate(T1 arg1, T2 arg2);
        public delegate Task Delegate1(T1 arg1, T2 arg2, object[] vars);
        object[] vars { get; set; } = new object[0];
        Delegate _callback = null;
        Delegate callback
        {
            get { return _callback; }
            set
            {
                _callback = value;
                _callback1 = null;
            }
        }
        Delegate1 _callback1 = null;
        Delegate1 callback1
        {
            get { return _callback1; }
            set
            {
                _callback1 = value;
                _callback = null;
            }
        }
        public AsyncCallback()
        {
        }
        public AsyncCallback(Delegate callback)
        {
            this.callback = callback;
        }
        public AsyncCallback(Delegate1 callback, params object[] vars)
        {
            callback1 = callback;
            this.vars = vars;
        }
        public void setCallback(Delegate callback)
        {
            this.callback = callback;
        }
        public void setCallback(Delegate1 callback, params object[] vars)
        {
            this.vars = vars;
            callback1 = callback;
        }
        public Task call(T1 arg1, T2 arg2)
        {
            if (callback != null)
                return callback.Invoke(arg1, arg2);
            if (callback1 != null)
                return callback1.Invoke(arg1, arg2, vars);
            return Task.CompletedTask;
        }
    }
}