using System;

namespace TBSGameCore.TriggerSystem
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class TriggerFuncAttribute : Attribute
    {
        public string idName { get; private set; }
        public string editorName { get; private set; }
        public string desc { get; private set; }
        public TriggerFuncAttribute(string idName, string editorName, string desc)
        {
            this.idName = idName;
            this.editorName = editorName;
            this.desc = desc;
        }
    }
}