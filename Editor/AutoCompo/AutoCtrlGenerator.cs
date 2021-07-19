using System;
using System.Linq;
using System.CodeDom;
using System.Reflection;
using System.Collections.Generic;
using Codo = BJSYGameCore.CodeDOMHelper;
// ReSharper disable SuggestVarOrType_SimpleTypes
namespace BJSYGameCore.AutoCompo
{
    public class AutoCtrlGenerator
    {
        public CodeCompileUnit genCtrlUnit(string namespaceName, string typeName, Type mainCtrlType, Type compoType, KeyValuePair<string, Type>[] childCtrlInfos)
        {
            CodeCompileUnit unit = new CodeCompileUnit();
            //命名空间
            CodeNamespace nameSpace = new CodeNamespace(namespaceName);
            unit.Namespaces.Add(nameSpace);
            //类型
            CodeTypeDeclaration type = new CodeTypeDeclaration();
            nameSpace.Types.Add(type);
            type.Attributes = MemberAttributes.Final;
            type.IsPartial = true;
            type.IsClass = true;
            type.Name = typeName;
            nameSpace.addTypeUsing(typeof(IController<>));
            nameSpace.addTypeUsing(mainCtrlType);
            type.BaseTypes.Add(Codo.type("IController", Codo.type(mainCtrlType.Name)));
            //成员
            //无参构造器
            CodeConstructor constructor = new CodeConstructor();
            type.Members.Add(constructor);
            constructor.Attributes = MemberAttributes.Public;
            //有参构造器
            constructor = new CodeConstructor();
            type.Members.Add(constructor);
            constructor.Attributes = MemberAttributes.Public;
            nameSpace.addTypeUsing(typeof(IAppManager));
            constructor.Parameters.Add(Codo.parameter(typeof(IAppManager).Name, "app"));
            constructor.Parameters.Add(Codo.parameter(mainCtrlType.Name, "main"));
            constructor.Parameters.Add(Codo.parameter(compoType.Name, "compo"));
            constructor.Statements
                .append(Codo.This.getField("app").assign(Codo.arg("app")))
                .append(Codo.This.getField("main").assign(Codo.arg("main")))
                .append(Codo.getField("_compo").assign(Codo.arg("compo")));
            CodeMemberField mainField = new CodeMemberField();
            type.Members.Add(mainField);
            mainField.Attributes = MemberAttributes.Private | MemberAttributes.Final;
            mainField.Type = Codo.type(mainCtrlType.Name);
            mainField.Name = "_main";
            CodeMemberProperty mainProp = new CodeMemberProperty();
            type.Members.Add(mainProp);
            mainProp.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            mainProp.Type = Codo.type(mainCtrlType.Name);
            mainProp.Name = "main";
            mainProp.HasGet = true;
            mainProp.GetStatements.append(Codo.Return(Codo.getField(mainField.Name)));
            CodeMemberField appField = new CodeMemberField();
            type.Members.Add(appField);
            appField.Attributes = MemberAttributes.Private | MemberAttributes.Final;
            appField.Type = Codo.type(typeof(IAppManager).Name);
            appField.Name = "_app";
            CodeMemberProperty appProp = new CodeMemberProperty();
            type.Members.Add(appProp);
            appProp.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            appProp.Type = Codo.type(typeof(IAppManager).Name);
            appProp.Name = "app";
            appProp.HasGet = true;
            appProp.GetStatements.append(Codo.Return(Codo.getField(appField.Name)));
            CodeMemberField compoField = new CodeMemberField();
            type.Members.Add(compoField);
            compoField.Attributes = MemberAttributes.Private | MemberAttributes.Final;
            compoField.Type = Codo.type(compoType.Name);
            compoField.Name = "_compo";
            //子控制器
            foreach (var childCtrlInfo in childCtrlInfos)
            {
                string childCtrlPath = childCtrlInfo.Key;
                Type childCtrlType = childCtrlInfo.Value;
                CodeMemberField ctrlField = new CodeMemberField();
                type.Members.Add(ctrlField);
                ctrlField.Attributes = MemberAttributes.Private | MemberAttributes.Final;
                nameSpace.addTypeUsing(childCtrlType);
                ctrlField.Type = Codo.type(childCtrlType.Name);
                ctrlField.Name = "_" + childCtrlType.Name.headToLower();
                CodeMemberField pathField = new CodeMemberField();
                type.Members.Add(pathField);
                pathField.Attributes = MemberAttributes.Public | MemberAttributes.Final;
                pathField.Type = Codo.type("const string");
                pathField.Name = "PATH_" + childCtrlType.Name.ToUpper();
                pathField.InitExpression = Codo.String(childCtrlPath);
                CodeMemberProperty ctrlProp = new CodeMemberProperty();
                type.Members.Add(ctrlProp);
                ctrlProp.Attributes = MemberAttributes.Public | MemberAttributes.Final;
                ctrlProp.Type = Codo.type(childCtrlType.Name);
                ctrlProp.Name = childCtrlType.Name.headToLower();
                ctrlProp.HasGet = true;
                ctrlProp.GetStatements
                    .append(Codo.If(Codo.getField(ctrlField.Name).op(CodeBinaryOperatorType.ValueEquality, Codo.Null))
                        .appendTrue(Codo.getField(ctrlField.Name).assign(Codo.New(childCtrlType.Name, Codo.getProp("app"), Codo.getProp("main"),
                            Codo.getField("_compo").getProp("transform").getMethod("Find").invoke(Codo.getField(pathField.Name)).getMethod("GetComponent", Codo.type(childCtrlType.Name)).invoke()))))
                    .append(Codo.Return(Codo.getField(ctrlField.Name)));
            }
            return unit;
        }
        public KeyValuePair<string, Type>[] getChildCtrlInfosFromType(Type ctrlType)
        {
            //获取类型IController接口的总控泛型类型
            Type interfaceType = ctrlType.GetInterfaces().First(i => i.GetGenericTypeDefinition() == typeof(IController<>));
            Type mainCtrlType = interfaceType.GetGenericArguments()[0];
            //通过无参构造器创建类型实例
            ConstructorInfo constructor = ctrlType.GetConstructor(new Type[0]);
            if (constructor == null)
                throw new InvalidOperationException("无法创建" + ctrlType.Name + "的实例，因为它不具有一个无参的构造函数");
            object obj = constructor.Invoke(new object[0]);
            //查找路径字段
            List<KeyValuePair<string, Type>> ctrlInfoList = new List<KeyValuePair<string, Type>>();
            foreach (var field in ctrlType.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                if (field.FieldType == typeof(string) && field.Name.StartsWith("PATH_"))
                {
                    string childCtrlName = field.Name.Substring(5, field.Name.Length - 5);
                    string path = (string)field.GetValue(obj);
                    //得到对应控件字段生成信息
                    var ctrlField = ctrlType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                        .FirstOrDefault(f => f.Name.ToUpper().EndsWith(childCtrlName));
                    ctrlInfoList.Add(new KeyValuePair<string, Type>(path, ctrlField.FieldType));
                }
            }
            return ctrlInfoList.ToArray();
        }
    }
}