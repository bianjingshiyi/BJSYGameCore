using System;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

namespace TBSGameCore
{
    [CustomPropertyDrawer(typeof(TriggerAction))]
    public class TriggerActionDrawer : PropertyDrawer
    {
        public static bool disable { get; set; }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (_drawer == null)
                _drawer = new ActionStringDrawer(property.serializedObject.targetObject);
            return disable ? EditorGUI.GetPropertyHeight(property, label, true) : _drawer.height;
        }
        ActionStringDrawer _drawer = null;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!disable)
            {
                SerializedProperty prop = property.FindPropertyRelative("value");
                if (_drawer == null)
                    _drawer = new ActionStringDrawer(property.serializedObject.targetObject);
                prop.stringValue = _drawer.drawActionString(position, label, prop.stringValue);
            }
            else
                EditorGUI.PropertyField(position, property, label, true);
        }
    }
}