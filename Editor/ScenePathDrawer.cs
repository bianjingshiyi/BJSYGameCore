
using UnityEditor;
using UnityEngine;

namespace TBSGameCore
{
    [CustomPropertyDrawer(typeof(ScenePath))]
    [CanEditMultipleObjects]
    public class ScenePathDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(SerializedPropertyType.ObjectReference, label);
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty valueProp = property.FindPropertyRelative("_value");
            SceneAsset scene;
            if (!string.IsNullOrEmpty(valueProp.stringValue))
                scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(valueProp.stringValue);
            else
                scene = null;
            scene = EditorGUI.ObjectField(position, label, scene, typeof(SceneAsset), false) as SceneAsset;
            if (scene != null)
                valueProp.stringValue = AssetDatabase.GetAssetPath(scene);
            else
                valueProp.stringValue = string.Empty;
        }
    }
}