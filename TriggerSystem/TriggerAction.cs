
using UnityEngine;

namespace BJSYGameCore.TriggerSystem
{
    public abstract class TriggerAction : MonoBehaviour
    {
        public TriggerScopeAction scope
        {
            get { return transform.parent != null ? transform.parent.GetComponent<TriggerScopeAction>() : null; }
        }
        public int index
        {
            get { return transform.GetSiblingIndex(); }
        }
        public abstract string desc
        {
            get;
        }
        public abstract void invoke(Object targetObject);
    }
}