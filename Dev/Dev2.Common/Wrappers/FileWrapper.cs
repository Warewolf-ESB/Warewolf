using System.Diagnostics.CodeAnalysis;
using System.IO;
using Dev2.Common.Wrappers.Interfaces;

namespace Dev2.Common.Wrappers
{
     // not required for code coverage this is simply a pass through required for unit testing
    public class FileWrapper : IFile
    {
        public string ReadAllText(string fileName)
        {
            return File.ReadAllText(fileName);
        }

         public void Move(string source, string destination)
         {
              File.Move(source,destination);
         }

         public Stream Open(string fileName, FileMode fileMode)
         {
             return File.Open(fileName, fileMode);
         }

         public bool Exists(string path)
         {
             return File.Exists(path);
         }

         public void Delete(string tmpFileName)
         {
             File.Delete(tmpFileName);
         }
    }
}