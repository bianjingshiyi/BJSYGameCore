using System;

namespace TBSGameCore.TriggerSystem
{
    public class TriggerParameterDefine
    {
        public string name { get; private set; }
        public Type type { get; private set; }
        public TriggerParameterDefine(string name,Type type)
        {
            this.name = name;
            this.type = type;
        }
    }
}