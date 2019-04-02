
using UnityEngine;

namespace BJSYGameCore.StateMachines
{
    public abstract class BehaviourStateMachine : StateMachine
    {
        protected override IState getDefaultState()
        {
            return _defaultState;
        }
        [SerializeField]
        BehaviourState _defaultState;
        protected override IState getState()
        {
            return _currentState;
        }
        protected override void setState(IState state)
        {
            _currentState = state as BehaviourState;
        }
        [SerializeField]
        BehaviourState _currentState;
        public override T getState<T>()
        {
            return GetComponentInChildren<T>(true);
        }
        public override IState[] getAllStates()
        {
            if (_states == null || _states.Length < 1)
                _states = GetComponentsInChildren<BehaviourState>(true);
            return _states;
        }
        [SerializeField]
        BehaviourState[] _states;
    }
}