using System.Windows;
using Dev2.Common.Interfaces.Studio.Controller;

namespace Dev2.Core.Tests.ProperMoqs
{
    public class MoqPopup : IPopupController
    {
        readonly MessageBoxResult _result;

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

        public string Header { get; set; }

        public string Description { get; set; }

        public string Question { get; set; }

        public MessageBoxImage ImageType { get; set; }

        public MessageBoxButton Buttons { get; set; }

        public MessageBoxResult Show()
        {
            ShowHitCount++;
            return _result;
        }
        // public MessageBoxResult Show(string description, string header = "", MessageBoxButton buttons = MessageBoxButton.OK, MessageBoxImage image = MessageBoxImage.Asterisk, string dontShowAgainKey = null)
        public MessageBoxResult Show(string description, string header, MessageBoxButton buttons, MessageBoxImage image, string dontShowAgainKey)
        {
            Buttons = buttons;
            Description = description;
            Header = header;
            ImageType = image;
            DontShowAgainKey = dontShowAgainKey;
            return Show();
        }

        public int ShowHitCount { get; private set; }

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

        public MessageBoxResult ShowSettingsCloseConfirmation()
        {
            return _result;
        }

        public MessageBoxResult ShowSchedulerCloseConfirmation()
        {
            return _result;
        }

        public MessageBoxResult ShowNoInputsSelectedWhenClickLink()
        {
            return _result;
        }

        public MessageBoxResult ShowSaveErrorDialog(string errorMessage)
        {
            return _result;
        }

        public MessageBoxResult ShowConnectionTimeoutConfirmation(string serverName)
        {
            return _result;
        }

        public void ShowInvalidCharacterMessage(string invalidText)
        {
        }

        public MessageBoxResult ShowDeleteVersionMessage(string displayName)
        {
            return MessageBoxResult.None;
        }

        public MessageBoxResult ShowRollbackVersionMessage(string displayName)
        {
            return MessageBoxResult.None;
        }

        public string DontShowAgainKey { get; set; }
    }
}
