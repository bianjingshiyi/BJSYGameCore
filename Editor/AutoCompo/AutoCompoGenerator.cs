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
            _rootGameObject = gameObject;
            CodeCompileUnit unit = new CodeCompileUnit();
            //命名空间，引用
            _nameSpace = new CodeNamespace(setting.Namespace);
            unit.Namespaces.Add(_nameSpace);
            foreach (string import in setting.usings)
            {
                _nameSpace.Imports.Add(new CodeNamespaceImport(import));
            }
            //类
            _type = new CodeTypeDeclaration();
            _nameSpace.Types.Add(_type);
            genType4RootGO();
            return unit;
        }
        /// <summary>
        /// 默认生成一个自动绑定方法。
        /// </summary>
        protected virtual void genType4RootGO()
        {
            addTypeUsing(typeof(AutoCompoAttribute));
            _type.CustomAttributes.Add(new CodeAttributeDeclaration(typeof(AutoCompoAttribute).Name,
                new CodeAttributeArgument(new CodePrimitiveExpression(_rootGameObject.GetInstanceID()))));
            _type.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            _type.IsPartial = true;
            _type.IsClass = true;
            _type.Name = genTypeName4GO(_rootGameObject);
            foreach (var baseType in _setting.baseTypes)
            {
                _type.BaseTypes.Add(baseType);
            }
            genMembers();
            genRootGameObject();
        }
        /// <summary>
        /// 默认生成autoBind方法。
        /// </summary>
        protected virtual void genMembers()
        {
            _initMethod = genMethod(MemberAttributes.Public | MemberAttributes.Final, typeof(void), "init");
            _clearMethod = genMethod(MemberAttributes.Public | MemberAttributes.Final, typeof(void), "clear");
            if (controllerType == CTRL_TYPE_LIST || controllerType == CTRL_TYPE_BUTTON_LIST)
            {
                //ItemPool类型
                CodeTypeDeclaration itemPoolType = new CodeTypeDeclaration();
                _type.Members.Add(itemPoolType);
                itemPoolType.CustomAttributes.Add(new CodeAttributeDeclaration(typeof(SerializableAttribute).Name));
                itemPoolType.Attributes = MemberAttributes.Public | MemberAttributes.Final;
                itemPoolType.IsClass = true;
                itemPoolType.Name = "ItemPool";
                itemPoolType.BaseTypes.Add(new CodeTypeReference(typeof(ComponentPool<>).Name, new CodeTypeReference(listItemTypeName)));
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
        /// <summary>
        /// 根据提供的字段字典进行生成。
        /// </summary>
        protected virtual void genRootGameObject()
        {
            if (controllerType == CTRL_TYPE_BUTTON && buttonMain == null)
                throw new InvalidOperationException("控件类型为按钮却没有主按钮");
            foreach (var fieldInfo in objFieldDict.Values.Where(f => f != null && f.instanceId != 0))
            {
                if (fieldInfo.targetType == typeof(GameObject))
                    genGameObject(TransformHelper.findGameObjectByPath(_rootGameObject, fieldInfo.path));
                else
                    genCompo(findComponentByPath(fieldInfo.path, fieldInfo.targetType));
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
            addTypeUsing(typeof(GameObject));
            var field = genField(typeof(GameObject).Name, fieldName);
            addAttribute2Field(field, gameObject);
        }
        /// <summary>
        /// 默认生成字段，属性，以及初始化语句。
        /// </summary>
        /// <param name="component"></param>
        protected virtual void genCompo(Component component)
        {
            addTypeUsing(component.GetType());
            //字段
            var field = genField4Compo(component, genFieldName4Compo(component));
            var autoCompo = addAttribute2Field(field, component);
            //属性
            string propName = field.Name;
            while (propName.StartsWith("_"))
                propName = propName.Substring(1, propName.Length - 1);
            propName = propName.headToLower();
            var prop = genProp4Compo(component, propName, field.Name);
            //初始化
            addTypeUsing(typeof(TransformHelper));
            _initMethod.Statements.append(Codo.assign(Codo.This.getField(field.Name),
                Codo.This.getProp(NAME_OF_TRANSFORM).getMethod(NAME_OF_FIND_BY_PATH).getMethod(NAME_OF_GETCOMPO, Codo.type(component.GetType().Name)).index()));
            if (component.GetType() == typeof(Button) || component.GetType().IsSubclassOf(typeof(Button)))
            {
                //是按钮
                if (controllerType == CTRL_TYPE_BUTTON && component == buttonMain)
                    autoCompo.Arguments.Add(new CodeAttributeArgument(new CodePrimitiveExpression("mainButton")));
                string name = prop.Name;
                name = name.headToUpper();
                //事件
                CodeMemberEvent Event;
                if (controllerType == CTRL_TYPE_BUTTON && component == buttonMain)
                    Event = genEvent(typeof(Action).Name, "onClick", Codo.type(typeName));
                else
                    Event = genEvent(typeof(Action).Name, "on" + name + "Click");
                //回调函数
                CodeMemberMethod callbackMethod;
                if (controllerType == CTRL_TYPE_BUTTON && component == buttonMain)
                {
                    callbackMethod = genMethod(MemberAttributes.Private | MemberAttributes.Final, typeof(void), "clickCallback");
                    callbackMethod.Statements.Add(new CodeConditionStatement(
                    new CodeBinaryOperatorExpression(Codo.This.getEvent(Event.Name), CodeBinaryOperatorType.IdentityInequality, Codo.Null),
                        Codo.This.getEvent(Event.Name).invoke(Codo.This).statement()));
                }
                else
                {
                    callbackMethod = genMethod(MemberAttributes.Private | MemberAttributes.Final, typeof(void), name + "ClickCallback");
                    callbackMethod.Statements.Add(new CodeConditionStatement(
                        new CodeBinaryOperatorExpression(Codo.This.getEvent(Event.Name), CodeBinaryOperatorType.IdentityInequality, Codo.Null),
                            Codo.This.getEvent(Event.Name).invoke().statement()));
                }
                //注册
                _initMethod.Statements.Add(Codo.This.getField(field.Name).getProp(NAME_OF_ONCLICK)
                    .getMethod(NAME_OF_ADDLISTENER).invoke(Codo.This.getMethod(callbackMethod.Name)).statement());
                //注销
                _clearMethod.Statements.Add(Codo.This.getField(field.Name).getProp(NAME_OF_ONCLICK)
                    .getMethod(NAME_OF_REMOVELISTENER).invoke(Codo.This.getMethod(callbackMethod.Name)).statement());
            }
        }
        private CodeAttributeDeclaration addAttribute2Field(CodeMemberField field, Object obj, params string[] tags)
        {
            CodeAttributeDeclaration autoCompo = new CodeAttributeDeclaration(typeof(AutoCompoAttribute).Name,
                            new CodeAttributeArgument(new CodePrimitiveExpression(objFieldDict[obj].instanceId)),
                            new CodeAttributeArgument(new CodePrimitiveExpression(objFieldDict[obj].path)));
            foreach (var tag in tags)
            {
                autoCompo.Arguments.Add(new CodeAttributeArgument(new CodePrimitiveExpression(tag)));
            }
            field.CustomAttributes.Add(autoCompo);
            return autoCompo;
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
            prop.GetStatements.Add(new CodeMethodReturnStatement(
                new CodeFieldReferenceExpression(new CodeThisReferenceExpression(),
                fieldName)));
            return prop;
        }
        protected CodeMemberField genField4Compo(Component component, string fieldName)
        {
            return genField(component.GetType(), fieldName);
        }
        protected virtual string genTypeName4GO(GameObject gameObject)
        {
            if (string.IsNullOrEmpty(typeName))
                throw new InvalidOperationException("类名不能为空");
            if (char.IsDigit(typeName[0]) || !typeName.All(c => char.IsLetterOrDigit(c) || c == '_'))
                throw new InvalidOperationException("类名" + typeName + "非法");
            return typeName;
            //string typeName;
            //string[] compoTypes;
            //if (tryParseGOName(gameObject.name, out typeName, out compoTypes))
            //    return typeName;
            //else
            //    throw new FormatException(gameObject.name + "不符合格式\\w.\\w*");
        }
        /// <summary>
        /// 默认自己就叫_gameObject，子物体叫_子物体名。
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        protected virtual string genFieldName4GO(GameObject gameObject)
        {
            if (!string.IsNullOrEmpty(objFieldDict[gameObject].fieldName))
                return objFieldDict[gameObject].fieldName;
            if (gameObject == _rootGameObject)
                return "_gameObject";
            else
                return "_" + gameObject.name;
        }
        /// <summary>
        /// 默认如果是根组件，那么叫做_as类型名，如果是子组件，那么叫_子物体名类型名。
        /// </summary>
        /// <param name="component"></param>
        /// <returns></returns>
        protected virtual string genFieldName4Compo(Component component)
        {
            if (!string.IsNullOrEmpty(objFieldDict[component].fieldName))
                return objFieldDict[component].fieldName;
            if (component.gameObject == _rootGameObject)
                return "_as" + component.GetType().Name;
            GameObject gameObject = component.gameObject;
            string name = "_" + gameObject.name;
            string fullname;
            string typeName = component.GetType().Name;
            if (name.Contains(typeName))
                fullname = name;
            else if (!name.tryMerge(typeName, out fullname))
                fullname = name + typeName;
            while (_type.Members.OfType<CodeMemberField>().Any(f => f.Name == fullname))
            {
                gameObject = gameObject.transform.parent.gameObject;
                fullname = "_" + gameObject.name + "_" + fullname.Substring(1, fullname.Length - 1);
            }
            return fullname;
        }
        Component findComponentByPath(string path, Type type)
        {
            return TransformHelper.findGameObjectByPath(_rootGameObject, path).GetComponent(type);
        }
        public virtual IEnumerable<string> ctrlTypes
        {
            get { return _ctrlTypes; }
        }
        public string typeName { get; set; }
        public Dictionary<Object, AutoBindFieldInfo> objFieldDict { get; set; }
        public string controllerType { get; set; }
        //按钮
        public Button buttonMain { get; set; }
        //列表
        public GameObject listOrigin { get; set; }
        public string listItemTypeName { get; set; }
        protected AutoCompoGenSetting _setting;
        protected GameObject _rootGameObject;
        protected CodeNamespace _nameSpace;
        protected CodeTypeDeclaration _type;
        protected CodeMemberMethod _initMethod;
        protected CodeMemberMethod _clearMethod;
        public const string CTRL_TYPE_BUTTON = "button";
        public const string CTRL_TYPE_LIST = "list";
        public const string CTRL_TYPE_BUTTON_LIST = "buttonList";
        protected const string FIELD_NAME_ORIGIN = "_origin";
        protected const string FIELD_NAME_ITEM_POOL = "_itemPool";
        protected const string METHOD_NAME_LIST_INIT_POOL = "initPool";
        protected const string NAME_OF_GAMEOBJECT = "gameObject";
        protected const string NAME_OF_SET_ACTIVE = "SetActive";
        protected const string NAME_OF_TRANSFORM = "transform";
        protected const string NAME_OF_FIND = "Find";
        protected const string NAME_OF_FIND_BY_PATH = "findByPath";
        protected const string NAME_OF_ADDCOMPO = "AddComponent";
        protected const string NAME_OF_GETCOMPO = "GetComponent";
        protected const string NAME_OF_ONCLICK = "onClick";
        protected const string NAME_OF_ADDLISTENER = "AddListener";
        protected const string NAME_OF_REMOVELISTENER = "RemoveListener";
        readonly string[] _ctrlTypes = new string[]
        {
            CTRL_TYPE_BUTTON,
            CTRL_TYPE_LIST,
            CTRL_TYPE_BUTTON_LIST
        };
    }
}