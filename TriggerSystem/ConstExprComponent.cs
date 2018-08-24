using System;
using System.Reflection;
using System.Collections.Generic;

using UnityEngine;

namespace TBSGameCore.TriggerSystem
{
    public abstract class ConstExprComponent : TriggerExprComponent
    {
        public static Dictionary<Type, Type> _dicTypeConst = null;
        public static ConstExprComponent createInstance(Type targetType)
        {
            TriggerConstDefine define = getComponentType(targetType);
            return define.attachTo(new GameObject(define.name)) as ConstExprComponent;
        }
        public static ConstExprComponent attachTo(GameObject go, Type targetType)
        {
            TriggerConstDefine define = getComponentType(targetType);
            go.name = define.name;
            return define.attachTo(go) as ConstExprComponent;
        }
        private static TriggerConstDefine getComponentType(Type targetType)
        {
            if (targetType == typeof(int))
                return new IntConstDefine();
            else if (targetType == typeof(float))
                return new FloatConstDefine();
            else if (targetType == typeof(bool))
                return new BoolConstDefine();
            else if (targetType == typeof(string))
                return new StringConstDefine();
            return new NullConstDefine();
        }
    }
}
