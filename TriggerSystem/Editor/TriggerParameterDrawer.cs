using System;
using System.Linq;

using UnityEngine;
using UnityEditor;

namespace TBSGameCore.TriggerSystem
{
    public class TriggerParameterDrawer
    {
        public TriggerParameterDrawer(TriggerParameterDefine paraDefine)
        {
            this.paraDefine = paraDefine;
        }
        GenericMenu paraOptionMenu { get; set; } = null;
        TriggerParameterDefine paraDefine { get; set; }
        Rect _rect;
        int type = -1;
        public void draw(TriggerExprComponent expr, ITriggerScope scope)
        {
            GUILayout.BeginHorizontal();
            //值
            if (expr is StringConstComponent)
                (expr as StringConstComponent).value = EditorGUILayout.TextField((expr as StringConstComponent).value);
            else if (expr is NullExprComponent)
                EditorGUILayout.LabelField(new GUIContent(""));
            else if (expr is TriggerVariableComponent)
            {
                TriggerVariableDefine[] variables = expr.scope.getVariables();
                if (variables.Length > 0)
                {
                    GUIContent[] variableOptions = variables.Select(e => { return new GUIContent(e.name); }).ToArray();
                    int index = Array.FindIndex(variables, e => { return e.name == (expr as TriggerVariableComponent).name; });
                    if (index < 0)
                        index = 0;
                    index = EditorGUILayout.Popup(index, variableOptions);
                    (expr as TriggerVariableComponent).setName(variables[index].name);
                }
                else
                {
                    EditorGUILayout.LabelField(new GUIContent("没有可用变量"));
                    (expr as TriggerVariableComponent).setName("空变量");
                }
            }
            else if (expr is ReflectedFuncComponent)
            {
                if (paraOptionMenu == null)
                {
                    paraOptionMenu = new GenericMenu();
                    TriggerExprDefine[] funcs = scope.getFuncs(paraDefine.type);
                    for (int i = 0; i < funcs.Length; i++)
                    {
                        paraOptionMenu.AddItem(new GUIContent(funcs[i].name), false, e =>
                        {
                            GameObject go = expr.gameObject;
                            UnityEngine.Object.DestroyImmediate(expr);
                            (e as TriggerExprDefine).attachTo(go);
                        }, funcs[i]);
                    }
                }
                if (paraOptionMenu.GetItemCount() > 0)
                {
                    if (GUILayout.Button((expr as ReflectedFuncComponent).desc))
                        paraOptionMenu.DropDown(_rect);
                    else if (Event.current.type == EventType.Repaint)
                        _rect = GUILayoutUtility.GetLastRect();
                }
                else
                {
                    EditorGUILayout.LabelField("没有可用的返回目标类型的函数");
                }
            }
            //类型
            if (type < 0)
            {
                if (expr is NullExprComponent)
                    type = 0;
                else if (expr is ConstExprComponent)
                    type = 1;
                else if (expr is TriggerVariableComponent)
                    type = 2;
                else if (expr is ReflectedFuncComponent)
                    type = 3;
                else
                    type = 0;
            }
            int newType = EditorGUILayout.Popup(type, new string[] { "空值", "常量", "变量", "函数" }, GUILayout.Width(48));
            if (newType != type)
            {
                if (expr != null)
                {
                    if (newType == 0)
                    {
                        GameObject go = expr.gameObject;
                        UnityEngine.Object.DestroyImmediate(expr);
                        new NullConstDefine().attachTo(go);
                    }
                    else if (newType == 1)
                    {
                        GameObject go = expr.gameObject;
                        UnityEngine.Object.DestroyImmediate(expr);
                        ConstExprComponent.attachTo(go, paraDefine.type);
                    }
                    else if (newType == 2)
                    {
                        GameObject go = expr.gameObject;
                        UnityEngine.Object.DestroyImmediate(expr);
                        new TriggerVariableDefine("空变量", paraDefine.type).attachTo(go);
                    }
                    else if (newType == 3)
                    {
                        TriggerExprDefine[] funcs = scope.getFuncs(paraDefine.type);
                        if (funcs != null && funcs.Length > 0)
                        {
                            GameObject go = expr.gameObject;
                            UnityEngine.Object.DestroyImmediate(expr);
                            funcs[0].attachTo(go);
                        }
                        else
                            Debug.LogWarning("没有可用的返回目标类型的函数");
                    }
                }
                type = newType;
            }
            GUILayout.EndHorizontal();
        }
    }
}
