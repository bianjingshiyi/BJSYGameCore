namespace BJSYGameCore
{
    public interface IController<TMainCtrl> where TMainCtrl : IController<TMainCtrl>
    {
        IGameManager app { get; }
        TMainCtrl main { get; }
    }
}
