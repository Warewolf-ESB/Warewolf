namespace Dev2.Common.Interfaces
{
    public interface ILocalPathManager
    {
        string GetDirectoryName();
        string GetFullFileName();
        bool FileExist();
    }
}