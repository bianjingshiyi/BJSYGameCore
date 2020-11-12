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
            CodeCompileUnit unit = new CodeCompileUnit();
            //命名空间，引用
            CodeNamespace nameSpace = new CodeNamespace(setting.Namespace);
            unit.Namespaces.Add(nameSpace);
            foreach (string us in setting.usings)
            {
                CodeNamespaceImport import = new CodeNamespaceImport(us);
                nameSpace.Imports.Add(import);
            }
            //类
            CodeTypeDeclaration Class = new CodeTypeDeclaration(gameObject.name)
            {
                IsClass = true
            };
            nameSpace.Types.Add(Class);
            foreach (string bt in setting.baseTypes)
            {
                Class.BaseTypes.Add(new CodeTypeReference(bt));
            }
            
            foreach(var com in gameObject.GetComponents<Component>())
            {
                CodeMemberField field = new CodeMemberField(com.GetType(), com.name);
                field.CustomAttributes.Add(new CodeAttributeDeclaration("SerializeField"));
                field.Attributes = MemberAttributes.Private | MemberAttributes.Final;               
                Class.Members.Add(field);

                CodeMemberProperty property = new CodeMemberProperty();
                
            }
            
            
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