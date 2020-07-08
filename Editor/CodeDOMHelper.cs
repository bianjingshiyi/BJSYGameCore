using System.IO;
using System.CodeDom;
using System.CodeDom.Compiler;

namespace BJSYGameCore
{
    static class CodeDOMHelper
    {
        public static void writeUnitToFile(FileInfo fileInfo, CodeCompileUnit unit)
        {
            using (StreamWriter writer = new StreamWriter(fileInfo.Create()))
            {
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
        }
    }
}