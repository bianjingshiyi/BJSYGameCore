using System;
using System.Reflection;
using System.Collections.Generic;

namespace BJSYGameCore
{
    public abstract class TriggerStringDrawer
    {
        protected bool isExpanded { get; set; } = false;
        public abstract float height
        {
            get;
        }
        protected TriggerStringDrawer parent
        {
            get; private set;
        }
        UnityEngine.Object _targetObject = null;
        protected UnityEngine.Object targetObject
        {
            get { return parent != null ? parent.targetObject : _targetObject; }
        }
        private Method[] _actions;
        protected Method[] actions
        {
            get { return parent != null ? parent.actions : _actions; }
        }
        public TriggerStringDrawer(UnityEngine.Object targetObject)
        {
            _targetObject = targetObject;
            //自带
            List<Method> actionList = new List<Method>();
            actionList.Add(new IfAction());
            //反射
            foreach (Type type in targetObject.GetType().Assembly.GetTypes())
            {
                foreach (MethodInfo method in type.GetMethods())
                {
                    ActionAttribute att = method.GetCustomAttribute<ActionAttribute>();
                    if (att != null)
                    {
                        actionList.Add(new ReflectedMethod(type, method, att.actionName, att.desc));
                    }
                }
            }
            _actions = actionList.ToArray();
        }
        public TriggerStringDrawer(TriggerStringDrawer parent)
        {
            this.parent = parent;
        }
    }
}