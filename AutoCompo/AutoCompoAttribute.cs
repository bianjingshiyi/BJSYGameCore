using System;
namespace BJSYGameCore.AutoCompo
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public sealed class AutoCompoAttribute : Attribute
    {
        public int instanceID { get; }
        public AutoCompoAttribute(int instanceID)
        {
            this.instanceID = instanceID;
        }
    }
}