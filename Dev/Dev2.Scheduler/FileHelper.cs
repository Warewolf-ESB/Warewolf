using System.Diagnostics.CodeAnalysis;
using System.IO;
using Dev2.Scheduler.Interfaces;

namespace Dev2.Scheduler
{
     // not required for code coverage this is simply a pass through required for unit testing
    public class FileHelper : IFileHelper
    {
        public string ReadAllText(string fileName)
        {
            return File.ReadAllText(fileName);
        }
    }
}