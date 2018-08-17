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
        float _height = 16;
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return disable ? EditorGUI.GetPropertyHeight(property, label, true) : _height;
        }
        Dictionary<string, bool> _dicIsExpanded;
        List<FuncStringDrawer.Method> _actionList;
        Dictionary<Type, List<FuncStringDrawer.Method>> _dicFuncOfReturnType;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!disable)
            {
                SerializedProperty prop = property.FindPropertyRelative("value");
                if (_dicIsExpanded == null)
                    _dicIsExpanded = new Dictionary<string, bool>();
                if (_actionList == null)
                    _actionList = new List<FuncStringDrawer.Method>();
                if (_dicFuncOfReturnType == null)
                    _dicFuncOfReturnType = new Dictionary<Type, List<FuncStringDrawer.Method>>();
                prop.stringValue = FuncStringDrawer.drawActionString(position, label, prop.stringValue, property.serializedObject.targetObject, _dicIsExpanded, label.text, _actionList, _dicFuncOfReturnType, out _height);
            }
            else
                EditorGUI.PropertyField(position, property, label, true);
        }
    }
}