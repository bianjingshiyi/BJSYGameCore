using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace BJSYGameCore.StateMachines
{
    public abstract class StateMachine : MonoBehaviour
    {
        protected void Awake()
        {
            state = getDefaultState();
        }
        protected void Update()
        {
            IState nextState = onTransit();
            if (nextState != null)
            {
                Debug.Log("发生状态转换：" + state + "->" + nextState, this);
                state = nextState;
            }
            if (state != null)
                state.onUpdate();
        }
        protected abstract IState getDefaultState();
        protected abstract IState onTransit();
        public IState state
        {
            get { return getState(); }
            set
            {
                if (state != null)
                    state.onExit();
                setState(value);
                if (state != null)
                    state.onEntry();
            }
        }
        protected abstract IState getState();
        protected abstract void setState(IState state);
        public abstract IState[] states
        {
            get;
        }
        public abstract T getState<T>() where T : IState;
    }
    public interface IState
    {
        void onEntry();
        void onUpdate();
        void onExit();
    }
}