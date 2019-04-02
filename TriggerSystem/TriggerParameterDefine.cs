using System;
using System.Reflection;

namespace BJSYGameCore.TriggerSystem
{
    public class TriggerParameterDefine
    {
        public Type type
        {
            get; private set;
        }
        public string name { get; private set; }
        public TriggerParameterDefine(Type type, string name)
        {
            this.type = type;
            this.name = name;
        }
        public TriggerParameterDefine(ParameterInfo info)
        {
            type = info.ParameterType;
            name = info.Name;
        }
        public TriggerParameterDefine(ParameterInfo info, TriggerParameterAttribute attribute)
        {
            type = info.ParameterType;
            name = attribute.name;
        }
    }
}