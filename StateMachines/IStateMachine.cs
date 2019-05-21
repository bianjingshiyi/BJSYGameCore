using System;

namespace BJSYGameCore.StateMachines
{
    public interface IStateMachine
    {
        IState getDefaultState();
        IState state { get; }
        event Action<IStateMachine, IState> onStateChange;
        IState[] getAllStates();
        TState getState<TState>() where TState : IState;
        void setNextState(IState state);
    }
}