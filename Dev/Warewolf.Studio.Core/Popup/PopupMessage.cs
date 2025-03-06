using System.Windows;
using Dev2.Common.Interfaces.PopupController;

namespace Warewolf.Studio.Core.Popup
{
    public class PopupMessage:IPopupMessage {
        #region Implementation of IPopupMessage

        public string Description { get; set; }
        public string Header { get; set; }
#if WINDOWS
        public MessageBoxButton Buttons { get; set; }
        public MessageBoxImage Image { get; set; }
#endif
        public string DontShowAgainKey { get; set; }
#if WINDOWS
        public MessageBoxResult DefaultResult { get; set; }
#endif
        public bool IsDependenciesButtonVisible { get; set; }
        public bool IsError { get; set; }
        public bool IsInfo { get; set; }
        public bool IsQuestion { get; set; }

#endregion
    }
}