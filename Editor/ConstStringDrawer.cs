using System;

using UnityEditor;
using UnityEngine;

namespace BJSYGameCore
{
    public class ConstStringDrawer : AbstractFuncStringDrawer
    {
        public ConstStringDrawer(UnityEngine.Object targetObject) : base(targetObject)
        {
        }
        public ConstStringDrawer(TriggerStringDrawer parent) : base(parent)
        {
        }
        public override float height
        {
            get { return 16; }
        }
        public override string draw(Rect position, GUIContent label, string value, Type returnType)
        {
            if (returnType == typeof(int))
            {
                return drawInt(position, label, value);
            }
            else if (returnType == typeof(float))
            {
                return drawFloat(position, label, value);
            }
            else if (returnType == typeof(bool))
            {
                return drawBool(position, label, value);
            }
            else if (returnType == typeof(string))
            {
                return drawString(position, label, value);
            }
            else if (returnType.IsSubclassOf(typeof(Component)))
            {
                return drawInstance(position, label, value, returnType);
            }
            else
            {
                EditorGUI.LabelField(position, label, new GUIContent("暂时不支持该类型常量"));
                return null;
            }
        }

        private string drawInstance(Rect position, GUIContent label, string value, Type returnType)
        {
            InstanceReference instanceValue = TriggerParser.parseInstanceReference(value);
            Component component;
            if (targetObject is Component)
                component = instanceValue.findInstanceIn((targetObject as Component).gameObject.scene, returnType);
            else
                component = null;
            component = EditorGUI.ObjectField(position, label, component, returnType, true) as Component;
            instanceValue = new InstanceReference(component);
            return instanceValue.ToString();
        }

        private static string drawString(Rect position, GUIContent label, string value)
        {
            return EditorGUI.TextField(position, label, value);
        }

        private static string drawBool(Rect position, GUIContent label, string value)
        {
            bool boolValue = TriggerParser.parseBool(value);
            boolValue = EditorGUI.Toggle(position, label, boolValue);
            return boolValue.ToString();
        }

        private static string drawFloat(Rect position, GUIContent label, string value)
        {
            float floatValue = TriggerParser.parseFloat(value);
            floatValue = EditorGUI.FloatField(position, label, floatValue);
            return floatValue.ToString();
        }

        private static string drawInt(Rect position, GUIContent label, string value)
        {
            int intValue = TriggerParser.parseInt(value);
            intValue = EditorGUI.IntField(position, label, intValue);
            return intValue.ToString();
        }
    }
}