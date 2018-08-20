using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

namespace TBSGameCore
{
    public class ActionStringDrawer : TriggerStringDrawer
    {
        public ActionStringDrawer(UnityEngine.Object targetObject) : base(targetObject)
        {
            init();
        }
        public ActionStringDrawer(TriggerStringDrawer parent) : base(parent)
        {
            init();
        }
        private void init()
        {
        }
        AbstractActionDrawer _drawer = null;
        public override float height
        {
            get
            {
                if (_drawer != null)
                    return _drawer.height;
                else
                    return 16;
            }
        }
        public string drawActionString(Rect position, GUIContent label, string value)
        {
            if (actions.Length > 0)
            {
                string formatName;
                TriggerParser.parseAction(value, out formatName);
                //检查与获取值
                int index = Array.FindIndex(actions, e => { return e.formatName == formatName; });
                if (index < 0)
                    index = 0;
                //绘制GUI
                GUIContent[] options = new GUIContent[actions.Length];
                for (int i = 0; i < actions.Length; i++)
                    options[i] = new GUIContent(actions[i].displayName);
                Rect valuePosition = new Rect(position.x, position.y, position.width - 16, 16);
                if (label != null)
                {
                    Rect labelPosition = new Rect(valuePosition.x, valuePosition.y, valuePosition.width / 3, valuePosition.height);
                    Rect optionsPosition = new Rect(valuePosition.x + labelPosition.width, valuePosition.y, valuePosition.width / 3, valuePosition.height);
                    Rect descPosition = new Rect(valuePosition.x + labelPosition.width + optionsPosition.width, valuePosition.y, valuePosition.width / 3, valuePosition.height);
                    EditorGUI.LabelField(labelPosition, label);
                    int newIndex = EditorGUI.Popup(optionsPosition, index, options);
                    if (newIndex != index)
                    {
                        index = newIndex;
                        _drawer = null;
                    }
                    EditorGUI.LabelField(descPosition, new GUIContent(actions[index].displayDesc));
                }
                else
                {
                    Rect optionsPosition = new Rect(valuePosition.x, valuePosition.y, valuePosition.width / 2, valuePosition.height);
                    Rect descPosition = new Rect(valuePosition.x + optionsPosition.width + optionsPosition.width, valuePosition.y, valuePosition.width / 2, valuePosition.height);
                    int newIndex = EditorGUI.Popup(optionsPosition, index, options);
                    if (newIndex != index)
                    {
                        index = newIndex;
                        _drawer = null;
                    }
                    EditorGUI.LabelField(descPosition, new GUIContent(actions[index].displayDesc));
                }
                Method action = actions[index];
                if (_drawer == null)
                    _drawer = AbstractActionDrawer.factory(action.formatName, this);
                if (_drawer != null)
                    return _drawer.draw(position, value, action);
                else
                    return null;
            }
            else
            {
                if (label != null)
                    EditorGUI.LabelField(position, label, new GUIContent("没有动作"));
                else
                    EditorGUI.LabelField(position, new GUIContent("没有动作"));
                return null;
            }
        }
    }
}