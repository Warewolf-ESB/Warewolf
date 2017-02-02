/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic; 
using System.Windows;
using Dev2.Common;
using Dev2.Common.Interfaces.PopupController;
using Dev2.Studio.ViewModels.Dialogs;
using Warewolf.Studio.Core.Popup;
using Warewolf.Studio.ViewModels;

// ReSharper disable CheckNamespace
namespace Dev2.Studio.Controller
{
    public class PopupController : Common.Interfaces.Studio.Controller.IPopupController
    {
        public string Header { get; set; }

        public string Description { get; set; }

        public string Question { get; set; }

        public MessageBoxImage ImageType { get; set; }

        public MessageBoxButton Buttons { get; set; }

        public string DontShowAgainKey { get; set; }

        public bool IsError { get; private set; }
        public bool IsInfo { get; private set; }
        public bool IsQuestion { get; private set; }
        public List<string> UrlsFound { get; private set; }
        public bool IsDependenciesButtonVisible { get; private set; }
        public bool IsDeleteAnywayButtonVisible { get; private set; }
        public bool DeleteAnyway { get; private set; }
        public bool ApplyToAll { get; private set; }

        public MessageBoxResult Show(IPopupMessage popupMessage)
        {
            return Show(popupMessage.Description, popupMessage.Header, popupMessage.Buttons, popupMessage.Image, popupMessage.DontShowAgainKey, popupMessage.IsDependenciesButtonVisible, popupMessage.IsError, popupMessage.IsInfo, popupMessage.IsQuestion);
        }

        public MessageBoxResult Show()
        {
            var dev2MessageBoxViewModel = ShowDev2MessageBox(Description, Header, Buttons, ImageType, DontShowAgainKey, IsDependenciesButtonVisible, IsError, IsInfo, IsQuestion, UrlsFound, IsDeleteAnywayButtonVisible, ApplyToAll);
            DeleteAnyway = dev2MessageBoxViewModel.IsDeleteAnywaySelected;
            ApplyToAll = dev2MessageBoxViewModel.ApplyToAll;
            return dev2MessageBoxViewModel.Result;
        }

        public MessageBoxResult Show(string description, string header = "", MessageBoxButton buttons = MessageBoxButton.OK, MessageBoxImage image = MessageBoxImage.Asterisk, string dontShowAgainKey = null, bool isDependenciesButtonVisible = false, bool isError = false, bool isInfo = false, bool isQuestion = false, bool isDeleteAnywayButtonVisible = false, bool applyToAll = false)
        {
            Buttons = buttons;
            Description = description;
            Header = header;
            ImageType = image;
            DontShowAgainKey = dontShowAgainKey;
            IsDependenciesButtonVisible = isDependenciesButtonVisible;
            IsError = isError;
            IsInfo = isInfo;
            IsQuestion = isQuestion;
            IsDeleteAnywayButtonVisible = isDeleteAnywayButtonVisible;
            ApplyToAll = applyToAll;
            return Show();
        }

        public Func<string, string, MessageBoxButton, MessageBoxImage, string, bool, bool, bool, bool, List<string>, bool, bool, MessageBoxViewModel> ShowDev2MessageBox = (description, header, buttons, imageType, dontShowAgainKey, isDependenciesButtonVisible, isError, isInfo, isQuestion, urlsFound, isDeleteAnywayButtonVisible, applyToAll) => Dev2MessageBoxViewModel.Show(description, header, buttons, imageType, dontShowAgainKey, isDependenciesButtonVisible, isError, isInfo, isQuestion, urlsFound, isDeleteAnywayButtonVisible, applyToAll);

        public MessageBoxResult ShowNotConnected()
        {
            Buttons = MessageBoxButton.OK;
            Header = "Server Unreachable";
            Description = "You can not change the settings for a server that is unreachable.";
            ImageType = MessageBoxImage.Error;
            IsDependenciesButtonVisible = false;
            IsInfo = false;
            IsError = true;
            IsQuestion = false;
            IsDeleteAnywayButtonVisible = false;
            ApplyToAll = false;
            return Show();
        }

