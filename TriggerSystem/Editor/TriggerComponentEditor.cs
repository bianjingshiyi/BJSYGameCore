using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;

using UnityEngine;
using UnityEditor;

namespace TBSGameCore.TriggerSystem
{
    [CustomEditor(typeof(TriggerComponent))]
    public class TriggerComponentEditor : Editor
    {
        protected void OnEnable()
        {
            _actionMenu = new GenericMenu();
            actionOperationMenu = new GenericMenu();
            foreach (TriggerActionDefine action in trigger.getActions())
            {
                _actionMenu.AddItem(new GUIContent(action.name), false, e =>
                {
                    TriggerActionComponent instance = (e as TriggerActionDefine).createInstance(trigger.transform);
                }, action);
                actionOperationMenu.AddItem(new GUIContent("新动作/" + action.name), false, e =>
                {
                    int index = selectedAction.transform.GetSiblingIndex();
                    TriggerActionComponent instance = (e as TriggerActionDefine).createInstance(trigger.transform);
                    instance.transform.SetSiblingIndex(index + 1);
                }, action);
                actionOperationMenu.AddItem(new GUIContent("删除"), false, e =>
                {
                    removeAction = selectedAction;
                }, action);
                actionOperationMenu.AddItem(new GUIContent("替换/" + action.name), false, e =>
                {
                    GameObject go = selectedAction.gameObject;
                    while (selectedAction.transform.childCount > 0)
                        DestroyImmediate(selectedAction.transform.GetChild(0).gameObject);
                    DestroyImmediate(selectedAction);
                    TriggerActionComponent instance = action.attachTo(go);
                }, action);
            }
        }
        public override void OnInspectorGUI()
        {
            if (EditorGUILayout.PropertyField(serializedObject.FindProperty("_eventSource"), true))
            {
            }
            TriggerComponent trigger = (target as TriggerComponent);
            if (_actionDrawers == null)
            {
                _actionDrawers = new TriggerActionDrawer[trigger.transform.childCount];
                for (int i = 0; i < _actionDrawers.Length; i++)
                {
                    _actionDrawers[i] = new TriggerActionDrawer(this);
                }
            }
            if (_actionDrawers.Length != trigger.transform.childCount)
            {
                TriggerActionDrawer[] newActionDrawers = new TriggerActionDrawer[trigger.transform.childCount];
                for (int i = 0; i < newActionDrawers.Length; i++)
                {
                    if (i < _actionDrawers.Length)
                        newActionDrawers[i] = _actionDrawers[i];
                    else
                        newActionDrawers[i] = new TriggerActionDrawer(this);
                }
                _actionDrawers = newActionDrawers;
            }
            for (int i = 0; i < trigger.transform.childCount; i++)
            {
                TriggerActionComponent action = trigger.transform.GetChild(i).GetComponent<TriggerActionComponent>();
                if (action != null)
                {
                    if (action == removeAction)
                    {
                        DestroyImmediate(action.gameObject);
                        i--;
                        removeAction = null;
                    }
                    else
                        _actionDrawers[i].draw(action, trigger);
                }
            }
            if (GUILayout.Button("添加动作"))
                _actionMenu.DropDown(_newActionButtonRect);
            else if (Event.current.type == EventType.Repaint)
                _newActionButtonRect = GUILayoutUtility.GetLastRect();
        }
        TriggerActionComponent removeAction { get; set; } = null;
        public GenericMenu actionOperationMenu { get; private set; }
        public TriggerActionComponent selectedAction { get; set; } = null;
        TriggerActionDrawer[] _actionDrawers;
        GenericMenu _actionMenu;
        Rect _newActionButtonRect;
        private void drawEvent(TriggerComponent trigger)
        {
            //获取所有事件
            TriggerEventDefine[] events = getEventNames();
            //获取当前的事件
            if (events.Length > 0)
            {
                //int index;
                //if (trigger.isRegistered)
                //    index = Array.FindIndex(events, e => { return e.eventName == trigger.eventName; });
                //else
                //{
                //    index = 0;
                //    events[index].addTrigger(trigger);
                //}
                //GUI
                GUIContent[] eventOptions = new GUIContent[events.Length];
                for (int i = 0; i < eventOptions.Length; i++)
                {
                    eventOptions[i] = new GUIContent(events[i].eventName);
                }
                //int newIndex = EditorGUILayout.Popup(new GUIContent("事件"), index, eventOptions);
                //if (index != newIndex)
                //{
                //    events[index].removeTrigger(trigger);
                //    index = newIndex;
                //    events[index].addTrigger(trigger);
                //}
            }
            else
            {
                EditorGUILayout.LabelField("没有事件");
            }
        }
        TriggerEventDefine[] getEventNames()
        {
            List<TriggerEventDefine> nameList = new List<TriggerEventDefine>();
            ITriggerEventSource[] eventSources = (target as TriggerComponent).GetComponentsInParent<ITriggerEventSource>();
            for (int i = 0; i < eventSources.Length; i++)
            {
                string[] eventNames = eventSources[i].getEventNames();
                for (int j = 0; j < eventNames.Length; j++)
                {
                    nameList.Add(new TriggerEventDefine(eventSources[i], eventNames[j]));
                }
            }
            return nameList.ToArray();
        }
        public TriggerComponent trigger
        {
            get { return target as TriggerComponent; }
        }
    }
}
