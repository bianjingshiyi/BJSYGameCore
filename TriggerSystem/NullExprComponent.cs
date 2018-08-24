using System;
using UnityEngine;

namespace TBSGameCore.TriggerSystem
{
    public class NullExprComponent : ConstExprComponent
    {
        public override TriggerExprDefine define
        {
            get { return new NullConstDefine(); }
        }
        public override string desc
        {
            get { return "空"; }
        }
        public override object invoke()
        {
            return null;
        }
    }
    public class NullConstDefine : TriggerConstDefine
    {
        public override string name
        {
            get { return "空值常量"; }
        }
        public override Type returnType
        {
            get { return typeof(object); }
        }
        public override TriggerExprComponent attachTo(GameObject go)
        {
            go.name = name;
            return go.AddComponent<NullExprComponent>();
        }
        public override TriggerExprComponent createInstance(Transform parent)
        {
            NullExprComponent instance = new GameObject(name).AddComponent<NullExprComponent>();
            instance.transform.parent = parent;
            return instance;
        }
    }
}
