
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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Caliburn.Micro;
using CubicOrange.Windows.Forms.ActiveDirectory;
using Dev2.Common.Interfaces.Scheduler.Interfaces;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Communication;
using Dev2.CustomControls.Connections;
using Dev2.Dialogs;
using Dev2.Scheduler;
using Dev2.Services.Security;
using Dev2.Settings.Scheduler;
using Dev2.Studio.Controller;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.AppResources.Repositories;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.TaskScheduler.Wrappers;
using Dev2.Threading;
using Dev2.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32.TaskScheduler;
using Moq;

// ReSharper disable InconsistentNaming
namespace Dev2.Core.Tests.Settings
{
    [TestClass]
    [Ignore] //TODO: Fix so not dependant on resource file or localize resource file to test project
    public class SchedulerViewModelTests
    {

        [TestInitialize]
        public void SetupForTest()
        {
            AppSettings.LocalHost = "http://localhost:3142";
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SchedulerViewModel_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SchedulerViewModel_Constructor_NullEventAggregator_Exception()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            // ReSharper disable ObjectCreationAsStatement
            new SchedulerViewModel(null, new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IPopupController>().Object, new TestAsyncWorker(), new Mock<IConnectControlViewModel>().Object);
            // ReSharper restore ObjectCreationAsStatement
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SchedulerViewModel_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SchedulerViewModel_Constructor_NullDirectoryObjectPickerDialog_Exception()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            // ReSharper disable ObjectCreationAsStatement
            new SchedulerViewModel(new Mock<IEventAggregator>().Object, null, new Mock<IPopupController>().Object, new TestAsyncWorker(), new Mock<IConnectControlViewModel>().Object);
            // ReSharper restore ObjectCreationAsStatement
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SchedulerViewModel_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SchedulerViewModel_Constructor_NullPopupController_Exception()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            // ReSharper disable ObjectCreationAsStatement
            new SchedulerViewModel(new Mock<IEventAggregator>().Object, new Mock<DirectoryObjectPickerDialog>().Object, null, new TestAsyncWorker(), new Mock<IConnectControlViewModel>().Object);
            // ReSharper restore ObjectCreationAsStatement
            //------------Assert Results-------------------------
        }
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SchedulerViewModel_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SchedulerViewModel_Constructor_NullAsyncWorker_Exception()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            // ReSharper disable ObjectCreationAsStatement
            new SchedulerViewModel(new Mock<IEventAggregator>().Object, new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IPopupController>().Object, null, new Mock<IConnectControlViewModel>().Object);
            // ReSharper restore ObjectCreationAsStatement
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SchedulerViewModel_Constructor")]
        public void SchedulerViewModel_Constructor_ValidConstruction_ShouldSetProperties()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var schdulerViewModel = new SchedulerViewModel();
            //------------Assert Results-------------------------
            Assert.IsNotNull(schdulerViewModel);
            Assert.IsNotNull(schdulerViewModel.Errors);
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("SchedulerViewModel_Constructor")]
        public void SchedulerViewModel_Constructor_ValidConstruction_ShouldSetHelpText()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var schedulerViewModel = new SchedulerViewModel(new Mock<IEventAggregator>().Object, new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IPopupController>().Object, new TestAsyncWorker(), new Mock<IConnectControlViewModel>().Object);

