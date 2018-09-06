using UnityEditor;
using UnityEngine;

namespace TBSGameCore.TriggerSystem
{
    class TriggerTypedActionDrawer : TriggerObjectDrawer
    {
        public TriggerTypedActionDrawer(Component targetObject, Transform transform) : base(targetObject, transform)
        {
        }
        public TriggerTypedActionDrawer(TriggerObjectDrawer parent, Transform transform) : base(parent, transform)
        {
        }
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
        public TriggerAction draw(Rect position, GUIContent label, TriggerAction action)
        {
            Rect typePosition = new Rect(position.x + position.width - 40, position.y, 40, 16);
            Rect actionPosition;
            //绘制动作
            if (action == null)
            {
                actionPosition = new Rect(position.x, position.y, position.width - typePosition.width, 16);
                if (label != null)
                    EditorGUI.LabelField(actionPosition, label, new GUIContent("空动作"));
                else
                    EditorGUI.LabelField(actionPosition, new GUIContent("空动作"));
            }
            else
            {
                if (_drawer == null)
                    _drawer = TriggerActionSubDrawer.getActionDrawer(action.GetType(), this, action.transform);
                else if (!_drawer.canDraw(action))
                    _drawer = TriggerActionSubDrawer.getActionDrawer(action.GetType(), this, action.transform);
                actionPosition = new Rect(position.x, position.y, position.width - typePosition.width, _drawer.height);
                _drawer.draw(actionPosition, label, action);
            }
            //绘制类型
            GUIContent[] typeOptions = new GUIContent[] { new GUIContent("空动作"), new GUIContent("动作"), new GUIContent("作用域") };
            int type = getActionType(action);
            int newType = EditorGUI.Popup(typePosition, type, typeOptions);
            if (newType != type)
            {
                //摧毁旧的
                if (action != null)
                    UnityEngine.Object.DestroyImmediate(action.gameObject);
                //创建新的
                action = createActionOfType(label != null ? label.text : "Action", newType, transform);
                if (action != null)
                    _drawer = TriggerActionSubDrawer.getActionDrawer(action.GetType(), this, action.transform);
                else
                    _drawer = null;
            }
            return action;
        }
        TriggerActionSubDrawer _drawer;
        int getActionType(TriggerAction action)
        {
            if (action != null)
            {
                if (action is TriggerReflectAction)
                    return 1;
                else if (action is TriggerScopeAction)
                    return 2;
                else
                    return 0;
            }
            else
                return 0;
        }
        TriggerAction createActionOfType(string name, int actionType, Transform transform)
        {
            if (actionType == 1)
            {
                //动作
                GameObject gameObject = new GameObject(name);
                gameObject.transform.parent = transform;
                return gameObject.AddComponent<TriggerReflectAction>();
            }
            else if (actionType == 2)
            {
                //作用域
                GameObject gameObject = new GameObject(name);
                gameObject.transform.parent = transform;
                return gameObject.AddComponent<TriggerScopeAction>();
            }
            else
                return null;
        }
    }
}