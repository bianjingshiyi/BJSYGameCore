using System;

using UnityEngine;

namespace TBSGameCore
{
    [Serializable]
    public class SavableEntityRegistration
    {
        [SerializeField]
        public int id;
        [SerializeField]
        public SavableEntity instance;
        public SavableEntityRegistration(int id, SavableEntity instance)
        {
            this.id = id;
            this.instance = instance;
        }
    }
}