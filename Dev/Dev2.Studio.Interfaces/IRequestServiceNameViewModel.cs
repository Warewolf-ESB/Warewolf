using System;
using System.Windows;
using System.Windows.Input;
using Dev2.Common.Interfaces.SaveDialog;

namespace Dev2.Studio.Interfaces
{
    public interface IRequestServiceNameViewModel : IDisposable
    {
        MessageBoxResult ShowSaveDialog();

        ResourceName ResourceName { get; }
        string Name { get; set; }
        string ErrorMessage { get; set; }
        ICommand OkCommand { get; set; }
        ICommand DuplicateCommand { get; set; }
        ICommand DoneCommand { get; }
        ICommand CancelCommand { get; }
        IExplorerViewModel SingleEnvironmentExplorerViewModel { get; }
        string Header { get; }
        bool IsDuplicate { get; set; }
        bool IsDuplicating { get; set; }
        bool FixReferences { get; }
    }
}