using System.Windows;
using System.Windows.Input;

namespace Dev2.Common.Interfaces.SaveDialog
{
    public interface IRequestServiceNameViewModel
    {
        MessageBoxResult ShowSaveDialog();
        ResourceName ResourceName { get; }
        string Name { get; set; }
        string ErrorMessage { get; set; }
        ICommand OkCommand { get; set; }
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