using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

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
    [CustomPropertyDrawer(typeof(AnimNameAttribute))]
    class AnimNamePropDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(SerializedPropertyType.String, label);
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
                EditorGUI.LabelField(position, "Field type must be string");
            if (property.serializedObject.targetObject is Component component)
            {
                Animator animator = component.GetComponentInChildren<Animator>();
                if (animator != null)
                {
                    List<string> animNameList = new List<string>((animator.runtimeAnimatorController as AnimatorController).layers[0].stateMachine.states.Select(s => s.state.name));
                    if (animNameList.Count > 0)
                    {
                        EditorGUI.BeginProperty(position, label, property);
                        int index = animNameList.IndexOf(property.stringValue);
                        if (index < 0)
                        {
                            animNameList.Insert(0, "Unknow AnimName");
                            index = 0;
                        }
                        index = EditorGUI.Popup(position, label, index, animNameList.Select(s => new GUIContent(s)).ToArray());
                        property.stringValue = animNameList[index];
                        EditorGUI.EndProperty();
                    }
                    else
                        EditorGUI.LabelField(position, "No AnimState");
                }
                else
                    EditorGUI.LabelField(position, "Can't find Animator");
            }
            else
                EditorGUI.LabelField(position, "SerializedObject must be Component");
        }
    }
}