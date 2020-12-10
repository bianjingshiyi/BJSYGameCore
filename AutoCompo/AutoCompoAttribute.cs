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
        public string path;
        public string[] tags;
        public AutoCompoAttribute(int instanceID, string path, string[] tags)
        {
            this.instanceID = instanceID;
            this.path = path;
            this.tags = tags;
        }
        public AutoCompoAttribute(int instanceID, params string[] tags)
            : this(instanceID, null, tags)
        {
        }
        public AutoCompoAttribute()
            : this(0, null, new string[0])
        {

        }
    }
}