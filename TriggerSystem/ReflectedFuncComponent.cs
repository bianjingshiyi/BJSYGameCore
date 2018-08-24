using System.Reflection;

namespace TBSGameCore.TriggerSystem
{
    public class ReflectedFuncComponent : TriggerExprComponent
    {
        public new string name
        {
            get { return gameObject.name; }
        }
        public override TriggerExprDefine define
        {
            get { return scope.getFunc(name); }
        }
        public override string desc
        {
            get
            {
                ReflectedFuncDefine define = this.define as ReflectedFuncDefine;
                if (define != null)
                    return define.desc;
                else
                    return "没有函数";
            }
        }
        public override object invoke()
        {
            TriggerExprDefine funcDefine = scope.getFunc(name);
            if (funcDefine != null && funcDefine is ReflectedFuncDefine)
            {
                MethodInfo method = (funcDefine as ReflectedFuncDefine).info;
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
                    return method.Invoke(null, args);
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
                    return method.Invoke(obj, args);
                }
            }
            else
                return null;
        }
    }
}