        public MessageBoxResult ShowRollbackVersionMessage(string displayName)
        {
            Header = Warewolf.Studio.Resources.Languages.Core.RollbackHeader;
            var description = String.Format(Warewolf.Studio.Resources.Languages.Core.RollbackMessage, displayName, Environment.NewLine);
            Description = description;
            Buttons = MessageBoxButton.YesNo;
            ImageType = MessageBoxImage.Warning;
            IsDependenciesButtonVisible = false;
            IsInfo = true;
            IsError = false;
            IsQuestion = false;
            IsDeleteAnywayButtonVisible = false;
            ApplyToAll = false;
            return Show();
        }


        public MessageBoxResult ShowNoInputsSelectedWhenClickLink()
        {
            Header = Warewolf.Studio.Resources.Languages.Core.VariablesInput_Information_Title;
            var description = string.Format(Warewolf.Studio.Resources.Languages.Core.VariablesInput_Information, Environment.NewLine);
            Description = description;
            Buttons = MessageBoxButton.OK;
            ImageType = MessageBoxImage.Information;
            DontShowAgainKey = GlobalConstants.Dev2MessageBoxNoInputsWhenHyperlinkClickedDialog;
            IsDependenciesButtonVisible = false;
            IsInfo = true;
            IsError = false;
            IsQuestion = false;
            IsDeleteAnywayButtonVisible = false;
            ApplyToAll = false;
            return Show();
        }

        public MessageBoxResult ShowResourcesConflict(List<string> duplicateResource)
        {
            Buttons = MessageBoxButton.OK;
            Header = "Duplicated Resources";
            UrlsFound = duplicateResource;
            Description = "Duplicate resources found. Please resolve the files on File Explorer. \nTo view the resource, click on the individual items below.";
            ImageType = MessageBoxImage.Error;
            IsDependenciesButtonVisible = false;
            IsInfo = false;
            IsError = true;
            IsQuestion = false;
            IsDeleteAnywayButtonVisible = false;
            ApplyToAll = false;
            return Show();
        }


        public MessageBoxResult ShowServerNotConnected(string server)
        {
            Buttons = MessageBoxButton.OK;
            Header = "Server Unreachable";
            Description = "The server " + server + " is unreachable. \n \nPlease make sure the Warewolf Server service is running on that machine.";
            ImageType = MessageBoxImage.Error;
            IsDependenciesButtonVisible = false;
            IsInfo = false;
            IsError = true;
            IsQuestion = false;
            IsDeleteAnywayButtonVisible = false;
            ApplyToAll = false;
            return Show();
        }

        public MessageBoxResult ShowDeleteConfirmation(string nameOfItemBeingDeleted)
        {
            Buttons = MessageBoxButton.YesNo;
            Header = "Are you sure?";
            Description = "Are you sure you want to delete " + nameOfItemBeingDeleted + "?";
            ImageType = MessageBoxImage.Warning;
            IsDependenciesButtonVisible = false;
            IsInfo = true;
            IsError = false;
            IsQuestion = false;
            IsDeleteAnywayButtonVisible = false;
            ApplyToAll = false;
            return Show();
        }
        public MessageBoxResult ShowExceptionViewAppreciation()
        {
            Buttons = MessageBoxButton.OK;
            Header = "We’ve got your feedback!";
            Description = "Thank you for taking the time to log it. Follow the issue " + Environment.NewLine +
                "in the Community to keep updated on the progress.";
            ImageType = MessageBoxImage.Information;
            IsDependenciesButtonVisible = false;
            IsInfo = true;
            IsError = false;
            IsQuestion = false;
            IsDeleteAnywayButtonVisible = false;
            ApplyToAll = false;
            return Show();
        }
        public MessageBoxResult ShowCorruptTaskResult(string errorMessage)
        {
            Buttons = MessageBoxButton.OK;
            Header = "Scheduler Load Error";
            Description = "Unable to retrieve tasks." + Environment.NewLine +
                          "ERROR: " + errorMessage + ". " + Environment.NewLine +
                          "Please check that there a no corrupt files." + Environment.NewLine +
                         @"C:\Windows\System32\Tasks\Warewolf";
            ImageType = MessageBoxImage.Error;
            IsDependenciesButtonVisible = false;
            IsInfo = false;
            IsError = true;
            IsQuestion = false;
            IsDeleteAnywayButtonVisible = false;
            ApplyToAll = false;
            return Show();
        }

