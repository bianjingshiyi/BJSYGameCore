using System;

namespace BJSYGameCore
{
    [Serializable]
    public class Range
    {
        public float min;
        public float max;
        public Range()
        {
            min = 0;
            max = 0;
        }
        public Range(float min, float max)
        {
            this.min = min;
            this.max = max;
        }
    }
}