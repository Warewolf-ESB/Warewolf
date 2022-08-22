using System.CodeDom.Compiler;

namespace Dev2.Runtime.DynamicProxy
{
    internal class ImportOptions
    {
        public ImportOptions()
        {
        }

        public bool ImportXmlType { get; internal set; }
        public CodeDomProvider CodeProvider { get; internal set; }
    }
}