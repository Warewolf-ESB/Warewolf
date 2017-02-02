using System;
using System.Windows;
using System.Windows.Input;

// ReSharper disable once CheckNamespace
namespace Dev2.Common.Interfaces.SaveDialog
{
    public interface IRequestServiceNameViewModel : IDisposable
    {
        MessageBoxResult ShowSaveDialog();

        ResourceName ResourceName { get; }
        string Name { get; set; }
        string ErrorMessage { get; set; }
        ICommand OkCommand { get; set; }
        ICommand DuplicateCommand { get; set; }
        ICommand CancelCommand { get; }
        IExplorerViewModel SingleEnvironmentExplorerViewModel { get; }
        string Header { get; }
        bool IsDuplicate { get; set; }
        bool IsDuplicating { get; set; }
        bool FixReferences { get; }
    }

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