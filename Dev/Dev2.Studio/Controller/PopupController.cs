using System;
using System.ComponentModel.Composition;
using System.Windows;
using Dev2.Studio.Core.Controller;
using Dev2.Studio.ViewModels.Dialogs;

// ReSharper disable CheckNamespace
namespace Dev2.Studio.Controller
{

    [Export(typeof(IPopupController))]
    public class PopupController : IPopupController
    {
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

        public string Header { get; set; }

        public string Description { get; set; }

        public string Question { get; set; }

        public MessageBoxImage ImageType { get; set; }

        public MessageBoxButton Buttons { get; set; }

        public string DontShowAgainKey { get; set; }

        public MessageBoxResult Show()
        {
            return ShowDev2MessageBox(Description, Header, Buttons, ImageType, DontShowAgainKey);
        }

        public MessageBoxResult Show(string description, string header = "", MessageBoxButton buttons = MessageBoxButton.OK, MessageBoxImage image = MessageBoxImage.Asterisk, string dontShowAgainKey = null)
        {
            Buttons = buttons;
            Description = description;
            Header = header;
            ImageType = image;
            DontShowAgainKey = dontShowAgainKey;
            return Show();
        }

        public Func<string, string, MessageBoxButton, MessageBoxImage, string, MessageBoxResult> ShowDev2MessageBox = (description, header, buttons, imageType, dontShowAgainKey) => Dev2MessageBoxViewModel.Show(description, header, buttons, imageType, dontShowAgainKey);

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
            Description = "The following task has been renamed " + oldName + " -> " + newName + ". You will lose the history for the old task." + Environment.NewLine +
                          " Would you like to save the new name?" + Environment.NewLine +
                          "-------------------------------------------------------------------" +
                          "Yes - Save with the new name." + Environment.NewLine +
                          "No - Save with the old name." + Environment.NewLine +
                          "Cancel - Returns you to Scheduler.";
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
