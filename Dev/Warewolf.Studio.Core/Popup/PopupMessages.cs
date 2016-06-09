using System;
using System.Windows;
using Dev2.Common.Interfaces.PopupController;

namespace Warewolf.Studio.Core.Popup
{
    public static class PopupMessages 
    {
        #region Implementation of IPopupMessages

        public static IPopupMessage GetDeleteConfirmation(string nameOfItemBeingDeleted)
        {
            return new PopupMessage
            {
                Buttons=MessageBoxButton.YesNo,
                Header = Resources.Languages.Core.GenericConfirmation,
                Description = string.Format(Resources.Languages.Core.DeleteConfirmation, nameOfItemBeingDeleted),
                Image = MessageBoxImage.Warning, 
                IsInfo = true,
                IsError = false,
                IsQuestion = false
            };
        }

        #endregion

        public static IPopupMessage GetDuplicateMessage(string name)
        {
            return new PopupMessage
            {
                Buttons = MessageBoxButton.OK,
                Header = Resources.Languages.Core.InvalidPermissionHeader,
                Description = String.Format("The name {0} already exists. Please choose a different name.", name),

            };
        }
    }
}
