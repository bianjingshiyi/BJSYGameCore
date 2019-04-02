using System;
using System.Linq;

using UnityEditor;
using UnityEngine;

namespace BJSYGameCore
{
    public class VariableStringDrawer : AbstractFuncStringDrawer
    {
        public VariableStringDrawer(UnityEngine.Object targetObject) : base(targetObject)
        {
        }
        public VariableStringDrawer(TriggerStringDrawer parent) : base(parent)
        {
        }
        public override float height
        {
            get { return 16; }
        }
        public override string draw(Rect position, GUIContent label, string value, Type returnType)
        {
            if (targetObject is Component)
            {
                //在场景中
                VariableManager manager = (targetObject as Component).gameObject.scene.findInstance<VariableManager>();
                if (manager != null)
                {
                    string[] varNames = manager.getVarNamesOfType(returnType);//获取可选值
                    if (varNames.Length > 0)
                    {
                        GUIContent[] options = varNames.Select(e => { return new GUIContent(e); }).ToArray();
                        int index = Array.IndexOf(varNames, value);//获取索引
                        if (index < 0)
                            index = 0;
                        index = EditorGUI.Popup(position, label, index, options);//GUI
                        return varNames[index];
                    }
                    else
                    {
                        EditorGUI.LabelField(position, label, new GUIContent("没有对应类型的变量"));
                        return null;
                    }
                }
                else
                {
                    EditorGUI.LabelField(position, label, new GUIContent("场景中不存在" + nameof(VariableManager)));
                    return null;
                }
            }
            else
            {
                //不在场景中
                EditorGUI.LabelField(position, label, new GUIContent("不在场景中"));
                return null;
            }
        }
    }
}