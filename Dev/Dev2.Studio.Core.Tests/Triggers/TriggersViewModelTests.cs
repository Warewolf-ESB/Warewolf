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
using Caliburn.Micro;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Common.Interfaces.Threading;
using Dev2.Services.Security;
using Dev2.Studio.Core;
using Dev2.Studio.Interfaces;
using Dev2.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Dev2.Triggers;
using Warewolf.Trigger.Queue;
using System.Collections.Generic;
using Warewolf.Triggers;

namespace Dev2.Core.Tests.Triggers
{
    [TestClass]
    public class TriggerViewModelTests
    {
        [TestInitialize]
        public void SetupForTest()
        {
            var mockExplorerToolTips = new Mock<IExplorerTooltips>();
            var mockShellViewModel = new Mock<IShellViewModel>();
            var mockServer = new Mock<IServer>();
            var mockServerRepository = new Mock<IServerRepository>();
            var mockResourceRepository = new Mock<IResourceRepository>();
            mockServer.Setup(a => a.DisplayName).Returns("Localhost");
            mockShellViewModel.Setup(x => x.LocalhostServer).Returns(mockServer.Object);
            mockServerRepository.Setup(sr => sr.All()).Returns(new List<IServer>());
            mockServer.Setup(s => s.ResourceRepository).Returns(mockResourceRepository.Object);
            CustomContainer.Register(mockServerRepository.Object);
            CustomContainer.Register(mockShellViewModel.Object);
            CustomContainer.Register(new Mock<IEventAggregator>().Object);
            CustomContainer.Register(mockExplorerToolTips.Object);
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

            var triggersViewModel = new TriggersViewModel(mockEventAggregator.Object, mockPopupController.Object, mockAsyncWorker.Object, mockServer.Object, a =>
            {
                return mockEnvironment.Object;
            });

            Assert.AreEqual(mockServer.Object, triggersViewModel.Server);
            Assert.AreEqual(mockEnvironment.Object, triggersViewModel.CurrentEnvironment);
            Assert.AreEqual("Triggers - TestServer", triggersViewModel.DisplayName);
            Assert.AreEqual("Queues", triggersViewModel.QueueEventsHeader);
            Assert.AreEqual("Scheduler", triggersViewModel.SchedulerHeader);
        }


        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(TriggersViewModel))]
        public void TriggersViewModel_IsEventsSelected_WhenSetToTrue_ShouldSetIsSchedulerToFalse()
        {
            var mockEventAggregator = new Mock<IEventAggregator>();
            var mockPopupController = new Mock<IPopupController>();
            var mockAsyncWorker = new Mock<IAsyncWorker>();
            var mockServer = new Mock<IServer>();
            mockServer.Setup(server => server.DisplayName).Returns("TestServer");

            var mockEnvironment = new Mock<IServer>();
            mockEnvironment.Setup(server => server.DisplayName).Returns("TestEnvironment");

            var triggersViewModel = new TriggersViewModel(mockEventAggregator.Object, mockPopupController.Object, mockAsyncWorker.Object, mockServer.Object, a =>
            {
                return mockEnvironment.Object;
            });
            triggersViewModel.IsEventsSelected = true;

            Assert.IsFalse(triggersViewModel.IsSchedulerSelected);
        }


        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(TriggersViewModel))]
        public void TriggersViewModel_IsSchedulerSelected_WhenSetToTrue_ShouldSetIsSchedulerToFalse()
        {
            var mockEventAggregator = new Mock<IEventAggregator>();
            var mockPopupController = new Mock<IPopupController>();
            var mockAsyncWorker = new Mock<IAsyncWorker>();
            var mockServer = new Mock<IServer>();
            mockServer.Setup(server => server.DisplayName).Returns("TestServer");

            var mockEnvironment = new Mock<IServer>();
            mockEnvironment.Setup(server => server.DisplayName).Returns("TestEnvironment");

            var triggersViewModel = new TriggersViewModel(mockEventAggregator.Object, mockPopupController.Object, mockAsyncWorker.Object, mockServer.Object, a =>
            {
                return mockEnvironment.Object;
            });
            triggersViewModel.IsSchedulerSelected = true;

            Assert.IsFalse(triggersViewModel.IsEventsSelected);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(TriggersViewModel))]
        public void TriggersViewModel_NewQueueEventCommand()
        {

            var triggersViewModel = CreateTriggerViewModel();
            triggersViewModel.NewQueueEventCommand.Execute(null);

            Assert.AreEqual(2, triggersViewModel.QueueEventsViewModel.Queues.Count); // The other item is the 'New Queue' item
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

            var triggersViewModel = new TriggersViewModel(mockEventAggregator.Object, mockPopupController.Object, mockAsyncWorker.Object, mockServer.Object, a =>
            {
                return mockEnvironment.Object;
            });

            var value = triggersViewModel.DoDeactivate(false);
            Assert.IsFalse(value);
            Assert.IsTrue(triggersViewModel.HasErrors);
            Assert.AreEqual(StringResources.SaveServerNotReachableErrorMsg, triggersViewModel.Errors);
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

            var triggersViewModel = new TriggersViewModel(mockEventAggregator.Object, mockPopupController.Object, mockAsyncWorker.Object, mockServer.Object, a =>
            {
                return mockEnvironment.Object;
            });

            var value = triggersViewModel.DoDeactivate(false);
            Assert.IsFalse(value);
            Assert.IsTrue(triggersViewModel.HasErrors);
            Assert.AreEqual(StringResources.SaveSettingsPermissionsErrorMsg, triggersViewModel.Errors);
            mockPopupController.Verify(popupController => popupController.ShowSaveSettingsPermissionsErrorMsg(), Times.Once);
        }

        public TriggersViewModel CreateTriggerViewModel()
        {
            var mockEventAggregator = new Mock<IEventAggregator>();
            var mockPopupController = new Mock<IPopupController>();
            var mockResourceRepository = new Mock<IResourceRepository>();
            mockResourceRepository.Setup(r => r.FetchTriggerQueues()).Returns(new List<ITriggerQueue>());
            var mockAsyncWorker = new Mock<IAsyncWorker>();
            var mockServer = new Mock<IServer>();
            mockServer.Setup(server => server.DisplayName).Returns("TestServer");

            var mockAuthorizationService = new Mock<IAuthorizationService>();
            mockAuthorizationService.Setup(authorizationService => authorizationService.IsAuthorized(Common.Interfaces.Enums.AuthorizationContext.Administrator, null)).Returns(true);

            var mockEnvironment = new Mock<IServer>();
            mockEnvironment.Setup(server => server.DisplayName).Returns("TestEnvironment");
            mockEnvironment.Setup(server => server.IsConnected).Returns(true);
            mockEnvironment.Setup(server => server.AuthorizationService).Returns(mockAuthorizationService.Object);
            mockEnvironment.Setup(server => server.ResourceRepository).Returns(mockResourceRepository.Object);

            var mockShellViewModel = new Mock<IShellViewModel>();
            mockShellViewModel.Setup(shellViewModel => shellViewModel.ActiveServer).Returns(mockEnvironment.Object);
            CustomContainer.Register(mockShellViewModel.Object);



            var asyncWorker = new SynchronousAsyncWorker();
            var triggersViewModel = new TriggersViewModel(mockEventAggregator.Object, mockPopupController.Object, asyncWorker, mockServer.Object, a =>
            {
                return mockEnvironment.Object;
            });
            return triggersViewModel;
        }

        public TriggersViewModel CreateTriggerViewModel(IPopupController popupController)
        {
            var mockEventAggregator = new Mock<IEventAggregator>();
            var mockResourceRepository = new Mock<IResourceRepository>();
            mockResourceRepository.Setup(r => r.FetchTriggerQueues()).Returns(new List<ITriggerQueue>());
            var mockAsyncWorker = new Mock<IAsyncWorker>();
            var mockServer = new Mock<IServer>();
            mockServer.Setup(server => server.DisplayName).Returns("TestServer");

            var mockAuthorizationService = new Mock<IAuthorizationService>();
            mockAuthorizationService.Setup(authorizationService => authorizationService.IsAuthorized(Common.Interfaces.Enums.AuthorizationContext.Administrator, null)).Returns(true);

            var mockEnvironment = new Mock<IServer>();
            mockEnvironment.Setup(server => server.DisplayName).Returns("TestEnvironment");
            mockEnvironment.Setup(server => server.IsConnected).Returns(true);
            mockEnvironment.Setup(server => server.AuthorizationService).Returns(mockAuthorizationService.Object);
            mockEnvironment.Setup(server => server.ResourceRepository).Returns(mockResourceRepository.Object);

            var mockShellViewModel = new Mock<IShellViewModel>();
            mockShellViewModel.Setup(shellViewModel => shellViewModel.ActiveServer).Returns(mockEnvironment.Object);
            CustomContainer.Register(mockShellViewModel.Object);



            var asyncWorker = new SynchronousAsyncWorker();
            var triggersViewModel = new TriggersViewModel(mockEventAggregator.Object, popupController, asyncWorker, mockServer.Object, a =>
            {
                return mockEnvironment.Object;
            });
            return triggersViewModel;
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(TriggersViewModel))]
        public void TriggersViewModel_DoDeactivate_ShowMessage_True_Expect_True()
        {
            var triggersViewModel = CreateTriggerViewModel();
            var value = triggersViewModel.DoDeactivate(false);
            Assert.IsTrue(value);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(TriggersViewModel))]
        public void TriggersViewModel_DoDeactivate_ShowMessage_True_Cancel_MessageBox_Expect_False()
        {
            var mockPopupController = new Mock<IPopupController>();
            var triggersViewModel = CreateTriggerViewModel(mockPopupController.Object);
            triggersViewModel.IsDirty = true;

            var value = triggersViewModel.DoDeactivate(true);
            Assert.IsFalse(value);
            mockPopupController.Verify(popupController => popupController.ShowTasksCloseConfirmation(), Times.Once);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(TriggersViewModel))]
        public void TriggersViewModel_DoDeactivate_ShowMessage_True_None_MessageBox_Expect_False()
        {
            var mockPopupController = new Mock<IPopupController>();
            var triggersViewModel = CreateTriggerViewModel(mockPopupController.Object);
            triggersViewModel.IsDirty = true;

            var value = triggersViewModel.DoDeactivate(true);
            Assert.IsFalse(value);
            mockPopupController.Verify(popupController => popupController.ShowTasksCloseConfirmation(), Times.Once);
        }


    }
}