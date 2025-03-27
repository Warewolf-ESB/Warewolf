using System.Windows;

namespace Dev2.Common.Interfaces.PopupController
{
    public interface IPopupMessage
    {
        string Description { get; set; }
        string Header { get; set; }
#if WINDOWS || NETFRAMEWORK
        MessageBoxButton Buttons { get; set; }
        MessageBoxImage Image { get; set; }
#endif
        string DontShowAgainKey { get; set; }
#if WINDOWS || NETFRAMEWORK
        MessageBoxResult DefaultResult { get; set; }
#endif
        bool IsDependenciesButtonVisible { get; set; }
        bool IsError { get; set; }
        bool IsInfo { get; set; }
        bool IsQuestion { get; set; }
    }
}
