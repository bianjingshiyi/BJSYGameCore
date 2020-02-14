using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using System.CodeDom;
using System.CodeDom.Compiler;

using UnityEditor;

namespace BJSYGameCore.UI
{
    class UGUIScriptGenerator
    {
        #region MenuItem
        [MenuItem("GameObject/Generate UGUI Script", true, 15)]
        public static bool validateGenerateScript()
        {
            if (Selection.gameObjects.Length < 1)
                return false;
            return true;
        }
        [MenuItem("GameObject/Generate UGUI Script", false, 15)]
        public static void generateScript()
        {
            generateScript(typeof(UIObject));
        }
        [MenuItem("GameObject/Generate UGUI Script As/List", true, 16)]
        public static bool validateGenerateScriptAsList()
        {
            if (!validateGenerateScript())
                return false;
            if (Selection.gameObjects.Length > 1)
                return false;
            if (Selection.gameObjects[0].GetComponent<LayoutGroup>() == null)
                return false;
            return true;
        }
        [MenuItem("GameObject/Generate UGUI Script As/List", false, 16)]
        public static void generateScriptAsList()
        {
            generateScript(typeof(UIList));
        }
        [MenuItem("GameObject/Generate UGUI Script As/PageGroup", true, 16)]
        public static bool validateGenerateScriptAsPageGroup()
        {
            if (!validateGenerateScript())
                return false;
            return true;
        }
        [MenuItem("GameObject/Generate UGUI Script As/PageGroup", false, 16)]
        public static void generateScriptAsPageGroup()
        {
            generateScript(typeof(UIPageGroup));
        }
        #endregion
        static UIScriptGeneratorPref pref { get; set; } = null;
        static List<GameObject> updatedGameObjectList { get; } = new List<GameObject>();
        static void generateScript(Type baseType)
        {
            pref = AssetDatabase.LoadAssetAtPath<UIScriptGeneratorPref>("Assets/Editor/UIScriptGeneratorPrefs.asset");
            if (pref == null)
            {
                pref = ScriptableObject.CreateInstance<UIScriptGeneratorPref>();
                AssetDatabase.CreateAsset(pref, "Assets/Editor/UIScriptGeneratorPrefs.asset");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            string dir = Directory.Exists(pref.lastDir) ? pref.lastDir : EditorUtility.OpenFolderPanel("将脚本文件保存至", "Assets", string.Empty);
            if (!string.IsNullOrEmpty(dir))
            {
                //保存上一次的地址
                pref.lastDir = dir.Replace(Environment.CurrentDirectory + "\\", string.Empty);
                updatedGameObjectList.Clear();
                foreach (GameObject gameObject in Selection.gameObjects)
                {
                    generateRoot(dir, gameObject, baseType);
                }
                AssetDatabase.Refresh();
            }
        }
        private static void generateRoot(string dir, GameObject rootGameObject, Type baseType, string fileName = null)
        {
            //确定文件名
            if (string.IsNullOrEmpty(fileName))
            {
                if (rootGameObject.name == "Canvas" && rootGameObject.GetComponent<Canvas>() != null)
                    fileName = rootGameObject.scene.name;
                else
                    fileName = rootGameObject.name;
                if (baseType == typeof(UIList) && !Regex.IsMatch(fileName, "List", RegexOptions.IgnoreCase))
                    fileName += "List";
            }
            //加载脚本文件
            string path = dir + "/" + fileName + ".cs";
            FileInfo fileInfo = new FileInfo(path);
            string rPath = fileInfo.FullName.Replace(Environment.CurrentDirectory + "\\", string.Empty);
            MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(rPath);
            Type scriptType = script != null ? script.GetClass() : null;
            //生成脚本文件
            using (StreamWriter writer = new StreamWriter(fileInfo.Create()))
            {
                CodeCompileUnit unit = new CodeCompileUnit();
                CodeNamespace codeNamespace = new CodeNamespace(pref.Namespace);
                {
                    codeNamespace.Imports.Add(new CodeNamespaceImport("UnityEngine"));
                    codeNamespace.Imports.Add(new CodeNamespaceImport("UnityEngine.UI"));
                    codeNamespace.Imports.Add(new CodeNamespaceImport("BJSYGameCore.UI"));
                    CodeTypeDeclaration codeType = new CodeTypeDeclaration(fileName);
                    {
                        codeType.Attributes = MemberAttributes.Private;
                        codeType.IsPartial = true;
                        codeType.BaseTypes.Add(baseType.Name);

                        CodeMemberMethod awake = new CodeMemberMethod();
                        {
                            awake.Attributes = MemberAttributes.Family | MemberAttributes.Final;
                            awake.ReturnType = new CodeTypeReference(typeof(void));
                            awake.Name = "Awake";
                        }
                        codeType.Members.Add(awake);

                        CodeMemberMethod autoBind = new CodeMemberMethod();
                        {
                            autoBind.Attributes = MemberAttributes.Public | MemberAttributes.Final;
                            autoBind.ReturnType = new CodeTypeReference(typeof(void));
                            autoBind.Name = "autoBind";
                        }
                        codeType.Members.Add(autoBind);

                        awake.Statements.Add(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "autoBind"));

                        if (rootGameObject.GetComponent<Button>() != null)
                            addAs(codeType, autoBind, rootGameObject, typeof(Button));
                        if (rootGameObject.GetComponent<Image>() != null)
                            addAs(codeType, autoBind, rootGameObject, typeof(Image));
                        if (rootGameObject.GetComponent<RawImage>() != null)
                            addAs(codeType, autoBind, rootGameObject, typeof(RawImage));
                        if (rootGameObject.GetComponent<Toggle>() != null)
                            addAs(codeType, autoBind, rootGameObject, typeof(Toggle));
                        if (rootGameObject.GetComponent<Slider>() != null)
                            addAs(codeType, autoBind, rootGameObject, typeof(Slider));
                        if (rootGameObject.GetComponent<Scrollbar>() != null)
                            addAs(codeType, autoBind, rootGameObject, typeof(Scrollbar));
                        if (rootGameObject.GetComponent<Dropdown>() != null)
                            addAs(codeType, autoBind, rootGameObject, typeof(Dropdown));
                        if (rootGameObject.GetComponent<InputField>() != null)
                            addAs(codeType, autoBind, rootGameObject, typeof(InputField));
                        if (rootGameObject.GetComponent<Canvas>() != null)
                            addAs(codeType, autoBind, rootGameObject, typeof(Canvas));
                        if (rootGameObject.GetComponent<ScrollRect>() != null)
                            addAs(codeType, autoBind, rootGameObject, typeof(ScrollRect));
                        if (rootGameObject.GetComponent<Text>() != null)
                            addAs(codeType, autoBind, rootGameObject, typeof(Text));
                        if (rootGameObject.GetComponent<VerticalLayoutGroup>() != null)
                            addAs(codeType, autoBind, rootGameObject, typeof(VerticalLayoutGroup));
                        if (rootGameObject.GetComponent<HorizontalLayoutGroup>() != null)
                            addAs(codeType, autoBind, rootGameObject, typeof(HorizontalLayoutGroup));
                        if (rootGameObject.GetComponent<GridLayoutGroup>() != null)
                            addAs(codeType, autoBind, rootGameObject, typeof(GridLayoutGroup));
                        if (baseType == typeof(UIList))
                        {
                            GameObject defaultItem = null;
                            for (int i = 0; i < rootGameObject.transform.childCount; i++)
                            {
                                if (rootGameObject.transform.GetChild(i).GetComponent<UIObject>() != null)
                                {
                                    defaultItem = rootGameObject.transform.GetChild(i).gameObject;
                                    break;
                                }
                            }
                            string itemTypeName;
                            if (defaultItem == null && rootGameObject.transform.childCount > 0)
                            {
                                itemTypeName = fileName + "Item";
                                defaultItem = rootGameObject.transform.GetChild(0).gameObject;
                                generateRoot(dir, defaultItem, typeof(UIObject), itemTypeName);
                            }
                            else
                            {
                                Type componentType = defaultItem.GetComponent<UIObject>().GetType();
                                generateRoot(dir, defaultItem, componentType.BaseType, componentType.Name);
                                itemTypeName = componentType.Name;
                            }
                            CodeMemberProperty defaultItemProp = new CodeMemberProperty();
                            {
                                defaultItemProp.Attributes = MemberAttributes.Public | MemberAttributes.New | MemberAttributes.Final;
                                defaultItemProp.Type = new CodeTypeReference(itemTypeName);
                                defaultItemProp.Name = nameof(UIList.defaultItem);
                                defaultItemProp.HasGet = true;
                                defaultItemProp.GetStatements.Add(new CodeConditionStatement(new CodeBinaryOperatorExpression(new CodeFieldReferenceExpression(new CodeBaseReferenceExpression(), nameof(UIList.defaultItem)), CodeBinaryOperatorType.ValueEquality, new CodePrimitiveExpression(null)),
                                    getPropAssignStatement(rootGameObject, defaultItem, itemTypeName, nameof(UIList.defaultItem))));
                                defaultItemProp.GetStatements.Add(new CodeMethodReturnStatement(new CodeCastExpression(itemTypeName, new CodePropertyReferenceExpression(new CodeBaseReferenceExpression(), nameof(UIList.defaultItem)))));
                                defaultItemProp.HasSet = true;
                                defaultItemProp.SetStatements.Add(new CodeAssignStatement(new CodePropertyReferenceExpression(new CodeBaseReferenceExpression(), nameof(UIList.defaultItem)), new CodeCastExpression(itemTypeName, new CodePropertySetValueReferenceExpression())));
                            }
                            codeType.Members.Add(defaultItemProp);
                            CodeMemberMethod addItemMethod = new CodeMemberMethod();
                            {
                                addItemMethod.Attributes = MemberAttributes.Public | MemberAttributes.New | MemberAttributes.Final;
                                addItemMethod.ReturnType = new CodeTypeReference(itemTypeName);
                                addItemMethod.Name = nameof(UIList.addItem);
                                addItemMethod.Statements.Add(new CodeMethodReturnStatement(new CodeCastExpression(itemTypeName, new CodeMethodInvokeExpression(new CodeBaseReferenceExpression(), nameof(UIList.addItem)))));
                            }
                            codeType.Members.Add(addItemMethod);
                            CodeMemberMethod getItemsMethod = new CodeMemberMethod();
                            {
                                getItemsMethod.Attributes = MemberAttributes.Public | MemberAttributes.New | MemberAttributes.Final;
                                getItemsMethod.ReturnType = new CodeTypeReference(itemTypeName, 1);
                                getItemsMethod.Name = nameof(UIList.getItems);
                                getItemsMethod.Statements.Add(new CodeMethodReturnStatement(new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), nameof(MonoBehaviour.GetComponentsInChildren), new CodeTypeReference(itemTypeName)))));
                            }
                            codeType.Members.Add(getItemsMethod);
                            CodeMemberMethod removeItemMethod = new CodeMemberMethod();
                            {
                                removeItemMethod.Attributes = MemberAttributes.Public | MemberAttributes.Final;
                                removeItemMethod.ReturnType = new CodeTypeReference(typeof(bool));
                                removeItemMethod.Name = nameof(UIList.removeItem);
                                removeItemMethod.Parameters.Add(new CodeParameterDeclarationExpression(itemTypeName, "item"));
                                removeItemMethod.Statements.Add(new CodeMethodReturnStatement(new CodeMethodInvokeExpression(new CodeBaseReferenceExpression(), nameof(UIList.removeItem), new CodeArgumentReferenceExpression("item"))));
                            }
                            codeType.Members.Add(removeItemMethod);
                            addAwakePropAssign(autoBind, rootGameObject, defaultItem, itemTypeName, nameof(UIList.defaultItem));
                            awake.Statements.Add(new CodeMethodInvokeExpression(new CodePropertyReferenceExpression(new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), nameof(UIList.defaultItem)), nameof(MonoBehaviour.gameObject)), nameof(GameObject.SetActive), new CodePrimitiveExpression(false)));
                            for (int i = 0; i < rootGameObject.transform.childCount; i++)
                            {
                                GameObject gameObject = rootGameObject.transform.GetChild(i).gameObject;
                                if (defaultItem != null && gameObject != defaultItem)
                                    generateChildMembers(codeType, autoBind, rootGameObject, gameObject);
                            }
                        }
                        else if (baseType == typeof(UIPageGroup))
                        {
                            List<CodeMemberProperty> childPropList = new List<CodeMemberProperty>();
                            for (int i = 0; i < rootGameObject.transform.childCount; i++)
                            {
                                GameObject gameObject = rootGameObject.transform.GetChild(i).gameObject;
                                childPropList.AddRange(generateChildMembers(codeType, autoBind, rootGameObject, gameObject));
                            }
                            CodeMemberMethod getPagesMethod = new CodeMemberMethod();
                            {
                                getPagesMethod.Attributes = MemberAttributes.Public | MemberAttributes.Override;
                                getPagesMethod.ReturnType = new CodeTypeReference(nameof(UIObject), 1);
                                getPagesMethod.Name = nameof(UIPageGroup.getPages);
                                getPagesMethod.Statements.Add(new CodeMethodReturnStatement(new CodeArrayCreateExpression(nameof(UIObject),
                                    childPropList.Select(p => new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), p.Name)).ToArray())));
                            }
                            codeType.Members.Add(getPagesMethod);
                        }
                        else
                        {
                            for (int i = 0; i < rootGameObject.transform.childCount; i++)
                            {
                                GameObject gameObject = rootGameObject.transform.GetChild(i).gameObject;
                                generateChildMembers(codeType, autoBind, rootGameObject, gameObject);
                            }
                        }
                        awake.Statements.Add(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "onAwake"));
                        CodeMemberField onAwakeMethod = new CodeMemberField();
                        {
                            onAwakeMethod.Attributes = MemberAttributes.Final;
                            onAwakeMethod.Type = new CodeTypeReference("partial void");
                            onAwakeMethod.Name = "onAwake()";
                        }
                        codeType.Members.Add(onAwakeMethod);
                    }
                    codeNamespace.Types.Add(codeType);
                }
                unit.Namespaces.Add(codeNamespace);

                CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");
                provider.GenerateCodeFromCompileUnit(unit, writer, new CodeGeneratorOptions()
                {
                    BlankLinesBetweenMembers = false,
                    BracingStyle = "C",
                    IndentString = "    ",
                    VerbatimOrder = true
                });
            }
            if (scriptType != null)
            {
                Component component = rootGameObject.GetComponent(scriptType);
                if (component == null)
                    component = rootGameObject.AddComponent(scriptType);
                component.GetType().GetMethod("autoBind").Invoke(component, new object[] { });
            }
            else
            {
                AddComponentWhenCompiledComponent addComponent = rootGameObject.AddComponent<AddComponentWhenCompiledComponent>();
                addComponent.path = rPath;
            }
        }
        private static CodeMemberProperty[] generateChildMembers(CodeTypeDeclaration codeType, CodeMemberMethod initMethod, GameObject rootGameObject, GameObject gameObject)
        {
            List<CodeMemberProperty> childPropList = new List<CodeMemberProperty>();
            if (gameObject.GetComponent<UIObject>() is UIObject uiObject)
            {
                return new CodeMemberProperty[] { generateChildComponent(codeType, initMethod, rootGameObject, gameObject, uiObject.GetType()) };
            }
            if (gameObject.GetComponent<Dropdown>() != null)
            {
                return new CodeMemberProperty[] { generateChildComponent(codeType, initMethod, rootGameObject, gameObject, typeof(Dropdown)) };
            }
            if (gameObject.GetComponent<Button>() != null)
            {
                return new CodeMemberProperty[] { generateChildComponent(codeType, initMethod, rootGameObject, gameObject, typeof(Button)) };
            }
            if (gameObject.GetComponent<ScrollRect>() is ScrollRect scrollRect)
            {
                childPropList.Add(generateChildComponent(codeType, initMethod, rootGameObject, gameObject, typeof(ScrollRect)));
                if (scrollRect.content != null)
                    childPropList.AddRange(generateChildMembers(codeType, initMethod, rootGameObject, scrollRect.content.gameObject));
                return childPropList.ToArray();
            }
            if (gameObject.GetComponent<RawImage>() != null)
                childPropList.Add(generateChildComponent(codeType, initMethod, rootGameObject, gameObject, typeof(RawImage)));
            if (gameObject.GetComponent<Image>() != null)
                childPropList.Add(generateChildComponent(codeType, initMethod, rootGameObject, gameObject, typeof(Image)));
            if (gameObject.GetComponent<Text>() != null)
                childPropList.Add(generateChildComponent(codeType, initMethod, rootGameObject, gameObject, typeof(Text)));
            if (gameObject.GetComponent<Toggle>() != null)
                childPropList.Add(generateChildComponent(codeType, initMethod, rootGameObject, gameObject, typeof(Toggle)));
            if (gameObject.GetComponent<Slider>() != null)
                childPropList.Add(generateChildComponent(codeType, initMethod, rootGameObject, gameObject, typeof(Slider)));
            if (gameObject.GetComponent<Scrollbar>() != null)
                childPropList.Add(generateChildComponent(codeType, initMethod, rootGameObject, gameObject, typeof(Scrollbar)));
            if (gameObject.GetComponent<InputField>() != null)
                childPropList.Add(generateChildComponent(codeType, initMethod, rootGameObject, gameObject, typeof(InputField)));
            if (gameObject.GetComponent<Canvas>() != null)
                childPropList.Add(generateChildComponent(codeType, initMethod, rootGameObject, gameObject, typeof(Canvas)));
            if (gameObject.GetComponent<VerticalLayoutGroup>() != null)
                childPropList.Add(generateChildComponent(codeType, initMethod, rootGameObject, gameObject, typeof(VerticalLayoutGroup)));
            if (gameObject.GetComponent<HorizontalLayoutGroup>() != null)
                childPropList.Add(generateChildComponent(codeType, initMethod, rootGameObject, gameObject, typeof(HorizontalLayoutGroup)));
            if (gameObject.GetComponent<GridLayoutGroup>() != null)
                childPropList.Add(generateChildComponent(codeType, initMethod, rootGameObject, gameObject, typeof(GridLayoutGroup)));
            for (int i = 0; i < gameObject.transform.childCount; i++)
            {
                GameObject childGameObject = gameObject.transform.GetChild(i).gameObject;
                childPropList.AddRange(generateChildMembers(codeType, initMethod, rootGameObject, childGameObject));
            }
            return childPropList.ToArray();
        }

        private static CodeMemberProperty generateChildComponent(CodeTypeDeclaration codeType, CodeMemberMethod initMethod, GameObject rootGameObject, GameObject gameObject, Type type)
        {
            string propName;
            string prefix = gameObject.name;//先从最短名称gameObject.name开始
            string suffix;//默认后缀为字段类型，但是有些太长了需要缩写
            if (type.BaseType == typeof(UIList))
                suffix = "List";
            else if (type == typeof(ScrollRect))
                suffix = "Scroll";
            else
                suffix = type.Name;
            Transform parent = gameObject.transform.parent;
            do
            {
                propName = prefix;//名称等于前缀+后缀（如果名称中已经有后缀则省略）
                if (!Regex.IsMatch(gameObject.name, suffix, RegexOptions.IgnoreCase))
                    propName += suffix;
                propName = removeInvalidChar(propName);
                if (codeType.Members.Cast<CodeTypeMember>().Any(m => m is CodeMemberField f && f.Name == "_" + propName))
                {
                    //短名称检查到重名
                    if (parent != null && parent != rootGameObject.transform)
                    {
                        //加长前缀
                        prefix = parent.gameObject.name + "_" + prefix;
                        parent = parent.parent;
                    }
                    else
                    {
                        //无法加长前缀，已经达到最长
                        if (Regex.Match(gameObject.name, @".*\((?<number>\d)\)") is Match m && m.Success)//是否有标序号
                        {
                            int number = int.Parse(m.Groups["number"].Value);
                            gameObject.name = gameObject.name.Remove(m.Groups["number"].Index - 1) + "(" + (number + 1) + ")";
                            prefix = prefix.Remove(prefix.Length - m.Groups["number"].Length, m.Groups["number"].Length) + (number + 1);
                        }
                        else//没有标
                        {
                            gameObject.name += " (1)";
                            prefix += "_1";
                        }
                        propName = prefix;
                        if (!Regex.IsMatch(gameObject.name, suffix, RegexOptions.IgnoreCase))
                            propName += suffix;
                        propName = removeInvalidChar(propName);
                    }
                }
                else
                    break;//没有重名，继续
            }
            while (true);
            CodeMemberField field = new CodeMemberField(type.Name, "_" + propName);
            {
                field.Attributes = MemberAttributes.Private;
                field.CustomAttributes.Add(new CodeAttributeDeclaration(nameof(SerializeField)));
            }
            codeType.Members.Add(field);
            CodeMemberProperty prop = new CodeMemberProperty();
            {
                prop.UserData.Add("GameObject", gameObject);
                prop.Attributes = MemberAttributes.Public | MemberAttributes.Final;
                prop.Type = new CodeTypeReference(type.Name);
                prop.Name = propName;
                prop.HasGet = true;
                prop.GetStatements.Add(new CodeConditionStatement(new CodeBinaryOperatorExpression(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "_" + propName), CodeBinaryOperatorType.ValueEquality, new CodePrimitiveExpression(null)), new CodeStatement[]
                {
                    getFieldAssignStatement(rootGameObject, gameObject, type.Name, "_" + propName)
                }));
                prop.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "_" + propName)));
            }
            codeType.Members.Add(prop);
            addAwakeFieldAssign(initMethod, rootGameObject, gameObject, type.Name, propName);
            return prop;
        }

        private static void addAwakeFieldAssign(CodeMemberMethod initMethod, GameObject rootGameObject, GameObject gameObject, string typeName, string propName)
        {
            CodeAssignStatement assign = getFieldAssignStatement(rootGameObject, gameObject, typeName, "_" + propName);
            initMethod.Statements.Add(assign);
        }
        private static CodeAssignStatement getFieldAssignStatement(GameObject rootGameObject, GameObject gameObject, string typeName, string fieldName)
        {
            CodeMethodInvokeExpression findExpr = getFindExpr(rootGameObject, gameObject);
            CodeAssignStatement assign = new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), fieldName), new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(findExpr, "GetComponent", new CodeTypeReference(typeName))));
            return assign;
        }
        private static CodeAssignStatement getPropAssignStatement(GameObject rootGameObject, GameObject gameObject, string typeName, string propName)
        {
            CodeMethodInvokeExpression findExpr = getFindExpr(rootGameObject, gameObject);
            CodeAssignStatement assign = new CodeAssignStatement(new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), propName), new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(findExpr, "GetComponent", new CodeTypeReference(typeName))));
            return assign;
        }
        private static void addAwakePropAssign(CodeMemberMethod initMethod, GameObject rootGameObject, GameObject gameObject, Type type, string propName)
        {
            addAwakePropAssign(initMethod, rootGameObject, gameObject, type.Name, propName);
        }
        private static void addAwakePropAssign(CodeMemberMethod initMethod, GameObject rootGameObject, GameObject gameObject, string typeName, string propName)
        {
            CodeMethodInvokeExpression findExpr = getFindExpr(rootGameObject, gameObject);
            initMethod.Statements.Add(new CodeAssignStatement(new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), propName), new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(findExpr, "GetComponent", new CodeTypeReference(typeName)))));
        }
        private static CodeMethodInvokeExpression getFindExpr(GameObject rootGameObject, GameObject gameObject)
        {
            CodeMethodInvokeExpression findExprHead = new CodeMethodInvokeExpression(null, "Find", new CodePrimitiveExpression(gameObject.name));
            CodeMethodInvokeExpression findExpr = findExprHead;
            for (Transform parent = gameObject.transform.parent; parent != null && parent != rootGameObject.transform; parent = parent.parent)
            {
                findExprHead.Method.TargetObject = new CodeMethodInvokeExpression(null, "Find", new CodePrimitiveExpression(parent.gameObject.name));
                findExprHead = findExprHead.Method.TargetObject as CodeMethodInvokeExpression;
            }
            findExprHead.Method.TargetObject = new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "transform");
            return findExpr;
        }

        static string removeInvalidChar(string str)
        {
            str = str.Replace(' ', '_');
            for (int i = 0; i < str.Length; i++)
            {
                if (!char.IsLetterOrDigit(str[i]) && str[i] != '_')
                {
                    str = str.Remove(i, 1);
                    i--;
                }
            }
            return str;
        }
        static void addAs(CodeTypeDeclaration codeType, CodeMemberMethod awake, GameObject rootGameObject, Type type)
        {
            CodeMemberField field = new CodeMemberField(type.Name, "m_as_" + type.Name);
            {
                field.Attributes = MemberAttributes.Private;
                field.CustomAttributes.Add(new CodeAttributeDeclaration(nameof(SerializeField)));
            }
            codeType.Members.Add(field);
            CodeMemberProperty prop = new CodeMemberProperty();
            {
                prop.Attributes = MemberAttributes.Public | MemberAttributes.Final;
                prop.Type = new CodeTypeReference(type.Name);
                prop.Name = "as" + type.Name;
                prop.HasGet = true;
                prop.GetStatements.Add(new CodeConditionStatement(new CodeBinaryOperatorExpression(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "m_as_" + type.Name), CodeBinaryOperatorType.ValueEquality, new CodePrimitiveExpression(null)),
                    getFieldAssignStatement(rootGameObject, rootGameObject, type.Name, "m_as_" + type.Name)));
                prop.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "m_as_" + type.Name)));
            }
            codeType.Members.Add(prop);
            awake.Statements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "m_as_" + type.Name), new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), "GetComponent", new CodeTypeReference(type.Name)))));
        }
    }
}