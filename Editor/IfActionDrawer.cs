using UnityEngine;
using UnityEditor;

namespace TBSGameCore
{
    public class IfActionDrawer : AbstractActionDrawer
    {
        public IfActionDrawer(UnityEngine.Object targetObject) : base(targetObject)
        {
            _conditionDrawer = new TypedFuncStringDrawer(this);
            _thenDrawer = new ActionsStringDrawer(this);
            _elseDrawer = new ActionsStringDrawer(this);
        }
        public IfActionDrawer(TriggerStringDrawer parent) : base(parent)
        {
            _conditionDrawer = new TypedFuncStringDrawer(this);
            _thenDrawer = new ActionsStringDrawer(this);
            _elseDrawer = new ActionsStringDrawer(this);
        }
        TypedFuncStringDrawer _conditionDrawer;
        ActionsStringDrawer _thenDrawer;
        ActionsStringDrawer _elseDrawer;
        public override float height
        {
            get
            {
                if (isExpanded)
                    return (_conditionDrawer != null ? _conditionDrawer.height : 16) +
                           (_thenDrawer != null ? _thenDrawer.height : 16) +
                           (_elseDrawer != null ? _elseDrawer.height : 16) + (isExpanded ? 16 : 0);
                else
                    return 16;
            }
        }
        public override string draw(Rect position, string value, Method action)
        {
            string condition;
            string thenActions;
            string elseActions;
            TriggerParser.parseIf(value, out condition, out thenActions, out elseActions);
            Rect foldPosition = new Rect(position.x + position.width, position.y, 16, 16);
            isExpanded = EditorGUI.Foldout(foldPosition, isExpanded, "");
            if (isExpanded)
            {
                Rect conditionPosition = new Rect(position.x + 16, position.y + 16, position.width - 16, _conditionDrawer.height);
                _conditionDrawer.draw(conditionPosition, new GUIContent("condition"), condition, typeof(bool));
                Rect thenPosition = new Rect(conditionPosition.x, conditionPosition.y + conditionPosition.height, conditionPosition.width, _thenDrawer.height);
                _thenDrawer.draw(thenPosition, new GUIContent("then"), thenActions);
                Rect elsePosition = new Rect(thenPosition.x, thenPosition.y + thenPosition.height, thenPosition.width, _elseDrawer.height);
                _elseDrawer.draw(elsePosition, new GUIContent("else"), elseActions);
            }
            return "if(" + condition + "){" + thenActions + "}else{" + elseActions + "}";
        }
    }
}