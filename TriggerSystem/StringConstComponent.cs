using System;
using UnityEngine;

namespace TBSGameCore.TriggerSystem
{
    public class StringConstComponent : ConstExprComponent
    {
        public override TriggerExprDefine define
        {
            get { return new StringConstDefine(); }
        }
        public override string desc
        {
            get { return _value; }
        }
        [SerializeField]
        string _value;
        public string value
        {
            get { return _value; }
            set { _value = value; }
        }
        public override object invoke()
        {
            return _value;
        }
    }
    public class StringConstDefine : TriggerConstDefine<StringConstComponent>
    {
        public override string name
        {
            get { return "字符串常量"; }
        }
        public override Type returnType
        {
            get { return typeof(string); }
        }
    }
}
