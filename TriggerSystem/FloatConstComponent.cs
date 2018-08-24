using System;
using UnityEngine;
namespace TBSGameCore.TriggerSystem
{
    public class FloatConstComponent : ConstExprComponent
    {
        public override TriggerExprDefine define
        {
            get { return new FloatConstDefine(); }
        }
        public override string desc
        {
            get { return _value.ToString(); }
        }
        [SerializeField]
        float _value;
        public override object invoke()
        {
            return _value;
        }
    }
    public class FloatConstDefine : TriggerConstDefine<FloatConstComponent>
    {
        public override string name
        {
            get { return "实数常量"; }
        }
        public override Type returnType
        {
            get { return typeof(float); }
        }
    }
}
