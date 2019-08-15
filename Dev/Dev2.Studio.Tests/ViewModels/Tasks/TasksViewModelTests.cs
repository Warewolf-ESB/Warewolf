/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Caliburn.Micro;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Common.Interfaces.Threading;
using Dev2.Services.Security;
using Dev2.Studio.Core;
using Dev2.Studio.Interfaces;
using Dev2.Triggers;
using Dev2.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace Dev2.Studio.Tests.ViewModels.Tasks
{
    [TestClass]
    public class TasksViewModelTests
    {
        [TestInitialize]
        public void SetupForTest()
        {
            var mockShellViewModel = new Mock<IShellViewModel>();
            var mockServer = new Mock<IServer>();
            mockServer.Setup(a => a.DisplayName).Returns("Localhost");
            mockShellViewModel.Setup(x => x.LocalhostServer).Returns(mockServer.Object);

            CustomContainer.Register(mockShellViewModel.Object);
            CustomContainer.Register(new Mock<IEventAggregator>().Object);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(TriggersViewModel))]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TasksViewModel_Constructor_NullPopupController_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            new TriggersViewModel(new Mock<IEventAggregator>().Object, null, null, new Mock<IServer>().Object, a => new Mock<IServer>().Object);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(TriggersViewModel))]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TasksViewModel_Constructor_NullAsyncWorker_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            new TriggersViewModel(new Mock<IEventAggregator>().Object, new Mock<IPopupController>().Object, null, new Mock<IServer>().Object, a => new Mock<IServer>().Object);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(TriggersViewModel))]
        public void TasksViewModel_Constructor_Properties_Initialized()
        {
            var mockEventAggregator = new Mock<IEventAggregator>();
            var mockPopupController = new Mock<IPopupController>();
            var mockAsyncWorker = new Mock<IAsyncWorker>();
            var mockServer = new Mock<IServer>();
            mockServer.Setup(server => server.DisplayName).Returns("TestServer");

            var mockEnvironment = new Mock<IServer>();
            mockEnvironment.Setup(server => server.DisplayName).Returns("TestEnvironment");

            var tasksViewModel = new TriggersViewModel(mockEventAggregator.Object, mockPopupController.Object, mockAsyncWorker.Object, mockServer.Object, a =>
            {
                return mockEnvironment.Object;
            });

            Assert.AreEqual(mockServer.Object, tasksViewModel.Server);
            Assert.AreEqual(mockEnvironment.Object, tasksViewModel.CurrentEnvironment);
            Assert.AreEqual("Tasks - TestServer", tasksViewModel.DisplayName);
            Assert.AreEqual("Queue Events", tasksViewModel.QueueEventsHeader);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(TriggersViewModel))]
        public void TasksViewModel_NewQueueEventCommand()
        {
            var foregroundWorkWasCalled = false;

            var mockEventAggregator = new Mock<IEventAggregator>();
            var mockPopupController = new Mock<IPopupController>();
            var mockAsyncWorker = new Mock<IAsyncWorker>();
            var asyncWorker = new SynchronousAsyncWorker();
            asyncWorker.Start(() => { },
                () =>
                {
                    foregroundWorkWasCalled = true;
                });

            var mockServer = new Mock<IServer>();

            var tasksViewModel = new TriggersViewModel(mockEventAggregator.Object, mockPopupController.Object, asyncWorker, mockServer.Object, a => new Mock<IServer>().Object);
            tasksViewModel.QueueEventsViewModel.QueueEvents = new System.Collections.ObjectModel.ObservableCollection<string>();
            tasksViewModel.NewQueueEventCommand.Execute(null);

            Assert.IsTrue(foregroundWorkWasCalled);
            Assert.AreEqual(1, tasksViewModel.QueueEventsViewModel.QueueEvents.Count);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(TriggersViewModel))]
        public void TasksViewModel_DoDeactivate_ShowMessage_False_Expect_True()
        {
            var mockEventAggregator = new Mock<IEventAggregator>();
            var mockPopupController = new Mock<IPopupController>();
            mockPopupController.Setup(popupController => popupController.ShowSaveServerNotReachableErrorMsg()).Returns(System.Windows.MessageBoxResult.OK);
            var mockAsyncWorker = new Mock<IAsyncWorker>();
            var mockServer = new Mock<IServer>();
            mockServer.Setup(server => server.DisplayName).Returns("TestServer");

            var mockEnvironment = new Mock<IServer>();
            mockEnvironment.Setup(server => server.DisplayName).Returns("TestEnvironment");

            var tasksViewModel = new TriggersViewModel(mockEventAggregator.Object, mockPopupController.Object, mockAsyncWorker.Object, mockServer.Object, a =>
            {
                return mockEnvironment.Object;
            });

            var value = tasksViewModel.DoDeactivate(false);
            Assert.IsFalse(value);
            Assert.IsTrue(tasksViewModel.HasErrors);
            Assert.AreEqual(StringResources.SaveServerNotReachableErrorMsg, tasksViewModel.Errors);
            mockPopupController.Verify(popupController => popupController.ShowSaveServerNotReachableErrorMsg(), Times.Once);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(TriggersViewModel))]
        public void TasksViewModel_DoDeactivate_ShowMessage_True_Unauthorized_Expect_True()
        {
            var mockEventAggregator = new Mock<IEventAggregator>();
            var mockPopupController = new Mock<IPopupController>();
            mockPopupController.Setup(popupController => popupController.ShowSaveSettingsPermissionsErrorMsg()).Returns(System.Windows.MessageBoxResult.OK);
            var mockAsyncWorker = new Mock<IAsyncWorker>();
            var mockServer = new Mock<IServer>();
            mockServer.Setup(server => server.DisplayName).Returns("TestServer");

            var mockAuthorizationService = new Mock<IAuthorizationService>();
            mockAuthorizationService.Setup(authorizationService => authorizationService.IsAuthorized(Common.Interfaces.Enums.AuthorizationContext.Administrator, null)).Returns(false);

            var mockEnvironment = new Mock<IServer>();
            mockEnvironment.Setup(server => server.DisplayName).Returns("TestEnvironment");
            mockEnvironment.Setup(server => server.IsConnected).Returns(true);
            mockEnvironment.Setup(server => server.AuthorizationService).Returns(mockAuthorizationService.Object);

            var tasksViewModel = new TriggersViewModel(mockEventAggregator.Object, mockPopupController.Object, mockAsyncWorker.Object, mockServer.Object, a =>
            {
                return mockEnvironment.Object;
            });

            var value = tasksViewModel.DoDeactivate(false);
            Assert.IsFalse(value);
            Assert.IsTrue(tasksViewModel.HasErrors);
            Assert.AreEqual(StringResources.SaveSettingsPermissionsErrorMsg, tasksViewModel.Errors);
            mockPopupController.Verify(popupController => popupController.ShowSaveSettingsPermissionsErrorMsg(), Times.Once);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(TriggersViewModel))]
        public void TasksViewModel_DoDeactivate_ShowMessage_True_Expect_True()
        {
            var mockEventAggregator = new Mock<IEventAggregator>();
            var mockPopupController = new Mock<IPopupController>();
            var mockAsyncWorker = new Mock<IAsyncWorker>();
            var mockServer = new Mock<IServer>();
            mockServer.Setup(server => server.DisplayName).Returns("TestServer");

            var mockAuthorizationService = new Mock<IAuthorizationService>();
            mockAuthorizationService.Setup(authorizationService => authorizationService.IsAuthorized(Common.Interfaces.Enums.AuthorizationContext.Administrator, null)).Returns(true);

            var mockEnvironment = new Mock<IServer>();
            mockEnvironment.Setup(server => server.DisplayName).Returns("TestEnvironment");
            mockEnvironment.Setup(server => server.IsConnected).Returns(true);
            mockEnvironment.Setup(server => server.AuthorizationService).Returns(mockAuthorizationService.Object);

            var tasksViewModel = new TriggersViewModel(mockEventAggregator.Object, mockPopupController.Object, mockAsyncWorker.Object, mockServer.Object, a =>
            {
                return mockEnvironment.Object;
            });

            var value = tasksViewModel.DoDeactivate(false);
            Assert.IsTrue(value);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(TriggersViewModel))]
        public void TasksViewModel_DoDeactivate_ShowMessage_True_Cancel_MessageBox_Expect_False()
        {
            var mockEventAggregator = new Mock<IEventAggregator>();
            var mockPopupController = new Mock<IPopupController>();
            mockPopupController.Setup(popupController => popupController.ShowTasksCloseConfirmation()).Returns(System.Windows.MessageBoxResult.Cancel);
            var mockAsyncWorker = new Mock<IAsyncWorker>();
            var mockServer = new Mock<IServer>();
            mockServer.Setup(server => server.DisplayName).Returns("TestServer");

            var mockAuthorizationService = new Mock<IAuthorizationService>();
            mockAuthorizationService.Setup(authorizationService => authorizationService.IsAuthorized(Common.Interfaces.Enums.AuthorizationContext.Administrator, null)).Returns(true);

            var mockEnvironment = new Mock<IServer>();
            mockEnvironment.Setup(server => server.DisplayName).Returns("TestEnvironment");
            mockEnvironment.Setup(server => server.IsConnected).Returns(true);
            mockEnvironment.Setup(server => server.AuthorizationService).Returns(mockAuthorizationService.Object);

            var tasksViewModel = new TriggersViewModel(mockEventAggregator.Object, mockPopupController.Object, mockAsyncWorker.Object, mockServer.Object, a =>
            {
                return mockEnvironment.Object;
            });
            tasksViewModel.IsDirty = true;

            var value = tasksViewModel.DoDeactivate(true);
            Assert.IsFalse(value);
            mockPopupController.Verify(popupController => popupController.ShowTasksCloseConfirmation(), Times.Once);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(TriggersViewModel))]
        public void TasksViewModel_DoDeactivate_ShowMessage_True_None_MessageBox_Expect_False()
        {
            var mockEventAggregator = new Mock<IEventAggregator>();
            var mockPopupController = new Mock<IPopupController>();
            mockPopupController.Setup(popupController => popupController.ShowTasksCloseConfirmation()).Returns(System.Windows.MessageBoxResult.None);
            var mockAsyncWorker = new Mock<IAsyncWorker>();
            var mockServer = new Mock<IServer>();
            mockServer.Setup(server => server.DisplayName).Returns("TestServer");

            var mockAuthorizationService = new Mock<IAuthorizationService>();
            mockAuthorizationService.Setup(authorizationService => authorizationService.IsAuthorized(Common.Interfaces.Enums.AuthorizationContext.Administrator, null)).Returns(true);

            var mockEnvironment = new Mock<IServer>();
            mockEnvironment.Setup(server => server.DisplayName).Returns("TestEnvironment");
            mockEnvironment.Setup(server => server.IsConnected).Returns(true);
            mockEnvironment.Setup(server => server.AuthorizationService).Returns(mockAuthorizationService.Object);

            var tasksViewModel = new TriggersViewModel(mockEventAggregator.Object, mockPopupController.Object, mockAsyncWorker.Object, mockServer.Object, a =>
            {
                return mockEnvironment.Object;
            });
            tasksViewModel.IsDirty = true;

            var value = tasksViewModel.DoDeactivate(true);
            Assert.IsFalse(value);
            mockPopupController.Verify(popupController => popupController.ShowTasksCloseConfirmation(), Times.Once);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(TriggersViewModel))]
        public void TasksViewModel_DoDeactivate_ShowMessage_True_No_MessageBox_Expect_False()
        {
            var mockEventAggregator = new Mock<IEventAggregator>();
            var mockPopupController = new Mock<IPopupController>();
            mockPopupController.Setup(popupController => popupController.ShowTasksCloseConfirmation()).Returns(System.Windows.MessageBoxResult.No);

            var asyncWorker = new SynchronousAsyncWorker();
            asyncWorker.Start(() => { }, () => { });

            var mockServer = new Mock<IServer>();
            mockServer.Setup(server => server.DisplayName).Returns("TestServer");

            var mockAuthorizationService = new Mock<IAuthorizationService>();
            mockAuthorizationService.Setup(authorizationService => authorizationService.IsAuthorized(Common.Interfaces.Enums.AuthorizationContext.Administrator, null)).Returns(true);

            var mockEnvironment = new Mock<IServer>();
            mockEnvironment.Setup(server => server.DisplayName).Returns("TestEnvironment");
            mockEnvironment.Setup(server => server.IsConnected).Returns(true);
            mockEnvironment.Setup(server => server.AuthorizationService).Returns(mockAuthorizationService.Object);

            var tasksViewModel = new TriggersViewModel(mockEventAggregator.Object, mockPopupController.Object, asyncWorker, mockServer.Object, a =>
            {
                return mockEnvironment.Object;
            })
            {
                IsDirty = true
            };

            var value = tasksViewModel.DoDeactivate(true);
            Assert.IsTrue(value);
            Assert.IsFalse(tasksViewModel.IsDirty);
            Assert.IsFalse(tasksViewModel.QueueEventsViewModel.IsDirty);
            mockPopupController.Verify(popupController => popupController.ShowTasksCloseConfirmation(), Times.Once);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(TriggersViewModel))]
        public void TasksViewModel_DoDeactivate_ShowMessage_True_Yes_MessageBox_Expect_False()
        {
            var mockEventAggregator = new Mock<IEventAggregator>();
            var mockPopupController = new Mock<IPopupController>();
            mockPopupController.Setup(popupController => popupController.ShowTasksCloseConfirmation()).Returns(System.Windows.MessageBoxResult.Yes);
            var mockAsyncWorker = new Mock<IAsyncWorker>();
            var mockServer = new Mock<IServer>();
            mockServer.Setup(server => server.DisplayName).Returns("TestServer");

            var mockAuthorizationService = new Mock<IAuthorizationService>();
            mockAuthorizationService.Setup(authorizationService => authorizationService.IsAuthorized(Common.Interfaces.Enums.AuthorizationContext.Administrator, null)).Returns(true);

            var mockEnvironment = new Mock<IServer>();
            mockEnvironment.Setup(server => server.DisplayName).Returns("TestEnvironment");
            mockEnvironment.Setup(server => server.IsConnected).Returns(true);
            mockEnvironment.Setup(server => server.AuthorizationService).Returns(mockAuthorizationService.Object);

            var tasksViewModel = new TriggersViewModel(mockEventAggregator.Object, mockPopupController.Object, mockAsyncWorker.Object, mockServer.Object, a =>
            {
                return mockEnvironment.Object;
            });
            tasksViewModel.IsDirty = true;

            var value = tasksViewModel.DoDeactivate(true);
            Assert.IsTrue(value);
            Assert.IsFalse(tasksViewModel.IsDirty);
            Assert.IsTrue(tasksViewModel.IsSaved);
            Assert.IsTrue(tasksViewModel.IsSavedSuccessVisible);
            Assert.IsFalse(tasksViewModel.IsErrorsVisible);
            mockPopupController.Verify(popupController => popupController.ShowTasksCloseConfirmation(), Times.Once);
        }
    }
}
