using System;

using UnityEngine;

namespace TBSGameCore
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class RefTypeAttribute : PropertyAttribute
    {
        Type _type;
        public Type type
        {
            get { return _type; }
        }
        public RefTypeAttribute(Type type)
        {
            _type = type;
        }
    }
}