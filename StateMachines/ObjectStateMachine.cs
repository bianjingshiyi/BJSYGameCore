using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace BJSYGameCore.StateMachines
{
    public abstract class ObjectStateMachine : StateMachine
    {
        public override IState[] getAllStates()
        {
            if (dicTypeState == null)
            {
                //获取所有ObjectState子类
                Type[] stateTypes = GetType().GetNestedTypes().Where(t => { return !t.IsAbstract && t.IsSubclassOf(typeof(ObjectState)); }).ToArray();
                dicTypeState = new Dictionary<Type, ObjectState>();
                for (int i = 0; i < stateTypes.Length; i++)
                {
                    //先获取带有参数的构造器
                    ConstructorInfo constructor = stateTypes[i].GetConstructor(new Type[] { typeof(ObjectStateMachine) });
                    if (constructor != null)
                    {
                        //如果存在，那么用有参数的构造
                        dicTypeState.Add(stateTypes[i], constructor.Invoke(new object[] { this }) as ObjectState);
                    }
                    else
                    {
                        //如果不存在，那么获取没有参数的
                        constructor = stateTypes[i].GetConstructor(new Type[0]);
                        dicTypeState.Add(stateTypes[i], constructor.Invoke(new object[0]) as ObjectState);
                    }
                }
            }
            return dicTypeState.Values.ToArray();
        }
        Dictionary<Type, ObjectState> dicTypeState { get; set; } = null;
        public override T getState<T>()
        {
            if (dicTypeState.ContainsKey(typeof(T)))
                return (T)(object)dicTypeState[typeof(T)];
            else
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
            public ObjectState(ObjectStateMachine machine)
            {
                this.machine = machine;
            }
            protected ObjectStateMachine machine { get; }
            public virtual void onEntry()
            {
            }
            public virtual void onExit()
            {
            }
            public virtual void onUpdate()
            {
            }
        }
    }
}