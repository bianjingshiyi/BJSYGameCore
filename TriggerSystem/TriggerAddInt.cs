using System;

using UnityEngine;

namespace TBSGameCore.TriggerSystem
{
    public class TriggerAddInt : TriggerExpr
    {
        [Type(typeof(int))]
        [SerializeField]
        TriggerExpr _left;
        public TriggerExpr left
        {
            get { return _left; }
            set { _left = value; }
        }
        [Type(typeof(int))]
        [SerializeField]
        TriggerExpr _right;
        public TriggerExpr right
        {
            get { return _right; }
            set { _right = value; }
        }
        public override string desc
        {
            get { return "(" + left.desc + "+" + right.desc + ")"; }
        }
        public override object getValue(UnityEngine.Object targetObject)
        {
            int leftValue;
            if (left != null)
                leftValue = (int)left.getValue(targetObject);
            else
                leftValue = 0;
            int rightValue;
            if (right != null)
                rightValue = (int)right.getValue(targetObject);
            else
                rightValue = 0;
            return leftValue + rightValue;
        }
    }
}