using System;

namespace TBSGameCore.TriggerSystem
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    public class TriggerParameterAttribute : Attribute
    {
        public string name { get; private set; }
        public TriggerParameterAttribute(string name)
        {
            this.name = name;
        }
    }
}