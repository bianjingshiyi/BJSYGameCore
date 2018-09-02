using System;

namespace TBSGameCore
{
    public class LoadAfterAttribute : Attribute
    {
        public Type targetType
        {
            get; private set;
        }
        public LoadAfterAttribute(Type targetType)
        {
            this.targetType = targetType;
        }
    }
}