using System;
using System.IO;
using UnityEngine;
using System.CodeDom;
using UnityEditor;
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
                CodeCompileUnit unit = new CodeCompileUnit(); //new ShaderControllerGenerator().generateGraphicController(shader, "Animations");
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
            throw new NotImplementedException();
        }
    }
}