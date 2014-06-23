using System.Diagnostics.CodeAnalysis;
using System.IO;
using Dev2.Common.Wrappers.Interfaces;

namespace Dev2.Common.Wrappers
{
     // not required for code coverage this is simply a pass through required for unit testing
    public class DirectoryWrapper : IDirectory
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

        public string[] GetLogicalDrives()
        {
            return Directory.GetLogicalDrives();
        }

        public bool Exists(string path)
        {
            return Directory.Exists(path);
        }

        public string[] GetFileSystemEntries(string path)
        {
            return Directory.GetFileSystemEntries(path);
        }

        public string[] GetFileSystemEntries(string path, string searchPattern)
        {
            return Directory.GetFileSystemEntries(path, searchPattern);
        }

        public string[] GetDirectories(string path)
        {
            return Directory.GetDirectories(path);
        }

        public void Move(string directoryStructureFromPath, string directoryStructureToPath)
        {
             Directory.Move(directoryStructureFromPath, directoryStructureToPath);
        }

        public void Delete(string directoryStructureFromPath, bool recursive)
        {
            Directory.Delete(directoryStructureFromPath,recursive);
        }
    }
}