namespace BJSYGameCore
{
    public static class StringHelper
    {
        public static string headToUpper(this string str)
        {
            if (char.IsLower(str[0]))
                str = char.ToUpper(str[0]) + str.Substring(1, str.Length - 1);
            return str;
        }
        public static string headToLower(this string str)
        {
            if (char.IsUpper(str[0]))
                str = char.ToLower(str[0]) + str.Substring(1, str.Length - 1);
            return str;
        }
        public static bool tryMerge(this string head, string rear, out string merged)
        {
            for (int i = 0; i < head.Length; i++)
            {
                string middle = head.Substring(i, head.Length - i);
                if (rear.StartsWith(middle))
                {
                    merged = head.Substring(0, head.Length - middle.Length) + rear;
                    return true;
                }
            }
            merged = string.Empty;
            return false;
        }
    }
}