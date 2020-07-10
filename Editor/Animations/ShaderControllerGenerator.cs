using System;
using System.IO;
using UnityEngine;
using System.CodeDom;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine.Rendering;

namespace BJSYGameCore.Animations
{
    public class ShaderControllerGenerator
    {
        [MenuItem("Assets/Create/Generate ShaderController", true, 410)]
        public static bool validateGenerateScript()
        {
            if (Selection.activeObject is Shader)
                return true;
            return false;
        }
        [MenuItem("Assets/Create/Generate ShaderController", false, 410)]
        public static void generateScript()
        {
            if (Selection.activeObject is Shader shader)
            {
                FileInfo shaderFile = new FileInfo(AssetDatabase.GetAssetPath(shader));
                CodeCompileUnit unit = new ShaderControllerGenerator().generateGraphicController(shader, "Animations");
                FileInfo scriptFile = new FileInfo(shaderFile.Directory + "/" + Path.GetFileNameWithoutExtension(shaderFile.Name) + "Controller.cs");
                CodeDOMHelper.writeUnitToFile(scriptFile, unit);
                AssetDatabase.Refresh();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="shader"></param>
        /// <param name="Namespace"></param>
        /// <returns></returns>
        /// <remarks>
        /// CodeCompileUnit相当于一个.cs文件中的内容，结构从Namespace开始，往下是Class……你懂得。
        /// 所以这个方法只需要往Unit里面添加一个类，类的名字等于Shader的名字+Controller，继承GraphMatPropCtrl，
        /// 然后枚举Shader的所有属性，往类里面添加每一个属性对应的字段，在Update方法里面设置material的对应属性为字段的值就行了。
        /// Reset方法的内容差不多。
        /// 可以参考一下ServantMatCtrl，这个玩意是我手写的。
        /// </remarks>
        public CodeCompileUnit generateGraphicController(Shader shader, string Namespace)
        {
            var ccu = new CodeCompileUnit();
            {
                var ns = new CodeNamespace(Namespace);
                {
                    var tClass = new CodeTypeDeclaration(Path.GetFileName(shader.name) + "Controller")
                    {
                        IsClass = true,
                        IsPartial = true,
                        Attributes = MemberAttributes.Public,
                    };
                    {
                        // 设置基类
                        tClass.BaseTypes.Add(new CodeTypeReference(typeof(GraphMatPropCtrl)));

                        // 添加方法
                        var updateMethod = new CodeMemberMethod()
                        {
                            Name = "Update",
                            Attributes = MemberAttributes.Override | MemberAttributes.Family,
                            ReturnType = new CodeTypeReference(typeof(void))
                        };
                        tClass.Members.Add(updateMethod);

                        var resetMethod = new CodeMemberMethod()
                        {
                            Name = "Reset",
                            Attributes = MemberAttributes.Family,
                            ReturnType = new CodeTypeReference(typeof(void))
                        };
                        tClass.Members.Add(resetMethod);

                        // 添加Prop以及向方法中添加代码
                        for (int i = 0; i < shader.GetPropertyCount(); i++)
                        {
                            var pName = shader.GetPropertyName(i);
                            var pType = shader.GetPropertyType(i);
                            var cType = GetType(pType);

                            var pMember = new CodeMemberField(cType, pName)
                            {
                                Attributes = MemberAttributes.Public
                            };
                            pMember.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference(typeof(SerializeField))));
                            tClass.Members.Add(pMember);

                            resetMethod.Statements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), pName), MaterialGetExp(pType, pName)));
                            updateMethod.Statements.Add(MaterialSetExp(pType, pName, pName));
                        }

                        // 添加 SHADER_ID
                        var shader_id = new CodeMemberField(typeof(int), "SHADER_ID")
                        {
                            Attributes = MemberAttributes.Public | MemberAttributes.Const,
                            InitExpression = new CodePrimitiveExpression(shader.GetInstanceID())
                        };
                        tClass.Members.Add(shader_id);

                    }
                    ns.Types.Add(tClass);
                }
                ccu.Namespaces.Add(ns);
            }
            return ccu;
        }

        static Type GetType(ShaderPropertyType t)
        {
            switch (t)
            {
                case ShaderPropertyType.Color:
                    return typeof(Color);
                case ShaderPropertyType.Vector:
                    return typeof(Vector4);
                case ShaderPropertyType.Float:
                    return typeof(float);
                case ShaderPropertyType.Range:
                    return typeof(Single);
                case ShaderPropertyType.Texture:
                    return typeof(Texture);
                default:
                    return typeof(float);
            }
        }

        static CodeMethodInvokeExpression MaterialSetExp(ShaderPropertyType t, string name, string val)
        {
            var mat = new CodePropertyReferenceExpression(new CodeBaseReferenceExpression(), "material");
            string methodName = "InvalidExp";
            switch (t)
            {
                case ShaderPropertyType.Color:
                    methodName = "SetColor";
                    break;
                case ShaderPropertyType.Vector:
                    methodName = "SetVector";
                    break;
                case ShaderPropertyType.Range:
                case ShaderPropertyType.Float:
                    methodName = "SetFloat";
                    break;
                case ShaderPropertyType.Texture:
                    methodName = "SetTexture";
                    break;
                default:
                    break;
            }
            return new CodeMethodInvokeExpression(
                new CodeMethodReferenceExpression(mat, methodName),
                new CodePrimitiveExpression(name),
                new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), val));
        }

        static CodeMethodInvokeExpression MaterialGetExp(ShaderPropertyType t, string name)
        {
            var mat = new CodePropertyReferenceExpression(new CodeBaseReferenceExpression(), "material");
            string methodName = "InvalidExp";
            switch (t)
            {
                case ShaderPropertyType.Color:
                    methodName = "GetColor";
                    break;
                case ShaderPropertyType.Vector:
                    methodName = "GetVector";
                    break;
                case ShaderPropertyType.Range:
                case ShaderPropertyType.Float:
                    methodName = "GetFloat";
                    break;
                case ShaderPropertyType.Texture:
                    methodName = "GetTexture";
                    break;
                default:
                    break;
            }
            return new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(mat, methodName), new CodePrimitiveExpression(name));
        }
    }
}