using System;
using System.Collections.Generic;

namespace BJSYGameCore.AutoCompo
{
    public class AutoBindFieldInfo
    {
        public string path;
        public Type targetType;
        public int instanceId;

        [Obsolete("这并不是一个必须的属性，应当保存在propDict中。")]
        public Type ctrlType
        {
            get
            {
                object value;
                if (propDict.TryGetValue("ctrlType", out value))
                    return value as Type;
                return null;
            }
            set { propDict["ctrlType"] = value; }
        }
        public string fieldName;
        public readonly Dictionary<string, object> propDict = new Dictionary<string, object>();
        readonly bool _isGenerated;
        public bool isGenerated
        {
            get { return instanceId != 0 || _isGenerated; }
        }
        public AutoBindFieldInfo(int instanceId, string path, Type targetType, Type ctrlType, string fieldName)
        {
            this.path = path;
            this.targetType = targetType;
            this.instanceId = instanceId;
            if (ctrlType != null)
                this.ctrlType = ctrlType;
            this.fieldName = fieldName;
        }
        public AutoBindFieldInfo(bool isGenerated, string path, Type targetType, Type ctrlType, string fieldName)
        {
            _isGenerated = isGenerated;
            this.path = path;
            this.targetType = targetType;
            if (ctrlType != null)
                this.ctrlType = ctrlType;
            this.fieldName = fieldName;
        }
        public T getValueOrDefault<T>(string key)
        {
            if (propDict.ContainsKey(key) && propDict[key] is T)
                return (T)propDict[key];
            else
                return default(T);
        }
    }
}