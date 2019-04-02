using System;

using UnityEngine;

namespace BJSYGameCore
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class FuncAttribute : PropertyAttribute
    {
        public string funcName { get; private set; }
        public string desc { get; private set; }
        public FuncAttribute(string funcName, string desc)
        {
            this.funcName = funcName;
            this.desc = desc;
        }
    }
}