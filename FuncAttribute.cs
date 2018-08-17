using System;

using UnityEngine;

namespace TBSGameCore
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class FuncAttribute : PropertyAttribute
    {
        public string funcName { get; private set; }
        public string description { get; private set; }
        public FuncAttribute(string funcName, string desc)
        {
            this.funcName = funcName;
            this.description = desc;
        }
    }
}