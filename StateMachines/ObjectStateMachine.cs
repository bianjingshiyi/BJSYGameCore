using System;
using System.Linq;
using System.Reflection;

namespace BJSYGameCore.StateMachines
{
    public abstract class ObjectStateMachine : StateMachine
    {
        public override IState[] getAllStates()
        {
            if (_states == null)
            {
                //获取所有ObjectState子类
                Type[] stateTypes = GetType().GetNestedTypes().Where(t => { return !t.IsAbstract && t.IsSubclassOf(typeof(ObjectState)); }).ToArray();
                _states = new ObjectState[stateTypes.Length];
                for (int i = 0; i < stateTypes.Length; i++)
                {
                    //先获取带有参数的构造器
                    ConstructorInfo constructor = stateTypes[i].GetConstructor(new Type[] { GetType() });
                    if (constructor != null)
                    {
                        //如果存在，那么用有参数的构造
                        _states[i] = constructor.Invoke(new object[] { this }) as ObjectState;
                    }
                    else
                    {
                        //如果不存在，那么获取没有参数的
                        constructor = stateTypes[i].GetConstructor(new Type[0]);
                        _states[i] = constructor.Invoke(new object[0]) as ObjectState;
                    }
                }
            }
            return _states;
        }
        ObjectState[] _states = null;
        public override T getState<T>()
        {
            foreach (IState state in getAllStates())
            {
                if (state is T)
                    return (T)state;
            }
            return default;
        }
        protected override IState getState()
        {
            return _state;
        }
        protected override void setState(IState state)
        {
            _state = state as ObjectState;
        }
        ObjectState _state;
        public abstract class ObjectState : IState
        {
            public abstract void onEntry();
            public abstract void onExit();
            public abstract void onUpdate();
        }
        public abstract class ObjectState<T> : ObjectState where T : ObjectStateMachine
        {
            public ObjectState(T machine)
            {
                this.machine = machine;
            }
            protected T machine { get; }
        }
    }
}