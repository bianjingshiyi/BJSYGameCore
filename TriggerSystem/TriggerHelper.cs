using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using UnityEngine;

namespace TBSGameCore.TriggerSystem
{
    public static class TriggerHelper
    {
        public static Dictionary<string, TriggerActionDefine> load(Assembly assembly)
        {
            Dictionary<string, TriggerActionDefine> dicActionDefine = new Dictionary<string, TriggerActionDefine>();
            foreach (Type type in assembly.GetTypes())
            {
                foreach (MethodInfo method in type.GetMethods())
                {
                    TriggerActionAttribute attribute = method.GetCustomAttribute<TriggerActionAttribute>();
                    if (attribute != null)
                    {
                        dicActionDefine.Add(attribute.name, new ReflectedActionDefine(attribute.name, attribute.desc, method));
                    }
                }
            }
            return dicActionDefine;
        }
        static Dictionary<string, TriggerActionDefine> _dicBuiltinActions = null;
        public static TriggerActionDefine getBuiltInAction(string name)
        {
            if (_dicBuiltinActions == null)
                _dicBuiltinActions = load(typeof(TriggerHelper).Assembly);
            return _dicBuiltinActions.ContainsKey(name) ? _dicBuiltinActions[name] : null;
        }
        public static TriggerActionDefine[] getBuiltInActions()
        {
            if (_dicBuiltinActions == null)
                _dicBuiltinActions = load(typeof(TriggerHelper).Assembly);
            return _dicBuiltinActions.Values.ToArray();
        }
    }
}