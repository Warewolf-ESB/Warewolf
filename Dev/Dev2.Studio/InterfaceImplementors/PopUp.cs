
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Studio.ViewModels.Dialogs;
using System.ComponentModel.Composition;
using System.Windows;

namespace Dev2.Studio.Core.ViewModels {
    [Export(typeof(IPopUp))]
    public class PopUp : IPopUp {
        private string _header;
        private string _discripton;
        private string _question;
        MessageBoxImage _imageType;
        MessageBoxButton _buttons;
        string _dontShowAgainKey;

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
