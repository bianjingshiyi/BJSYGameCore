namespace BJSYGameCore
{
    public class Callback
    {
        public delegate void Delegate(object[] args);
        object[] args { get; }
        Delegate callback { get; }
        public Callback(object[] args, Delegate callback)
        {
            this.args = args;
            this.callback = callback;
        }
        public void call()
        {
            callback?.Invoke(args);
        }
    }
}