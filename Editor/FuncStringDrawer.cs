using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

namespace TBSGameCore
{
    public static class FuncStringDrawer
    {
        public static string drawTypedFuncString(Rect position, GUIContent label, string value, Type returnType, UnityEngine.Object targetObject, Dictionary<string, bool> dicIsExpanded, string path, Dictionary<Type, List<Method>> dicFuncOfReturnType, out float height)
        {
            //切割字符串得到类型与真正的值
            int type = 0;
            string realValue = null;
            if (value != null)
            {
                if (value.Length > 1)
                {
                    int.TryParse(value.Substring(0, 1), out type);
                    realValue = value.Substring(1, value.Length - 1);
                }
                else if (value.Length == 1)
                {
                    int.TryParse(value, out type);
                }
            }
            else
            {
                type = 0;
                realValue = null;
            }
            //绘制类型GUI，改变类型
            Rect typePosition = new Rect(position);
            typePosition.x = position.x + (position.width - 20);
            typePosition.width = 20;
            typePosition.height = 16;
            switch (type)
            {
                case 2:
                    if (GUI.Button(typePosition, "F"))
                    {
                        type = 0;
                        realValue = null;
                    }
                    break;
                case 1:
                    if (GUI.Button(typePosition, "V"))
                    {
                        type = 2;
                        realValue = null;
                    }
                    break;
                default:
                    if (GUI.Button(typePosition, "C"))
                    {
                        type = 1;
                        realValue = null;
                    }
                    break;
            }
            //绘制值GUI，改变值
            Rect valuePosition = new Rect(position);
            valuePosition.width -= 20;
            valuePosition.height = 16;
            if (type == 2)
                realValue = drawFuncString(valuePosition, label, realValue, returnType, targetObject, dicIsExpanded, path, dicFuncOfReturnType, out height);
            else if (type == 1)
                realValue = drawVariableString(valuePosition, label, realValue, returnType, targetObject, out height);
            else
                realValue = drawConstString(valuePosition, label, realValue, returnType, targetObject, out height);
            //重新组合类型和真值得到值
            return type.ToString() + realValue;
        }
        private static string drawConstString(Rect position, GUIContent label, string value, Type returnType, UnityEngine.Object targetObject, out float height)
        {
            if (returnType == typeof(int))
            {
                height = EditorGUI.GetPropertyHeight(SerializedPropertyType.Integer, label);
                int intValue = TriggerParser.parseInt(value);
                intValue = EditorGUI.IntField(position, label, intValue);
                return intValue.ToString();
            }
            else if (returnType == typeof(float))
            {
                height = EditorGUI.GetPropertyHeight(SerializedPropertyType.Float, label);
                float floatValue = TriggerParser.parseFloat(value);
                floatValue = EditorGUI.FloatField(position, label, floatValue);
                return floatValue.ToString();
            }
            else if (returnType == typeof(bool))
            {
                height = EditorGUI.GetPropertyHeight(SerializedPropertyType.Boolean, label);
                bool boolValue = TriggerParser.parseBool(value);
                boolValue = EditorGUI.Toggle(position, label, boolValue);
                return boolValue.ToString();
            }
            else if (returnType == typeof(string))
            {
                height = EditorGUI.GetPropertyHeight(SerializedPropertyType.String, label);
                return EditorGUI.TextField(position, label, value);
            }
            else if (returnType.IsSubclassOf(typeof(Component)))
            {
                height = EditorGUI.GetPropertyHeight(SerializedPropertyType.ObjectReference, label);
                InstanceReference instanceValue = TriggerParser.parseInstanceReference(value);
                Component component;
                if (targetObject is Component)
                    component = instanceValue.findInstanceIn((targetObject as Component).gameObject.scene, returnType);
                else
                    component = null;
                component = EditorGUI.ObjectField(position, label, component, returnType, true) as Component;
                instanceValue = new InstanceReference(component);
                return instanceValue.ToString();
            }
            else
            {
                EditorGUI.LabelField(position, label, new GUIContent("暂时不支持该类型常量"));
                height = 16;
                return null;
            }
        }
        private static string drawVariableString(Rect position, GUIContent label, string value, Type returnType, UnityEngine.Object targetObject, out float height)
        {
            if (targetObject is Component)
            {
                //在场景中
                VariableManager manager = (targetObject as Component).gameObject.scene.findInstance<VariableManager>();
                if (manager != null)
                {
                    string[] varNames = manager.getVarNamesOfType(returnType);//获取可选值
                    if (varNames.Length > 0)
                    {
                        GUIContent[] options = varNames.Select(e => { return new GUIContent(e); }).ToArray();
                        int index = Array.IndexOf(varNames, value);//获取索引
                        if (index < 0)
                            index = 0;
                        index = EditorGUI.Popup(position, label, index, options);//GUI
                        height = 16;
                        return varNames[index];
                    }
                    else
                    {
                        EditorGUI.LabelField(position, label, new GUIContent("没有对应类型的变量"));
                        height = 16;
                        return null;
                    }
                }
                else
                {
                    EditorGUI.LabelField(position, label, new GUIContent("场景中不存在" + nameof(VariableManager)));
                    height = 16;
                    return null;
                }
            }
            else
            {
                //不在场景中
                EditorGUI.LabelField(position, label, new GUIContent("不在场景中"));
                height = 16;
                return null;
            }
        }
        public static string drawActionString(Rect position, GUIContent label, string value, UnityEngine.Object targetObject, Dictionary<string, bool> dicIsExpanded, string path, List<Method> actionList, Dictionary<Type, List<Method>> dicFuncOfReturnType, out float height)
        {
            string className;
            string methodName;
            string[] args;
            TriggerParser.parseAction(value, out className, out methodName, out args);
            Method[] actions = getActions(targetObject.GetType().Assembly, actionList);
            if (actions.Length > 0)
            {
                //检查与获取值
                int index = Array.FindIndex(actions, e => { return e.objType.FullName == className && e.info.Name == methodName; });
                if (index < 0)
                    index = 0;
                Method action = actions[index];
                //绘制GUI
                Rect valuePosition = new Rect(position);
                valuePosition.width = position.width - 16;
                valuePosition.height = 16;
                Rect labelPosition = new Rect(valuePosition);
                labelPosition.width = valuePosition.width / 2;
                Rect buttonPosition = new Rect(valuePosition);
                buttonPosition.width = valuePosition.width / 2;
                buttonPosition.x += valuePosition.width / 2;
                GenericMenu menu = new GenericMenu();
                for (int i = 0; i < actions.Length; i++)
                {
                    menu.AddItem(new GUIContent(actions[i].name), false, e =>
                    {
                        action = (e as Method);
                        index = Array.IndexOf(actions, e);
                    }, actions[i]);
                }
                EditorGUI.LabelField(labelPosition, label);
                if (GUI.Button(buttonPosition, new GUIContent(action.desc)))
                    menu.DropDown(buttonPosition);
                height = 16;
                //绘制参数
                Parameter[] paras = action.getParameters();
                if (args.Length != paras.Length)
                    args = new string[paras.Length];
                if (paras.Length > 0)
                {
                    Rect foldPosition = new Rect(position);
                    foldPosition.x += position.width;
                    foldPosition.width = 16;
                    foldPosition.height = 16;
                    bool isExpanded = dicIsExpanded.ContainsKey(path) ? dicIsExpanded[path] : false;
                    isExpanded = EditorGUI.Foldout(foldPosition, isExpanded, "");
                    dicIsExpanded[path] = isExpanded;
                    if (isExpanded)
                    {
                        Rect argPosition = new Rect(position);
                        argPosition.x = position.x + 16;
                        argPosition.y += 16;
                        argPosition.width = position.width - 16;
                        argPosition.height = 16;
                        float argHeight;
                        for (int i = 0; i < paras.Length; i++)
                        {
                            args[i] = drawTypedFuncString(argPosition, new GUIContent(paras[i].name), args[i], paras[i].type, targetObject, dicIsExpanded, path + '/' + paras[i].name, dicFuncOfReturnType, out argHeight);
                            argPosition.y += argHeight;
                            height += argHeight;
                        }
                    }
                }
                //返回值
                value = action.objType.FullName + '.' + action.info.Name + '(';
                for (int i = 0; i < args.Length; i++)
                {
                    value += args[i];
                    if (i < args.Length - 1)
                        value += ',';
                }
                value += ')';
                return value;
            }
            else
            {
                EditorGUI.LabelField(position, label, new GUIContent("没有动作"));
                height = 16;
                return null;
            }
        }
        private static Method[] getActions(Assembly assembly, List<Method> actionList)
        {
            if (actionList.Count == 0)
            {
                foreach (Type type in assembly.GetTypes())
                {
                    foreach (MethodInfo method in type.GetMethods())
                    {
                        ActionAttribute att = method.GetCustomAttribute<ActionAttribute>();
                        if (att != null)
                        {
                            actionList.Add(new Method(type, method, att.actionName, att.desc));
                        }
                    }
                }
            }
            return actionList.ToArray();
        }
        private static string drawFuncString(Rect position, GUIContent label, string value, Type returnType, UnityEngine.Object targetObject, Dictionary<string, bool> dicIsExpanded, string path, Dictionary<Type, List<Method>> dicFuncOfReturnType, out float height)
        {
            //Name(0123,1VarName,2FuncName(0234))，分解字符串
            if (dicIsExpanded == null)
                dicIsExpanded = new Dictionary<string, bool>();
            string className;
            string methodName;
            string[] args;
            TriggerParser.parseFunc(value, out className, out methodName, out args);
            Method[] funcs = getFuncOfType(returnType, targetObject.GetType().Assembly, dicFuncOfReturnType);
            if (funcs.Length > 0)
            {
                //转换方法为索引
                int index = Array.FindIndex(funcs, e => { return e.objType.FullName == className && e.info.Name == methodName; });
                if (index < 0)
                {
                    index = 0;
                    args = new string[funcs[index].info.GetParameters().Length];
                }
                Method func = funcs[index];
                Rect valuePosition = new Rect(position);
                valuePosition.width = position.width - 16;
                //GUI
                GenericMenu menu = new GenericMenu();
                for (int i = 0; i < funcs.Length; i++)
                {
                    menu.AddItem(new GUIContent(funcs[i].name), false, e =>
                    {
                        //更换方法
                        func = (e as Method);
                        args = new string[(e as Method).info.GetParameters().Length];
                    }, funcs[i]);
                }
                Rect labelPosition = new Rect(valuePosition);
                labelPosition.width = valuePosition.width / 2;
                Rect buttonPosition = new Rect(valuePosition);
                buttonPosition.x += valuePosition.width / 2;
                buttonPosition.width = valuePosition.width / 2;
                EditorGUI.LabelField(labelPosition, label);
                if (GUI.Button(buttonPosition, new GUIContent(func.desc)))
                    menu.DropDown(buttonPosition);
                //绘制参数
                Parameter[] paras = func.getParameters();
                if (args.Length != paras.Length)
                {
                    string[] newArgs = new string[paras.Length];
                    for (int i = 0; i < newArgs.Length; i++)
                    {
                        if (i < args.Length)
                            newArgs[i] = args[i];
                    }
                    args = newArgs;
                }
                if (!string.IsNullOrEmpty(path) && paras.Length > 0)
                {
                    Rect foldPosition = new Rect(position);
                    foldPosition.x = position.x + (position.width);
                    foldPosition.width = 16;
                    bool isExpanded = dicIsExpanded.ContainsKey(path) ? dicIsExpanded[path] : false;
                    isExpanded = EditorGUI.Foldout(foldPosition, isExpanded, "");
                    dicIsExpanded[path] = isExpanded;
                    if (isExpanded)
                    {
                        height = 16;
                        Rect argPosition = new Rect(position);
                        argPosition.x = position.x + 16;
                        float argHeight = 16;
                        argPosition.y += argHeight;
                        for (int i = 0; i < paras.Length; i++)
                        {
                            args[i] = drawTypedFuncString(argPosition, new GUIContent(paras[i].name), args[i], paras[i].type, targetObject, dicIsExpanded, path + '/' + paras[i].name, dicFuncOfReturnType, out argHeight);
                            argPosition.y += argHeight;
                            height += argHeight;
                        }
                    }
                    else
                        height = 16;
                }
                else
                    height = 16;
                //拼接字符串
                value = func.objType.FullName + '.' + func.info.Name + '(';
                for (int i = 0; i < args.Length; i++)
                {
                    value += args[i];
                    if (i < args.Length - 1)
                        value += ',';
                }
                value += ')';
                return value;
            }
            else
            {
                EditorGUI.LabelField(position, label, new GUIContent("没有该返回类型的函数"));
                height = 16;
                return null;
            }
        }
        private static Method[] getFuncOfType(Type returnType, Assembly assembly, Dictionary<Type, List<Method>> dicFuncOfReturnType)
        {
            if (dicFuncOfReturnType.Count == 0)
            {
                foreach (Type type in assembly.GetTypes())
                {
                    foreach (MethodInfo method in type.GetMethods())
                    {
                        FuncAttribute att = method.GetCustomAttribute<FuncAttribute>();
                        if (att != null)
                        {
                            if (!dicFuncOfReturnType.ContainsKey(method.ReturnType))
                                dicFuncOfReturnType.Add(method.ReturnType, new List<Method>());
                            dicFuncOfReturnType[method.ReturnType].Add(new Method(type, method, att.funcName, att.desc));
                        }
                    }
                }
            }
            if (dicFuncOfReturnType.ContainsKey(returnType))
                return dicFuncOfReturnType[returnType].ToArray();
            else
                return new Method[0];
        }
        public class Method
        {
            public Type objType { get; private set; }
            public string name { get; private set; }
            public string desc { get; private set; }
            public MethodInfo info { get; private set; }
            public Method(Type objType, MethodInfo info, string funcName, string desc)
            {
                this.objType = objType;
                this.info = info;
                this.name = funcName;
                this.desc = desc;
            }
            public Parameter[] getParameters()
            {
                List<Parameter> paraList = new List<Parameter>();
                if (!info.IsStatic)
                {
                    paraList.Add(new Parameter(objType, "object"));
                }
                foreach (ParameterInfo p in info.GetParameters())
                {
                    paraList.Add(new Parameter(p.ParameterType, p.Name));
                }
                return paraList.ToArray();
            }
        }
        public class Parameter
        {
            public Type type { get; private set; }
            public string name { get; private set; }
            public Parameter(Type type, string name)
            {
                this.type = type;
                this.name = name;
            }
        }
    }
}