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
            return isExpired(Time.time);
        }
        public bool isExpired(bool includeNotStarted)
        {
            return includeNotStarted ? isExpired() || startTime < 0 : isExpired();
        }
        public bool isExpired(float now)
        {
            return duration > 0 ? now - startTime >= duration : false;
        }
        public float getRemainedTime()
        {
            return startTime + duration <= Time.time ? duration - (Time.time - startTime) : 0;
        }
        public float progress
        {
            get
            {
                if (startTime < 0)
                    return 0;
                else if (startTime + duration >= Time.time)
                    return (Time.time - startTime) / duration;
                else
                    return 1;
            }
        }
    }
}