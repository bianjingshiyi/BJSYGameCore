using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

namespace TBSGameCore.TriggerSystem
{
    public class TriggerFuncDefine
    {
        public Type returnType
        {
            get { return info.ReturnType; }
        }
        public string idName { get; private set; }
        public string editorName { get; private set; }
        public string descString { get; private set; }
        public TriggerParameterDefine[] paras
        {
            get; private set;
        }
        MethodInfo info { get; set; }
        public TriggerFuncDefine(TriggerFuncAttribute attribute, MethodInfo method)
        {
            idName = attribute.idName;
            editorName = attribute.editorName;
            descString = attribute.desc;
            info = method;
            List<TriggerParameterDefine> paraList = new List<TriggerParameterDefine>();
            if (!info.IsStatic)
                paraList.Add(new TriggerParameterDefine(method.DeclaringType, "对象"));
            foreach (ParameterInfo para in info.GetParameters())
            {
                TriggerParameterAttribute paraAtt = para.GetCustomAttribute<TriggerParameterAttribute>();
                if (paraAtt != null)
                    paraList.Add(new TriggerParameterDefine(para, paraAtt));
                else
                    paraList.Add(new TriggerParameterDefine(para));
            }
            paras = paraList.ToArray();
        }
        public object invoke(object[] args)
        {
            if (info.IsStatic)
                return info.Invoke(null, args);
            else
                return info.Invoke(args[0], args.Skip(1).ToArray());
        }
    }
}