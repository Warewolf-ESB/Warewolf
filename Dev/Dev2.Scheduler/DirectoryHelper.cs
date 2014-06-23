using System.Diagnostics.CodeAnalysis;
using System.IO;
using Dev2.Scheduler.Interfaces;

namespace Dev2.Scheduler
{
     // not required for code coverage this is simply a pass through required for unit testing
    public class DirectoryHelper : IDirectoryHelper
    {
        public string[] GetFiles(string path)
        {
            return Directory.GetFiles(path);
        }

        public string CreateIfNotExists(string debugOutputPath)
        {
            if(!Directory.Exists(debugOutputPath))
            {
                return Directory.CreateDirectory(debugOutputPath).Name;
            }

            return debugOutputPath;
        }
    }
}