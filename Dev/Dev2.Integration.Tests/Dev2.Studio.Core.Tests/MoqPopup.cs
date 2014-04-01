using Dev2.Studio.Core.Controller;
using System.ComponentModel.Composition;
using System.Windows;

namespace Dev2.Core.Tests.ProperMoqs
{
    [Export(typeof(IPopupController))]
    public class MoqPopup : IPopupController
    {
        private string _header;
        private string _discripton;
        private string _question;
        MessageBoxImage _imageType;
        MessageBoxButton _buttons;
        MessageBoxResult _result;
        string _dontShowAgainKey;

        public MoqPopup(string headerText, string discriptionText, MessageBoxImage imageType, MessageBoxButton buttons)
        {
            Header = headerText;
            Description = discriptionText;
            ImageType = imageType;
            Buttons = buttons;
        }

        public MoqPopup()
            : this(MessageBoxResult.OK)
        {

        }

        public MoqPopup(MessageBoxResult result)
        {
            _result = result;
        }

        public string Header
        {
            get
            {
                return _header;
            }
            set
            {
                _header = value;
            }
        }

        public string Description
        {
            get
            {
                return _discripton;
            }
            set
            {
                _discripton = value;
            }
        }

        public string Question
        {
            get
            {
                return _question;
            }
            set
            {
                _question = value;
            }
        }

        public MessageBoxImage ImageType
        {
            get
            {
                return _imageType;
            }
            set
            {
                _imageType = value;
            }
        }

        public MessageBoxButton Buttons
        {
            get
            {
                return _buttons;
            }
            set
            {
                _buttons = value;
            }
        }
        public MessageBoxResult Show()
        {
            return _result;
        }

        public MessageBoxResult ShowNotConnected()
        {
            return _result;
        }

        public MessageBoxResult ShowDeleteConfirmation(string nameOfItemBeingDeleted)
        {
            return _result;
        }

        public MessageBoxResult ShowNameChangedConflict(string oldName, string newName)
        {
            return _result;
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
    }
}
