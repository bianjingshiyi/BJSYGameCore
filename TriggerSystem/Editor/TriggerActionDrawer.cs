
using UnityEngine;
using UnityEditor;

namespace TBSGameCore.TriggerSystem
{
    public class TriggerActionDrawer
    {
        TriggerComponentEditor _parent;
        public TriggerActionDrawer(TriggerComponentEditor parent)
        {
            _parent = parent;
        }
        Rect _rect;
        bool isExpaned { get; set; } = false;
        TriggerParameterDrawer[] _argDrawers = null;
        public void draw(TriggerActionComponent action, ITriggerScope scope)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(action.define.desc))
            {
                _parent.selectedAction = action;
                _parent.actionOperationMenu.DropDown(_rect);
            }
            else if (Event.current.type == EventType.Repaint)
                _rect = GUILayoutUtility.GetLastRect();
            TriggerParameterDefine[] paras = action.define.getParameters();
            if (paras.Length > 0)
            {
                GUILayout.Space(16);
                isExpaned = EditorGUI.Foldout(new Rect(_rect.x + _rect.width + 16, _rect.y, 16, _rect.height), isExpaned, new GUIContent(""));
            }
            GUILayout.EndHorizontal();
            if (paras.Length > 0 && isExpaned)
            {
                if (_argDrawers == null)
                {
                    _argDrawers = new TriggerParameterDrawer[paras.Length];
                    for (int i = 0; i < _argDrawers.Length; i++)
                        _argDrawers[i] = new TriggerParameterDrawer(paras[i]);
                }
                if (_argDrawers.Length != paras.Length)
                {
                    TriggerParameterDrawer[] newArgDrawers = new TriggerParameterDrawer[paras.Length];
                    for (int i = 0; i < newArgDrawers.Length; i++)
                    {
                        if (i < _argDrawers.Length)
                            newArgDrawers[i] = _argDrawers[i];
                        else
                            newArgDrawers[i] = new TriggerParameterDrawer(paras[i]);
                    }
                    _argDrawers = newArgDrawers;
                }
                for (int i = 0; i < paras.Length; i++)
                {
                    TriggerExprComponent arg;
                    if (i < action.transform.childCount)
                        arg = action.transform.GetChild(i).GetComponent<TriggerExprComponent>();
                    else
                    {
                        arg = ConstExprComponent.createInstance(paras[i].type);
                        arg.transform.parent = action.transform;
                        arg.transform.SetSiblingIndex(i);
                    }
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(new GUIContent(paras[i].name + ":"));
                    _argDrawers[i].draw(arg, scope);
                    GUILayout.EndHorizontal();
                }
            }
        }
    }
}
