using System;

namespace BJSYGameCore.SaveSystem
{
    [Serializable]
    public class SaveObjectData
    {
        public float priority;
        public ILoadableData data;
        public SaveObjectData(float priority, ILoadableData data)
        {
            this.priority = priority;
            this.data = data;
        }
    }
}