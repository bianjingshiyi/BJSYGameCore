using System;
using System.Reflection;

namespace TBSGameCore.TriggerSystem
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class TriggerActionAttribute : Attribute
    {
        public string name { get; private set; }
        public string desc { get; private set; }
        public TriggerActionAttribute(string name, string desc)
        {
            this.name = name;
            this.desc = desc;
        }
    }
}
