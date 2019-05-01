using System;

using UnityEngine;

namespace BJSYGameCore.StateMachines
{
    public abstract class BehaviourSubStateMachine : BehaviourState, IStateMachine
    {
        [SerializeField]
        BehaviourState _defaultState;
        [SerializeField]
        BehaviourState _state;
        GundamMoveSubStateMachine _subMachine = null;
        GundamMoveSubStateMachine subMachine
        {
            get
            {
                if (_subMachine == null)
                    _subMachine = new GundamMoveSubStateMachine(this);
                return _subMachine;
            }
        }
        public IState state { get => subMachine.state; set => subMachine.state = value; }
        public event Action<IStateMachine, IState> onStateChange
        {
            add { ((IStateMachine)subMachine).onStateChange += value; }
            remove { ((IStateMachine)subMachine).onStateChange -= value; }
        }
        public IState getDefaultState()
        {
            return ((IStateMachine)subMachine).getDefaultState();
        }
        public IState[] getAllStates()
        {
            return ((IStateMachine)subMachine).getAllStates();
        }
        public TState getState<TState>() where TState : IState
        {
            return ((IStateMachine)subMachine).getState<TState>();
        }
        protected abstract IState onTransit();
        class GundamMoveSubStateMachine : StateMachineObject<BehaviourSubStateMachine>
        {
            public GundamMoveSubStateMachine(BehaviourSubStateMachine monobehaviour) : base(monobehaviour)
            {
            }
            protected override IState getState()
            {
                return monobehaviour._state;
            }
            protected override void setState(IState state)
            {
                monobehaviour._state = state as BehaviourState;
            }
            public override IState getDefaultState()
            {
                return monobehaviour._defaultState;
            }
            public override IState[] getAllStates()
            {
                return monobehaviour.GetComponentsInChildren<IState>(true);
            }
            public override TState getState<TState>()
            {
                return monobehaviour.GetComponentInChildren<TState>(true);
            }
            protected override IState onTransit()
            {
                return monobehaviour.onTransit();
            }
        }
    }
    public abstract class BehaviourSubStateMachine<T> : BehaviourSubStateMachine where T : IStateMachine
    {
        public T machine
        {
            get
            {
                if (_machine == null)
                    _machine = GetComponentInParent<T>();
                return _machine;
            }
        }
        [SerializeField]
        T _machine;
    }
}