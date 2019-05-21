using System;

using UnityEngine;

namespace BJSYGameCore.StateMachines
{
    public abstract class BehaviourSubStateMachine : BehaviourState, IStateMachine
    {
        [SerializeField]
        BehaviourState _defaultState;
        [SerializeField]
        BehaviourState _nextState;
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
        public IState state { get => subMachine.state; }
        public override void onEntry()
        {
            base.onEntry();
            subMachine.onEnable();
        }
        public override void onUpdate()
        {
            base.onUpdate();
            subMachine.onUpdate();
        }
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
        public void setNextState(IState state)
        {
            ((IStateMachine)subMachine).setNextState(state);
        }
        protected abstract IState onTransit(IState state);
        class GundamMoveSubStateMachine : StateMachineObject<BehaviourSubStateMachine>
        {
            public GundamMoveSubStateMachine(BehaviourSubStateMachine monobehaviour) : base(monobehaviour)
            {
            }
            protected override IState getStateField()
            {
                return monobehaviour._state;
            }
            protected override void setStateField(IState state)
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
            protected override IState onTransit(IState state)
            {
                return monobehaviour.onTransit(state);
            }
            protected override IState getNextState()
            {
                return monobehaviour._nextState;
            }
            public override void setNextState(IState state)
            {
                monobehaviour._nextState = state as BehaviourState;
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