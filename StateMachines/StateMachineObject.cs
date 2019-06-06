using System;

using UnityEngine;

namespace BJSYGameCore.StateMachines
{
    public abstract class StateMachineObject : IStateMachine
    {
        public virtual void onEnable()
        {
            state = getDefaultState();
        }
        /// <summary>
        /// 调用onTransit方法获取下一个状态，如果为空则不进行转换。如果当前状态为空，则设置为默认状态，并调用当前状态的onUpdate方法。
        /// </summary>
        public virtual void onUpdate()
        {
            IState nextState = getNextStateField();
            if (nextState != null)
            {
                setNextStateField(null);
                nextState = onTransit(nextState);
            }
            else
                nextState = onTransit(state);
            if (nextState != null)
                state = nextState;
            if (state == null)
                state = getDefaultState();
            if (state != null)
                state.onUpdate();
        }
        public abstract IState getDefaultState();
        /// <summary>
        /// 默认返回null，不会发生任何状态转换。
        /// </summary>
        /// <returns></returns>
        protected virtual IState onTransit(IState state)
        {
            return null;
        }
        /// <summary>
        /// 状态机的当前状态。直接赋值会立刻改变状态机的状态，不推荐这么干。应当考虑重写onTransit方法来实现状态之间的转换。
        /// </summary>
        public IState state
        {
            get { return getStateField(); }
            private set
            {
                IState lastState = state;
                if (lastState != null)
                    state.onExit();
                setStateField(value);
                if (state != null)
                    state.onEntry();
                onStateChange?.Invoke(this, lastState, state);
            }
        }
        public event Action<IStateMachine, IState, IState> onStateChange;
        /// <summary>
        /// 用于实现state属性。
        /// </summary>
        /// <returns></returns>
        protected abstract IState getStateField();
        /// <summary>
        /// 用于实现state属性。
        /// </summary>
        /// <param name="state"></param>
        protected abstract void setStateField(IState state);
        public abstract IState[] getAllStates();
        public abstract TState getState<TState>() where TState : IState;
        protected abstract IState getNextStateField();
        public abstract void setNextStateField(IState state);
    }
    [Serializable]
    public abstract class StateMachineObject<T> : StateMachineObject, IStateMachine where T : MonoBehaviour, IStateMachine
    {
        public T monobehaviour
        {
            get { return _monobehaviour; }
        }
        [SerializeField]
        T _monobehaviour;
        public StateMachineObject(T monobehaviour)
        {
            _monobehaviour = monobehaviour;
        }
    }
}