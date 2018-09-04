using UnityEngine;

namespace TBSGameCore.TriggerSystem
{
    public abstract class TriggerConst : TriggerExpr
    {
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
            get { return value.ToString(); }
        }
        public override object getValue(Object targetObject)
        {
            return value;
        }
    }
}
