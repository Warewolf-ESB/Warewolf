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
using System.Linq;
using System.Windows;
using Dev2.Common;
using FontAwesome.WPF;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Studio.Core.Popup;
using Warewolf.Studio.ViewModels;
using PopupController = Dev2.Studio.Controller.PopupController;

namespace Dev2.Core.Tests
{
    [TestClass]
    // ReSharper disable InconsistentNaming
    public class PopupControllerTests
    {
        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("PopupController_ShowDeleteConfirmation")]
        public void PopupController_ShowDeleteConfirmation_SetProperties_AllPropertiesDisplayed()
        {
            //------------Setup for test--------------------------
            var popupWasCalled = false;
            string description = string.Empty;
            string header = string.Empty;
            MessageBoxButton buttons = MessageBoxButton.YesNoCancel;

            var popupController = new PopupController
                {
                    ShowDev2MessageBox = (desc, hdr, btn, img, dntShwAgKy, isDependBtnVisible, isErr, isInf, isQuest, duplicates, isDeleteAnywayBtnVisible, applyToAll) =>
                        {
                            description = desc;
                            header = hdr;
                            buttons = btn;
                            popupWasCalled = true;
                            return new MessageBoxViewModel(desc,hdr,btn,FontAwesomeIcon.Adn, isDependBtnVisible,isErr,isInf,isQuest,duplicates,isDeleteAnywayBtnVisible,applyToAll)
                            {
                                Result = MessageBoxResult.OK
                            };
                        }
                };

            const string NameOfItemBeingDeleted = "Random button";
            //------------Execute Test---------------------------
            popupController.ShowDeleteConfirmation(NameOfItemBeingDeleted);
            //------------Assert Results-------------------------
            Assert.IsTrue(popupWasCalled);
            Assert.AreEqual(MessageBoxButton.YesNo, buttons);
            Assert.AreEqual("Are you sure?", header);
            Assert.AreEqual("Are you sure you want to delete " + NameOfItemBeingDeleted + "?", description);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("PopupController_ShowDeleteVersionMessage")]
        public void PopupController_ShowDeleteVersionMessage_SetProperties_AllPropertiesDisplayed()
        {
            //------------Setup for test--------------------------
            var popupWasCalled = false;
            string description = string.Empty;
            string header = string.Empty;
            MessageBoxButton buttons = MessageBoxButton.YesNo;

            var popupController = new PopupController
            {
                ShowDev2MessageBox = (desc, hdr, btn, img, dntShwAgKy, isDependBtnVisible, isErr, isInf, isQuest, duplicates, isDeleteAnywayBtnVisible, applyToAll) =>
                {
                    description = desc;
                    header = hdr;
                    buttons = btn;
                    popupWasCalled = true;
                    return new MessageBoxViewModel(desc, hdr, btn, FontAwesomeIcon.Adn, isDependBtnVisible, isErr, isInf, isQuest, duplicates, isDeleteAnywayBtnVisible, applyToAll)
                    {
                        Result = MessageBoxResult.OK
                    };
                }
            };

            const string NameOfItemBeingDeleted = "Random button";
            //------------Execute Test---------------------------
            popupController.ShowDeleteVersionMessage(NameOfItemBeingDeleted);
            //------------Assert Results-------------------------
            Assert.IsTrue(popupWasCalled);
            Assert.AreEqual(MessageBoxButton.YesNo, buttons);
            Assert.AreEqual("Delete Version", header);
            Assert.AreEqual("Are you sure you want to delete " + NameOfItemBeingDeleted + "?", description);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("PopupController_ShowDeployConflict")]
        public void PopupController_ShowDeployConflict_SetProperties_AllPropertiesDisplayed()
        {
            //------------Setup for test--------------------------
            var popupWasCalled = false;
            string description = string.Empty;
            string header = string.Empty;
            MessageBoxButton buttons = MessageBoxButton.OKCancel;

            var popupController = new PopupController
            {
                ShowDev2MessageBox = (desc, hdr, btn, img, dntShwAgKy, isDependBtnVisible, isErr, isInf, isQuest, duplicates, isDeleteAnywayBtnVisible, applyToAll) =>
                {
                    description = desc;
                    header = hdr;
                    buttons = btn;
                    popupWasCalled = true;
                    return new MessageBoxViewModel(desc, hdr, btn, FontAwesomeIcon.Adn, isDependBtnVisible, isErr, isInf, isQuest, duplicates, isDeleteAnywayBtnVisible, applyToAll)
                    {
                        Result = MessageBoxResult.OK
                    };
                }
            };
            
            //------------Execute Test---------------------------
            int conflictCount = 1;

            popupController.ShowDeployConflict(conflictCount);

            string correctDesc = String.Empty;
            if (conflictCount == 1)
            {
                correctDesc = "There is [ " + conflictCount + " ] conflict that occurs";
            }

            string Description = correctDesc + " in this deploy." + Environment.NewLine + "Click OK to override the conflicts or Cancel to view the conflicting resources." + Environment.NewLine +
                          "--------------------------------------------------------------------------------" +
                              Environment.NewLine +
                          "OK - Continue to deploy resources." + Environment.NewLine +
                          "Cancel - Cancel the deploy and view the conflicts.";

            //------------Assert Results-------------------------
            Assert.IsTrue(popupWasCalled);
            Assert.AreEqual(MessageBoxButton.OKCancel, buttons);
            Assert.AreEqual("Deploy Conflicts", header);
            Assert.AreEqual(Description, description);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("PopupController_ShowDeployConflict")]
        public void PopupController_ShowDeployConflictTwo_SetProperties_AllPropertiesDisplayed()
        {
            //------------Setup for test--------------------------
            var popupWasCalled = false;
            string description = string.Empty;
            string header = string.Empty;
            MessageBoxButton buttons = MessageBoxButton.OKCancel;

            var popupController = new PopupController
            {
                ShowDev2MessageBox = (desc, hdr, btn, img, dntShwAgKy, isDependBtnVisible, isErr, isInf, isQuest, duplicates, isDeleteAnywayBtnVisible, applyToAll) =>
                {
                    description = desc;
                    header = hdr;
                    buttons = btn;
                    popupWasCalled = true;
                    return new MessageBoxViewModel(desc, hdr, btn, FontAwesomeIcon.Adn, isDependBtnVisible, isErr, isInf, isQuest, duplicates, isDeleteAnywayBtnVisible, applyToAll)
                    {
                        Result = MessageBoxResult.OK
                    };
                }
            };

            //------------Execute Test---------------------------
            int conflictCount = 2;

            popupController.ShowDeployConflict(conflictCount);

            string correctDesc = String.Empty;
            if (conflictCount > 1)
            {
                correctDesc = "There are [ " + conflictCount + " ] conflicts that occur";
            }

            string Description = correctDesc + " in this deploy." + Environment.NewLine + "Click OK to override the conflicts or Cancel to view the conflicting resources." + Environment.NewLine +
                          "--------------------------------------------------------------------------------" +
                              Environment.NewLine +
                          "OK - Continue to deploy resources." + Environment.NewLine +
                          "Cancel - Cancel the deploy and view the conflicts.";

            //------------Assert Results-------------------------
            Assert.IsTrue(popupWasCalled);
            Assert.AreEqual(MessageBoxButton.OKCancel, buttons);
            Assert.AreEqual("Deploy Conflicts", header);
            Assert.AreEqual(Description, description);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("PopupController_ShowDeployNameConflict")]
        public void PopupController_ShowDeployNameConflict_SetProperties_AllPropertiesDisplayed()
        {
            //------------Setup for test--------------------------
            var popupWasCalled = false;
            string description = string.Empty;
            string header = string.Empty;
            MessageBoxButton buttons = MessageBoxButton.OKCancel;

            var popupController = new PopupController
            {
                ShowDev2MessageBox = (desc, hdr, btn, img, dntShwAgKy, isDependBtnVisible, isErr, isInf, isQuest, duplicates, isDeleteAnywayBtnVisible, applyToAll) =>
                {
                    description = desc;
                    header = hdr;
                    buttons = btn;
                    popupWasCalled = true;
                    return new MessageBoxViewModel(desc, hdr, btn, FontAwesomeIcon.Adn, isDependBtnVisible, isErr, isInf, isQuest, duplicates, isDeleteAnywayBtnVisible, applyToAll)
                    {
                        Result = MessageBoxResult.OK
                    };
                }
            };

            //------------Execute Test---------------------------
            string message = "Workflow1 already exists";
            popupController.ShowDeployNameConflict(message);

            //------------Assert Results-------------------------
            Assert.IsTrue(popupWasCalled);
            Assert.AreEqual(MessageBoxButton.OK, buttons);
            Assert.AreEqual("Deploy Name Conflicts", header);
            Assert.AreEqual(message, description);
            Assert.IsFalse(popupController.IsInfo);
            Assert.IsFalse(popupController.IsDependenciesButtonVisible);
            Assert.IsTrue(popupController.IsError);
            Assert.IsFalse(popupController.IsQuestion);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("PopupController_ShowDeployResourceNameConflict")]
        public void PopupController_ShowDeployResourceNameConflict_SetProperties_AllPropertiesDisplayed()
        {
            //------------Setup for test--------------------------
            var popupWasCalled = false;
            string description = string.Empty;
            string header = string.Empty;
            MessageBoxButton buttons = MessageBoxButton.OK;

            var popupController = new PopupController
            {
                ShowDev2MessageBox = (desc, hdr, btn, img, dntShwAgKy, isDependBtnVisible, isErr, isInf, isQuest, duplicates, isDeleteAnywayBtnVisible, applyToAll) =>
                {
                    description = desc;
                    header = hdr;
                    buttons = btn;
                    popupWasCalled = true;
                    return new MessageBoxViewModel(desc, hdr, btn, FontAwesomeIcon.Adn, isDependBtnVisible, isErr, isInf, isQuest, duplicates, isDeleteAnywayBtnVisible, applyToAll)
                    {
                        Result = MessageBoxResult.OK
                    };
                }
            };

            //------------Execute Test---------------------------
            string conflictResourceName = "Workflow1";
            popupController.ShowDeployResourceNameConflict(conflictResourceName);

            string message = "There is a conflict between the two resources in this deploy." +
                Environment.NewLine + "Conflict Resource Name: " + conflictResourceName +
                Environment.NewLine + "Click OK and rename the conflicting resource/s." +
                Environment.NewLine +
                          "--------------------------------------------------------------------------------" +
                              Environment.NewLine +
                          "OK - Cancel the deploy.";

            //------------Assert Results-------------------------
            Assert.IsTrue(popupWasCalled);
            Assert.AreEqual(MessageBoxButton.OK, buttons);
            Assert.AreEqual("Deploy ResourceName Conflicts", header);
            Assert.AreEqual(message, description);
            Assert.IsTrue(popupController.IsInfo);
            Assert.IsFalse(popupController.IsDependenciesButtonVisible);
            Assert.IsFalse(popupController.IsError);
            Assert.IsFalse(popupController.IsQuestion);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("PopupController_ShowDeployServerMinVersionConflict")]
        public void PopupController_ShowDeployServerMinVersionConflict_SetProperties_AllPropertiesDisplayed()
        {
            //------------Setup for test--------------------------
            var popupWasCalled = false;
            string description = string.Empty;
            string header = string.Empty;
            MessageBoxButton buttons = MessageBoxButton.OKCancel;

            var popupController = new PopupController
            {
                ShowDev2MessageBox = (desc, hdr, btn, img, dntShwAgKy, isDependBtnVisible, isErr, isInf, isQuest, duplicates, isDeleteAnywayBtnVisible, applyToAll) =>
                {
                    description = desc;
                    header = hdr;
                    buttons = btn;
                    popupWasCalled = true;
                    return new MessageBoxViewModel(desc, hdr, btn, FontAwesomeIcon.Adn, isDependBtnVisible, isErr, isInf, isQuest, duplicates, isDeleteAnywayBtnVisible, applyToAll)
                    {
                        Result = MessageBoxResult.OK
                    };
                }
            };

            //------------Execute Test---------------------------
            string sourceServerVersion = "0.0.0.9";
            string destinationServerVersion = "1.0.0.0";
            popupController.ShowDeployServerMinVersionConflict(sourceServerVersion, destinationServerVersion);

            string message = "There is a conflict between the two versions in this deploy." +
                Environment.NewLine + "Source Server Version: " + sourceServerVersion +
                Environment.NewLine + "Destination Minimum supported version: " + destinationServerVersion +
                Environment.NewLine + "The destination server does not support all the same features as the source server and your deployment is not guaranteed to work. " +
                Environment.NewLine + "Click OK to continue or Cancel to return." +
                Environment.NewLine +
                          "--------------------------------------------------------------------------------" +
                              Environment.NewLine +
                          "OK - Continue to deploy resources." + Environment.NewLine +
                          "Cancel - Cancel the deploy.";

            //------------Assert Results-------------------------
            Assert.IsTrue(popupWasCalled);
            Assert.AreEqual(MessageBoxButton.OKCancel, buttons);
            Assert.AreEqual("Deploy Version Conflicts", header);
            Assert.AreEqual(message, description);
            Assert.IsTrue(popupController.IsInfo);
            Assert.IsFalse(popupController.IsDependenciesButtonVisible);
            Assert.IsFalse(popupController.IsError);
            Assert.IsFalse(popupController.IsQuestion);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("PopupController_ShowDeployServerVersionConflict")]
        public void PopupController_ShowDeployServerVersionConflict_SetProperties_AllPropertiesDisplayed()
        {
            //------------Setup for test--------------------------
            var popupWasCalled = false;
            string description = string.Empty;
            string header = string.Empty;
            MessageBoxButton buttons = MessageBoxButton.OKCancel;

            var popupController = new PopupController
            {
                ShowDev2MessageBox = (desc, hdr, btn, img, dntShwAgKy, isDependBtnVisible, isErr, isInf, isQuest, duplicates, isDeleteAnywayBtnVisible, applyToAll) =>
                {
                    description = desc;
                    header = hdr;
                    buttons = btn;
                    popupWasCalled = true;
                    return new MessageBoxViewModel(desc, hdr, btn, FontAwesomeIcon.Adn, isDependBtnVisible, isErr, isInf, isQuest, duplicates, isDeleteAnywayBtnVisible, applyToAll)
                    {
                        Result = MessageBoxResult.OK
                    };
                }
            };

            //------------Execute Test---------------------------
            string sourceServerVersion = "0.0.0.9";
            string destinationServerVersion = "1.0.0.0";
            popupController.ShowDeployServerVersionConflict(sourceServerVersion, destinationServerVersion);

            string message = "There is a conflict between the two versions in this deploy." +
                Environment.NewLine + "Source Server Version: " + sourceServerVersion +
                Environment.NewLine + "Destination Server Version: " + destinationServerVersion +
                Environment.NewLine + "Click OK to continue or Cancel to return." +
                Environment.NewLine +
                          "--------------------------------------------------------------------------------" +
                              Environment.NewLine +
                          "OK - Continue to deploy resources." + Environment.NewLine +
                          "Cancel - Cancel the deploy.";

            //------------Assert Results-------------------------
            Assert.IsTrue(popupWasCalled);
            Assert.AreEqual(MessageBoxButton.OKCancel, buttons);
            Assert.AreEqual("Deploy Version Conflicts", header);
            Assert.AreEqual(message, description);
            Assert.IsTrue(popupController.IsInfo);
            Assert.IsFalse(popupController.IsDependenciesButtonVisible);
            Assert.IsFalse(popupController.IsError);
            Assert.IsFalse(popupController.IsQuestion);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("PopupController_ShowRollbackVersionMessage")]
        public void PopupController_ShowRollbackVersionMessage_SetProperties_AllPropertiesDisplayed()
        {
            //------------Setup for test--------------------------
            var popupWasCalled = false;
            string description = string.Empty;
            string header = string.Empty;
            MessageBoxButton buttons = MessageBoxButton.YesNo;

            var popupController = new PopupController
            {
                ShowDev2MessageBox = (desc, hdr, btn, img, dntShwAgKy, isDependBtnVisible, isErr, isInf, isQuest, duplicates, isDeleteAnywayBtnVisible, applyToAll) =>
                {
                    description = desc;
                    header = hdr;
                    buttons = btn;
                    popupWasCalled = true;
                    return new MessageBoxViewModel(desc, hdr, btn, FontAwesomeIcon.Adn, isDependBtnVisible, isErr, isInf, isQuest, duplicates, isDeleteAnywayBtnVisible, applyToAll)
                    {
                        Result = MessageBoxResult.OK
                    };
                }
            };

            //------------Execute Test---------------------------
            string displayName = "DisplayName";
            popupController.ShowRollbackVersionMessage(displayName);

            var message = $"{displayName} will become the current version.{Environment.NewLine}Do you want to proceed ?";

            //------------Assert Results-------------------------
            Assert.IsTrue(popupWasCalled);
            Assert.AreEqual(MessageBoxButton.YesNo, buttons);
            Assert.AreEqual("Make current version", header);
            Assert.AreEqual(message, description);
            Assert.IsTrue(popupController.IsInfo);
            Assert.IsFalse(popupController.IsDependenciesButtonVisible);
            Assert.IsFalse(popupController.IsError);
            Assert.IsFalse(popupController.IsQuestion);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("PopupController_GetDeleteConfirmation")]
        public void PopupController_GetDeleteConfirmation_SetProperties_AllPropertiesDisplayed()
        {
            //------------Setup for test--------------------------
            var popupController = new PopupController();

            //------------Execute Test---------------------------
            string displayName = "DisplayName";
            var popup = popupController.GetDeleteConfirmation(displayName);

            var message = string.Format(Warewolf.Studio.Resources.Languages.Core.DeleteConfirmation, displayName);

            //------------Assert Results-------------------------
            Assert.AreEqual(MessageBoxButton.YesNo, popup.Buttons);
            Assert.AreEqual(Warewolf.Studio.Resources.Languages.Core.GenericConfirmation, popup.Header);
            Assert.AreEqual(message, popup.Description);
            Assert.IsTrue(popup.IsInfo);
            Assert.IsFalse(popup.IsDependenciesButtonVisible);
            Assert.IsFalse(popup.IsError);
            Assert.IsFalse(popup.IsQuestion);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("PopupController_GetDuplicateMessage")]
        public void PopupController_GetDuplicateMessage_SetProperties_AllPropertiesDisplayed()
        {
            //------------Setup for test--------------------------
            var popupController = new PopupController();

            //------------Execute Test---------------------------
            string displayName = "DisplayName";
            var popup = popupController.GetDuplicateMessage(displayName);

            var message = $"The name {displayName} already exists. Please choose a different name.";

            //------------Assert Results-------------------------
            Assert.AreEqual(MessageBoxButton.OK, popup.Buttons);
            Assert.AreEqual(Warewolf.Studio.Resources.Languages.Core.InvalidPermissionHeader, popup.Header);
            Assert.AreEqual(message, popup.Description);
            Assert.IsFalse(popup.IsInfo);
            Assert.IsFalse(popup.IsDependenciesButtonVisible);
            Assert.IsFalse(popup.IsError);
            Assert.IsFalse(popup.IsQuestion);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("PopupController_GetDuplicateResources")]
        public void PopupController_GetDuplicateResources_SetProperties_AllPropertiesDisplayed()
        {
            //------------Setup for test--------------------------
            var popupWasCalled = false;
            string description = string.Empty;
            var duplicateResources = new List<string>();
            string header = string.Empty;
            MessageBoxButton buttons = MessageBoxButton.OK;

            var popupController = new PopupController
            {
                ShowDev2MessageBox = (desc, hdr, btn, img, dntShwAgKy, isDependBtnVisible, isErr, isInf, isQuest, duplicates, isDeleteAnywayBtnVisible, applyToAll) =>
                {
                    description = desc;
                    duplicateResources = duplicates;
                    header = hdr;
                    buttons = btn;
                    popupWasCalled = true;
                    return new MessageBoxViewModel(desc, hdr, btn, FontAwesomeIcon.Adn, isDependBtnVisible, isErr, isInf, isQuest, duplicates, isDeleteAnywayBtnVisible, applyToAll)
                    {
                        Result = MessageBoxResult.OK
                    };
                }
            };

            //------------Execute Test---------------------------
            List<string> duplicateList = new List<string> {"test1", "test2"};
            popupController.ShowResourcesConflict(duplicateList);

            var message = "Duplicate resources found. Please resolve the files on File Explorer. \nTo view the resource, click on the individual items below.";

            //------------Assert Results-------------------------
            Assert.IsTrue(popupWasCalled);
            Assert.AreEqual(MessageBoxButton.OK, buttons);
            Assert.AreEqual("Duplicated Resources", header);
            Assert.AreEqual(message, description);
            var value = duplicateList.First();
            Assert.IsTrue(duplicateResources.Contains(value));
            value = duplicateList.Last();
            Assert.IsTrue(duplicateResources.Contains(value));
            Assert.IsFalse(popupController.IsInfo);
            Assert.IsFalse(popupController.IsDependenciesButtonVisible);
            Assert.IsTrue(popupController.IsError);
            Assert.IsFalse(popupController.IsQuestion);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("PopupController_ShowServerNotConnected")]
        public void PopupController_ShowServerNotConnected_SetProperties_AllPropertiesDisplayed()
        {
            //------------Setup for test--------------------------
            var popupWasCalled = false;
            string description = string.Empty;
            string header = string.Empty;
            MessageBoxButton buttons = MessageBoxButton.OK;

            var popupController = new PopupController
            {
                ShowDev2MessageBox = (desc, hdr, btn, img, dntShwAgKy, isDependBtnVisible, isErr, isInf, isQuest, duplicates, isDeleteAnywayBtnVisible, applyToAll) =>
                {
                    description = desc;
                    header = hdr;
                    buttons = btn;
                    popupWasCalled = true;
                    return new MessageBoxViewModel(desc, hdr, btn, FontAwesomeIcon.Adn, isDependBtnVisible, isErr, isInf, isQuest, duplicates, isDeleteAnywayBtnVisible, applyToAll)
                    {
                        Result = MessageBoxResult.OK
                    };
                }
            };

            //------------Execute Test---------------------------
            string server = "localhost";
            popupController.ShowServerNotConnected(server);

            var message = "The server " + server + " is unreachable. \n \nPlease make sure the Warewolf Server service is running on that machine.";

            //------------Assert Results-------------------------
            Assert.IsTrue(popupWasCalled);
            Assert.AreEqual(MessageBoxButton.OK, buttons);
            Assert.AreEqual("Server Unreachable", header);
            Assert.AreEqual(message, description);
            Assert.IsFalse(popupController.IsInfo);
            Assert.IsFalse(popupController.IsDependenciesButtonVisible);
            Assert.IsTrue(popupController.IsError);
            Assert.IsFalse(popupController.IsQuestion);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("PopupController_ShowConnectServerVersionConflict")]
        public void PopupController_ShowConnectServerVersionConflict_SetProperties_AllPropertiesDisplayed()
        {
            //------------Setup for test--------------------------
            var popupWasCalled = false;
            string description = string.Empty;
            string header = string.Empty;
            MessageBoxButton buttons = MessageBoxButton.OK;

            var popupController = new PopupController
            {
                ShowDev2MessageBox = (desc, hdr, btn, img, dntShwAgKy, isDependBtnVisible, isErr, isInf, isQuest, duplicates, isDeleteAnywayBtnVisible, applyToAll) =>
                {
                    description = desc;
                    header = hdr;
                    buttons = btn;
                    popupWasCalled = true;
                    return new MessageBoxViewModel(desc, hdr, btn, FontAwesomeIcon.Adn, isDependBtnVisible, isErr, isInf, isQuest, duplicates, isDeleteAnywayBtnVisible, applyToAll)
                    {
                        Result = MessageBoxResult.OK
                    };
                }
            };

            //------------Execute Test---------------------------
            string selectedServerVersion = "0.0.0.9";
            string currentServerVersion = "1.0.0.0";
            popupController.ShowConnectServerVersionConflict(selectedServerVersion, currentServerVersion);

            string message = "There is a version conflict with the current selected server." + Environment.NewLine +
                Environment.NewLine + "Selected Server Version: " + selectedServerVersion +
                Environment.NewLine + "Current Server Version: " + currentServerVersion + Environment.NewLine +
                Environment.NewLine + "Please make sure that the server you are trying to connect to has the latest version.";

            //------------Assert Results-------------------------
            Assert.IsTrue(popupWasCalled);
            Assert.AreEqual(MessageBoxButton.OK, buttons);
            Assert.AreEqual("Server Version Conflict", header);
            Assert.AreEqual(message, description);
            Assert.IsFalse(popupController.IsInfo);
            Assert.IsFalse(popupController.IsDependenciesButtonVisible);
            Assert.IsTrue(popupController.IsError);
            Assert.IsFalse(popupController.IsQuestion);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("PopupController_ShowConnectionTimeoutConfirmation")]
        public void PopupController_ShowConnectionTimeoutConfirmation_SetProperties_AllPropertiesDisplayed()
        {
            //------------Setup for test--------------------------
            var popupWasCalled = false;
            string description = string.Empty;
            string header = string.Empty;
            MessageBoxButton buttons = MessageBoxButton.YesNo;

            var popupController = new PopupController
            {
                ShowDev2MessageBox = (desc, hdr, btn, img, dntShwAgKy, isDependBtnVisible, isErr, isInf, isQuest, duplicates, isDeleteAnywayBtnVisible, applyToAll) =>
                {
                    description = desc;
                    header = hdr;
                    buttons = btn;
                    popupWasCalled = true;
                    return new MessageBoxViewModel(desc, hdr, btn, FontAwesomeIcon.Adn, isDependBtnVisible, isErr, isInf, isQuest, duplicates, isDeleteAnywayBtnVisible, applyToAll)
                    {
                        Result = MessageBoxResult.OK
                    };
                }
            };

            //------------Execute Test---------------------------
            string server = "localhost";
            popupController.ShowConnectionTimeoutConfirmation(server);

            var message = " Unable to reach " + server + ": Connection timed out." + Environment.NewLine
                              + " Make sure the remote computer is powered on." + Environment.NewLine
                              + Environment.NewLine
                              + " Would you like to re-try? " + Environment.NewLine;

            //------------Assert Results-------------------------
            Assert.IsTrue(popupWasCalled);
            Assert.AreEqual(MessageBoxButton.YesNo, buttons);
            Assert.AreEqual("Server Is Unreachable", header);
            Assert.AreEqual(message, description);
            Assert.IsFalse(popupController.IsInfo);
            Assert.IsFalse(popupController.IsDependenciesButtonVisible);
            Assert.IsTrue(popupController.IsError);
            Assert.IsFalse(popupController.IsQuestion);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("PopupController_ShowPopupMessage")]
        public void PopupController_ShowPopupMessage_SetProperties_AllPropertiesDisplayed()
        {
            //------------Setup for test--------------------------
            var popupWasCalled = false;
            string description = string.Empty;
            string header = string.Empty;
            MessageBoxButton buttons = MessageBoxButton.OK;

            var popupController = new PopupController
            {
                ShowDev2MessageBox = (desc, hdr, btn, img, dntShwAgKy, isDependBtnVisible, isErr, isInf, isQuest, duplicates, isDeleteAnywayBtnVisible, applyToAll) =>
                {
                    description = desc;
                    header = hdr;
                    buttons = btn;
                    popupWasCalled = true;
                    return new MessageBoxViewModel(desc, hdr, btn, FontAwesomeIcon.Adn, isDependBtnVisible, isErr, isInf, isQuest, duplicates, isDeleteAnywayBtnVisible, applyToAll)
                    {
                        Result = MessageBoxResult.OK
                    };
                }
            };

            var popupMessage = new PopupMessage
            {
                Description = "This is a test error message",
                Image = MessageBoxImage.Error,
                Buttons = MessageBoxButton.OK,
                IsError = true,
                Header = @"Error"

            };

            //------------Execute Test---------------------------
            popupController.Show(popupMessage);

            //------------Assert Results-------------------------
            Assert.IsTrue(popupWasCalled);
            Assert.AreEqual(MessageBoxButton.OK, buttons);
            Assert.AreEqual("Error", header);
            Assert.AreEqual("This is a test error message", description);
            Assert.IsFalse(popupController.IsInfo);
            Assert.IsFalse(popupController.IsDependenciesButtonVisible);
            Assert.IsTrue(popupController.IsError);
            Assert.IsFalse(popupController.IsQuestion);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("PopupController_ShowCorruptTaskResult")]
        public void PopupController_ShowCorruptTaskResult_SetProperties_AllPropertiesDisplayed()
        {
            //------------Setup for test--------------------------
            var popupWasCalled = false;
            string description = string.Empty;
            string header = string.Empty;
            string errorMessage = string.Empty;

            string expectedDescription = "Unable to retrieve tasks." + Environment.NewLine +
                                         "ERROR: " + errorMessage + ". " + Environment.NewLine +
                                         "Please check that there a no corrupt files." + Environment.NewLine +
                                        @"C:\Windows\System32\Tasks\Warewolf";

            MessageBoxButton buttons = MessageBoxButton.OK;

            var popupController = new PopupController
            {
                ShowDev2MessageBox = (desc, hdr, btn, img, dntShwAgKy, isDependBtnVisible, isErr, isInf, isQuest, duplicates, isDeleteAnywayBtnVisible, applyToAll) =>
                {
                    description = desc;
                    header = hdr;
                    buttons = btn;
                    popupWasCalled = true;
                    return new MessageBoxViewModel(desc, hdr, btn, FontAwesomeIcon.Adn, isDependBtnVisible, isErr, isInf, isQuest, duplicates, isDeleteAnywayBtnVisible, applyToAll)
                    {
                        Result = MessageBoxResult.OK
                    };
                }
            };

            //------------Execute Test---------------------------
            popupController.ShowCorruptTaskResult(errorMessage);
            //------------Assert Results-------------------------
            Assert.IsTrue(popupWasCalled);
            Assert.AreEqual(MessageBoxButton.OK, buttons);
            Assert.AreEqual("Scheduler Load Error", header);
            Assert.AreEqual(expectedDescription, description);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("PopupController_ShowExceptionViewAppreciation")]
        public void PopupController_ShowExceptionViewAppreciation_SetProperties_AllPropertiesDisplayed()
        {
            //------------Setup for test--------------------------
            var popupWasCalled = false;
            string description = string.Empty;
            string header = string.Empty;

            string expectedDescription = "Thank you for taking the time to log it. Follow the issue " + Environment.NewLine +
                "in the Community to keep updated on the progress.";

            MessageBoxButton buttons = MessageBoxButton.OK;

            var popupController = new PopupController
            {
                ShowDev2MessageBox = (desc, hdr, btn, img, dntShwAgKy, isDependBtnVisible, isErr, isInf, isQuest, duplicates, isDeleteAnywayBtnVisible, applyToAll) =>
                {
                    description = desc;
                    header = hdr;
                    buttons = btn;
                    popupWasCalled = true;
                    return new MessageBoxViewModel(desc, hdr, btn, FontAwesomeIcon.Adn, isDependBtnVisible, isErr, isInf, isQuest, duplicates, isDeleteAnywayBtnVisible, applyToAll)
                    {
                        Result = MessageBoxResult.OK
                    };
                }
            };

            //------------Execute Test---------------------------
            popupController.ShowExceptionViewAppreciation();
            //------------Assert Results-------------------------
            Assert.IsTrue(popupWasCalled);
            Assert.AreEqual(MessageBoxButton.OK, buttons);
            Assert.AreEqual("We’ve got your feedback!", header);
            Assert.AreEqual(expectedDescription, description);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("PopupController_ShowNameChangedConflict")]
        public void PopupController_ShowNameChangedConflict_SetProperties_AllPropertiesDisplayed()
        {
            //------------Setup for test--------------------------
            var popupWasCalled = false;
            string description = string.Empty;
            string header = string.Empty;
            string oldName = string.Empty;
            string newName = string.Empty;
            MessageBoxButton buttons = MessageBoxButton.YesNoCancel;
            string expectedDescription = "The following task has been renamed " + oldName + " -> " + newName + ". You will lose the history for the old task." + Environment.NewLine +
                          " Would you like to save the new name?" + Environment.NewLine +
                          "-----------------------------------------------------------------" +
                              Environment.NewLine +
                          "Yes - Save with the new name." + Environment.NewLine +
                          "No - Save with the old name." + Environment.NewLine +
                          "Cancel - Returns you to Scheduler.";
            MessageBoxImage imageType = MessageBoxImage.Error;


            var popupController = new PopupController
            {
                ShowDev2MessageBox = (desc, hdr, btn, img, dntShwAgKy, isDependBtnVisible, isErr, isInf, isQuest, duplicates, isDeleteAnywayBtnVisible, applyToAll) =>
                {
                    description = desc;
                    header = hdr;
                    buttons = btn;
                    imageType = img;
                    popupWasCalled = true;
                    return new MessageBoxViewModel(desc, hdr, btn, FontAwesomeIcon.Adn, isDependBtnVisible, isErr, isInf, isQuest, duplicates, isDeleteAnywayBtnVisible, applyToAll)
                    {
                        Result = MessageBoxResult.OK
                    };
                }
            };


            //------------Execute Test---------------------------
            popupController.ShowNameChangedConflict(oldName, newName);
            //------------Assert Results-------------------------
            Assert.IsTrue(popupWasCalled);
            Assert.AreEqual(MessageBoxButton.YesNoCancel, buttons);
            Assert.AreEqual("Rename Conflicts", header);
            Assert.AreEqual(expectedDescription, description);
            Assert.AreEqual(MessageBoxImage.Information, imageType);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("PopupController_ShowNotConnected")]
        public void PopupController_ShowNotConnected_SetProperties_AllPropertiesDisplayed()
        {
            //------------Setup for test--------------------------
            var popupWasCalled = false;
            string description = string.Empty;
            string header = string.Empty;
            MessageBoxButton buttons = MessageBoxButton.YesNoCancel;
            MessageBoxImage imageType = MessageBoxImage.Error;

            var popupController = new PopupController
            {
                ShowDev2MessageBox = (desc, hdr, btn, img, dntShwAgKy, isDependBtnVisible, isErr, isInf, isQuest, duplicates, isDeleteAnywayBtnVisible, applyToAll) =>
                {
                    description = desc;
                    header = hdr;
                    buttons = btn;
                    imageType = img;
                    popupWasCalled = true;
                    return new MessageBoxViewModel(desc, hdr, btn, FontAwesomeIcon.Adn, isDependBtnVisible, isErr, isInf, isQuest, duplicates, isDeleteAnywayBtnVisible, applyToAll)
                    {
                        Result = MessageBoxResult.OK
                    };
                }
            };


            //------------Execute Test---------------------------
            popupController.ShowNotConnected();
            //------------Assert Results-------------------------
            Assert.IsTrue(popupWasCalled);
            Assert.AreEqual(MessageBoxButton.OK, buttons);
            Assert.AreEqual("Server Unreachable", header);
            Assert.AreEqual("You can not change the settings for a server that is unreachable.", description);
            Assert.AreEqual(MessageBoxImage.Error, imageType);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("PopupController_ShowSaveErrorDialog")]
        public void PopupController_ShowSaveErrorDialog_SetProperties_AllPropertiesDisplayed()
        {
            //------------Setup for test--------------------------
            var popupWasCalled = false;
            string description = string.Empty;
            string header = string.Empty;
            MessageBoxButton buttons = MessageBoxButton.YesNoCancel;
            MessageBoxImage imageType = MessageBoxImage.Error;
            string errorMessage = string.Empty;

            var popupController = new PopupController
            {
                ShowDev2MessageBox = (desc, hdr, btn, img, dntShwAgKy, isDependBtnVisible, isErr, isInf, isQuest, duplicates, isDeleteAnywayBtnVisible, applyToAll) =>
                {
                    description = desc;
                    header = hdr;
                    buttons = btn;
                    imageType = img;
                    popupWasCalled = true;
                    return new MessageBoxViewModel(desc, hdr, btn, FontAwesomeIcon.Adn, isDependBtnVisible, isErr, isInf, isQuest, duplicates, isDeleteAnywayBtnVisible, applyToAll)
                    {
                        Result = MessageBoxResult.OK
                    };
                }
            };

            //------------Execute Test---------------------------
            popupController.ShowSaveErrorDialog(errorMessage);
            //------------Assert Results-------------------------
            Assert.IsTrue(popupWasCalled);
            Assert.AreEqual(MessageBoxButton.OK, buttons);
            Assert.AreEqual("Saving Error", header);
            Assert.AreEqual("The following error occurred on save:" + Environment.NewLine + errorMessage, description);
            Assert.AreEqual(MessageBoxImage.Error, imageType);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("PopupController_ShowSchedulerCloseConfirmation")]
        public void PopupController_ShowSchedulerCloseConfirmation_SetProperties_AllPropertiesDisplayed()
        {
            //------------Setup for test--------------------------
            var popupWasCalled = false;
            string description = string.Empty;
            string header = string.Empty;
            MessageBoxButton buttons = MessageBoxButton.YesNoCancel;
            MessageBoxImage imageType = MessageBoxImage.Error;
            var expectedDesc = "Scheduler Task has not been saved." + Environment.NewLine
                              + "Would you like to save the Task? " + Environment.NewLine +
                              "-----------------------------------------------------------------" +
                              Environment.NewLine +
                              "Yes - Save the Task." + Environment.NewLine +
                              "No - Discard your changes." + Environment.NewLine +
                              "Cancel - Returns you to Scheduler.";

            var popupController = new PopupController
            {
                ShowDev2MessageBox = (desc, hdr, btn, img, dntShwAgKy, isDependBtnVisible, isErr, isInf, isQuest, duplicates, isDeleteAnywayBtnVisible, applyToAll) =>
                {
                    description = desc;
                    header = hdr;
                    buttons = btn;
                    imageType = img;
                    popupWasCalled = true;
                    return new MessageBoxViewModel(desc, hdr, btn, FontAwesomeIcon.Adn, isDependBtnVisible, isErr, isInf, isQuest, duplicates, isDeleteAnywayBtnVisible, applyToAll)
                    {
                        Result = MessageBoxResult.OK
                    };
                }
            };

            //------------Execute Test---------------------------
            popupController.ShowSchedulerCloseConfirmation();
            //------------Assert Results-------------------------
            Assert.IsTrue(popupWasCalled);
            Assert.AreEqual(MessageBoxButton.YesNoCancel, buttons);
            Assert.AreEqual("Scheduler Task Has Changes", header);
            Assert.AreEqual(expectedDesc, description);
            Assert.AreEqual(MessageBoxImage.Information, imageType);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("PopupController_ShowSettingsCloseConfirmation")]
        public void PopupController_ShowSettingsCloseConfirmation_SetProperties_AllPropertiesDisplayed()
        {
            //------------Setup for test--------------------------
            var popupWasCalled = false;
            string description = string.Empty;
            string header = string.Empty;
            MessageBoxButton buttons = MessageBoxButton.YesNoCancel;
            MessageBoxImage imageType = MessageBoxImage.Error;
            var expectedDesc = "Settings have not been saved." + Environment.NewLine
                              + "Would you like to save the settings? " + Environment.NewLine +
                              "-----------------------------------------------------------------" +
                              Environment.NewLine +
                              "Yes - Save the settings." + Environment.NewLine +
                              "No - Discard your changes." + Environment.NewLine +
                              "Cancel - Returns you to settings.";

            var popupController = new PopupController
            {
                ShowDev2MessageBox = (desc, hdr, btn, img, dntShwAgKy, isDependBtnVisible, isErr, isInf, isQuest, duplicates, isDeleteAnywayBtnVisible, applyToAll) =>
                {
                    description = desc;
                    header = hdr;
                    buttons = btn;
                    imageType = img;
                    popupWasCalled = true;
                    return new MessageBoxViewModel(desc, hdr, btn, FontAwesomeIcon.Adn, isDependBtnVisible, isErr, isInf, isQuest, duplicates, isDeleteAnywayBtnVisible, applyToAll)
                    {
                        Result = MessageBoxResult.OK
                    };
                }
            };

            //------------Execute Test---------------------------
            popupController.ShowSettingsCloseConfirmation();
            //------------Assert Results-------------------------
            Assert.IsTrue(popupWasCalled);
            Assert.AreEqual(MessageBoxButton.YesNoCancel, buttons);
            Assert.AreEqual("Settings Have Changed", header);
            Assert.AreEqual(expectedDesc, description);
            Assert.AreEqual(MessageBoxImage.Information, imageType);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("PopupController_ShowNoInputsSelectedWhenClickLink")]
        public void PopupController_ShowNoInputsSelectedWhenClickLink_SetProperties_AllPropertiesDisplayed()
        {
            //------------Setup for test--------------------------
            var popupWasCalled = false;
            string description = string.Empty;
            string header = string.Empty;
            string dontShowAgainKey = string.Empty;
            MessageBoxButton buttons = MessageBoxButton.YesNoCancel;
            MessageBoxImage imageType = MessageBoxImage.Error;
            var expectedDesc = "You can pass variables into your workflow" + Environment.NewLine
                              + "by selecting the Input checkbox" + Environment.NewLine +
                              "in the Variables window.";

            var popupController = new PopupController
            {
                ShowDev2MessageBox = (desc, hdr, btn, img, dntShwAgKy, isDependBtnVisible, isErr, isInf, isQuest, duplicates, isDeleteAnywayBtnVisible, applyToAll) =>
                {
                    description = desc;
                    header = hdr;
                    buttons = btn;
                    imageType = img;
                    popupWasCalled = true;
                    dontShowAgainKey = dntShwAgKy;
                    return new MessageBoxViewModel(desc, hdr, btn, FontAwesomeIcon.Adn, isDependBtnVisible, isErr, isInf, isQuest, duplicates, isDeleteAnywayBtnVisible, applyToAll)
                    {
                        Result = MessageBoxResult.OK
                    };
                }
            };

            //------------Execute Test---------------------------
            popupController.ShowNoInputsSelectedWhenClickLink();
            //------------Assert Results-------------------------
            Assert.IsTrue(popupWasCalled);
            Assert.AreEqual(MessageBoxButton.OK, buttons);
            Assert.AreEqual("Did you know?", header);
            Assert.AreEqual(expectedDesc, description);
            Assert.AreEqual(MessageBoxImage.Information, imageType);
            Assert.AreEqual(GlobalConstants.Dev2MessageBoxNoInputsWhenHyperlinkClickedDialog, dontShowAgainKey);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("PopupController_ShowInvalidCharacterMessagek")]
        public void PopupController_ShowInvalidCharacterMessage_SetProperties_AllPropertiesDisplayed()
        {
            //------------Setup for test--------------------------
            var popupWasCalled = false;
            string description = string.Empty;
            string header = string.Empty;
            string dontShowAgainKey = string.Empty;
            MessageBoxButton buttons = MessageBoxButton.OK;
            MessageBoxImage imageType = MessageBoxImage.Error;
            const string expectedDesc = "some invalid text is invalid. Warewolf only supports latin characters";

            var popupController = new PopupController
            {
                ShowDev2MessageBox = (desc, hdr, btn, img, dntShwAgKy, isDependBtnVisible, isErr, isInf, isQuest, duplicates, isDeleteAnywayBtnVisible, applyToAll) =>
                {
                    description = desc;
                    header = hdr;
                    buttons = btn;
                    imageType = img;
                    popupWasCalled = true;
                    dontShowAgainKey = dntShwAgKy;
                    return new MessageBoxViewModel(desc, hdr, btn, FontAwesomeIcon.Adn, isDependBtnVisible, isErr, isInf, isQuest, duplicates, isDeleteAnywayBtnVisible, applyToAll)
                    {
                        Result = MessageBoxResult.OK
                    };
                }
            };

            //------------Execute Test---------------------------
            popupController.ShowInvalidCharacterMessage("some invalid text");
            //------------Assert Results-------------------------
            Assert.IsTrue(popupWasCalled);
            Assert.AreEqual(MessageBoxButton.OK, buttons);
            Assert.AreEqual("Invalid text", header);
            Assert.AreEqual(expectedDesc, description);
            Assert.AreEqual(MessageBoxImage.Error, imageType);
            Assert.AreEqual(null, dontShowAgainKey);
        }
    }
}
