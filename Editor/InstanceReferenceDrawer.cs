using System;
using System.Linq;

using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TBSGameCore
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
            RefTypeAttribute att = fieldInfo.GetCustomAttributes(typeof(RefTypeAttribute), false).FirstOrDefault(e => { return e is RefTypeAttribute; }) as RefTypeAttribute;
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
                UnityEngine.Object obj = null;
                if (idProp.intValue > 0)
                {
                    SaveManager saveManager = (property.serializedObject.targetObject as Component).gameObject.scene.findInstance<SaveManager>();
                    if (saveManager != null)
                    {
                        SavableInstance instance = saveManager.getInstanceById(idProp.intValue);
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
                        GUI.Box(position, "场景里没有SaveManager");
                        return;
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
                //GUI
                obj = EditorGUI.ObjectField(position, label, obj, _refType, true);
                //再设置值
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
            else
            {
                EditorGUI.PropertyField(position, property, true);
                return;
            }
        }
    }
}