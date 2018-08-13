using System;

using UnityEngine;

namespace TBSGameCore
{
    [Serializable]
    public class SavableInstanceRegistration
    {
        [SerializeField]
        public int id;
        [SerializeField]
        public SavableInstance instance;
        public SavableInstanceRegistration(int id, SavableInstance instance)
        {
            this.id = id;
            this.instance = instance;
        }
    }
}