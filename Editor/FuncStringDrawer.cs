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
        public static string drawTypedFuncString(Rect position, GUIContent label, string value, Type returnType, UnityEngine.Object targetObject, string path, out float height)
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
                    if (GUI.Button(typePosition, "M"))
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
                realValue = drawFuncString(valuePosition, label, realValue, returnType, targetObject, path, out height);
            else if (type == 1)
                realValue = drawVariableString(valuePosition, label, realValue, returnType, targetObject, out height);
            else
                realValue = drawConstString(valuePosition, label, realValue, returnType, out height);
            //重新组合类型和真值得到值
            return type.ToString() + realValue;
        }
        private static string drawConstString(Rect position, GUIContent label, string value, Type returnType, out float height)
        {
            if (returnType == typeof(int))
            {
                height = EditorGUI.GetPropertyHeight(SerializedPropertyType.Integer, label);
                int intValue;
                if (!int.TryParse(value, out intValue))
                    intValue = 0;
                intValue = EditorGUI.IntField(position, label, intValue);
                return intValue.ToString();
            }
            else if (returnType == typeof(float))
            {
                height = EditorGUI.GetPropertyHeight(SerializedPropertyType.Float, label);
                float floatValue;
                if (!float.TryParse(value, out floatValue))
                    floatValue = 0;
                floatValue = EditorGUI.FloatField(position, label, floatValue);
                return floatValue.ToString();
            }
            else if (returnType == typeof(bool))
            {
                height = EditorGUI.GetPropertyHeight(SerializedPropertyType.Boolean, label);
                bool boolValue;
                if (!bool.TryParse(value, out boolValue))
                    boolValue = false;
                boolValue = EditorGUI.Toggle(position, label, boolValue);
                return boolValue.ToString();
            }
            else if (returnType == typeof(string))
            {
                height = EditorGUI.GetPropertyHeight(SerializedPropertyType.String, label);
                return EditorGUI.TextField(position, label, value);
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
        static Dictionary<string, bool> _dicIsExpanded;
        private static string drawFuncString(Rect position, GUIContent label, string value, Type returnType, UnityEngine.Object targetObject, string path, out float height)
        {
            //Name(0123,1VarName,2FuncName(0234))，分解字符串
            if (_dicIsExpanded == null)
                _dicIsExpanded = new Dictionary<string, bool>();
            string funcName;
            string[] args;
            parseFunc(value, out funcName, out args);
            Func[] funcs = getFuncOfType(returnType, targetObject.GetType().Assembly);
            if (funcs.Length > 0)
            {
                //转换方法为索引
                int index = Array.FindIndex(funcs, e => { return e.funcName == funcName; });
                if (index < 0)
                    index = 0;
                funcName = funcs[index].funcName;
                Func func = funcs[index];
                Rect valuePosition = new Rect(position);
                valuePosition.width = position.width - 16;
                //GUI
                GenericMenu menu = new GenericMenu();
                for (int i = 0; i < funcs.Length; i++)
                {
                    menu.AddItem(new GUIContent(funcs[i].funcName), false, e =>
                    {
                        //更换方法
                        Debug.Log("更换方法");
                        funcName = (e as Func).funcName;
                        func = (e as Func);
                        args = new string[0];
                    }, funcs[i]);
                }
                if (label != null)
                {
                    Rect labelPosition = new Rect(valuePosition);
                    labelPosition.width = valuePosition.width / 2;
                    Rect buttonPosition = new Rect(valuePosition);
                    buttonPosition.x += valuePosition.width / 2;
                    buttonPosition.width = valuePosition.width / 2;
                    EditorGUI.LabelField(labelPosition, label);
                    if (GUI.Button(buttonPosition, new GUIContent(func.desc)))
                    {
                        menu.DropDown(buttonPosition);
                    }
                }
                else
                {
                    if (GUI.Button(valuePosition, new GUIContent(func.desc)))
                    {
                        menu.DropDown(valuePosition);
                    }
                }
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
                //绘制参数
                if (!string.IsNullOrEmpty(path) && paras.Length > 0)
                {
                    Rect foldPosition = new Rect(position);
                    foldPosition.x = position.x + (position.width);
                    foldPosition.width = 16;
                    bool isExpanded = _dicIsExpanded.ContainsKey(path) ? _dicIsExpanded[path] : false;
                    isExpanded = EditorGUI.Foldout(foldPosition, isExpanded, "");
                    _dicIsExpanded[path] = isExpanded;
                    if (isExpanded)
                    {
                        height = 16;
                        Rect argPosition = new Rect(position);
                        argPosition.x = position.x + 16;
                        float argHeight = 16;
                        argPosition.y += argHeight;
                        for (int i = 0; i < paras.Length; i++)
                        {
                            args[i] = drawTypedFuncString(argPosition, new GUIContent(paras[i].name), args[i], paras[i].type, targetObject, path + '/' + paras[i].name, out argHeight);
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
                value = funcName + '(';
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
                if (label != null)
                    EditorGUI.LabelField(position, label, new GUIContent("没有该返回类型的函数"));
                else
                    EditorGUI.LabelField(position, new GUIContent("没有该返回类型的函数"));
                height = 16;
                return null;
            }
        }
        private static void parseFunc(string value, out string name, out string[] args)
        {
            name = null;
            args = new string[0];
            if (value == null)
                return;
            int b = -1;
            List<string> argList = new List<string>();
            int startIndex = 0;
            for (int i = 0; i < value.Length; i++)
            {
                if (value[i] == '(')
                {
                    if (b == -1)
                    {
                        name = value.Substring(0, i);
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
        static Dictionary<Type, List<Func>> _dicFuncOfReturnType = null;
        private static Func[] getFuncOfType(Type returnType, Assembly assembly)
        {
            if (_dicFuncOfReturnType == null)
            {
                _dicFuncOfReturnType = new Dictionary<Type, List<Func>>();
                foreach (Type type in assembly.GetTypes())
                {
                    foreach (MethodInfo method in type.GetMethods())
                    {
                        FuncAttribute att = method.GetCustomAttribute<FuncAttribute>();
                        if (att != null)
                        {
                            if (!_dicFuncOfReturnType.ContainsKey(method.ReturnType))
                                _dicFuncOfReturnType.Add(method.ReturnType, new List<Func>());
                            _dicFuncOfReturnType[method.ReturnType].Add(new Func(type, method, att.funcName, att.description));
                        }
                    }
                }
            }
            if (_dicFuncOfReturnType.ContainsKey(returnType))
                return _dicFuncOfReturnType[returnType].ToArray();
            else
                return new Func[0];
        }
        class Func
        {
            public Type objType { get; private set; }
            public string funcName { get; private set; }
            public string desc { get; private set; }
            public MethodInfo info { get; private set; }
            public Func(Type objType, MethodInfo info, string funcName, string desc)
            {
                this.objType = objType;
                this.info = info;
                this.funcName = funcName;
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
        class Parameter
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