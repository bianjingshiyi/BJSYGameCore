using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using UnityEngine;

namespace BJSYGameCore
{
    [Serializable]
    public class TriggerFunc
    {
        public string value;
        public TriggerFunc(string value)
        {
            this.value = value;
        }
        public object getValue(UnityEngine.Object targetObject, Type targetType)
        {
            string realValue = value.Substring(1, value.Length - 1);
            if (value[0] == '0')
                return getConstValue(realValue, targetObject, targetType);
            else if (value[0] == '1')
                return getVariableValue(realValue, targetObject);
            else if (value[0] == '2')
                return getInvokeValue(realValue, targetObject);
            else
                return null;
        }
        public object getConstValue(string value, UnityEngine.Object targetObject, Type targetType)
        {
            if (targetType == typeof(int))
                return TriggerParser.parseInt(value);
            else if (targetType == typeof(float))
                return TriggerParser.parseFloat(value);
            else if (targetType == typeof(bool))
                return TriggerParser.parseBool(value);
            else if (targetType == typeof(string))
                return value;
            else if (targetType.IsSubclassOf(typeof(Component)) && targetObject is Component)
                return TriggerParser.parseInstanceReference(value).findInstanceIn((targetObject as Component).gameObject.scene, targetType);
            else
                return null;
        }
        public object getVariableValue(string value, UnityEngine.Object targetObject)
        {
            if (targetObject is Component)
            {
                VariableManager manager = (targetObject as Component).gameObject.scene.findInstance<VariableManager>();
                if (manager != null)
                {
                    return null;//变量的坑
                }
                else
                    return null;
            }
            else
                return null;
        }
        public object getInvokeValue(string value, UnityEngine.Object targetObject)
        {
            string className;
            string methodName;
            string[] args;
            TriggerParser.parseFunc(value, out className, out methodName, out args);
            Type targetType = targetObject.GetType().Assembly.GetType(className);
            if (targetType != null)
            {
                MethodInfo targetMethod = targetType.GetMethod(methodName);
                if (targetMethod != null)
                {
                    ParameterInfo[] targetParas = targetMethod.GetParameters();
                    if (targetMethod.IsStatic)
                    {
                        if (args.Length == targetParas.Length)
                        {
                            object[] paras = new object[targetParas.Length];
                            for (int i = 0; i < paras.Length; i++)
                            {
                                paras[i] = new TriggerFunc(args[i]).getValue(targetObject, targetParas[i].ParameterType);
                            }
                            return targetMethod.Invoke(null, paras);
                        }
                        else
                            return null;
                    }
                    else
                    {
                        if (args.Length == targetParas.Length + 1)
                        {
                            object obj = new TriggerFunc(args[0]).getValue(targetObject, targetType);
                            object[] paras = new object[targetParas.Length];
                            for (int i = 1; i < args.Length; i++)
                            {
                                paras[i - 1] = new TriggerFunc(args[i]).getValue(targetObject, targetParas[i - 1].ParameterType);
                            }
                            return targetMethod.Invoke(obj, paras);
                        }
                        else
                            return null;
                    }
                }
                else
                    return null;
            }
            else
                return null;
        }
    }
}