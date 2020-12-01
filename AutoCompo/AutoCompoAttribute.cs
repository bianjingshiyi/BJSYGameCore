using System;
namespace BJSYGameCore.AutoCompo
{
    [AttributeUsage(AttributeTargets.Class |
                    AttributeTargets.Field |
                    AttributeTargets.Property |
                    AttributeTargets.Method |
                    AttributeTargets.Event, Inherited = true, AllowMultiple = false)]
    public sealed class AutoCompoAttribute : Attribute
    {
        public int instanceID;
        public string[] tags;
        public AutoCompoAttribute(int instanceID = 0, params string[] tags)
        {
            this.instanceID = instanceID;
            this.tags = tags;
        }
    }
}