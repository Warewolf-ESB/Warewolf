#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Studio.Core;

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

        public MessageBoxResult Show(IPopupMessage popupMessage) => Show(popupMessage.Description, popupMessage.Header, popupMessage.Buttons, popupMessage.Image, popupMessage.DontShowAgainKey, popupMessage.IsDependenciesButtonVisible, popupMessage.IsError, popupMessage.IsInfo, popupMessage.IsQuestion);

        public MessageBoxResult Show()
        {
            if (ShowDev2MessageBox != null)
            {
                var dev2MessageBoxViewModel = ShowDev2MessageBox.Invoke(Description, Header, Buttons, ImageType, DontShowAgainKey, IsDependenciesButtonVisible, IsError, IsInfo, IsQuestion, UrlsFound, IsDeleteAnywayButtonVisible, ApplyToAll);
                DeleteAnyway = dev2MessageBoxViewModel.IsDeleteAnywaySelected;
                ApplyToAll = dev2MessageBoxViewModel.ApplyToAll;
                return dev2MessageBoxViewModel.Result;
            }
            else
            {
                throw new NullReferenceException("Cannot show popup dialog. Show Message Box function is null.");
            }
        }

        public MessageBoxResult Show(string description) => Show(description, "", MessageBoxButton.OK, MessageBoxImage.Asterisk, null, false, false, false, false, false, false);
        public MessageBoxResult Show(string description, string header) => Show(description, header, MessageBoxButton.OK, MessageBoxImage.Asterisk, null, false, false, false, false, false, false);
        public MessageBoxResult Show(string description, string header, MessageBoxButton buttons) => Show(description, header, buttons, MessageBoxImage.Asterisk, null, false, false, false, false, false, false);
        public MessageBoxResult Show(string description, string header, MessageBoxButton buttons, MessageBoxImage image) => Show(description, header, buttons, image, null, false, false, false, false, false, false);
        public MessageBoxResult Show(string description, string header, MessageBoxButton buttons, MessageBoxImage image, string dontShowAgainKey) => Show(description, header, buttons, image, dontShowAgainKey, false, false, false, false, false, false);
        public MessageBoxResult Show(string description, string header, MessageBoxButton buttons, MessageBoxImage image, string dontShowAgainKey, bool isDependenciesButtonVisible) => Show(description, header, buttons, image, dontShowAgainKey, isDependenciesButtonVisible, false, false, false, false, false);
        public MessageBoxResult Show(string description, string header, MessageBoxButton buttons, MessageBoxImage image, string dontShowAgainKey, bool isDependenciesButtonVisible, bool isError) => Show(description, header, buttons, image, dontShowAgainKey, isDependenciesButtonVisible, isError, false, false, false, false);
        public MessageBoxResult Show(string description, string header, MessageBoxButton buttons, MessageBoxImage image, string dontShowAgainKey, bool isDependenciesButtonVisible, bool isError, bool isInfo) => Show(description, header, buttons, image, dontShowAgainKey, isDependenciesButtonVisible, isError, isInfo, false, false, false);
        public MessageBoxResult Show(string description, string header, MessageBoxButton buttons, MessageBoxImage image, string dontShowAgainKey, bool isDependenciesButtonVisible, bool isError, bool isInfo, bool isQuestion) => Show(description, header, buttons, image, dontShowAgainKey, isDependenciesButtonVisible, isError, isInfo, isQuestion, false, false);
        public MessageBoxResult Show(string description, string header, MessageBoxButton buttons, MessageBoxImage image, string dontShowAgainKey, bool isDependenciesButtonVisible, bool isError, bool isInfo, bool isQuestion, bool isDeleteAnywayButtonVisible) => Show(description, header, buttons, image, dontShowAgainKey, isDependenciesButtonVisible, isError, isInfo, isQuestion, isDeleteAnywayButtonVisible, false);
        public MessageBoxResult Show(string description, string header, MessageBoxButton buttons, MessageBoxImage image, string dontShowAgainKey, bool isDependenciesButtonVisible, bool isError, bool isInfo, bool isQuestion, bool isDeleteAnywayButtonVisible, bool applyToAll)
        {
            AssignCommonValues(header, description, buttons);
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

        internal Func<string, string, MessageBoxButton, MessageBoxImage, string, bool, bool, bool, bool, List<string>, bool, bool, MessageBoxViewModel>
            ShowDev2MessageBox = (description, header, buttons, imageType, dontShowAgainKey, isDependenciesButtonVisible, isError, isInfo, isQuestion, urlsFound, isDeleteAnywayButtonVisible, applyToAll) => Dev2MessageBoxViewModel.Show(description, header, buttons, imageType, dontShowAgainKey, isDependenciesButtonVisible, isError, isInfo, isQuestion, urlsFound, isDeleteAnywayButtonVisible, applyToAll);

        public MessageBoxResult ShowNotConnected()
        {
            AssignCommonValues("Server Unreachable", "You can not change the settings for a server that is unreachable.", MessageBoxButton.OK);
            ImageType = MessageBoxImage.Error;
            IsDependenciesButtonVisible = false;
            IsInfo = false;
            IsError = true;
            IsQuestion = false;
            IsDeleteAnywayButtonVisible = false;
            ApplyToAll = false;
            return Show();
        }
        
        public MessageBoxResult ShowErrorMessage(string error)
        {
            AssignCommonValues("Error", error, MessageBoxButton.OK);
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
            var description = String.Format(Warewolf.Studio.Resources.Languages.Core.RollbackMessage, displayName, Environment.NewLine);
            AssignCommonValues(Warewolf.Studio.Resources.Languages.Core.RollbackHeader, description, MessageBoxButton.YesNo);
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
            var description = string.Format(Warewolf.Studio.Resources.Languages.Core.VariablesInput_Information, Environment.NewLine);
            AssignCommonValues(Warewolf.Studio.Resources.Languages.Core.VariablesInput_Information_Title, description, MessageBoxButton.OK);
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

        public MessageBoxResult ShowResourcesConflict(List<string> resourceDuplicates)
        {
            UrlsFound = resourceDuplicates;
            AssignCommonValues("Duplicated Resources", "Duplicate resources found. Please resolve the files on File Explorer. \nTo view the resource, click on the individual items below.", MessageBoxButton.OK);
            ImageType = MessageBoxImage.Error;
            IsDependenciesButtonVisible = false;
            IsInfo = false;
            IsError = true;
            IsQuestion = false;
            IsDeleteAnywayButtonVisible = false;
            ApplyToAll = false;
            return Show();
        }

        public MessageBoxResult ShowSourceAlreadyExistOpenFromResources()
        {
            AssignCommonValues("Source already exists", "The Source you are attempting to Open already exists. \nOpen the Source from " + EnvironmentVariables.ResourcePath + ".", MessageBoxButton.OK);
            ImageType = MessageBoxImage.Information;
            IsDependenciesButtonVisible = false;
            IsInfo = true;
            IsError = false;
            IsQuestion = false;
            IsDeleteAnywayButtonVisible = false;
            ApplyToAll = false;
            return Show();
        }
        public MessageBoxResult ShowOverwiteResourceDialog()
        {
            AssignCommonValues("Edited Resource already exists", "The Resource you are attempting to Save already exists in "+ EnvironmentVariables.ResourcePath + ". \nClick Ok to Overwrite the existing resource or Cancel to Exit.", MessageBoxButton.OKCancel);
            ImageType = MessageBoxImage.Information;
            IsDependenciesButtonVisible = false;
            IsInfo = true;
            IsError = false;
            IsQuestion = false;
            IsDeleteAnywayButtonVisible = false;
            ApplyToAll = false;
            return Show();
        }
        public MessageBoxResult ShowResourcesNotInCorrectPath()
        {
            AssignCommonValues("Unknown Resource", "The Resource you are attempting to open is unknown by the server. \nClick Ok to have the resource moved to the server or Cancel to Exit.", MessageBoxButton.OKCancel);
            ImageType = MessageBoxImage.Information;
            IsDependenciesButtonVisible = false;
            IsInfo = true;
            IsError = false;
            IsQuestion = false;
            IsDeleteAnywayButtonVisible = false;
            ApplyToAll = false;
            return Show();
        }

        public MessageBoxResult ShowInstallationErrorOccurred()
        {
            AssignCommonValues("Server Startup Error", Warewolf.Studio.Resources.Languages.Core.DotNetFrameworkInstallError, MessageBoxButton.OK);
            ImageType = MessageBoxImage.Error;
            IsDependenciesButtonVisible = false;
            IsInfo = false;
            IsError = true;
            IsQuestion = false;
            IsDeleteAnywayButtonVisible = false;
            ApplyToAll = false;
            return Show();
        }

        public MessageBoxResult ShowCanNotMoveResource()
        {
            AssignCommonValues("Source data contains encrypted connections strings.", "If the Source was created on this Server, Click Continue Warewolf will attempt to Open it. \nIf the Source was created on the Remote server, click Cancel and then deploy it to this machine from the resource's originating server.", MessageBoxButton.OKCancel);
            ImageType = MessageBoxImage.Information;
            IsDependenciesButtonVisible = false;
            IsInfo = true;
            IsError = false;
            IsQuestion = false;
            IsDeleteAnywayButtonVisible = false;
            ApplyToAll = false;
            return Show();
        }

        public MessageBoxResult ShowServerNotConnected(string server)
        {
            var description = "The server " + server + " is unreachable. \n \nPlease make sure the Warewolf Server service is running on that machine.";
            AssignCommonValues("Server Unreachable", description, MessageBoxButton.OK);
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
            var description = "Are you sure you want to delete " + nameOfItemBeingDeleted + "?";
            AssignCommonValues("Are you sure?", description, MessageBoxButton.YesNo);
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
            var description = "Thank you for taking the time to log it. Follow the issue " + Environment.NewLine +
                "in the Community to keep updated on the progress.";
            AssignCommonValues("We've got your feedback!", description, MessageBoxButton.OK);
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
            var description = "Unable to retrieve tasks." + Environment.NewLine +
                          "ERROR: " + errorMessage + ". " + Environment.NewLine +
                          "Please check that there a no corrupt files." + Environment.NewLine +
                         @"C:\Windows\System32\Tasks\Warewolf";
            AssignCommonValues("Scheduler Load Error", description, MessageBoxButton.OK);
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
            var description = "The following task has been renamed " + oldName + " -> " + newName + ". You will lose the history for the old task." + Environment.NewLine +
                          " Would you like to save the new name?" + Environment.NewLine +
                          "-----------------------------------------------------------------" +
                              Environment.NewLine +
                          "Yes - Save with the new name." + Environment.NewLine +
                          "No - Save with the old name." + Environment.NewLine +
                          "Cancel - Returns you to Scheduler.";
            AssignCommonValues("Rename Conflicts", description, MessageBoxButton.YesNoCancel);
            ImageType = MessageBoxImage.Information;
            IsDependenciesButtonVisible = false;
            IsInfo = true;
            IsError = false;
            IsQuestion = false;
            IsDeleteAnywayButtonVisible = false;
            ApplyToAll = false;
            return Show();
        }

        public MessageBoxResult ShowLoggerSourceChange(string resourceName)
        {
            var description = "You are about to make changes to the source assigned to log queries." + Environment.NewLine
                                   + "In doing so, you will need to manually restart the logger for the changes to take effect." + Environment.NewLine
                                   + "Would you like to continue to save the changes? " + Environment.NewLine +
                                   "-----------------------------------------------------------------" +
                                   Environment.NewLine +
                                   "Yes - Save changes." + Environment.NewLine +
                                   "No - Discard your changes." + Environment.NewLine +
                                   $"Cancel - Returns you to {resourceName}.";
            AssignCommonValues($"{resourceName} Has Changes", description, MessageBoxButton.YesNoCancel);
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
            var correctDesc = String.Empty;
            if (conflictCount == 1)
            {
                correctDesc = "There is [ " + conflictCount + " ] conflict that occurs";
            }
            if (conflictCount > 1)
            {
                correctDesc = "There are [ " + conflictCount + " ] conflicts that occur";
            }
            var description = correctDesc + " in this deploy." + Environment.NewLine + "Click OK to override the conflicts or Cancel to view the conflicting resources." + Environment.NewLine +
                          "--------------------------------------------------------------------------------" +
                              Environment.NewLine +
                          "OK - Continue to deploy resources." + Environment.NewLine +
                          "Cancel - Cancel the deploy and view the conflicts.";
            AssignCommonValues("Deploy Conflicts", description, MessageBoxButton.OKCancel);
            ImageType = MessageBoxImage.Information;
            IsDependenciesButtonVisible = false;
            IsInfo = true;
            IsError = false;
            IsQuestion = false;
            IsDeleteAnywayButtonVisible = false;
            ApplyToAll = false;
            return Show();
        }

        public MessageBoxResult ShowDeployNoResourcesToDeploy(string header, string description)
        {
            AssignCommonValues(header, description, MessageBoxButton.OK);
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
            var description = "There is a conflict between the two versions in this deploy." +
                Environment.NewLine + "Source Server Version: " + sourceServerVersion +
                Environment.NewLine + "Destination Server Version: " + destinationServerVersion +
                Environment.NewLine + "Click OK to continue or Cancel to return." +
                Environment.NewLine +
                          "--------------------------------------------------------------------------------" +
                              Environment.NewLine +
                          "OK - Continue to deploy resources." + Environment.NewLine +
                          "Cancel - Cancel the deploy.";
            AssignCommonValues("Deploy Version Conflicts", description, MessageBoxButton.OKCancel);
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
            var description = "There is a conflict between the two versions in this deploy." +
                Environment.NewLine + "Source Server Version: " + sourceServerVersion +
                Environment.NewLine + "Destination Minimum supported version: " + destinationServerVersion +
                Environment.NewLine + "The destination server does not support all the same features as the source server and your deployment is not guaranteed to work. " +
                Environment.NewLine + "Click OK to continue or Cancel to return." +
                Environment.NewLine +
                          "--------------------------------------------------------------------------------" +
                              Environment.NewLine +
                          "OK - Continue to deploy resources." + Environment.NewLine +
                          "Cancel - Cancel the deploy.";
            AssignCommonValues("Deploy Version Conflicts", description, MessageBoxButton.OKCancel);
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
            var description = "There is a version conflict with the current selected server." + Environment.NewLine +
                Environment.NewLine + "Selected Server Version: " + selectedServerVersion +
                Environment.NewLine + "Current Server Version: " + currentServerVersion + Environment.NewLine +
                Environment.NewLine + "Please make sure that the server you are trying to connect to has the latest version.";
            AssignCommonValues("Server Version Conflict", description, MessageBoxButton.OK);
            ImageType = MessageBoxImage.Error;
            IsDependenciesButtonVisible = false;
            IsInfo = false;
            IsError = true;
            IsQuestion = false;
            IsDeleteAnywayButtonVisible = false;
            ApplyToAll = false;
            return Show();
        }

        public MessageBoxResult ShowSearchServerVersionConflict(string serverVersion, string minimumSupportedVersion)
        {
            var description = Warewolf.Studio.Resources.Languages.Core.SearchVersionConflictError +
                                        Environment.NewLine + GlobalConstants.ServerVersion + serverVersion +
                                        Environment.NewLine + GlobalConstants.MinimumSupportedVersion + minimumSupportedVersion +
                                        Environment.NewLine + "Click OK to continue or Cancel to return." +
                Environment.NewLine +
                          "--------------------------------------------------------------------------------" +
                              Environment.NewLine +
                          "OK - Continue to search resources." + Environment.NewLine +
                          "Cancel - Cancel the search.";
            AssignCommonValues("Server Version Conflict", description, MessageBoxButton.OKCancel);
            ImageType = MessageBoxImage.Information;
            IsDependenciesButtonVisible = false;
            IsInfo = true;
            IsError = false;
            IsQuestion = false;
            IsDeleteAnywayButtonVisible = false;
            ApplyToAll = false;

            return Show();
        }

        public MessageBoxResult ShowDeployResourceNameConflict(string conflictResourceName)
        {

            var description = "There is a conflict between the two resources in this deploy." +
                Environment.NewLine + "Conflict Resource Name: " + conflictResourceName +
                Environment.NewLine + "Click OK and rename the conflicting resource/s." +
                Environment.NewLine +
                          "--------------------------------------------------------------------------------" +
                              Environment.NewLine +
                          "OK - Cancel the deploy.";
            AssignCommonValues("Deploy ResourceName Conflicts", description, MessageBoxButton.OK);
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
            AssignCommonValues("Deploy Name Conflicts", message, MessageBoxButton.OK);
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
            AssignCommonValues("Resource(s) Deployed Successfully", message, MessageBoxButton.OK);
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
            var description = "Settings have not been saved." + Environment.NewLine
                              + "Would you like to save the settings? " + Environment.NewLine +
                              "-----------------------------------------------------------------" +
                              Environment.NewLine +
                              "Yes - Save the settings." + Environment.NewLine +
                              "No - Discard your changes." + Environment.NewLine +
                              "Cancel - Returns you to settings.";
            AssignCommonValues("Settings Have Changed", description, MessageBoxButton.YesNoCancel);
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
            var description = "Scheduler Task has not been saved." + Environment.NewLine
                              + "Would you like to save the Task? " + Environment.NewLine +
                              "-----------------------------------------------------------------" +
                              Environment.NewLine +
                              "Yes - Save the Task." + Environment.NewLine +
                              "No - Discard your changes." + Environment.NewLine +
                              "Cancel - Returns you to Scheduler.";
            AssignCommonValues("Scheduler Task Has Changes", description, MessageBoxButton.YesNoCancel);
            ImageType = MessageBoxImage.Information;
            IsDependenciesButtonVisible = false;
            IsInfo = true;
            IsError = false;
            IsQuestion = false;
            IsDeleteAnywayButtonVisible = false;
            ApplyToAll = false;
            return Show();
        }

        public MessageBoxResult ShowTasksCloseConfirmation()
        {
            var description = "Tasks have not been saved." + Environment.NewLine
                              + "Would you like to save the tasks? " + Environment.NewLine +
                              "-----------------------------------------------------------------" +
                              Environment.NewLine +
                              "Yes - Save the tasks." + Environment.NewLine +
                              "No - Discard your changes." + Environment.NewLine +
                              "Cancel - Returns you to tasks.";
            AssignCommonValues("Tasks Have Changed", description, MessageBoxButton.YesNoCancel);
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
            var description = "The following error occurred on save:" + Environment.NewLine
                              + errorMessage;
            AssignCommonValues("Saving Error", description, MessageBoxButton.OK);
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
            var description = " Unable to reach " + serverName + ": Connection timed out." + Environment.NewLine
                              + " Make sure the remote computer is powered on." + Environment.NewLine
                              + Environment.NewLine
                              + " Would you like to re-try? " + Environment.NewLine;
            AssignCommonValues("Server Is Unreachable", description, MessageBoxButton.YesNo);
            ImageType = MessageBoxImage.Error;
            IsDependenciesButtonVisible = false;
            IsInfo = false;
            IsError = true;
            IsQuestion = false;
            IsDeleteAnywayButtonVisible = false;
            ApplyToAll = false;
            return Show();
        }

        public void ShowInvalidElasticsearchIndexFormatMessage(string invalidText)
        {
            var description = $"{invalidText} is invalid. Elasticsearch Index only supports: " + Environment.NewLine
                                                                                               + " Lowercase" + Environment.NewLine
                                                                                               + " Cannot be . or .." + Environment.NewLine
                                                                                               + " Cannot start with -, _, +" + Environment.NewLine
                                                                                               + " Cannot include special characters" + Environment.NewLine
                                                                                               + " Cannot be longer than 255 characters" + Environment.NewLine;

            AssignCommonValues("Invalid Elasticsearch Index", description, MessageBoxButton.OK);
            ImageType = MessageBoxImage.Error;
            IsDependenciesButtonVisible = false;
            IsInfo = false;
            IsError = true;
            IsQuestion = false;
            IsDeleteAnywayButtonVisible = false;
            ApplyToAll = false;
            Show();
        }
        public void ShowInvalidCharacterMessage(string invalidText)
        {
            var description = $"{invalidText} is invalid. Warewolf only supports latin characters";
            AssignCommonValues("Invalid text", description, MessageBoxButton.OK);
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
            var description = $"Are you sure you want to delete {displayName}?";
            AssignCommonValues("Delete Version", description, MessageBoxButton.YesNo);
            ImageType = MessageBoxImage.Warning;
            IsDependenciesButtonVisible = false;
            IsInfo = true;
            IsError = false;
            IsQuestion = false;
            IsDeleteAnywayButtonVisible = false;
            ApplyToAll = false;
            return Show();
        }
        public MessageBoxResult ShowInvalidResourcePermission()
        {
            AssignCommonValues(StringResources.SaveErrorPrefix,
            StringResources.SaveSettingsInvalidPermissionEntry, MessageBoxButton.OK);
            ImageType = MessageBoxImage.Error;
            IsDependenciesButtonVisible = false;
            IsInfo = false;
            IsError = true;
            IsQuestion = false;
            IsDeleteAnywayButtonVisible = false;
            ApplyToAll = false;
            return Show();
        }
        public MessageBoxResult ShowHasDuplicateResourcePermissions()
        {
            AssignCommonValues(StringResources.SaveErrorPrefix,
            StringResources.SaveSettingsDuplicateResourcePermissions, MessageBoxButton.OK);
            ImageType = MessageBoxImage.Error;
            IsDependenciesButtonVisible = false;
            IsInfo = false;
            IsError = true;
            IsQuestion = false;
            IsDeleteAnywayButtonVisible = false;
            ApplyToAll = false;
            return Show();
        }
        public MessageBoxResult ShowHasDuplicateServerPermissions()
        {
            AssignCommonValues(StringResources.SaveErrorPrefix,
            StringResources.SaveSettingsDuplicateServerPermissions, MessageBoxButton.OK);
            ImageType = MessageBoxImage.Error;
            IsDependenciesButtonVisible = false;
            IsInfo = false;
            IsError = true;
            IsQuestion = false;
            IsDeleteAnywayButtonVisible = false;
            ApplyToAll = false;
            return Show();
        }
        public MessageBoxResult ShowSaveServerNotReachableErrorMsg()
        {
            AssignCommonValues(StringResources.SaveErrorPrefix,
            StringResources.SaveServerNotReachableErrorMsg, MessageBoxButton.OK);
            ImageType = MessageBoxImage.Error;
            IsDependenciesButtonVisible = false;
            IsInfo = false;
            IsError = true;
            IsQuestion = false;
            IsDeleteAnywayButtonVisible = false;
            ApplyToAll = false;
            return Show();
        }
        public MessageBoxResult ShowSaveSettingsPermissionsErrorMsg()
        {
            AssignCommonValues(StringResources.SaveErrorPrefix,
            StringResources.SaveSettingsPermissionsErrorMsg, MessageBoxButton.OK);
            ImageType = MessageBoxImage.Error;
            IsDependenciesButtonVisible = false;
            IsInfo = false;
            IsError = true;
            IsQuestion = false;
            IsDeleteAnywayButtonVisible = false;
            ApplyToAll = false;
            return Show();
        }
        #region Implementation of IPopupMessages

        public IPopupMessage GetDeleteConfirmation(string nameOfItemBeingDeleted) => new PopupMessage
        {
            Buttons = MessageBoxButton.YesNo,
            Header = Warewolf.Studio.Resources.Languages.Core.GenericConfirmation,
            Description = string.Format(Warewolf.Studio.Resources.Languages.Core.DeleteConfirmation, nameOfItemBeingDeleted),
            Image = MessageBoxImage.Warning,
            IsInfo = true,
            IsError = false,
            IsQuestion = false
        };

        #endregion

        public IPopupMessage GetDuplicateMessage(string name) => new PopupMessage
        {
            Buttons = MessageBoxButton.OK,
            Header = Warewolf.Studio.Resources.Languages.Core.InvalidPermissionHeader,
            Description = $"The name {name} already exists. Please choose a different name."
        };

        void AssignCommonValues(string header, string description, MessageBoxButton button)
        {
            Header = header;
            Description = description;
            Buttons = button;
        }
    }
}
