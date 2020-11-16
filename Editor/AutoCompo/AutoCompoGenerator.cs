using System;
using UnityEngine;
using System.CodeDom;
using System.Text.RegularExpressions;
using NUnit.Framework.Interfaces;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using UnityEditor;
using System.Linq;
using UnityEngine.SceneManagement;
using System.IO;

namespace BJSYGameCore.AutoCompo
{
    public class AutoCompoWindow : EditorWindow
    {
        #region 公共成员
        [MenuItem("Assets/Create/AutoCompo/Generate", true, 81)]
        [MenuItem("GameObject/AutoCompo/Generate", true, 15)]
        public static bool onValidateMenuItemGenerate()
        {
            if (Selection.gameObjects.Length != 1)
                return false;
            return true;
        }
        [MenuItem("Assets/Create/AutoCompo/Generate", false, 81)]
        [MenuItem("GameObject/AutoCompo/Generate", false, 15)]
        public static void onMenuItemGenerate()
        {
            GameObject gameObject = Selection.gameObjects[0];
            string path = gameObject.scene.path;
            if (string.IsNullOrEmpty(path))
                path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(gameObject);
            AutoCompoWindow window = GetWindow<AutoCompoWindow>(nameof(AutoCompo), true);
            window._gameObject = gameObject;
            window._saveDir = Path.GetDirectoryName(path);
        }
        #endregion
        #region 私有成员
        protected void OnEnable()
        {
            _serializedObject = new SerializedObject(this);
        }
        protected void OnGUI()
        {
            if (_serializedObject == null)
                _serializedObject = new SerializedObject(this);
            GUI.enabled = false;
            EditorGUILayout.ObjectField("要生成脚本的游戏物体", _gameObject, typeof(GameObject), true);
            GUI.enabled = true;
            EditorGUILayout.LabelField("保存路径", _saveDir);
            EditorGUILayout.PropertyField(_serializedObject.FindProperty(nameof(_setting)), new GUIContent("设置"), true);
            AutoCompoGenerator generator = null;
            if (GUILayout.Button("保存脚本"))
                generator = new AutoCompoGenerator();
            if (GUILayout.Button("另存为脚本"))
            {
                _saveDir = EditorUtility.SaveFolderPanel("另存为脚本", _saveDir, string.Empty);
                if (Directory.Exists(_saveDir))
                    generator = new AutoCompoGenerator();
            }
            _serializedObject.ApplyModifiedProperties();
            if (generator != null)
            {
                var unit = generator.genScript4GO(_gameObject, _setting);
                CodeDOMHelper.writeUnitToFile(new FileInfo(_saveDir + "/" + unit.Namespaces[0].Types[0].Name + ".cs"), unit);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }
        protected void OnDisable()
        {
            _serializedObject.Dispose();
            _serializedObject = null;
        }
        #endregion
        [SerializeField]
        GameObject _gameObject;
        [SerializeField]
        string _saveDir;
        [SerializeField]
        AutoCompoGenSetting _setting;
        SerializedObject _serializedObject;
    }
    public partial class AutoCompoGenerator
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
            _type = new CodeTypeDeclaration();
            nameSpace.Types.Add(_type);
            _type.CustomAttributes.Add(new CodeAttributeDeclaration(nameof(AutoCompoAttribute),
                new CodeAttributeArgument(new CodePrimitiveExpression(gameObject.GetInstanceID()))));
            _type.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            _type.IsPartial = true;
            _type.IsClass = true;
            _type.Name = genTypeName4GO(gameObject);
            foreach (var baseType in setting.baseTypes)
            {
                _type.BaseTypes.Add(baseType);
            }
            //自动绑定方法
            _autoBindMethod = new CodeMemberMethod();
            _type.Members.Add(_autoBindMethod);
            _autoBindMethod.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            _autoBindMethod.ReturnType = new CodeTypeReference(typeof(void));
            _autoBindMethod.Name = "autoBind";
            //根物体组件引用
            string[] compoTypes;
            if (tryParseGOName(gameObject.name, out _, out compoTypes))
            {
                foreach (var compoTypeName in compoTypes)
                {
                    Component component = gameObject.GetComponent(compoTypeName);
                    if (component == null)
                        continue;
                    genRootCompo(component);
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

        void genRootCompo(Component component)
        {
            string fieldName = genFieldName4RootCompo(component);
            string[] path = new string[0];
            string propName = genPropName4RootCompo(component);
            genFieldPropInit4Compo(component, fieldName, propName, path);
        }

        void genFieldPropInit4Compo(Component component, string fieldName, string propName, string[] path)
        {
            genFieldWithInit4Compo(component, fieldName, path);
            genProp4Compo(component, propName, fieldName);
        }

        void genProp4Compo(Component component, string propName, string fieldName)
        {
            CodeMemberProperty prop = new CodeMemberProperty();
            _type.Members.Add(prop);
            prop.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            prop.Type = new CodeTypeReference(component.GetType().Name);
            prop.Name = propName;
            prop.HasGet = true;
            prop.GetStatements.Add(new CodeMethodReturnStatement(
                new CodeFieldReferenceExpression(new CodeThisReferenceExpression(),
                fieldName)));
        }
        void genFieldWithInit4Compo(Component component, string fieldName, string[] path)
        {
            genField4Compo(component, fieldName);
            CodeAssignStatement assign = new CodeAssignStatement();
            _autoBindMethod.Statements.Add(assign);
            assign.Left = new CodeFieldReferenceExpression(
                new CodeThisReferenceExpression(), fieldName);
            CodeExpression target = new CodeThisReferenceExpression();
            for (int i = 0; i < path.Length; i++)
            {
                if (i == 0)
                    target = new CodePropertyReferenceExpression(target, nameof(GameObject.transform));
                target = new CodeMethodInvokeExpression(target, nameof(Transform.Find),
                    new CodePrimitiveExpression(path[i]));
            }
            assign.Right = new CodeMethodInvokeExpression(
                new CodeMethodReferenceExpression(target, nameof(Component.GetComponent),
                new CodeTypeReference(component.GetType().Name)));
        }
        void genField4Compo(Component component, string fieldName)
        {
            CodeMemberField field = new CodeMemberField();
            _type.Members.Add(field);
            foreach (var fieldAttName in _setting.fieldAttributes)
            {
                field.CustomAttributes.Add(new CodeAttributeDeclaration(fieldAttName));
            }
            field.Attributes = MemberAttributes.Private | MemberAttributes.Final;
            field.Type = new CodeTypeReference(component.GetType().Name);
            field.Name = fieldName;
        }
        void genChildGO(GameObject gameObject)
        {
            string[] compoTypes;
            if (tryParseGOName(gameObject.name, out _, out compoTypes))
            {
                foreach (var compoTypeName in compoTypes)
                {
                    Component component = gameObject.GetComponent(compoTypeName);
                    if (component == null)
                        continue;
                    genFieldPropInit4Compo(component, genFieldName4Compo(component),
                        genPropName4Compo(component), getPath(_rootGameObject, gameObject));
                }
            }
            //处理子物体
            for (int i = 0; i < gameObject.transform.childCount; i++)
            {
                GameObject childGO = gameObject.transform.GetChild(i).gameObject;
                genChildGO(childGO);
            }
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
        string genFieldName4Compo(Component component)
        {
            string fieldName;
            if (tryParseGOName(component.gameObject.name, out fieldName, out _))
            {
                return "_" + fieldName + component.GetType().Name;
            }
            else
                throw new FormatException();
        }
        string genPropName4Compo(Component component)
        {
            string propName;
            if (tryParseGOName(component.gameObject.name, out propName, out _))
                return propName + component.GetType().Name;
            else
                throw new FormatException();
        }
        string[] getPath(GameObject parent, GameObject child)
        {
            if (parent.transform == child.transform)
                return new string[0];
            List<string> pathList = new List<string>();
            for (Transform transform = child.transform; transform != null; transform = transform.parent)
            {
                if (transform.gameObject == parent)
                    break;
                pathList.Add(transform.gameObject.name);
            }
            return pathList.ToArray();
        }
        AutoCompoGenSetting _setting;
        GameObject _rootGameObject;
        CodeTypeDeclaration _type;
        CodeMemberMethod _autoBindMethod;
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