using System.IO;

namespace Dev2.Common.Wrappers.Interfaces
{
    public interface IFile
    {
        string ReadAllText(string fileName);
        void Move(string source, string destination);
        Stream Open(string fileName, FileMode fileMode);
        bool Exists(string path);
        void Delete(string tmpFileName);
        void WriteAllText(string p1, string p2);
        void Copy(string source, string destination);
    }
}