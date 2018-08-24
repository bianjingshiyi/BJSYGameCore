using System;
using UnityEngine;

namespace TBSGameCore.TriggerSystem
{
    public class IntConstComponent : ConstExprComponent
    {
        public override TriggerExprDefine define
        {
            get { return new IntConstDefine(); }
        }
        public override string desc
        {
            get { return _value.ToString(); }
        }
        [SerializeField]
        int _value;
        public override object invoke()
        {
            return _value;
        }
    }
    public class IntConstDefine : TriggerConstDefine<IntConstComponent>
    {
        public override string name
        {
            get { return "整数常量"; }
        }
        public override Type returnType
        {
            get { return typeof(int); }
        }
    }
}
