using System;

namespace BJSYGameCore.AutoCompo
{
    public class AutoBindFieldInfo
    {
        public string path;
        public Type targetType;
        public int instanceId;
        public Type ctrlType;
        public string fieldName = null;
        public AutoBindFieldInfo(int instanceId, string path, Type targetType, Type ctrlType, string fieldName)
        {
            this.path = path;
            this.targetType = targetType;
            this.instanceId = instanceId;
            this.ctrlType = ctrlType;
            this.fieldName = fieldName;
        }
    }
}