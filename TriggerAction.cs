using System;
using System.Reflection;



namespace TBSGameCore
{
    [Serializable]
    public class TriggerAction
    {
        public string value;
        public TriggerAction(string value)
        {
            this.value = value;
        }
        public void invoke(UnityEngine.Object targetObject)
        {
            if (value.Substring(0, 2) == "if")
                invokeIf(targetObject);
            else
                invokeAction(targetObject);
        }
        private void invokeIf(UnityEngine.Object targetObject)
        {
            string condition;
            string thenActions;
            string elseActions;
            TriggerParser.parseIf(value, out condition, out thenActions, out elseActions);
            object conObj = new TriggerFunc(condition).getValue(targetObject, typeof(bool));
            bool conValue = false;
            if (conObj != null)
                conValue = (bool)conObj;
            if (conValue)
            {
                string[] actions;
                TriggerParser.parseActions(thenActions, out actions);
                for (int i = 0; i < actions.Length; i++)
                {
                    new TriggerAction(actions[i]).invoke(targetObject);
                }
            }
            else
            {
                string[] actions;
                TriggerParser.parseActions(elseActions, out actions);
                for (int i = 0; i < actions.Length; i++)
                {
                    new TriggerAction(actions[i]).invoke(targetObject);
                }
            }
        }
        private void invokeAction(UnityEngine.Object targetObject)
        {
            string className;
            string methodName;
            string[] args;
            TriggerParser.parseFunc(value, out className, out methodName, out args);
            Type targetType = targetObject.GetType().Assembly.GetType(className);
            if (targetType != null)
            {
                MethodInfo targetMethod = targetType.GetMethod(methodName);
                if (targetMethod != null)
                {
                    ParameterInfo[] targetParas = targetMethod.GetParameters();
                    if (targetMethod.IsStatic)
                    {
                        if (args.Length == targetParas.Length)
                        {
                            object[] paras = new object[targetParas.Length];
                            for (int i = 0; i < paras.Length; i++)
                            {
                                paras[i] = new TriggerFunc(args[i]).getValue(targetObject, targetParas[i].ParameterType);
                            }
                            targetMethod.Invoke(null, paras);
                        }
                    }
                    else
                    {
                        if (args.Length == targetParas.Length + 1)
                        {
                            object obj = new TriggerFunc(args[0]).getValue(targetObject, targetType);
                            object[] paras = new object[targetParas.Length];
                            for (int i = 1; i < args.Length; i++)
                            {
                                paras[i - 1] = new TriggerFunc(args[i]).getValue(targetObject, targetParas[i - 1].ParameterType);
                            }
                            targetMethod.Invoke(obj, paras);
                        }
                    }
                }
            }
        }
    }
}