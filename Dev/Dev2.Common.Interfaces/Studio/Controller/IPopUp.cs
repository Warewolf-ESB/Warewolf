// ReSharper disable CheckNamespace

using System.Windows;

namespace Dev2.Common.Interfaces.Studio.Controller
// ReSharper restore CheckNamespace
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
        MessageBoxResult Show(string description, string header, MessageBoxButton buttons, MessageBoxImage image, string dontShowAgainKey);
        MessageBoxResult ShowNotConnected();
        MessageBoxResult ShowDeleteConfirmation(string nameOfItemBeingDeleted);
        MessageBoxResult ShowNameChangedConflict(string oldName, string newName);
        MessageBoxResult ShowSettingsCloseConfirmation();
        MessageBoxResult ShowSchedulerCloseConfirmation();
        MessageBoxResult ShowNoInputsSelectedWhenClickLink();
        MessageBoxResult ShowSaveErrorDialog(string errorMessage);
        MessageBoxResult ShowConnectionTimeoutConfirmation(string serverName);
        MessageBoxResult ShowDeleteVersionMessage(string displayName);
        MessageBoxResult ShowRollbackVersionMessage(string displayName);

        void ShowInvalidCharacterMessage(string invalidText);
    }
}
