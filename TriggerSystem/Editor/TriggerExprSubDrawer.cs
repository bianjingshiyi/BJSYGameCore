using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEditor;

namespace TBSGameCore.TriggerSystem
{
    abstract class TriggerExprSubDrawer : TriggerObjectDrawer
    {
        public TriggerExprSubDrawer(TriggerObjectDrawer parent, Transform transform) : base(parent, transform)
        {
        }
        public abstract void draw(Rect position, GUIContent label, TriggerExpr expr);
        public static TriggerExprSubDrawer getExprDrawer(Type targetType, TriggerObjectDrawer parent, Transform transform)
        {
            Type targetDrawerType = typeof(TriggerExprSubDrawer<>).MakeGenericType(targetType);
            Type drawerType = typeof(TriggerExprSubDrawer).Assembly.GetTypes().FirstOrDefault(e => { return e.IsSubclassOf(targetDrawerType); });
            if (drawerType != null)
            {
                ConstructorInfo constructor = drawerType.GetConstructor(new Type[] { typeof(TriggerObjectDrawer), typeof(Transform) });
                if (constructor != null)
                {
                    return constructor.Invoke(new object[] { parent, transform }) as TriggerExprSubDrawer;
                }
                else
                {
                    Debug.Log(drawerType.Name + "没有符合规范的构造器！");
                    return new NotSupportSubDrawer(parent, transform);
                }
            }
            else
            {
                Debug.LogError(targetType.Name + "没有对应类型的" + nameof(TriggerExprSubDrawer));
                return new NotSupportSubDrawer(parent, transform);
            }
        }
    }
    abstract class TriggerExprSubDrawer<T> : TriggerExprSubDrawer where T : TriggerExpr
    {
        public TriggerExprSubDrawer(TriggerObjectDrawer parent, Transform transform) : base(parent, transform)
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
    class NotSupportSubDrawer : TriggerExprSubDrawer
    {
        public NotSupportSubDrawer(TriggerObjectDrawer parent, Transform transform) : base(parent, transform)
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