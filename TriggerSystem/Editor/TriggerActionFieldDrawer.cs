
using UnityEngine;
using UnityEditor;

namespace TBSGameCore.TriggerSystem
{
    [CustomPropertyDrawer(typeof(TriggerAction))]
    public class TriggerActionFieldDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (_drawer == null)
            {
                Component targetObject = property.serializedObject.targetObject as Component;
                _drawer = new TriggerTypedActionDrawer(targetObject, targetObject.transform);
            }
            return _drawer.height;
        }
        TriggerTypedActionDrawer _drawer;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            if (property.serializedObject.targetObject is Component)
            {
                //绘制类型
                Component targetObject = property.serializedObject.targetObject as Component;
                if (_drawer == null)
                    _drawer = new TriggerTypedActionDrawer(targetObject, targetObject.transform);
                TriggerAction action = property.objectReferenceValue as TriggerAction;
                action = _drawer.draw(position, label, action);
                property.objectReferenceValue = action;
            }
            else
            {
                EditorGUI.LabelField(position, label, new GUIContent("目前TriggerSystem只支持作为组件的成员字段！"));
            }
            EditorGUI.EndProperty();
        }
    }
}