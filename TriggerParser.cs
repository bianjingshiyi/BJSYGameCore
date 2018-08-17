using System.Collections.Generic;

namespace TBSGameCore
{
    public static class TriggerParser
    {
        public static int parseInt(string value)
        {
            int intValue;
            if (!int.TryParse(value, out intValue))
                intValue = 0;
            return intValue;
        }
        public static float parseFloat(string value)
        {
            float floatValue;
            if (!float.TryParse(value, out floatValue))
                floatValue = 0;
            return floatValue;
        }
        public static bool parseBool(string value)
        {
            bool boolValue;
            if (!bool.TryParse(value, out boolValue))
                boolValue = false;
            return boolValue;
        }
        public static InstanceReference parseInstanceReference(string value)
        {
            if (value == null)
                return new InstanceReference(0);
            int id = 0;
            string path = "";
            for (int i = 0; i < value.Length; i++)
            {
                if (value[i] == ':')
                {
                    id = parseInt(value.Substring(0, i));
                    path = value.Substring(i + 1, value.Length - i - 1);
                }
            }
            return new InstanceReference(id, path);
        }
        public static void parseFunc(string value, out string className, out string methodName, out string[] args)
        {
            className = null;
            methodName = null;
            args = new string[0];
            if (value == null)
                return;
            int b = -1;
            List<string> argList = new List<string>();
            int startIndex = 0;
            for (int i = 0; i < value.Length; i++)
            {
                if (value[i] == '.')
                {
                    className = value.Substring(0, i);
                    startIndex = i + 1;
                }
                else if (value[i] == '(')
                {
                    if (b == -1)
                    {
                        methodName = value.Substring(startIndex, i - startIndex);
                        b = 0;
                        startIndex = i + 1;
                    }
                    b++;
                }
                else if (value[i] == ')')
                {
                    b--;
                    if (b == 0)
                    {
                        if (startIndex < i)
                        {
                            string arg = value.Substring(startIndex, i - startIndex);
                            argList.Add(arg);
                        }
                    }
                }
                else if (value[i] == ',' && b == 1)
                {
                    string arg = value.Substring(startIndex, i - startIndex);
                    argList.Add(arg);
                    startIndex = i + 1;
                }
            }
            args = argList.ToArray();
        }
        public static void parseAction(string value, out string className, out string methodName, out string[] args)
        {
            className = null;
            methodName = null;
            args = new string[0];
            if (value == null)
                return;
            int b = -1;
            List<string> argList = new List<string>();
            int startIndex = 0;
            for (int i = 0; i < value.Length; i++)
            {
                if (value[i] == '.' && b == -1)
                {
                    className = value.Substring(0, i);
                    startIndex = i + 1;
                }
                else if (value[i] == '(')
                {
                    if (b == -1)
                    {
                        methodName = value.Substring(startIndex, i - startIndex);
                        b = 0;
                        startIndex = i + 1;
                    }
                    b++;
                }
                else if (value[i] == ')')
                {
                    b--;
                    if (b == 0)
                    {
                        if (startIndex < i)
                        {
                            string arg = value.Substring(startIndex, i - startIndex);
                            argList.Add(arg);
                        }
                    }
                }
                else if (value[i] == ',' && b == 1)
                {
                    string arg = value.Substring(startIndex, i - startIndex);
                    argList.Add(arg);
                    startIndex = i + 1;
                }
            }
            args = argList.ToArray();
        }
    }
}