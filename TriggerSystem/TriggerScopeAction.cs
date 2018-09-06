using System;
using System.Collections.Generic;

using UnityEngine;

namespace TBSGameCore.TriggerSystem
{
    public class TriggerScopeAction : TriggerAction
    {
        public void resetActions(TriggerAction[] actions)
        {
            _actions.Clear();
            _actions.AddRange(actions);
        }
        public void insertAction(int index, TriggerAction action)
        {
            _actions.Insert(index, action);
            action.transform.parent = transform;
            action.transform.SetSiblingIndex(index);
        }
        public void addAction(TriggerAction action)
        {
            _actions.Add(action);
            action.transform.parent = transform;
        }
        public TriggerAction[] getActions()
        {
            return _actions.ToArray();
        }
        [SerializeField]
        List<TriggerAction> _actions = new List<TriggerAction>();
        public override string desc
        {
            get { return "{……}"; }
        }
        public override void invoke(UnityEngine.Object targetObject)
        {
            TriggerAction[] actions = getActions();
            for (int i = 0; i < actions.Length; i++)
            {
                if (actions[i] != null)
                    actions[i].invoke(targetObject);
            }
        }
    }
}