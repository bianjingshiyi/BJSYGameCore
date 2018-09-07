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
            get
            {
                float height;
                if (dicActionDrawer != null && dicActionDrawer.Count > 0)
                {
                    height = 0;
                    foreach (var actionDrawerPair in dicActionDrawer)
                    {
                        height += actionDrawerPair.Value.height;
                    }
                }
                else
                    height = 16;
                return height;
            }
        }
        protected override void draw(Rect position, GUIContent label, TriggerScopeAction action)
        {
            Rect scopePosition;
            //绘制label
            if (label != null)
            {
                Rect labelPosition = new Rect(position.x, position.y, 64, 16);
                EditorGUI.LabelField(labelPosition, label);
                scopePosition = new Rect(position.x + labelPosition.width, position.y, position.width - labelPosition.width, position.height);
            }
            else
                scopePosition = position;
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
            //绘制Scope
            drawScope(scopePosition, action);
        }
        void drawScope(Rect position, TriggerScopeAction scope)
        {
            currentScope = scope;
            //先清除多余子物体
            scope.cleanInvaildChild();
            //绘制动作
            if (scope.childCount > 0)
            {
                Rect actionPosition = new Rect(position.x, position.y, position.width, 0);
                bool needRepaint = false;
                for (int i = 0; i < scope.childCount; i++)
                {
                    TriggerAction action = scope.getAction(i);
                    if (!(action is TriggerScopeAction))
                    {
                        //检查绘制器
                        if (!dicActionDrawer.ContainsKey(action))
                        {
                            dicActionDrawer.Add(action, new TriggerTypedActionDrawer(this, scope.transform));
                            //不存在绘制器那高度肯定对不上，需要重新绘制
                            needRepaint = true;
                        }
                        actionPosition.height = dicActionDrawer[action].height;
                        dicActionDrawer[action].draw(actionPosition, null, action);
                        checkActionMenuEvent(actionPosition, action);
                        actionPosition.y += actionPosition.height;
                    }
                    else
                    {

                    }
                }
                if (needRepaint)
                    repaint();
            }
            else
            {
                checkAddActionEvent(position, scope);
                GUI.Box(position, new GUIContent("双击左键添加动作"), GUI.skin.button);
            }
        }
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