
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Windows;
using Dev2.Common;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Studio.ViewModels.Dialogs;

// ReSharper disable CheckNamespace
namespace Dev2.Studio.Controller
{
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

            Header = "Settings have changed";
            var description = "Settings have not been saved." + Environment.NewLine
                              + "Would you like to save the settings? " + Environment.NewLine +
                              "-------------------------------------------------------------------" +
                              "Yes - Save the settings." + Environment.NewLine +
                              "No - Discard your changes." + Environment.NewLine +
                              "Cancel - Returns you to settings.";
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

        public MessageBoxResult ShowNoInputsSelectedWhenClickLink()
        {
            Header = "Did you know?";
            var description = "You can pass variables into your workflow" + Environment.NewLine
                              + "by selecting the Input checkbox" + Environment.NewLine +
                              "in the Variables window.";
            Description = description;
            Buttons = MessageBoxButton.OK;
            ImageType = MessageBoxImage.Information;
            DontShowAgainKey = GlobalConstants.Dev2MessageBoxNoInputsWhenHyperlinkClickedDialog;
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

        public MessageBoxResult ShowConnectionTimeoutConfirmation(string serverName)
        {
            Header = "Server is unreachable";
            var description = " Unable to reach " + serverName + ": Connection timed out." + Environment.NewLine
                              + " Make sure the remote computer is powered on." + Environment.NewLine
                              + Environment.NewLine
                              + " Would you like to re-try? " + Environment.NewLine;
            Description = description;
            Buttons = MessageBoxButton.YesNo;
            ImageType = MessageBoxImage.Information;
            return Show();
        }

        public void ShowInvalidCharacterMessage(string invalidText)
        {
            Description = string.Format("{0} is invalid. Warewolf only supports latin characters", invalidText);
            Header = "Invalid text";
            Buttons = MessageBoxButton.OK;
            ImageType = MessageBoxImage.Error;
            Show();
        }

        public MessageBoxResult ShowDeleteVersionMessage(string displayName)
        {
            Header = "Delete version";
            var description = string.Format("Are you sure to delete {0}?", displayName);
            Description = description;
            Buttons = MessageBoxButton.YesNo;
            ImageType = MessageBoxImage.Information;
            return Show();
        }

        public MessageBoxResult ShowRollbackVersionMessage(string displayName)
        {
            Header = "Make current version";
            var description = string.Format("{0} will become the current version.{1}Do you want to proceed ?", displayName, Environment.NewLine);
            Description = description;
            Buttons = MessageBoxButton.YesNo;
            ImageType = MessageBoxImage.Information;
            return Show();
        }
    }
}
