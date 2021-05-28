namespace BJSYGameCore
{
    public interface IController<TMainCtrl> where TMainCtrl : IController<TMainCtrl>
    {
        IAppManager app { get; }
        TMainCtrl main { get; }
    }
}
