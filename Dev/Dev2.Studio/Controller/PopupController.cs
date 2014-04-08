using System;
using Dev2.Studio.Core.Controller;
using Dev2.Studio.ViewModels.Dialogs;
using System.ComponentModel.Composition;
using System.Windows;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Controller
{

    [Export(typeof(IPopupController))]
    public class PopupController : IPopupController
    {
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

        public PopupController()
        {

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

        public MessageBoxResult Show()
        {
            return Dev2MessageBoxViewModel.Show(Description, Header, Buttons, ImageType, DontShowAgainKey);
        }

        public MessageBoxResult ShowNotConnected()
        {
            Buttons = MessageBoxButton.OK;
            Header = "Server is not connected";
            Description = "You can not change the settings for a server that is offline.";
            ImageType = MessageBoxImage.Error;
            return Show();
        }

        public MessageBoxResult ShowDeleteConfirmation(string nameOfItemBeingDeleted)
        {
            Buttons = MessageBoxButton.YesNo;
            Header = "Are you sure?";
            Description = "Are you sure you want to delete " + nameOfItemBeingDeleted + "?";
            ImageType = MessageBoxImage.Information;
            return Show();
        }

        public MessageBoxResult ShowNameChangedConflict(string oldName, string newName)
        {
            Buttons = MessageBoxButton.YesNoCancel;
            Header = "Rename conflict";
            Description = "The following task has been renamed " + oldName + " -> " + newName + ". You will lose the history for the old task. Would you like to save the new name?";
            ImageType = MessageBoxImage.Information;
            return Show();
        }

        public MessageBoxResult ShowSettingsCloseConfirmation()
        {

            Header = "Security Settings have changed";
            var description = "Security settings have not been saved." + Environment.NewLine
                              + "Would you like to save the settings? " + Environment.NewLine +
                              "-------------------------------------------------------------------" +
                              "Yes - Save the security settings." + Environment.NewLine +
                              "No - Discard your changes." + Environment.NewLine +
                              "Cancel - Returns you to security settings.";
            Description = description;
            Buttons = MessageBoxButton.YesNoCancel;
            ImageType = MessageBoxImage.Information;
            return Show();
        }

        public MessageBoxResult ShowSchedulerCloseConfirmation()
        {
            Header = "Scheduler Task has changes";
            var description = "Scheduler Task has not been saved." + Environment.NewLine
                              + "Would you like to save the Task? " + Environment.NewLine +
                              "-------------------------------------------------------------------" +
                              "Yes - Save the Task." + Environment.NewLine +
                              "No - Discard your changes." + Environment.NewLine +
                              "Cancel - Returns you to Scheduler.";
            Description = description;
            Buttons = MessageBoxButton.YesNoCancel;
            ImageType = MessageBoxImage.Information;
            return Show();
        }

        public MessageBoxResult ShowSaveErrorDialog(string errorMessage)
        {
            Header = "Saving Error";
            var description = "The following error occurred on save:" + Environment.NewLine
                              + errorMessage;
            Description = description;
            Buttons = MessageBoxButton.OK;
            ImageType = MessageBoxImage.Error;
            return Show();
        }
    }
}
