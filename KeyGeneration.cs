using System;
using System.Linq;
using System.Collections.Generic;
namespace BJSYGameCore
{
    public static class KeyGeneration
    {
        /// <summary>
        /// 生成指定数量不重复的字符串
        /// </summary>
        /// <param name="count"></param>
        /// <param name="keyLength">最长32，不填默认最长</param>
        /// <param name="maxTime">生成key的最大次数，防止死循环。默认为36的Count次幂</param>
        /// <param name="existingKeys">已经存在的Key的集合，防止重复</param>
        /// <returns></returns>
        public static string[] generateUniqueKeys(int count, int keyLength = -1, int maxTime = -1, ISet<string> existingKeys = null)
        {
            if (count < 1)
                throw new ArgumentException("生成字符串的数量不能小于1", nameof(count));
            if (keyLength == 0 || keyLength > 32)
                throw new ArgumentException("Key的长度不能为0或者超过32", nameof(keyLength));
            if (maxTime < count)
                maxTime = (int)Math.Pow(36, count);
            HashSet<string> keySet = new HashSet<string>();
            int time = 0;
            do
            {
                Guid guid = Guid.NewGuid();
                string key = keyLength < 0 ? guid.ToString() : guid.ToString().Substring(0, keyLength);
                key = key.Replace("-", string.Empty).ToUpper();
                if (!keySet.Contains(key) && (existingKeys == null || !existingKeys.Contains(key)))
                    keySet.Add(key);
                time++;
            }
            while (keySet.Count < count || time > maxTime);
            return keySet.ToArray();
        }
    }
}