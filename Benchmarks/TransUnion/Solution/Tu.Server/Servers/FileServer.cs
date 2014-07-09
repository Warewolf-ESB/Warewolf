using System;
using System.IO;

namespace Tu.Servers
{
    public class FileServer : IFileServer
    {
        public FileServer()
            : this(AppDomain.CurrentDomain.BaseDirectory)  // use the current execution folder
        {
        }

        public FileServer(string path)
        {
            if(path == null)
            {
                throw new ArgumentNullException("path");
            }
            Path = path;
        }

        public string Path { get; private set; }

        public void WriteAllText(string fileName, string contents)
        {
            var path = GetFullPath(fileName);
            var dirPath = System.IO.Path.GetDirectoryName(path);
            if(!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
            File.WriteAllText(path, contents);
        }

        public string ReadAllText(string fileName)
        {
            var path = GetFullPath(fileName);

            return File.Exists(path) ? File.ReadAllText(path) : string.Empty;
        }

        public void Delete(string fileName)
        {
            var path = GetFullPath(fileName);

            if(File.Exists(path))
            {
                File.Delete(path);
            }
        }

        string GetFullPath(string fileName)
        {
            return System.IO.Path.Combine(Path, fileName);
        }
    }
}