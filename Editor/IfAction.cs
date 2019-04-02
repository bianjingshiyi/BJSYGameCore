namespace BJSYGameCore
{
    public class IfAction : Method
    {
        public override string formatName
        {
            get { return "if"; }
        }
        public IfAction() : base("常规/条件", "如果-则-否则")
        {
        }
    }
}