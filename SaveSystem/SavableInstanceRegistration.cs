using System;

using UnityEngine;

namespace BJSYGameCore.SaveSystem
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