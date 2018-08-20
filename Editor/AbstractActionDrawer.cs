using UnityEngine;

namespace TBSGameCore
{
    public abstract class AbstractActionDrawer : TriggerStringDrawer
    {
        public AbstractActionDrawer(UnityEngine.Object targetObject) : base(targetObject)
        {
        }
        public AbstractActionDrawer(TriggerStringDrawer parent) : base(parent)
        {
        }
        public abstract string draw(Rect position, string value, Method action);
        public static AbstractActionDrawer factory(string formatName, TriggerStringDrawer parent)
        {
            if (formatName == "if")
                return new IfActionDrawer(parent);
            else
                return new ReflectedActionDrawer(parent);
        }
    }
}