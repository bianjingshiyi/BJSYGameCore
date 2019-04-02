using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

namespace BJSYGameCore
{
    public class ActionsStringDrawer : TriggerStringDrawer
    {
        ActionStringDrawer[] _actionDrawers = null;
        public ActionsStringDrawer(UnityEngine.Object targetObject) : base(targetObject)
        {
        }
        public ActionsStringDrawer(TriggerStringDrawer parent) : base(parent)
        {
        }
        public override float height
        {
            get
            {
                float height = 16;
                if (_actionDrawers != null && isExpanded)
                {
                    for (int i = 0; i < _actionDrawers.Length; i++)
                        height += _actionDrawers[i].height;
                }
                return height;
            }
        }
        public string draw(Rect position, GUIContent label, string value)
        {
            //解析值
            string[] actions;
            TriggerParser.parseActions(value, out actions);
            List<string> actionList = new List<string>(actions);
            //绘制值
            if (label != null)
            {
                Rect labelPosition = new Rect(position.x, position.y, position.width, 16);
                EditorGUI.LabelField(labelPosition, label);
                Rect buttonPosition = new Rect(position.x + position.width - 32, position.y, 16, 16);
                if (GUI.Button(buttonPosition, new GUIContent("+")))
                    actionList.Add(null);
                Rect foldPosition = new Rect(position.x + position.width, position.y, 16, 16);
                isExpanded = EditorGUI.Foldout(foldPosition, isExpanded, "");
                if (isExpanded)
                {
                    //绘制的准备工作
                    if (_actionDrawers == null)
                    {
                        _actionDrawers = new ActionStringDrawer[actionList.Count];
                        for (int i = 0; i < _actionDrawers.Length; i++)
                            _actionDrawers[i] = new ActionStringDrawer(targetObject);
                    }
                    if (_actionDrawers.Length != actionList.Count)
                    {
                        ActionStringDrawer[] newActionDrawers = new ActionStringDrawer[actionList.Count];
                        for (int i = 0; i < newActionDrawers.Length; i++)
                        {
                            if (i < _actionDrawers.Length && _actionDrawers[i] != null)
                                newActionDrawers[i] = _actionDrawers[i];
                            else
                                newActionDrawers[i] = new ActionStringDrawer(this);
                        }
                        _actionDrawers = newActionDrawers;
                    }
                    Rect actionPosition = new Rect(labelPosition.x + 16, labelPosition.y + 16, labelPosition.width - 16, 0);
                    for (int i = 0; i < actionList.Count; i++)
                    {
                        actionPosition.height = _actionDrawers[i].height;
                        actionList[i] = _actionDrawers[i].drawActionString(actionPosition, null, actionList[i]);
                        actionPosition.y += _actionDrawers[i].height;
                    }
                }
            }
            else
            {
                Rect buttonPosition = new Rect(position.x + position.width - 32, position.y, 16, 16);
                if (GUI.Button(buttonPosition, new GUIContent("+")))
                    actionList.Add(null);
                Rect foldPosition = new Rect(position.x + position.width, position.y, 16, 16);
                isExpanded = EditorGUI.Foldout(foldPosition, isExpanded, "");
                if (isExpanded)
                {
                    //绘制的准备工作
                    if (_actionDrawers == null)
                    {
                        _actionDrawers = new ActionStringDrawer[actionList.Count];
                        for (int i = 0; i < _actionDrawers.Length; i++)
                            _actionDrawers[i] = new ActionStringDrawer(targetObject);
                    }
                    if (_actionDrawers.Length != actionList.Count)
                    {
                        ActionStringDrawer[] newActionDrawers = new ActionStringDrawer[actionList.Count];
                        for (int i = 0; i < newActionDrawers.Length; i++)
                        {
                            if (i < _actionDrawers.Length && _actionDrawers[i] != null)
                                newActionDrawers[i] = _actionDrawers[i];
                            else
                                newActionDrawers[i] = new ActionStringDrawer(this);
                        }
                        _actionDrawers = newActionDrawers;
                    }
                    Rect actionPosition = new Rect(position.x + 16, position.y + 16, position.width - 16, 0);
                    for (int i = 0; i < actionList.Count; i++)
                    {
                        actionPosition.height = _actionDrawers[i].height;
                        actionList[i] = _actionDrawers[i].drawActionString(actionPosition, null, actionList[i]);
                        actionPosition.y += _actionDrawers[i].height;
                    }
                }
            }
            //重新生成值
            value = string.Empty;
            for (int i = 0; i < actionList.Count; i++)
            {
                value += actionList[i] + ';';
                Debug.Log("Actions[" + i + "]：" + actionList[i]);
            }
            return value;
        }
    }
}