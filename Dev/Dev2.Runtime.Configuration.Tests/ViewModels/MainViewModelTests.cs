
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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Xml.Linq;
using Dev2.Runtime.Configuration.ComponentModel;
using Dev2.Runtime.Configuration.Services;
using Dev2.Runtime.Configuration.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Runtime.Configuration.Tests.ViewModels
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class MainViewModelTests
    {
        [TestMethod]
        public void InstantiationWithMalformedDataExpectedErrorInErrorsCollection()
        {
            MainViewModel mainViewModel = new MainViewModel(new XElement("Malformed"), null, null, null);
            Assert.AreEqual(1, mainViewModel.Errors.Count, "When instantiated with malformed xml an error is expected.");
            Assert.AreEqual(Visibility.Visible, mainViewModel.ErrorsVisible, "When there are errors the ErrorsVisible property should have a value of visible.");
        }

        [TestMethod]
        public void InstantiationWithNullDataExpectedErrorInErrorsCollection()
        {
            MainViewModel mainViewModel = new MainViewModel(null, null, null, null);
            Assert.AreEqual(1, mainViewModel.Errors.Count, "When instantiated with null data an error is expected.");
            Assert.AreEqual(Visibility.Visible, mainViewModel.ErrorsVisible, "When there are errors the ErrorsVisible property should have a value of visible.");
        }

        [TestMethod]
        public void SaveCommandExecutedExpectedSaveCallbackInvokedWithCorrectData()
        {
            bool callbackExecuted = false;
            Configuration.Settings.Configuration config = new Configuration.Settings.Configuration("localhost");
            string expected = config.ToXml().ToString();
            string actual = "";

            MainViewModel mainViewModel = new MainViewModel(config.ToXml(), x =>
            {
                actual = x.ToString();
                callbackExecuted = true;
                return config.ToXml();
            }, null, null);

            mainViewModel.SaveCommand.Execute(null);

            Assert.IsTrue(callbackExecuted, "The save callback was not invoked.");
            Assert.AreEqual(expected, actual, "The data coming through in the save callback isn't thedata from the configuration object.");
        }

        [TestMethod]
        public void SaveCommandExecutedWhereSaveCallbackIsNullExpectedNoException()
        {
            Configuration.Settings.Configuration config = new Configuration.Settings.Configuration("localhost");
            MainViewModel mainViewModel = new MainViewModel(config.ToXml(), null, null, null);

            mainViewModel.SaveCommand.Execute(null);
        }

        [TestMethod]
        public void SaveCommandExecutedWhereSaveCallbackThrowsExceptionExpectedErrorInErrorsCollection()
        {
            Configuration.Settings.Configuration config = new Configuration.Settings.Configuration("localhost");

            MainViewModel mainViewModel = new MainViewModel(config.ToXml(), x =>
            {
                throw new Exception();
            }, null, null);

            mainViewModel.SaveCommand.Execute(null);

            Assert.AreEqual(1, mainViewModel.Errors.Count, "When the save callback throws an exception an error is expected.");
            Assert.AreEqual(Visibility.Visible, mainViewModel.ErrorsVisible, "When there are errors the ErrorsVisible property should have a value of visible.");
        }


        [TestMethod]
        public void CancelCommandExecutedExpectedCancelCallbackInvoked()
        {
            bool callbackExecuted = false;
            Configuration.Settings.Configuration config = new Configuration.Settings.Configuration("localhost");

            MainViewModel mainViewModel = new MainViewModel(config.ToXml(), null, () =>
            {
                callbackExecuted = true;
            }, null);

            mainViewModel.CancelCommand.Execute(null);

            Assert.IsTrue(callbackExecuted, "The cancel callback was not invoked.");
        }

        [TestMethod]
        public void CancelCommandExecutedWhereCancelCallbackIsNullExpectedNoException()
        {
            Configuration.Settings.Configuration config = new Configuration.Settings.Configuration("localhost");
            MainViewModel mainViewModel = new MainViewModel(config.ToXml(), null, null, null);

            mainViewModel.SaveCommand.Execute(null);
        }

        [TestMethod]
        public void CancelCommandExecutedWhereCancelCallbackThrowsExceptionExpectedErrorInErrorsCollection()
        {
            Configuration.Settings.Configuration config = new Configuration.Settings.Configuration("localhost");

            MainViewModel mainViewModel = new MainViewModel(config.ToXml(), null, () =>
            {
                throw new Exception();
            }, null);

            mainViewModel.CancelCommand.Execute(null);

            Assert.AreEqual(1, mainViewModel.Errors.Count, "When the cancel callback throws an exception an error is expected.");
            Assert.AreEqual(Visibility.Visible, mainViewModel.ErrorsVisible, "When there are errors the ErrorsVisible property should have a value of visible.");
        }

        [TestMethod]
        public void SetSelectedSettingsObjectsExpectedSettingsViewModelActivated()
        {
            Configuration.Settings.Configuration config = new Configuration.Settings.Configuration("localhost");
            MainViewModel mainViewModel = new MainViewModel(config.ToXml(), null, null, null);

            var commService = new Mock<ICommunicationService>();

            commService.Setup(s => s.GetResources(It.IsAny<string>())).Returns(new List<WorkflowDescriptor>());
            commService.Setup(s => s.GetDataListInputs(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new List<DataListVariable> { new DataListVariable { Name = "TestInput" } });

            mainViewModel.CommunicationService = commService.Object;

            mainViewModel.SelectedSettingsObjects = mainViewModel.SettingsObjects[0];

            Assert.IsNotNull(mainViewModel.ActiveItem, "When a settings object is selected the viewmodel isnt activated");
        }

        [TestMethod]
        public void ClearErrorsCommandWhenThereAreErrorsExpectedErrorsCleared()
        {
            Configuration.Settings.Configuration config = new Configuration.Settings.Configuration("localhost");

            MainViewModel mainViewModel = new MainViewModel(config.ToXml(), null, () =>
            {
                throw new Exception();
            }, null);

            mainViewModel.CancelCommand.Execute(null);

            Assert.AreEqual(1, mainViewModel.Errors.Count, "When the cancel callback throws an exception an error is expected.");

            mainViewModel.ClearErrorsCommand.Execute(null);

            Assert.AreEqual(0, mainViewModel.Errors.Count, "After clear errors is called there shouldn't be any errors.");
            Assert.AreEqual(Visibility.Collapsed, mainViewModel.ErrorsVisible, "When there are no errors the ErrorsVisible property should have a value of collapsed.");
        }
    }
}
