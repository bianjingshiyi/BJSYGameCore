using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEditor;

namespace BJSYGameCore.TriggerSystem
{
    abstract class TriggerExprSubDrawer : TriggerExprDrawer
    {
        public TriggerExprSubDrawer(TriggerObjectDrawer parent, Transform transform, Type targetType, string targetName) : base(parent, transform, targetType, targetName)
        {
        }
        public abstract void draw(Rect position, GUIContent label, TriggerExpr expr);
        public static TriggerExprSubDrawer getExprDrawer(Type exprType, TriggerObjectDrawer parent, Transform transform, Type targetType, string targetName)
        {
            Type targetDrawerType = typeof(TriggerExprSubDrawer<>).MakeGenericType(exprType);
            Type drawerType = typeof(TriggerExprSubDrawer).Assembly.GetTypes().FirstOrDefault(e => { return e.IsSubclassOf(targetDrawerType); });
            if (drawerType != null)
            {
                ConstructorInfo constructor = drawerType.GetConstructor(new Type[] { typeof(TriggerObjectDrawer), typeof(Transform), typeof(Type), typeof(string) });
                if (constructor != null)
                {
                    return constructor.Invoke(new object[] { parent, transform, targetType, targetName }) as TriggerExprSubDrawer;
                }
                else
                {
                    Debug.Log(drawerType.Name + "没有符合规范的构造器！");
                    return new NotSupportExprSubDrawer(parent, transform, targetType, targetName);
                }
            }
            else
            {
                Debug.LogError(exprType.Name + "没有对应类型的" + nameof(TriggerExprSubDrawer));
                return new NotSupportExprSubDrawer(parent, transform, targetType, targetName);
            }
        }
    }
    abstract class TriggerExprSubDrawer<T> : TriggerExprSubDrawer where T : TriggerExpr
    {
        public TriggerExprSubDrawer(TriggerObjectDrawer parent, Transform transform, Type targetType, string targetName) : base(parent, transform, targetType, targetName)
        {
        }
        public override void draw(Rect position, GUIContent label, TriggerExpr expr)
        {
            if (expr is T)
                draw(position, label, expr as T);
            else
            {
                if (label != null)
                    EditorGUI.LabelField(position, label, new GUIContent("目标类型不是" + nameof(T)));
                else
                    EditorGUI.LabelField(position, new GUIContent("目标类型不是" + nameof(T)));
            }
        }
        protected abstract void draw(Rect position, GUIContent label, T expr);
    }
    class NotSupportExprSubDrawer : TriggerExprSubDrawer
    {
        public NotSupportExprSubDrawer(TriggerObjectDrawer parent, Transform transform, Type targetType, string targetName) : base(parent, transform, targetType, targetName)
        {
        }
        public override float height { get { return 16; } }
        public override void draw(Rect position, GUIContent label, TriggerExpr expr)
        {
            if (label != null)
                EditorGUI.LabelField(position, label, new GUIContent("未找到用于绘制" + (expr != null ? expr.GetType().Name : "Null") + "的绘制器"));
            else
                EditorGUI.LabelField(position, new GUIContent("未找到用于绘制" + (expr != null ? expr.GetType().Name : "Null") + "的绘制器"));
        }
    }
}