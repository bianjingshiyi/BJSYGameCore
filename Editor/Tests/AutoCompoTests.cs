using NUnit.Framework;
using UnityEngine;
using BJSYGameCore.AutoCompo;
using UnityEngine.UI;
using System.CodeDom;
using System.Reflection;
namespace Tests
{
    public class AutoCompoTests
    {
        [Test]
        public void generateTest()
        {
            GameObject gameObject = new GameObject("Panel.RectTransform");
            gameObject.AddComponent<RectTransform>();
            new GameObject("return.Image,Button").transform.SetParent(gameObject.transform);
            gameObject = gameObject.transform.Find("return.Image,Button").gameObject;
            gameObject.AddComponent<Image>();
            gameObject.AddComponent<Button>();
            new GameObject("background").transform.SetParent(gameObject.transform.root);
            gameObject = gameObject.transform.root.Find("background").gameObject;
            gameObject.AddComponent<Image>();
            gameObject = gameObject.transform.root.gameObject;

            var unit = AutoCompoGenerator.genScript4GO(gameObject, new AutoCompoGenSetting()
            {
                usings = new string[]
                {
                    "UnityEngine",
                    "UnityEngine.UI",
                    "BJSYGameCore.AutoCompo"
                },
                Namespace = "UI",
                baseTypes = new string[]
                {
                    "MonoBehaviour"
                },
                fieldAttributes = new string[]
                {
                    "SerializeField"
                }
            });

            CodeNamespace Namespace = unit.Namespaces[0];
            Assert.AreEqual("UnityEngine", Namespace.Imports[0].Namespace);
            Assert.AreEqual("UnityEngine.UI", Namespace.Imports[1].Namespace);
            Assert.AreEqual("BJSYGameCore.AutoCompo", Namespace.Imports[2].Namespace);
            Assert.AreEqual("UI", Namespace.Name);
            CodeTypeDeclaration Class = Namespace.Types[0];
            Assert.AreEqual("AutoCompo", Class.CustomAttributes[0].Name);
            Assert.AreEqual(gameObject.GetInstanceID(), (Class.CustomAttributes[0].Arguments[0].Value as CodePrimitiveExpression).Value);
            Assert.AreEqual(TypeAttributes.Public | TypeAttributes.Class, Class.TypeAttributes);
            Assert.AreEqual("Panel", Class.Name);

            CodeMemberField field = Class.Members[0] as CodeMemberField;
            Assert.AreEqual("SerializeField", field.CustomAttributes[0].Name);
            Assert.AreEqual(MemberAttributes.Private | MemberAttributes.Final, field.Attributes);
            Assert.AreEqual("RectTransform", field.Type.BaseType);
            Assert.AreEqual("_asRectTransform", field.Name);
            CodeMemberProperty prop = Class.Members[1] as CodeMemberProperty;
            Assert.AreEqual(MemberAttributes.Public | MemberAttributes.Final, prop.Attributes);
            Assert.AreEqual("RectTransform", prop.Type.BaseType);
            Assert.AreEqual("asRectTransform", prop.Name);
            Assert.True(prop.HasGet);
            //...剩下的我懒得写了，自己对着下面注释掉的代码补完测试代码吧。
            //注意不要把不包括在内的background生成进去了哦。
            CodeMemberField imageField = Class.Members[2] as CodeMemberField;
            Assert.AreEqual("SerializeField", imageField.CustomAttributes[0].Name);
            Assert.AreEqual(MemberAttributes.Private | MemberAttributes.Final, imageField.Attributes);
            Assert.AreEqual("Image", imageField.Type.BaseType);
            Assert.AreEqual("_returnImage", imageField.Name);
            CodeMemberProperty imageProp = Class.Members[3] as CodeMemberProperty;
            Assert.AreEqual(MemberAttributes.Public | MemberAttributes.Final, imageProp.Attributes);
            Assert.AreEqual("Image", imageProp.Type.BaseType);
            Assert.AreEqual("returnImage", imageProp.Name);
            Assert.True(imageProp.HasGet);

            CodeMemberField buttonField = Class.Members[4] as CodeMemberField;
            Assert.AreEqual("SerializeField", buttonField.CustomAttributes[0].Name);
            Assert.AreEqual(MemberAttributes.Private | MemberAttributes.Final, buttonField.Attributes);
            Assert.AreEqual("Button", buttonField.Type.BaseType);
            Assert.AreEqual("_returnButton", buttonField.Name);
            CodeMemberProperty buttonProp = Class.Members[5] as CodeMemberProperty;
            Assert.AreEqual(MemberAttributes.Public | MemberAttributes.Final, buttonProp.Attributes);
            Assert.AreEqual("Button", buttonProp.Type.BaseType);
            Assert.AreEqual("returnButton", buttonProp.Name);
            Assert.True(buttonProp.HasGet);

            CodeMemberMethod autoBindMethod = Class.Members[6] as CodeMemberMethod;
            Assert.AreEqual(MemberAttributes.Public | MemberAttributes.Final, autoBindMethod.Attributes);
            Assert.AreEqual("autoBind", autoBindMethod.Name);

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