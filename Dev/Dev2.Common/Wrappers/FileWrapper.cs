using System.Diagnostics.CodeAnalysis;
using System.IO;
using Dev2.Common.Wrappers.Interfaces;

namespace Dev2.Common.Wrappers
{
     [ExcludeFromCodeCoverage]// not required for code coverage this is simply a pass through required for unit testing
    public class FileWrapper : IFile
    {
        public string ReadAllText(string fileName)
        {
            return File.ReadAllText(fileName);
        }
    }
}