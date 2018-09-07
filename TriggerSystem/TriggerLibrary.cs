using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using UnityEngine;

namespace TBSGameCore.TriggerSystem
{
    public static class TriggerLibrary
    {
        static Dictionary<Assembly, Dictionary<string, TriggerReflectMethodDefine>> _dicFuncLibrary = null;
        public static void load(Assembly assembly)
        {
            if (_dicFuncLibrary == null)
                _dicFuncLibrary = new Dictionary<Assembly, Dictionary<string, TriggerReflectMethodDefine>>();
            if (!_dicFuncLibrary.ContainsKey(assembly))
            {
                _dicFuncLibrary.Add(assembly, new Dictionary<string, TriggerReflectMethodDefine>());
                foreach (Type type in assembly.GetTypes())
                {
                    foreach (MethodInfo method in type.GetMethods())
                    {
                        TriggerMethodAttribute att = method.GetCustomAttribute<TriggerMethodAttribute>();
                        if (att != null)
                        {
                            _dicFuncLibrary[assembly].Add(att.idName, new TriggerReflectMethodDefine(att, method));
                        }
                    }
                }
            }
        }
        public static bool isAssemblyLoaded(Assembly assembly)
        {
            return _dicFuncLibrary != null && _dicFuncLibrary.ContainsKey(assembly);
        }
        public static TriggerReflectMethodDefine getMethodDefine(string idName)
        {
            if (string.IsNullOrEmpty(idName))
                return null;
            if (_dicFuncLibrary != null)
            {
                foreach (var adp in _dicFuncLibrary)
                {
                    if (adp.Value.ContainsKey(idName))
                        return adp.Value[idName];
                }
                return null;
            }
            else
                return null;
        }
        public static TriggerReflectMethodDefine[] getMethodDefines()
        {
            List<TriggerReflectMethodDefine> funcList = new List<TriggerReflectMethodDefine>();
            foreach (var adp in _dicFuncLibrary)
            {
                funcList.AddRange(adp.Value.Values);
            }
            return funcList.ToArray();
        }
        public static TriggerReflectMethodDefine[] getFuncDefines(Type returnType)
        {
            List<TriggerReflectMethodDefine> funcList = new List<TriggerReflectMethodDefine>();
            foreach (var adp in _dicFuncLibrary)
            {
                funcList.AddRange(adp.Value.Values.Where(e => { return returnType.IsAssignableFrom(e.returnType) || e.returnType.IsSubclassOf(returnType); }));
            }
            return funcList.ToArray();
        }
        public static TriggerReflectMethodDefine[] getActionDefines()
        {
            List<TriggerReflectMethodDefine> actionList = new List<TriggerReflectMethodDefine>();
            foreach (var adp in _dicFuncLibrary)
            {
                actionList.AddRange(adp.Value.Values.Where(e => { return e.returnType == typeof(void); }));
            }
            return actionList.ToArray();
        }
    }
}