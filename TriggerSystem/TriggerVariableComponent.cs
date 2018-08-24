namespace TBSGameCore.TriggerSystem
{
    public class TriggerVariableComponent : TriggerExprComponent
    {
        public new string name
        {
            get { return gameObject.name; }
        }
        public void setName(string name)
        {
            gameObject.name = name;
        }
        public override TriggerExprDefine define
        {
            get { return scope.getVariable(name); }
        }
        public override string desc
        {
            get { return name; }
        }
        public override object invoke()
        {
            return scope.getVariableValue(name);
        }
    }
}
