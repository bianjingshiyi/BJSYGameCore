using System.Linq;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

namespace TBSGameCore.TriggerSystem
{
    class TriggerReflectActionDrawer : TriggerActionSubDrawer<TriggerReflectAction>
    {
        public TriggerReflectActionDrawer(TriggerObjectDrawer parent, Transform transform) : base(parent, transform)
        {
            if (!TriggerLibrary.isAssemblyLoaded(targetObject.GetType().Assembly))
                TriggerLibrary.load(targetObject.GetType().Assembly);
        }
        public override float height
        {
            get
            {
                float height = 16;
                if (isExpanded && paraDrawers != null)
                {
                    for (int i = 0; i < paraDrawers.Length; i++)
                    {
                        height += paraDrawers[i].height;
                    }
                }
                return height;
            }
        }
        bool isExpanded { get; set; } = false;
        TriggerTypedExprDrawer[] paraDrawers { get; set; } = null;
        protected override void draw(Rect position, GUIContent label, TriggerReflectAction action)
        {
            if (action == null)
                return;
            TriggerMethodDefine[] actions = TriggerLibrary.getActionDefines();
            if (actions.Length > 0)
            {
                //生成选项
                GUIContent[] options = new GUIContent[actions.Length + 1];
                for (int i = 0; i < actions.Length; i++)
                {
                    options[i + 1] = new GUIContent(actions[i].editorName);
                }
                //获取函数定义，如果没有就给一个默认值
                TriggerMethodDefine define = TriggerLibrary.getMethodDefine(action.idName);
                if (define == null)
                {
                    define = actions[0];
                    switchAction(action, define);
                }
                options[0] = new GUIContent(action.desc);
                //计算绘制区域
                Rect foldPosition;
                if (action.args == null || action.args.Length != define.paras.Length)
                    switchAction(action, define);
                if (action.args != null && action.args.Length > 0)
                    foldPosition = new Rect(position.x + position.width, position.y, 16, 16);
                else
                    foldPosition = new Rect(position.x + position.width, position.y, 0, 16);
                Rect popPosition;
                if (label != null)
                {
                    Rect labelPosition = new Rect(position.x, position.y, 64, 16);
                    EditorGUI.LabelField(labelPosition, label);
                    popPosition = new Rect(position.x + labelPosition.width, position.y, position.width - foldPosition.width - labelPosition.width, 16);
                }
                else
                    popPosition = new Rect(position.x, position.y, position.width - foldPosition.width, 16);
                //双击打开菜单
                if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && popPosition.Contains(Event.current.mousePosition) && Event.current.clickCount > 1)
                {
                    GenericMenu menu = new GenericMenu();
                    for (int i = 0; i < actions.Length; i++)
                    {
                        menu.AddItem(new GUIContent(actions[i].editorName), actions[i] == define, e =>
                        {
                            SwitchActionOperation operation = e as SwitchActionOperation;
                            define = operation.define;
                            operation.execute();
                        }, new SwitchActionOperation(this, action, actions[i]));
                    }
                    menu.DropDown(popPosition);
                    Event.current.Use();
                }
                GUI.Button(popPosition, new GUIContent(action.desc));
                //绘制参数
                if (action.args == null || action.args.Length != define.paras.Length)
                    switchAction(action, define);
                if (action.args != null && action.args.Length > 0)
                {
                    bool changeExpand = EditorGUI.Foldout(foldPosition, isExpanded, new GUIContent(""));
                    if (changeExpand != isExpanded)
                    {
                        isExpanded = changeExpand;
                        repaint();
                    }
                    if (isExpanded)
                    {
                        if (paraDrawers == null || paraDrawers.Length != define.paras.Length || action.args.Length != define.paras.Length)
                            switchAction(action, define);
                        Rect argPosition = new Rect(popPosition.x, popPosition.y + popPosition.height, popPosition.width, 0);
                        for (int i = 0; i < action.args.Length; i++)
                        {
                            argPosition.height = paraDrawers[i].height;
                            action.args[i] = paraDrawers[i].draw(argPosition, new GUIContent(define.paras[i].name), action.args[i]);
                            argPosition.y += argPosition.height;
                        }
                    }
                }
            }
            else
            {
                if (label != null)
                    EditorGUI.LabelField(position, label, new GUIContent("没有可用的函数"));
                else
                    EditorGUI.LabelField(position, new GUIContent("没有可用的函数"));
            }
        }
        private void switchAction(TriggerReflectAction action, TriggerMethodDefine define)
        {
            if (action.idName != define.idName)
            {
                action.idName = define.idName;
                if (action.args != null)
                {
                    for (int i = 0; i < action.args.Length; i++)
                    {
                        if (action.args[i] != null)
                            UnityEngine.Object.DestroyImmediate(action.args[i].gameObject);
                    }
                }
                action.args = new TriggerExpr[define.paras.Length];
                paraDrawers = new TriggerTypedExprDrawer[define.paras.Length];
                for (int i = 0; i < paraDrawers.Length; i++)
                {
                    paraDrawers[i] = new TriggerTypedExprDrawer(this, action.transform, define.paras[i].type, define.paras[i].name);
                }
            }
            else
            {
                if (action.args.Length != define.paras.Length)
                {
                    TriggerExpr[] newArgs = new TriggerExpr[define.paras.Length];
                    for (int i = 0; i < newArgs.Length; i++)
                    {
                        if (i < action.args.Length)
                            newArgs[i] = action.args[i];
                        else
                            newArgs[i] = null;
                    }
                    action.args = newArgs;
                }
                if (paraDrawers == null || paraDrawers.Length != define.paras.Length)
                {
                    paraDrawers = new TriggerTypedExprDrawer[define.paras.Length];
                    for (int i = 0; i < paraDrawers.Length; i++)
                    {
                        paraDrawers[i] = new TriggerTypedExprDrawer(this, action.transform, define.paras[i].type, define.paras[i].name);
                    }
                }
            }
        }
        class SwitchActionOperation
        {
            public TriggerReflectActionDrawer drawer { get; private set; }
            public TriggerReflectAction action { get; private set; }
            public TriggerMethodDefine define { get; private set; }
            public SwitchActionOperation(TriggerReflectActionDrawer drawer, TriggerReflectAction action, TriggerMethodDefine define)
            {
                this.drawer = drawer;
                this.action = action;
                this.define = define;
            }
            public void execute()
            {
                drawer.switchAction(action, define);
            }
        }
    }
    class TriggerScopeActionDrawer : TriggerActionSubDrawer<TriggerScopeAction>
    {
        public TriggerScopeActionDrawer(TriggerObjectDrawer parent, Transform transform) : base(parent, transform)
        {
        }
        public override float height
        {
            get
            {
                float height;
                if (actionDrawerList != null && actionDrawerList.Count > 0)
                {
                    height = 0;
                    for (int i = 0; i < actionDrawerList.Count; i++)
                    {
                        height += actionDrawerList[i].height;
                    }
                }
                else
                    height = 16;
                return height;
            }
        }
        protected override void draw(Rect position, GUIContent label, TriggerScopeAction action)
        {
            Rect actionsPosition;
            //绘制label
            if (label != null)
            {
                Rect labelPosition = new Rect(position.x, position.y, 64, 16);
                EditorGUI.LabelField(labelPosition, label);
                actionsPosition = new Rect(position.x + labelPosition.width, position.y, position.width - labelPosition.width, position.height);
            }
            else
                actionsPosition = position;
            //获取数组
            List<TriggerAction> actionList = new List<TriggerAction>(action.getActions());
            actionList.RemoveAll(e => { return e == null; });
            //检查绘制器
            if (actionDrawerList == null)
            {
                actionDrawerList = new List<TriggerTypedActionDrawer>(actionList.Count);
                for (int i = 0; i < actionList.Count; i++)
                {
                    actionDrawerList.Add(new TriggerTypedActionDrawer(this, action.transform));
                }
            }
            else if (actionDrawerList.Count != actionList.Count)
            {
                TriggerTypedActionDrawer[] newDrawers = new TriggerTypedActionDrawer[actionList.Count];
                for (int i = 0; i < newDrawers.Length; i++)
                {
                    if (i < actionDrawerList.Count)
                        newDrawers[i] = actionDrawerList[i];
                    else
                        newDrawers[i] = new TriggerTypedActionDrawer(this, action.transform);
                }
                actionDrawerList.Clear();
                actionDrawerList.AddRange(newDrawers);
            }
            //绘制所有动作
            if (actionList.Count > 0)
            {
                Rect actionPosition = new Rect(actionsPosition.x, actionsPosition.y, actionsPosition.width, 0);
                //取消选择
                if (Event.current.type == EventType.MouseDown && !actionsPosition.Contains(Event.current.mousePosition))
                {
                    _selectStartIndex = -1;
                    _selectEndIndex = -1;
                    repaint();
                }
                Color originColor = GUI.color;
                for (int i = 0; i < actionList.Count; i++)
                {
                    actionPosition.height = actionDrawerList[i].height;
                    //各种事件
                    checkEvent(actionPosition, i, actionList);
                    //设置选中颜色
                    if (isValidSelection())
                    {
                        if (_selectStartIndex <= i && i <= _selectEndIndex)
                        {
                            GUI.color = Color.cyan;
                        }
                        else
                            GUI.color = originColor;
                    }
                    //绘制动作
                    actionList[i] = actionDrawerList[i].draw(actionPosition, null, actionList[i]);
                    actionPosition.y += actionPosition.height;
                }
                GUI.color = originColor;
            }
            else
            {
                Rect actionPosition = new Rect(actionsPosition.x, actionsPosition.y, actionsPosition.width, 16);
                //各种事件
                checkEvent(actionPosition, -1, actionList);
                EditorGUI.LabelField(actionPosition, new GUIContent("右键菜单添加动作"));
            }
            //添加动作
            if (_addActionDefine != null)
            {
                if (_dropIndex > -1)
                {
                    TriggerReflectAction newAction = createNewAction(_addActionDefine);
                    actionList.Insert(_dropIndex + 1, newAction);
                    newAction.transform.parent = action.transform;
                    newAction.transform.SetSiblingIndex(_dropIndex + 1);
                }
                else
                {
                    TriggerReflectAction newAction = createNewAction(_addActionDefine);
                    newAction.transform.parent = action.transform;
                    actionList.Add(newAction);
                }
                _addActionDefine = null;
                _dropIndex = -1;
                repaint();
            }
            //删除动作
            if (_removeAction)
            {
                if (0 <= _dropIndex && _dropIndex < actionList.Count)
                {
                    UnityEngine.Object.DestroyImmediate(actionList[_dropIndex].gameObject);
                    actionList.RemoveAt(_dropIndex);
                    repaint();
                }
                _removeAction = false;
            }
            for (int i = 0; i < actionList.Count; i++)
            {
                actionList[i].transform.SetSiblingIndex(i);
            }
            action.resetActions(actionList.ToArray());
        }
        private void checkEvent(Rect position, int actionIndex, List<TriggerAction> actionList)
        {
            //点击选择
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && Event.current.clickCount == 1)
            {
                if (position.Contains(Event.current.mousePosition))
                {
                    if (Event.current.shift && isValidSelection())
                    {
                        //复选
                        if (actionIndex < _selectStartIndex)
                            _selectStartIndex = actionIndex;
                        if (actionIndex > _selectEndIndex)
                            _selectEndIndex = actionIndex;
                    }
                    else
                    {
                        //选中单个
                        _selectStartIndex = actionIndex;
                        _selectEndIndex = actionIndex;
                    }
                    repaint();
                }
            }
            //拖拽移动
            if (actionIndex >= 0)
            {
                if (Event.current.type == EventType.MouseDrag && position.Contains(Event.current.mousePosition))
                {
                    DragAndDrop.PrepareStartDrag();
                    DragAndDrop.SetGenericData("originIndex", actionIndex);
                    DragAndDrop.StartDrag("DragAction");
                    Event.current.Use();
                }
                if (Event.current.type == EventType.DragPerform && position.Contains(Event.current.mousePosition))
                {
                    int originIndex = (int)DragAndDrop.GetGenericData("originIndex");
                    TriggerAction origin = actionList[originIndex];
                    TriggerAction target = actionList[actionIndex];
                    if (origin != target)
                    {
                        if (actionIndex > 0)
                        {
                            actionList.Remove(origin);
                            actionList.Insert(actionList.IndexOf(target) + 1, origin);
                        }
                        else
                        {
                            actionList.Remove(origin);
                            actionList.Insert(0, origin);
                        }
                    }
                    Event.current.Use();
                }
                if (Event.current.type == EventType.DragUpdated && position.Contains(Event.current.mousePosition))
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Move;
                    Event.current.Use();
                }
            }
            //右键菜单
            if (_actionMenu == null)
                initMenu();
            if (Event.current.type == EventType.MouseUp && Event.current.button == 1 && position.Contains(Event.current.mousePosition))
            {
                _dropIndex = actionIndex;
                _actionMenu.DropDown(new Rect(Event.current.mousePosition, Vector2.zero));
                Event.current.Use();
            }
        }
        /// <summary>
        /// 当前选择是合法的
        /// </summary>
        /// <returns></returns>
        private bool isValidSelection()
        {
            return _selectStartIndex >= 0 && _selectStartIndex <= _selectEndIndex;
        }
        int _selectStartIndex = -1;
        int _selectEndIndex = -1;
        private TriggerReflectAction createNewAction(TriggerMethodDefine define)
        {
            TriggerReflectAction newAction = new GameObject(define.editorName).AddComponent<TriggerReflectAction>();
            newAction.idName = define.idName;
            newAction.args = new TriggerExpr[define.paras.Length];
            return newAction;
        }
        List<TriggerTypedActionDrawer> actionDrawerList { get; set; } = null;
        GenericMenu _actionMenu;
        int _dropIndex = -1;
        TriggerMethodDefine _addActionDefine = null;
        bool _removeAction = false;
        void initMenu()
        {
            _actionMenu = new GenericMenu();
            //添加动作
            if (!TriggerLibrary.isAssemblyLoaded(targetObject.GetType().Assembly))
                TriggerLibrary.load(targetObject.GetType().Assembly);
            TriggerMethodDefine[] actions = TriggerLibrary.getActionDefines();
            for (int i = 0; i < actions.Length; i++)
            {
                _actionMenu.AddItem(new GUIContent("添加动作/" + actions[i].editorName), false, e =>
                {
                    _addActionDefine = e as TriggerMethodDefine;
                }, actions[i]);
            }
            //删除动作
            _actionMenu.AddItem(new GUIContent("删除动作"), false, () =>
            {
                _removeAction = true;
            });
        }
    }
}