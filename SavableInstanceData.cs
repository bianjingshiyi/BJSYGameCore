using System;
using System.Linq;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace BJSYGameCore
{
    [Serializable]
    public class SavableInstanceData
    {
        public int id;
        public string path;
        public SavableInstanceData(int id, string path)
        {
            this.id = id;
            this.path = path;
        }
    }
}