
using UnityEngine;

namespace TBSGameCore.TriggerSystem
{
    public abstract class TriggerActionDefine
    {
        public string name { get; private set; }
        public string desc { get; private set; }
        public TriggerActionDefine(string name, string desc)
        {
            this.name = name;
            this.desc = desc;
        }
        public abstract TriggerActionComponent createInstance(Transform parent);
        public abstract TriggerActionComponent attachTo(GameObject go);
        public override bool Equals(object obj)
        {
            if (obj is TriggerActionDefine)
                return name == (obj as TriggerActionDefine).name;
            else
                return false;
        }
        public override int GetHashCode()
        {
            return name.GetHashCode();
        }
        public abstract TriggerParameterDefine[] getParameters();
    }
}