
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
using Dev2.Studio.Controller;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            MessageBoxImage imageType = MessageBoxImage.Error;

            var popupController = new PopupController
                {
                    ShowDev2MessageBox = (desc, hdr, btn, img, dntShwAgKy) =>
                        {
                            description = desc;
                            header = hdr;
                            buttons = btn;
                            imageType = img;
                            popupWasCalled = true;
                            return MessageBoxResult.OK;
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
            Assert.AreEqual(MessageBoxImage.Information, imageType);
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
                          "-------------------------------------------------------------------" +
                          "Yes - Save with the new name." + Environment.NewLine +
                          "No - Save with the old name." + Environment.NewLine +
                          "Cancel - Returns you to Scheduler.";
            MessageBoxImage imageType = MessageBoxImage.Error;


            var popupController = new PopupController
            {
                ShowDev2MessageBox = (desc, hdr, btn, img, dntShwAgKy) =>
                {
                    description = desc;
                    header = hdr;
                    buttons = btn;
                    imageType = img;
                    popupWasCalled = true;
                    return MessageBoxResult.OK;
                }
            };


            //------------Execute Test---------------------------
            popupController.ShowNameChangedConflict(oldName, newName);
            //------------Assert Results-------------------------
            Assert.IsTrue(popupWasCalled);
            Assert.AreEqual(MessageBoxButton.YesNoCancel, buttons);
            Assert.AreEqual("Rename conflict", header);
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
                ShowDev2MessageBox = (desc, hdr, btn, img, dntShwAgKy) =>
                {
                    description = desc;
                    header = hdr;
                    buttons = btn;
                    imageType = img;
                    popupWasCalled = true;
                    return MessageBoxResult.OK;
                }
            };


            //------------Execute Test---------------------------
            popupController.ShowNotConnected();
            //------------Assert Results-------------------------
            Assert.IsTrue(popupWasCalled);
            Assert.AreEqual(MessageBoxButton.OK, buttons);
            Assert.AreEqual("Server is not connected", header);
            Assert.AreEqual("You can not change the settings for a server that is offline.", description);
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
                ShowDev2MessageBox = (desc, hdr, btn, img, dntShwAgKy) =>
                {
                    description = desc;
                    header = hdr;
                    buttons = btn;
                    imageType = img;
                    popupWasCalled = true;
                    return MessageBoxResult.OK;
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
                            "-------------------------------------------------------------------" +
                            "Yes - Save the Task." + Environment.NewLine +
                            "No - Discard your changes." + Environment.NewLine +
                            "Cancel - Returns you to Scheduler.";

            var popupController = new PopupController
            {
                ShowDev2MessageBox = (desc, hdr, btn, img, dntShwAgKy) =>
                {
                    description = desc;
                    header = hdr;
                    buttons = btn;
                    imageType = img;
                    popupWasCalled = true;
                    return MessageBoxResult.OK;
                }
            };

            //------------Execute Test---------------------------
            popupController.ShowSchedulerCloseConfirmation();
            //------------Assert Results-------------------------
            Assert.IsTrue(popupWasCalled);
            Assert.AreEqual(MessageBoxButton.YesNoCancel, buttons);
            Assert.AreEqual("Scheduler Task has changes", header);
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
                              "-------------------------------------------------------------------" +
                              "Yes - Save the settings." + Environment.NewLine +
                              "No - Discard your changes." + Environment.NewLine +
                              "Cancel - Returns you to settings.";

            var popupController = new PopupController
            {
                ShowDev2MessageBox = (desc, hdr, btn, img, dntShwAgKy) =>
                {
                    description = desc;
                    header = hdr;
                    buttons = btn;
                    imageType = img;
                    popupWasCalled = true;
                    return MessageBoxResult.OK;
                }
            };

            //------------Execute Test---------------------------
            popupController.ShowSettingsCloseConfirmation();
            //------------Assert Results-------------------------
            Assert.IsTrue(popupWasCalled);
            Assert.AreEqual(MessageBoxButton.YesNoCancel, buttons);
            Assert.AreEqual("Settings have changed", header);
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
                ShowDev2MessageBox = (desc, hdr, btn, img, dntShwAgKy) =>
                {
                    description = desc;
                    header = hdr;
                    buttons = btn;
                    imageType = img;
                    popupWasCalled = true;
                    dontShowAgainKey = dntShwAgKy;
                    return MessageBoxResult.OK;
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
                ShowDev2MessageBox = (desc, hdr, btn, img, dntShwAgKy) =>
                {
                    description = desc;
                    header = hdr;
                    buttons = btn;
                    imageType = img;
                    popupWasCalled = true;
                    dontShowAgainKey = dntShwAgKy;
                    return MessageBoxResult.OK;
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
