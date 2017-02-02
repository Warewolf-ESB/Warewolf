using System.Windows;

namespace Dev2.Common.Interfaces.PopupController
{
    public interface IPopupMessage
    {
        string Description { get; set; }
        string Header { get; set; }
        MessageBoxButton Buttons { get; set; }
        MessageBoxImage Image { get; set; }
        string DontShowAgainKey { get; set; }
        MessageBoxResult DefaultResult { get; set; }
        bool IsDependenciesButtonVisible { get; set; }
        bool IsError { get; set; }
        bool IsInfo { get; set; }
        bool IsQuestion { get; set; }
    }
}
