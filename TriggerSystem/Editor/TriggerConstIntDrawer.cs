using System;
using UnityEditor;
using UnityEngine;

namespace BJSYGameCore.TriggerSystem
{
    class TriggerConstIntDrawer : TriggerExprSubDrawer<TriggerConstInt>
    {
        public TriggerConstIntDrawer(TriggerObjectDrawer parent, Transform transform, Type targetType, string targetName) : base(parent, transform, targetType, targetName)
        {
        }
        public override float height
        {
            get { return 16; }
        }
        protected override void draw(Rect position, GUIContent label, TriggerConstInt expr)
        {
            Rect valuePosition;
            if (label != null)
            {
                Rect labelPosition = new Rect(position.x, position.y, 64, 16);
                EditorGUI.LabelField(labelPosition, label);
                valuePosition = new Rect(position.x + labelPosition.width, position.y, position.width - labelPosition.width, 16);
            }
            else
                valuePosition = position;
            expr.value = EditorGUI.IntField(valuePosition, expr.value);
        }
    }
    abstract class TriggerConstDrawer<T> : TriggerExprSubDrawer<T> where T : TriggerConst
    {
        public TriggerConstDrawer(TriggerObjectDrawer parent, Transform transform, Type targetType, string targetName) : base(parent, transform, targetType, targetName)
        {
        }
        protected override void draw(Rect position, GUIContent label, T expr)
        {
            Rect valuePosition;
            if (label != null)
            {
                Rect labelPosition = new Rect(position.x, position.y, 64, 16);
                EditorGUI.LabelField(labelPosition, label);
                valuePosition = new Rect(position.x + labelPosition.width, position.y, position.width - labelPosition.width, height);
            }
            else
                valuePosition = position;
            drawField(valuePosition, expr);
        }
        protected abstract void drawField(Rect position, T expr);
    }
    class TriggerConstStringDrawer : TriggerConstDrawer<TriggerConstString>
    {
        public TriggerConstStringDrawer(TriggerObjectDrawer parent, Transform transform, Type targetType, string targetName) : base(parent, transform, targetType, targetName)
        {
        }
        public override float height
        {
            get { return 16; }
        }
        protected override void drawField(Rect position, TriggerConstString expr)
        {
            expr.value = EditorGUI.TextField(position, expr.value);
        }
    }
}