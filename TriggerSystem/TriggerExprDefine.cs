using System;

using UnityEngine;

namespace TBSGameCore.TriggerSystem
{
    public abstract class TriggerExprDefine
    {
        public abstract string name
        {
            get;
        }
        public abstract Type returnType
        {
            get;
        }
        public abstract TriggerExprComponent createInstance(Transform parent);
        public abstract TriggerExprComponent attachTo(GameObject go);
        public override bool Equals(object obj)
        {
            if (obj is TriggerExprDefine)
                return name == (obj as TriggerExprDefine).name;
            else
                return false;
        }
        public override int GetHashCode()
        {
            return name.GetHashCode();
        }
    }
}