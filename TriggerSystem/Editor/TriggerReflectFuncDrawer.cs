﻿using System;
using System.Linq;

using UnityEditor;
using UnityEngine;

namespace TBSGameCore.TriggerSystem
{
    class TriggerReflectFuncDrawer : TriggerExprSubDrawer<TriggerReflectFunc>
    {
        public TriggerReflectFuncDrawer(TriggerObjectDrawer parent, Transform transform, Type targetType, string targetName) : base(parent, transform, targetType, targetName)
        {
            if (!TriggerLibrary.isAssemblyLoaded(targetObject.GetType().Assembly))
                TriggerLibrary.load(targetObject.GetType().Assembly);
            funcs = TriggerLibrary.getFuncDefines(targetType);
        }
        public override float height
        {
            get
            {
                float height = 16;
                if (isExpanded && paraDrawers != null)
                {
                    for (int i = 0; i < paraDrawers.Length; i++)
                    {
                        height += paraDrawers[i].height;
                    }
                }
                return height;
            }
        }
        TriggerMethodDefine[] funcs { get; set; }
        bool isExpanded { get; set; } = false;
        TriggerTypedExprDrawer[] paraDrawers { get; set; } = null;
        protected override void draw(Rect position, GUIContent label, TriggerReflectFunc expr)
        {
            if (funcs.Length > 0)
            {
                //生成选项
                GUIContent[] options = new GUIContent[funcs.Length + 1];
                for (int i = 0; i < funcs.Length; i++)
                {
                    options[i + 1] = new GUIContent(funcs[i].editorName);
                }
                //获取函数定义，如果没有就给一个默认值
                TriggerMethodDefine define = TriggerLibrary.getMethodDefine(expr.idName);
                if (define == null)
                {
                    define = funcs[0];
                    switchFunc(expr, define);
                }
                options[0] = new GUIContent(expr.desc);
                //计算绘制区域
                Rect foldPosition;
                if (expr.args.Length > 0)
                    foldPosition = new Rect(position.x + position.width, position.y, 16, 16);
                else
                    foldPosition = new Rect(position.x + position.width, position.y, 0, 16);
                Rect popPosition;
                if (label != null)
                {
                    Rect labelPosition = new Rect(position.x, position.y, 64, 16);
                    EditorGUI.LabelField(labelPosition, label);
                    popPosition = new Rect(position.x + labelPosition.width, position.y, position.width - foldPosition.width - labelPosition.width, 16);
                }
                else
                    popPosition = new Rect(position.x, position.y, position.width - foldPosition.width, 16);
                //绘制菜单，点击菜单改变函数
                int index = EditorGUI.Popup(popPosition, 0, options);
                if (index != 0)
                {
                    define = funcs[index - 1];
                    switchFunc(expr, define);
                }
                //绘制参数
                if (expr.args.Length > 0)
                {
                    isExpanded = EditorGUI.Foldout(foldPosition, isExpanded, new GUIContent(""));
                    if (isExpanded)
                    {
                        if (paraDrawers == null || paraDrawers.Length != define.paras.Length || expr.args.Length != define.paras.Length)
                            switchFunc(expr, define);
                        Rect argPosition = new Rect(popPosition.x, popPosition.y + popPosition.height, popPosition.width, 0);
                        for (int i = 0; i < expr.args.Length; i++)
                        {
                            argPosition.height = paraDrawers[i].height;
                            expr.args[i] = paraDrawers[i].draw(argPosition, new GUIContent(define.paras[i].name), expr.args[i]);
                            argPosition.y += argPosition.height;
                        }
                    }
                }
            }
            else
            {
                if (label != null)
                    EditorGUI.LabelField(position, label, new GUIContent("没有可用的函数"));
                else
                    EditorGUI.LabelField(position, new GUIContent("没有可用的函数"));
            }
        }
        private void switchFunc(TriggerReflectFunc expr, TriggerMethodDefine define)
        {
            if (expr.idName != define.idName)
            {
                expr.idName = define.idName;
                if (expr.args != null)
                {
                    for (int i = 0; i < expr.args.Length; i++)
                    {
                        UnityEngine.Object.DestroyImmediate(expr.args[i].gameObject);
                    }
                }
                expr.args = new TriggerExpr[define.paras.Length];
                paraDrawers = new TriggerTypedExprDrawer[define.paras.Length];
                for (int i = 0; i < paraDrawers.Length; i++)
                {
                    paraDrawers[i] = new TriggerTypedExprDrawer(this, expr.transform, define.paras[i].type, define.paras[i].name);
                }
            }
            else
            {
                if (expr.args.Length != define.paras.Length)
                {
                    TriggerExpr[] newArgs = new TriggerExpr[define.paras.Length];
                    for (int i = 0; i < newArgs.Length; i++)
                    {
                        if (i < expr.args.Length)
                            newArgs[i] = expr.args[i];
                        else
                            newArgs[i] = null;
                    }
                    expr.args = newArgs;
                }
                if (paraDrawers == null || paraDrawers.Length != define.paras.Length)
                {
                    paraDrawers = new TriggerTypedExprDrawer[define.paras.Length];
                    for (int i = 0; i < paraDrawers.Length; i++)
                    {
                        paraDrawers[i] = new TriggerTypedExprDrawer(this, expr.transform, define.paras[i].type, define.paras[i].name);
                    }
                }
            }
        }
    }
}