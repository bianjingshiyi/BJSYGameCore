using System;
using UnityEngine;

namespace BJSYGameCore
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class InterfaceAttribute : PropertyAttribute
    {
        public Type type { get; }
        public InterfaceAttribute(Type interfaceType)
        {
            type = interfaceType;
        }
    }
}