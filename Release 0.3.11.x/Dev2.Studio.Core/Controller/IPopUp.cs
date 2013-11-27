using System.Windows;

namespace Dev2.Studio.Core.Controller{

    public interface IPopupController {
        string Header { get; set; }
        string Description { get; set; }
        string Question { get; set; }
        MessageBoxImage ImageType { get; set; }
        MessageBoxButton Buttons { get; set; }
        string DontShowAgainKey { get; set; }
        MessageBoxResult Show();
        MessageBoxResult ShowNotConnected();
    }
}
