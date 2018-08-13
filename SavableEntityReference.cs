using System;

using UnityEngine;

using TBSGameCore;

namespace TBSGameCore
{
    [Serializable]
    public class SavableEntityReference
    {
        [SerializeField]
        public int id;
        public SavableEntityReference(int id)
        {
            this.id = id;
        }
        public T to<T>(MonoBehaviour behaviour) where T : SavableEntity
        {
            return behaviour.findInstance<Game>().getInstanceById<T>(id);
        }
    }
}