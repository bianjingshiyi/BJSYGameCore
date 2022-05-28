using System;
using UnityEngine;
using UnityEditor;

namespace BJSYGameCore
{
    [CustomPropertyDrawer(typeof(TypeStringAttribute))]
    class TypeStringPropDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(SerializedPropertyType.String, label);
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
                EditorGUI.LabelField(position, "Field type must be string");
            TypeStringAttribute att = attribute as TypeStringAttribute;

            Rect labelPos = new Rect(position);
            labelPos.width /= 2;
            GUI.Label(labelPos, label);

            Rect btnPos = new Rect(position);
            btnPos.width /= 2;
            btnPos.position += Vector2.right * btnPos.width;
            Type currentType = ReflectionHelper.getType(property.stringValue);
            if (GUI.Button(btnPos, new GUIContent(currentType != null ? currentType.Name : "None")))
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("None"), currentType == null, () =>
                {
                    property.stringValue = string.Empty;
                    property.serializedObject.ApplyModifiedProperties();
                });
                Type[] types = att.baseType == null ? ReflectionHelper.types : ReflectionHelper.getSubclass(att.assemblies, att.baseType);
                foreach (var type in types)
                {
                    string typeName = type.FullName.Replace('.', '/').Replace('+', '/');
                    menu.AddItem(new GUIContent(typeName), type == currentType, () =>
                    {
                        property.stringValue = type.FullName;
                        property.serializedObject.ApplyModifiedProperties();
                    });
                }
                menu.DropDown(btnPos);
            }
        }
    }
}