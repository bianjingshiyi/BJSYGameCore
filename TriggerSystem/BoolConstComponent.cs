using System;
using UnityEngine;

namespace TBSGameCore.TriggerSystem
{
    public class BoolConstComponent : ConstExprComponent
    {
        public override TriggerExprDefine define
        {
            get { return new BoolConstDefine(); }
        }
        public override string desc
        {
            get { return _value ? "真" : "假"; }
        }
        public override object invoke()
        {
            return _value;
        }
        [SerializeField]
        bool _value;
    }
    public class BoolConstDefine : TriggerConstDefine<BoolConstComponent>
    {
        public override string name
        {
            get { return "真值常量"; }
        }
        public override Type returnType
        {
            get { return typeof(bool); }
        }
    }
}
