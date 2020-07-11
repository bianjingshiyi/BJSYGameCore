using System;
using System.Reflection;
namespace BJSYGameCore
{
    public static class SystemHelper
    {
        public static string removeHead(this string path, string head)
        {
            int index = path.LastIndexOf(head);
            if (index < 0)
                throw new IndexOutOfRangeException();
            path = path.Substring(index + head.Length, path.Length - index - head.Length);
            return path;
        }
        public static string removeRear(this string path, string rear)
        {
            int index = path.LastIndexOf(rear);
            if (index < 0)
                throw new IndexOutOfRangeException();
            return path.Substring(0, index);
        }
        public static bool isObsolete(this object obj)
        {
            return obj.GetType().GetCustomAttribute<ObsoleteAttribute>() != null;
        }
    }
}