using System.Reflection;

using UnityEngine;

namespace TBSGameCore.TriggerSystem
{
    public class ReflectedActionComponent : TriggerActionComponent
    {
        public new string name
        {
            get { return gameObject.name; }
            private set { gameObject.name = value; }
        }
        public override string desc
        {
            get { return define.desc; }
        }
        public static ReflectedActionComponent createInstance(ReflectedActionDefine action)
        {
            ReflectedActionComponent instance = new GameObject(action.name).AddComponent<ReflectedActionComponent>();
            instance.name = action.name;
            foreach (ParameterInfo para in action.info.GetParameters())
            {
                ConstExprComponent arg = ConstExprComponent.createInstance(para.ParameterType);
                arg.transform.parent = instance.transform;
            }
            return instance;
        }
        public static ReflectedActionComponent attachTo(GameObject go, ReflectedActionDefine action)
        {
            ReflectedActionComponent instance = go.AddComponent<ReflectedActionComponent>();
            instance.name = action.name;
            foreach (ParameterInfo para in action.info.GetParameters())
            {
                ConstExprComponent arg = ConstExprComponent.createInstance(para.ParameterType);
                arg.transform.parent = instance.transform;
            }
            return instance;
        }
        public override TriggerActionDefine define
        {
            get { return scope.getAction(name); }
        }
        public override void invoke()
        {
            TriggerActionDefine actionDefine = scope.getAction(name);
            if (actionDefine != null && actionDefine is ReflectedActionDefine)
            {
                MethodInfo method = (actionDefine as ReflectedActionDefine).info;
                if (method.IsStatic)
                {
                    ParameterInfo[] paras = method.GetParameters();
                    object[] args = new object[paras.Length];
                    for (int i = 0; i < args.Length; i++)
                    {
                        if (i < transform.childCount)
                            args[i] = transform.GetChild(i).GetComponent<TriggerExprComponent>().invoke();
                        else
                            args[i] = null;
                    }
                    method.Invoke(null, args);
                }
                else
                {
                    object obj = transform.GetChild(0).GetComponent<TriggerExprComponent>().invoke();
                    ParameterInfo[] paras = method.GetParameters();
                    object[] args = new object[paras.Length];
                    for (int i = 0; i < args.Length; i++)
                    {
                        if (i + 1 < transform.childCount)
                            args[i] = transform.GetChild(i + 1).GetComponent<TriggerExprComponent>().invoke();
                        else
                            args[i] = null;
                    }
                    method.Invoke(obj, args);
                }
            }
        }
    }
}