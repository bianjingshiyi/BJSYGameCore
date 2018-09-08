using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

namespace TBSGameCore.TriggerSystem
{
    public class TriggerScopeAction : TriggerAction
    {
        public void insertAction(int index,TriggerAction action)
        {
            action.transform.parent = transform;
            action.transform.SetSiblingIndex(index);
        }
        public void addAction(TriggerAction action)
        {
            action.transform.parent = transform;
        }
        /// <summary>
        /// 清除多余的不包含动作的子物体
        /// </summary>
        public void cleanInvaildChild()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                TriggerAction action = child.GetComponent<TriggerAction>();
                if (action == null)
                {
#if UNITY_EDITOR
                    if (UnityEditor.EditorApplication.isPlaying)
                        Destroy(child.gameObject);
                    else
                        DestroyImmediate(child.gameObject);
#else
                    Destroy(child.gameObject);    
#endif
                    i--;
                }
            }
        }
        /// <summary>
        /// 获取子动作的数量。注意这个方法返回的结果中可能包含子物体中的空动作的数量，所以推荐在使用这个方法之前先调用cleanInvaildChild来清除多余的子物体。
        /// </summary>
        public int actionCount
        {
            get { return transform.childCount; }
        }
        /// <summary>
        /// 获取子物体中的动作。注意这个方法并不保证返回的动作可能为空，所以推荐在使用这个方法之前先调用cleanInvaildChild来保证子物体清洁。
        /// </summary>
        /// <param name="siblingIndex"></param>
        /// <returns></returns>
        public TriggerAction getAction(int siblingIndex)
        {
            return transform.GetChild(siblingIndex).GetComponent<TriggerAction>();
        }
        public TriggerAction[] getActions()
        {
            List<TriggerAction> actionList = new List<TriggerAction>(transform.childCount);
            for (int i = 0; i < transform.childCount; i++)
            {
                TriggerAction action = getAction(i);
                if (action != null)
                    actionList.Add(action);
            }
            return actionList.ToArray();
        }
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