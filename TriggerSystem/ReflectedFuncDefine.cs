using System;
using System.Linq;
using System.Reflection;

using UnityEngine;

namespace TBSGameCore.TriggerSystem
{
    public class ReflectedFuncDefine : TriggerExprDefine
    {
        public override string name
        {
            get;
        }
        public string desc
        {
            get; private set;
        }
        public override Type returnType
        {
            get { return info.ReturnType; }
        }
        public MethodInfo info
        {
            get;
        }
        public ReflectedFuncDefine(string name, string desc, MethodInfo info)
        {
            this.name = name;
            this.desc = desc;
            this.info = info;
        }
        public override TriggerExprComponent createInstance(Transform parent)
        {
            TriggerExprComponent instance = new GameObject(name).AddComponent<ReflectedFuncComponent>();
            instance.transform.parent = parent;
            foreach (TriggerParameterDefine para in getParameters())
            {
                ConstExprComponent arg = ConstExprComponent.createInstance(para.type);
                arg.transform.parent = instance.transform;
            }
            return instance;
        }
        public override TriggerExprComponent attachTo(GameObject go)
        {
            go.name = name;
            ReflectedFuncComponent instance = go.AddComponent<ReflectedFuncComponent>();
            foreach (TriggerParameterDefine para in getParameters())
            {
                ConstExprComponent arg = ConstExprComponent.createInstance(para.type);
                arg.transform.parent = instance.transform;
            }
            return instance;
        }
        public TriggerParameterDefine[] getParameters()
        {
            return info != null ? info.GetParameters().Select(e => { return new TriggerParameterDefine(e.Name, e.ParameterType); }).ToArray() : new TriggerParameterDefine[0];
        }
    }
}