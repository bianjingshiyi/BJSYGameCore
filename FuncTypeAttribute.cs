using System;

using UnityEngine;

namespace BJSYGameCore
{
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    sealed class FuncTypeAttribute : PropertyAttribute
    {
        Type _type;
        public Type type
        {
            get { return _type; }
        }
        public FuncTypeAttribute(Type type)
        {
            _type = type;
        }
    }
}