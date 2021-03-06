﻿using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using Object = UnityEngine.Object;
using Codo = BJSYGameCore.CodeDOMHelper;
using UnityEngine.UI;
using System.Reflection;
using UnityEditor.Animations;
// ReSharper disable SuggestVarOrType_SimpleTypes
namespace BJSYGameCore.AutoCompo
{
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
            rootGameObject = gameObject;
            CodeCompileUnit unit = new CodeCompileUnit();
            //命名空间，引用
            _nameSpace = new CodeNamespace(setting.Namespace);
            unit.Namespaces.Add(_nameSpace);
            foreach (string import in setting.usings)
            {
                if (Regex.IsMatch(import, @"(\w\.)*\w"))
                    _nameSpace.Imports.Add(new CodeNamespaceImport(import));
                else
                    Debug.LogError(import + "不是合法的命名空间");
            }
            //类
            _type = new CodeTypeDeclaration();
            _nameSpace.Types.Add(_type);
            genType4RootGO();
            return unit;
        }
        public CodeCompileUnit genScript4GO(GameObject gameObject, Type existType)
        {
            rootGameObject = gameObject;
            CodeCompileUnit unit = new CodeCompileUnit();
            //命名空间
            _nameSpace = new CodeNamespace(existType.Namespace);
            unit.Namespaces.Add(_nameSpace);
            //类
            _type = new CodeTypeDeclaration();
            _nameSpace.Types.Add(_type);
            genType4RootGO(existType);
            return unit;
        }
        public virtual string genTypeName4GO(GameObject gameObject)
        {
            if (string.IsNullOrEmpty(specifiedTypeName))
                return gameObject.name;
            return specifiedTypeName;
        }
        /// <summary>
        /// 默认生成一个自动绑定方法。
        /// </summary>
        protected virtual void genType4RootGO(Type existType = null)
        {
            _type.Attributes = MemberAttributes.Final;
            _type.IsPartial = true;
            _type.IsClass = true;
            _type.Name = existType == null ? genTypeName4GO(rootGameObject) : existType.Name;
            if (_setting != null)
            {
                foreach (string baseType in _setting.baseTypes)
                {
                    _type.BaseTypes.Add(baseType);
                }
            }
            genMembers();
            genBindFields();
        }
        protected virtual Dictionary<Type, ComponentGenerator> initComponentGenrators()
        {
            Dictionary<Type, ComponentGenerator> dict = new Dictionary<Type, ComponentGenerator>();
            dict[typeof(Button)] = new ButtonGenerator();
            dict[typeof(Animator)] = new AnimatorGenerator();
            return dict;
        }
        /// <summary>
        /// 默认生成init和clear方法。
        /// </summary>
        protected virtual void genMembers()
        {
            _initMethod = genMethod(MemberAttributes.Public | MemberAttributes.Final, typeof(void), getInitMethodName());
            _clearMethod = genMethod(MemberAttributes.Public | MemberAttributes.Final, typeof(void), getClearMethodName());
            if (controllerType == CTRL_TYPE_LIST || controllerType == CTRL_TYPE_BUTTON_LIST)
            {
                //ItemPool类型
                CodeTypeDeclaration itemPoolType = new CodeTypeDeclaration();
                _type.Members.Add(itemPoolType);
                itemPoolType.CustomAttributes.Add(new CodeAttributeDeclaration(typeof(SerializableAttribute).Name));
                itemPoolType.Attributes = MemberAttributes.Public | MemberAttributes.Final;
                itemPoolType.IsClass = true;
                itemPoolType.Name = "ItemPool";
                //itemPoolType.BaseTypes.Add(new CodeTypeReference(typeof(ComponentPool<>).Name, new CodeTypeReference(listItemTypeName)));
                //构造器
                CodeConstructor itemPoolTypeConstructor = new CodeConstructor();
                itemPoolType.Members.Add(itemPoolTypeConstructor);
                itemPoolTypeConstructor.Attributes = MemberAttributes.Public | MemberAttributes.Final;
                itemPoolTypeConstructor.Parameters
                    .append(typeof(Transform).Name, "root")
                    .append(listItemTypeName, "origin");
                itemPoolTypeConstructor.BaseConstructorArgs
                    .appendArg("root")
                    .appendArg("origin");
                //ItemPool字段
                CodeMemberField itemPool = genField(itemPoolType.Name, FIELD_NAME_ITEM_POOL, false);
                itemPool.CustomAttributes.Add(new CodeAttributeDeclaration(typeof(SerializeField).Name));
                //onItemClick事件
                CodeMemberEvent onItemClick = null;
                if (controllerType == CTRL_TYPE_BUTTON_LIST)
                    onItemClick = genEvent(typeof(Action).Name, "onItemClick", Codo.type(listItemTypeName));
                //itemClickCallback方法
                CodeMemberMethod itemClickCallback = null;
                if (controllerType == CTRL_TYPE_BUTTON_LIST)
                {
                    itemClickCallback = genMethod(MemberAttributes.Private | MemberAttributes.Final, typeof(void), "itemClickCallback");
                    itemClickCallback.Parameters.append(listItemTypeName, "item");
                    itemClickCallback.Statements.append(new CodeConditionStatement(
                        Codo.op(Codo.This.getEvent(onItemClick.Name), CodeBinaryOperatorType.IdentityInequality, Codo.Null),
                            Codo.This.getEvent(onItemClick.Name).invoke(Codo.arg("item")).statement()));
                }
                //onItemCreate方法
                if (controllerType == CTRL_TYPE_BUTTON_LIST)
                {
                    genMethod(MemberAttributes.Private | MemberAttributes.Final, typeof(void), "onItemCreate")
                        .appendParam(listItemTypeName, "item")
                        .appendStatement(Codo.arg("item").getMethod("autoReg").invoke())
                        .appendStatement(Codo.arg("item").getEvent("onClick").attach(Codo.This.getMethod(itemClickCallback.Name)));
                }
                else
                    genPartialMethod("void", "onItemCreate", Codo.parameter(listItemTypeName, "item"));
                //onItemRemove方法
                if (controllerType == CTRL_TYPE_BUTTON_LIST)
                {
                    genMethod(MemberAttributes.Private | MemberAttributes.Final, typeof(void), "onItemRemove")
                       .appendParam(listItemTypeName, "item")
                       .appendStatement(Codo.arg("item").getMethod("autoUnreg").invoke())
                       .appendStatement(Codo.arg("item").getEvent("onClick").remove(Codo.This.getMethod(itemClickCallback.Name)));
                }
                else
                    genPartialMethod("void", "onItemRemove", Codo.parameter(listItemTypeName, "item"));
                //initPool方法
                var initPool = genMethod(MemberAttributes.Private | MemberAttributes.Final, typeof(void), METHOD_NAME_LIST_INIT_POOL);
                _initMethod.Statements.Add(Codo.This.getMethod(initPool.Name).invoke().statement());
                const string VAR_NAME_ITEM = "item";
                initPool.Statements.append(Codo.decVar(listItemTypeName, VAR_NAME_ITEM,
                    string.IsNullOrEmpty(listItemTypeName) ?//有无指定脚本类型？
                    Codo.This.getField(FIELD_NAME_ORIGIN) as CodeExpression :
                    Codo.This.getField(FIELD_NAME_ORIGIN).getMethod(NAME_OF_ADDCOMPO, Codo.type(listItemTypeName)).invoke()));
                initPool.Statements.Add(Codo.This.getField(FIELD_NAME_ORIGIN).getProp(NAME_OF_GAMEOBJECT).getMethod(NAME_OF_SET_ACTIVE).invoke(Codo.False));
                initPool.Statements.Add(Codo.assign(Codo.This.getField(itemPool.Name),
                    Codo.New(itemPoolType.Name, Codo.This.getProp("transform"), Codo.Var(VAR_NAME_ITEM))));
                initPool.Statements.Add(Codo.This.getField(itemPool.Name).getEvent("onCreate").attach(Codo.This.getMethod("onItemCreate")));
                initPool.Statements.Add(Codo.This.getField(itemPool.Name).getEvent("onRemove").attach(Codo.This.getMethod("onItemRemove")));
                //setCount方法
                CodeMemberMethod setCount = genMethod(MemberAttributes.Public | MemberAttributes.Final, typeof(void), "setCount");
                setCount.Parameters.append(typeof(int), "count");
                setCount.Statements.Add(Codo.This.getField(itemPool.Name).getMethod("setCount").invoke(Codo.arg("count")));
                //indexer
                CodeMemberProperty indexer = genIndexer(Codo.type(listItemTypeName));
                indexer.Parameters.append(typeof(int), "index");
                indexer.HasGet = true;
                indexer.GetStatements.Add(Codo.Return(Codo.This.getField(itemPool.Name).index(Codo.arg("index"))));
                //indexOf方法
                CodeMemberMethod indexOf = genMethod(MemberAttributes.Public | MemberAttributes.Final, typeof(int), "indexOf");
                indexOf.Parameters.append(listItemTypeName, "item");
                indexOf.Statements.append(Codo.Return(Codo.This.getField(itemPool.Name).getMethod("indexOf").invoke(Codo.arg("item"))));
            }
        }

        protected virtual string getInitMethodName()
        {
            return "autoInit";
        }

        protected virtual string getClearMethodName()
        {
            return "autoClear";
        }
        /// <summary>
        /// 根据提供的字段字典进行生成。
        /// </summary>
        protected virtual void genBindFields()
        {
            if (controllerType == CTRL_TYPE_BUTTON && buttonMain == null)
                throw new InvalidOperationException("控件类型为按钮却没有主按钮");
            _compoGenDict = initComponentGenrators();
            foreach (var fieldInfo in objFieldDict.Values.Where(f => f != null && f.isGenerated))
            {
                if (fieldInfo.targetType == typeof(GameObject))
                    genGameObject(rootGameObject.find(fieldInfo.path));
                else
                {
                    GameObject targetGameObject = rootGameObject.find(fieldInfo.path);
                    if (targetGameObject == null)
                    {
                        Debug.LogError("无法找到物体" + fieldInfo.path);
                        continue;
                    }
                    Component targetComponent = targetGameObject.GetComponent(fieldInfo.targetType);
                    if (targetComponent == null)
                    {
                        Debug.LogError("无法找到组件" + fieldInfo.targetType.Name, targetGameObject);
                        continue;
                    }
                    genCompo(targetComponent, fieldInfo);
                }
            }
            //如果方法里面没有任何绑定内容，那么就不需要。
            if (_initMethod.Statements.Count < 1)
                _type.Members.Remove(_initMethod);
            if (_clearMethod.Statements.Count < 1)
                _type.Members.Remove(_clearMethod);
        }
        /// <summary>
        /// 默认生成该物体及组件和子物体及组件的字段，属性，初始化。
        /// </summary>
        /// <param name="gameObject"></param>
        protected virtual void genGameObject(GameObject gameObject)
        {
            string fieldName;
            if (gameObject == listOrigin)
                fieldName = FIELD_NAME_ORIGIN;
            else
                fieldName = genFieldName4GO(gameObject);
            //字段
            addTypeUsing(typeof(GameObject));
            var field = genField(typeof(GameObject), fieldName);
            //常量
            var constField = genField("const string", "PATH" + field.Name.ToUpper(), false);
            constField.InitExpression = Codo.String(rootGameObject.transform.getChildPath(gameObject.transform));
            //属性
            string propName = field.Name;
            while (propName.StartsWith("_"))
                propName = propName.Substring(1, propName.Length - 1);
            propName = propName.headToLower();
            genProp4GO(gameObject, propName, field.Name);
            //初始化
            addTypeUsing(typeof(TransformHelper));
            _initMethod.Statements.append(Codo.This.getField(field.Name).assign(Codo.This.getProp(NAME_OF_TRANSFORM)
                .getMethod(NAME_OF_FIND).invoke(Codo.getField("PATH" + field.Name.ToUpper()))
                .getProp(NAME_OF_GAMEOBJECT)));
        }
        /// <summary>
        /// 默认生成字段，属性，以及初始化语句（一个常量用于自动查找）。
        /// </summary>
        /// <param name="component"></param>
        protected virtual void genCompo(Component component, AutoBindFieldInfo fieldInfo)
        {
            //字段
            var field = genField4Compo(component, genFieldName4Compo(component));
            //常量
            var constField = genField("const string", "PATH" + field.Name.ToUpper(), false);
            constField.InitExpression = Codo.String(fieldInfo.path);
            //属性
            var prop = genProp4Compo(component, genPropName4Field(field.Name), field.Name);
            //初始化
            addTypeUsing(typeof(TransformHelper));
            _initMethod.Statements.append(Codo.getField(field.Name).assign(Codo.getProp(NAME_OF_TRANSFORM)
                .getMethod(NAME_OF_FIND).invoke(Codo.getField("PATH" + field.Name.ToUpper()))
                .getMethod(NAME_OF_GETCOMPO, Codo.type(component.GetType().Name)).invoke()));
            //特殊组件处理
            foreach (var pair in _compoGenDict)
            {
                if (pair.Key == component.GetType() || component.GetType().IsSubclassOf(pair.Key))
                {
                    pair.Value.onGen(this, component, component.gameObject == rootGameObject, fieldInfo, field);
                    return;
                }
            }
            if (component is Button)
                onGenButton(component as Button, component.gameObject == rootGameObject, field, prop);
            else if (component is Animator)
                onGenAnimator(component as Animator, field, prop);
            else if (component is RectTransform && fieldInfo.getValueOrDefault<bool>("isList"))
                onGenList(component as RectTransform, field, fieldInfo);
        }
        protected virtual void onGenButton(Button button, bool isRootComponent, CodeMemberField field, CodeMemberProperty prop)
        {
            CodeAttributeDeclaration autoCompo = field.CustomAttributes.OfType<CodeAttributeDeclaration>()
                .FirstOrDefault(a => a.AttributeType.BaseType == typeof(AutoCompoAttribute).Name);
            //是按钮
            if (controllerType == CTRL_TYPE_BUTTON && button == buttonMain)
                autoCompo.Arguments.Add(new CodeAttributeArgument(new CodePrimitiveExpression("mainButton")));
            string name = prop.Name;
            name = name.headToUpper();
            //事件
            CodeMemberEvent Event;
            if ((controllerType == CTRL_TYPE_BUTTON && button == buttonMain) ||
                isRootComponent)
                Event = genEvent(typeof(Action).Name, "onClick", Codo.type(_type.Name));
            else
                Event = genEvent(typeof(Action).Name, "on" + name + "Click", Codo.type(_type.Name));
            //回调函数
            CodeMemberMethod callbackMethod;
            if ((controllerType == CTRL_TYPE_BUTTON && button == buttonMain) || isRootComponent)
            {
                callbackMethod = genMethod(MemberAttributes.Private | MemberAttributes.Final, typeof(void), "clickCallback");
                callbackMethod.Statements.Add(new CodeConditionStatement(
                new CodeBinaryOperatorExpression(Codo.getEvent(Event.Name), CodeBinaryOperatorType.IdentityInequality, Codo.Null),
                    Codo.getEvent(Event.Name).invoke(Codo.This).statement()));
            }
            else
            {
                callbackMethod = genMethod(MemberAttributes.Private | MemberAttributes.Final, typeof(void), name + "ClickCallback");
                callbackMethod.Statements.Add(new CodeConditionStatement(
                    new CodeBinaryOperatorExpression(Codo.getEvent(Event.Name), CodeBinaryOperatorType.IdentityInequality, Codo.Null),
                        Codo.getEvent(Event.Name).invoke(Codo.This).statement()));
            }
            //注册
            _initMethod.Statements.Add(Codo.getField(field.Name).getProp(NAME_OF_ONCLICK)
                .getMethod(NAME_OF_ADDLISTENER).invoke(Codo.getMethod(callbackMethod.Name)).statement());
            //注销
            _clearMethod.Statements.Add(Codo.getField(field.Name).getProp(NAME_OF_ONCLICK)
                .getMethod(NAME_OF_REMOVELISTENER).invoke(Codo.getMethod(callbackMethod.Name)).statement());
        }
        /// <summary>
        /// 生成动画参数对应的常量
        /// </summary>
        /// <param name="animator"></param>
        /// <param name="field"></param>
        /// <param name="prop"></param>
        private void onGenAnimator(Animator animator, CodeMemberField field, CodeMemberProperty prop)
        {
            AnimatorController controller = animator.runtimeAnimatorController as AnimatorController;
            foreach (var parameter in controller.parameters)
            {
                string fieldName = "ANIM_PARAM_";
                string animatorName = prop.Name;
                if (animatorName.EndsWith("Animator", StringComparison.OrdinalIgnoreCase))
                    animatorName = animatorName.Substring(0, animatorName.Length - 8);
                if (animatorName.StartsWith("_"))
                    animatorName = animatorName.Substring(1, animatorName.Length - 1);
                if (!(animatorName.StartsWith("as") && char.IsUpper(animatorName[2])))
                    fieldName += animatorName.ToUpper() + "_";
                fieldName += parameter.name.ToUpper();
                CodeMemberField Const = genField("const string", fieldName, false);
                Const.InitExpression = Codo.String(parameter.name);
            }
        }
        protected virtual void onGenList(RectTransform transform, CodeMemberField field, AutoBindFieldInfo fieldInfo)
        {

        }
        protected CodeAttributeDeclaration addAttribute2Field(CodeMemberField field, Object obj, params string[] tags)
        {
            return addAttribute2Field(field, obj, true, tags);
        }

        protected CodeAttributeDeclaration addAttribute2Field(CodeMemberField field, Object obj, bool withPath, params string[] tags)
        {
            CodeAttributeDeclaration autoCompo;
            if (withPath)
            {
                autoCompo = field.CustomAttributes.add(typeof(AutoCompoAttribute).Name,
                    Codo.Int(objFieldDict[obj].instanceId),
                    Codo.String(objFieldDict[obj].path));
            }
            else
            {
                autoCompo = field.CustomAttributes.add(typeof(AutoCompoAttribute).Name,
                    Codo.Int(objFieldDict[obj].instanceId));
            }
            foreach (var tag in tags)
            {
                autoCompo.Arguments.Add(new CodeAttributeArgument(new CodePrimitiveExpression(tag)));
            }
            return autoCompo;
        }

        protected CodeMemberProperty genProp4GO(GameObject gameObject, string propName, string fieldName)
        {
            CodeMemberProperty prop = genProp(MemberAttributes.Public | MemberAttributes.Final, propName, typeof(GameObject));
            prop.HasGet = true;
            addTypeUsing(typeof(GameObject));
            CodeConditionStatement If = Codo.If(Codo.This.getField(fieldName).op(CodeBinaryOperatorType.IdentityEquality, Codo.Null));
            If.TrueStatements.append(Codo.This.getField(fieldName).assign(Codo.This.getProp(NAME_OF_TRANSFORM)
                .getMethod(NAME_OF_FIND).invoke(Codo.getField("PATH" + fieldName.ToUpper()))
                .getProp(NAME_OF_GAMEOBJECT)));
            prop.GetStatements.Add(If);
            prop.GetStatements.Add(new CodeMethodReturnStatement(
                new CodeFieldReferenceExpression(new CodeThisReferenceExpression(),
                fieldName)));
            return prop;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="component"></param>
        /// <param name="propName"></param>
        /// <param name="fieldName">要封装的字段名，供属性引用</param>
        protected CodeMemberProperty genProp4Compo(Component component, string propName, string fieldName)
        {
            CodeMemberProperty prop = genProp(MemberAttributes.Public | MemberAttributes.Final, propName, component.GetType());
            prop.HasGet = true;
            CodeConditionStatement If = Codo.If(Codo.getField(fieldName).op(CodeBinaryOperatorType.IdentityEquality, Codo.Null));
            If.TrueStatements.append(Codo.getField(fieldName).assign(Codo.getProp(NAME_OF_TRANSFORM)
                .getMethod(NAME_OF_FIND).invoke(Codo.getField("PATH" + fieldName.ToUpper()))
                .getMethod(NAME_OF_GETCOMPO, Codo.type(component.GetType().Name)).invoke()));
            prop.GetStatements.Add(If);
            prop.GetStatements.Add(Codo.Return(Codo.getField(fieldName)));
            return prop;
        }
        protected CodeMemberField genField4Compo(Component component, string fieldName)
        {
            return genField(component.GetType(), fieldName);
        }
        /// <summary>
        /// 默认自己就叫_gameObject，子物体叫_子物体名。
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        public virtual string genFieldName4GO(GameObject gameObject)
        {
            if (objFieldDict != null)
            {
                if (!string.IsNullOrEmpty(objFieldDict[gameObject].fieldName))
                    return objFieldDict[gameObject].fieldName;
            }
            if (gameObject == rootGameObject)
                return "_gameObject";
            else
                return "_" + gameObject.name.headToLower();
        }
        /// <summary>
        /// 默认如果是根组件，那么叫做_as类型名，如果是子组件，那么叫_子物体名类型名。
        /// </summary>
        /// <param name="component"></param>
        /// <returns></returns>
        public virtual string genFieldName4Compo(Component component)
        {
            if (objFieldDict != null && !string.IsNullOrEmpty(objFieldDict[component].fieldName))
                return objFieldDict[component].fieldName;
            if (component.gameObject == rootGameObject)
                return "_as" + component.GetType().Name;
            GameObject gameObject = component.gameObject;
            string name = "_" + gameObject.name.headToLower();
            string fullname;
            string typeName = component.GetType().Name;
            if (name.Contains(typeName) ||
                gameObject.name == typeName)
                fullname = name;
            else if (!name.tryMerge(typeName, out fullname))
                fullname = name + typeName;
            if (_type == null)
                return fullname;
            while (_type.Members.OfType<CodeMemberField>().Any(f => f.Name == fullname) &&
                gameObject.transform.parent != null &&
                gameObject.transform.parent.gameObject != rootGameObject)
            {
                gameObject = gameObject.transform.parent.gameObject;
                fullname = "_" + gameObject.name.headToLower() + "_" + fullname.Substring(1, fullname.Length - 1).headToUpper();
            }
            return fullname;
        }
        public virtual string genPropName4Field(string fieldName)
        {
            string propName = fieldName;
            while (propName.StartsWith("_"))
                propName = propName.Substring(1, propName.Length - 1);
            propName = propName.headToLower();
            return propName;
        }
        protected virtual string getSimpleTypeName(Type type)
        {
            return type.Name;
        }
        public virtual IEnumerable<string> ctrlTypes
        {
            get { return _ctrlTypes; }
        }
        public string specifiedTypeName { get; set; }
        public Dictionary<Object, AutoBindFieldInfo> objFieldDict { get; set; }
        public string controllerType { get; set; }
        //按钮
        public Button buttonMain { get; set; }
        //列表
        public GameObject listOrigin { get; set; }
        public string listItemTypeName { get; set; }
        public GameObject rootGameObject { get; set; }
        public CodeTypeDeclaration type
        {
            get { return _type; }
        }
        public CodeMemberMethod initMethod
        {
            get { return _initMethod; }
        }
        public CodeMemberMethod clearMethod
        {
            get { return _clearMethod; }
        }
        protected AutoCompoGenSetting _setting;
        protected Dictionary<Type, ComponentGenerator> _compoGenDict = new Dictionary<Type, ComponentGenerator>();
        protected CodeNamespace _nameSpace;
        protected CodeTypeDeclaration _type;
        protected CodeMemberMethod _initMethod;
        protected CodeMemberMethod _clearMethod;
        public const string CTRL_TYPE_BUTTON = "button";
        public const string CTRL_TYPE_LIST = "list";
        public const string CTRL_TYPE_BUTTON_LIST = "buttonList";
        public const string FIELD_NAME_ORIGIN = "_origin";
        public const string FIELD_NAME_ITEM_POOL = "_itemPool";
        public const string METHOD_NAME_LIST_INIT_POOL = "initPool";
        public const string NAME_OF_GAMEOBJECT = "gameObject";
        public const string NAME_OF_SET_ACTIVE = "SetActive";
        public const string NAME_OF_TRANSFORM = "transform";
        public const string NAME_OF_FIND = "Find";
        public const string NAME_OF_FIND_BY_PATH = "Find";
        public const string NAME_OF_ADDCOMPO = "AddComponent";
        public const string NAME_OF_GETCOMPO = "GetComponent";
        public const string NAME_OF_ONCLICK = "onClick";
        public const string NAME_OF_ADDLISTENER = "AddListener";
        public const string NAME_OF_REMOVELISTENER = "RemoveListener";
        readonly string[] _ctrlTypes =
        {
            CTRL_TYPE_BUTTON,
            CTRL_TYPE_LIST,
            CTRL_TYPE_BUTTON_LIST
        };
    }
}