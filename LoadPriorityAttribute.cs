using System;

namespace TBSGameCore
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class LoadPriorityAttribute : Attribute
    {
        public float priority
        {
            get; private set;
        }
        public LoadPriorityAttribute(float priority)
        {
            this.priority = priority;
        }
    }
}