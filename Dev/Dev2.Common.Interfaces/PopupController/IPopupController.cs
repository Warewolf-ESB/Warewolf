using System.Windows;

namespace Dev2.Common.Interfaces.PopupController
{
    public interface IPopupController
    {
        MessageBoxResult Show(IPopupMessage message);
    }
}