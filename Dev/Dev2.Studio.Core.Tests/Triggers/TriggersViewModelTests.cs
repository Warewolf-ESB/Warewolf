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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Caliburn.Micro;
using CubicOrange.Windows.Forms.ActiveDirectory;
using Dev2.Common.Interfaces.Enums;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Common.Interfaces.Scheduler.Interfaces;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Common.Interfaces.Threading;
using Dev2.Communication;
using Dev2.Data.TO;
using Dev2.Dialogs;
using Dev2.Scheduler;
using Dev2.Services.Security;
using Dev2.Studio.Controller;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.Repositories;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Interfaces;
using Dev2.TaskScheduler.Wrappers;
using Dev2.Threading;
using Dev2.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32.TaskScheduler;
using Moq;
using Dev2.Studio.Interfaces.Enums;
using Dev2.Services.Events;
using Dev2.Triggers;
using Dev2.Triggers.Scheduler;

namespace Dev2.Core.Tests.Triggers
{
    [TestClass]
    public class TriggerViewModelTests
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
        public void TriggersViewModel_Constructor_NullPopupController_ThrowsArgumentNullException()
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
        public void TriggersViewModel_Constructor_NullAsyncWorker_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            new TriggersViewModel(new Mock<IEventAggregator>().Object, new Mock<IPopupController>().Object, null, new Mock<IServer>().Object, a => new Mock<IServer>().Object);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(TriggersViewModel))]
        public void TriggersViewModel_Constructor_Properties_Initialized()
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
            Assert.AreEqual("Triggers - TestServer", tasksViewModel.DisplayName);
            Assert.AreEqual("Queue Events", tasksViewModel.QueueEventsHeader);
            Assert.AreEqual("Scheduler", tasksViewModel.SchedulerHeader);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(TriggersViewModel))]
        public void TriggersViewModel_NewQueueEventCommand()
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
            tasksViewModel.NewScheduleCommand.Execute(null);

            Assert.IsTrue(foregroundWorkWasCalled);
            Assert.AreEqual(1, tasksViewModel.QueueEventsViewModel.QueueEvents.Count);
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(TriggersViewModel))]
        public void TriggersViewModel_NewScheduleCommand()
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
            //  tasksViewModel.SchedulerViewModel.SelectedTask = new System.Collections.ObjectModel.ObservableCollection<string>();
            tasksViewModel.NewScheduleCommand.Execute(null);

            Assert.IsTrue(foregroundWorkWasCalled);
            Assert.AreEqual(1, tasksViewModel.QueueEventsViewModel.QueueEvents.Count);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(TriggersViewModel))]
        public void TriggersViewModel_DoDeactivate_ShowMessage_False_Expect_True()
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
        public void TriggersViewModel_DoDeactivate_ShowMessage_True_Unauthorized_Expect_True()
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
        public void TriggersViewModel_DoDeactivate_ShowMessage_True_Expect_True()
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
        public void TriggersViewModel_DoDeactivate_ShowMessage_True_Cancel_MessageBox_Expect_False()
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
        public void TriggersViewModel_DoDeactivate_ShowMessage_True_None_MessageBox_Expect_False()
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
        public void TriggersViewModel_DoDeactivate_ShowMessage_True_No_MessageBox_Expect_False()
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
        public void TriggersViewModel_DoDeactivate_ShowMessage_True_Yes_MessageBox_Expect_False()
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