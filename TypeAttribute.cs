using System;
using UnityEngine;
namespace BJSYGameCore
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class TypeAttribute : PropertyAttribute
    {
        public Type type { get; }
        public TypeAttribute(Type type)
        {
            this.type = type;
        }
    }
}