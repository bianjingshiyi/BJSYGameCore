using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace BJSYGameCore
{
    public static class LinqHelper
    {
        public static IEnumerable<T> skipUntil<T>(this IEnumerable<T> c, Func<T, bool> func)
        {
            return c.SkipWhile(e => !func(e));
        }
        public static IEnumerable<T> takeUntil<T>(this IEnumerable<T> c, Func<T, bool> func)
        {
            return c.TakeWhile(e => !func(e));
        }
        public static bool isSubset<T>(this IEnumerable<T> set, IEnumerable<T> subset)
        {
            return subset.All(e => set.Contains(e));
        }
        public static bool SequenceEqual<T>(this IEnumerable<T> c1, IEnumerable<T> c2, Comparison<T> comparison)
        {
            if (comparison == null)
                throw new ArgumentNullException("comparison");
            IEnumerator<T> e1 = c1.GetEnumerator();
            IEnumerator<T> e2 = c2.GetEnumerator();
            do
            {
                bool b1 = e1.MoveNext();
                bool b2 = e2.MoveNext();
                if (b1 != b2)
                    return false;
                if (b1)
                {
                    T t1 = e1.Current;
                    T t2 = e2.Current;
                    if (comparison(t1, t2) != 0)
                        return false;
                }
                else
                    return true;
            }
            while (true);
        }
    }
}