using System.Windows;
using Dev2.Common.Interfaces.PopupController;

namespace Warewolf.Studio.Core.Popup
{
    public class PopupMessage:IPopupMessage {
        #region Implementation of IPopupMessage

        public string Description { get; set; }
        public string Header { get; set; }
        public MessageBoxButton Buttons { get; set; }
        public MessageBoxImage Image { get; set; }
        public string DontShowAgainKey { get; set; }
        public MessageBoxResult DefaultResult { get; set; }

        #endregion
    }
}