using System;
using System.Linq;
using System.Reflection;

using UnityEngine;
using UnityEditor;

namespace TBSGameCore.TriggerSystem
{
    abstract class TriggerActionSubDrawer : TriggerObjectDrawer
    {
        public TriggerActionSubDrawer(TriggerObjectDrawer parent, Transform transform) : base(parent, transform)
        {
        }
        public abstract void draw(Rect position, GUIContent label, TriggerAction action);
        public static TriggerActionSubDrawer getActionDrawer(Type actionType, TriggerObjectDrawer parent, Transform transform)
        {
            Type targetDrawerType = typeof(TriggerActionSubDrawer<>).MakeGenericType(actionType);
            Type drawerType = typeof(TriggerActionSubDrawer).Assembly.GetTypes().FirstOrDefault(e => { return e.IsSubclassOf(targetDrawerType); });
            if (drawerType != null)
            {
                ConstructorInfo constructor = drawerType.GetConstructor(new Type[] { typeof(TriggerObjectDrawer), typeof(Transform) });
                if (constructor != null)
                {
                    return constructor.Invoke(new object[] { parent, transform }) as TriggerActionSubDrawer;
                }
                else
                {
                    Debug.Log(drawerType.Name + "没有符合规范的构造器！");
                    return new NotSupportActionSubDrawer(parent, transform);
                }
            }
            else
            {
                Debug.LogError(actionType.Name + "没有对应类型的" + nameof(TriggerExprSubDrawer));
                return new NotSupportActionSubDrawer(parent, transform);
            }
        }
    }
    abstract class TriggerActionSubDrawer<T> : TriggerActionSubDrawer where T : TriggerAction
    {
        public TriggerActionSubDrawer(TriggerObjectDrawer parent, Transform transform) : base(parent, transform)
        {
        }
        public override void draw(Rect position, GUIContent label, TriggerAction action)
        {
            if (action is T)
                draw(position, label, action as T);
            else
            {
                if (label != null)
                    EditorGUI.LabelField(position, label, new GUIContent("目标类型不是" + nameof(T)));
                else
                    EditorGUI.LabelField(position, new GUIContent("目标类型不是" + nameof(T)));
            }
        }
        protected abstract void draw(Rect position, GUIContent label, T action);
    }
    class NotSupportActionSubDrawer : TriggerActionSubDrawer
    {
        public NotSupportActionSubDrawer(TriggerObjectDrawer parent, Transform transform) : base(parent, transform)
        {
        }
        public override float height
        {
            get { return 16; }
        }
        public override void draw(Rect position, GUIContent label, TriggerAction action)
        {
            if (label != null)
                EditorGUI.LabelField(position, label, new GUIContent("未找到用于绘制" + (action != null ? action.GetType().Name : "Null") + "的绘制器"));
            else
                EditorGUI.LabelField(position, new GUIContent("未找到用于绘制" + (action != null ? action.GetType().Name : "Null") + "的绘制器"));
        }
    }
}