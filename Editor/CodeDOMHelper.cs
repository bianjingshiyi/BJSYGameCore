using System.IO;
using System.CodeDom;
using System.CodeDom.Compiler;
using System;
using System.Linq;
namespace BJSYGameCore
{
    static class CodeDOMHelper
    {
        public static void writeUnitToFile(FileInfo fileInfo, CodeCompileUnit unit)
        {
            using (StreamWriter writer = new StreamWriter(fileInfo.Create()))
            {
                using (CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp"))
                {
                    provider.GenerateCodeFromCompileUnit(unit, writer, new CodeGeneratorOptions()
                    {
                        BlankLinesBetweenMembers = false,
                        BracingStyle = "C",
                        IndentString = "    ",
                        VerbatimOrder = true
                    });
                }
            }
        }
        public static CodeFieldReferenceExpression getField(this CodeExpression target, string fieldName)
        {
            return new CodeFieldReferenceExpression(target, fieldName);
        }
        public static CodePropertyReferenceExpression getProp(this CodeExpression target, string propName)
        {
            return new CodePropertyReferenceExpression(target, propName);
        }
        public static CodeMethodReferenceExpression getMethod(this CodeExpression target, string methodName, params CodeTypeReference[] typeParameters)
        {
            return new CodeMethodReferenceExpression(target, methodName, typeParameters);
        }
        public static CodeEventReferenceExpression getEvent(this CodeExpression target, string eventName)
        {
            return new CodeEventReferenceExpression(target, eventName);
        }
        public static CodeMethodInvokeExpression invoke(this CodeMethodReferenceExpression method, params CodeExpression[] parameters)
        {
            return new CodeMethodInvokeExpression(method, parameters);
        }
        public static CodeDelegateInvokeExpression invoke(this CodeEventReferenceExpression Event, params CodeExpression[] parameters)
        {
            return new CodeDelegateInvokeExpression(Event, parameters);
        }
        public static CodeExpressionStatement statement(this CodeExpression expr)
        {
            return new CodeExpressionStatement(expr);
        }
        public static CodeTypeReference type(string typeName)
        {
            return new CodeTypeReference(typeName);
        }
        public static CodeTypeReferenceExpression expr(this CodeTypeReference type)
        {
            return new CodeTypeReferenceExpression(type);
        }
        public static CodeAssignStatement assign(this CodeExpression left, CodeExpression right)
        {
            return new CodeAssignStatement(left, right);
        }
        public static CodeParameterDeclarationExpressionCollection append(this CodeParameterDeclarationExpressionCollection collection, string typeName, string name)
        {
            collection.Add(new CodeParameterDeclarationExpression(typeName, name));
            return collection;
        }
        public static CodeParameterDeclarationExpressionCollection append(this CodeParameterDeclarationExpressionCollection collection, Type type, string name)
        {
            collection.Add(new CodeParameterDeclarationExpression(type, name));
            return collection;
        }
        public static CodeExpressionCollection append(this CodeExpressionCollection collection, CodeExpression expression)
        {
            collection.Add(expression);
            return collection;
        }
        public static CodeExpressionCollection appendArg(this CodeExpressionCollection collection, string name)
        {
            collection.Add(new CodeArgumentReferenceExpression(name));
            return collection;
        }
        public static CodeObjectCreateExpression New(string typeName, params CodeExpression[] parameters)
        {
            return new CodeObjectCreateExpression(typeName, parameters);
        }
        public static CodeVariableReferenceExpression Var(string name)
        {
            return new CodeVariableReferenceExpression(name);
        }
        public static CodeAttachEventStatement attach(this CodeEventReferenceExpression Event, CodeExpression expression)
        {
            return new CodeAttachEventStatement(Event, expression);
        }
        public static CodeRemoveEventStatement remove(this CodeEventReferenceExpression Event, CodeExpression expression)
        {
            return new CodeRemoveEventStatement(Event, expression);
        }
        public static CodeStatementCollection append(this CodeStatementCollection collection, CodeStatement statement)
        {
            collection.Add(statement);
            return collection;
        }
        public static CodeStatementCollection append(this CodeStatementCollection collection, CodeExpression expression)
        {
            collection.Add(expression);
            return collection;
        }
        public static CodeArgumentReferenceExpression arg(string name)
        {
            return new CodeArgumentReferenceExpression(name);
        }
        public static CodeParameterDeclarationExpression parameter(string typeName, string name)
        {
            return new CodeParameterDeclarationExpression(typeName, name);
        }
        public static CodeMemberMethod appendParam(this CodeMemberMethod method, string typeName, string name)
        {
            method.Parameters.Add(new CodeParameterDeclarationExpression(typeName, name));
            return method;
        }
        public static CodeMemberMethod appendStatement(this CodeMemberMethod method, CodeStatement statement)
        {
            method.Statements.Add(statement);
            return method;
        }
        public static CodeMemberMethod appendStatement(this CodeMemberMethod method, CodeExpression expression)
        {
            method.Statements.Add(expression);
            return method;
        }
        public static CodeMethodReturnStatement Return(CodeExpression expression)
        {
            return new CodeMethodReturnStatement(expression);
        }
        public static CodeIndexerExpression index(this CodeExpression target, params CodeExpression[] parameters)
        {
            return new CodeIndexerExpression(target, parameters);
        }
        public static CodeVariableDeclarationStatement decVar(string typeName, string name, CodeExpression initExpr = null)
        {
            if (initExpr == null)
                return new CodeVariableDeclarationStatement(typeName, name);
            else
                return new CodeVariableDeclarationStatement(typeName, name, initExpr);
        }
        public static CodePrimitiveExpression String(string str)
        {
            return new CodePrimitiveExpression(str);
        }
        public static CodeCastExpression cast(this CodeExpression expression, string typeName)
        {
            return new CodeCastExpression(typeName, expression);
        }
        public static CodeConditionStatement If(CodeExpression condition)
        {
            return new CodeConditionStatement(condition);
        }
        public static CodeConditionStatement appendTrue(this CodeConditionStatement If, CodeStatement statement)
        {
            If.TrueStatements.Add(statement);
            return If;
        }
        public static CodeConditionStatement appendFalse(this CodeConditionStatement If, CodeStatement statement)
        {
            If.FalseStatements.Add(statement);
            return If;
        }
        public static CodeBinaryOperatorExpression op(this CodeExpression left, CodeBinaryOperatorType op, CodeExpression right)
        {
            return new CodeBinaryOperatorExpression(left, op, right);
        }
        public static CodePrimitiveExpression Int(int value)
        {
            return new CodePrimitiveExpression(value);
        }
        public static CodeArrayCreateExpression createArray(string typeName, int size)
        {
            return new CodeArrayCreateExpression(typeName, size);
        }
        public static CodeAttributeDeclarationCollection append(this CodeAttributeDeclarationCollection collection, string name, params CodeExpression[] arguements)
        {
            collection.Add(new CodeAttributeDeclaration(name, arguements.Select(e => new CodeAttributeArgument(e)).ToArray()));
            return collection;
        }
        public static readonly CodeThisReferenceExpression This = new CodeThisReferenceExpression();
        public static readonly CodeBaseReferenceExpression Base = new CodeBaseReferenceExpression();
        public static readonly CodePrimitiveExpression Null = new CodePrimitiveExpression(null);
        public static readonly CodePrimitiveExpression False = new CodePrimitiveExpression(false);
        public static readonly CodePrimitiveExpression True = new CodePrimitiveExpression(true);
    }
    public static class ArrayHelper
    {
        public static int indexOf(this Array array, object obj)
        {
            for (int i = 0; i < array.Length; i++)
            {
                if (array.GetValue(i) == obj)
                    return i;
            }
            return -1;
        }
    }
    public static class StringHelper
    {
        public static string headToUpper(this string str)
        {
            if (char.IsLower(str[0]))
                str = char.ToUpper(str[0]) + str.Substring(1, str.Length - 1);
            return str;
        }
        public static string headToLower(this string str)
        {
            if (char.IsUpper(str[0]))
                str = char.ToLower(str[0]) + str.Substring(1, str.Length - 1);
            return str;
        }
        public static bool tryMerge(this string head, string rear, out string merged)
        {
            for (int i = 0; i < head.Length; i++)
            {
                string middle = head.Substring(i, head.Length - i);
                if (rear.StartsWith(middle))
                {
                    merged = head.Substring(0, head.Length - middle.Length) + rear;
                    return true;
                }
            }
            merged = string.Empty;
            return false;
        }
    }
}