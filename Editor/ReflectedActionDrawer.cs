
using UnityEditor;
using UnityEngine;

namespace BJSYGameCore
{
    public class ReflectedActionDrawer : AbstractActionDrawer
    {
        TypedFuncStringDrawer[] _argDrawers = null;
        public ReflectedActionDrawer(UnityEngine.Object targetObject) : base(targetObject)
        {
        }
        public ReflectedActionDrawer(TriggerStringDrawer parent) : base(parent)
        {
        }
        public override float height
        {
            get
            {
                if (isExpanded)
                {
                    if (_argDrawers != null)
                    {
                        float height = 16;
                        for (int i = 0; i < _argDrawers.Length; i++)
                        {
                            height += _argDrawers[i].height;
                        }
                        return height;
                    }
                    else
                        return 16;
                }
                else
                    return 16;
            }
        }
        public override string draw(Rect position, string value, Method action)
        {
            if (!(action is ReflectedMethod))
                return null;
            string className;
            string methodName;
            string[] args;
            TriggerParser.parseReflectedAction(value, out className, out methodName, out args);
            Parameter[] paras = (action as ReflectedMethod).getParameters();
            if (args.Length != paras.Length)
                args = new string[paras.Length];
            if (paras.Length > 0)
            {
                Rect foldPosition = new Rect(position.x + position.width, position.y, 16, 16);
                isExpanded = EditorGUI.Foldout(foldPosition, isExpanded, "");
                if (isExpanded)
                {
                    Rect argPosition = new Rect(position.x + 16, position.y + 16, position.width - 16, 16);
                    if (_argDrawers == null || _argDrawers.Length != paras.Length)
                    {
                        _argDrawers = new TypedFuncStringDrawer[paras.Length];
                        for (int i = 0; i < paras.Length; i++)
                        {
                            _argDrawers[i] = new TypedFuncStringDrawer(targetObject);
                        }
                    }
                    for (int i = 0; i < paras.Length; i++)
                    {
                        args[i] = _argDrawers[i].draw(argPosition, new GUIContent(paras[i].name), args[i], paras[i].type);
                        argPosition.y += _argDrawers[i].height;
                    }
                }
                else
                {
                    for (int i = 0; i < paras.Length; i++)
                    {
                        if (string.IsNullOrEmpty(args[i]))
                        {
                            args[i] = "0";
                        }
                    }
                }
            }
            //返回值
            value = action.formatName + '(';
            for (int i = 0; i < args.Length; i++)
            {
                value += args[i];
                if (i < args.Length - 1)
                    value += ',';
            }
            value += ')';
            return value;
        }
    }
}