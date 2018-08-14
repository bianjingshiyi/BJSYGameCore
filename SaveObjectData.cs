using System;

namespace TBSGameCore
{
    [Serializable]
    public class SaveObjectData
    {
        public int id;
        public string path;
        public ILoadableData data;
    }
}