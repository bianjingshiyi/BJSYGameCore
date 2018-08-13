using System;
using System.Collections.Generic;

using UnityEngine;

namespace TBSGameCore
{
    [Serializable]
    public class GameData
    {
        public string name = "本地游戏";
        public DateTime date = DateTime.Now;
        public List<ILoadableData> savedObjects = null;
    }
}