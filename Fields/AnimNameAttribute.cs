using System;
using UnityEngine;

namespace BJSYGameCore
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class AnimNameAttribute : PropertyAttribute
    {
    }
}