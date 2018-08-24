using System;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

namespace TBSGameCore
{
    [CustomPropertyDrawer(typeof(TriggerFunc))]
    public class TriggerFuncDrawer : PropertyDrawer
    {
        public static bool disable { get; set; }
        public static Type returnType { get; private set; } = null;
        float _height = 16;
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            Type returnType = TriggerFuncDrawer.returnType != null ? TriggerFuncDrawer.returnType : getCustomReturnType();
            if (_drawer == null)
                _drawer = new TypedFuncStringDrawer(property.serializedObject.targetObject);
            return disable ? EditorGUI.GetPropertyHeight(property, label, true) : _drawer.height;
        }
        TypedFuncStringDrawer _drawer = null;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!disable)
            {
                SerializedProperty prop = property.FindPropertyRelative("value");
                Type returnType = TriggerFuncDrawer.returnType != null ? TriggerFuncDrawer.returnType : getCustomReturnType();
                if (returnType != null)
                {
                    if (_drawer == null)
                        _drawer = new TypedFuncStringDrawer(property.serializedObject.targetObject);
                    EditorGUI.BeginProperty(position, label, property);
                    prop.stringValue = _drawer.draw(position, label, prop.stringValue, returnType);
                    EditorGUI.EndProperty();
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