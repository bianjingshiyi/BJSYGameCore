using System;

using UnityEngine;

using TBSGameCore;

namespace TBSGameCore
{
    [Serializable]
    public class SavableInstanceReference
    {
        [SerializeField]
        public int id;
        public SavableInstanceReference(int id)
        {
            this.id = id;
        }
        public T to<T>(MonoBehaviour behaviour) where T : SavableInstance
        {
            return behaviour.findInstance<SaveManager>().getInstanceById<T>(id);
        }
    }
}