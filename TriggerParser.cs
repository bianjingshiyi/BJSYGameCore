using System.Collections.Generic;

namespace BJSYGameCore
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
                        if (startIndex < i)
                            methodName = value.Substring(startIndex, i - startIndex);
                        startIndex = i + 1;
                        b = 0;
                    }
                    b++;
                }
                else if (value[i] == ')')
                {
                    b--;
                    if (b == 0)
                    {
                        if (startIndex < i)
                            argList.Add(value.Substring(startIndex, i - startIndex));
                        else
                            argList.Add(null);
                    }
                }
                else if (value[i] == ',' && b == 1)
                {
                    if (startIndex < i)
                        argList.Add(value.Substring(startIndex, i - startIndex));
                    else
                        argList.Add(null);
                    startIndex = i + 1;
                }
            }
            args = argList.ToArray();
        }
        public static void parseAction(string value, out string formatName)
        {
            formatName = null;
            if (value == null)
                return;
            for (int i = 0; i < value.Length; i++)
            {
                if (value[i] == '(')
                    formatName = value.Substring(0, i);
            }
        }
        public static void parseReflectedAction(string value, out string className, out string methodName, out string[] args)
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
                        b = 0;
                        if (startIndex < i)
                            methodName = value.Substring(startIndex, i - startIndex);
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
                            argList.Add(value.Substring(startIndex, i - startIndex));
                        else
                            argList.Add(null);
                    }
                }
                else if (value[i] == ',' && b == 1)
                {
                    if (startIndex < i)
                        argList.Add(value.Substring(startIndex, i - startIndex));
                    else
                        argList.Add(null);
                    startIndex = i + 1;
                }
            }
            args = argList.ToArray();
        }
        public static void parseActions(string value, out string[] actions)
        {
            if (string.IsNullOrEmpty(value))
            {
                actions = new string[0];
                return;
            }
            List<string> actionList = new List<string>();
            int startIndex = 0;
            int b = 0;
            for (int i = 0; i < value.Length; i++)
            {
                if (value[i] == '{')
                    b++;
                else if (value[i] == ';' && b == 0)
                {
                    actionList.Add(value.Substring(startIndex, i - startIndex));
                    startIndex = i + 1;
                }
                else if (value[i] == '}')
                    b--;
            }
            actions = actionList.ToArray();
        }
        public static void parseIf(string value, out string condition, out string thenActions, out string elseActions)
        {
            condition = null;
            thenActions = null;
            elseActions = null;
            if (value == null)
                return;
            int b0 = 0;
            int b1 = 0;
            int b2 = 0;
            int startIndex = 0;
            for (int i = 0; i < value.Length; i++)
            {
                if (b0 > 0)
                {
                    if (value[i] == '(')
                    {
                        b0++;
                        startIndex = i + 1;
                    }
                    else if (value[i] == ')')
                    {
                        b0--;
                        if (b0 == 0)
                        {
                            condition = value.Substring(startIndex, i - startIndex);
                            b0 = -1;
                        }
                    }
                }
                else if (b1 > 0)
                {
                    if (value[i] == '{')
                    {
                        b1++;
                        startIndex = i + 1;
                    }
                    else if (value[i] == '}')
                    {
                        b1--;
                        if (b1 == 0)
                        {
                            thenActions = value.Substring(startIndex, i - startIndex);
                            b1 = -1;
                        }
                    }
                }
                else if (b2 > 0)
                {
                    if (value[i] == '{')
                    {
                        b2++;
                        startIndex = i + 1;
                    }
                    else if (value[i] == '}')
                    {
                        b2--;
                        if (b2 == 0)
                        {
                            elseActions = value.Substring(startIndex, i - startIndex);
                            b2 = -1;
                        }
                    }
                }
            }
        }
    }
}