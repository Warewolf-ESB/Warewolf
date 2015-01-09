using System;
using System.Windows;
using Dev2.Common;
using Dev2.Common.Interfaces.PopupController;

namespace Warewolf.Studio.Core.Popup
{
    public class PopupMessages : IPopupMessages
    {
        #region Implementation of IPopupMessages

        public IPopupMessage GetNotConnected()
        {
            return new PopupMessage
            {
                Buttons = MessageBoxButton.OK,
                Header = "Server is not connected",
                Description = "You can not change the settings for a server that is offline.",
                Image = MessageBoxImage.Error
            };
        }

        public IPopupMessage GetDeleteConfirmation(string nameOfItemBeingDeleted)
        {
            return new PopupMessage
            {
                Buttons=MessageBoxButton.YesNo,
                Header = "Are you sure?",
                Description =  string.Format("Are you sure you want to delete {0}?", nameOfItemBeingDeleted),
                Image = MessageBoxImage.Warning
            };
        }

        public IPopupMessage GetNameChangedConflict(string oldName, string newName)
        {
            return new PopupMessage
            {
                Buttons = MessageBoxButton.YesNoCancel,
                Header = "Rename conflict",
                Description = string.Format("The following task has been renamed {0} -> {1}. You will lose the history for the old task.{2} Would you like to save the new name?{2}-------------------------------------------------------------------" + "Yes - Save with the new name.{2}No - Save with the old name.{2}Cancel - Returns you to Scheduler.", oldName, newName, Environment.NewLine),
                Image = MessageBoxImage.Information
            };
        }

        public IPopupMessage GetSettingsCloseConfirmation()
        {
            return new PopupMessage
            {
                Buttons = MessageBoxButton.YesNoCancel,
                Header = "Settings have changed",
                Description = string.Format("Settings have not been saved.{0}Would you like to save the settings? {0}-------------------------------------------------------------------" + "Yes - Save the settings.{0}No - Discard your changes.{0}Cancel - Returns you to settings.", Environment.NewLine),
                Image = MessageBoxImage.Information
            };
        }

        public IPopupMessage GetSchedulerCloseConfirmation()
        {
             return new PopupMessage
            {
                Buttons = MessageBoxButton.YesNoCancel,
                Header = "Scheduler Task has changes",
                Description = string.Format("Scheduler Task has not been saved.{0}Would you like to save the Task? {0}-------------------------------------------------------------------" + "Yes - Save the Task.{0}No - Discard your changes.{0}Cancel - Returns you to Scheduler.", Environment.NewLine),
                Image = MessageBoxImage.Information
            };
        
        }

        public IPopupMessage GetNoInputsSelectedWhenClickLink()
        {
            return new PopupMessage
            {
                Buttons = MessageBoxButton.OK,
                Header = "Did you know?",
                Description = string.Format("You can pass variables into your workflow{0}by selecting the Input checkbox{0}in the Variables window.", Environment.NewLine),
                Image = MessageBoxImage.Information,
                DontShowAgainKey = GlobalConstants.Dev2MessageBoxNoInputsWhenHyperlinkClickedDialog
            };
        }

        public IPopupMessage GetSaveErrorDialog(string errorMessage)
        {
            return new PopupMessage
            {
                Buttons = MessageBoxButton.OK,
                Header = "Saving Error",
                Description = "The following error occurred on save:" + Environment.NewLine
                              + errorMessage,
                Image = MessageBoxImage.Error,

            };
        }

        public IPopupMessage GetConnectionTimeoutConfirmation(string serverName)
        {
            return new PopupMessage
            {
                Buttons = MessageBoxButton.YesNo,
                Header = "Server is unreachable",
                Description = string.Format(" Unable to reach {0}: Connection timed out.{1} Make sure the remote computer is powered on.{1}{1} Would you like to re-try? {1}", serverName, Environment.NewLine),
                Image = MessageBoxImage.Error,

            };
        }

        public IPopupMessage GetDeleteVersionMessage(string displayName)
        {
            return new PopupMessage
            {
                Buttons = MessageBoxButton.YesNo,
                Header = "Delete Version",
                Description = string.Format("Are you sure you want to delete {0}?", displayName),
                Image = MessageBoxImage.Warning,

            };
        }

        public IPopupMessage GetRollbackVersionMessage(string displayName)
        {
            return new PopupMessage
            {
                Buttons = MessageBoxButton.YesNo,
                Header = "Make current version",
                Description = string.Format("{0} will become the current version.{1}Do you want to proceed ?", displayName, Environment.NewLine),
                Image = MessageBoxImage.Warning,

            };
        }

        public IPopupMessage GetInvalidCharacterMessage(string invalidText)
        {
            return new PopupMessage
            {
                Buttons = MessageBoxButton.OK,
                Header = "Invalid text",
                Description = string.Format("{0} is invalid. Warewolf only supports latin characters", invalidText),
                Image = MessageBoxImage.Warning,

            };
        }

        #endregion
    }
}
