using System.Linq;

using UnityEngine;

namespace BJSYGameCore.TriggerSystem
{
    public class TriggerReflectAction : TriggerAction
    {
        [SerializeField]
        string _idName;
        public string idName
        {
            get { return _idName; }
            set { _idName = value; }
        }
        [SerializeField]
        TriggerExpr[] _args;
        public TriggerExpr[] args
        {
            get { return _args; }
            set { _args = value; }
        }
        public override string desc
        {
            get
            {
                TriggerReflectMethodDefine define = TriggerLibrary.getMethodDefine(idName);
                if (define != null)
                {
                    string desc = define.descString;
                    for (int i = 0; i < define.paras.Length; i++)
                    {
                        string argDesc;
                        if (args != null && i < args.Length && args[i] != null)
                            argDesc = args[i].desc;
                        else
                            argDesc = "Null";
                        desc = desc.Replace("{" + i + "}", argDesc);
                    }
                    return desc;
                }
                else
                    return "空动作";
            }
        }
        public override void invoke(UnityEngine.Object targetObject)
        {
            if (!TriggerLibrary.isAssemblyLoaded(targetObject.GetType().Assembly))
                TriggerLibrary.load(targetObject.GetType().Assembly);
            TriggerReflectMethodDefine define = TriggerLibrary.getMethodDefine(idName);
            define.invoke(args.Select(e => { return e != null ? e.getValue(targetObject) : null; }).ToArray());
        }
    }
}