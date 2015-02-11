using System.Windows;
using Dev2.Common.Interfaces.Studio.ViewModels.Dialogues;

namespace Dev2.Common.Interfaces.PopupController
{
    public interface IPopupMessageBoxFactory
    {
        IPopupWindow View { get; set; }
        IDev2MessageBoxViewModel Create(IPopupMessage message);
    }

    public interface IPopupWindow
    {
        MessageBoxResult Show(IPopupMessage message);
    }    

    public enum DialogResult
    {
        Failure,
        Success
    }
}