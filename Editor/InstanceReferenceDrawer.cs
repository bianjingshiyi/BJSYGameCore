using System;
using System.Linq;

using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BJSYGameCore
{
    [CustomPropertyDrawer(typeof(InstanceReference))]
    [CanEditMultipleObjects]
    public class InstanceReferenceDrawer : PropertyDrawer
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
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
        Type _refType = null;
        void init()
        {
            TypeAttribute att = fieldInfo.GetCustomAttributes(typeof(TypeAttribute), false).FirstOrDefault(e => { return e is TypeAttribute; }) as TypeAttribute;
            if (att != null)
            {
                _refType = att.type;
            }
            else
            {
                _refType = typeof(GameObject);
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
                SerializedProperty pathProp = property.FindPropertyRelative("path");
                //先获取值
                UnityEngine.Object obj = getValue(position, property, idProp, pathProp);
                //GUI
                if (_refType.IsInterface)
                {
                    obj = EditorGUI.ObjectField(position, label, obj, typeof(UnityEngine.Object), true);
                    if (obj != null && !_refType.IsInstanceOfType(obj))
                    {
                        if (obj is GameObject)
                        {
                            obj = (obj as GameObject).GetComponent(_refType);
                        }
                        else
                            obj = null;
                    }
                }
                else
                    obj = EditorGUI.ObjectField(position, label, obj, _refType, true);
                //再设置值
                resetValue(idProp, pathProp, obj);
            }
            else
                EditorGUI.PropertyField(position, property, true);
        }
        private void resetValue(SerializedProperty idProp, SerializedProperty pathProp, UnityEngine.Object obj)
        {
            if (obj != null)
            {
                GameObject go = null;
                if (obj is GameObject)
                {
                    go = (obj as GameObject);
                }
                else if (obj is Component)
                {
                    go = (obj as Component).gameObject;
                }
                else
                {
                    idProp.intValue = 0;
                    pathProp.stringValue = string.Empty;
                    return;
                }
                SavableInstance instance = go.GetComponentInParent<SavableInstance>();
                if (instance != null)
                {
                    idProp.intValue = instance.id;
                    pathProp.stringValue = go.name;
                    for (Transform parent = go.transform.parent; parent != null; parent = parent.parent)
                    {
                        if (parent != instance.transform)
                            pathProp.stringValue = parent.gameObject.name + '/' + pathProp.stringValue;
                        else
                            break;
                    }
                }
                else
                {
                    idProp.intValue = 0;
                    pathProp.stringValue = go.name;
                    for (Transform parent = go.transform.parent; parent != null; parent = parent.parent)
                    {
                        pathProp.stringValue = parent.gameObject.name + '/' + pathProp.stringValue;
                    }
                }
            }
            else
            {
                idProp.intValue = 0;
                pathProp.stringValue = string.Empty;
            }
        }
        private UnityEngine.Object getValue(Rect position, SerializedProperty property, SerializedProperty idProp, SerializedProperty pathProp)
        {
            UnityEngine.Object obj = null;
            if (idProp.intValue > 0)
            {
                InstanceManager manager = (property.serializedObject.targetObject as Component).gameObject.scene.findInstance<InstanceManager>();
                if (manager != null)
                {
                    SavableInstance instance = manager.getInstanceById(idProp.intValue);
                    if (!string.IsNullOrEmpty(pathProp.stringValue))
                    {
                        GameObject child = instance.findChild(pathProp.stringValue);
                        if (child != null)
                        {
                            if (_refType == typeof(GameObject))
                                obj = child;
                            else
                                obj = child.GetComponent(_refType);
                        }
                    }
                    else
                    {
                        if (_refType == typeof(GameObject))
                            obj = instance.gameObject;
                        else
                            obj = instance.GetComponent(_refType);
                    }
                }
                else
                {
                    GUI.Box(position, "场景里没有" + nameof(InstanceManager));
                    obj = null;
                }
            }
            else if (!string.IsNullOrEmpty(pathProp.stringValue))
            {
                Scene scene = (property.serializedObject.targetObject as Component).gameObject.scene;
                GameObject child = scene.findGameObjectAt(pathProp.stringValue);
                if (child != null)
                {
                    if (_refType == typeof(GameObject))
                        obj = child;
                    else
                        obj = child.GetComponent(_refType);
                }
            }
            return obj;
        }
    }
}