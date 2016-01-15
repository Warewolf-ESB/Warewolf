using Dev2.Common.Interfaces.PopupController;

namespace Warewolf.Studio.Core.Popup
{
    public class PopupMessageBoxFactory : IPopupMessageBoxFactory 
    {
        #region Implementation of IPopupMessageBoxFactory

        public IPopupWindow View { get; set; }

        public IDev2MessageBoxViewModel Create(IPopupMessage message)
        {
            return new WarewolfPopupMessage(message, View);
        }

        #endregion
    }
}