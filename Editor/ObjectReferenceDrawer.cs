using System;
using System.Linq;

using UnityEditor;
using UnityEngine;

namespace TBSGameCore
{
    [CustomPropertyDrawer(typeof(ObjectReference), true)]
    [CanEditMultipleObjects]
    public class ObjectReferenceDrawer : PropertyDrawer
    {
        public static bool disable
        {
            get; set;
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
        Type _refType = null;
        void init()
        {
            RefTypeAttribute att = fieldInfo.GetCustomAttributes(typeof(RefTypeAttribute), false).FirstOrDefault(e => { return e is RefTypeAttribute; }) as RefTypeAttribute;
            if (att != null)
            {
                _refType = att.type;
            }
            else
            {
                _refType = typeof(UnityEngine.Object);
            }
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (_refType == null)
            {
                init();
            }
            if (property.serializedObject.targetObject is Component && !disable)
            {
                SerializedProperty idProp = property.FindPropertyRelative("id");
                //先获取值
                UnityEngine.Object obj = null;
                Type keeperType = typeof(IObjectKeeper<>).MakeGenericType(_refType);
                Component objectKeeper = (property.serializedObject.targetObject as Component).gameObject.scene.findInstance(keeperType);
                if (objectKeeper != null)
                    obj = keeperType.GetMethod("getObjectById").Invoke(objectKeeper, new object[] { idProp.intValue }) as UnityEngine.Object;
                else
                {
                    GUI.Box(position, "场景里没有对应的ObjectKeeper");
                    return;
                }
                //GUI
                obj = EditorGUI.ObjectField(position, label, obj, _refType, true);
                //再设置值
                if (obj != null)
                {
                    idProp.intValue = (int)keeperType.GetMethod("getIdOfObject").Invoke(objectKeeper, new object[] { obj });
                }
                else
                {
                    idProp.intValue = -1;
                }
            }
            else
            {
                EditorGUI.PropertyField(position, property, true);
                return;
            }
        }
    }
}