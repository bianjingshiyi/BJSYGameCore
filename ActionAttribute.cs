using System;

using UnityEngine;

namespace TBSGameCore
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class ActionAttribute : PropertyAttribute
    {
        public string actionName { get; private set; }
        public string desc { get; private set; }
        public ActionAttribute(string actionName, string desc)
        {
            this.actionName = actionName;
            this.desc = desc;
        }
    }
}