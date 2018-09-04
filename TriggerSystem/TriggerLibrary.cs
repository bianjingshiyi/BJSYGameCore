using System;
using System.Reflection;
using System.Collections.Generic;

namespace TBSGameCore.TriggerSystem
{
    public static class TriggerLibrary
    {
        static Dictionary<Assembly, Dictionary<string, TriggerFuncDefine>> _dicLibrary = null;
        public static void load(Assembly assembly)
        {
            if (_dicLibrary == null)
                _dicLibrary = new Dictionary<Assembly, Dictionary<string, TriggerFuncDefine>>();
            if (!_dicLibrary.ContainsKey(assembly))
            {
                _dicLibrary.Add(assembly, new Dictionary<string, TriggerFuncDefine>());
                foreach (Type type in assembly.GetTypes())
                {
                    foreach (MethodInfo method in type.GetMethods())
                    {
                        TriggerFuncAttribute att = method.GetCustomAttribute<TriggerFuncAttribute>();
                        if (att != null)
                        {
                            _dicLibrary[assembly].Add(att.idName, new TriggerFuncDefine(att, method));
                        }
                    }
                }
            }
        }
        public static bool isAssemblyLoaded(Assembly assembly)
        {
            return _dicLibrary != null && _dicLibrary.ContainsKey(assembly);
        }
        public static TriggerFuncDefine getFuncDefine(string idName)
        {
            if (string.IsNullOrEmpty(idName))
                return null;
            if (_dicLibrary != null)
            {
                foreach (var adp in _dicLibrary)
                {
                    if (adp.Value.ContainsKey(idName))
                        return adp.Value[idName];
                }
                return null;
            }
            else
                return null;
        }
        public static TriggerFuncDefine[] getFuncDefines()
        {
            List<TriggerFuncDefine> funcList = new List<TriggerFuncDefine>();
            foreach (var adp in _dicLibrary)
            {
                funcList.AddRange(adp.Value.Values);
            }
            return funcList.ToArray();
        }
    }
}