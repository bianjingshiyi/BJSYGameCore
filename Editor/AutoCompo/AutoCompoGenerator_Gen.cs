using System;
using System.CodeDom;
using System.Linq;
using System.Reflection;
using Codo = BJSYGameCore.CodeDOMHelper;
namespace BJSYGameCore.AutoCompo
{
    partial class AutoCompoGenerator
    {
        protected void addTypeUsing(Type type)
        {
            if (string.IsNullOrEmpty(type.Namespace))
                return;
            if (_nameSpace.Imports.OfType<CodeNamespaceImport>().Any(n => n.Namespace == type.Namespace))
                return;
            _nameSpace.Imports.Add(new CodeNamespaceImport(type.Namespace));
        }
        protected CodeMemberField genField(CodeTypeDeclaration type, string typeName, string fieldName, bool applyAttributes = true)
        {
            CodeMemberField field = new CodeMemberField();
            type.Members.Add(field);
            if (applyAttributes)
            {
                foreach (var fieldAttName in _setting.fieldAttributes)
                {
                    field.CustomAttributes.Add(new CodeAttributeDeclaration(fieldAttName));
                }
            }
            field.Attributes = MemberAttributes.Private | MemberAttributes.Final;
            field.Type = new CodeTypeReference(typeName);
            field.Name = fieldName;
            return field;
        }
        protected CodeMemberField genField(CodeTypeDeclaration type, Type fieldType, string fieldName, bool applyAttributes = true)
        {
            addTypeUsing(fieldType);
            CodeMemberField field = new CodeMemberField();
            type.Members.Add(field);
            if (applyAttributes)
            {
                foreach (var fieldAttName in _setting.fieldAttributes)
                {
                    field.CustomAttributes.Add(new CodeAttributeDeclaration(fieldAttName));
                }
            }
            field.Attributes = MemberAttributes.Private | MemberAttributes.Final;
            field.Type = new CodeTypeReference(fieldType.Name);
            field.Name = fieldName;
            return field;
        }
        protected CodeMemberField genField(string typeName, string fieldName, bool applyAttributes = true)
        {
            return genField(_type, typeName, fieldName, applyAttributes);
        }
        protected CodeMemberField genField(Type fieldType, string fieldName, bool applyAttributes = true)
        {
            return genField(_type, fieldType, fieldName, applyAttributes);
        }
        protected CodeMemberProperty genProp(CodeTypeDeclaration type, MemberAttributes attributes, string typeName, string propName)
        {
            CodeMemberProperty prop = new CodeMemberProperty();
            type.Members.Add(prop);
            prop.Attributes = attributes;
            prop.Type = new CodeTypeReference(typeName);
            prop.Name = propName;
            return prop;
        }
        protected CodeMemberProperty genProp(CodeTypeDeclaration type, MemberAttributes attributes, Type propType, string propName)
        {
            addTypeUsing(propType);
            CodeMemberProperty prop = new CodeMemberProperty();
            type.Members.Add(prop);
            prop.Attributes = attributes;
            prop.Type = new CodeTypeReference(propType.Name);
            prop.Name = propName;
            return prop;
        }
        protected CodeMemberProperty genProp(MemberAttributes attributes, string propName, string typeName)
        {
            return genProp(_type, attributes, typeName, propName);
        }
        protected CodeMemberProperty genProp(MemberAttributes attributes, string propName, Type propType)
        {
            return genProp(_type, attributes, propType, propName);
        }
        protected CodeMemberMethod genMethod(CodeTypeDeclaration type, MemberAttributes attributes, string returnTypeName, string methodName)
        {
            CodeMemberMethod method = new CodeMemberMethod
            {
                Attributes = attributes,
                ReturnType = new CodeTypeReference(returnTypeName),
                Name = methodName
            };
            type.Members.Add(method);
            return method;
        }
        protected CodeMemberMethod genMethod(CodeTypeDeclaration type, MemberAttributes attributes, Type returnType, string methodName)
        {
            addTypeUsing(returnType);
            CodeMemberMethod method = new CodeMemberMethod
            {
                Attributes = attributes,
                ReturnType = new CodeTypeReference(returnType),
                Name = methodName
            };
            type.Members.Add(method);
            return method;
        }
        protected CodeMemberMethod genMethod(MemberAttributes attributes, Type returnType, string methodName)
        {
            return genMethod(_type, attributes, returnType, methodName);
        }
        protected CodeMemberMethod genMethod(MemberAttributes attributes, string returnTypeName, string methodName)
        {
            return genMethod(_type, attributes, returnTypeName, methodName);
        }
        protected CodeMemberEvent genEvent(string eventTypeName, string eventName, params CodeTypeReference[] typeParameters)
        {
            CodeMemberEvent Event = new CodeMemberEvent();
            _type.Members.Add(Event);
            Event.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            Event.Type = new CodeTypeReference(eventTypeName, typeParameters);
            Event.Name = eventName;
            return Event;
        }
        protected CodeMemberProperty genIndexer(CodeTypeReference indexerType)
        {
            CodeMemberProperty indexer = new CodeMemberProperty();
            _type.Members.Add(indexer);
            indexer.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            indexer.Type = indexerType;
            indexer.Name = "item";
            return indexer;
        }
        protected CodeMemberField genPartialMethod(string typeName, string methodName, params CodeParameterDeclarationExpression[] parameters)
        {
            return genPartialMethod(_type, typeName, methodName, parameters);
        }
        protected CodeMemberField genPartialMethod(CodeTypeDeclaration type, string typeName, string methodName, params CodeParameterDeclarationExpression[] parameters)
        {
            CodeMemberField method = new CodeMemberField();
            type.Members.Add(method);
            method.Attributes = MemberAttributes.ScopeMask;
            method.Type = new CodeTypeReference("partial " + typeName);
            method.Name = methodName + "(" + string.Join(",", parameters.Select(p => p.Type.BaseType + " " + p.Name).ToArray()) + ")";
            return method;
        }
        protected CodeMemberMethod genOverrrideMethod(MethodInfo methodInfo)
        {
            MemberAttributes attributes;
            if ((methodInfo.Attributes & MethodAttributes.FamANDAssem) == MethodAttributes.FamANDAssem)
                attributes = MemberAttributes.Public;
            else
                attributes = MemberAttributes.Family;
            CodeMemberMethod method = genMethod(attributes | MemberAttributes.Override, methodInfo.ReturnType, methodInfo.Name);
            method.Statements.Add(Codo.Base.getMethod(method.Name).invoke());
            return method;
        }
        protected CodeMemberMethod genOverrideAndPartialMethod(MethodInfo methodInfo, out CodeMemberField partialMethod)
        {
            CodeMemberMethod method = genOverrrideMethod(methodInfo);
            method.Statements.Add(Codo.This.getMethod(method.Name.headToLower()).invoke());
            string fixedTypeName;
            switch (methodInfo.ReturnType.Name)
            {
                case "Void":
                    fixedTypeName = "void";
                    break;
                default:
                    fixedTypeName = methodInfo.ReturnType.Name;
                    break;
            }
            partialMethod = genPartialMethod(fixedTypeName, method.Name.headToLower());
            return method;
        }
    }
}