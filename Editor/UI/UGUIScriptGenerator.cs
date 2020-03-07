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
using UnityEditor.Animations;

namespace BJSYGameCore.UI
{
    class UGUIScriptGenerator
    {
        #region MenuItem
        [MenuItem("GameObject/UGUI AutoScript", true, 15)]
        public static bool validateGenerateScript()
        {
            if (Selection.gameObjects.Length < 1)
                return false;
            if (Selection.gameObjects.Any(obj => PrefabUtility.IsPartOfPrefabAsset(obj)))
                return false;
            return true;
        }
        [MenuItem("GameObject/UGUI AutoScript", false, 15)]
        public static void generateScript()
        {
            generateOrUpdateSelected(typeof(UIObject));
        }
        [MenuItem("GameObject/UGUI AutoScript As/List", true, 16)]
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
        [MenuItem("GameObject/UGUI AutoScript As/List", false, 16)]
        public static void generateScriptAsList()
        {
            generateOrUpdateSelected(typeof(UIList));
        }
        [MenuItem("GameObject/UGUI AutoScript As/PageGroup", true, 16)]
        public static bool validateGenerateScriptAsPageGroup()
        {
            if (!validateGenerateScript())
                return false;
            if (Selection.gameObjects.Length > 1)
                return false;
            return true;
        }
        [MenuItem("GameObject/UGUI AutoScript As/PageGroup", false, 16)]
        public static void generateScriptAsPageGroup()
        {
            generateOrUpdateSelected(typeof(UIPageGroup));
        }
        #endregion
        static UGUIAutoScriptPref pref { get; set; } = null;
        static Dictionary<GameObject, string> updatedGameObjectDic { get; } = new Dictionary<GameObject, string>();
        static void generateOrUpdateSelected(Type baseType)
        {
            pref = UGUIAutoScriptPref.getDefaultPref();
            string dir;
            if (Directory.Exists(pref.lastDir))
            {
                dir = pref.lastDir;
            }
            else
            {
                dir = EditorUtility.OpenFolderPanel("将脚本文件保存至", "Assets", string.Empty);
                pref.lastDir = dir.Replace(Environment.CurrentDirectory + "\\", string.Empty);//保存上一次的地址
            }
            if (!string.IsNullOrEmpty(dir))
            {
                updatedGameObjectDic.Clear();
                foreach (GameObject gameObject in Selection.gameObjects)
                {
                    Component[] components = gameObject.GetComponentsInChildren(baseType, true);
                    if (components.Length > 0)
                    {
                        Component rootComponent = gameObject.GetComponent(baseType);
                        if (rootComponent != null)
                            generateOrUpdateRoot(dir, null, gameObject, rootComponent.GetType().BaseType);
                        else
                            generateOrUpdateRoot(dir, null, gameObject, baseType);
                        foreach (Component component in components)
                        {
                            generateOrUpdateRoot(dir, gameObject, component.gameObject, component.GetType().BaseType);
                        }
                    }
                    else
                        generateOrUpdateRoot(dir, null, gameObject, baseType);//没有，重新生成
                }
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }
        private static string generateOrUpdateRoot(string dir, GameObject parentGameObject, GameObject rootGameObject, Type baseType = null, string className = null)
        {
            if (updatedGameObjectDic.ContainsKey(rootGameObject))
            {
                //Debug.Log("忽略重复的GameObject" + rootGameObject.name, rootGameObject);
                return updatedGameObjectDic[rootGameObject];
            }
            GameObject prefabGameObject = null;
            string prefabPath = null;
            if (PrefabUtility.IsAnyPrefabInstanceRoot(rootGameObject))
            {
                prefabGameObject = PrefabUtility.GetCorrespondingObjectFromSource(rootGameObject);
                if (updatedGameObjectDic.ContainsKey(prefabGameObject))
                {
                    //Debug.Log("忽略重复的PrefabInstance" + rootGameObject, rootGameObject);
                    return updatedGameObjectDic[prefabGameObject];
                }
                prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(prefabGameObject);
                //Debug.Log("更新或生成预制件" + rootGameObject.name + "，路径：" + prefabPath, rootGameObject);
                rootGameObject = PrefabUtility.LoadPrefabContents(prefabPath);
                parentGameObject = null;//Prefab是没有Parent的。
            }
            //确定文件名
            Component component;
            if (baseType == null)
            {
                component = rootGameObject.GetComponent(typeof(UIObject));
                if (component != null)
                    baseType = component.GetType().BaseType;
                else
                    baseType = typeof(UIObject);
            }
            else
                component = rootGameObject.GetComponent(baseType);
            className = getFixedClassName(rootGameObject, baseType, className);
            string targetPath = dir.Replace('\\', '/') + "/" + className + ".cs";
            MonoScript script;
            string rPath;
            if (component != null)
            {
                script = MonoScript.FromMonoBehaviour(component as MonoBehaviour);
                rPath = AssetDatabase.GetAssetPath(script);
                string originPath = Environment.CurrentDirectory.Replace('\\', '/') + "/" + rPath;
                if (targetPath != originPath)
                {
                    if (EditorUtility.DisplayDialog("UGUI AutoScript", rootGameObject.name + "已有的组件脚本位置与目标位置不一致，是否要在目标位置创建一个新的脚本文件？", "Yes", "No"))
                    {
                        UnityEngine.Object.DestroyImmediate(component);//删除已有组件
                        component = null;
                        script = null;
                        rPath = targetPath.Replace(Environment.CurrentDirectory.Replace('\\', '/') + "/", string.Empty);
                    }
                    else
                    {
                        targetPath = originPath;
                        className = script.GetClass().Name;
                    }
                }
            }
            else
            {
                rPath = targetPath.Replace(Environment.CurrentDirectory.Replace('\\', '/') + "/", string.Empty);
                script = AssetDatabase.LoadAssetAtPath<MonoScript>(rPath);
            }
            updatedGameObjectDic.Add(rootGameObject, className);
            if (prefabGameObject != null)
                updatedGameObjectDic.Add(prefabGameObject, className);
            //加载脚本文件
            FileInfo fileInfo = new FileInfo(targetPath);
            Type scriptType = script != null ? script.GetClass() : null;
            //生成脚本文件
            using (StreamWriter writer = new StreamWriter(fileInfo.Create()))
            {
                CodeCompileUnit unit = generateCompileUnit(dir, parentGameObject, rootGameObject, baseType, className);
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
            if (component == null)
            {
                if (scriptType != null)
                {
                    component = rootGameObject.GetComponent(scriptType);
                    if (component == null)
                        component = rootGameObject.AddComponent(scriptType);
                    try
                    {
                        component.GetType().GetMethod("autoBind").Invoke(component, new object[] { });
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning("自动绑定组件失败：" + e + "\n请尝试重新生成脚本", rootGameObject);
                    }
                }
                else
                {
                    AddComponentWhenCompiledComponent addComponent = rootGameObject.AddComponent<AddComponentWhenCompiledComponent>();
                    addComponent.path = rPath;
                    if (prefabGameObject != null)
                        pref.updateList.Add(prefabGameObject);
                }
            }
            else
            {
                try
                {
                    component.GetType().GetMethod("autoBind").Invoke(component, new object[] { });
                }
                catch (Exception e)
                {
                    Debug.LogWarning("自动绑定组件失败：" + e + "\n请尝试重新生成脚本", rootGameObject);
                }
            }
            if (prefabGameObject != null)
            {
                PrefabUtility.SaveAsPrefabAsset(rootGameObject, prefabPath);
                PrefabUtility.UnloadPrefabContents(rootGameObject);
            }
            return className;
        }
        private static string getFixedClassName(GameObject rootGameObject, Type baseType, string className)
        {
            if (string.IsNullOrEmpty(className))
            {
                if (rootGameObject.name == "Canvas" && rootGameObject.GetComponent<Canvas>() != null)
                    className = rootGameObject.scene.name;
                else
                    className = rootGameObject.name;
                if (baseType == typeof(UIList) && !Regex.IsMatch(className, "List", RegexOptions.IgnoreCase))
                    className += "List";
            }
            return className;
        }
        private static GameObject getRelativePrefabGameObject(GameObject rootGameObject, GameObject childGameObject, GameObject rootPrefab)
        {
            if (rootGameObject == childGameObject)
                return rootPrefab;
            Stack<string> nameStack = new Stack<string>();
            bool isFound = false;
            for (Transform parent = childGameObject.transform.parent; parent != null; parent = parent.parent)
            {
                if (parent.transform == rootGameObject.transform)
                {
                    isFound = true;
                    break;
                }
                else
                    nameStack.Push(parent.gameObject.name);
            }
            if (isFound)
            {
                Transform relativeChild = rootPrefab.transform;
                while (nameStack.Count > 0)
                {
                    relativeChild = relativeChild.Find(nameStack.Pop());
                    if (relativeChild == null)
                        return null;
                }
                return relativeChild.gameObject;
            }
            else
                return null;
        }
        private static CodeCompileUnit generateCompileUnit(string dir, GameObject parentGameObject, GameObject rootGameObject, Type baseType, string className)
        {
            CodeCompileUnit unit = new CodeCompileUnit();
            CodeNamespace codeNamespace = new CodeNamespace(pref.Namespace);
            {
                codeNamespace.Imports.Add(new CodeNamespaceImport("System"));
                codeNamespace.Imports.Add(new CodeNamespaceImport("UnityEngine"));
                codeNamespace.Imports.Add(new CodeNamespaceImport("UnityEngine.UI"));
                codeNamespace.Imports.Add(new CodeNamespaceImport("BJSYGameCore.UI"));
                CodeTypeDeclaration codeType = new CodeTypeDeclaration(className);
                {
                    codeType.Attributes = MemberAttributes.Private;
                    codeType.IsPartial = true;
                    codeType.BaseTypes.Add(baseType.Name);

                    CodeMemberMethod awake = new CodeMemberMethod();
                    {
                        awake.Attributes = MemberAttributes.Family | MemberAttributes.Override;
                        awake.ReturnType = new CodeTypeReference(typeof(void));
                        awake.Name = "Awake";
                        awake.Statements.Add(new CodeMethodInvokeExpression(new CodeBaseReferenceExpression(), "Awake"));
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

                    if (parentGameObject != null && parentGameObject != rootGameObject)
                    {
                        Transform parent = rootGameObject.transform.parent;
                        int parentCount = 1;
                        while (parent != null)
                        {
                            if (parent == parentGameObject.transform)
                                break;
                            parentCount++;
                            parent = parent.parent;
                        }
                        if (parent != null)
                        {
                            string parentClassName = null;
                            try
                            {
                                parentClassName = generateOrUpdateRoot(dir, null, parentGameObject);
                            }
                            catch (Exception e)
                            {
                                Debug.LogError("为父物体" + parentGameObject + "生成脚本失败：" + e, rootGameObject);
                            }
                            if (!string.IsNullOrEmpty(parentClassName))
                            {
                                CodeMemberField parentField = new CodeMemberField();
                                {
                                    parentField.Attributes = MemberAttributes.Private | MemberAttributes.Final;
                                    parentField.Type = new CodeTypeReference(parentClassName);
                                    parentField.Name = "_parent";
                                }
                                codeType.Members.Add(parentField);
                                CodeMemberProperty parentProp = new CodeMemberProperty();
                                {
                                    parentProp.Attributes = MemberAttributes.Public | MemberAttributes.Final;
                                    parentProp.Type = new CodeTypeReference(parentClassName);
                                    parentProp.Name = "parent";
                                    parentProp.HasGet = true;
                                    CodeExpression expr = new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "transform");//this.transform
                                    for (int i = 0; i < parentCount; i++)
                                    {
                                        expr = new CodePropertyReferenceExpression(expr, "parent");//this.transform.parent
                                    }
                                    expr = new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(expr, "GetComponent", new CodeTypeReference(parentClassName)));//this.transform.parent.GetComponent<ClassName>();
                                    parentProp.GetStatements.Add(new CodeMethodReturnStatement(expr));
                                }
                                codeType.Members.Add(parentProp);
                            }
                        }
                        else
                            Debug.LogError(rootGameObject.name + "无法找到父物体" + parentGameObject.name, rootGameObject);
                    }

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
                    if (rootGameObject.GetComponent<ListLayoutGroup>() != null)
                        addAs(codeType, autoBind, rootGameObject, typeof(ListLayoutGroup));
                    if (baseType == typeof(UIList) || baseType.IsSubclassOf(typeof(UIList)))
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
                        string itemTypeName = null;
                        if (defaultItem == null && rootGameObject.transform.childCount > 0)
                        {
                            itemTypeName = className + "Item";
                            defaultItem = rootGameObject.transform.GetChild(0).gameObject;
                            try
                            {
                                itemTypeName = generateOrUpdateRoot(dir, rootGameObject, defaultItem, typeof(UIObject), itemTypeName);
                            }
                            catch (Exception e)
                            {
                                Debug.LogError("为子物体" + defaultItem + "生成脚本失败：" + e, rootGameObject);
                            }
                        }
                        else
                        {
                            Type componentType = defaultItem.GetComponent<UIObject>().GetType();
                            try
                            {
                                itemTypeName = generateOrUpdateRoot(dir, rootGameObject, defaultItem, componentType.BaseType, componentType.Name);
                            }
                            catch (Exception e)
                            {
                                Debug.LogError("为子物体" + defaultItem + "生成脚本失败：" + e, rootGameObject);
                            }
                        }
                        if (!string.IsNullOrEmpty(itemTypeName))
                        {
                            codeType.BaseTypes.Clear();
                            codeType.BaseTypes.Add(new CodeTypeReference("UIList", new CodeTypeReference(itemTypeName)));
                            CodeMemberMethod getDefaultItemMethod = new CodeMemberMethod();
                            {
                                getDefaultItemMethod.Attributes = MemberAttributes.Family | MemberAttributes.Override;
                                getDefaultItemMethod.ReturnType = new CodeTypeReference(itemTypeName);
                                getDefaultItemMethod.Name = "getDefaultItem";
                                getDefaultItemMethod.Statements.Add(new CodeMethodReturnStatement(new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(getFindExpr(rootGameObject, defaultItem), "GetComponent", new CodeTypeReference(itemTypeName)))));
                            }
                            codeType.Members.Add(getDefaultItemMethod);
                            addAwakePropAssign(autoBind, rootGameObject, defaultItem, itemTypeName, nameof(UIList.defaultItem));
                            awake.Statements.Add(new CodeMethodInvokeExpression(new CodePropertyReferenceExpression(new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), nameof(UIList.defaultItem)), nameof(MonoBehaviour.gameObject)), nameof(GameObject.SetActive), new CodePrimitiveExpression(false)));
                        }
                        for (int i = 0; i < rootGameObject.transform.childCount; i++)
                        {
                            GameObject gameObject = rootGameObject.transform.GetChild(i).gameObject;
                            if (defaultItem != null && gameObject != defaultItem)
                                generateChildMembers(dir, codeType, autoBind, rootGameObject, gameObject);
                        }
                    }
                    else if (baseType == typeof(UIPageGroup))
                    {
                        List<CodeMemberProperty> childPropList = new List<CodeMemberProperty>();
                        for (int i = 0; i < rootGameObject.transform.childCount; i++)
                        {
                            GameObject gameObject = rootGameObject.transform.GetChild(i).gameObject;
                            childPropList.AddRange(generateChildMembers(dir, codeType, autoBind, rootGameObject, gameObject));
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
                            generateChildMembers(dir, codeType, autoBind, rootGameObject, gameObject);
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
                    #region Controller
                    Animator animator = rootGameObject.GetComponent<Animator>();
                    if (animator != null && animator.runtimeAnimatorController is AnimatorController controller)
                    {
                        foreach (AnimatorControllerLayer layer in controller.layers)
                        {
                            Match m = Regex.Match(layer.name, @"(?<name>\w+)Controller");
                            if (m.Success)
                            {
                                string controllerName = m.Groups["name"].Value;
                                if (layer.stateMachine == null)
                                {
                                    Debug.LogError("为" + controllerName + "控制器生成脚本失败，丢失状态机", rootGameObject);
                                    continue;
                                }
                                CodeTypeDeclaration controllerEnum = new CodeTypeDeclaration();
                                {
                                    controllerEnum.Attributes = MemberAttributes.Public;
                                    controllerEnum.IsEnum = true;
                                    controllerEnum.Name = controllerName;
                                    foreach (ChildAnimatorState state in layer.stateMachine.states)
                                    {
                                        string stateName = state.state.name;
                                        CodeMemberField stateField = new CodeMemberField();
                                        {
                                            stateField.Name = stateName;
                                        }
                                        controllerEnum.Members.Add(stateField);
                                    }
                                }
                                codeType.Members.Add(controllerEnum);
                                CodeMemberProperty controllerProp = new CodeMemberProperty();
                                {
                                    controllerProp.Attributes = MemberAttributes.Public | MemberAttributes.Final;
                                    controllerProp.Type = new CodeTypeReference(controllerName);
                                    controllerProp.Name = controllerName + "Controller";
                                    controllerProp.HasGet = true;
                                    CodeExpression expr;
                                    expr = new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("Enum"), "GetNames", new CodeTypeOfExpression(controllerName));//Enum.GetNames(typeof(Type))
                                    expr = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "getController", new CodePrimitiveExpression(controllerName), expr);//getController("Type", Enum.GetNames(typeof(Type)))
                                    expr = new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("Enum"), "Parse", new CodeTypeOfExpression(controllerName), expr);//Enum.Parse(typeof(Type), getController("Type", Enum.GetNames(typeof(Type))))
                                    expr = new CodeCastExpression(new CodeTypeReference(controllerName), expr);//(Type)Enum.Parse(typeof(Type), getController("Type", Enum.GetNames(typeof(Type))))
                                    controllerProp.GetStatements.Add(new CodeMethodReturnStatement(expr));
                                    controllerProp.HasSet = true;
                                    expr = new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("Enum"), "GetName", new CodeTypeOfExpression(controllerName), new CodePropertySetValueReferenceExpression());//Enum.GetName(typeof(Type), value)
                                    expr = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "setController", new CodePrimitiveExpression(controllerName), expr);//setController("Type", Enum.GetName(typeof(Type), value))
                                    controllerProp.SetStatements.Add(expr);
                                }
                                codeType.Members.Add(controllerProp);
                            }
                        }
                    }
                    #endregion
                }
                codeNamespace.Types.Add(codeType);
            }
            unit.Namespaces.Add(codeNamespace);
            return unit;
        }

        private static CodeMemberProperty[] generateChildMembers(string dir, CodeTypeDeclaration codeType, CodeMemberMethod initMethod, GameObject rootGameObject, GameObject gameObject)
        {
            if (PrefabUtility.IsAnyPrefabInstanceRoot(gameObject))
            {
                if (gameObject.GetComponent<UIObject>() is UIObject prefabCompo)
                {
                    Debug.Log("处理预制件" + gameObject, gameObject);
                    return new CodeMemberProperty[] { generateChildComponent(codeType, initMethod, rootGameObject, gameObject, prefabCompo.GetType()) };
                }
                else
                {
                    string className = null;
                    try
                    {
                        className = generateOrUpdateRoot(dir, rootGameObject, gameObject, typeof(UIObject));
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("为子物体" + gameObject + "生成脚本失败：" + e, rootGameObject);
                    }
                    if (!string.IsNullOrEmpty(className))
                    {
                        Debug.Log("处理预制件" + gameObject, gameObject);
                        return new CodeMemberProperty[] { generateChildComponent(codeType, initMethod, rootGameObject, gameObject, className) };
                    }
                    else
                    {
                        Debug.LogWarning("无法为预制件" + gameObject + "生成脚本！", gameObject);
                        return new CodeMemberProperty[0];
                    }
                }
            }
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
                    childPropList.AddRange(generateChildMembers(dir, codeType, initMethod, rootGameObject, scrollRect.content.gameObject));
                return childPropList.ToArray();
            }
            if (gameObject.GetComponent<InputField>() != null)
            {
                return new CodeMemberProperty[] { generateChildComponent(codeType, initMethod, rootGameObject, gameObject, typeof(InputField)) };
            }
            if (gameObject.GetComponent<Mask>() != null)
            {
                childPropList.Add(generateChildComponent(codeType, initMethod, rootGameObject, gameObject, typeof(Mask)));
                for (int i = 0; i < gameObject.transform.childCount; i++)
                {
                    GameObject childGameObject = gameObject.transform.GetChild(i).gameObject;
                    childPropList.AddRange(generateChildMembers(dir, codeType, initMethod, rootGameObject, childGameObject));
                }
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
            {
                return new CodeMemberProperty[] { generateChildComponent(codeType, initMethod, rootGameObject, gameObject, typeof(Slider)) };
            }
            if (gameObject.GetComponent<Scrollbar>() != null)
                childPropList.Add(generateChildComponent(codeType, initMethod, rootGameObject, gameObject, typeof(Scrollbar)));
            if (gameObject.GetComponent<Canvas>() != null)
                childPropList.Add(generateChildComponent(codeType, initMethod, rootGameObject, gameObject, typeof(Canvas)));
            if (gameObject.GetComponent<VerticalLayoutGroup>() != null)
                childPropList.Add(generateChildComponent(codeType, initMethod, rootGameObject, gameObject, typeof(VerticalLayoutGroup)));
            if (gameObject.GetComponent<HorizontalLayoutGroup>() != null)
                childPropList.Add(generateChildComponent(codeType, initMethod, rootGameObject, gameObject, typeof(HorizontalLayoutGroup)));
            if (gameObject.GetComponent<GridLayoutGroup>() != null)
                childPropList.Add(generateChildComponent(codeType, initMethod, rootGameObject, gameObject, typeof(GridLayoutGroup)));
            if (gameObject.GetComponent<ListLayoutGroup>() != null)
                childPropList.Add(generateChildComponent(codeType, initMethod, rootGameObject, gameObject, typeof(ListLayoutGroup)));
            for (int i = 0; i < gameObject.transform.childCount; i++)
            {
                GameObject childGameObject = gameObject.transform.GetChild(i).gameObject;
                childPropList.AddRange(generateChildMembers(dir, codeType, initMethod, rootGameObject, childGameObject));
            }
            return childPropList.ToArray();
        }
        private static CodeMemberProperty generateChildComponent(CodeTypeDeclaration codeType, CodeMemberMethod initMethod, GameObject rootGameObject, GameObject gameObject, Type type)
        {
            return generateChildComponent(codeType, initMethod, rootGameObject, gameObject, type.Name);
        }
        private static CodeMemberProperty generateChildComponent(CodeTypeDeclaration codeType, CodeMemberMethod initMethod, GameObject rootGameObject, GameObject gameObject, string typeName)
        {
            string propName;
            string prefix = gameObject.name;//先从最短名称gameObject.name开始
            string suffix;//默认后缀为字段类型，但是有些太长了需要缩写
            if (typeName.EndsWith("List"))
                suffix = "List";
            else if (typeName == typeof(ScrollRect).Name)
                suffix = "Scroll";
            else if (typeName == typeof(InputField).Name)
                suffix = "Input";
            else
                suffix = typeName;
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
            CodeMemberField field = new CodeMemberField(typeName, "_" + propName);
            {
                field.Attributes = MemberAttributes.Private;
                field.CustomAttributes.Add(new CodeAttributeDeclaration(nameof(SerializeField)));
            }
            codeType.Members.Add(field);
            CodeMemberProperty prop = new CodeMemberProperty();
            {
                prop.UserData.Add("GameObject", gameObject);
                prop.Attributes = MemberAttributes.Public | MemberAttributes.Final;
                prop.Type = new CodeTypeReference(typeName);
                prop.Name = propName;
                prop.HasGet = true;
                prop.GetStatements.Add(new CodeConditionStatement(new CodeBinaryOperatorExpression(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "_" + propName), CodeBinaryOperatorType.ValueEquality, new CodePrimitiveExpression(null)), new CodeStatement[]
                {
                    getFieldAssignStatement(rootGameObject, gameObject, typeName, "_" + propName)
                }));
                prop.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "_" + propName)));
            }
            codeType.Members.Add(prop);
            addAwakeFieldAssign(initMethod, rootGameObject, gameObject, typeName, propName);
            return prop;
        }

        private static void addAwakeFieldAssign(CodeMemberMethod initMethod, GameObject rootGameObject, GameObject gameObject, string typeName, string propName)
        {
            CodeAssignStatement assign = getFieldAssignStatement(rootGameObject, gameObject, typeName, "_" + propName);
            initMethod.Statements.Add(assign);
        }
        private static CodeAssignStatement getFieldAssignStatement(GameObject rootGameObject, GameObject gameObject, string typeName, string fieldName)
        {
            CodeExpression findExpr = getFindExpr(rootGameObject, gameObject);
            CodeAssignStatement assign = new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), fieldName), new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(findExpr, "GetComponent", new CodeTypeReference(typeName))));
            return assign;
        }
        private static CodeAssignStatement getPropAssignStatement(GameObject rootGameObject, GameObject gameObject, string typeName, string propName)
        {
            CodeExpression findExpr = getFindExpr(rootGameObject, gameObject);
            CodeAssignStatement assign = new CodeAssignStatement(new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), propName), new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(findExpr, "GetComponent", new CodeTypeReference(typeName))));
            return assign;
        }
        private static void addAwakePropAssign(CodeMemberMethod initMethod, GameObject rootGameObject, GameObject gameObject, Type type, string propName)
        {
            addAwakePropAssign(initMethod, rootGameObject, gameObject, type.Name, propName);
        }
        private static void addAwakePropAssign(CodeMemberMethod initMethod, GameObject rootGameObject, GameObject gameObject, string typeName, string propName)
        {
            CodeExpression findExpr = getFindExpr(rootGameObject, gameObject);
            initMethod.Statements.Add(new CodeAssignStatement(new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), propName), new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(findExpr, "GetComponent", new CodeTypeReference(typeName)))));
        }
        private static CodeExpression getFindExpr(GameObject rootGameObject, GameObject gameObject)
        {
            if (rootGameObject == gameObject)
                return new CodeThisReferenceExpression();
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