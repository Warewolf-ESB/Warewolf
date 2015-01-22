using Dev2.Common.Interfaces.PopupController;

namespace Warewolf.Studio.Core.Popup
{
    public class PopupMessageBoxFactory : IPopupMessageBoxFactory 
    {
        #region Implementation of IPopupMessageBoxFactory

        public IDev2MessageBoxViewModel Create(IPopupMessage message,IPopupWindow popupWindow)
        {
            return new WarewolfPopupMessage(message, popupWindow);
        }

        #endregion
    }
}