            //------------Assert Results-------------------------
            Assert.AreEqual(@"To schedule a workflow execution, setup the trigger you want to use  and the workflow you want to execute.
Warewolf leverages Windows Task Scheduler and the schedules can be viewed there as well.", schedulerViewModel.HelpText);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("SchedulerViewModel_ShowError")]
        public void SchedulerViewModel_ShowError_WithSaveError_HasErrorsTrue()
        {
            //------------Setup for test--------------------------
            var schedulerViewModel = new SchedulerViewModel(new Mock<IEventAggregator>().Object, new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IPopupController>().Object, new TestAsyncWorker(), new Mock<IConnectControlViewModel>().Object) { SelectedTask = new ScheduledResourceForTest() };

            //------------Execute Test---------------------------
            schedulerViewModel.ShowError("Error while saving: test error");
            //------------Assert Results-------------------------
            Assert.IsTrue(schedulerViewModel.HasErrors);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("SchedulerViewModel_ShowError")]
        public void SchedulerViewModel_ShowError_WithNormalError_HasErrorsTrue()
        {
            //------------Setup for test--------------------------
            var schedulerViewModel = new SchedulerViewModel(new Mock<IEventAggregator>().Object, new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IPopupController>().Object, new TestAsyncWorker(), new Mock<IConnectControlViewModel>().Object) { SelectedTask = new ScheduledResourceForTest() };
            var _hasErrorChange = false;
            var _errorChange = false;

            schedulerViewModel.PropertyChanged += delegate(object sender, PropertyChangedEventArgs args)
            {
                switch(args.PropertyName)
                {
                    case "HasErrors":
                        _hasErrorChange = true;
                        break;
                    case "Error":
                        _errorChange = true;
                        break;
                }
            };

            //------------Execute Test---------------------------
            schedulerViewModel.ShowError("test error");
            //------------Assert Results-------------------------
            Assert.IsTrue(_hasErrorChange);
            Assert.IsTrue(_errorChange);
            Assert.IsTrue(schedulerViewModel.HasErrors);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("SchedulerViewModel_ClearError")]
        public void SchedulerViewModel_ClearError_WithNormalError_HasErrorsSet()
        {
            //------------Setup for test--------------------------
            var schedulerViewModel = new SchedulerViewModel(new Mock<IEventAggregator>().Object, new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IPopupController>().Object, new TestAsyncWorker(), new Mock<IConnectControlViewModel>().Object) { SelectedTask = new ScheduledResourceForTest() };
            var _hasErrorChange = false;
            var _errorChange = false;

            schedulerViewModel.ShowError("test error");

            Assert.IsTrue(schedulerViewModel.HasErrors);

            schedulerViewModel.PropertyChanged += delegate(object sender, PropertyChangedEventArgs args)
            {
                switch(args.PropertyName)
                {
                    case "HasErrors":
                        _hasErrorChange = true;
                        break;
                    case "Error":
                        _errorChange = true;
                        break;
                }
            };

            //------------Execute Test---------------------------
            schedulerViewModel.ClearError("test error");
            //------------Assert Results-------------------------
            Assert.IsTrue(_hasErrorChange);
            Assert.IsTrue(_errorChange);
            Assert.IsFalse(schedulerViewModel.HasErrors);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("SchedulerViewModel_Trigger")]
        public void SchedulerViewModel_Trigger_SetTrigger_IsDirtyTrue()
        {
            //------------Setup for test--------------------------
            var schedulerViewModel = new SchedulerViewModel(new Mock<IEventAggregator>().Object, new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IPopupController>().Object, new TestAsyncWorker(), new Mock<IConnectControlViewModel>().Object);
            var scheduledResourceForTest = new ScheduledResourceForTest();
            schedulerViewModel.SelectedTask = scheduledResourceForTest;
            //------------Execute Test---------------------------
            Assert.IsFalse(schedulerViewModel.SelectedTask.IsDirty);
            ScheduleTrigger scheduleTrigger = new ScheduleTrigger(TaskState.Ready, new Dev2DailyTrigger(new TaskServiceConvertorFactory(), new DailyTrigger()), new Dev2TaskService(new TaskServiceConvertorFactory()), new TaskServiceConvertorFactory());
            schedulerViewModel.Trigger = scheduleTrigger;
            //------------Assert Results-------------------------
            Assert.IsTrue(schedulerViewModel.SelectedTask.IsDirty);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("SchedulerViewModel_RunAsapIfScheduleMissed")]
        public void SchedulerViewModel_RunAsapIfScheduleMissed_SetRunAsapIfScheduleMissed_IsDirtyTrue()
        {
            //------------Setup for test--------------------------
            var schedulerViewModel = new SchedulerViewModel(new Mock<IEventAggregator>().Object, new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IPopupController>().Object, new TestAsyncWorker(), new Mock<IConnectControlViewModel>().Object);
            var scheduledResourceForTest = new ScheduledResourceForTest();
            schedulerViewModel.SelectedTask = scheduledResourceForTest;
            //------------Execute Test---------------------------
            Assert.IsFalse(schedulerViewModel.SelectedTask.IsDirty);
            schedulerViewModel.RunAsapIfScheduleMissed = true;
            //------------Assert Results-------------------------
            Assert.IsTrue(schedulerViewModel.SelectedTask.IsDirty);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("SchedulerViewModel_Status")]
        public void SchedulerViewModel_Status_SetStatus_IsDirtyTrue()
        {
            //------------Setup for test--------------------------
            var schedulerViewModel = new SchedulerViewModel(new Mock<IEventAggregator>().Object, new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IPopupController>().Object, new TestAsyncWorker(), new Mock<IConnectControlViewModel>().Object);
            var scheduledResourceForTest = new ScheduledResourceForTest();
            schedulerViewModel.SelectedTask = scheduledResourceForTest;
            //------------Execute Test---------------------------
            Assert.IsFalse(schedulerViewModel.SelectedTask.IsDirty);
            schedulerViewModel.Status = SchedulerStatus.Disabled;
            //------------Assert Results-------------------------
            Assert.IsTrue(schedulerViewModel.SelectedTask.IsDirty);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("SchedulerViewModel_NumberOfRecordsToKeep")]
        public void SchedulerViewModel_NumberOfRecordsToKeep_SetNumberOfRecordsToKeep_IsDirtyTrue()
        {
            //------------Setup for test--------------------------
            var schedulerViewModel = new SchedulerViewModel(new Mock<IEventAggregator>().Object, new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IPopupController>().Object, new TestAsyncWorker(), new Mock<IConnectControlViewModel>().Object);
            var scheduledResourceForTest = new ScheduledResourceForTest();
            schedulerViewModel.SelectedTask = scheduledResourceForTest;
            //------------Execute Test---------------------------
            Assert.IsFalse(schedulerViewModel.SelectedTask.IsDirty);
            schedulerViewModel.NumberOfRecordsToKeep = "10";
            //------------Assert Results-------------------------
            Assert.IsTrue(schedulerViewModel.SelectedTask.IsDirty);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("SchedulerViewModel_NumberOfRecordsToKeep")]
        public void SchedulerViewModel_NumberOfRecordsToKeep_SetNumberOfRecordsToKeepToBlank_ValueIsZero()
        {
            //------------Setup for test--------------------------
            var schedulerViewModel = new SchedulerViewModel(new Mock<IEventAggregator>().Object, new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IPopupController>().Object, new TestAsyncWorker(), new Mock<IConnectControlViewModel>().Object);
            var scheduledResourceForTest = new ScheduledResourceForTest();
            schedulerViewModel.SelectedTask = scheduledResourceForTest;
            //------------Execute Test---------------------------
            schedulerViewModel.NumberOfRecordsToKeep = "";
            //------------Assert Results-------------------------
            Assert.AreEqual("0", schedulerViewModel.NumberOfRecordsToKeep);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("SchedulerViewModel_NumberOfRecordsToKeep")]
        public void SchedulerViewModel_NumberOfRecordsToKeep_SetNumberOfRecordsToKeepToNoNumeric_ValueIsZero()
        {
            //------------Setup for test--------------------------
            var schedulerViewModel = new SchedulerViewModel(new Mock<IEventAggregator>().Object, new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IPopupController>().Object, new TestAsyncWorker(), new Mock<IConnectControlViewModel>().Object);
            var scheduledResourceForTest = new ScheduledResourceForTest();
            schedulerViewModel.SelectedTask = scheduledResourceForTest;
            //------------Execute Test---------------------------
            schedulerViewModel.NumberOfRecordsToKeep = "asdasd";
            //------------Assert Results-------------------------
            Assert.AreEqual("0", schedulerViewModel.NumberOfRecordsToKeep);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("SchedulerViewModel_SelectedHistory")]
        public void SchedulerViewModel_SelectedHistory_SetSelectedHistory_DebugMessageFiredTwice()
        {
            //------------Setup for test--------------------------
            Mock<IEventAggregator> eventAggregator = new Mock<IEventAggregator>();
            eventAggregator.Setup(c => c.Publish(It.IsAny<DebugOutputMessage>())).Verifiable();
            var schedulerViewModel = new SchedulerViewModel(eventAggregator.Object, new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IPopupController>().Object, new TestAsyncWorker(), new Mock<IConnectControlViewModel>().Object);
            var scheduledResourceForTest = new ScheduledResourceForTest();
            schedulerViewModel.SelectedTask = scheduledResourceForTest; //Fires DebugOutMessage for null SelectedHistory
            //------------Execute Test---------------------------
            schedulerViewModel.SelectedHistory = new ResourceHistoryForTest(); //Fires DebugOutMessage for set SelectedHistory
            //------------Assert Results-------------------------
            eventAggregator.Verify(c => c.Publish(It.IsAny<DebugOutputMessage>()), Times.Exactly(2));
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("SchedulerViewModel_AccountName")]
        public void SchedulerViewModel_AccountName_SetAccountName_IsDirty()
        {
            //------------Setup for test--------------------------
            var _accountNameChanged = false;
            Mock<IEventAggregator> eventAggregator = new Mock<IEventAggregator>();
            eventAggregator.Setup(c => c.Publish(It.IsAny<DebugOutputMessage>())).Verifiable();
            var schedulerViewModel = new SchedulerViewModel(eventAggregator.Object, new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IPopupController>().Object, new TestAsyncWorker(), new Mock<IConnectControlViewModel>().Object);
            var scheduledResourceForTest = new ScheduledResourceForTest();
            schedulerViewModel.PropertyChanged += (sender, args) =>
            {
                if(args.PropertyName == "AccountName")
                {
                    _accountNameChanged = true;
                }
            };
            schedulerViewModel.SelectedTask = scheduledResourceForTest;
            schedulerViewModel.ShowError("Error while saving: Logon failure: unknown user name or bad password");
            Assert.AreEqual("Error while saving: Logon failure: unknown user name or bad password", schedulerViewModel.Error);
            //------------Execute Test---------------------------
            schedulerViewModel.AccountName = "someAccountName";
            //------------Assert Results-------------------------
            Assert.AreEqual("", schedulerViewModel.Error);
            Assert.IsTrue(schedulerViewModel.SelectedTask.IsDirty);
            Assert.AreEqual("someAccountName", schedulerViewModel.SelectedTask.UserName);
            Assert.IsTrue(_accountNameChanged);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("SchedulerViewModel_AccountName")]
        public void SchedulerViewModel_AccountName_SetAccountName_SelectedTaskNull_NothingChangedOnTask()
        {
            //------------Setup for test--------------------------
            var _accountNameChanged = false;
            Mock<IEventAggregator> eventAggregator = new Mock<IEventAggregator>();
            eventAggregator.Setup(c => c.Publish(It.IsAny<DebugOutputMessage>())).Verifiable();
            var schedulerViewModel = new SchedulerViewModel(eventAggregator.Object, new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IPopupController>().Object, new TestAsyncWorker(), new Mock<IConnectControlViewModel>().Object);
            var scheduledResourceForTest = new ScheduledResourceForTest();
            schedulerViewModel.SelectedTask = scheduledResourceForTest;
            schedulerViewModel.ShowError("Error while saving: Logon failure: unknown user name or bad password");
            Assert.AreEqual("Error while saving: Logon failure: unknown user name or bad password", schedulerViewModel.Error);
            schedulerViewModel.AccountName = "someAccountName";
            schedulerViewModel.PropertyChanged += (sender, args) =>
            {
                if(args.PropertyName == "AccountName")
                {
                    _accountNameChanged = true;
                }
            };
            //--------------Assert Preconditions------------------
            Assert.AreEqual("", schedulerViewModel.Error);
            var scheduledResource = schedulerViewModel.SelectedTask;
            Assert.IsTrue(scheduledResource.IsDirty);
            Assert.AreEqual("someAccountName", scheduledResource.UserName);
            //------------Execute Test---------------------------
            schedulerViewModel.SelectedTask = null;
            schedulerViewModel.AccountName = "another account name";
            //------------Assert Results-------------------------
            Assert.IsNull(schedulerViewModel.SelectedTask);
            Assert.IsTrue(scheduledResource.IsDirty);
            Assert.AreEqual("someAccountName", scheduledResource.UserName);
            Assert.IsFalse(_accountNameChanged);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("SchedulerViewModel_Password")]
        public void SchedulerViewModel_Password_SetPassword_IsDirty()
        {
            //------------Setup for test--------------------------
            Mock<IEventAggregator> eventAggregator = new Mock<IEventAggregator>();
            eventAggregator.Setup(c => c.Publish(It.IsAny<DebugOutputMessage>())).Verifiable();
            var schedulerViewModel = new SchedulerViewModel(eventAggregator.Object, new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IPopupController>().Object, new TestAsyncWorker(), new Mock<IConnectControlViewModel>().Object);
            var scheduledResourceForTest = new ScheduledResourceForTest();
            schedulerViewModel.SelectedTask = scheduledResourceForTest;
            schedulerViewModel.ShowError("Error while saving: Logon failure: unknown user name or bad password");
            Assert.AreEqual("Error while saving: Logon failure: unknown user name or bad password", schedulerViewModel.Error);
            //------------Execute Test---------------------------
            schedulerViewModel.Password = "somePassword";
            //------------Assert Results-------------------------
            Assert.AreEqual("", schedulerViewModel.Error);
            Assert.IsTrue(schedulerViewModel.SelectedTask.IsDirty);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SchedulerViewModel_SaveCommand")]
        public void SchedulerViewModel_SaveCommand_UserNamePasswordNotSet_CallsGetCredentials()
        {
            //------------Setup for test--------------------------
            var resources = new ObservableCollection<IScheduledResource>();
            var scheduledResourceForTest = new ScheduledResourceForTest { IsDirty = true };
            resources.Add(scheduledResourceForTest);
            var schedulerViewModel = new SchedulerViewModelForTest();
            var env = new Mock<IEnvironmentModel>();
            var auth = new Mock<IAuthorizationService>();
            env.Setup(a => a.IsConnected).Returns(true);
            env.Setup(a => a.AuthorizationService).Returns(auth.Object);
            auth.Setup(a => a.IsAuthorized(AuthorizationContext.Administrator, null)).Returns(true).Verifiable();
            schedulerViewModel.CurrentEnvironment = env.Object;
            var mockScheduledResourceModel = new Mock<IScheduledResourceModel>();
            mockScheduledResourceModel.Setup(model => model.ScheduledResources).Returns(resources);
            mockScheduledResourceModel.Setup(model => model.Save(It.IsAny<IScheduledResource>(), It.IsAny<string>(), It.IsAny<string>())).Verifiable();
            schedulerViewModel.ScheduledResourceModel = mockScheduledResourceModel.Object;
            schedulerViewModel.SelectedTask = schedulerViewModel.TaskList[0];
            //------------Execute Test---------------------------
            schedulerViewModel.SaveCommand.Execute(null);
            //------------Assert Results-------------------------
            Assert.IsTrue(schedulerViewModel.GetCredentialsCalled);
            Assert.IsTrue(schedulerViewModel.ShowSaveErrorDialogCalled);
            string errorMessage;
            mockScheduledResourceModel.Verify(model => model.Save(It.IsAny<IScheduledResource>(), out errorMessage), Times.Once());
            auth.Verify(a => a.IsAuthorized(AuthorizationContext.Administrator, null));
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("SchedulerViewModel_SaveCommand")]
        public void SchedulerViewModel_SaveCommand_WithNewNameDiffToOldNameYesDialogResponse_DialogShowsConflict()
        {
            //------------Setup for test--------------------------
            var resources = new ObservableCollection<IScheduledResource>();
            var scheduledResourceForTest = new ScheduledResource("Task2", SchedulerStatus.Enabled, DateTime.MaxValue, new Mock<IScheduleTrigger>().Object, "TestFlow1") { OldName = "Task1", IsDirty = true };
            scheduledResourceForTest.IsDirty = true;
            resources.Add(scheduledResourceForTest);
            resources.Add(new ScheduledResource("Task3", SchedulerStatus.Enabled, DateTime.MaxValue, new Mock<IScheduleTrigger>().Object, "TestFlow1") { OldName = "Task3", IsDirty = true });
            Mock<IPopupController> mockPopupController = new Mock<IPopupController>();
            mockPopupController.Setup(c => c.ShowNameChangedConflict(It.IsAny<string>(), It.IsAny<string>())).Returns(MessageBoxResult.Yes).Verifiable();
            var schedulerViewModel = new SchedulerViewModelForTest(new Mock<IEventAggregator>().Object, new Mock<DirectoryObjectPickerDialog>().Object, mockPopupController.Object, new TestAsyncWorker());
            var env = new Mock<IEnvironmentModel>();
            var auth = new Mock<IAuthorizationService>();
            env.Setup(a => a.IsConnected).Returns(true);
            env.Setup(a => a.AuthorizationService).Returns(auth.Object);
            auth.Setup(a => a.IsAuthorized(AuthorizationContext.Administrator, null)).Returns(true).Verifiable();
            schedulerViewModel.CurrentEnvironment = env.Object;
            var mockScheduledResourceModel = new Mock<IScheduledResourceModel>();
            mockScheduledResourceModel.Setup(model => model.ScheduledResources).Returns(resources);
            mockScheduledResourceModel.Setup(model => model.Save(It.IsAny<IScheduledResource>(), It.IsAny<string>(), It.IsAny<string>())).Verifiable();
            schedulerViewModel.ScheduledResourceModel = mockScheduledResourceModel.Object;
            schedulerViewModel.SelectedTask = schedulerViewModel.TaskList[0];
            //------------Execute Test---------------------------
            schedulerViewModel.SaveCommand.Execute(null);
            //------------Assert Results-------------------------
            Assert.IsTrue(schedulerViewModel.GetCredentialsCalled);
            string errorMessage;
            mockScheduledResourceModel.Verify(model => model.Save(It.IsAny<IScheduledResource>(), out errorMessage), Times.Once());
            Assert.AreEqual("Task2", schedulerViewModel.TaskList[0].Name);
            mockPopupController.Verify(c => c.ShowNameChangedConflict(It.IsAny<string>(), It.IsAny<string>()), Times.Once());
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("SchedulerViewModel_SaveCommand")]
        public void SchedulerViewModel_SaveCommand_WithNewNameDiffToOldNameYesDialogResponse_OldNameChanges()
        {
            //------------Setup for test--------------------------
            var resources = new ObservableCollection<IScheduledResource>();
            var scheduledResourceForTest = new ScheduledResource("Task2", SchedulerStatus.Enabled, DateTime.MaxValue, new Mock<IScheduleTrigger>().Object, "TestFlow1") { OldName = "Task1", IsDirty = true };
            scheduledResourceForTest.IsDirty = true;
            resources.Add(scheduledResourceForTest);
            resources.Add(new ScheduledResource("Task3", SchedulerStatus.Enabled, DateTime.MaxValue, new Mock<IScheduleTrigger>().Object, "TestFlow1") { OldName = "Task3", IsDirty = true });
            Mock<IPopupController> mockPopupController = new Mock<IPopupController>();
            mockPopupController.Setup(c => c.ShowNameChangedConflict(It.IsAny<string>(), It.IsAny<string>())).Returns(MessageBoxResult.Yes).Verifiable();
            var schedulerViewModel = new SchedulerViewModelForTest(new Mock<IEventAggregator>().Object, new Mock<DirectoryObjectPickerDialog>().Object, mockPopupController.Object, new TestAsyncWorker());
            var env = new Mock<IEnvironmentModel>();
            var auth = new Mock<IAuthorizationService>();
            env.Setup(a => a.AuthorizationService).Returns(auth.Object);
            env.Setup(a => a.IsConnected).Returns(true);
            auth.Setup(a => a.IsAuthorized(AuthorizationContext.Administrator, null)).Returns(true).Verifiable();
            schedulerViewModel.CurrentEnvironment = env.Object;
            var mockScheduledResourceModel = new Mock<IScheduledResourceModel>();
            mockScheduledResourceModel.Setup(model => model.ScheduledResources).Returns(resources);
            string test;
            mockScheduledResourceModel.Setup(model => model.Save(It.IsAny<IScheduledResource>(), out test)).Returns(true).Verifiable();
            schedulerViewModel.ScheduledResourceModel = mockScheduledResourceModel.Object;
            schedulerViewModel.SelectedTask = schedulerViewModel.TaskList[0];
            schedulerViewModel.ShowError("Error while saving: test error");
            Assert.IsTrue(schedulerViewModel.HasErrors);

            var _errorsChanged = false;
            var _errorChanged = false;
            var _taskListChanged = false;


            schedulerViewModel.PropertyChanged += delegate(object sender, PropertyChangedEventArgs args)
            {
                switch(args.PropertyName)
                {
                    case "Errors":
                        _errorsChanged = true;
                        break;
                    case "Error":
                        _errorChanged = true;
                        break;
                    case "TaskList":
                        _taskListChanged = true;
                        break;
                }
            };

            //------------Execute Test---------------------------
            schedulerViewModel.SaveCommand.Execute(null);
            //------------Assert Results-------------------------
            Assert.IsTrue(_errorsChanged);
            Assert.IsTrue(_errorChanged);
            Assert.IsTrue(_taskListChanged);
            Assert.IsFalse(schedulerViewModel.HasErrors);
            Assert.IsTrue(schedulerViewModel.GetCredentialsCalled);
            string errorMessage;
            mockScheduledResourceModel.Verify(model => model.Save(It.IsAny<IScheduledResource>(), out errorMessage), Times.Once());
            Assert.AreEqual("Task2", schedulerViewModel.TaskList[0].Name);
            Assert.AreEqual("Task2", schedulerViewModel.TaskList[0].OldName);
            mockPopupController.Verify(c => c.ShowNameChangedConflict(It.IsAny<string>(), It.IsAny<string>()), Times.Once());
        }


        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("SchedulerViewModel_SaveCommand")]
        public void SchedulerViewModel_SaveCommand_WithNewNameDiffToOldNameYesDialogResponse_OldNameChangesNewTask()
        {
            //------------Setup for test--------------------------
            var resources = new ObservableCollection<IScheduledResource>();
            var scheduledResourceForTest = new ScheduledResource("Task2", SchedulerStatus.Enabled, DateTime.MaxValue, new Mock<IScheduleTrigger>().Object, "TestFlow1") { OldName = "New Task 1", IsDirty = true };
            scheduledResourceForTest.IsDirty = true;
            resources.Add(scheduledResourceForTest);
            resources.Add(new ScheduledResource("Task3", SchedulerStatus.Enabled, DateTime.MaxValue, new Mock<IScheduleTrigger>().Object, "TestFlow1") { OldName = "Task3", IsDirty = true });
            Mock<IPopupController> mockPopupController = new Mock<IPopupController>();
            mockPopupController.Setup(c => c.ShowNameChangedConflict(It.IsAny<string>(), It.IsAny<string>())).Returns(MessageBoxResult.Yes).Verifiable();
            var schedulerViewModel = new SchedulerViewModelForTest(new Mock<IEventAggregator>().Object, new Mock<DirectoryObjectPickerDialog>().Object, mockPopupController.Object, new TestAsyncWorker());
            var env = new Mock<IEnvironmentModel>();
            var auth = new Mock<IAuthorizationService>();
            env.Setup(a => a.AuthorizationService).Returns(auth.Object);
            env.Setup(a => a.IsConnected).Returns(true);
            auth.Setup(a => a.IsAuthorized(AuthorizationContext.Administrator, null)).Returns(true).Verifiable();
            schedulerViewModel.CurrentEnvironment = env.Object;
            var mockScheduledResourceModel = new Mock<IScheduledResourceModel>();
            mockScheduledResourceModel.Setup(model => model.ScheduledResources).Returns(resources);
            string test;
            mockScheduledResourceModel.Setup(model => model.Save(It.IsAny<IScheduledResource>(), out test)).Returns(true).Verifiable();
            schedulerViewModel.ScheduledResourceModel = mockScheduledResourceModel.Object;
            schedulerViewModel.SelectedTask = schedulerViewModel.TaskList[0];
            //------------Execute Test---------------------------
            schedulerViewModel.SaveCommand.Execute(null);
            //------------Assert Results-------------------------
            Assert.IsTrue(schedulerViewModel.GetCredentialsCalled);
            string errorMessage;
            mockScheduledResourceModel.Verify(model => model.Save(It.IsAny<IScheduledResource>(), out errorMessage), Times.Once());
            Assert.AreEqual("Task2", schedulerViewModel.TaskList[0].Name);
            Assert.AreEqual("Task2", schedulerViewModel.TaskList[0].OldName);

        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("SchedulerViewModel_SaveCommand")]
        public void SchedulerViewModel_SaveCommand_ServerNotConnected_ErrorMessageSet()
        {
            //------------Setup for test--------------------------
            var resources = new ObservableCollection<IScheduledResource>();
            var scheduledResourceForTest = new ScheduledResource("Task2", SchedulerStatus.Enabled, DateTime.MaxValue, new Mock<IScheduleTrigger>().Object, "TestFlow1") { OldName = "New Task 1", IsDirty = true };
            scheduledResourceForTest.IsDirty = true;
            resources.Add(scheduledResourceForTest);
            resources.Add(new ScheduledResource("Task3", SchedulerStatus.Enabled, DateTime.MaxValue, new Mock<IScheduleTrigger>().Object, "TestFlow1") { OldName = "Task3", IsDirty = true });
            Mock<IPopupController> mockPopupController = new Mock<IPopupController>();
            mockPopupController.Setup(c => c.ShowNameChangedConflict(It.IsAny<string>(), It.IsAny<string>())).Returns(MessageBoxResult.Yes).Verifiable();
            var schedulerViewModel = new SchedulerViewModelForTest(new Mock<IEventAggregator>().Object, new Mock<DirectoryObjectPickerDialog>().Object, mockPopupController.Object, new TestAsyncWorker());
            var env = new Mock<IEnvironmentModel>();
            var auth = new Mock<IAuthorizationService>();
            env.Setup(c => c.IsConnected).Returns(false);
            env.Setup(a => a.AuthorizationService).Returns(auth.Object);
            auth.Setup(a => a.IsAuthorized(AuthorizationContext.Administrator, null)).Returns(false).Verifiable();
            schedulerViewModel.CurrentEnvironment = env.Object;
            var mockScheduledResourceModel = new Mock<IScheduledResourceModel>();
            mockScheduledResourceModel.Setup(model => model.ScheduledResources).Returns(resources);

            schedulerViewModel.ScheduledResourceModel = mockScheduledResourceModel.Object;
            schedulerViewModel.SelectedTask = schedulerViewModel.TaskList[0];
            //------------Execute Test---------------------------
            schedulerViewModel.SaveCommand.Execute(null);
            //------------Assert Results-------------------------
            auth.Verify(a => a.IsAuthorized(AuthorizationContext.Administrator, null), Times.Never());
            Assert.AreEqual(schedulerViewModel.Errors.FetchErrors().Count, 1);
            Assert.AreEqual(@"Error while saving: Server unreachable.", schedulerViewModel.Errors.FetchErrors().FirstOrDefault());
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("SchedulerViewModel_SaveCommand")]
        public void SchedulerViewModel_SaveCommand_AuthorizationFails()
        {
            //------------Setup for test--------------------------
            var resources = new ObservableCollection<IScheduledResource>();
            var scheduledResourceForTest = new ScheduledResource("Task2", SchedulerStatus.Enabled, DateTime.MaxValue, new Mock<IScheduleTrigger>().Object, "TestFlow1") { OldName = "New Task 1", IsDirty = true };
            scheduledResourceForTest.IsDirty = true;
            resources.Add(scheduledResourceForTest);
            resources.Add(new ScheduledResource("Task3", SchedulerStatus.Enabled, DateTime.MaxValue, new Mock<IScheduleTrigger>().Object, "TestFlow1") { OldName = "Task3", IsDirty = true });
            Mock<IPopupController> mockPopupController = new Mock<IPopupController>();
            mockPopupController.Setup(c => c.ShowNameChangedConflict(It.IsAny<string>(), It.IsAny<string>())).Returns(MessageBoxResult.Yes).Verifiable();
            var schedulerViewModel = new SchedulerViewModelForTest(new Mock<IEventAggregator>().Object, new Mock<DirectoryObjectPickerDialog>().Object, mockPopupController.Object, new TestAsyncWorker());
            var env = new Mock<IEnvironmentModel>();
            var auth = new Mock<IAuthorizationService>();
            env.Setup(a => a.IsConnected).Returns(true);
            env.Setup(a => a.AuthorizationService).Returns(auth.Object);
            auth.Setup(a => a.IsAuthorized(AuthorizationContext.Administrator, null)).Returns(false).Verifiable();
            schedulerViewModel.CurrentEnvironment = env.Object;
            var mockScheduledResourceModel = new Mock<IScheduledResourceModel>();
            mockScheduledResourceModel.Setup(model => model.ScheduledResources).Returns(resources);

            schedulerViewModel.ScheduledResourceModel = mockScheduledResourceModel.Object;
            schedulerViewModel.SelectedTask = schedulerViewModel.TaskList[0];
            //------------Execute Test---------------------------
            schedulerViewModel.SaveCommand.Execute(null);
            //------------Assert Results-------------------------
            auth.Verify(a => a.IsAuthorized(AuthorizationContext.Administrator, null));
            Assert.AreEqual(schedulerViewModel.Errors.FetchErrors().Count, 1);
            Assert.AreEqual(@"Error while saving: You don't have permission to schedule on this server.
You need Administrator permission.", schedulerViewModel.Errors.FetchErrors().FirstOrDefault());
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("SchedulerViewModel_SaveCommand")]
        public void SchedulerViewModel_SaveCommand_WithNewNameDiffToOldNameNoDialogResponse_DialogShowsConflict()
        {
            //------------Setup for test--------------------------
            var resources = new ObservableCollection<IScheduledResource>();
            var scheduledResourceForTest = new ScheduledResource("Task2", SchedulerStatus.Enabled, DateTime.MaxValue, new Mock<IScheduleTrigger>().Object, "TestFlow1") { OldName = "Task1", IsDirty = true };
            scheduledResourceForTest.IsDirty = true;
            resources.Add(scheduledResourceForTest);
            resources.Add(new ScheduledResource("Task3", SchedulerStatus.Enabled, DateTime.MaxValue, new Mock<IScheduleTrigger>().Object, "TestFlow1") { OldName = "Task3", IsDirty = true });
            Mock<IPopupController> mockPopupController = new Mock<IPopupController>();
            mockPopupController.Setup(c => c.ShowNameChangedConflict(It.IsAny<string>(), It.IsAny<string>())).Returns(MessageBoxResult.No).Verifiable();
            var schedulerViewModel = new SchedulerViewModelForTest(new Mock<IEventAggregator>().Object, new Mock<DirectoryObjectPickerDialog>().Object, mockPopupController.Object, new TestAsyncWorker());
            var env = new Mock<IEnvironmentModel>();
            var auth = new Mock<IAuthorizationService>();
            env.Setup(a => a.IsConnected).Returns(true);
            env.Setup(a => a.AuthorizationService).Returns(auth.Object);
            auth.Setup(a => a.IsAuthorized(AuthorizationContext.Administrator, null)).Returns(true).Verifiable();
            schedulerViewModel.CurrentEnvironment = env.Object;
            var mockScheduledResourceModel = new Mock<IScheduledResourceModel>();
            mockScheduledResourceModel.Setup(model => model.ScheduledResources).Returns(resources);
            mockScheduledResourceModel.Setup(model => model.Save(It.IsAny<IScheduledResource>(), It.IsAny<string>(), It.IsAny<string>())).Verifiable();
            schedulerViewModel.ScheduledResourceModel = mockScheduledResourceModel.Object;
            schedulerViewModel.SelectedTask = schedulerViewModel.TaskList[0];
            var _nameChanged = false;


            schedulerViewModel.PropertyChanged += delegate(object sender, PropertyChangedEventArgs args)
            {
                switch(args.PropertyName)
                {
                    case "Name":
                        _nameChanged = true;
                        break;
                }
            };

            //------------Execute Test---------------------------
            schedulerViewModel.SaveCommand.Execute(null);
            //------------Assert Results-------------------------
            Assert.IsTrue(_nameChanged);
            Assert.IsTrue(schedulerViewModel.GetCredentialsCalled);
            string errorMessage;
            mockScheduledResourceModel.Verify(model => model.Save(It.IsAny<IScheduledResource>(), out errorMessage), Times.Once());
            Assert.AreEqual("Task1", schedulerViewModel.TaskList[0].Name);
            mockPopupController.Verify(c => c.ShowNameChangedConflict(It.IsAny<string>(), It.IsAny<string>()), Times.Once());
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("SchedulerViewModel_SaveCommand")]
        public void SchedulerViewModel_SaveCommand_WithNewNameDiffToOldNameCancelDialogResponse_DialogShowsConflict()
        {
            //------------Setup for test--------------------------
            var resources = new ObservableCollection<IScheduledResource>();
            var scheduledResourceForTest = new ScheduledResource("Task2", SchedulerStatus.Enabled, DateTime.MaxValue, new Mock<IScheduleTrigger>().Object, "TestFlow1") { OldName = "Task1", IsDirty = true };
            scheduledResourceForTest.IsDirty = true;
            resources.Add(scheduledResourceForTest);
            resources.Add(new ScheduledResource("Task3", SchedulerStatus.Enabled, DateTime.MaxValue, new Mock<IScheduleTrigger>().Object, "TestFlow1") { OldName = "Task3", IsDirty = true });
            Mock<IPopupController> mockPopupController = new Mock<IPopupController>();
            mockPopupController.Setup(c => c.ShowNameChangedConflict(It.IsAny<string>(), It.IsAny<string>())).Returns(MessageBoxResult.Cancel).Verifiable();
            var schedulerViewModel = new SchedulerViewModelForTest(new Mock<IEventAggregator>().Object, new Mock<DirectoryObjectPickerDialog>().Object, mockPopupController.Object, new TestAsyncWorker());
            var env = new Mock<IEnvironmentModel>();
            var auth = new Mock<IAuthorizationService>();
            env.Setup(a => a.IsConnected).Returns(true);
            env.Setup(a => a.AuthorizationService).Returns(auth.Object);
            auth.Setup(a => a.IsAuthorized(AuthorizationContext.Administrator, null)).Returns(true).Verifiable();
            schedulerViewModel.CurrentEnvironment = env.Object;
            var mockScheduledResourceModel = new Mock<IScheduledResourceModel>();
            mockScheduledResourceModel.Setup(model => model.ScheduledResources).Returns(resources);
            mockScheduledResourceModel.Setup(model => model.Save(It.IsAny<IScheduledResource>(), It.IsAny<string>(), It.IsAny<string>())).Verifiable();
            schedulerViewModel.ScheduledResourceModel = mockScheduledResourceModel.Object;
            schedulerViewModel.SelectedTask = schedulerViewModel.TaskList[0];
            //------------Execute Test---------------------------
            schedulerViewModel.SaveCommand.Execute(null);
            //------------Assert Results-------------------------
            Assert.IsFalse(schedulerViewModel.GetCredentialsCalled);
            string errorMessage;
            mockScheduledResourceModel.Verify(model => model.Save(It.IsAny<IScheduledResource>(), out errorMessage), Times.Never());
            Assert.AreEqual("Task2", schedulerViewModel.TaskList[0].Name);
            mockPopupController.Verify(c => c.ShowNameChangedConflict(It.IsAny<string>(), It.IsAny<string>()), Times.Once());
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SchedulerViewModel_SaveCommand")]
        public void SchedulerViewModel_SaveCommand_WithNewNameDiffToOldNameCancelDialogResponse_IsNewTrue_ShouldNotShowDialog()
        {
            //------------Setup for test--------------------------
            var resources = new ObservableCollection<IScheduledResource>();
            var scheduledResourceForTest = new ScheduledResource("Task2", SchedulerStatus.Enabled, DateTime.MaxValue, new Mock<IScheduleTrigger>().Object, "TestFlow1") { OldName = "Task1", IsDirty = true };
            scheduledResourceForTest.IsDirty = true;
            resources.Add(scheduledResourceForTest);
            resources.Add(new ScheduledResource("Task3", SchedulerStatus.Enabled, DateTime.MaxValue, new Mock<IScheduleTrigger>().Object, "TestFlow1") { OldName = "Task3", IsDirty = true });
            Mock<IPopupController> mockPopupController = new Mock<IPopupController>();
            mockPopupController.Setup(c => c.ShowNameChangedConflict(It.IsAny<string>(), It.IsAny<string>())).Returns(MessageBoxResult.Cancel).Verifiable();
            var schedulerViewModel = new SchedulerViewModelForTest(new Mock<IEventAggregator>().Object, new Mock<DirectoryObjectPickerDialog>().Object, mockPopupController.Object, new TestAsyncWorker());
            var env = new Mock<IEnvironmentModel>();
            var auth = new Mock<IAuthorizationService>();
            env.Setup(a => a.IsConnected).Returns(true);
            env.Setup(a => a.AuthorizationService).Returns(auth.Object);
            auth.Setup(a => a.IsAuthorized(AuthorizationContext.Administrator, null)).Returns(true).Verifiable();
            schedulerViewModel.CurrentEnvironment = env.Object;
            var mockScheduledResourceModel = new Mock<IScheduledResourceModel>();
            mockScheduledResourceModel.Setup(model => model.ScheduledResources).Returns(resources);
            mockScheduledResourceModel.Setup(model => model.Save(It.IsAny<IScheduledResource>(), It.IsAny<string>(), It.IsAny<string>())).Verifiable();
            schedulerViewModel.ScheduledResourceModel = mockScheduledResourceModel.Object;
            schedulerViewModel.SelectedTask = schedulerViewModel.TaskList[0];
            schedulerViewModel.SelectedTask.IsNew = true;
            //------------Execute Test---------------------------
            schedulerViewModel.SaveCommand.Execute(null);
            //------------Assert Results-------------------------
            mockPopupController.Verify(c => c.ShowNameChangedConflict(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SchedulerViewModel_SaveCommand")]
        public void SchedulerViewModel_SaveCommand_WithNewNameDiffToOldNameCancelDialogResponse_IsNewFalse_ShouldShowDialog()
        {
            //------------Setup for test--------------------------
            var resources = new ObservableCollection<IScheduledResource>();
            var scheduledResourceForTest = new ScheduledResource("Task2", SchedulerStatus.Enabled, DateTime.MaxValue, new Mock<IScheduleTrigger>().Object, "TestFlow1") { OldName = "Task1", IsDirty = true };
            scheduledResourceForTest.IsDirty = true;
            resources.Add(scheduledResourceForTest);
            resources.Add(new ScheduledResource("Task3", SchedulerStatus.Enabled, DateTime.MaxValue, new Mock<IScheduleTrigger>().Object, "TestFlow1") { OldName = "Task3", IsDirty = true });
            Mock<IPopupController> mockPopupController = new Mock<IPopupController>();
            mockPopupController.Setup(c => c.ShowNameChangedConflict(It.IsAny<string>(), It.IsAny<string>())).Returns(MessageBoxResult.Cancel).Verifiable();
            var schedulerViewModel = new SchedulerViewModelForTest(new Mock<IEventAggregator>().Object, new Mock<DirectoryObjectPickerDialog>().Object, mockPopupController.Object, new TestAsyncWorker());
            var env = new Mock<IEnvironmentModel>();
            var auth = new Mock<IAuthorizationService>();
            env.Setup(a => a.IsConnected).Returns(true);
            env.Setup(a => a.AuthorizationService).Returns(auth.Object);
            auth.Setup(a => a.IsAuthorized(AuthorizationContext.Administrator, null)).Returns(true).Verifiable();
            schedulerViewModel.CurrentEnvironment = env.Object;
            var mockScheduledResourceModel = new Mock<IScheduledResourceModel>();
            mockScheduledResourceModel.Setup(model => model.ScheduledResources).Returns(resources);
            mockScheduledResourceModel.Setup(model => model.Save(It.IsAny<IScheduledResource>(), It.IsAny<string>(), It.IsAny<string>())).Verifiable();
            schedulerViewModel.ScheduledResourceModel = mockScheduledResourceModel.Object;
            schedulerViewModel.SelectedTask = schedulerViewModel.TaskList[0];
            schedulerViewModel.SelectedTask.IsNew = false;
            //------------Execute Test---------------------------
            schedulerViewModel.SaveCommand.Execute(null);
            //------------Assert Results-------------------------
            Assert.IsFalse(schedulerViewModel.GetCredentialsCalled);
            string errorMessage;
            mockScheduledResourceModel.Verify(model => model.Save(It.IsAny<IScheduledResource>(), out errorMessage), Times.Never());
            Assert.AreEqual("Task2", schedulerViewModel.TaskList[0].Name);
            mockPopupController.Verify(c => c.ShowNameChangedConflict(It.IsAny<string>(), It.IsAny<string>()), Times.Once());
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SchedulerViewModel_SaveCommand")]
        public void SchedulerViewModel_SaveCommand_UserNamePasswordSet_CallsScheduledResourceModelSave()
        {
            //------------Setup for test--------------------------
            var resources = new ObservableCollection<IScheduledResource>();
            var scheduledResourceForTest = new ScheduledResourceForTest { IsDirty = true };
            resources.Add(scheduledResourceForTest);
            scheduledResourceForTest.UserName = "some user";
            scheduledResourceForTest.Password = "some password";
            var schedulerViewModel = new SchedulerViewModelForTest();
            var env = new Mock<IEnvironmentModel>();
            var auth = new Mock<IAuthorizationService>();
            env.Setup(a => a.IsConnected).Returns(true);
            env.Setup(a => a.AuthorizationService).Returns(auth.Object);
            auth.Setup(a => a.IsAuthorized(AuthorizationContext.Administrator, null)).Returns(true).Verifiable();
            schedulerViewModel.CurrentEnvironment = env.Object;
            var mockScheduledResourceModel = new Mock<IScheduledResourceModel>();
            mockScheduledResourceModel.Setup(model => model.ScheduledResources).Returns(resources);
            mockScheduledResourceModel.Setup(model => model.Save(It.IsAny<IScheduledResource>(), It.IsAny<string>(), It.IsAny<string>())).Verifiable();
            schedulerViewModel.ScheduledResourceModel = mockScheduledResourceModel.Object;
            schedulerViewModel.SelectedTask = schedulerViewModel.TaskList[0];
            //------------Execute Test---------------------------
            schedulerViewModel.SaveCommand.Execute(null);
            //------------Assert Results-------------------------
            Assert.IsTrue(schedulerViewModel.ShowSaveErrorDialogCalled);
            Assert.IsFalse(schedulerViewModel.GetCredentialsCalled);
            string errorMessage;
            mockScheduledResourceModel.Verify(model => model.Save(It.IsAny<IScheduledResource>(), out errorMessage), Times.Once());
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SchedulerViewModel_SaveCommand")]
        public void SchedulerViewModel_SaveCommand_UserNamePasswordSet_CallsIsAuthorised()
        {
            //------------Setup for test--------------------------
            var resources = new ObservableCollection<IScheduledResource>();
            var scheduledResourceForTest = new ScheduledResourceForTest { IsDirty = true };
            resources.Add(scheduledResourceForTest);
            scheduledResourceForTest.UserName = "some user";
            scheduledResourceForTest.Password = "some password";
            var schedulerViewModel = new SchedulerViewModelForTest();
            var env = new Mock<IEnvironmentModel>();
            var auth = new Mock<IAuthorizationService>();
            env.Setup(a => a.IsConnected).Returns(true);
            env.Setup(a => a.AuthorizationService).Returns(auth.Object);
            auth.Setup(a => a.IsAuthorized(AuthorizationContext.Administrator, null)).Returns(true).Verifiable();
            schedulerViewModel.CurrentEnvironment = env.Object;
            var mockScheduledResourceModel = new Mock<IScheduledResourceModel>();
            mockScheduledResourceModel.Setup(model => model.ScheduledResources).Returns(resources);
            mockScheduledResourceModel.Setup(model => model.Save(It.IsAny<IScheduledResource>(), It.IsAny<string>(), It.IsAny<string>())).Verifiable();
            schedulerViewModel.ScheduledResourceModel = mockScheduledResourceModel.Object;
            schedulerViewModel.SelectedTask = schedulerViewModel.TaskList[0];
            //------------Execute Test---------------------------
            schedulerViewModel.SaveCommand.Execute(null);
            //------------Assert Results-------------------------
            Assert.IsTrue(schedulerViewModel.ShowSaveErrorDialogCalled);
            Assert.IsFalse(schedulerViewModel.GetCredentialsCalled);
            string errorMessage;
            mockScheduledResourceModel.Verify(model => model.Save(It.IsAny<IScheduledResource>(), out errorMessage), Times.Once());
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SchedulerViewModel_SelectedTask")]
        public void SchedulerViewModel_SelectedTask_SetValue_ShouldFirePropertyChangedNotifications()
        {
            //------------Setup for test--------------------------
            var _selectedTaskChanged = false;
            var _triggerChange = false;
            var _statusChange = false;
            var _workflowNameChange = false;
            var _nameChange = false;
            var _runasapChange = false;
            var _numRecordsChange = false;
            var _triggerTextChange = false;
            var _historyMustChange = false;
            var _accountNameChange = false;
            var _passwordChange = false;
            var _errorsChange = false;
            var _errorChange = false;
            var _selectedHistoryChange = false;

            var agg = new Mock<IEventAggregator>();
            agg.Setup(a => a.Publish(It.IsAny<DebugOutputMessage>())).Verifiable();
            var schedulerViewModel = new SchedulerViewModel(agg.Object, new DirectoryObjectPickerDialog(), new PopupController(), new AsyncWorker(), new Mock<IConnectControlViewModel>().Object);


            schedulerViewModel.PropertyChanged += delegate(object sender, PropertyChangedEventArgs args)
            {
                switch(args.PropertyName)
                {
                    case "SelectedTask":
                        _selectedTaskChanged = true;
                        break;
                    case "Trigger":
                        _triggerChange = true;
                        break;
                    case "Status":
                        _statusChange = true;
                        break;
                    case "WorkflowName":
                        _workflowNameChange = true;
                        break;
                    case "Name":
                        _nameChange = true;
                        break;
                    case "RunAsapIfScheduleMissed":
                        _runasapChange = true;
                        break;
                    case "NumberOfRecordsToKeep":
                        _numRecordsChange = true;
                        break;
                    case "TriggerText":
                        _triggerTextChange = true;
                        break;
                    case "History":
                        _historyMustChange = true;
                        break;
                    case "AccountName":
                        _accountNameChange = true;
                        break;
                    case "Password":
                        _passwordChange = true;
                        break;
                    case "Errors":
                        _errorsChange = true;
                        break;
                    case "Error":
                        _errorChange = true;
                        break;
                    case "SelectedHistory":
                        _selectedHistoryChange = true;
                        break;
                }
            };
            //------------Execute Test---------------------------
            schedulerViewModel.SelectedTask = new ScheduledResourceForTest();
            //------------Assert Results-------------------------
            Assert.IsTrue(_selectedTaskChanged);
            Assert.IsTrue(_triggerChange);
            Assert.IsTrue(_statusChange);
            Assert.IsTrue(_workflowNameChange);
            Assert.IsTrue(_nameChange);
            Assert.IsTrue(_runasapChange);
            Assert.IsTrue(_numRecordsChange);
            Assert.IsTrue(_triggerTextChange);
            Assert.IsFalse(_historyMustChange);
            Assert.IsTrue(_accountNameChange);
            Assert.IsTrue(_passwordChange);
            Assert.IsTrue(_errorsChange);
            Assert.IsTrue(_errorChange);
            Assert.IsTrue(_selectedHistoryChange);
            agg.Verify(a => a.Publish(It.IsAny<DebugOutputMessage>()));
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SchedulerViewModel_SelectedTask")]
        public void SchedulerViewModel_SelectedTask_SetValueWithHistoryTab_ShouldFirePropertyChangedNotifications()
        {
            //------------Setup for test--------------------------
            var _selectedTaskChanged = false;
            var _triggerChange = false;
            var _statusChange = false;
            var _workflowNameChange = false;
            var _nameChange = false;
            var _runasapChange = false;
            var _numRecordsChange = false;
            var _triggerTextChange = false;
            var _historyMustChange = false;
            var _accountNameChange = false;
            var _passwordChange = false;
            var _errorsChange = false;
            var _errorChange = false;
            var _selectedHistoryChange = false;
            var schedulerViewModel = new SchedulerViewModel();
            var activeItem = new TabItem { Header = "History" };
            schedulerViewModel.ActiveItem = activeItem;
            schedulerViewModel.PropertyChanged += delegate(object sender, PropertyChangedEventArgs args)
            {
                switch(args.PropertyName)
                {
                    case "SelectedTask":
                        _selectedTaskChanged = true;
                        break;
                    case "Trigger":
                        _triggerChange = true;
                        break;
                    case "Status":
                        _statusChange = true;
                        break;
                    case "WorkflowName":
                        _workflowNameChange = true;
                        break;
                    case "Name":
                        _nameChange = true;
                        break;
                    case "RunAsapIfScheduleMissed":
                        _runasapChange = true;
                        break;
                    case "NumberOfRecordsToKeep":
                        _numRecordsChange = true;
                        break;
                    case "TriggerText":
                        _triggerTextChange = true;
                        break;
                    case "History":
                        _historyMustChange = true;
                        break;
                    case "AccountName":
                        _accountNameChange = true;
                        break;
                    case "Password":
                        _passwordChange = true;
                        break;
                    case "Errors":
                        _errorsChange = true;
                        break;
                    case "Error":
                        _errorChange = true;
                        break;
                    case "SelectedHistory":
                        _selectedHistoryChange = true;
                        break;
                }
            };
            //------------Execute Test---------------------------
            schedulerViewModel.SelectedTask = new ScheduledResourceForTest();
            //------------Assert Results-------------------------
            Assert.IsTrue(_selectedTaskChanged);
            Assert.IsTrue(_triggerChange);
            Assert.IsTrue(_statusChange);
            Assert.IsTrue(_workflowNameChange);
            Assert.IsTrue(_nameChange);
            Assert.IsTrue(_runasapChange);
            Assert.IsTrue(_numRecordsChange);
            Assert.IsTrue(_triggerTextChange);
            Assert.IsTrue(_historyMustChange);
            Assert.IsTrue(_accountNameChange);
            Assert.IsTrue(_passwordChange);
            Assert.IsTrue(_errorsChange);
            Assert.IsTrue(_errorChange);
            Assert.IsTrue(_selectedHistoryChange);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SchedulerViewModel_Name")]
        public void SchedulerViewModel_Name_EmptyString_AddsErrorMessage()
        {
            //------------Setup for test--------------------------
        
            var schedulerViewModel = new SchedulerViewModel(new Mock<IEventAggregator>().Object, new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IPopupController>().Object, new TestAsyncWorker(), new Mock<IConnectControlViewModel>().Object);

            var scheduledResourceForTest = new ScheduledResourceForTest { Name = "Test" };
            schedulerViewModel.SelectedTask = scheduledResourceForTest;

            //------------------Assert Preconditions---------------------------
            Assert.AreNotEqual(null, schedulerViewModel.SelectedTask, "Scheduler view model selected task cannot be set.");

            //------------Execute Test---------------------------
            schedulerViewModel.Name = "";

            //------------Assert Results-------------------------
            Assert.IsTrue(schedulerViewModel.HasErrors, "Scheduler view model does not have errors after name is made empty.");
            Assert.AreEqual("The name can not be blank", schedulerViewModel.Error);
            Assert.AreEqual(string.Empty, schedulerViewModel.SelectedTask.Name);
            Assert.IsTrue(schedulerViewModel.SelectedTask.IsDirty);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SchedulerViewModel_Name")]
        public void SchedulerViewModel_Name_WasEmptyStringValidString_ClearsErrorMessage()
        {
            //------------Setup for test--------------------------
            var schedulerViewModel = new SchedulerViewModel(new Mock<IEventAggregator>().Object, new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IPopupController>().Object, new TestAsyncWorker(), new Mock<IConnectControlViewModel>().Object);

            var scheduledResourceForTest = new ScheduledResourceForTest { Name = "Test" };
            schedulerViewModel.SelectedTask = scheduledResourceForTest;
            schedulerViewModel.Name = "";

            //------------------Assert Preconditions---------------------------
            Assert.AreNotEqual(null, schedulerViewModel.SelectedTask, "Scheduler view model selected task cannot be set.");
            Assert.AreEqual(string.Empty, schedulerViewModel.Name, "Scheduler view model name cannot be set.");
            Assert.IsTrue(schedulerViewModel.HasErrors, "Scheduler view model does not have errors when its name is empty.");
            Assert.AreEqual("The name can not be blank", schedulerViewModel.Error);

            //------------Execute Test---------------------------
            schedulerViewModel.Name = "This is a test";

            //------------Assert Results-------------------------
            Assert.IsFalse(schedulerViewModel.HasErrors);
            Assert.AreEqual(string.Empty, schedulerViewModel.Error);
            Assert.AreEqual("This is a test", schedulerViewModel.SelectedTask.Name);
            Assert.IsTrue(schedulerViewModel.SelectedTask.IsDirty);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("SchedulerViewModel_WorkflowName")]
        public void SchedulerViewModel_WorkflowName_BlankName_SetsError()
        {
            //------------Setup for test--------------------------

            var schedulerViewModel = new SchedulerViewModel(new Mock<IEventAggregator>().Object, new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IPopupController>().Object, new TestAsyncWorker(), new Mock<IConnectControlViewModel>().Object);

            var scheduledResourceForTest = new ScheduledResourceForTest { WorkflowName = "Test" };
            schedulerViewModel.SelectedTask = scheduledResourceForTest;

            //------------Execute Test---------------------------
            schedulerViewModel.WorkflowName = "";
            //------------Assert Results-------------------------
            Assert.IsTrue(schedulerViewModel.HasErrors);
            Assert.AreEqual("Please select a workflow to schedule", schedulerViewModel.Error);
            Assert.AreEqual(string.Empty, schedulerViewModel.SelectedTask.WorkflowName);
            Assert.IsTrue(schedulerViewModel.SelectedTask.IsDirty);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SchedulerViewModel_Name")]
        public void SchedulerViewModel_Name_DuplicateName_SetsError()
        {
            //------------Setup for test--------------------------

            var resources = new ObservableCollection<IScheduledResource>();
            var scheduledResourceForTest = new ScheduledResourceForTest { Name = "Test" };
            var scheduledResourceForTest2 = new ScheduledResourceForTest { Name = "Test2" };
            scheduledResourceForTest.IsDirty = true;
            resources.Add(scheduledResourceForTest);
            resources.Add(scheduledResourceForTest2);
            var schedulerViewModel = new SchedulerViewModel();

            var mockScheduledResourceModel = new Mock<IScheduledResourceModel>();
            mockScheduledResourceModel.Setup(model => model.ScheduledResources).Returns(resources);
            schedulerViewModel.ScheduledResourceModel = mockScheduledResourceModel.Object;
            schedulerViewModel.SelectedTask = scheduledResourceForTest;
            //------------Execute Test---------------------------
            schedulerViewModel.Name = "Test2";
            //------------Assert Results-------------------------
            Assert.IsTrue(schedulerViewModel.HasErrors);
            Assert.AreEqual("There is already a task with the same name", schedulerViewModel.Error);
            Assert.AreEqual("Test2", schedulerViewModel.SelectedTask.Name);
            Assert.IsTrue(schedulerViewModel.SelectedTask.IsDirty);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SchedulerViewModel_Name")]
        public void SchedulerViewModel_Name_NonDuplicateName_ClearsError()
        {
            //------------Setup for test--------------------------

            var resources = new ObservableCollection<IScheduledResource>();
            var scheduledResourceForTest = new ScheduledResourceForTest { Name = "Test" };
            var scheduledResourceForTest2 = new ScheduledResourceForTest { Name = "Test2" };
            scheduledResourceForTest.IsDirty = true;
            resources.Add(scheduledResourceForTest);
            resources.Add(scheduledResourceForTest2);
            var schedulerViewModel = new SchedulerViewModel();

            var mockScheduledResourceModel = new Mock<IScheduledResourceModel>();
            mockScheduledResourceModel.Setup(model => model.ScheduledResources).Returns(resources);
            schedulerViewModel.ScheduledResourceModel = mockScheduledResourceModel.Object;
            schedulerViewModel.SelectedTask = scheduledResourceForTest;
            schedulerViewModel.Name = "Test2";
            //------------Assert Preconditions------------------
            Assert.IsTrue(schedulerViewModel.HasErrors);
            Assert.AreEqual("There is already a task with the same name", schedulerViewModel.Error);
            Assert.AreEqual("Test2", schedulerViewModel.SelectedTask.Name);
            Assert.IsTrue(schedulerViewModel.SelectedTask.IsDirty);

            //------------Execute Test---------------------------
            schedulerViewModel.Name = "Test Some";
            //------------Assert Results-------------------------
            Assert.IsFalse(schedulerViewModel.HasErrors);
            Assert.AreEqual(string.Empty, schedulerViewModel.Error);
            Assert.AreEqual("Test Some", schedulerViewModel.SelectedTask.Name);
            Assert.IsTrue(schedulerViewModel.SelectedTask.IsDirty);

        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SchedulerViewModel_NumberOfRecordsToKeep")]
        public void SchedulerViewModel_NumberOfRecordsToKeep_NotWholeNumber_KeepsOldNumber()
        {
            //------------Setup for test--------------------------

            var resources = new ObservableCollection<IScheduledResource>();
            var scheduledResourceForTest = new ScheduledResourceForTest { Name = "Test" };
            var scheduledResourceForTest2 = new ScheduledResourceForTest { Name = "Test2" };
            scheduledResourceForTest.IsDirty = false;
            resources.Add(scheduledResourceForTest);
            resources.Add(scheduledResourceForTest2);
            var schedulerViewModel = new SchedulerViewModel();

            var mockScheduledResourceModel = new Mock<IScheduledResourceModel>();
            mockScheduledResourceModel.Setup(model => model.ScheduledResources).Returns(resources);
            schedulerViewModel.ScheduledResourceModel = mockScheduledResourceModel.Object;
            schedulerViewModel.SelectedTask = scheduledResourceForTest;
            //------------Execute Test---------------------------
            schedulerViewModel.NumberOfRecordsToKeep = "5";
            schedulerViewModel.NumberOfRecordsToKeep = "-a5";
            //------------Assert Results-------------------------
            Assert.AreEqual(5, schedulerViewModel.SelectedTask.NumberOfHistoryToKeep);
            Assert.IsTrue(schedulerViewModel.SelectedTask.IsDirty);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SchedulerViewModel_History")]
        public void SchedulerViewModel_History_Get_ShouldCallCreateHistoryOnScheduledResourceModel()
        {
            //------------Setup for test--------------------------
            var schedulerViewModel = new SchedulerViewModel(new Mock<IEventAggregator>().Object, new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IPopupController>().Object, new TestAsyncWorker(), new Mock<IConnectControlViewModel>().Object);
            var activeItem = new TabItem { Header = "History" };
            schedulerViewModel.ActiveItem = activeItem;
            var mockScheduledResourceModel = new Mock<IScheduledResourceModel>();
            var histories = new List<IResourceHistory> { new Mock<IResourceHistory>().Object };
            mockScheduledResourceModel.Setup(model => model.CreateHistory(It.IsAny<IScheduledResource>())).Returns(histories);
            schedulerViewModel.ScheduledResourceModel = mockScheduledResourceModel.Object;
            schedulerViewModel.SelectedTask = new Mock<IScheduledResource>().Object;
            //------------Execute Test---------------------------
            var resourceHistories = schedulerViewModel.History;
            //------------Assert Results-------------------------
            mockScheduledResourceModel.Verify(model => model.CreateHistory(It.IsAny<IScheduledResource>()), Times.Once());
            Assert.IsNotNull(resourceHistories);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SchedulerViewModel_ActiveItem")]
        public void SchedulerViewModel_ActiveItem_HeaderNotHistory_ShouldNotFirePropertyChangeOnHistory()
        {
            //------------Setup for test--------------------------
            var _historyMustChange = false;
            var schedulerViewModel = new SchedulerViewModel();
            var activeItem = new TabItem { Header = "Settings" };
            schedulerViewModel.PropertyChanged += (sender, args) =>
            {
                if(args.PropertyName == "History")
                {
                    _historyMustChange = true;
                }
            };
            //------------Execute Test---------------------------
            schedulerViewModel.ActiveItem = activeItem;
            //------------Assert Results-------------------------
            Assert.IsFalse(_historyMustChange);
        }




        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SchedulerViewModel_ActiveItem")]
        public void SchedulerViewModel_ActiveItem_HeaderHistory_ShouldFirePropertyChangeOnHistory()
        {
            //------------Setup for test--------------------------
            var _historyMustChange = false;
            var schedulerViewModel = new SchedulerViewModel();
            var activeItem = new TabItem { Header = "History" };
            schedulerViewModel.PropertyChanged += (sender, args) =>
            {
                if(args.PropertyName == "History")
                {
                    _historyMustChange = true;
                }
            };
            //------------Execute Test---------------------------
            schedulerViewModel.ActiveItem = activeItem;
            //------------Assert Results-------------------------
            Assert.IsTrue(_historyMustChange);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("SchedulerViewModel_Validation")]
        public void SchedulerViewModel_Validation_NoErrorsWhenNothingSelected()
        {
            var resourceModel = new Mock<IScheduledResourceModel>();
            var resources = new ObservableCollection<IScheduledResource> { new ScheduledResource("bob", SchedulerStatus.Enabled, DateTime.MaxValue, new Mock<IScheduleTrigger>().Object, "c"), new ScheduledResource("dave", SchedulerStatus.Enabled, DateTime.MaxValue, new Mock<IScheduleTrigger>().Object, "c") };
            resourceModel.Setup(a => a.ScheduledResources).Returns(resources);

            var schedulerViewModel = new SchedulerViewModel
            {
                ScheduledResourceModel = resourceModel.Object
            };
            Assert.IsFalse(schedulerViewModel.HasErrors);

        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("SchedulerViewModel_Validation")]
        public void SchedulerViewModel_Validation_NoErrorOnSelected()
        {
            var resourceModel = new Mock<IScheduledResourceModel>();
            var resources = new ObservableCollection<IScheduledResource> { new ScheduledResource("bob", SchedulerStatus.Enabled, DateTime.MaxValue, new Mock<IScheduleTrigger>().Object, "c"), new ScheduledResource("dave", SchedulerStatus.Enabled, DateTime.MaxValue, new Mock<IScheduleTrigger>().Object, "c") };
            resourceModel.Setup(a => a.ScheduledResources).Returns(resources);

            var schedulerViewModel = new SchedulerViewModel
                {
                    ScheduledResourceModel = resourceModel.Object,
                    SelectedTask = resources[0],
                    Name = "monkeys"
                };
            // validation occurs on property changes
            Assert.IsFalse(schedulerViewModel.HasErrors);

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("SchedulerViewModel_Validation")]
        public void SchedulerViewModel_Validation_IfDuplicateNames()
        {
            var resourceModel = new Mock<IScheduledResourceModel>();
            var resources = new ObservableCollection<IScheduledResource> { new ScheduledResource("bob", SchedulerStatus.Enabled, DateTime.MaxValue, new Mock<IScheduleTrigger>().Object, "c") { NumberOfHistoryToKeep = 1 }, new ScheduledResource("dave", SchedulerStatus.Enabled, DateTime.MaxValue, new Mock<IScheduleTrigger>().Object, "c") };
            resourceModel.Setup(a => a.ScheduledResources).Returns(resources);

            var schedulerViewModel = new SchedulerViewModel
                {
                    ScheduledResourceModel = resourceModel.Object,
                    SelectedTask = resources[0],
                    Name = "dave"
                };
            //create duplicate name
            Assert.IsTrue(schedulerViewModel.HasErrors);
            Assert.AreEqual("There is already a task with the same name", schedulerViewModel.Error);
            Assert.IsTrue(resources.All(a => a.Errors.HasErrors()));
            Assert.IsTrue(resources.All(a => a.Errors.FetchErrors()[0] == "There is already a task with the same name"));


        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("SchedulerViewModel_CreateNewTask")]
        public void SchedulerViewModel_CreateNewTask_ShouldAddTaskToListWithDefaultSettings()
        {
            //------------Setup for test--------------------------
            var schedulerViewModel = new SchedulerViewModel(new Mock<IEventAggregator>().Object, new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IPopupController>().Object, new TestAsyncWorker(), new Mock<IConnectControlViewModel>().Object);
            var resources = new ObservableCollection<IScheduledResource> { new ScheduledResource("bob", SchedulerStatus.Enabled, DateTime.MaxValue, new Mock<IScheduleTrigger>().Object, "c") { NumberOfHistoryToKeep = 1 }, new ScheduledResource("dave", SchedulerStatus.Enabled, DateTime.MaxValue, new Mock<IScheduleTrigger>().Object, "c") };

            var resourceModel = new Mock<IScheduledResourceModel>();
            resourceModel.Setup(c => c.ScheduledResources).Returns(resources);
            schedulerViewModel.ScheduledResourceModel = resourceModel.Object;
            //------------Execute Test---------------------------

            Assert.AreEqual(2, schedulerViewModel.TaskList.Count);

            schedulerViewModel.NewCommand.Execute(null);
            Assert.AreEqual(3, schedulerViewModel.TaskList.Count);
            Assert.AreEqual("New Task1", schedulerViewModel.TaskList[2].Name);
            Assert.AreEqual("New Task1", schedulerViewModel.TaskList[2].OldName);
            Assert.IsTrue(schedulerViewModel.TaskList[2].IsDirty);
            Assert.AreEqual(SchedulerStatus.Enabled, schedulerViewModel.TaskList[2].Status);
            Assert.AreEqual(string.Empty, schedulerViewModel.TaskList[2].WorkflowName);
            Assert.AreEqual(schedulerViewModel.SelectedTask, schedulerViewModel.TaskList[2]);

            schedulerViewModel.NewCommand.Execute(null);
            //------------Assert Results-------------------------
            Assert.AreEqual(4, schedulerViewModel.TaskList.Count);
            Assert.AreEqual("New Task2", schedulerViewModel.TaskList[3].Name);
            Assert.AreEqual("New Task2", schedulerViewModel.TaskList[3].OldName);
            Assert.IsTrue(schedulerViewModel.TaskList[3].IsDirty);
            Assert.AreEqual(SchedulerStatus.Enabled, schedulerViewModel.TaskList[3].Status);
            Assert.AreEqual(string.Empty, schedulerViewModel.TaskList[3].WorkflowName);
            Assert.AreEqual(schedulerViewModel.SelectedTask, schedulerViewModel.TaskList[3]);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("SchedulerViewModel_DeleteTask")]
        public void SchedulerViewModel_DeleteTask_DeleteSecondTask_ShouldDeleteTaskFromList()
        {
            //------------Setup for test--------------------------

            Mock<IPopupController> mockPopUpController = new Mock<IPopupController>();
            mockPopUpController.Setup(c => c.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            var schedulerViewModel = new SchedulerViewModel(new Mock<IEventAggregator>().Object, new Mock<DirectoryObjectPickerDialog>().Object, mockPopUpController.Object, new TestAsyncWorker(), new Mock<IConnectControlViewModel>().Object);
            var env = new Mock<IEnvironmentModel>();
            var auth = new Mock<IAuthorizationService>();
            env.Setup(a => a.IsConnected).Returns(true);
            env.Setup(a => a.AuthorizationService).Returns(auth.Object);
            auth.Setup(a => a.IsAuthorized(AuthorizationContext.Administrator, null)).Returns(true).Verifiable();
            schedulerViewModel.CurrentEnvironment = env.Object;
            var resources = new ObservableCollection<IScheduledResource> { new ScheduledResource("bob", SchedulerStatus.Enabled, DateTime.MaxValue, new Mock<IScheduleTrigger>().Object, "c") { NumberOfHistoryToKeep = 1 }, new ScheduledResource("dave", SchedulerStatus.Enabled, DateTime.MaxValue, new Mock<IScheduleTrigger>().Object, "c") };

            var resourceModel = new Mock<IScheduledResourceModel>();
            resourceModel.Setup(c => c.ScheduledResources).Returns(resources);
            resourceModel.Setup(c => c.DeleteSchedule(It.IsAny<IScheduledResource>())).Verifiable();
            schedulerViewModel.ScheduledResourceModel = resourceModel.Object;
            //------------Execute Test---------------------------
            Assert.AreEqual(2, schedulerViewModel.TaskList.Count);
            schedulerViewModel.SelectedTask = schedulerViewModel.TaskList[1];

            var _taskListChange = false;
            var _historyChange = false;

            schedulerViewModel.PropertyChanged += delegate(object sender, PropertyChangedEventArgs args)
          {
              switch(args.PropertyName)
              {
                  case "TaskList":
                      _taskListChange = true;
                      break;
                  case "History":
                      _historyChange = true;
                      break;
              }
          };

            schedulerViewModel.DeleteCommand.Execute(null);
            //------------Assert Results-------------------------
            Assert.IsTrue(_taskListChange);
            Assert.IsTrue(_historyChange);
            Assert.AreEqual(1, schedulerViewModel.TaskList.Count);
            Assert.AreEqual(schedulerViewModel.SelectedTask, schedulerViewModel.TaskList[0]);
            Assert.AreEqual("bob", schedulerViewModel.TaskList[0].Name);
            Assert.AreEqual(SchedulerStatus.Enabled, schedulerViewModel.TaskList[0].Status);
            resourceModel.Verify(a => a.DeleteSchedule(It.IsAny<IScheduledResource>()), Times.Once());
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("SchedulerViewModel_DeleteTask")]
        public void SchedulerViewModel_DeleteTask_DeleteFirstTask_ShouldDeleteTaskFromList()
        {
            //------------Setup for test--------------------------

            Mock<IPopupController> mockPopUpController = new Mock<IPopupController>();
            mockPopUpController.Setup(c => c.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            var schedulerViewModel = new SchedulerViewModel(new Mock<IEventAggregator>().Object, new Mock<DirectoryObjectPickerDialog>().Object, mockPopUpController.Object, new TestAsyncWorker(), new Mock<IConnectControlViewModel>().Object);
            var env = new Mock<IEnvironmentModel>();
            var auth = new Mock<IAuthorizationService>();
            env.Setup(a => a.IsConnected).Returns(true);
            env.Setup(a => a.AuthorizationService).Returns(auth.Object);
            auth.Setup(a => a.IsAuthorized(AuthorizationContext.Administrator, null)).Returns(true).Verifiable();
            schedulerViewModel.CurrentEnvironment = env.Object;
            var resources = new ObservableCollection<IScheduledResource> { new ScheduledResource("bob", SchedulerStatus.Enabled, DateTime.MaxValue, new Mock<IScheduleTrigger>().Object, "c") { NumberOfHistoryToKeep = 1 }, new ScheduledResource("dave", SchedulerStatus.Enabled, DateTime.MaxValue, new Mock<IScheduleTrigger>().Object, "c") };

            var resourceModel = new Mock<IScheduledResourceModel>();
            resourceModel.Setup(c => c.ScheduledResources).Returns(resources);
            resourceModel.Setup(c => c.DeleteSchedule(It.IsAny<IScheduledResource>())).Verifiable();
            schedulerViewModel.ScheduledResourceModel = resourceModel.Object;
            //------------Execute Test---------------------------
            Assert.AreEqual(2, schedulerViewModel.TaskList.Count);
            schedulerViewModel.SelectedTask = schedulerViewModel.TaskList[0];

            schedulerViewModel.DeleteCommand.Execute(null);
            //------------Assert Results-------------------------
            Assert.AreEqual(1, schedulerViewModel.TaskList.Count);
            Assert.AreEqual(schedulerViewModel.SelectedTask, schedulerViewModel.TaskList[0]);
            Assert.AreEqual("dave", schedulerViewModel.TaskList[0].Name);
            Assert.AreEqual(SchedulerStatus.Enabled, schedulerViewModel.TaskList[0].Status);
            resourceModel.Verify(a => a.DeleteSchedule(It.IsAny<IScheduledResource>()), Times.Once());
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("SchedulerViewModel_DeleteTask")]
        public void SchedulerViewModel_DeleteTask_DeleteWithNoAdminRights_ShouldShowError()
        {
            //------------Setup for test--------------------------

            Mock<IPopupController> mockPopUpController = new Mock<IPopupController>();
            mockPopUpController.Setup(c => c.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            var schedulerViewModel = new SchedulerViewModel(new Mock<IEventAggregator>().Object, new Mock<DirectoryObjectPickerDialog>().Object, mockPopUpController.Object, new TestAsyncWorker(), new Mock<IConnectControlViewModel>().Object);
            var env = new Mock<IEnvironmentModel>();
            var auth = new Mock<IAuthorizationService>();
            env.Setup(a => a.IsConnected).Returns(true);
            env.Setup(a => a.AuthorizationService).Returns(auth.Object);
            auth.Setup(a => a.IsAuthorized(AuthorizationContext.Administrator, null)).Returns(false).Verifiable();
            schedulerViewModel.CurrentEnvironment = env.Object;
            var resources = new ObservableCollection<IScheduledResource> { new ScheduledResource("bob", SchedulerStatus.Enabled, DateTime.MaxValue, new Mock<IScheduleTrigger>().Object, "c") { NumberOfHistoryToKeep = 1 }, new ScheduledResource("dave", SchedulerStatus.Enabled, DateTime.MaxValue, new Mock<IScheduleTrigger>().Object, "c") };

            var resourceModel = new Mock<IScheduledResourceModel>();
            resourceModel.Setup(c => c.ScheduledResources).Returns(resources);
            resourceModel.Setup(c => c.DeleteSchedule(It.IsAny<IScheduledResource>())).Verifiable();
            schedulerViewModel.ScheduledResourceModel = resourceModel.Object;
            //------------Execute Test---------------------------
            Assert.AreEqual(2, schedulerViewModel.TaskList.Count);
            schedulerViewModel.SelectedTask = schedulerViewModel.TaskList[0];

            schedulerViewModel.DeleteCommand.Execute(null);
            //------------Assert Results-------------------------
            Assert.AreEqual(@"Error while saving: You don't have permission to schedule on this server.
You need Administrator permission.", schedulerViewModel.Error);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("SchedulerViewModel_DeleteTask")]
        public void SchedulerViewModel_DeleteTask_DeleteWhenEnvironmentIsntConnected_ShouldShowError()
        {
            //------------Setup for test--------------------------

            Mock<IPopupController> mockPopUpController = new Mock<IPopupController>();
            mockPopUpController.Setup(c => c.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            var schedulerViewModel = new SchedulerViewModel(new Mock<IEventAggregator>().Object, new Mock<DirectoryObjectPickerDialog>().Object, mockPopUpController.Object, new TestAsyncWorker(), new Mock<IConnectControlViewModel>().Object);
            var env = new Mock<IEnvironmentModel>();
            var auth = new Mock<IAuthorizationService>();
            env.Setup(a => a.IsConnected).Returns(false);
            env.Setup(a => a.AuthorizationService).Returns(auth.Object);
            auth.Setup(a => a.IsAuthorized(AuthorizationContext.Administrator, null)).Returns(true).Verifiable();
            schedulerViewModel.CurrentEnvironment = env.Object;
            var resources = new ObservableCollection<IScheduledResource> { new ScheduledResource("bob", SchedulerStatus.Enabled, DateTime.MaxValue, new Mock<IScheduleTrigger>().Object, "c") { NumberOfHistoryToKeep = 1 }, new ScheduledResource("dave", SchedulerStatus.Enabled, DateTime.MaxValue, new Mock<IScheduleTrigger>().Object, "c") };

            var resourceModel = new Mock<IScheduledResourceModel>();
            resourceModel.Setup(c => c.ScheduledResources).Returns(resources);
            resourceModel.Setup(c => c.DeleteSchedule(It.IsAny<IScheduledResource>())).Verifiable();
            schedulerViewModel.ScheduledResourceModel = resourceModel.Object;
            //------------Execute Test---------------------------
            Assert.AreEqual(2, schedulerViewModel.TaskList.Count);
            schedulerViewModel.SelectedTask = schedulerViewModel.TaskList[0];

            schedulerViewModel.DeleteCommand.Execute(null);
            //------------Assert Results-------------------------
            Assert.AreEqual("Error while saving: Server unreachable.", schedulerViewModel.Error);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("SchedulerViewModel_EditTrigger")]
        public void SchedulerViewModel_EditTrigger_ShouldEditTheTrigger()
        {
            //------------Setup for test--------------------------

            var schedulerViewModel = new SchedulerViewModelForTest();
            var resources = new ObservableCollection<IScheduledResource>();
            Mock<IScheduleTrigger> mockScheduleTrigger = new Mock<IScheduleTrigger>();
            mockScheduleTrigger.Setup(c => c.Trigger.Instance).Returns(new DailyTrigger());
            resources.Add(new ScheduledResource("bob", SchedulerStatus.Enabled, DateTime.MaxValue, mockScheduleTrigger.Object, "c") { NumberOfHistoryToKeep = 1 });
            resources.Add(new ScheduledResource("dave", SchedulerStatus.Enabled, DateTime.MaxValue, mockScheduleTrigger.Object, "c"));

            var resourceModel = new Mock<IScheduledResourceModel>();
            resourceModel.Setup(c => c.ScheduledResources).Returns(resources);
            schedulerViewModel.ScheduledResourceModel = resourceModel.Object;
            //------------Execute Test---------------------------
            Assert.AreEqual(2, schedulerViewModel.TaskList.Count);
            schedulerViewModel.SelectedTask = schedulerViewModel.TaskList[0];

            var _triggerTextChange = false;

            schedulerViewModel.PropertyChanged += delegate(object sender, PropertyChangedEventArgs args)
            {
                switch(args.PropertyName)
                {
                    case "TriggerText":
                        _triggerTextChange = true;
                        break;
                }
            };

            schedulerViewModel.EditTriggerCommand.Execute(null);
            //------------Assert Results-------------------------
            Assert.IsTrue(_triggerTextChange);
            Assert.AreEqual("bob", schedulerViewModel.SelectedTask.Name);
            Assert.AreEqual(2013, schedulerViewModel.SelectedTask.NextRunDate.Year);
            Assert.AreEqual(02, schedulerViewModel.SelectedTask.NextRunDate.Hour);
            Assert.AreEqual(21, schedulerViewModel.SelectedTask.NextRunDate.Minute);
            Assert.IsTrue(schedulerViewModel.TriggerText.StartsWith("At"));
            Assert.IsTrue(schedulerViewModel.TriggerText.EndsWith("AM every day"));
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SchedulerViewModel_AddWorkflow")]
        public void SchedulerViewModel_AddWorkflow_WithNewTaskNameSet_WorkflowNameChangedAndNameChanged()
        {
            //------------Setup for test--------------------------
            var resources = new ObservableCollection<IScheduledResource> { new ScheduledResource("New Task1", SchedulerStatus.Enabled, DateTime.MaxValue, new Mock<IScheduleTrigger>().Object, "TestFlow1") { NumberOfHistoryToKeep = 1 } };
            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            var mockConnection = new Mock<IEnvironmentConnection>();
            mockConnection.Setup(connection => connection.IsConnected).Returns(true);
            mockConnection.Setup(connection => connection.WorkspaceID).Returns(Guid.NewGuid());
            mockEnvironmentModel.Setup(model => model.Connection).Returns(mockConnection.Object);
            mockEnvironmentModel.Setup(model => model.IsConnected).Returns(true);
            ResourceRepository resourceRepo = new ResourceRepository(mockEnvironmentModel.Object);
            var setupResourceModelMock = Dev2MockFactory.SetupResourceModelMock(ResourceType.WorkflowService, "TestFlow2");
            var resId = Guid.NewGuid();
            setupResourceModelMock.Setup(c => c.ID).Returns(resId);
            resourceRepo.Add(Dev2MockFactory.SetupResourceModelMock(ResourceType.WorkflowService, "TestFlow1").Object);
            resourceRepo.Add(setupResourceModelMock.Object);
            resourceRepo.Add(Dev2MockFactory.SetupResourceModelMock(ResourceType.WorkflowService, "TestFlow3").Object);
            mockEnvironmentModel.Setup(c => c.ResourceRepository).Returns(resourceRepo);

            var schedulerViewModel = new SchedulerViewModel(new Mock<IEventAggregator>().Object, new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IPopupController>().Object, new TestAsyncWorker(), new Mock<IConnectControlViewModel>().Object) { CurrentEnvironment = mockEnvironmentModel.Object };
            Mock<IResourcePickerDialog> mockResourcePickerDialog = new Mock<IResourcePickerDialog>();
            mockResourcePickerDialog.Setup(c => c.ShowDialog(It.IsAny<IEnvironmentModel>())).Returns(true);
            mockResourcePickerDialog.Setup(c => c.SelectedResource).Returns(setupResourceModelMock.Object);

            schedulerViewModel.ResourcePickerDialog = mockResourcePickerDialog.Object;

            Mock<IScheduledResourceModel> scheduledResourceModelMock = new Mock<IScheduledResourceModel>();
            scheduledResourceModelMock.Setup(c => c.ScheduledResources).Returns(resources);
            schedulerViewModel.ScheduledResourceModel = scheduledResourceModelMock.Object;
            //------------Execute Test---------------------------
            schedulerViewModel.TaskList[0].WorkflowName = "TestFlow3";
            schedulerViewModel.SelectedTask = schedulerViewModel.TaskList[0];

            var _nameChange = false;
            var _workflowNameChange = false;
            var _taskListChange = false;

            schedulerViewModel.PropertyChanged += delegate(object sender, PropertyChangedEventArgs args)
            {
                switch(args.PropertyName)
                {
                    case "TaskList":
                        _taskListChange = true;
                        break;
                    case "Name":
                        _nameChange = true;
                        break;
                    case "WorkflowName":
                        _workflowNameChange = true;
                        break;
                }
            };

            schedulerViewModel.AddWorkflowCommand.Execute(null);
            //------------Assert Results-------------------------  
            Assert.IsTrue(_nameChange);
            Assert.IsTrue(_workflowNameChange);
            Assert.IsTrue(_taskListChange);
            Assert.IsTrue(schedulerViewModel.SelectedTask.IsDirty);
            Assert.AreEqual("TestFlow2", schedulerViewModel.Name);
            Assert.AreEqual("Category\\Testing", schedulerViewModel.WorkflowName);
            Assert.AreEqual(resId, schedulerViewModel.SelectedTask.ResourceId);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("SchedulerViewModel_DeActivate")]
        public void SchedulerViewModel_DeActivateDiscard_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var resources = new ObservableCollection<IScheduledResource>();
            var scheduledResourceForTest = new ScheduledResourceForTest { Trigger = new ScheduleTrigger(TaskState.Ready, new Dev2DailyTrigger(new TaskServiceConvertorFactory(), new DailyTrigger()), new Dev2TaskService(new TaskServiceConvertorFactory()), new TaskServiceConvertorFactory()), IsDirty = true };
            resources.Add(scheduledResourceForTest);
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            var serializeObject = serializer.SerializeToBuilder(resources);
            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            var mockConnection = new Mock<IEnvironmentConnection>();
            mockConnection.Setup(connection => connection.IsConnected).Returns(true);
            mockConnection.Setup(connection => connection.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(serializeObject);
            mockConnection.Setup(connection => connection.WorkspaceID).Returns(Guid.NewGuid());
            mockEnvironmentModel.Setup(model => model.Connection).Returns(mockConnection.Object);
            mockEnvironmentModel.Setup(model => model.IsConnected).Returns(true);
            ResourceRepository resourceRepo = new ResourceRepository(mockEnvironmentModel.Object);
            var setupResourceModelMock = Dev2MockFactory.SetupResourceModelMock(ResourceType.WorkflowService, "TestFlow2");
            resourceRepo.Add(Dev2MockFactory.SetupResourceModelMock(ResourceType.WorkflowService, "TestFlow1").Object);
            resourceRepo.Add(setupResourceModelMock.Object);
            resourceRepo.Add(Dev2MockFactory.SetupResourceModelMock(ResourceType.WorkflowService, "TestFlow3").Object);
            mockEnvironmentModel.Setup(c => c.ResourceRepository).Returns(resourceRepo);
            var popup = new Mock<IPopupController>();
            popup.Setup(a => a.ShowSchedulerCloseConfirmation()).Returns(MessageBoxResult.No);
            var schedulerViewModel = new SchedulerViewModel(new Mock<IEventAggregator>().Object, new Mock<DirectoryObjectPickerDialog>().Object, popup.Object, new TestAsyncWorker(), new Mock<IConnectControlViewModel>().Object) { SelectedTask = scheduledResourceForTest, CurrentEnvironment = null };
            //------------Execute Test---------------------------
            Assert.IsTrue(schedulerViewModel.DoDeactivate());
            //------------Assert Results-------------------------

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("SchedulerViewModel_TriggerEquals")]
        public void SchedulerViewModel_TriggerEquals_Equals_ExpectTrue()
        {
            //------------Setup for test--------------------------

// ReSharper disable UseObjectOrCollectionInitializer
            DailyTrigger t = new DailyTrigger();

            t.StartBoundary = new DateTime(2014, 01, 01);
            DailyTrigger t2 = new DailyTrigger();
            t2.StartBoundary = new DateTime(2014, 01, 01);
// ReSharper restore UseObjectOrCollectionInitializer           
            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.IsTrue(SchedulerViewModel.TriggerEquals(t, t2));
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("SchedulerViewModel_TriggerEquals")]
        public void SchedulerViewModel_TriggerEquals_NotEquals_ExpectFalse()
        {
            //------------Setup for test--------------------------

            // ReSharper disable UseObjectOrCollectionInitializer
            DailyTrigger t = new DailyTrigger();

            t.StartBoundary = new DateTime(2014, 01, 01);
            DailyTrigger t2 = new DailyTrigger();
            t2.StartBoundary = new DateTime(2015, 01, 01);
            // ReSharper restore UseObjectOrCollectionInitializer
            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.IsFalse(SchedulerViewModel.TriggerEquals(t, t2));
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("SchedulerViewModel_DeActivate")]
        public void SchedulerViewModel_DeActivateCancel_ReturnsFalse()
        {
            //------------Setup for test--------------------------
            var resources = new ObservableCollection<IScheduledResource>();
            var scheduledResourceForTest = new ScheduledResourceForTest { Trigger = new ScheduleTrigger(TaskState.Ready, new Dev2DailyTrigger(new TaskServiceConvertorFactory(), new DailyTrigger()), new Dev2TaskService(new TaskServiceConvertorFactory()), new TaskServiceConvertorFactory()), IsDirty = true };
            resources.Add(scheduledResourceForTest);
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            var serializeObject = serializer.SerializeToBuilder(resources);
            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            var mockConnection = new Mock<IEnvironmentConnection>();
            mockConnection.Setup(connection => connection.IsConnected).Returns(true);
            mockConnection.Setup(connection => connection.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(serializeObject);
            mockConnection.Setup(connection => connection.WorkspaceID).Returns(Guid.NewGuid());
            mockEnvironmentModel.Setup(model => model.Connection).Returns(mockConnection.Object);
            mockEnvironmentModel.Setup(model => model.IsConnected).Returns(true);
            ResourceRepository resourceRepo = new ResourceRepository(mockEnvironmentModel.Object);
            var setupResourceModelMock = Dev2MockFactory.SetupResourceModelMock(ResourceType.WorkflowService, "TestFlow2");
            resourceRepo.Add(Dev2MockFactory.SetupResourceModelMock(ResourceType.WorkflowService, "TestFlow1").Object);
            resourceRepo.Add(setupResourceModelMock.Object);
            resourceRepo.Add(Dev2MockFactory.SetupResourceModelMock(ResourceType.WorkflowService, "TestFlow3").Object);
            mockEnvironmentModel.Setup(c => c.ResourceRepository).Returns(resourceRepo);
            var popup = new Mock<IPopupController>();
            popup.Setup(a => a.ShowSchedulerCloseConfirmation()).Returns(MessageBoxResult.Cancel);
            var schedulerViewModel = new SchedulerViewModel(new Mock<IEventAggregator>().Object, new Mock<DirectoryObjectPickerDialog>().Object, popup.Object, new TestAsyncWorker(), new Mock<IConnectControlViewModel>().Object) { SelectedTask = scheduledResourceForTest, CurrentEnvironment = null };
            //------------Execute Test---------------------------
            Assert.IsFalse(schedulerViewModel.DoDeactivate());
            //------------Assert Results-------------------------

        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("SchedulerViewModel_HandleServerSelectionChangedMessage")]
        public void SchedulerViewModel_DeActivateSave_AttemptsSave()
        {
            //------------Setup for test--------------------------
            var resources = new ObservableCollection<IScheduledResource>();
            var scheduledResourceForTest = new ScheduledResourceForTest { Trigger = new ScheduleTrigger(TaskState.Ready, new Dev2DailyTrigger(new TaskServiceConvertorFactory(), new DailyTrigger()), new Dev2TaskService(new TaskServiceConvertorFactory()), new TaskServiceConvertorFactory()), IsDirty = true };
            resources.Add(scheduledResourceForTest);
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            var serializeObject = serializer.SerializeToBuilder(resources);
            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            var mockConnection = new Mock<IEnvironmentConnection>();
            mockConnection.Setup(connection => connection.IsConnected).Returns(true);
            mockConnection.Setup(connection => connection.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(serializeObject);
            mockConnection.Setup(connection => connection.WorkspaceID).Returns(Guid.NewGuid());
            mockEnvironmentModel.Setup(model => model.Connection).Returns(mockConnection.Object);
            mockEnvironmentModel.Setup(model => model.IsConnected).Returns(true);
            ResourceRepository resourceRepo = new ResourceRepository(mockEnvironmentModel.Object);
            var setupResourceModelMock = Dev2MockFactory.SetupResourceModelMock(ResourceType.WorkflowService, "TestFlow2");
            resourceRepo.Add(Dev2MockFactory.SetupResourceModelMock(ResourceType.WorkflowService, "TestFlow1").Object);
            resourceRepo.Add(setupResourceModelMock.Object);
            resourceRepo.Add(Dev2MockFactory.SetupResourceModelMock(ResourceType.WorkflowService, "TestFlow3").Object);
            mockEnvironmentModel.Setup(c => c.ResourceRepository).Returns(resourceRepo);
            var popup = new Mock<IPopupController>();
            popup.Setup(a => a.ShowSchedulerCloseConfirmation()).Returns(MessageBoxResult.Yes);
            var schedulerViewModel = new SchedulerViewModel(new Mock<IEventAggregator>().Object, new Mock<DirectoryObjectPickerDialog>().Object, popup.Object, new TestAsyncWorker(), new Mock<IConnectControlViewModel>().Object) { SelectedTask = scheduledResourceForTest };

            var auth = new Mock<IAuthorizationService>();
            mockEnvironmentModel.Setup(a => a.AuthorizationService).Returns(auth.Object);
            auth.Setup(a => a.IsAuthorized(AuthorizationContext.Administrator, null)).Returns(false).Verifiable();
            schedulerViewModel.CurrentEnvironment = mockEnvironmentModel.Object;
            //------------Execute Test---------------------------
            schedulerViewModel.DoDeactivate();
            //------------Assert Results-------------------------
            auth.Verify(a => a.IsAuthorized(AuthorizationContext.Administrator, null));
            Assert.AreEqual(@"Error while saving: You don't have permission to schedule on this server.
You need Administrator permission.", schedulerViewModel.Error);



        }

    }

    internal class SchedulerViewModelForTest : SchedulerViewModel
    {
        public SchedulerViewModelForTest()
        {

        }

        public SchedulerViewModelForTest(IEventAggregator eventPublisher, DirectoryObjectPickerDialog directoryObjectPicker, IPopupController popupController, IAsyncWorker asyncWorker)
            : base(eventPublisher, directoryObjectPicker, popupController, asyncWorker, new Mock<IConnectControlViewModel>().Object)
        {

        }
        #region Overrides of SchedulerViewModel

        public bool GetCredentialsCalled
        {
            get;
            private set;
        }

        public bool ShowSaveErrorDialogCalled
        {
            get;
            private set;
        }

        public override void ShowSaveErrorDialog(string error)
        {
            ShowSaveErrorDialogCalled = true;
        }

        public override void GetCredentials(IScheduledResource scheduledResource)
        {
            if(string.IsNullOrEmpty(scheduledResource.UserName) || string.IsNullOrEmpty(scheduledResource.Password))
            {
                GetCredentialsCalled = true;
                scheduledResource.UserName = "some username";
                scheduledResource.Password = "some password";
            }
        }

        public override IScheduleTrigger ShowEditTriggerDialog()
        {
            DailyTrigger dailyTrigger = new DailyTrigger { StartBoundary = new DateTime(2013, 04, 01, 02, 21, 25) };
            return SchedulerFactory.CreateTrigger(TaskState.Disabled, new Dev2Trigger(null, dailyTrigger));
        }

        #endregion
    }
}
