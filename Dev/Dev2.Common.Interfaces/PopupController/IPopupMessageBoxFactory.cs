using System.Windows;

namespace Dev2.Common.Interfaces.PopupController
{
    public interface IPopupMessageBoxFactory
    {
        IDev2MessageBoxViewModel Create(IPopupMessage message,IPopupWindow popupWindow);
    }

    public interface IPopupWindow
    {
        MessageBoxResult Show(IPopupMessage message);
    }
}