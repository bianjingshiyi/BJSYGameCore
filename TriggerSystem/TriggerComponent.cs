using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace TBSGameCore.TriggerSystem
{
    public class TriggerComponent : TriggerObjectComponent, ITriggerScope
    {
        [SerializeField]
        [Type(typeof(ITriggerEventSource))]
        private InstanceReference _eventSource;
        public TriggerVariableDefine getVariable(string name)
        {
            return null;
        }
        public object getVariableValue(string name)
        {
            return null;
        }
        public TriggerVariableDefine[] getVariables()
        {
            return new TriggerVariableDefine[0];
        }
        public TriggerActionDefine getAction(string name)
        {
            return (!_eventSource.isNull) ? _eventSource.findInstanceIn<ITriggerEventSource>(gameObject.scene).getAction(name) : TriggerHelper.getBuiltInAction(name);
        }
        public TriggerActionDefine[] getActions()
        {
            return (!_eventSource.isNull) ?
                   _eventSource.findInstanceIn<ITriggerEventSource>(gameObject.scene).getActions().Union(TriggerHelper.getBuiltInActions()).ToArray() :
                   TriggerHelper.getBuiltInActions();
        }
        public TriggerExprDefine getFunc(string name)
        {
            return (!_eventSource.isNull) ? _eventSource.findInstanceIn<ITriggerEventSource>(gameObject.scene).getFunc(name) : null;
        }
        public TriggerExprDefine[] getFuncs(Type returnType)
        {
            return (!_eventSource.isNull) ?
                   _eventSource.findInstanceIn<ITriggerEventSource>(gameObject.scene).getFuncs(returnType) :
                   new TriggerExprDefine[0];
        }
        [ContextMenu("执行")]
        public void invoke()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                TriggerActionComponent action = transform.GetChild(i).GetComponent<TriggerActionComponent>();
                if (action != null)
                    action.invoke();
            }
        }
    }
    public abstract class TriggerActionComponent : TriggerObjectComponent
    {
        public abstract TriggerActionDefine define
        {
            get;
        }
        public abstract string desc
        {
            get;
        }
        public abstract void invoke();
    }
    public abstract class TriggerExprComponent : TriggerObjectComponent
    {
        public abstract TriggerExprDefine define
        {
            get;
        }
        public abstract string desc
        {
            get;
        }
        public abstract object invoke();
    }
}
