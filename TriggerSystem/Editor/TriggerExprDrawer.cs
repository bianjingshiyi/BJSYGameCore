using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEditor;

namespace TBSGameCore.TriggerSystem
{
    [CustomPropertyDrawer(typeof(TriggerExpr))]
    public class TriggerExprDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(SerializedPropertyType.ObjectReference, label);
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
        }
    }
}