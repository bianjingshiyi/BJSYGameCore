using System;

using UnityEngine;
using UnityEditor;

namespace BJSYGameCore
{
    public class TypedFuncStringDrawer : TriggerStringDrawer
    {
        public TypedFuncStringDrawer(UnityEngine.Object targetObject) : base(targetObject)
        {
        }
        public TypedFuncStringDrawer(TriggerStringDrawer parent) : base(parent)
        {
        }
        AbstractFuncStringDrawer _drawer = null;
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
        public string draw(Rect position, GUIContent label, string value, Type returnType)
        {
            //切割字符串得到类型与真正的值
            int type = 0;
            string realValue = null;
            if (!string.IsNullOrEmpty(value))
            {
                if (value[0] == '0')
                    type = 0;
                else if (value[0] == '1')
                    type = 1;
                else if (value[0] == '2')
                    type = 2;
                if (value.Length > 1)
                    realValue = value.Substring(1, value.Length - 1);
            }
            //绘制类型GUI，改变类型
            Rect typePosition = new Rect(position.x + position.width - 48, position.y, 48, 16);
            int newType = EditorGUI.Popup(typePosition, type, new string[] { "常量", "变量", "表达式" });
            if (newType != type)
            {
                type = newType;
                realValue = null;
                _drawer = AbstractFuncStringDrawer.factory(type.ToString(), this);
            }
            //自动生成Drawer
            if (_drawer == null)
                _drawer = AbstractFuncStringDrawer.factory(type.ToString(), this);
            if (_drawer != null)
            {
                //绘制值GUI，改变值
                Rect valuePosition = new Rect(position.x, position.y, position.width - 48, _drawer.height);
                realValue = _drawer.draw(valuePosition, label, realValue, returnType);
            }
            //重新组合类型和真值得到值
            return type.ToString() + realValue;
        }
    }
}