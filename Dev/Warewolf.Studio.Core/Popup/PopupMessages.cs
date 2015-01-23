using System;
using System.Windows;
using Dev2.Common;
using Dev2.Common.Interfaces.PopupController;

namespace Warewolf.Studio.Core.Popup
{
    public static class PopupMessages 
    {
        #region Implementation of IPopupMessages

        public static IPopupMessage GetNotConnected()
        {
            return new PopupMessage
            {
                Buttons = MessageBoxButton.OK,
                Header = Resources.Languages.Core.ServerNotConnected,
                Description = Resources.Languages.Core.ServerNotConnectedSettingsDescription,
                Image = MessageBoxImage.Error
            };
        }

        public static IPopupMessage GetDeleteConfirmation(string nameOfItemBeingDeleted)
        {
            return new PopupMessage
            {
                Buttons=MessageBoxButton.YesNo,
                Header = Resources.Languages.Core.GenericConfirmation,
                Description = string.Format(Resources.Languages.Core.DeleteConfirmation, nameOfItemBeingDeleted),
                Image = MessageBoxImage.Warning
            };
        }

        public static IPopupMessage GetNameChangedConflict(string oldName, string newName)
        {
            return new PopupMessage
            {
                Buttons = MessageBoxButton.YesNoCancel,
                Header = Resources.Languages.Core.RenameConflictHeader,
                Description = string.Format(Resources.Languages.Core.RenameConflictDescription, oldName, newName, Environment.NewLine),
                Image = MessageBoxImage.Information
            };
        }

        public static IPopupMessage GetSettingsCloseConfirmation()
        {
            return new PopupMessage
            {
                Buttons = MessageBoxButton.YesNoCancel,
                Header = Resources.Languages.Core.SettingsChangedHeader,
                Description =  string.Format(Resources.Languages.Core.SettingsChangedDescription, Environment.NewLine),
                Image = MessageBoxImage.Information
            };
        }

        public static IPopupMessage GetSchedulerCloseConfirmation()
        {
             return new PopupMessage
            {
                Buttons = MessageBoxButton.YesNoCancel,
                Header = Resources.Languages.Core.SchedulerChangesHeader,
                Description = string.Format(Resources.Languages.Core.SchedulerChangesHeader, Environment.NewLine),
                Image = MessageBoxImage.Information
            };
        
        }

        public static IPopupMessage GetNoInputsSelectedWhenClickLink()
        {
            return new PopupMessage
            {
                Buttons = MessageBoxButton.OK,
                Header = Resources.Languages.Core.DidYouKnow,
                Description = string.Format(Resources.Languages.Core.InputVariablesTip, Environment.NewLine),
                Image = MessageBoxImage.Information,
                DontShowAgainKey = GlobalConstants.Dev2MessageBoxNoInputsWhenHyperlinkClickedDialog
            };
        }

        public static IPopupMessage GetSaveErrorDialog(string errorMessage)
        {
            return new PopupMessage
            {
                Buttons = MessageBoxButton.OK,
                Header = Resources.Languages.Core.SaveErrorHeader,
                Description = string.Format("{0}{1}{2}", Resources.Languages.Core.SaveErrorHeader, Environment.NewLine, errorMessage),
                Image = MessageBoxImage.Error,

            };
        }

        public static IPopupMessage GetConnectionTimeoutConfirmation(string serverName)
        {
            return new PopupMessage
            {
                Buttons = MessageBoxButton.YesNo,
                Header = Resources.Languages.Core.ServerUnreachableErrorHeader,
                Description = string.Format(Resources.Languages.Core.ServerUnreachableHeaderMessage, serverName, Environment.NewLine),
                Image = MessageBoxImage.Error,

            };
        }

        public static IPopupMessage GetDeleteVersionMessage(string displayName)
        {
            return new PopupMessage
            {
                Buttons = MessageBoxButton.YesNo,
                Header = Resources.Languages.Core.DeleteVersionMessageHeader,
                Description = string.Format(Resources.Languages.Core.DeleteVersionMessage, displayName),
                Image = MessageBoxImage.Warning,

            };
        }

        public static IPopupMessage GetRollbackVersionMessage(string displayName)
        {
            return new PopupMessage
            {
                Buttons = MessageBoxButton.YesNo,
                Header = Resources.Languages.Core.RollbackHeader,
                Description = string.Format(Resources.Languages.Core.RollbackMessage, displayName, Environment.NewLine),
                Image = MessageBoxImage.Warning,

            };
        }

        public static IPopupMessage GetInvalidCharacterMessage(string invalidText)
        {
            return new PopupMessage
            {
                Buttons = MessageBoxButton.OK,
                Header = Resources.Languages.Core.InvalidTextHeader,
                Description = string.Format(Resources.Languages.Core.InvalidTextMessage, invalidText),
                Image = MessageBoxImage.Warning,

            };
        }

        #endregion

        public static IPopupMessage GetInvalidPermissionException()
        {
            return new PopupMessage
            {
                Buttons = MessageBoxButton.OK,
                Header = Resources.Languages.Core.InvalidPermissionHeader,
                Description = Resources.Languages.Core.InvalidPermissionMessage,
                Image = MessageBoxImage.Warning,

            };
        }
    }
}
