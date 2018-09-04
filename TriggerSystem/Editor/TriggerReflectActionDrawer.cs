
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
            actions = TriggerLibrary.getActionDefines();
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
        TriggerMethodDefine[] actions { get; set; }
        TriggerTypedExprDrawer[] paraDrawers { get; set; } = null;
        protected override void draw(Rect position, GUIContent label, TriggerReflectAction action)
        {
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
                if (action.args.Length > 0)
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
                //绘制菜单，点击菜单改变函数
                int index = EditorGUI.Popup(popPosition, 0, options);
                if (index != 0)
                {
                    define = actions[index - 1];
                    switchAction(action, define);
                }
                //绘制参数
                if (action.args.Length > 0)
                {
                    isExpanded = EditorGUI.Foldout(foldPosition, isExpanded, new GUIContent(""));
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
    }
}