using Dev2.Common.Interfaces.Wrappers;

namespace Dev2.Common.Wrappers
{
    public class PathWrapper : IFilePath
    {
        public string Combine(params string[] paths)
        {
            return System.IO.Path.Combine(paths);
        }

        public string GetFileName(string path)
        {
            return System.IO.Path.GetFileName(path);
        }
    }
}
