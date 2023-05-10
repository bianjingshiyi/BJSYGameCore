using System;
namespace BJSYGameCore
{
    public static class ArrayHelper
    {
        public static int indexOf(this Array array, object obj)
        {
            for (int i = 0; i < array.Length; i++)
            {
                if (array.GetValue(i).Equals(obj))
                    return i;
            }
            return -1;
        }
    }
}