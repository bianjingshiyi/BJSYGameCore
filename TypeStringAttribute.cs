using System;
using UnityEngine;
namespace BJSYGameCore
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class TypeStringAttribute : PropertyAttribute
    {
        public Type baseType { get; }
        public TypeStringAttribute(Type baseType = null)
        {
            this.baseType = baseType;
        }
    }
}