
using UnityEngine;

namespace TBSGameCore.TriggerSystem
{
    [DisallowMultipleComponent]
    public abstract class TriggerObjectComponent : MonoBehaviour
    {
        public ITriggerScope scope
        {
            get { return GetComponentInParent<ITriggerScope>(); }
        }
    }
}
