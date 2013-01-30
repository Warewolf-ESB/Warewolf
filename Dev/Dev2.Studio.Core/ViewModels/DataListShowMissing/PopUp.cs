using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.ComponentModel.Composition;

namespace Dev2.Studio.Core.ViewModels {
    [Export(typeof(IPopUp))]
    public class PopUp : IPopUp {
        private string _header;
        private string _discripton;
        private string _question;
        MessageBoxImage _imageType;
        MessageBoxButton _buttons;

        public PopUp(string headerText, string discriptionText, MessageBoxImage imageType, MessageBoxButton buttons) {
            Header = headerText;
            Description = discriptionText;
            ImageType = imageType;
            Buttons = buttons;
        }

        public PopUp() {

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
        public MessageBoxResult Show() {
            return MessageBox.Show(Description, Header, Buttons, ImageType);
        }
    }
}
