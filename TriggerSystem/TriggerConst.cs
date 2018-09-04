using System;
using System.Linq;

using UnityEngine;

namespace TBSGameCore.TriggerSystem
{
    public abstract class TriggerConst : TriggerExpr
    {
        public static TriggerConst getConstOfType(Type targetType)
        {
            Type parentType = typeof(TriggerConst<>).MakeGenericType(targetType);
            Type childType = typeof(TriggerConst).Assembly.GetTypes().FirstOrDefault(e => { return e.IsSubclassOf(parentType); });
            if (childType != null)
                return new GameObject().AddComponent(childType) as TriggerConst;
            else
                return null;
        }
    }
    public abstract class TriggerConst<T> : TriggerConst
    {
        [SerializeField]
        T _value;
        public T value
        {
            get { return _value; }
            set { _value = value; }
        }
        public override string desc
        {
            get { return value != null ? value.ToString() : "Null"; }
        }
        public override object getValue(UnityEngine.Object targetObject)
        {
            return value;
        }
    }
}
