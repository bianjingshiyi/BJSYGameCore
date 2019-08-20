using System;

namespace BJSYGameCore.SaveSystem
{
    public class LoadBeforeAttribute : Attribute
    {
        public Type targetType
        {
            get; private set;
        }
        public LoadBeforeAttribute(Type targetType)
        {
            this.targetType = targetType;
        }
    }
}