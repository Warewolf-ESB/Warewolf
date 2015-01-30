using System.Windows;

namespace Dev2.Common.Interfaces.SaveDialog
{
    public interface ISaveDialog
    {
        MessageBoxResult ShowSaveDialog();
        ResourceName ResourceName { get; }
    }

    public class ResourceName
    {
        readonly string _name;
        readonly string _path;

        public ResourceName(string path, string name)
        {
            _path = path;
            _name = name;
        }

        public string Name
        {
            get
            {
                return _name;
            }

        }
        string Path
        {
            get
            {
                return _path;
            }
  
        }
    }
}