using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace BJSYGameCore
{
    [CustomPropertyDrawer(typeof(CopyPasteAttribute))]
    class CopyPastePropDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
        static object value { get; set; } = null;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position = new Rect(position.x, position.y, position.width - 100, position.height);
            EditorGUI.PropertyField(position, property, label, true);
            if (GUI.Button(new Rect(position.x + position.width, position.y, 50, position.height), "复制"))
            {
                value = fieldInfo.GetValue(property.serializedObject.targetObject);
            }
            if (GUI.Button(new Rect(position.x + position.width + 50, position.y, 50, position.height), "粘贴"))
            {
                fieldInfo.SetValue(property.serializedObject.targetObject, value);
            }
        }
    }
}