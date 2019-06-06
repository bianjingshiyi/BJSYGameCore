using System;

namespace BJSYGameCore.StateMachines
{
    public interface IStateMachine
    {
        IState getDefaultState();
        IState state { get; }
        event Action<IStateMachine, IState, IState> onStateChange;
        IState[] getAllStates();
        TState getState<TState>() where TState : IState;
        void setNextStateField(IState state);
    }
}