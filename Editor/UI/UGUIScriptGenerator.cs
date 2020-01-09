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
            UIScriptGeneratorPref pref = AssetDatabase.LoadAssetAtPath<UIScriptGeneratorPref>("Assets/Editor/UIScriptGeneratorPrefs.asset");
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
                foreach (GameObject gameObject in Selection.gameObjects)
                {
                    generateRoot(pref, dir, gameObject);
                }
            }
        }

        private static void generateRoot(UIScriptGeneratorPref pref, string dir, GameObject rootGameObject)
        {
            string path = dir + "/" + rootGameObject.name + ".cs";
            FileInfo fileInfo = new FileInfo(path);
            //生成脚本文件
            using (StreamWriter writer = new StreamWriter(fileInfo.Create()))
            {
                CodeCompileUnit unit = new CodeCompileUnit();
                CodeNamespace codeNamespace = new CodeNamespace(pref.Namespace);
                {
                    codeNamespace.Imports.Add(new CodeNamespaceImport("UnityEngine"));
                    codeNamespace.Imports.Add(new CodeNamespaceImport("UnityEngine.UI"));
                    CodeTypeDeclaration codeType = new CodeTypeDeclaration(fileInfo.Name.Substring(0, fileInfo.Name.Length - 3));
                    {
                        codeType.Attributes = MemberAttributes.Private;
                        codeType.IsPartial = true;
                        codeType.BaseTypes.Add(nameof(UIObject));

                        CodeMemberMethod awake = new CodeMemberMethod();
                        {
                            awake.Attributes = MemberAttributes.Family;
                            awake.ReturnType = new CodeTypeReference(typeof(void));
                            awake.Name = "Awake";
                        }
                        codeType.Members.Add(awake);

                        if (rootGameObject.GetComponent<Button>() != null)
                            addAs(codeType, awake, typeof(Button));
                        if (rootGameObject.GetComponent<Image>() != null)
                            addAs(codeType, awake, typeof(Image));
                        if (rootGameObject.GetComponent<RawImage>() != null)
                            addAs(codeType, awake, typeof(RawImage));
                        if (rootGameObject.GetComponent<Toggle>() != null)
                            addAs(codeType, awake, typeof(Toggle));
                        if (rootGameObject.GetComponent<Slider>() != null)
                            addAs(codeType, awake, typeof(Slider));
                        if (rootGameObject.GetComponent<Scrollbar>() != null)
                            addAs(codeType, awake, typeof(Scrollbar));
                        if (rootGameObject.GetComponent<Dropdown>() != null)
                            addAs(codeType, awake, typeof(Dropdown));
                        if (rootGameObject.GetComponent<InputField>() != null)
                            addAs(codeType, awake, typeof(InputField));
                        if (rootGameObject.GetComponent<Canvas>() != null)
                            addAs(codeType, awake, typeof(Canvas));
                        if (rootGameObject.GetComponent<ScrollRect>() != null)
                            addAs(codeType, awake, typeof(ScrollRect));
                        if (rootGameObject.GetComponent<Text>() != null)
                            addAs(codeType, awake, typeof(Text));
                        if (rootGameObject.GetComponent<VerticalLayoutGroup>() != null)
                            addAs(codeType, awake, typeof(VerticalLayoutGroup));
                        if (rootGameObject.GetComponent<HorizontalLayoutGroup>() != null)
                            addAs(codeType, awake, typeof(HorizontalLayoutGroup));
                        if (rootGameObject.GetComponent<GridLayoutGroup>() != null)
                            addAs(codeType, awake, typeof(GridLayoutGroup));
                        for (int i = 0; i < rootGameObject.transform.childCount; i++)
                        {
                            GameObject gameObject = rootGameObject.transform.GetChild(i).gameObject;
                            generateChildMembers(codeType, awake, rootGameObject, gameObject);
                        }
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
            AssetDatabase.Refresh();
            string rPath = fileInfo.FullName.Replace(Environment.CurrentDirectory + "\\", string.Empty);
            MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(rPath);
            Type scriptType = script.GetClass();
            if (scriptType != null)
            {
                if (rootGameObject.GetComponent(scriptType) == null)
                    rootGameObject.AddComponent(scriptType);
            }
            else
            {
                AddComponentWhenCompiled addComponent = rootGameObject.AddComponent<AddComponentWhenCompiled>();
                addComponent.path = rPath;
            }
        }

        private static void generateChildMembers(CodeTypeDeclaration codeType, CodeMemberMethod awake, GameObject rootGameObject, GameObject gameObject)
        {
            if (gameObject.GetComponent<Button>() != null)
                generateChildComponent(codeType, awake, rootGameObject, gameObject, typeof(Button));
            if (gameObject.GetComponent<RawImage>() != null)
                generateChildComponent(codeType, awake, rootGameObject, gameObject, typeof(RawImage));
            if (gameObject.GetComponent<Image>() != null)
                generateChildComponent(codeType, awake, rootGameObject, gameObject, typeof(Image));
            if (gameObject.GetComponent<Text>() != null)
                generateChildComponent(codeType, awake, rootGameObject, gameObject, typeof(Text));
            if (gameObject.GetComponent<Toggle>() != null)
                generateChildComponent(codeType, awake, rootGameObject, gameObject, typeof(Toggle));
            if (gameObject.GetComponent<Slider>() != null)
                generateChildComponent(codeType, awake, rootGameObject, gameObject, typeof(Slider));
            if (gameObject.GetComponent<Scrollbar>() != null)
                generateChildComponent(codeType, awake, rootGameObject, gameObject, typeof(Scrollbar));
            if (gameObject.GetComponent<Dropdown>() != null)
                generateChildComponent(codeType, awake, rootGameObject, gameObject, typeof(Dropdown));
            if (gameObject.GetComponent<InputField>() != null)
                generateChildComponent(codeType, awake, rootGameObject, gameObject, typeof(InputField));
            if (gameObject.GetComponent<Canvas>() != null)
                generateChildComponent(codeType, awake, rootGameObject, gameObject, typeof(Canvas));
            if (gameObject.GetComponent<ScrollRect>() != null)
                generateChildComponent(codeType, awake, rootGameObject, gameObject, typeof(ScrollRect));
            if (gameObject.GetComponent<VerticalLayoutGroup>() != null)
                generateChildComponent(codeType, awake, rootGameObject, gameObject, typeof(VerticalLayoutGroup));
            if (gameObject.GetComponent<HorizontalLayoutGroup>() != null)
                generateChildComponent(codeType, awake, rootGameObject, gameObject, typeof(HorizontalLayoutGroup));
            if (gameObject.GetComponent<GridLayoutGroup>() != null)
                generateChildComponent(codeType, awake, rootGameObject, gameObject, typeof(GridLayoutGroup));
            for (int i = 0; i < gameObject.transform.childCount; i++)
            {
                GameObject childGameObject = gameObject.transform.GetChild(i).gameObject;
                generateChildMembers(codeType, awake, rootGameObject, childGameObject);
            }
        }

        private static void generateChildComponent(CodeTypeDeclaration codeType, CodeMemberMethod awake, GameObject rootGameObject, GameObject gameObject, Type type)
        {
            string prefix = gameObject.name;
            for (Transform parent = gameObject.transform.parent; parent != null && parent != rootGameObject.transform; parent = parent.parent)
            {
                prefix = parent.gameObject.name + "_" + prefix;
            }
            string propName = prefix + "_" + type.Name;
            propName = removeInvalidChar(propName);
            if (codeType.Members.Cast<CodeTypeMember>().Any(m => m is CodeMemberField f && f.Name == "_" + propName))//重名
            {
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
                propName = removeInvalidChar(prefix + "_" + type.Name);
            }
            CodeMemberField field = new CodeMemberField(type.Name, "_" + propName);
            {
                field.Attributes = MemberAttributes.Private;
                field.CustomAttributes.Add(new CodeAttributeDeclaration(nameof(SerializeField)));
            }
            codeType.Members.Add(field);
            CodeMemberProperty prop = new CodeMemberProperty();
            {
                prop.Attributes = MemberAttributes.Public | MemberAttributes.Final;
                prop.Type = new CodeTypeReference(type.Name);
                prop.Name = propName;
                prop.HasGet = true;
                prop.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "_" + propName)));
            }
            codeType.Members.Add(prop);
            CodeMethodInvokeExpression findExprHead = new CodeMethodInvokeExpression(null, "Find", new CodePrimitiveExpression(gameObject.name));
            CodeMethodInvokeExpression findExpr = findExprHead;
            for (Transform parent = gameObject.transform.parent; parent != null && parent != rootGameObject.transform; parent = parent.parent)
            {
                findExprHead.Method.TargetObject = new CodeMethodInvokeExpression(null, "Find", new CodePrimitiveExpression(parent.gameObject.name));
                findExprHead = findExprHead.Method.TargetObject as CodeMethodInvokeExpression;
            }
            findExprHead.Method.TargetObject = new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "transform");
            awake.Statements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "_" + propName), new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(findExpr, "GetComponent", new CodeTypeReference(type.Name)))));
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

        static void addAs(CodeTypeDeclaration codeType, CodeMemberMethod awake, Type type)
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
                prop.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "m_as_" + type.Name)));
            }
            codeType.Members.Add(prop);
            awake.Statements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "m_as_" + type.Name), new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), "GetComponent", new CodeTypeReference(type.Name)))));
        }
    }
}