        public MessageBoxResult ShowNameChangedConflict(string oldName, string newName)
        {
            Buttons = MessageBoxButton.YesNoCancel;
            Header = "Rename Conflicts";
            Description = "The following task has been renamed " + oldName + " -> " + newName + ". You will lose the history for the old task." + Environment.NewLine +
                          " Would you like to save the new name?" + Environment.NewLine +
                          "-----------------------------------------------------------------" +
                              Environment.NewLine +
                          "Yes - Save with the new name." + Environment.NewLine +
                          "No - Save with the old name." + Environment.NewLine +
                          "Cancel - Returns you to Scheduler.";
            ImageType = MessageBoxImage.Information;
            IsDependenciesButtonVisible = false;
            IsInfo = true;
            IsError = false;
            IsQuestion = false;
            IsDeleteAnywayButtonVisible = false;
            ApplyToAll = false;
            return Show();
        }

        public MessageBoxResult ShowDeployConflict(int conflictCount)
        {
            string correctDesc = String.Empty;
            Buttons = MessageBoxButton.OKCancel;
            Header = "Deploy Conflicts";
            if (conflictCount == 1)
            {
                correctDesc = "There is [ " + conflictCount + " ] conflict that occurs";
            }
            if (conflictCount > 1)
            {
                correctDesc = "There are [ " + conflictCount + " ] conflicts that occur";
            }
            Description = correctDesc + " in this deploy." + Environment.NewLine + "Click OK to override the conflicts or Cancel to view the conflicting resources." + Environment.NewLine +
                          "--------------------------------------------------------------------------------" +
                              Environment.NewLine +
                          "OK - Continue to deploy resources." + Environment.NewLine +
                          "Cancel - Cancel the deploy and view the conflicts.";
            ImageType = MessageBoxImage.Information;
            IsDependenciesButtonVisible = false;
            IsInfo = true;
            IsError = false;
            IsQuestion = false;
            IsDeleteAnywayButtonVisible = false;
            ApplyToAll = false;
            return Show();
        }

        public MessageBoxResult ShowDeployServerVersionConflict(string sourceServerVersion, string destinationServerVersion)
        {
            Buttons = MessageBoxButton.OKCancel;
            Header = "Deploy Version Conflicts";
            Description = "There is a conflict between the two versions in this deploy." +
                Environment.NewLine + "Source Server Version: " + sourceServerVersion +
                Environment.NewLine + "Destination Server Version: " + destinationServerVersion +
                Environment.NewLine + "Click OK to continue or Cancel to return." +
                Environment.NewLine +
                          "--------------------------------------------------------------------------------" +
                              Environment.NewLine +
                          "OK - Continue to deploy resources." + Environment.NewLine +
                          "Cancel - Cancel the deploy.";
            ImageType = MessageBoxImage.Information;
            IsDependenciesButtonVisible = false;
            IsInfo = true;
            IsError = false;
            IsQuestion = false;
            IsDeleteAnywayButtonVisible = false;
            ApplyToAll = false;
            return Show();
        }

