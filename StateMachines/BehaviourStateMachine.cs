
using UnityEngine;

namespace BJSYGameCore.StateMachines
{
    public class BehaviourStateMachine : StateMachine
    {
        public override IState getDefaultState()
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
            return GetComponentInChildren<T>();
        }
        public override IState[] getAllStates()
        {
            return GetComponentsInChildren<IState>();
        }
    }
}