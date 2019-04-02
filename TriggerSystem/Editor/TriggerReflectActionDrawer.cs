using System;
using System.Linq;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

namespace BJSYGameCore.TriggerSystem
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
            TriggerReflectMethodDefine[] actions = TriggerLibrary.getActionDefines();
            if (actions.Length > 0)
            {
                //生成选项
                GUIContent[] options = new GUIContent[actions.Length + 1];
                for (int i = 0; i < actions.Length; i++)
                {
                    options[i + 1] = new GUIContent(actions[i].editorName);
                }
                //获取函数定义，如果没有就给一个默认值
                TriggerReflectMethodDefine define = TriggerLibrary.getMethodDefine(action.idName);
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
                GUI.Box(popPosition, new GUIContent(action.desc), GUI.skin.button);
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
        private void switchAction(TriggerReflectAction action, TriggerReflectMethodDefine define)
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
                if (action.args == null || action.args.Length != define.paras.Length)
                {
                    TriggerExpr[] newArgs = new TriggerExpr[define.paras.Length];
                    for (int i = 0; i < newArgs.Length; i++)
                    {
                        if (action.args != null && i < action.args.Length)
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
            public TriggerReflectMethodDefine define { get; private set; }
            public SwitchActionOperation(TriggerReflectActionDrawer drawer, TriggerReflectAction action, TriggerReflectMethodDefine define)
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
            addActionMenu = new GenericMenu();
            //添加反射动作
            if (!TriggerLibrary.isAssemblyLoaded(targetObject.GetType().Assembly))
                TriggerLibrary.load(targetObject.GetType().Assembly);
            TriggerReflectMethodDefine[] actions = TriggerLibrary.getActionDefines();
            for (int i = 0; i < actions.Length; i++)
            {
                addActionMenu.AddItem(new GUIContent(actions[i].editorName), false, e =>
                {
                    TriggerReflectMethodDefine define = e as TriggerReflectMethodDefine;
                    if (currentScope != null)
                    {
                        TriggerAction action = define.createAction();
                        Undo.RegisterCreatedObjectUndo(action.gameObject, "New Action");
                        currentScope.addAction(action);
                    }
                    else
                        Debug.LogWarning("当前作用域为空", targetObject);
                }, actions[i]);
            }
        }
        public override float height
        {
            get { return _height; }
        }
        float _height = 16;
        protected override void draw(Rect position, GUIContent label, TriggerScopeAction action)
        {
            //清理多余的绘制器
            List<TriggerAction> destroyedActionList = new List<TriggerAction>();
            foreach (var actionDrawerPair in dicActionDrawer)
            {
                if (actionDrawerPair.Key == null)
                    destroyedActionList.Add(actionDrawerPair.Key);
            }
            for (int i = 0; i < destroyedActionList.Count; i++)
            {
                dicActionDrawer.Remove(destroyedActionList[i]);
            }
            //取消选择事件
            if (checkCancelSelectEvent(position))
            {
                //取消选择颜色变化，重新绘制
                repaint();
            }
            //绘制Scope
            currentSequenceNumber = 0;
            float height;
            drawScope(position, label, action, action, out height);
            if (height != _height)
            {
                //如果高度不相同，重新绘制
                _height = height;
                repaint();
            }
        }
        void drawScope(Rect position, GUIContent label, TriggerScopeAction scope, TriggerScopeAction rootScope, out float height)
        {
            Color originColor = GUI.color;
            Color selectedColor = Color.cyan;
            currentScope = scope;
            //先清除多余子物体
            scope.cleanInvaildChild();
            //绘制动作
            if (scope.actionCount > 0)
            {
                Rect foldPosition;
                Rect scopePosition;
                if (label != null)
                {
                    foldPosition = new Rect(position.x, position.y, 64, 16);
                    scopePosition = new Rect(position.x + foldPosition.width, position.y, position.width - foldPosition.width, position.height);
                }
                else
                {
                    foldPosition = new Rect(position.x, position.y, 16, 16);
                    scopePosition = new Rect(position);
                }
                setScopeFoldout(scope, EditorGUI.Foldout(foldPosition, getScopeFoldout(scope), label));
                if (getScopeFoldout(scope))
                {
                    height = 0;
                    Rect actionPosition = new Rect(scopePosition.x, scopePosition.y, scopePosition.width, 0);
                    bool needRepaint = false;
                    for (int i = 0; i < scope.actionCount; i++)
                    {
                        TriggerAction action = scope.getAction(i);
                        if (!(action is TriggerScopeAction))
                        {
                            //累加顺序号
                            currentSequenceNumber++;
                            //检查绘制器
                            if (!dicActionDrawer.ContainsKey(action))
                            {
                                dicActionDrawer.Add(action, new TriggerTypedActionDrawer(this, scope.transform));
                                //不存在绘制器那高度肯定对不上，需要重新绘制
                                needRepaint = true;
                            }
                            actionPosition.height = dicActionDrawer[action].height;
                            //绘制前事件
                            if (checkSelectEvent(actionPosition))
                            {
                                //选中动作的颜色有延迟，也需要重新绘制
                                needRepaint = true;
                            }
                            //绘制
                            if (isSelected(currentSequenceNumber))
                                GUI.color = selectedColor;
                            else
                                GUI.color = originColor;
                            dicActionDrawer[action].draw(actionPosition, null, action);
                            GUI.color = originColor;
                            //绘制后事件
                            checkActionMenuEvent(actionPosition, action);
                            actionPosition.y += actionPosition.height;
                            height += actionPosition.height;
                        }
                        else
                        {
                            TriggerScopeAction childScope = action as TriggerScopeAction;
                            actionPosition.height = getScopeHeight(childScope);
                            float childScopeHeight;
                            drawScope(actionPosition, new GUIContent("Scope"), childScope, rootScope, out childScopeHeight);
                            height += childScopeHeight;
                        }
                    }
                    if (needRepaint)
                        repaint();
                }
                else
                    height = foldPosition.height;
            }
            else
            {
                Rect scopePosition;
                if (label != null)
                {
                    Rect labelPosition = new Rect(position.x, position.y, 64, 16);
                    EditorGUI.LabelField(labelPosition, label);
                    scopePosition = new Rect(position.x + labelPosition.width, position.y, position.width - labelPosition.width, 16);
                }
                else
                    scopePosition = new Rect(position.x, position.y, position.width, 16);
                checkAddActionEvent(scopePosition, scope);
                GUI.Box(scopePosition, new GUIContent("双击左键添加动作"), GUI.skin.button);
                height = scopePosition.height;
            }
        }
        float getScopeHeight(TriggerScopeAction scope)
        {
            float height;
            if (scope.actionCount > 0)
            {
                height = 0;
                for (int i = 0; i < scope.actionCount; i++)
                {
                    TriggerAction action = scope.getAction(i);
                    if (action is TriggerScopeAction)
                        height += getScopeHeight(action as TriggerScopeAction);
                    else
                    {
                        if (!dicActionDrawer.ContainsKey(action))
                            dicActionDrawer.Add(action, new TriggerTypedActionDrawer(this, scope.transform));
                        height += dicActionDrawer[action].height;
                    }
                }
            }
            else
                height = 16;
            return height;
        }
        void setScopeFoldout(TriggerScopeAction scope, bool value)
        {
            dicScopeFold[scope] = value;
        }
        bool getScopeFoldout(TriggerScopeAction scope)
        {
            return dicScopeFold.ContainsKey(scope) ? dicScopeFold[scope] : false;
        }
        Dictionary<TriggerScopeAction, bool> dicScopeFold { get; set; } = new Dictionary<TriggerScopeAction, bool>();
        TriggerAction[] getSelectedActions(TriggerScopeAction rootScope, int startNumber, int endNumber)
        {
            List<TriggerAction> sequence = new List<TriggerAction>(rootScope.actionCount);
            getActionSequence(rootScope, sequence);
            return sequence.Skip(startNumber - 1).Take(endNumber - startNumber + 1).ToArray();
        }
        void getActionSequence(TriggerScopeAction scope, List<TriggerAction> sequence)
        {
            for (int i = 0; i < scope.actionCount; i++)
            {
                TriggerAction action = scope.getAction(i);
                if (action is TriggerScopeAction)
                    getActionSequence(action as TriggerScopeAction, sequence);
                else
                    sequence.Add(action);
            }
        }
        bool isSelected(int sequenceNumber)
        {
            if (isCurrentSelected())
                return selectStartNumber <= sequenceNumber && sequenceNumber <= selectEndNumber;
            else
                return false;
        }
        bool checkCancelSelectEvent(Rect position)
        {
            if (Event.current.type == EventType.MouseDown && !position.Contains(Event.current.mousePosition) && Event.current.button == 0)
            {
                selectStartNumber = -1;
                selectEndNumber = -1;
                return true;
            }
            else
                return false;
        }
        bool checkSelectEvent(Rect position)
        {
            if (Event.current.type == EventType.MouseDown && position.Contains(Event.current.mousePosition) && Event.current.button == 0)
            {
                if (Event.current.shift && isCurrentSelected())
                {
                    //复选
                    if (currentSequenceNumber < selectStartNumber)
                        selectStartNumber = currentSequenceNumber;
                    if (currentSequenceNumber > selectEndNumber)
                        selectEndNumber = currentSequenceNumber;
                }
                else
                {
                    //单选
                    selectStartNumber = currentSequenceNumber;
                    selectEndNumber = currentSequenceNumber;
                }
                return true;
            }
            else
                return false;
        }
        private bool isCurrentSelected()
        {
            return selectStartNumber >= 0 && selectStartNumber <= selectEndNumber;
        }
        int selectStartNumber { get; set; } = -1;
        int selectEndNumber { get; set; } = -1;
        /// <summary>
        /// 当前动作的顺序号。
        /// </summary>
        int currentSequenceNumber { get; set; } = 0;
        void checkActionMenuEvent(Rect position, TriggerAction action)
        {
            if (Event.current.type == EventType.MouseDown && position.Contains(Event.current.mousePosition) && Event.current.button == 1)
            {
                GenericMenu actionMenu = new GenericMenu();
                //添加动作
                TriggerReflectMethodDefine[] defines = TriggerLibrary.getActionDefines();
                for (int i = 0; i < defines.Length; i++)
                {
                    actionMenu.AddItem(new GUIContent("添加动作/" + defines[i].editorName), false, e =>
                    {
                        (e as AddActionOperation).execute();
                    }, new AddActionOperation(action, defines[i]));
                }
                //删除动作
                actionMenu.AddItem(new GUIContent("删除动作"), false, e =>
                {
                    (e as RemoveActionOperation).execute();
                }, new RemoveActionOperation(action));
                actionMenu.DropDown(position);
                Event.current.Use();
            }
        }
        TriggerScopeAction currentScope { get; set; } = null;
        void checkAddActionEvent(Rect position, TriggerScopeAction scope)
        {
            if (Event.current.type == EventType.MouseDown && position.Contains(Event.current.mousePosition) && Event.current.button == 0 && Event.current.clickCount > 1)
            {
                addActionMenu.DropDown(position);
                Event.current.Use();
            }
        }
        GenericMenu addActionMenu { get; set; } = null;
        Dictionary<TriggerAction, TriggerTypedActionDrawer> dicActionDrawer { get; set; } = new Dictionary<TriggerAction, TriggerTypedActionDrawer>();
        class AddActionOperation
        {
            TriggerAction action { get; set; }
            TriggerReflectMethodDefine define { get; set; }
            public AddActionOperation(TriggerAction action, TriggerReflectMethodDefine define)
            {
                this.action = action;
                this.define = define;
            }
            public void execute()
            {
                if (action.scope != null)
                {
                    TriggerAction newAction = define.createAction();
                    Undo.RegisterCreatedObjectUndo(newAction.gameObject, "New Action");
                    action.scope.insertAction(action.index + 1, newAction);
                }
                else
                    Debug.LogWarning("添加动作失败，" + action + "不在作用域中", action);
            }
        }
        class RemoveActionOperation
        {
            TriggerAction action { get; set; }
            public RemoveActionOperation(TriggerAction action)
            {
                this.action = action;
            }
            public void execute()
            {
                Undo.DestroyObjectImmediate(action.gameObject);
            }
        }
    }
}