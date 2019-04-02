using System;
using System.Reflection;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

namespace BJSYGameCore
{
    public class FuncStringDrawer : AbstractFuncStringDrawer
    {
        Dictionary<Type, List<ReflectedMethod>> _dicFuncOfReturnType = new Dictionary<Type, List<ReflectedMethod>>();
        TypedFuncStringDrawer[] _argDrawers = null;
        public FuncStringDrawer(UnityEngine.Object targetObject) : base(targetObject)
        {
            foreach (Type type in targetObject.GetType().Assembly.GetTypes())
            {
                foreach (MethodInfo method in type.GetMethods())
                {
                    FuncAttribute att = method.GetCustomAttribute<FuncAttribute>();
                    if (att != null)
                    {
                        if (!_dicFuncOfReturnType.ContainsKey(method.ReturnType))
                            _dicFuncOfReturnType.Add(method.ReturnType, new List<ReflectedMethod>());
                        _dicFuncOfReturnType[method.ReturnType].Add(new ReflectedMethod(type, method, att.funcName, att.desc));
                    }
                }
            }
        }
        public FuncStringDrawer(TriggerStringDrawer parent) : base(parent)
        {
            foreach (Type type in targetObject.GetType().Assembly.GetTypes())
            {
                foreach (MethodInfo method in type.GetMethods())
                {
                    FuncAttribute att = method.GetCustomAttribute<FuncAttribute>();
                    if (att != null)
                    {
                        if (!_dicFuncOfReturnType.ContainsKey(method.ReturnType))
                            _dicFuncOfReturnType.Add(method.ReturnType, new List<ReflectedMethod>());
                        _dicFuncOfReturnType[method.ReturnType].Add(new ReflectedMethod(type, method, att.funcName, att.desc));
                    }
                }
            }
        }
        public override float height
        {
            get
            {
                if (isExpanded)
                {
                    if (_argDrawers != null)
                    {
                        float height = 16;
                        for (int i = 0; i < _argDrawers.Length; i++)
                        {
                            height += _argDrawers[i].height;
                        }
                        return height;
                    }
                    else
                        return 16;
                }
                else
                    return 16;
            }
        }
        public override string draw(Rect position, GUIContent label, string value, Type returnType)
        {
            //Name(0123,1VarName,2FuncName(0234))，分解字符串
            string className = null;
            string methodName = null;
            string[] args = null;
            TriggerParser.parseFunc(value, out className, out methodName, out args);
            ReflectedMethod[] funcs = getFuncOfType(returnType, targetObject.GetType().Assembly);
            if (funcs.Length > 0)
            {
                //转换方法为索引
                int index = Array.FindIndex(funcs, e => { return e.objType.FullName == className && e.info.Name == methodName; });
                if (index < 0)
                    index = 0;
                ReflectedMethod func = funcs[index];
                //GUI
                GenericMenu menu = new GenericMenu();
                for (int i = 0; i < funcs.Length; i++)
                {
                    menu.AddItem(new GUIContent(funcs[i].displayName), false, e =>
                    {
                        //更换方法
                        func = (e as ReflectedMethod);
                        args = new string[(e as ReflectedMethod).info.GetParameters().Length];
                    }, funcs[i]);
                }
                Rect valuePosition = new Rect(position.x, position.y, position.width - 16, 16);
                if (label != null)
                {
                    Rect labelPosition = new Rect(valuePosition.x, valuePosition.y, valuePosition.width / 2, valuePosition.height);
                    Rect buttonPosition = new Rect(valuePosition.x + labelPosition.width, valuePosition.y, valuePosition.width - labelPosition.width, valuePosition.height);
                    EditorGUI.LabelField(labelPosition, label);
                    if (GUI.Button(buttonPosition, new GUIContent(func.displayDesc)))
                        menu.DropDown(buttonPosition);
                }
                else
                {
                    if (GUI.Button(valuePosition, new GUIContent(func.displayDesc)))
                        menu.DropDown(valuePosition);
                }
                //绘制参数
                Parameter[] paras = func.getParameters();
                if (args == null)
                    args = new string[paras.Length];
                else if (args.Length != paras.Length)
                {
                    string[] newArgs = new string[paras.Length];
                    for (int i = 0; i < newArgs.Length; i++)
                    {
                        if (i < args.Length)
                            newArgs[i] = args[i];
                    }
                    args = newArgs;
                }
                if (paras.Length > 0)
                {
                    Rect foldPosition = new Rect(position.x + position.width, position.y, 16, 16);
                    isExpanded = EditorGUI.Foldout(foldPosition, isExpanded, "");
                    if (isExpanded)
                    {
                        Rect argPosition = new Rect(position.x + 16, position.y + 16, position.width, 16);
                        if (_argDrawers == null || _argDrawers.Length != paras.Length)
                        {
                            _argDrawers = new TypedFuncStringDrawer[paras.Length];
                            for (int i = 0; i < paras.Length; i++)
                                _argDrawers[i] = new TypedFuncStringDrawer(targetObject);
                        }
                        for (int i = 0; i < paras.Length; i++)
                        {
                            args[i] = _argDrawers[i].draw(argPosition, new GUIContent(paras[i].name), args[i], paras[i].type);
                            argPosition.y += _argDrawers[i].height;
                        }
                    }
                    else
                    {
                        for (int i = 0; i < paras.Length; i++)
                        {
                            if (string.IsNullOrEmpty(args[i]))
                                args[i] = "0";
                        }
                    }
                }
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
                return null;
            }
        }
        private ReflectedMethod[] getFuncOfType(Type returnType, Assembly assembly)
        {
            if (_dicFuncOfReturnType.ContainsKey(returnType))
                return _dicFuncOfReturnType[returnType].ToArray();
            else
                return new ReflectedMethod[0];
        }
    }
}