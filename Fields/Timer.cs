using System;
using UnityEngine;
namespace BJSYGameCore
{
    [Serializable]
    public class Timer
    {
        public float startTime = -1;
        public float duration = 0;
        public bool isStarted
        {
            get { return startTime >= 0; }
        }
        public void reset()
        {
            startTime = -1;
        }
        public void start()
        {
            startTime = Time.time;
        }
        public void start(float now)
        {
            startTime = now;
        }
        public bool isExpired()
        {
            return Time.time - startTime >= duration;
        }
        public bool isExpired(float now)
        {
            return now - startTime >= duration;
        }
    }
}