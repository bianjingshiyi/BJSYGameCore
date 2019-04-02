using System;

namespace BJSYGameCore
{
    public class Parameter
    {
        public Type type { get; private set; }
        public string name { get; private set; }
        public Parameter(Type type, string name)
        {
            this.type = type;
            this.name = name;
        }
    }
}