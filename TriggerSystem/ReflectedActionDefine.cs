using System.Reflection;
using System.Linq;

using UnityEngine;

namespace TBSGameCore.TriggerSystem
{
    public class ReflectedActionDefine : TriggerActionDefine
    {
        public MethodInfo info { get; private set; }
        public ReflectedActionDefine(string displayName, string displayDesc, MethodInfo info) : base(displayName, displayDesc)
        {
            this.info = info;
        }
        public override TriggerActionComponent createInstance(Transform parent)
        {
            ReflectedActionComponent instance = ReflectedActionComponent.createInstance(this);
            instance.transform.parent = parent;
            return instance;
        }
        public override TriggerActionComponent attachTo(GameObject go)
        {
            ReflectedActionComponent instance = ReflectedActionComponent.attachTo(go, this);
            return instance;
        }
        public override TriggerParameterDefine[] getParameters()
        {
            return info.GetParameters().Select(e => { return new TriggerParameterDefine(e.Name, e.ParameterType); }).ToArray();
        }
    }
}