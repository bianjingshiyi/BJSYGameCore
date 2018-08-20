namespace TBSGameCore
{
    public abstract class Method
    {
        public string displayName { get; private set; }
        public string displayDesc { get; private set; }
        public abstract string formatName { get; }
        public Method(string name, string desc)
        {
            displayName = name;
            displayDesc = desc;
        }
    }
}