        public MessageBoxResult ShowDeployServerMinVersionConflict(string sourceServerVersion, string destinationServerVersion)
        {
            Buttons = MessageBoxButton.OKCancel;
            Header = "Deploy Version Conflicts";
            Description = "There is a conflict between the two versions in this deploy." +
                Environment.NewLine + "Source Server Version: " + sourceServerVersion +
                Environment.NewLine + "Destination Minimum supported version: " + destinationServerVersion +
                Environment.NewLine + "The destination server does not support all the same features as the source server and your deployment is not guaranteed to work. " +
                Environment.NewLine + "Click OK to continue or Cancel to return." +
                Environment.NewLine +
                          "--------------------------------------------------------------------------------" +
                              Environment.NewLine +
                          "OK - Continue to deploy resources." + Environment.NewLine +
                          "Cancel - Cancel the deploy.";
            ImageType = MessageBoxImage.Information;
            IsDependenciesButtonVisible = false;
            IsInfo = true;
            IsError = false;
            IsQuestion = false;
            IsDeleteAnywayButtonVisible = false;
            ApplyToAll = false;
            return Show();
        }

        public MessageBoxResult ShowConnectServerVersionConflict(string selectedServerVersion, string currentServerVersion)
        {
            Buttons = MessageBoxButton.OK;
            Header = "Server Version Conflict";
            Description = "There is a version conflict with the current selected server." + Environment.NewLine +
                Environment.NewLine + "Selected Server Version: " + selectedServerVersion +
                Environment.NewLine + "Current Server Version: " + currentServerVersion + Environment.NewLine +
                Environment.NewLine + "Please make sure that the server you are trying to connect to has the latest version.";
            ImageType = MessageBoxImage.Error;
            IsDependenciesButtonVisible = false;
            IsInfo = false;
            IsError = true;
            IsQuestion = false;
            IsDeleteAnywayButtonVisible = false;
            ApplyToAll = false;
            return Show();
        }

        public MessageBoxResult ShowDeployResourceNameConflict(string conflictResourceName)
        {
            Buttons = MessageBoxButton.OK;
            Header = "Deploy ResourceName Conflicts";
            Description = "There is a conflict between the two resources in this deploy." +
                Environment.NewLine + "Conflict Resource Name: " + conflictResourceName +
                Environment.NewLine + "Click OK and rename the conflicting resource/s." +
                Environment.NewLine +
                          "--------------------------------------------------------------------------------" +
                              Environment.NewLine +
                          "OK - Cancel the deploy.";
            ImageType = MessageBoxImage.Information;
            IsDependenciesButtonVisible = false;
            IsInfo = true;
            IsError = false;
            IsQuestion = false;
            IsDeleteAnywayButtonVisible = false;
            ApplyToAll = false;
            return Show();
        }

        public MessageBoxResult ShowDeployNameConflict(string message)
        {
            Buttons = MessageBoxButton.OK;
            Header = "Deploy Name Conflicts";
            Description = message;
            ImageType = MessageBoxImage.Error;
            IsDependenciesButtonVisible = false;
            IsInfo = false;
            IsError = true;
            IsQuestion = false;
            IsDeleteAnywayButtonVisible = false;
            ApplyToAll = false;
            return Show();
        }

        public MessageBoxResult ShowDeploySuccessful(string message)
        {
            Buttons = MessageBoxButton.OK;
            Header = "Resource(s) Deployed Successfully";
            Description = message;
            ImageType = MessageBoxImage.Information;
            IsDependenciesButtonVisible = false;
            IsInfo = true;
            IsError = false;
            IsQuestion = false;
            IsDeleteAnywayButtonVisible = false;
            ApplyToAll = false;
            return Show();
        }

        public MessageBoxResult ShowSettingsCloseConfirmation()
        {
            Header = "Settings Have Changed";
            var description = "Settings have not been saved." + Environment.NewLine
                              + "Would you like to save the settings? " + Environment.NewLine +
                              "-----------------------------------------------------------------" +
                              Environment.NewLine +
                              "Yes - Save the settings." + Environment.NewLine +
                              "No - Discard your changes." + Environment.NewLine +
                              "Cancel - Returns you to settings.";
            Description = description;
            Buttons = MessageBoxButton.YesNoCancel;
            ImageType = MessageBoxImage.Information;
            IsDependenciesButtonVisible = false;
            IsInfo = true;
            IsError = false;
            IsQuestion = false;
            IsDeleteAnywayButtonVisible = false;
            ApplyToAll = false;
            return Show();
        }

