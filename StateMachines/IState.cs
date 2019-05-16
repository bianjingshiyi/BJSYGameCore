namespace BJSYGameCore.StateMachines
{
    public interface IState
    {
        bool isExiting { get; }
        void onEntry();
        void onUpdate();
        void onExit();
    }
}