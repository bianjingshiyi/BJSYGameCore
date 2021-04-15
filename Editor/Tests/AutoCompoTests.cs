using NUnit.Framework;
using UnityEngine;
using BJSYGameCore.AutoCompo;
using UnityEngine.UI;
using System.CodeDom;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using System;
using Object = UnityEngine.Object;
namespace Tests
{
    public class AutoCompoTests
    {
        //[Test]
        //public void generateTest()
        //{
        //    GameObject gameObject = new GameObject("Panel.RectTransform");
        //    gameObject.AddComponent<RectTransform>();
        //    new GameObject("return.Image,Button").transform.SetParent(gameObject.transform);
        //    gameObject = gameObject.transform.Find("return.Image,Button").gameObject;
        //    gameObject.AddComponent<Image>();
        //    gameObject.AddComponent<Button>();
        //    new GameObject("background").transform.SetParent(gameObject.transform.root);
        //    gameObject = gameObject.transform.root.Find("background").gameObject;
        //    gameObject.AddComponent<Image>();
        //    gameObject = gameObject.transform.root.gameObject;

        //    var unit = new AutoCompoGenerator().genScript4GO(gameObject, new AutoCompoGenSetting()
        //    {
        //        usings = new string[]
        //        {
        //            "UnityEngine",
        //            "BJSYGameCore.AutoCompo"
        //        },
        //        Namespace = "UI",
        //        baseTypes = new string[]
        //        {
        //            "MonoBehaviour"
        //        },
        //        fieldAttributes = new string[]
        //        {
        //            "SerializeField"
        //        }
        //    });

        //    CodeNamespace Namespace = unit.Namespaces[0];
        //    Assert.True(Namespace.Imports.OfType<CodeNamespaceImport>()
        //                                 .Any(u => u.Namespace == "UnityEngine"));
        //    Assert.True(Namespace.Imports.OfType<CodeNamespaceImport>()
        //                                 .Any(u => u.Namespace == "UnityEngine.UI"));
        //    Assert.True(Namespace.Imports.OfType<CodeNamespaceImport>()
        //                                 .Any(u => u.Namespace == "BJSYGameCore.AutoCompo"));
        //    Assert.AreEqual("UI", Namespace.Name);
        //    CodeTypeDeclaration Class = Namespace.Types[0];
        //    Assert.AreEqual(nameof(AutoCompoAttribute), Class.CustomAttributes[0].Name);
        //    Assert.AreEqual(gameObject.GetInstanceID(), (Class.CustomAttributes[0].Arguments[0].Value as CodePrimitiveExpression).Value);
        //    Assert.AreEqual(TypeAttributes.Public | TypeAttributes.Class, Class.TypeAttributes);
        //    Assert.AreEqual("Panel", Class.Name);

