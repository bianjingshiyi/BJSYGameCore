using System;
using System.Linq;

using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TBSGameCore
{
    [CustomPropertyDrawer(typeof(SavableInstanceReference))]
    [CanEditMultipleObjects]
    public class SavableInstanceReferenceDrawer : PropertyDrawer
    {
        private static bool _disable = false;
        public static bool disable
        {
            get { return _disable; }
            set
            {
                _disable = value;
            }
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(SerializedPropertyType.ObjectReference, label);
        }
        public Type refType
        {
            get
            {
                RefTypeAttribute attribute = fieldInfo.GetCustomAttributes(typeof(RefTypeAttribute), true).FirstOrDefault(e => { return e is RefTypeAttribute; }) as RefTypeAttribute;
                if (attribute != null)
                {
                    return attribute.type;
                }
                return typeof(SavableInstance);
            }
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty idProperty = property.FindPropertyRelative("id");
            if (!disable)
            {
                MonoBehaviour behaviour = property.serializedObject.targetObject as MonoBehaviour;
                if (behaviour != null)
                {
                    SavableInstance instance = behaviour.findObject<SaveManager>().getInstanceById(idProperty.intValue);
                    instance = EditorGUI.ObjectField(position, label, instance, typeof(SavableInstance), true) as SavableInstance;
                    if (instance != null)
                        idProperty.intValue = instance.id;
                    else
                        idProperty.intValue = 0;
                }
                else
                {
                    EditorGUI.LabelField(position, label, new GUIContent("物体不在场景中，无法找到对应实例"));
                }
            }
            else
            {
                idProperty.intValue = EditorGUI.IntField(position, label, idProperty.intValue);
            }
        }
    }
}