        public MessageBoxResult ShowSchedulerCloseConfirmation()
        {
            Header = "Scheduler Task Has Changes";
            var description = "Scheduler Task has not been saved." + Environment.NewLine
                              + "Would you like to save the Task? " + Environment.NewLine +
                              "-----------------------------------------------------------------" +
                              Environment.NewLine +
                              "Yes - Save the Task." + Environment.NewLine +
                              "No - Discard your changes." + Environment.NewLine +
                              "Cancel - Returns you to Scheduler.";
            Description = description;
            Buttons = MessageBoxButton.YesNoCancel;
            ImageType = MessageBoxImage.Information;
            IsDependenciesButtonVisible = false;
            IsInfo = true;
            IsError = false;
            IsQuestion = false;
            IsDeleteAnywayButtonVisible = false;
            ApplyToAll = false;
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
            IsDependenciesButtonVisible = false;
            IsInfo = false;
            IsError = true;
            IsQuestion = false;
            IsDeleteAnywayButtonVisible = false;
            ApplyToAll = false;
            return Show();
        }

        public MessageBoxResult ShowConnectionTimeoutConfirmation(string serverName)
        {
            Header = "Server Is Unreachable";
            var description = " Unable to reach " + serverName + ": Connection timed out." + Environment.NewLine
                              + " Make sure the remote computer is powered on." + Environment.NewLine
                              + Environment.NewLine
                              + " Would you like to re-try? " + Environment.NewLine;
            Description = description;
            Buttons = MessageBoxButton.YesNo;
            ImageType = MessageBoxImage.Error;
            IsDependenciesButtonVisible = false;
            IsInfo = false;
            IsError = true;
            IsQuestion = false;
            IsDeleteAnywayButtonVisible = false;
            ApplyToAll = false;
            return Show();
        }

        public void ShowInvalidCharacterMessage(string invalidText)
        {
            Description = $"{invalidText} is invalid. Warewolf only supports latin characters";
            Header = "Invalid text";
            Buttons = MessageBoxButton.OK;
            ImageType = MessageBoxImage.Error;
            IsDependenciesButtonVisible = false;
            IsInfo = false;
            IsError = true;
            IsQuestion = false;
            IsDeleteAnywayButtonVisible = false;
            ApplyToAll = false;
            Show();
        }

        public MessageBoxResult ShowDeleteVersionMessage(string displayName)
        {
            Header = "Delete Version";
            var description = $"Are you sure you want to delete {displayName}?";
            Description = description;
            Buttons = MessageBoxButton.YesNo;
            ImageType = MessageBoxImage.Warning;
            IsDependenciesButtonVisible = false;
            IsInfo = true;
            IsError = false;
            IsQuestion = false;
            IsDeleteAnywayButtonVisible = false;
            ApplyToAll = false;
            return Show();
        }

        #region Implementation of IPopupMessages

        public IPopupMessage GetDeleteConfirmation(string nameOfItemBeingDeleted)
        {
            return new PopupMessage
            {
                Buttons = MessageBoxButton.YesNo,
                Header = Warewolf.Studio.Resources.Languages.Core.GenericConfirmation,
                Description = string.Format(Warewolf.Studio.Resources.Languages.Core.DeleteConfirmation, nameOfItemBeingDeleted),
                Image = MessageBoxImage.Warning,
                IsInfo = true,
                IsError = false,
                IsQuestion = false
            };
        }

        #endregion

        public IPopupMessage GetDuplicateMessage(string name)
        {
            return new PopupMessage
            {
                Buttons = MessageBoxButton.OK,
                Header = Warewolf.Studio.Resources.Languages.Core.InvalidPermissionHeader,
                Description = $"The name {name} already exists. Please choose a different name."
            };
        }
    }
}
