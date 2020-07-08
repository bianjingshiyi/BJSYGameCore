using System;
using System.Reflection;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
namespace BJSYGameCore
{
    public static class LinqExtension
    {
        //public static TCast LastCast<TCast>(this IEnumerable source)
        //{
        //    return source.(e => e is TCast) is TCast t ? t : default;
        //}
    }
    public static class SystemHelper
    {
        public static bool isObsolete(this object obj)
        {
            return obj.GetType().GetCustomAttribute<ObsoleteAttribute>() != null;
        }
        public static IEnumerable<T> skipUntil<T>(this IEnumerable<T> c, Func<T, bool> func)
        {
            return c.SkipWhile(e => !func(e));
        }
        public static IEnumerable<T> takeUntil<T>(this IEnumerable<T> c, Func<T, bool> func)
        {
            return c.TakeWhile(e => !func(e));
        }
    }
}