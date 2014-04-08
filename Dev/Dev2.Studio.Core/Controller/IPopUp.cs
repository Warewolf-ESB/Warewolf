using System.Windows;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.Controller
{

    public interface IPopupController
    {
        string Header { get; set; }
        string Description { get; set; }
        string Question { get; set; }
        MessageBoxImage ImageType { get; set; }
        MessageBoxButton Buttons { get; set; }
        string DontShowAgainKey { get; set; }
        MessageBoxResult Show();
        MessageBoxResult ShowNotConnected();
        MessageBoxResult ShowDeleteConfirmation(string nameOfItemBeingDeleted);
        MessageBoxResult ShowNameChangedConflict(string oldName, string newName);
        MessageBoxResult ShowSettingsCloseConfirmation();
        MessageBoxResult ShowSchedulerCloseConfirmation();
        MessageBoxResult ShowSaveErrorDialog(string errorMessage);
    }
}
