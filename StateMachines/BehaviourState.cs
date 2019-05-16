
using UnityEngine;

namespace BJSYGameCore.StateMachines
{
    public abstract class BehaviourState : MonoBehaviour, IState
    {
        public virtual bool isExiting
        {
            get { return false; }
        }
        public virtual void onEntry()
        {
        }
        public virtual void onUpdate()
        {
        }
        public virtual void onExit()
        {
        }
    }
    public abstract class BehaviourState<T> : BehaviourState where T : IStateMachine
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