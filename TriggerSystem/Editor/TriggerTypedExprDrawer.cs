using System;
using UnityEditor;
using UnityEngine;

namespace BJSYGameCore.TriggerSystem
{
    class TriggerTypedExprDrawer : TriggerExprDrawer
    {
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
        public TriggerTypedExprDrawer(Component targetObject, Transform transform, Type targetType, string targetName) : base(targetObject, transform, targetType, targetName)
        {
        }
        public TriggerTypedExprDrawer(TriggerObjectDrawer parent, Transform transform, Type targetType, string targetName) : base(parent, transform, targetType, targetName)
        {
        }
        public TriggerExpr draw(Rect position, GUIContent label, TriggerExpr expr)
        {
            Rect typePosition = new Rect(position.x + position.width - 40, position.y, 40, 16);
            Rect exprPosition;
            //绘制表达式
            if (expr == null)
            {
                exprPosition = new Rect(position.x, position.y, position.width - typePosition.width, 16);
                if (label != null)
                    EditorGUI.LabelField(exprPosition, label, new GUIContent("空值"));
                else
                    EditorGUI.LabelField(exprPosition, new GUIContent("空值"));
            }
            else
            {
                if (_drawer == null)
                    _drawer = TriggerExprSubDrawer.getExprDrawer(expr.GetType(), this, expr.transform, targetType, targetName);
                exprPosition = new Rect(position.x, position.y, position.width - typePosition.width, _drawer.height);
                _drawer.draw(exprPosition, label, expr);
            }
            //绘制类型
            GUIContent[] typeOptions = new GUIContent[] { new GUIContent("空值"), new GUIContent("常量"), new GUIContent("变量"), new GUIContent("函数") };
            int type = getExprType(expr);
            int newType = EditorGUI.Popup(typePosition, type, typeOptions);
            if (newType != type)
            {
                //摧毁旧的
                if (expr != null)
                    UnityEngine.Object.DestroyImmediate(expr.gameObject);
                //创建新的
                expr = createExprOfType(targetName, newType, targetType, transform);
                if (expr != null)
                    _drawer = TriggerExprSubDrawer.getExprDrawer(expr.GetType(), this, expr.transform, targetType, targetName);
                else
                    _drawer = null;
            }
            return expr;
        }
        TriggerExprSubDrawer _drawer;
        int getExprType(TriggerExpr expr)
        {
            if (expr != null)
            {
                if (expr.GetType().BaseType != null && expr.GetType().BaseType.IsGenericType && expr.GetType().BaseType.GetGenericTypeDefinition() == typeof(TriggerConst<>))
                    return 1;
                else if (expr is TriggerReflectFunc)
                    return 3;
                else
                    return 0;
            }
            else
                return 0;
        }
        TriggerExpr createExprOfType(string name, int exprType, Type targetType, Transform transform)
        {
            if (exprType == 1)
            {
                //常量
                TriggerConst constExpr = TriggerConst.getConstOfType(targetType);
                if (constExpr != null)
                {
                    constExpr.gameObject.name = name;
                    constExpr.transform.parent = transform;
                    return constExpr;
                }
                else
                {
                    Debug.LogWarning("TriggerSystem不支持" + targetType.Name + "类型的常量！");
                    return null;
                }
            }
            else if (exprType == 2)
            {
                //变量
                return null;
            }
            else if (exprType == 3)
            {
                //函数
                GameObject gameObject = new GameObject(name);
                gameObject.transform.parent = transform;
                return gameObject.AddComponent<TriggerReflectFunc>();
            }
            else
                return null;
        }
    }
}