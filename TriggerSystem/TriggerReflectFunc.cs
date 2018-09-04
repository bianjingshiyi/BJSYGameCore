using System.Linq;

using UnityEngine;

namespace TBSGameCore.TriggerSystem
{
    public class TriggerReflectFunc : TriggerExpr
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
                TriggerFuncDefine define = TriggerLibrary.getFuncDefine(idName);
                if (define != null)
                {
                    string desc = define.descString;
                    for (int i = 0; i < define.paras.Length; i++)
                    {
                        string argDesc;
                        if (i < args.Length && args[i] != null)
                            argDesc = args[i].desc;
                        else
                            argDesc = "Null";
                        desc = desc.Replace("{" + i + "}", argDesc);
                    }
                    return desc;
                }
                else
                    return "空函数";
            }
        }
        public override object getValue(UnityEngine.Object targetObject)
        {
            if (!TriggerLibrary.isAssemblyLoaded(targetObject.GetType().Assembly))
                TriggerLibrary.load(targetObject.GetType().Assembly);
            TriggerFuncDefine define = TriggerLibrary.getFuncDefine(idName);
            return define.invoke(args.Select(e => { return e.getValue(targetObject); }).ToArray());
        }
    }
}