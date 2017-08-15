
namespace Dev2.Common.Interfaces.SaveDialog
{
    public class ResourceName
    {
        private readonly string _name;
        private readonly string _path;

        public ResourceName(string path, string name)
        {
            _path = path;
            _name = name;
        }

        public string Name => _name;

        public string Path => _path;
    }
}