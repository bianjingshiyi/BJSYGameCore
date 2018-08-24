using System;
using UnityEngine;

namespace TBSGameCore.TriggerSystem
{
    public class TriggerVariableDefine : TriggerExprDefine
    {
        public override string name { get; }
        public override Type returnType { get; } = null;
        public TriggerVariableDefine(string name, Type type)
        {
            this.name = name;
            returnType = type;
        }
        public override TriggerExprComponent createInstance(Transform parent)
        {
            TriggerVariableComponent instance = new GameObject(name).AddComponent<TriggerVariableComponent>();
            instance.transform.parent = parent;
            return instance;
        }
        public override TriggerExprComponent attachTo(GameObject go)
        {
            go.name = name;
            return go.AddComponent<TriggerVariableComponent>();
        }
    }
}