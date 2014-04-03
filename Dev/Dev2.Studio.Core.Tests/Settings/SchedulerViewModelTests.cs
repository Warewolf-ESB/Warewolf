using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Caliburn.Micro;
using CubicOrange.Windows.Forms.ActiveDirectory;
using Dev2.AppResources.Enums;
using Dev2.Communication;
using Dev2.Dialogs;
using Dev2.Messages;
using Dev2.Scheduler;
using Dev2.Scheduler.Interfaces;
using Dev2.Services.Events;
using Dev2.Services.Security;
using Dev2.Settings.Scheduler;
using Dev2.Studio.Controller;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.AppResources.Repositories;
using Dev2.Studio.Core.Controller;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.TaskScheduler.Wrappers;
using Dev2.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32.TaskScheduler;
using Moq;
using System.Linq;

// ReSharper disable InconsistentNaming
namespace Dev2.Core.Tests.Settings
{
    [TestClass]
    public class SchedulerViewModelTests
    {


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SchedulerViewModel_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SchedulerViewModel_Constructor_NullEventAggregator_Exception()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            new SchedulerViewModel(null, new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IPopupController>().Object, new TestAsyncWorker());
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
            new SchedulerViewModel(new Mock<IEventAggregator>().Object, null, new Mock<IPopupController>().Object, new TestAsyncWorker());
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
            new SchedulerViewModel(new Mock<IEventAggregator>().Object, new Mock<DirectoryObjectPickerDialog>().Object, null, new TestAsyncWorker());
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
            Assert.IsNotNull(schdulerViewModel.ServerChangedCommand);
            Assert.IsTrue(schdulerViewModel.IsSaveEnabled);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SchedulerViewModel_HandleServerSelectionChangedMessage")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SchedulerViewModel_HandleServerSelectionChangedMessage_NullEnvironmentModelOnMessage_Exception()
        {
            //------------Setup for test--------------------------
            var message = new ServerSelectionChangedMessage(null, ConnectControlInstanceType.Scheduler);
            var schedulerViewModel = new SchedulerViewModel();

            //------------Execute Test---------------------------
            schedulerViewModel.Handle(message);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SchedulerViewModel_HandleServerSelectionChangeMessage")]
        public void SchedulerViewModel_HandleSetActiveEnvironmentMessage_WithValidEnvironmentModel_SetsScheduledResourceModel()
        {
            //------------Setup for test--------------------------
            var resources = new ObservableCollection<IScheduledResource>();
            var scheduledResourceForTest = new ScheduledResourceForTest();
            scheduledResourceForTest.Trigger = new ScheduleTrigger(TaskState.Ready, new Dev2DailyTrigger(new TaskServiceConvertorFactory(), new DailyTrigger()), new Dev2TaskService(new TaskServiceConvertorFactory()), new TaskServiceConvertorFactory());
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
            mockEnvironmentModel.Setup(c => c.AuthorizationService.IsAuthorized(It.IsAny<AuthorizationContext>(), null)).Returns(true);
            ResourceRepository resourceRepo = new ResourceRepository(mockEnvironmentModel.Object);
            var setupResourceModelMock = Dev2MockFactory.SetupResourceModelMock(ResourceType.WorkflowService, "TestFlow2");
            resourceRepo.Add(Dev2MockFactory.SetupResourceModelMock(ResourceType.WorkflowService, "TestFlow1").Object);
            resourceRepo.Add(setupResourceModelMock.Object);
            resourceRepo.Add(Dev2MockFactory.SetupResourceModelMock(ResourceType.WorkflowService, "TestFlow3").Object);
            mockEnvironmentModel.Setup(c => c.ResourceRepository).Returns(resourceRepo);
            var message = new ServerSelectionChangedMessage(mockEnvironmentModel.Object, ConnectControlInstanceType.Scheduler);
            var schedulerViewModel = new SchedulerViewModel(new Mock<IEventAggregator>().Object, new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IPopupController>().Object, new TestAsyncWorker());

            //------------Execute Test---------------------------
            schedulerViewModel.Handle(message);
            //------------Assert Results-------------------------
            Assert.IsNotNull(schedulerViewModel.CurrentEnvironment);
            Assert.IsNotNull(schedulerViewModel.ScheduledResourceModel);
            Assert.AreEqual(1, schedulerViewModel.ScheduledResourceModel.ScheduledResources.Count);
            Assert.IsNotNull(schedulerViewModel.SelectedTask);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("SchedulerViewModel_HandleServerSelectionChangeMessage")]
        public void SchedulerViewModel_HandleSetActiveEnvironmentMessage_WithValidEnvironmentModelWithWrongPermissions_SetsConnectionError()
        {
            //------------Setup for test--------------------------
            var resources = new ObservableCollection<IScheduledResource>();
            var scheduledResourceForTest = new ScheduledResourceForTest();
            scheduledResourceForTest.Trigger = new ScheduleTrigger(TaskState.Ready, new Dev2DailyTrigger(new TaskServiceConvertorFactory(), new DailyTrigger()), new Dev2TaskService(new TaskServiceConvertorFactory()), new TaskServiceConvertorFactory());
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
            mockEnvironmentModel.Setup(c => c.AuthorizationService.IsAuthorized(It.IsAny<AuthorizationContext>(), null)).Returns(false);
            var message = new ServerSelectionChangedMessage(mockEnvironmentModel.Object, ConnectControlInstanceType.Scheduler);
            var schedulerViewModel = new SchedulerViewModel(new Mock<IEventAggregator>().Object, new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IPopupController>().Object, new TestAsyncWorker());

            //------------Execute Test---------------------------
            schedulerViewModel.Handle(message);
            //------------Assert Results-------------------------
            Assert.IsTrue(schedulerViewModel.HasConnectionError);
            Assert.AreEqual(@"You don't have permission to schedule on this server.
You need Administrator permission.", schedulerViewModel.ConnectionError);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SchedulerViewModel_SaveCommand")]
        public void SchedulerViewModel_SaveCommand_UserNamePasswordNotSet_CallsGetCredentials()
        {
            //------------Setup for test--------------------------
            var resources = new ObservableCollection<IScheduledResource>();
            var scheduledResourceForTest = new ScheduledResourceForTest();
            scheduledResourceForTest.IsDirty = true;
            resources.Add(scheduledResourceForTest);
            var schedulerViewModel = new SchedulerViewModelForTest();
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
        public void SchedulerViewModel_SaveCommand_UserNamePasswordSet_CallsScheduledResourceModelSave()
        {
            //------------Setup for test--------------------------
            var resources = new ObservableCollection<IScheduledResource>();
            var scheduledResourceForTest = new ScheduledResourceForTest();
            scheduledResourceForTest.IsDirty = true;
            resources.Add(scheduledResourceForTest);
            scheduledResourceForTest.UserName = "some user";
            scheduledResourceForTest.Password = "some password";
            var schedulerViewModel = new SchedulerViewModelForTest();
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
            var agg = new Mock<IEventAggregator>();
            agg.Setup(a=>a.Publish(It.IsAny<DebugOutputMessage>())).Verifiable();
            var schedulerViewModel = new SchedulerViewModel(agg.Object, new DirectoryObjectPickerDialog(), new PopupController(), new AsyncWorker());
           
       
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
            agg.Verify(a=>a.Publish(It.IsAny<DebugOutputMessage>()));
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
            var schedulerViewModel = new SchedulerViewModel();
            var activeItem = new TabItem();
            activeItem.Header = "History";
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
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SchedulerViewModel_Name")]
        public void SchedulerViewModel_Name_EmptyString_AddsErrorMessage()
        {
            //------------Setup for test--------------------------

            var schedulerViewModel = new SchedulerViewModel();
            var scheduledResourceForTest = new ScheduledResourceForTest();
            scheduledResourceForTest.Name = "Test";
            schedulerViewModel.SelectedTask = scheduledResourceForTest;

            //------------Execute Test---------------------------
            schedulerViewModel.Name = "";
            //------------Assert Results-------------------------
            Assert.IsTrue(schedulerViewModel.HasErrors);
            Assert.AreEqual("The name can not be blank", schedulerViewModel.Error);
            Assert.IsFalse(schedulerViewModel.IsSaveEnabled);
            Assert.AreEqual(string.Empty, schedulerViewModel.SelectedTask.Name);
            Assert.IsTrue(schedulerViewModel.SelectedTask.IsDirty);

        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SchedulerViewModel_Name")]
        public void SchedulerViewModel_Name_WasEmptyStringValidString_ClearsErrorMessage()
        {
            //------------Setup for test--------------------------

            var schedulerViewModel = new SchedulerViewModel();
            var scheduledResourceForTest = new ScheduledResourceForTest();
            scheduledResourceForTest.Name = "Test";
            schedulerViewModel.SelectedTask = scheduledResourceForTest;
            schedulerViewModel.Name = "";

            //------------------Assert Preconditions---------------------------
            Assert.IsTrue(schedulerViewModel.HasErrors);
            Assert.AreEqual("The name can not be blank", schedulerViewModel.Error);
            Assert.IsFalse(schedulerViewModel.IsSaveEnabled);
            //------------Execute Test---------------------------
            schedulerViewModel.Name = "This is a test";
            //------------Assert Results-------------------------
            Assert.IsFalse(schedulerViewModel.HasErrors);
            Assert.AreEqual(string.Empty, schedulerViewModel.Error);
            Assert.IsTrue(schedulerViewModel.IsSaveEnabled);
            Assert.AreEqual("This is a test", schedulerViewModel.SelectedTask.Name);
            Assert.IsTrue(schedulerViewModel.SelectedTask.IsDirty);

        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("SchedulerViewModel_WorkflowName")]
        public void SchedulerViewModel_WorkflowName_BlankName_SetsError()
        {
            //------------Setup for test--------------------------

            var schedulerViewModel = new SchedulerViewModel();
            var scheduledResourceForTest = new ScheduledResourceForTest();
            scheduledResourceForTest.WorkflowName = "Test";
            schedulerViewModel.SelectedTask = scheduledResourceForTest;

            //------------Execute Test---------------------------
            schedulerViewModel.WorkflowName = "";
            //------------Assert Results-------------------------
            Assert.IsTrue(schedulerViewModel.HasErrors);
            Assert.AreEqual("Please select a workflow to schedule", schedulerViewModel.Error);
            Assert.IsFalse(schedulerViewModel.IsSaveEnabled);
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
            var scheduledResourceForTest = new ScheduledResourceForTest();
            scheduledResourceForTest.Name = "Test";
            var scheduledResourceForTest2 = new ScheduledResourceForTest();
            scheduledResourceForTest2.Name = "Test2";
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
            Assert.IsFalse(schedulerViewModel.IsSaveEnabled);
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
            var scheduledResourceForTest = new ScheduledResourceForTest();
            scheduledResourceForTest.Name = "Test";
            var scheduledResourceForTest2 = new ScheduledResourceForTest();
            scheduledResourceForTest2.Name = "Test2";
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
            Assert.IsFalse(schedulerViewModel.IsSaveEnabled);
            Assert.AreEqual("Test2", schedulerViewModel.SelectedTask.Name);
            Assert.IsTrue(schedulerViewModel.SelectedTask.IsDirty);

            //------------Execute Test---------------------------
            schedulerViewModel.Name = "Test Some";
            //------------Assert Results-------------------------
            Assert.IsFalse(schedulerViewModel.HasErrors);
            Assert.AreEqual(string.Empty, schedulerViewModel.Error);
            Assert.IsTrue(schedulerViewModel.IsSaveEnabled);
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
            var scheduledResourceForTest = new ScheduledResourceForTest();
            scheduledResourceForTest.Name = "Test";
            var scheduledResourceForTest2 = new ScheduledResourceForTest();
            scheduledResourceForTest2.Name = "Test2";
            scheduledResourceForTest.IsDirty = true;
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
            var schedulerViewModel = new SchedulerViewModel(new Mock<IEventAggregator>().Object, new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IPopupController>().Object, new TestAsyncWorker());
            var activeItem = new TabItem();
            activeItem.Header = "History";
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
            var activeItem = new TabItem();
            activeItem.Header = "Settings";
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
            var activeItem = new TabItem();
            activeItem.Header = "History";
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
            var resources = new ObservableCollection<IScheduledResource>();
            resources.Add(new ScheduledResource("bob", SchedulerStatus.Enabled, DateTime.MaxValue, new Mock<IScheduleTrigger>().Object, "c"));
            resources.Add(new ScheduledResource("dave", SchedulerStatus.Enabled, DateTime.MaxValue, new Mock<IScheduleTrigger>().Object, "c"));
            resourceModel.Setup(a => a.ScheduledResources).Returns(resources);

            var schedulerViewModel = new SchedulerViewModel()
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
            var resources = new ObservableCollection<IScheduledResource>();
            resources.Add(new ScheduledResource("bob", SchedulerStatus.Enabled, DateTime.MaxValue, new Mock<IScheduleTrigger>().Object, "c"));
            resources.Add(new ScheduledResource("dave", SchedulerStatus.Enabled, DateTime.MaxValue, new Mock<IScheduleTrigger>().Object, "c"));
            resourceModel.Setup(a => a.ScheduledResources).Returns(resources);

            var schedulerViewModel = new SchedulerViewModel()
            {
                ScheduledResourceModel = resourceModel.Object
            };
            // validation occurs on property changes
            schedulerViewModel.SelectedTask = resources[0];
            schedulerViewModel.Name = "monkeys";
            Assert.IsFalse(schedulerViewModel.HasErrors);

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("SchedulerViewModel_Validation")]
        public void SchedulerViewModel_Validation_IfDuplicateNames()
        {
            var resourceModel = new Mock<IScheduledResourceModel>();
            var resources = new ObservableCollection<IScheduledResource>();
            resources.Add(new ScheduledResource("bob", SchedulerStatus.Enabled, DateTime.MaxValue, new Mock<IScheduleTrigger>().Object, "c") { NumberOfHistoryToKeep = 1 });
            resources.Add(new ScheduledResource("dave", SchedulerStatus.Enabled, DateTime.MaxValue, new Mock<IScheduleTrigger>().Object, "c"));
            resourceModel.Setup(a => a.ScheduledResources).Returns(resources);

            var schedulerViewModel = new SchedulerViewModel
                {
                    ScheduledResourceModel = resourceModel.Object
                };
            //create duplicate name
            schedulerViewModel.SelectedTask = resources[0];
            schedulerViewModel.Name = "dave";
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
            var schedulerViewModel = new SchedulerViewModel(new Mock<IEventAggregator>().Object, new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IPopupController>().Object, new TestAsyncWorker());
            var resources = new ObservableCollection<IScheduledResource>();
            resources.Add(new ScheduledResource("bob", SchedulerStatus.Enabled, DateTime.MaxValue, new Mock<IScheduleTrigger>().Object, "c") { NumberOfHistoryToKeep = 1 });
            resources.Add(new ScheduledResource("dave", SchedulerStatus.Enabled, DateTime.MaxValue, new Mock<IScheduleTrigger>().Object, "c"));

            var resourceModel = new Mock<IScheduledResourceModel>();
            resourceModel.Setup(c => c.ScheduledResources).Returns(resources);
            schedulerViewModel.ScheduledResourceModel = resourceModel.Object;
            //------------Execute Test---------------------------

            Assert.AreEqual(2, schedulerViewModel.TaskList.Count);

            schedulerViewModel.NewCommand.Execute(null);
            //------------Assert Results-------------------------
            Assert.AreEqual(3, schedulerViewModel.TaskList.Count);
            Assert.AreEqual("New Task1", schedulerViewModel.TaskList[2].Name);
            Assert.AreEqual(SchedulerStatus.Enabled, schedulerViewModel.TaskList[2].Status);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("SchedulerViewModel_DeleteTask")]
        public void SchedulerViewModel_DeleteTask_ShouldDeleteTaskFromList()
        {
            //------------Setup for test--------------------------

            Mock<IPopupController> mockPopUpController = new Mock<IPopupController>();
            mockPopUpController.Setup(c => c.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            var schedulerViewModel = new SchedulerViewModel(new Mock<IEventAggregator>().Object, new Mock<DirectoryObjectPickerDialog>().Object, mockPopUpController.Object, new TestAsyncWorker());
            var resources = new ObservableCollection<IScheduledResource>();
            resources.Add(new ScheduledResource("bob", SchedulerStatus.Enabled, DateTime.MaxValue, new Mock<IScheduleTrigger>().Object, "c") { NumberOfHistoryToKeep = 1 });
            resources.Add(new ScheduledResource("dave", SchedulerStatus.Enabled, DateTime.MaxValue, new Mock<IScheduleTrigger>().Object, "c"));

            var resourceModel = new Mock<IScheduledResourceModel>();
            resourceModel.Setup(c => c.ScheduledResources).Returns(resources);
            schedulerViewModel.ScheduledResourceModel = resourceModel.Object;
            //------------Execute Test---------------------------
            Assert.AreEqual(2, schedulerViewModel.TaskList.Count);
            schedulerViewModel.SelectedTask = schedulerViewModel.TaskList[1];

            schedulerViewModel.DeleteCommand.Execute(null);
            //------------Assert Results-------------------------
            Assert.AreEqual(1, schedulerViewModel.TaskList.Count);
            Assert.AreEqual("bob", schedulerViewModel.TaskList[0].Name);
            Assert.AreEqual(SchedulerStatus.Enabled, schedulerViewModel.TaskList[0].Status);
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
            schedulerViewModel.EditTriggerCommand.Execute(null);
            //------------Assert Results-------------------------
            Assert.AreEqual("bob", schedulerViewModel.SelectedTask.Name);
            Assert.AreEqual(2013, schedulerViewModel.SelectedTask.NextRunDate.Year);
            Assert.AreEqual(02, schedulerViewModel.SelectedTask.NextRunDate.Hour);
            Assert.AreEqual(21, schedulerViewModel.SelectedTask.NextRunDate.Minute);
            Assert.IsTrue( schedulerViewModel.TriggerText.StartsWith("At"));
            Assert.IsTrue(schedulerViewModel.TriggerText.EndsWith("AM every day"));
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SchedulerViewModel_HandleServerSelectionChangeMessage")]
        public void SchedulerViewModel_HandleSetActiveEnvironmentMessage_WithValidEnvironmentModelNotConnected_CallsClearViewModel()
        {
            //------------Setup for test--------------------------
            var resources = new ObservableCollection<IScheduledResource>();
            var scheduledResourceForTest = new ScheduledResourceForTest();
            resources.Add(scheduledResourceForTest);
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            var serializeObject = serializer.SerializeToBuilder(resources);
            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            var mockConnection = new Mock<IEnvironmentConnection>();
            mockConnection.Setup(connection => connection.IsConnected).Returns(false);
            mockConnection.Setup(connection => connection.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(serializeObject);
            mockConnection.Setup(connection => connection.WorkspaceID).Returns(Guid.NewGuid());
            mockEnvironmentModel.Setup(model => model.Connection).Returns(mockConnection.Object);
            mockEnvironmentModel.Setup(model => model.IsConnected).Returns(false);
            var message = new ServerSelectionChangedMessage(mockEnvironmentModel.Object, ConnectControlInstanceType.Scheduler);
            var schedulerViewModel = new SchedulerViewModel(new Mock<IEventAggregator>().Object, new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IPopupController>().Object, new TestAsyncWorker());

            //------------Execute Test---------------------------
            schedulerViewModel.Handle(message);
            //------------Assert Results-------------------------            
            Assert.IsNotNull(schedulerViewModel.CurrentEnvironment);
            Assert.AreEqual(string.Empty, schedulerViewModel.Name);
            Assert.AreEqual(string.Empty, schedulerViewModel.WorkflowName);
            Assert.AreEqual(SchedulerStatus.Enabled, schedulerViewModel.Status);
            Assert.AreEqual(string.Empty, schedulerViewModel.AccountName);
            Assert.AreEqual(string.Empty, schedulerViewModel.NumberOfRecordsToKeep);
            Assert.IsNull(schedulerViewModel.SelectedTask);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SchedulerViewModel_AddWorkflow")]
        public void SchedulerViewModel_AddWorkflow_WithNewTaskNameSet_WorkflowNameChangedAndNameChanged()
        {
            //------------Setup for test--------------------------
            var resources = new ObservableCollection<IScheduledResource>();
            resources.Add(new ScheduledResource("New Task1", SchedulerStatus.Enabled, DateTime.MaxValue, new Mock<IScheduleTrigger>().Object, "TestFlow1") { NumberOfHistoryToKeep = 1 });
            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            var mockConnection = new Mock<IEnvironmentConnection>();
            mockConnection.Setup(connection => connection.IsConnected).Returns(true);
            mockConnection.Setup(connection => connection.WorkspaceID).Returns(Guid.NewGuid());
            mockEnvironmentModel.Setup(model => model.Connection).Returns(mockConnection.Object);
            mockEnvironmentModel.Setup(model => model.IsConnected).Returns(true);
            ResourceRepository resourceRepo = new ResourceRepository(mockEnvironmentModel.Object);
            var setupResourceModelMock = Dev2MockFactory.SetupResourceModelMock(ResourceType.WorkflowService, "TestFlow2");
            resourceRepo.Add(Dev2MockFactory.SetupResourceModelMock(ResourceType.WorkflowService, "TestFlow1").Object);
            resourceRepo.Add(setupResourceModelMock.Object);
            resourceRepo.Add(Dev2MockFactory.SetupResourceModelMock(ResourceType.WorkflowService, "TestFlow3").Object);
            mockEnvironmentModel.Setup(c => c.ResourceRepository).Returns(resourceRepo);

            var schedulerViewModel = new SchedulerViewModel(new Mock<IEventAggregator>().Object, new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IPopupController>().Object, new TestAsyncWorker());
            schedulerViewModel.CurrentEnvironment = mockEnvironmentModel.Object;
            Mock<IResourcePickerDialog> mockResourcePickerDialog = new Mock<IResourcePickerDialog>();
            mockResourcePickerDialog.Setup(c => c.ShowDialog()).Returns(true);
            mockResourcePickerDialog.Setup(c => c.SelectedResource).Returns(setupResourceModelMock.Object);

            schedulerViewModel.ResourcePickerDialog = mockResourcePickerDialog.Object;

            Mock<IScheduledResourceModel> scheduledResourceModelMock = new Mock<IScheduledResourceModel>();
            scheduledResourceModelMock.Setup(c => c.ScheduledResources).Returns(resources);
            schedulerViewModel.ScheduledResourceModel = scheduledResourceModelMock.Object;
            //------------Execute Test---------------------------
            schedulerViewModel.SelectedTask = schedulerViewModel.TaskList[0];
            schedulerViewModel.AddWorkflowCommand.Execute(null);
            //------------Assert Results-------------------------            
            Assert.AreEqual("TestFlow2", schedulerViewModel.Name);
            Assert.AreEqual("TestFlow2", schedulerViewModel.WorkflowName);
        }

    }

    internal class SchedulerViewModelForTest : SchedulerViewModel
    {
        public SchedulerViewModelForTest()
        {

        }

        public SchedulerViewModelForTest(IEventAggregator eventPublisher, DirectoryObjectPickerDialog directoryObjectPicker, IPopupController popupController, IAsyncWorker asyncWorker)
            : base(eventPublisher, directoryObjectPicker, popupController, asyncWorker)
        {

        }
        #region Overrides of SchedulerViewModel

        public bool GetCredentialsCalled
        {
            get;
            private set;
        }

        public override void GetCredentials(IScheduledResource scheduledResource)
        {
            if(string.IsNullOrEmpty(scheduledResource.UserName) || string.IsNullOrEmpty(scheduledResource.UserName))
            {
                GetCredentialsCalled = true;
                scheduledResource.UserName = "some username";
                scheduledResource.Password = "some password";
            }
        }

        public override IScheduleTrigger ShowEditTriggerDialog()
        {
            DailyTrigger dailyTrigger = new DailyTrigger();
            dailyTrigger.StartBoundary = new DateTime(2013, 04, 01, 02, 21, 25);
            return SchedulerFactory.CreateTrigger(TaskState.Disabled, new Dev2Trigger(null, dailyTrigger));
        }

        #endregion
    }
}
