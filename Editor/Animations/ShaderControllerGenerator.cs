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
        public CodeCompileUnit generateGraphicController(Shader shader, string Namespace)
        {
            throw new NotImplementedException();
        }
    }
}