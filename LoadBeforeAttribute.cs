using System;

namespace BJSYGameCore
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