        //    int index = 0;
        //    CodeMemberMethod autoBindMethod = Class.Members[index] as CodeMemberMethod;
        //    Assert.AreEqual(MemberAttributes.Public | MemberAttributes.Final, autoBindMethod.Attributes);
        //    Assert.AreEqual("autoBind", autoBindMethod.Name);
        //    index++;
        //    CodeMemberField field = Class.Members[index] as CodeMemberField;
        //    Assert.AreEqual("SerializeField", field.CustomAttributes[0].Name);
        //    Assert.AreEqual(MemberAttributes.Private | MemberAttributes.Final, field.Attributes);
        //    Assert.AreEqual("RectTransform", field.Type.BaseType);
        //    Assert.AreEqual("_asRectTransform", field.Name);
        //    index++;
        //    CodeMemberProperty prop = Class.Members[index] as CodeMemberProperty;
        //    Assert.AreEqual(MemberAttributes.Public | MemberAttributes.Final, prop.Attributes);
        //    Assert.AreEqual("RectTransform", prop.Type.BaseType);
        //    Assert.AreEqual("asRectTransform", prop.Name);
        //    Assert.True(prop.HasGet);
        //    index++;
        //    //...剩下的我懒得写了，自己对着下面注释掉的代码补完测试代码吧。
        //    //注意不要把不包括在内的background生成进去了哦。
        //    CodeMemberField imageField = Class.Members[index] as CodeMemberField;
        //    Assert.AreEqual("SerializeField", imageField.CustomAttributes[0].Name);
        //    Assert.AreEqual(MemberAttributes.Private | MemberAttributes.Final, imageField.Attributes);
        //    Assert.AreEqual("Image", imageField.Type.BaseType);
        //    Assert.AreEqual("_returnImage", imageField.Name);
        //    index++;
        //    CodeMemberProperty imageProp = Class.Members[index] as CodeMemberProperty;
        //    Assert.AreEqual(MemberAttributes.Public | MemberAttributes.Final, imageProp.Attributes);
        //    Assert.AreEqual("Image", imageProp.Type.BaseType);
        //    Assert.AreEqual("returnImage", imageProp.Name);
        //    Assert.True(imageProp.HasGet);
        //    index++;
        //    CodeMemberField buttonField = Class.Members[index] as CodeMemberField;
        //    Assert.AreEqual("SerializeField", buttonField.CustomAttributes[0].Name);
        //    Assert.AreEqual(MemberAttributes.Private | MemberAttributes.Final, buttonField.Attributes);
        //    Assert.AreEqual("Button", buttonField.Type.BaseType);
        //    Assert.AreEqual("_returnButton", buttonField.Name);
        //    index++;
        //    CodeMemberProperty buttonProp = Class.Members[index] as CodeMemberProperty;
        //    Assert.AreEqual(MemberAttributes.Public | MemberAttributes.Final, buttonProp.Attributes);
        //    Assert.AreEqual("Button", buttonProp.Type.BaseType);
        //    Assert.AreEqual("returnButton", buttonProp.Name);
        //    Assert.True(buttonProp.HasGet);
        //}
        [Test]
        public void generateTest()
        {
            GameObject gameObject = new GameObject("AutoUI");
            Animator animator = gameObject.AddComponent<Animator>();
            GameObject child = new GameObject("Child");
            child.transform.parent = gameObject.transform;
            AutoCompoGenerator generator = new AutoCompoGenerator()
            {
                specifiedTypeName = "AutoUIView",
                objFieldDict = new Dictionary<Object, AutoBindFieldInfo>()
                {
                    { animator, new AutoBindFieldInfo(animator.GetInstanceID(),"./",typeof(Animator),null,"_animator") },
                    { child, new AutoBindFieldInfo(child.GetInstanceID(),"./Child",typeof(GameObject),null,"_child") }
                },
            };
            var unit = generator.genScript4GO(gameObject, new AutoCompoGenSetting()
            {
                baseTypes = new string[] { nameof(MonoBehaviour) },
                fieldAttributes = new string[] { nameof(SerializeField) },
                Namespace = "UI",
            });

            CodeNamespace nameSpace = unit.Namespaces[0];
            Assert.True(nameSpace.Imports.OfType<CodeNamespaceImport>().Any(n => n.Namespace == "UnityEngine"));
            Assert.True(nameSpace.Imports.OfType<CodeNamespaceImport>().Any(n => n.Namespace == "System"));
            Assert.AreEqual("UI", nameSpace.Name);
            CodeTypeDeclaration type = nameSpace.Types[0];
            Assert.AreEqual("AutoUIView", type.Name);
            CodeMemberField field = type.Members.OfType<CodeMemberField>().First(f => f.Name == "_animator");
            CodeAttributeDeclaration autoCompo = field.CustomAttributes.OfType<CodeAttributeDeclaration>().First(d => d.Name == nameof(AutoCompoAttribute));
            Assert.AreEqual(animator.GetInstanceID(), (autoCompo.Arguments[0].Value as CodePrimitiveExpression).Value);
            Assert.AreEqual("./", (autoCompo.Arguments[1].Value as CodePrimitiveExpression).Value);
            CodeAttributeDeclaration serFie = field.CustomAttributes.OfType<CodeAttributeDeclaration>().First(d => d.Name == nameof(SerializeField));
            Assert.NotNull(serFie);
            Assert.AreEqual(nameof(Animator), field.Type.BaseType);
            field = type.Members.OfType<CodeMemberField>().First(f => f.Name == "_child");
            autoCompo = field.CustomAttributes.OfType<CodeAttributeDeclaration>().First(d => d.Name == nameof(AutoCompoAttribute));
            Assert.AreEqual(child.GetInstanceID(), (autoCompo.Arguments[0].Value as CodePrimitiveExpression).Value);
            Assert.AreEqual("./Child", (autoCompo.Arguments[1].Value as CodePrimitiveExpression).Value);
            serFie = field.CustomAttributes.OfType<CodeAttributeDeclaration>().First(d => d.Name == nameof(SerializeField));
            Assert.NotNull(serFie);
            Assert.AreEqual(nameof(GameObject), field.Type.BaseType);
        }
        [Test]
        public void genButtonTest()
        {
            GameObject gameObject = new GameObject("Button");
            Button button = gameObject.AddComponent<Button>();
            AutoCompoGenerator generator = new AutoCompoGenerator()
            {
                specifiedTypeName = "CommonButton",
                objFieldDict = new Dictionary<Object, AutoBindFieldInfo>()
                {
                    { button, new AutoBindFieldInfo(button.GetInstanceID(),"./",typeof(Button),null,"_button") }
                }
            };
            var unit = generator.genScript4GO(gameObject, new AutoCompoGenSetting()
            {
                Namespace = "UI",
                baseTypes = new string[] { nameof(MonoBehaviour) },
                fieldAttributes = new string[] { nameof(SerializeField) }
            });

            //字段
            CodeTypeDeclaration type = unit.Namespaces[0].Types[0];
            CodeMemberField field = type.Members.OfType<CodeMemberField>().First(f => f.Name == "_button");
            Assert.NotNull(field);
            //属性
            CodeMemberProperty prop = type.Members.OfType<CodeMemberProperty>().First(f => f.Name == "button");
            Assert.NotNull(prop);
            Assert.True(prop.HasGet);
            Assert.False(prop.HasSet);
            //初始化
            CodeMemberMethod init = type.Members.OfType<CodeMemberMethod>().First(m => m.Name == "init");
            CodeAssignStatement assign = init.Statements.OfType<CodeAssignStatement>().First();
            Assert.NotNull(assign);
            //事件
            CodeMemberEvent Event = type.Members.OfType<CodeMemberEvent>().First(m => m.Name == "onButtonClick");
            Assert.AreEqual(nameof(Action), Event.Type.BaseType);
        }
        [Test]
        public void genButtonCtrlTest()
        {
            GameObject gameObject = new GameObject("Button");
            Button button = gameObject.AddComponent<Button>();
            AutoCompoGenerator generator = new AutoCompoGenerator()
            {
                specifiedTypeName = "CommonButton",
                objFieldDict = new Dictionary<Object, AutoBindFieldInfo>()
                {
                    { button, new AutoBindFieldInfo(button.GetInstanceID(),"./",typeof(Button),null,null) }
                },
                controllerType = AutoCompoGenerator.CTRL_TYPE_BUTTON,
                buttonMain = button
            };
            var unit = generator.genScript4GO(gameObject, new AutoCompoGenSetting()
            {
                Namespace = "UI",
                baseTypes = new string[] { nameof(MonoBehaviour) },
                fieldAttributes = new string[] { nameof(SerializeField) }
            });
            CodeTypeDeclaration type = unit.Namespaces[0].Types[0];
            CodeMemberField field = type.Members.OfType<CodeMemberField>().First(f => f.Name == "_asButton");
            //主按钮标记
            CodeAttributeDeclaration autoCompo = field.CustomAttributes.OfType<CodeAttributeDeclaration>().First(a => a.Name == typeof(AutoCompoAttribute).Name);
            Assert.AreEqual(button.GetInstanceID(), (autoCompo.Arguments[0].Value as CodePrimitiveExpression).Value);
            Assert.AreEqual("./", (autoCompo.Arguments[1].Value as CodePrimitiveExpression).Value);
            Assert.AreEqual("mainButton", (autoCompo.Arguments[2].Value as CodePrimitiveExpression).Value);
            //事件
            CodeMemberEvent Event = type.Members.OfType<CodeMemberEvent>().First(e => e.Name == "onClick");
            Assert.AreEqual("CommonButton", Event.Type.TypeArguments[0].BaseType);
            //回调函数
            CodeMemberMethod callback = type.Members.OfType<CodeMemberMethod>().First(m => m.Name == "clickCallback");
            Assert.NotNull(callback);
        }
        [Test]
        public void genListCtrlTest()
        {
            GameObject gameObject = new GameObject("Content");
            GameObject item = new GameObject("Item");
            item.transform.SetParent(gameObject.transform);
            AutoCompoGenerator generator = new AutoCompoGenerator()
            {
                specifiedTypeName = "ItemList",
                objFieldDict = new Dictionary<Object, AutoBindFieldInfo>()
                {
                    { item, new AutoBindFieldInfo(item.GetInstanceID(),"./Item",typeof(GameObject),null,null) }
                },
                controllerType = AutoCompoGenerator.CTRL_TYPE_LIST,
                listOrigin = item,
            };
            var unit = generator.genScript4GO(gameObject, new AutoCompoGenSetting()
            {
                Namespace = "UI",
                baseTypes = new string[] { nameof(MonoBehaviour) },
                fieldAttributes = new string[] { nameof(SerializeField) }
            });
            CodeTypeDeclaration type = unit.Namespaces[0].Types[0];
            CodeTypeDeclaration poolType = type.Members.OfType<CodeTypeDeclaration>().First(t => t.Name == "ItemPool");
            CodeMemberField poolField = type.Members.OfType<CodeMemberField>().First(f => f.Name == "_itemPool");
            Assert.AreEqual(poolType.Name, poolField.Type.BaseType);
            CodeMemberField itemCreatePartial = type.Members.OfType<CodeMemberField>().First(f => f.Name.Contains("onItemCreate"));
            Assert.NotNull(itemCreatePartial);
            CodeMemberField itemRemovePartial = type.Members.OfType<CodeMemberField>().First(f => f.Name.Contains("onItemRemove"));
            Assert.NotNull(itemRemovePartial);
            CodeMemberMethod initPoolMethod = type.Members.OfType<CodeMemberMethod>().First(m => m.Name == "initPool");
            Assert.NotNull(initPoolMethod);
        }
    }
}
//namespace UI
//{
//    using UnityEngine;
//    using UnityEngine.UI;
//    using BJSYGameCore.AutoCompo;

//    [AutoCompo(1457)]
//    public class Panel : MonoBehaviour
//    {
//        [SerializeField]
//        RectTransform _asRectTransform;
//        public RectTransform asRectTransform
//        {
//            get { return _asRectTransform; }
//        }
//        [SerializeField]
//        Image _returnImage;
//        public Image returnImage
//        {
//            get { return _returnImage; }
//        }
//        [SerializeField]
//        Button _returnButton;
//        public Button returnButton
//        {
//            get { return _returnButton; }
//        }
//        public void autoBind()
//        {
//            _asRectTransform = GetComponent<RectTransform>();
//            _returnImage = transform.Find("return").GetComponent<Image>();
//            _returnButton = transform.Find("return").GetComponent<Button>();
//        }
//    }
//}