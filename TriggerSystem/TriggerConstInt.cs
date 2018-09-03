
using UnityEngine;

namespace TBSGameCore.TriggerSystem
{
    public class TriggerConstInt : TriggerExpr
    {
        [SerializeField]
        int _value;
        public int value
        {
            get { return _value; }
            set { _value = value; }
        }
        public override object getValue()
        {
            return _value;
        }
    }
}
