namespace Dev2.Common.Interfaces.Wrappers
{
    public interface IDirectory
    {
        string[] GetFiles(string path);
        string CreateIfNotExists(string debugOutputPath);
        string[] GetLogicalDrives();
        bool Exists(string path);
        string[] GetFileSystemEntries(string path);
        string[] GetFileSystemEntries(string path, string searchPattern);
        string[] GetDirectories(string workspacePath);
        void Move(string directoryStructureFromPath, string directoryStructureToPath);
        void Delete(string directoryStructureFromPath, bool recursive);
    }
}