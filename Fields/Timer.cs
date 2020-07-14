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
            if (startTime < 0)
                return false;
            if (duration < 0)
                return false;
            return now - startTime >= duration;
        }
        public float getRemainedTime()
        {
            return startTime + duration <= Time.time ? duration - (Time.time - startTime) : 0;
        }
        public float time
        {
            get { return startTime < 0 ? 0 : Time.time - startTime; }
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
        /// <summary>
        /// 获取截取计时器上一段时间的进度。
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public float getProgress(float start, float end)
        {
            if (Time.time - startTime > end)
                return 1;
            else if (Time.time - startTime > start)
                return (Time.time - startTime - start) / (start - end);
            else
                return 0;
        }
    }
}