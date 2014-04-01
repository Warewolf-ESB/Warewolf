namespace Dev2.Scheduler.Interfaces
{
    public interface IDirectoryHelper
    {
        string[] GetFiles(string path);
        string CreateIfNotExists(string debugOutputPath);
    }
}