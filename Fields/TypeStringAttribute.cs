using System;
using UnityEngine;
using System.Reflection;
namespace BJSYGameCore
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class TypeStringAttribute : PropertyAttribute
    {
        public Assembly[] assemblies { get; } = null;
        public Type baseType { get; }
        public TypeStringAttribute(Type baseType)
        {
            this.baseType = baseType;
            this.assemblies = assemblies;
        }
    }
}