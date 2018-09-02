using System;

namespace TBSGameCore
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