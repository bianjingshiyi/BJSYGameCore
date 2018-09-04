
using UnityEngine;

namespace TBSGameCore.TriggerSystem
{
    public abstract class TriggerAction : MonoBehaviour
    {
        public abstract string desc
        {
            get;
        }
        public abstract void invoke(UnityEngine.Object targetObject);
    }
}