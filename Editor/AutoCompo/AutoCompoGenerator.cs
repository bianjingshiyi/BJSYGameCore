using System;
using UnityEngine;
using System.CodeDom;
using System.Text.RegularExpressions;
using NUnit.Framework.Interfaces;
using System.Runtime.CompilerServices;

namespace BJSYGameCore.AutoCompo
{
    public class AutoCompoGenerator
    {
        /// <summary>
        /// 为游戏物体生成编译单元。
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        public CodeCompileUnit genScript4GO(GameObject gameObject, AutoCompoGenSetting setting)
        {
            _setting = setting;
            _rootGameObject = gameObject;
            CodeCompileUnit unit = new CodeCompileUnit();
            //命名空间，引用
            CodeNamespace nameSpace = new CodeNamespace(setting.Namespace);
            unit.Namespaces.Add(nameSpace);
            foreach (string import in setting.usings)
            {
                nameSpace.Imports.Add(new CodeNamespaceImport(import));
            }
            //类
            CodeTypeDeclaration type = new CodeTypeDeclaration();
            nameSpace.Types.Add(type);
            type.CustomAttributes.Add(new CodeAttributeDeclaration(nameof(AutoCompoAttribute),
                new CodeAttributeArgument(new CodePrimitiveExpression(gameObject.GetInstanceID()))));
            type.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            type.IsPartial = true;
            type.IsClass = true;
            type.Name = genTypeName4GO(gameObject);
            foreach (var baseType in setting.baseTypes)
            {
                type.BaseTypes.Add(baseType);
            }
            //自动绑定方法
            CodeMemberMethod autoBindMethod = new CodeMemberMethod();
            type.Members.Add(autoBindMethod);
            autoBindMethod.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            autoBindMethod.ReturnType = new CodeTypeReference("void");
            autoBindMethod.Name = "autoBind";
            //根物体组件引用
            string[] compoTypes;
            if (tryParseGOName(gameObject.name, out _, out compoTypes))
            {
                foreach (var compoTypeName in compoTypes)
                {
                    Component component = gameObject.GetComponent(compoTypeName);
                    if (component == null)
                        continue;
                    CodeMemberField field = new CodeMemberField();
                    type.Members.Add(field);
                    foreach (var fieldAttName in setting.fieldAttributes)
                    {
                        field.CustomAttributes.Add(new CodeAttributeDeclaration(fieldAttName));
                    }
                    field.Attributes = MemberAttributes.Private | MemberAttributes.Final;
                    field.Type = new CodeTypeReference(component.GetType().Name);
                    field.Name = genFieldName4RootCompo(component);
                    CodeMemberProperty prop = new CodeMemberProperty();
                    type.Members.Add(prop);
                    prop.Attributes = MemberAttributes.Public | MemberAttributes.Final;
                    prop.Type = new CodeTypeReference(component.GetType().Name);
                    prop.Name = genPropName4RootCompo(component);
                    prop.HasGet = true;
                    prop.GetStatements.Add(new CodeMethodReturnStatement(
                        new CodeFieldReferenceExpression(new CodeThisReferenceExpression(),
                        genFieldName4RootCompo(component))));
                    CodeAssignStatement assign = new CodeAssignStatement();
                    autoBindMethod.Statements.Add(assign);
                    assign.Left = new CodeFieldReferenceExpression(
                        new CodeThisReferenceExpression(), genFieldName4RootCompo(component));
                    assign.Right = new CodeMethodInvokeExpression(
                        new CodeThisReferenceExpression(), nameof(GameObject.GetComponent),
                        new CodePrimitiveExpression(component.GetType().Name));
                }
            }
            //处理子物体
            for (int i = 0; i < gameObject.transform.childCount; i++)
            {
                GameObject childGO = gameObject.transform.GetChild(i).gameObject;
                genChildGO(childGO);
            }
            return unit;
        }
        void genChildGO(GameObject gameObject)
        {

        }
        string genTypeName4GO(GameObject gameObject)
        {
            if (tryParseGOName(gameObject.name, out var typeName, out _))
                return typeName;
            else
                throw new FormatException(gameObject.name + "不符合格式\\w.\\w*");
        }
        bool tryParseGOName(string name, out string typeName, out string[] compoTypes)
        {
            var match = Regex.Match(name, @"(?<name>.+)\.(?<args>\w+(,\w+)*)");
            if (match.Success)
            {
                typeName = match.Groups["name"].Value;
                compoTypes = match.Groups["args"].Value.Split(',');
                return true;
            }
            else
            {
                typeName = string.Empty;
                compoTypes = new string[0];
                return false;
            }
        }
        string genFieldName4RootCompo(Component component)
        {
            return "_as" + component.GetType().Name;
        }
        string genPropName4RootCompo(Component component)
        {
            return "as" + component.GetType().Name;
        }
        AutoCompoGenSetting _setting;
        GameObject _rootGameObject;
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