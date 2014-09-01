namespace Dev2.Common.Interfaces.Scheduler.Interfaces
{
    public interface IDirectoryHelper
    {
        string[] GetFiles(string path);
        string CreateIfNotExists(string debugOutputPath);
    }
}