using System;
namespace BJSYGameCore.AutoCompo
{
    [AttributeUsage(AttributeTargets.Class |
                    AttributeTargets.Field |
                    AttributeTargets.Property |
                    AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public sealed class AutoCompoAttribute : Attribute
    {
        public int instanceID;
        public AutoCompoAttribute(int instanceID)
        {
            this.instanceID = instanceID;
        }
    }
}