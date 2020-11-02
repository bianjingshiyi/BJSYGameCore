using System;
using UnityEngine;
using System.CodeDom;

namespace BJSYGameCore.AutoCompo
{
    public class AutoCompoGenerator
    {
        /// <summary>
        /// 为游戏物体生成编译单元。
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        public static CodeCompileUnit genScript4GO(GameObject gameObject, AutoCompoGenSetting setting)
        {
            throw new NotImplementedException();
        }
    }
    [Serializable]
    public class AutoCompoGenSetting
    {
        public string[] usings;
        public string Namespace;
        public string[] baseTypes;
        public string[] fieldAttributes;
    }
}