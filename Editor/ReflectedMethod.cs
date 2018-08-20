using System;
using System.Reflection;
using System.Collections.Generic;

namespace TBSGameCore
{
    public class ReflectedMethod : Method
    {
        public Type objType { get; private set; }
        public MethodInfo info { get; private set; }
        public override string formatName
        {
            get { return objType.FullName + '.' + info.Name; }
        }
        public ReflectedMethod(Type objType, MethodInfo info, string funcName, string desc) : base(funcName, desc)
        {
            this.objType = objType;
            this.info = info;
        }
        public Parameter[] getParameters()
        {
            List<Parameter> paraList = new List<Parameter>();
            if (!info.IsStatic)
            {
                paraList.Add(new Parameter(objType, "object"));
            }
            foreach (ParameterInfo p in info.GetParameters())
            {
                paraList.Add(new Parameter(p.ParameterType, p.Name));
            }
            return paraList.ToArray();
        }
    }
}