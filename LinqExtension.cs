using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
namespace BJSYGameCore
{
    public static class LinqExtension
    {
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