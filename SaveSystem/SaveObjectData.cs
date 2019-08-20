using System;

namespace BJSYGameCore.SaveSystem
{
    [Serializable]
    public class SaveObjectData
    {
        public int id;
        public string path;
        public float priority;
        public ILoadableData data;
        public SaveObjectData(int id, string path, float priority, ILoadableData data)
        {
            this.id = id;
            this.path = path;
            this.priority = priority;
            this.data = data;
        }
    }
}