using System.CodeDom;
using System.CodeDom.Compiler;

namespace Dev2.Runtime.DynamicProxy
{
    internal class XmlSerializerImportOptions
    {
        private CodeCompileUnit codeCompileUnit;

        public XmlSerializerImportOptions(CodeCompileUnit codeCompileUnit)
        {
            this.codeCompileUnit = codeCompileUnit;
        }

        public CodeDomProvider CodeProvider { get; internal set; }
        public System.Web.Services.Description.WebReferenceOptions WebReferenceOptions { get; internal set; }
    }
}