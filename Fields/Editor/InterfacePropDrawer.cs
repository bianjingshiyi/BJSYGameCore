using System;
using System.Linq;

using UnityEngine;
using UnityEditor;

namespace BJSYGameCore
{
    [CustomPropertyDrawer(typeof(InterfaceAttribute))]
    class InterfacePropDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Type interfaceType = (attribute as InterfaceAttribute).type;
            EditorGUI.BeginProperty(position, label, property);
            property.objectReferenceValue = EditorGUI.ObjectField(position, label, property.objectReferenceValue, fieldInfo.FieldType, true);
            if (property.objectReferenceValue != null && !interfaceType.IsInstanceOfType(property.objectReferenceValue))
            {
                if (property.objectReferenceValue is Component component)
                {
                    Component otherComponent = component.GetComponent(interfaceType);
                    if (otherComponent != null)
                        property.objectReferenceValue = otherComponent;
                    else
                        Debug.LogWarning(property.objectReferenceValue + "不是" + interfaceType.Name + "，其所在物体也不包含其他实现" + interfaceType.Name + "的组件", property.objectReferenceValue);
                }
                else
                {
                    Debug.LogWarning(property.objectReferenceValue + "不是" + interfaceType.Name, property.objectReferenceValue);
                    property.objectReferenceValue = null;
                }
            }
            EditorGUI.EndProperty();
        }
    }
}