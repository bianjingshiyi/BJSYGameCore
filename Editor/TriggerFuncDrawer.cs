using System;

using UnityEditor;
using UnityEngine;

namespace TBSGameCore
{
    [CustomPropertyDrawer(typeof(TriggerFunc))]
    [CanEditMultipleObjects]
    public class TriggerFuncDrawer : PropertyDrawer
    {
        public static bool disable { get; set; }
        public static Type returnType { get; private set; } = null;
        float _height = 16;
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return disable ? EditorGUI.GetPropertyHeight(property, label, true) : _height;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!disable)
            {
                SerializedProperty prop = property.FindPropertyRelative("value");
                Type returnType = TriggerFuncDrawer.returnType != null ? TriggerFuncDrawer.returnType : getCustomReturnType();
                if (returnType != null)
                {
                    prop.stringValue = FuncStringDrawer.drawTypedFuncString(position, label, prop.stringValue, returnType, property.serializedObject.targetObject, label.text, out _height);
                }
                else
                    EditorGUI.LabelField(position, label, new GUIContent("必须用TypeAttribute指定类型"));
            }
            else
                EditorGUI.PropertyField(position, property, label, true);
        }
        private Type getCustomReturnType()
        {
            var atts = fieldInfo.GetCustomAttributes(typeof(TypeAttribute), false);
            if (atts.Length > 0)
            {
                TypeAttribute att = atts[0] as TypeAttribute;
                return att.type;
            }
            else
                return null;
        }
    }
}