using System.CodeDom;

namespace Dev2.Runtime.DynamicProxy
{
    internal class XsdDataContractImporter
    {
        private CodeCompileUnit codeCompileUnit;

        public XsdDataContractImporter(CodeCompileUnit codeCompileUnit)
        {
            this.codeCompileUnit = codeCompileUnit;
        }

        public ImportOptions Options { get; internal set; }
    }
}