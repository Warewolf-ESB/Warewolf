using System.Windows;
using Dev2;
using Dev2.Common.Interfaces.PopupController;

namespace Warewolf.Studio.Core.Popup
{
    public class PopupController:IPopupController
    {
        #region Implementation of IPopupController
        private readonly IPopupMessageBoxFactory _popupMessageBoxFactory;

        public PopupController(IPopupMessageBoxFactory popupMessageBoxFactory)
        {
            VerifyArgument.IsNotNull("popupMessageBoxFactory",popupMessageBoxFactory);
            _popupMessageBoxFactory = popupMessageBoxFactory;
        }

        public MessageBoxResult Show(IPopupMessage message)
        {
            var window = _popupMessageBoxFactory.Create(message);
            var result = window.Show();
            return result;
        }

        #endregion
    }
}
