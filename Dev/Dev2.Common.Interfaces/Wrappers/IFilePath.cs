namespace Dev2.Common.Interfaces.Wrappers
{
    public interface IFilePath
    {
        string Combine(params string[] paths);
        string GetFileName(string path);        
    }
}