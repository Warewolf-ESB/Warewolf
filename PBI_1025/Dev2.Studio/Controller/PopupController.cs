using Dev2.Studio.Core.Controller;
using Dev2.Studio.Core.ViewModels;
using Dev2.Studio.ViewModels.Dialogs;
using System.ComponentModel.Composition;
using System.Windows;

namespace Dev2.Studio.Controller {
    [Export(typeof(IPopupController))]
    public class PopupController : IPopupController {
        private string _header;
        private string _discripton;
        private string _question;
        MessageBoxImage _imageType;
        MessageBoxButton _buttons;
        string _dontShowAgainKey;

        public PopupController(string headerText, string discriptionText, MessageBoxImage imageType, MessageBoxButton buttons) 
        {
            Header = headerText;
            Description = discriptionText;
            ImageType = imageType;
            Buttons = buttons;
        }

        public PopupController() {

        }

        public string Header {
            get {
                return _header;
            }
            set {
                _header = value;
            }
        }

        public string Description {
            get {
                return _discripton;
            }
            set {
                _discripton = value;
            }
        }

        public string Question {
            get {
                return _question;
            }
            set {
                _question = value;
            }
        }

        public MessageBoxImage ImageType {
            get {
                return _imageType;
            }
            set {
                _imageType = value;
            }
        }

        public MessageBoxButton Buttons {
            get {
                return _buttons;
            }
            set {
                _buttons = value;
            }
        }

        public string DontShowAgainKey
        {
            get
            {
                return _dontShowAgainKey;
            }
            set
            {
                _dontShowAgainKey = value;
            }
        }

        public MessageBoxResult Show() {
            //return MessageBox.Show(Description, Header, Buttons, ImageType);
            return Dev2MessageBoxViewModel.Show(Description, Header, Buttons, ImageType, DontShowAgainKey);
        }
    }
}
