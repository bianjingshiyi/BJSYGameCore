
using UnityEngine;

namespace BJSYGameCore.StateMachines
{
    public abstract class BehaviourState : MonoBehaviour, IState
    {
        public abstract void onEntry();
        public abstract void onUpdate();
        public abstract void onExit();
    }
    public abstract class BehaviourState<T> : BehaviourState where T : BehaviourStateMachine
    {
        public T machine
        {
            get { return _machine; }
        }
        [SerializeField]
        T _machine;
    }
}