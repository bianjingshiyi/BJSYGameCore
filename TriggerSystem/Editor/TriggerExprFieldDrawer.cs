using System;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEditor;

namespace TBSGameCore.TriggerSystem
{
    [CustomPropertyDrawer(typeof(TriggerExpr))]
    public class TriggerExprFieldDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (_drawer == null)
            {
                Type exprType;
                TypeAttribute att = fieldInfo.GetCustomAttribute<TypeAttribute>();
                if (att != null)
                    exprType = att.type;
                else
                    exprType = typeof(object);
                Component targetObject = property.serializedObject.targetObject as Component;
                _drawer = new TriggerTypedExprDrawer(targetObject, targetObject.transform, exprType, label.text);
            }
            return _drawer.height;
        }
        TriggerTypedExprDrawer _drawer;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            if (property.serializedObject.targetObject is Component)
            {
                //绘制类型
                Type exprType;
                TypeAttribute att = fieldInfo.GetCustomAttribute<TypeAttribute>();
                if (att != null)
                    exprType = att.type;
                else
                    exprType = typeof(object);
                Component targetObject = property.serializedObject.targetObject as Component;
                if (_drawer == null)
                    _drawer = new TriggerTypedExprDrawer(targetObject, targetObject.transform, exprType, label.text);
                TriggerExpr expr = property.objectReferenceValue as TriggerExpr;
                expr = _drawer.draw(position, label, expr);
                property.objectReferenceValue = expr;
            }
            else
            {
                EditorGUI.LabelField(position, label, new GUIContent("目前TriggerSystem只支持作为组件的成员字段！"));
            }
            EditorGUI.EndProperty();
        }
    }
}