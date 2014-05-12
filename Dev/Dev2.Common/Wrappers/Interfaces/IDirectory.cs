namespace Dev2.Common.Wrappers.Interfaces
{
    public interface IDirectory
    {
        string[] GetFiles(string path);
        string CreateIfNotExists(string debugOutputPath);
        string[] GetLogicalDrives();
        bool Exists(string path);
        string[] GetFileSystemEntries(string path);
        string[] GetFileSystemEntries(string path, string searchPattern);
    }
}