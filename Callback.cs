namespace BJSYGameCore
{
    public class Callback
    {
        public delegate void Delegate();
        public delegate void Delegate1(object[] vars);
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
        public Callback()
        {
        }
        public Callback(Delegate callback)
        {
            this.callback = callback;
        }
        public Callback(Delegate1 callback, params object[] vars)
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
        public void call()
        {
            callback?.Invoke();
            callback1?.Invoke(vars);
        }
    }
    public class Callback<T>
    {
        public delegate void Delegate(T arg);
        public delegate void Delegate1(T arg, object[] vars);
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
        public Callback()
        {
        }
        public Callback(Delegate callback)
        {
            this.callback = callback;
        }
        public Callback(Delegate1 callback, params object[] vars)
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
        public void call(T arg)
        {
            callback?.Invoke(arg);
            callback1?.Invoke(arg, vars);
        }
    }
    public class Callback<T1, T2>
    {
        public delegate void Delegate(T1 arg1, T2 arg2);
        public delegate void Delegate1(T1 arg1, T2 arg2, object[] vars);
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
        public Callback()
        {
        }
        public Callback(Delegate callback)
        {
            this.callback = callback;
        }
        public Callback(Delegate1 callback, params object[] vars)
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
        public void call(T1 arg1, T2 arg2)
        {
            callback?.Invoke(arg1, arg2);
            callback1?.Invoke(arg1, arg2, vars);
        }
    }
}