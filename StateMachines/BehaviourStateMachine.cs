using System;
using System.Collections.Generic;

using UnityEngine;

namespace BJSYGameCore.StateMachines
{
    [Obsolete("请使用MonoBehaviour通过StateMachineObject实现IStateMachine作为替代")]
    public class BehaviourStateMachine : StateMachine
    {
        [SerializeField]
        BehaviourState _defaultState;
        [SerializeField]
        BehaviourState _currentState;
        [SerializeField]
        BehaviourState[] _states;
        public override IState getDefaultState()
        {
            return _defaultState;
        }
        protected override IState getState()
        {
            return _currentState;
        }
        protected override void setState(IState state)
        {
            _currentState = state as BehaviourState;
        }
        public override T getState<T>()
        {
            return GetComponentInChildren<T>();
        }
        public override IState[] getAllStates()
        {
            if (_states == null || _states.Length == 0)
            {
                List<BehaviourState> stateList = new List<BehaviourState>();
                //先把当前物体上的状态都获取到，这些状态肯定是这个状态机的状态。
                stateList.AddRange(GetComponents<BehaviourState>());
                for (int i = 0; i < transform.childCount; i++)
                {
                    //然后获取子物体上的状态。
                    stateList.AddRange(getStates(transform.GetChild(i)));
                }
                _states = stateList.ToArray();
            }
            return _states;
        }
        BehaviourState[] getStates(Transform transform)
        {

            BehaviourSubStateMachine subMachine = transform.GetComponent<BehaviourSubStateMachine>();
            if (subMachine != null)
            {
                //如果有子状态机，那么这个物体上的状态和子物体上的状态都是这个状态机的状态。
                return new BehaviourState[] { subMachine };
            }
            else
            {
                //没有状态机，这个子物体以及它的子物体里的状态都是这个状态机的子状态。
                List<BehaviourState> stateList = new List<BehaviourState>();
                stateList.AddRange(transform.GetComponents<BehaviourState>());
                for (int i = 0; i < transform.childCount; i++)
                {
                    stateList.AddRange(getStates(transform.GetChild(i)));
                }
                return stateList.ToArray();
            }
        }
    }
}