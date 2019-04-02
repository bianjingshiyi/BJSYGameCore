using System;
using UnityEngine;

namespace BJSYGameCore
{
    public abstract class AbstractFuncStringDrawer : TriggerStringDrawer
    {
        public AbstractFuncStringDrawer(UnityEngine.Object targetObject) : base(targetObject)
        {
        }
        public AbstractFuncStringDrawer(TriggerStringDrawer parent) : base(parent)
        {
        }
        public abstract string draw(Rect position, GUIContent label, string value, Type returnType);
        public static AbstractFuncStringDrawer factory(string value, TriggerStringDrawer parent)
        {
            if (string.IsNullOrEmpty(value))
                return null;
            else if (value[0] == '0')
                return new ConstStringDrawer(parent);
            else if (value[0] == '1')
                return new VariableStringDrawer(parent);
            else if (value[0] == '2')
                return new FuncStringDrawer(parent);
            else
                return null;
        }
    }
}