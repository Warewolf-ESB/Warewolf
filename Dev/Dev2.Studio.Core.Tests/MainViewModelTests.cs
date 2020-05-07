/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using Caliburn.Micro;
using CubicOrange.Windows.Forms.ActiveDirectory;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Enums;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Infrastructure.Events;
using Dev2.Common.Interfaces.PopupController;
using Dev2.Common.Interfaces.Scheduler.Interfaces;
using Dev2.Common.Interfaces.Security;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.Threading;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Common.Interfaces.ToolBase.ExchangeEmail;
using Dev2.Common.Interfaces.Versioning;
using Dev2.Communication;
using Dev2.Core.Tests.Utils;
using Dev2.Factory;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Services.Events;
using Dev2.Services.Security;
using Dev2.Settings;
using Dev2.Triggers.Scheduler;
using Dev2.Studio.AppResources.Comparers;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.Browsers;
using Dev2.Studio.Core.Helpers;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.Workspaces;
using Dev2.Studio.Interfaces;
using Dev2.Studio.Interfaces.Enums;
using Dev2.Studio.ViewModels;
using Dev2.Studio.ViewModels.Help;
using Dev2.Studio.ViewModels.Workflow;
using Dev2.Studio.ViewModels.WorkSurface;
using Dev2.Threading;
using Dev2.Util;
using Dev2.Utilities;
using Dev2.Workspaces;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Dev2.ViewModels;
using Warewolf.Studio.ViewModels;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Instrumentation;
using Dev2.Triggers;
using Dev2.Dialogs;
using Warewolf.Data;
using Warewolf.Options;

namespace Dev2.Core.Tests
{
    [TestClass]
    public class MainViewModelTests : MainViewModelBase
    {
        [TestInitialize]
        public void Initialize()
        {
            AppUsageStats.LocalHost = "http://localhost:3142";
            var svr = new Mock<IServer>();
            svr.Setup(a => a.DisplayName).Returns("Localhost");
            svr.Setup(a => a.Name).Returns("Localhost");
            var mockEnvironmentConnection = SetupMockConnection();
            svr.SetupGet(it => it.Connection).Returns(mockEnvironmentConnection.Object);

            var explorerTooltips = new Mock<IExplorerTooltips>();
            CustomContainer.Register(explorerTooltips.Object);

            var ac = new Task<IExplorerItem>(() => new Mock<IExplorerItem>().Object);
            svr.Setup(a => a.LoadExplorer(false)).Returns(() => ac);
            CustomContainer.Register(svr.Object);
            CustomContainer.Register(new Mock<Microsoft.Practices.Prism.PubSubEvents.IEventAggregator>().Object);
            var serverRepo = new Mock<IServerRepository>();
            serverRepo.Setup(repository => repository.ActiveServer).Returns(svr.Object);
            CustomContainer.Register(serverRepo.Object);
        }

        [TestMethod]
        public void DeployCommandCanExecuteIrrespectiveOfEnvironments()
        {
            CreateFullExportsAndVm();
            Assert.IsTrue(_shellViewModel.DeployCommand.CanExecute(null));
        }

        [TestMethod]
        public void MainViewModel_ShowPopupMessage_CallsPopupController()
        {
            CreateFullExportsAndVm();
            _shellViewModel.ShowPopup(new Mock<IPopupMessage>().Object);
            _popupController.Verify(controller => controller.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), MessageBoxImage.Error, @"", false, true, false, false, false, false), Times.Once);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("MainViewModel_ToolboxViewModel")]
        public void MainViewModel_ToolboxViewModel_WhenInContainer_ShouldReturnContainerValue()
        {
            //------------Setup for test--------------------------
            var toolboxViewModel = new Mock<IToolboxViewModel>().Object;
            CustomContainer.Register(toolboxViewModel);
            CreateFullExportsAndVm();

            //------------Execute Test---------------------------
            var mainViewModelToolboxViewModel = _shellViewModel.ToolboxViewModel;
            //------------Assert Results-------------------------
            Assert.AreEqual(toolboxViewModel, mainViewModelToolboxViewModel);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("MainViewModel_SettingsCommand")]
        public void MainViewModel_SettingsCommand_CanExecute_Correct()
        {
            Verify_SettingsCommand_CanExecute(expected: true, isAuthorized: true, isConnected: true, canStudioExecute: true);
            Verify_SettingsCommand_CanExecute(expected: false, isAuthorized: true, isConnected: true, canStudioExecute: false);
            Verify_SettingsCommand_CanExecute(expected: false, isAuthorized: true, isConnected: false, canStudioExecute: true);
            Verify_SettingsCommand_CanExecute(expected: false, isAuthorized: false, isConnected: true, canStudioExecute: true);
        }

        void Verify_SettingsCommand_CanExecute(bool isConnected, bool canStudioExecute, bool isAuthorized, bool expected)
        {
            CreateFullExportsAndVm();
            _activeEnvironment.Setup(e => e.IsConnected).Returns(isConnected);
            _activeEnvironment.Setup(e => e.CanStudioExecute).Returns(canStudioExecute);

            _authorizationService.Setup(a => a.IsAuthorized(AuthorizationContext.Administrator, It.IsAny<string>())).Returns(isAuthorized);


            var actual = _shellViewModel.SettingsCommand.CanExecute(null);
            Assert.AreEqual(expected, actual);
        }

        #region Constructor

        // PBI 9397 - 2013.06.09 - TWR: added
        [TestMethod]
        public void MainViewModelConstructorWithWorkspaceItemsInRepositoryExpectedLoadsWorkspaceItems()
        {
            var workspaceID = Guid.NewGuid();
            var serverID = Guid.NewGuid();
            var resourceName = "TestResource_" + Guid.NewGuid();
            var resourceID = Guid.NewGuid();

            var wsi = new WorkspaceItem(workspaceID, serverID, Guid.Empty, resourceID) { ServiceName = resourceName, ServiceType = WorkspaceItem.ServiceServiceType };
            var wsiRepo = new Mock<IWorkspaceItemRepository>();
            wsiRepo.Setup(r => r.WorkspaceItems).Returns(new List<IWorkspaceItem>(new[] { wsi }));
            wsiRepo.Setup(r => r.AddWorkspaceItem(It.IsAny<IContextualResourceModel>())).Verifiable();

            SetupImportServiceForPersistenceTests(wsiRepo);

            var resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(m => m.ResourceName).Returns(resourceName);
            resourceModel.Setup(m => m.ID).Returns(resourceID);
            resourceModel.Setup(m => m.ResourceType).Returns(ResourceType.WorkflowService);

            var resourceRepo = new Mock<IResourceRepository>();
            resourceRepo.Setup(r => r.All()).Returns(new List<IResourceModel>(new[] { resourceModel.Object }));
            resourceRepo.Setup(r => r.FetchResourceDefinition(It.IsAny<IServer>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>())).Returns(new ExecuteMessage());


            var envConn = SetupMockConnection();
            envConn.Setup(conn => conn.ServerEvents).Returns(new Mock<IEventPublisher>().Object);
            envConn.Setup(conn => conn.WorkspaceID).Returns(workspaceID);
            envConn.Setup(conn => conn.ServerID).Returns(serverID);
            envConn.Setup(conn => conn.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>())).Returns(new StringBuilder());

            var env = new Mock<IServer>();
            env.Setup(e => e.Connection).Returns(envConn.Object);
            env.Setup(e => e.IsConnected).Returns(true);
            env.Setup(e => e.ResourceRepository).Returns(resourceRepo.Object);

            resourceModel.Setup(m => m.Environment).Returns(env.Object);

            var envRepo = new Mock<IServerRepository>();
            envRepo.Setup(r => r.All()).Returns(new List<IServer>(new[] { env.Object }));
            envRepo.Setup(r => r.Get(It.IsAny<Guid>())).Returns(env.Object);
            envRepo.Setup(r => r.Source).Returns(env.Object);



            var asyncWorker = AsyncWorkerTests.CreateSynchronousAsyncWorker();


            // FetchResourceDefinitionService
            var viewModel = new ShellViewModelPersistenceMock(envRepo.Object, asyncWorker.Object, false);

            wsiRepo.Verify(r => r.AddWorkspaceItem(It.IsAny<IContextualResourceModel>()));

            Assert.AreEqual(2, viewModel.Items.Count); // 1 extra for the help tab!
            var expected = viewModel.Items.FirstOrDefault(i => i.WorkSurfaceKey.ResourceID == resourceID);
            Assert.IsNotNull(expected);
        }

        // PBI 9397 - 2013.06.09 - TWR: added
        [TestMethod]
        public void MainViewModelConstructorWithWorkspaceItemsInRepositoryExpectedNotLoadsWorkspaceItemsWithDifferentEnvID()
        {
            var workspaceID = Guid.NewGuid();
            var serverID = Guid.NewGuid();
            var resourceName = "TestResource_" + Guid.NewGuid();
            var resourceID = Guid.NewGuid();

            var wsi = new WorkspaceItem(workspaceID, serverID, Guid.NewGuid(), resourceID) { ServiceName = resourceName, ServiceType = WorkspaceItem.ServiceServiceType };
            var wsiRepo = new Mock<IWorkspaceItemRepository>();
            wsiRepo.Setup(r => r.WorkspaceItems).Returns(new List<IWorkspaceItem>(new[] { wsi }));
            wsiRepo.Setup(r => r.AddWorkspaceItem(It.IsAny<IContextualResourceModel>())).Verifiable();

            SetupImportServiceForPersistenceTests(wsiRepo);

            var resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(m => m.ResourceName).Returns(resourceName);
            resourceModel.Setup(m => m.ID).Returns(resourceID);
            resourceModel.Setup(m => m.ResourceType).Returns(ResourceType.WorkflowService);

            var resourceRepo = new Mock<IResourceRepository>();
            resourceRepo.Setup(r => r.All()).Returns(new List<IResourceModel>(new[] { resourceModel.Object }));

            var envConn = SetupMockConnection();
            envConn.Setup(conn => conn.WorkspaceID).Returns(workspaceID);
            envConn.Setup(conn => conn.ServerID).Returns(serverID);
            var env = new Mock<IServer>();
            env.Setup(e => e.Connection).Returns(envConn.Object);
            env.Setup(e => e.IsConnected).Returns(true);
            env.Setup(e => e.ResourceRepository).Returns(resourceRepo.Object);

            resourceModel.Setup(m => m.Environment).Returns(env.Object);

            var envRepo = new Mock<IServerRepository>();
            envRepo.Setup(r => r.All()).Returns(new List<IServer>(new[] { env.Object }));
            envRepo.Setup(r => r.Source).Returns(env.Object);
            envRepo.Setup(r => r.ActiveServer).Returns(env.Object);
            var asyncWorker = AsyncWorkerTests.CreateSynchronousAsyncWorker();
            var viewModel = new ShellViewModelPersistenceMock(envRepo.Object, asyncWorker.Object, false);

            wsiRepo.Verify(r => r.AddWorkspaceItem(It.IsAny<IContextualResourceModel>()), Times.Never());

            Assert.AreEqual(1, viewModel.Items.Count); // 1 extra for the help tab!
            var expected = viewModel.Items.FirstOrDefault(i => i.WorkSurfaceKey.ResourceID == resourceID);
            Assert.IsNull(expected);
        }

        // PBI 9397 - 2013.06.09 - TWR: added
        [TestMethod]
        public void MainViewModelConstructorWithWorkspaceItemsInRepositoryExpectedNotLoadsWorkspaceItemsWithSameEnvID()
        {
            var workspaceID = Guid.NewGuid();
            var serverID = Guid.NewGuid();
            var resourceName = "TestResource_" + Guid.NewGuid();
            var resourceID = Guid.NewGuid();

            var environmentID = Guid.NewGuid();
            var wsi = new WorkspaceItem(workspaceID, serverID, environmentID, resourceID) { ServiceName = resourceName, ServiceType = WorkspaceItem.ServiceServiceType };
            var wsiRepo = new Mock<IWorkspaceItemRepository>();
            wsiRepo.Setup(r => r.WorkspaceItems).Returns(new List<IWorkspaceItem>(new[] { wsi }));
            wsiRepo.Setup(r => r.AddWorkspaceItem(It.IsAny<IContextualResourceModel>())).Verifiable();

            SetupImportServiceForPersistenceTests(wsiRepo);
            WorkspaceItemRepository.Instance.WorkspaceItems.Clear();
            WorkspaceItemRepository.Instance.WorkspaceItems.Add(wsi);

            var resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(m => m.ResourceName).Returns(resourceName);
            resourceModel.Setup(m => m.ID).Returns(resourceID);
            resourceModel.Setup(m => m.ResourceType).Returns(ResourceType.WorkflowService);

            var resourceRepo = new Mock<IResourceRepository>();
            resourceRepo.Setup(r => r.All()).Returns(new List<IResourceModel>(new[] { resourceModel.Object }));
            resourceRepo.Setup(r => r.FetchResourceDefinition(It.IsAny<IServer>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>())).Returns(new ExecuteMessage());

            var envConn = SetupMockConnection();
            envConn.Setup(conn => conn.WorkspaceID).Returns(workspaceID);
            envConn.Setup(conn => conn.ServerID).Returns(serverID);
            envConn.Setup(conn => conn.ServerEvents).Returns(new Mock<IEventPublisher>().Object);

            var env = new Mock<IServer>();
            env.Setup(e => e.Connection).Returns(envConn.Object);
            env.Setup(e => e.IsConnected).Returns(true);
            env.Setup(e => e.ResourceRepository).Returns(resourceRepo.Object);
            env.Setup(e => e.EnvironmentID).Returns(environmentID);

            resourceModel.Setup(m => m.Environment).Returns(env.Object);

            var envRepo = new Mock<IServerRepository>();
            envRepo.Setup(r => r.All()).Returns(new List<IServer>(new[] { env.Object }));
            envRepo.Setup(r => r.Source).Returns(env.Object);
            envRepo.Setup(r => r.Get(It.IsAny<Guid>())).Returns(env.Object);
            var asyncWorker = AsyncWorkerTests.CreateSynchronousAsyncWorker();
            var viewModel = new ShellViewModelPersistenceMock(envRepo.Object, asyncWorker.Object, false);

            wsiRepo.Verify(r => r.AddWorkspaceItem(It.IsAny<IContextualResourceModel>()), Times.AtLeastOnce());

            Assert.AreEqual(2, viewModel.Items.Count); // 1 extra for the help tab!
            var expected = viewModel.Items.FirstOrDefault(i => i.WorkSurfaceKey.ResourceID == resourceID);
            Assert.IsNotNull(expected);
        }

        [TestMethod]
        [TestCategory("MainViewModel_Constructor")]
        [Description("Constructor must not allow null AsyncWorker.")]
        [Owner("Trevor Williams-Ros")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void MainViewModel_UnitTest_ConstructorWithNullAsyncWorker_ThrowsArgumentNullException()
        {
            var eventPublisher = new Mock<IEventAggregator>();
            var environmentRepository = new Mock<IServerRepository>();
            var versionChecker = new Mock<IVersionChecker>();
            var vieFactory = new Mock<IViewFactory>();
            var viewMock = new Mock<IView>();
            vieFactory.Setup(factory => factory.GetViewGivenServerResourceType(It.IsAny<string>())).Returns(viewMock.Object);
            var mvm = new ShellViewModel(eventPublisher.Object, null, environmentRepository.Object, versionChecker.Object, vieFactory.Object, false);
            Assert.IsNull(mvm);
        }


        #endregion

        #region AddMode Work Surface


        #endregion

        #region Close Context

        [TestMethod]
        public void MainViewModel_CloseWorkSurfaceContext_CloseTrueAndResourceSaved_RemoveWorkspaceItemRemoveCalledAndTabClosedMessageAndContextRemoved()
        {
            CreateFullExportsAndVm();

            Assert.AreEqual(2, _shellViewModel.Items.Count);

            _firstResource.Setup(r => r.IsWorkflowSaved).Returns(true);
            _firstResource.Setup(r => r.IsAuthorized(AuthorizationContext.Contribute)).Returns(true);
            var activetx =
                _shellViewModel.Items.ToList()
                    .First(i => i.WorkSurfaceViewModel.WorkSurfaceContext == WorkSurfaceContext.Workflow);


            _shellViewModel.DeactivateItem(activetx, true);
            _mockWorkspaceRepo.Verify(c => c.Remove(_firstResource.Object), Times.Once());
            Assert.IsTrue(_shellViewModel.Items.Count == 1);
        }

        [TestMethod]
        public void MainViewModel_CloseWorkSurfaceContext_CloseTrueAndResourceNotSavedPopupOk_RemoveWorkspaceItemCalledAndContextRemovedAndSaveResourceEventAggregatorMessage()
        {
            CreateFullExportsAndVm();

            Assert.AreEqual(2, _shellViewModel.Items.Count);
            _firstResource.Setup(r => r.IsWorkflowSaved).Returns(false);
            _firstResource.Setup(r => r.IsAuthorized(AuthorizationContext.Contribute)).Returns(true);
            _popupController.Setup(s => s.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(MessageBoxResult.Yes);

            var activetx =
                _shellViewModel.Items.ToList()
                    .First(i => i.WorkSurfaceViewModel.WorkSurfaceContext == WorkSurfaceContext.Workflow);


            _eventAggregator.Setup(e => e.Publish(It.IsAny<SaveResourceMessage>()))
                .Callback<object>(o =>
                {
                    var msg = (SaveResourceMessage)o;
                    Assert.IsTrue(msg.Resource.Equals(_firstResource.Object));
                });

            _shellViewModel.DeactivateItem(activetx, true);
            _mockWorkspaceRepo.Verify(c => c.Remove(_firstResource.Object), Times.Once());
            Assert.IsTrue(_shellViewModel.Items.Count == 1);
            _eventAggregator.Verify(e => e.Publish(It.IsAny<SaveResourceMessage>()), Times.Once());
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("MainViewModel_DeactivateItem")]
        public void MainViewModel_DeactivateItem_WorkSurfaceContextViewModelIsNull_RemoveIsNotCalledOnTheRepo()
        {
            CreateFullExportsAndVm();
            Assert.AreEqual(2, _shellViewModel.Items.Count);
            _firstResource.Setup(r => r.IsWorkflowSaved).Returns(false);
            _firstResource.Setup(r => r.IsAuthorized(AuthorizationContext.Contribute)).Returns(true);
            _popupController.Setup(s => s.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(MessageBoxResult.Yes);
            _shellViewModel.DeactivateItem(null, true);
            _mockWorkspaceRepo.Verify(c => c.Remove(_firstResource.Object), Times.Never());
        }

        [TestMethod]
        public void MainViewModel_CloseWorkSurfaceContext_CloseTrueAndResourceNotSavedPopupNotOk_WorkspaceItemNotRemoved()
        {
            CreateFullExportsAndVm();
            Assert.AreEqual(2, _shellViewModel.Items.Count);
            _firstResource.Setup(r => r.IsWorkflowSaved).Returns(false);
            _firstResource.Setup(r => r.IsAuthorized(AuthorizationContext.Contribute)).Returns(true);

            _popupController.Setup(s => s.Show()).Returns(MessageBoxResult.No);
            var activetx =
                _shellViewModel.Items.ToList()
                    .First(i => i.WorkSurfaceViewModel.WorkSurfaceContext == WorkSurfaceContext.Workflow);
            _shellViewModel.DeactivateItem(activetx, false);
            _mockWorkspaceRepo.Verify(c => c.Remove(_firstResource.Object), Times.Never());
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("MainViewModel_IsWorkFlowOpened")]
        public void MainViewModel_IsWorkFlowOpened_ResourceIsOpened_True()
        {
            //------------Setup for test--------------------------
            CreateFullExportsAndVm();
            _firstResource.Setup(r => r.IsAuthorized(AuthorizationContext.Contribute)).Returns(true);
            //------------Execute Test---------------------------
            var isWorkflowOpened = _shellViewModel.IsWorkFlowOpened(_firstResource.Object);
            //------------Execute Test---------------------------
            Assert.IsTrue(isWorkflowOpened);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("MainViewModel_IsWorkFlowOpened")]
        public void MainViewModel_IsWorkFlowOpened_ResourceIsNotOpened_False()
        {
            //------------Setup for test--------------------------
            CreateFullExportsAndVm();
            //------------Execute Test---------------------------
            var resource = new Mock<IContextualResourceModel>();
            var environmentModel = new Mock<IServer>();
            environmentModel.Setup(e => e.EnvironmentID).Returns(Guid.NewGuid);
            resource.Setup(r => r.Environment).Returns(environmentModel.Object);
            resource.Setup(r => r.IsAuthorized(AuthorizationContext.Contribute)).Returns(true);
            var isWorkflowOpened = _shellViewModel.IsWorkFlowOpened(resource.Object);
            //------------Execute Test---------------------------
            Assert.IsFalse(isWorkflowOpened);
        }

        [TestMethod]
        public void MainViewModel_CloseWorkSurfaceContext_CloseFalse_PreviousItemActivatedAndAllItemsPResent()
        {
            CreateFullExportsAndVm();
            AddAdditionalContext();
            Assert.AreEqual(3, _shellViewModel.Items.Count);

            _firstResource.Setup(r => r.IsAuthorized(AuthorizationContext.Contribute)).Returns(true);
            _secondResource.Setup(r => r.IsAuthorized(AuthorizationContext.Contribute)).Returns(true);

            var firstCtx = _shellViewModel.WorksurfaceContextManager.FindWorkSurfaceContextViewModel(_firstResource.Object);
            var secondCtx = _shellViewModel.WorksurfaceContextManager.FindWorkSurfaceContextViewModel(_secondResource.Object);

            _shellViewModel.ActivateItem(firstCtx);
            _shellViewModel.DeactivateItem(secondCtx, false);

            Assert.AreEqual(3, _shellViewModel.Items.Count);
            Assert.IsTrue(_shellViewModel.ActiveItem.Equals(firstCtx));
        }

        [TestMethod]
        public void MainViewModel_CloseWorkSurfaceContext_CloseTrue_PreviousItemActivatedAndOneLessItem()
        {
            CreateFullExportsAndVm();
            AddAdditionalContext();
            Assert.AreEqual(3, _shellViewModel.Items.Count);

            _firstResource.Setup(r => r.IsAuthorized(AuthorizationContext.Contribute)).Returns(true);
            _secondResource.Setup(r => r.IsAuthorized(AuthorizationContext.Contribute)).Returns(true);

            var firstCtx = _shellViewModel.WorksurfaceContextManager.FindWorkSurfaceContextViewModel(_firstResource.Object);
            var secondCtx = _shellViewModel.WorksurfaceContextManager.FindWorkSurfaceContextViewModel(_secondResource.Object);

            _shellViewModel.ActivateItem(firstCtx);
            _shellViewModel.DeactivateItem(firstCtx, true);

            Assert.AreEqual(3, _shellViewModel.Items.Count);
            Assert.IsFalse(_shellViewModel.ActiveItem.Equals(secondCtx));
        }

        [TestMethod]
        public void MainViewModel_CloseWorkSurfaceContext_CloseFalse_ContextNotRemoved()
        {
            CreateFullExportsAndVm();
            _firstResource.Setup(r => r.IsAuthorized(AuthorizationContext.Contribute)).Returns(true);
            var activetx =
                _shellViewModel.Items.ToList()
                    .First(i => i.WorkSurfaceViewModel.WorkSurfaceContext == WorkSurfaceContext.Workflow);
            _shellViewModel.DeactivateItem(activetx, false);
            _mockWorkspaceRepo.Verify(c => c.Remove(_firstResource.Object), Times.Never());
        }

        [TestMethod]
        [TestCategory("MainViewModel_CloseWorkSurfaceContext")]
        [Description("An exisiting workflow with unsaved changes that is not saved, must rollback the resource model.")]
        [Owner("Trevor Williams-Ros")]
        public void MainViewModel_CloseWorkSurfaceContext_ExistingUnsavedWorkflowNotSaved_ResourceModelRolledback()
        {
            CreateFullExportsAndVm();
            Assert.IsTrue(_shellViewModel.Items.Count == 2);
            _firstResource.Setup(r => r.IsWorkflowSaved).Returns(false);
            _firstResource.Setup(r => r.Commit()).Verifiable();
            _firstResource.Setup(r => r.Rollback()).Verifiable();
            _firstResource.Setup(r => r.IsAuthorized(AuthorizationContext.Contribute)).Returns(true);

            _popupController.Setup(s => s.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(MessageBoxResult.No);
            var activetx = _shellViewModel.Items.ToList().First(i => i.WorkSurfaceViewModel.WorkSurfaceContext == WorkSurfaceContext.Workflow);
            _shellViewModel.WorksurfaceContextManager.CloseWorkSurfaceContext(activetx, null);
            _firstResource.Verify(r => r.Commit(), Times.Never(), "ResourceModel was committed when not saved.");
            _firstResource.Verify(r => r.Rollback(), Times.Once(), "ResourceModel was not rolled back when not saved.");
        }

        [TestMethod]
        [TestCategory("MainViewModel_CloseWorkSurfaceContext")]
        [Description("An exisiting workflow with unsaved changes that is saved, must commit the resource model.")]
        [Owner("Trevor Williams-Ros")]
        public void MainViewModel_CloseWorkSurfaceContext_ExistingUnsavedWorkflowSaved_ResourceModelCommitted()
        {
            CreateFullExportsAndVm();
            Assert.IsTrue(_shellViewModel.Items.Count == 2);
            _firstResource.Setup(r => r.IsWorkflowSaved).Returns(false);
            _firstResource.Setup(r => r.IsAuthorized(AuthorizationContext.Contribute)).Returns(true);
            _firstResource.Setup(r => r.Commit()).Verifiable();
            _firstResource.Setup(r => r.Rollback()).Verifiable();

            _popupController.Setup(s => s.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(MessageBoxResult.Yes);
            var activetx = _shellViewModel.Items.ToList().First(i => i.WorkSurfaceViewModel.WorkSurfaceContext == WorkSurfaceContext.Workflow);
            _shellViewModel.WorksurfaceContextManager.CloseWorkSurfaceContext(activetx, null);
            _firstResource.Verify(r => r.Commit(), Times.Once(), "ResourceModel was not committed when saved.");
            _firstResource.Verify(r => r.Rollback(), Times.Never(), "ResourceModel was rolled back when saved.");
        }


        [TestMethod]
        [TestCategory("MainViewModel_CloseWorkSurfaceContext")]
        [Description("An exisiting workflow with unsaved changes that is saved, must commit the resource model.")]
        [Owner("Leon Rajindrapersadh")]
        public void MainViewModel_CloseWorkSurfaceContext_ExistingUnsavedWorkflowSaved_WhenDeletedNoPopup()
        {
            CreateFullExportsAndVm();
            Assert.IsTrue(_shellViewModel.Items.Count == 2);
            _firstResource.Setup(r => r.IsWorkflowSaved).Returns(false);
            _firstResource.Setup(r => r.IsAuthorized(AuthorizationContext.Contribute)).Returns(true);
            _firstResource.Setup(r => r.Commit()).Verifiable();
            _firstResource.Setup(r => r.Rollback()).Verifiable();

            _popupController.Setup(s => s.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(MessageBoxResult.Yes);
            var activetx = _shellViewModel.Items.ToList().First(i => i.WorkSurfaceViewModel.WorkSurfaceContext == WorkSurfaceContext.Workflow);
            _shellViewModel.WorksurfaceContextManager.CloseWorkSurfaceContext(activetx, null, true);
            _popupController.Verify(
                s =>
                    s.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(),
                        It.IsAny<MessageBoxImage>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(),
                        It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Never);


        }

        [TestMethod]
        [TestCategory("MainViewModel_CloseWorkSurfaceContext")]
        [Description("An exisiting workflow with unsaved changes that is saved, must commit the resource model.")]
        [Owner("Leon Rajindrapersadh")]
        public void MainViewModel_CloseResource()
        {
            CreateFullExportsAndVm();
            Assert.IsTrue(_shellViewModel.Items.Count == 2);
            _firstResource.Setup(r => r.IsWorkflowSaved).Returns(false);
            _firstResource.Setup(r => r.IsAuthorized(AuthorizationContext.Contribute)).Returns(true);
            _firstResource.Setup(r => r.Commit()).Verifiable();
            _firstResource.Setup(r => r.Rollback()).Verifiable();
            var gu = Guid.NewGuid();
            _firstResource.Setup(a => a.ID).Returns(gu);
            _popupController.Setup(s => s.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(MessageBoxResult.Yes);
            var mckEnv = new Mock<IServerRepository>();
            var mockEnv = new Mock<IServer>();
            mckEnv.Setup(a => a.Get(It.IsAny<Guid>()))
                .Returns(mockEnv.Object);
            var res = new Mock<IResourceRepository>();
            mockEnv.Setup(a => a.ResourceRepository).Returns(res.Object);
            res.Setup(a => a.LoadContextualResourceModel(It.IsAny<Guid>())).Returns(_firstResource.Object);
            _shellViewModel.CloseResource(gu, Guid.Empty);
            _popupController.Verify(
                s =>
                    s.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(),
                        It.IsAny<MessageBoxImage>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(),
                        It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Never);
        }

        [TestMethod]
        [TestCategory("MainViewModel_CreateTest")]
        [Description("An exisiting workflow with unsaved changes that is saved, must commit the resource model.")]
        [Owner("Pieter Terblanche")]
        public void MainViewModel_CreateTest()
        {
            CreateFullExportsAndVm();
            Assert.IsTrue(_shellViewModel.Items.Count == 2);
            _firstResource.Setup(r => r.IsWorkflowSaved).Returns(false);
            _firstResource.Setup(r => r.IsAuthorized(AuthorizationContext.Contribute)).Returns(true);
            _firstResource.Setup(r => r.Commit()).Verifiable();
            _firstResource.Setup(r => r.Rollback()).Verifiable();
            var resourceId = Guid.NewGuid();
            _firstResource.Setup(a => a.ID).Returns(resourceId);
            _popupController.Setup(s => s.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(MessageBoxResult.Yes);
            var mckEnv = new Mock<IServerRepository>();
            var mockEnv = new Mock<IServer>();
            mckEnv.Setup(a => a.Get(resourceId)).Returns(mockEnv.Object);
            var res = new Mock<IResourceRepository>();
            mockEnv.Setup(a => a.ResourceRepository).Returns(res.Object);
            res.Setup(a => a.LoadContextualResourceModel(resourceId)).Returns(_firstResource.Object);
            _shellViewModel.CreateTest(resourceId);
        }

        [TestMethod]
        [TestCategory("MainViewModel_RunAllTests")]
        [Description("An exisiting workflow with unsaved changes that is saved, must commit the resource model.")]
        [Owner("Pieter Terblanche")]
        public void MainViewModel_RunAllTests()
        {
            CreateFullExportsAndVm();
            Assert.IsTrue(_shellViewModel.Items.Count == 2);
            _firstResource.Setup(r => r.IsWorkflowSaved).Returns(false);
            _firstResource.Setup(r => r.IsAuthorized(AuthorizationContext.Contribute)).Returns(true);
            _firstResource.Setup(r => r.Commit()).Verifiable();
            _firstResource.Setup(r => r.Rollback()).Verifiable();
            var resourceId = Guid.NewGuid();
            _firstResource.Setup(a => a.ID).Returns(resourceId);
            _popupController.Setup(s => s.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(MessageBoxResult.Yes);
            var mckEnv = new Mock<IServerRepository>();
            var mockEnv = new Mock<IServer>();
            mckEnv.Setup(a => a.Get(resourceId)).Returns(mockEnv.Object);
            var res = new Mock<IResourceRepository>();
            mockEnv.Setup(a => a.ResourceRepository).Returns(res.Object);
            res.Setup(a => a.LoadContextualResourceModel(resourceId)).Returns(_firstResource.Object);
            _shellViewModel.RunAllTests(string.Empty, resourceId, new Mock<IExternalProcessExecutor>().Object);
        }

        [TestMethod]
        [TestCategory("MainViewModel_CloseResourceTestView")]
        [Description("An exisiting workflow with unsaved changes that is saved, must commit the resource model.")]
        [Owner("Pieter Terblanche")]
        public void MainViewModel_CloseResourceTestView()
        {
            CreateFullExportsAndVm();
            Assert.IsTrue(_shellViewModel.Items.Count == 2);
            _firstResource.Setup(r => r.IsWorkflowSaved).Returns(false);
            _firstResource.Setup(r => r.IsAuthorized(AuthorizationContext.Contribute)).Returns(true);
            _firstResource.Setup(r => r.Commit()).Verifiable();
            _firstResource.Setup(r => r.Rollback()).Verifiable();
            var resourceId = Guid.NewGuid();
            _firstResource.Setup(a => a.ID).Returns(resourceId);
            _popupController.Setup(s => s.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(MessageBoxResult.Yes);
            var mckEnv = new Mock<IServerRepository>();
            var mockEnv = new Mock<IServer>();
            mckEnv.Setup(a => a.Get(resourceId)).Returns(mockEnv.Object);
            var res = new Mock<IResourceRepository>();
            mockEnv.Setup(a => a.ResourceRepository).Returns(res.Object);
            res.Setup(a => a.LoadContextualResourceModel(resourceId)).Returns(_firstResource.Object);
            _shellViewModel.CloseResourceTestView(resourceId, _serverId, mockEnv.Object.EnvironmentID);
        }

        [TestMethod]
        [TestCategory("MainViewModel_CloseResourceMergeView")]
        [Description("An exisiting workflow with unsaved changes that is saved, must commit the resource model.")]
        [Owner("Pieter Terblanche")]
        public void MainViewModel_CloseResourceMergeView()
        {
            CreateFullExportsAndVm();
            Assert.IsTrue(_shellViewModel.Items.Count == 2);
            _firstResource.Setup(r => r.IsWorkflowSaved).Returns(false);
            _firstResource.Setup(r => r.IsAuthorized(AuthorizationContext.Contribute)).Returns(true);
            _firstResource.Setup(r => r.Commit()).Verifiable();
            _firstResource.Setup(r => r.Rollback()).Verifiable();
            var resourceId = Guid.NewGuid();
            _firstResource.Setup(a => a.ID).Returns(resourceId);
            _popupController.Setup(s => s.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(MessageBoxResult.Yes);
            var mckEnv = new Mock<IServerRepository>();
            var mockEnv = new Mock<IServer>();
            mckEnv.Setup(a => a.Get(resourceId)).Returns(mockEnv.Object);
            var res = new Mock<IResourceRepository>();
            mockEnv.Setup(a => a.ResourceRepository).Returns(res.Object);
            res.Setup(a => a.LoadContextualResourceModel(resourceId)).Returns(_firstResource.Object);
            _shellViewModel.CloseResourceMergeView(resourceId, _serverId, mockEnv.Object.EnvironmentID);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("MainViewModel_CloseWorkSurfaceContext")]
        public void MainViewModel_CloseWorkSurfaceContext_UnsavedWorkflowAndResourceCanSaveIsFalse_ResourceModelIsNotSaved()
        {
            //------------Setup for test--------------------------
            CreateFullExportsAndVm();
            Assert.IsTrue(_shellViewModel.Items.Count == 2);

            _firstResource.Setup(r => r.Commit()).Verifiable();
            _firstResource.Setup(r => r.Rollback()).Verifiable();
            _firstResource.Setup(r => r.IsWorkflowSaved).Returns(false);
            _firstResource.Setup(r => r.IsAuthorized(AuthorizationContext.Contribute)).Returns(false);

            _eventAggregator.Setup(e => e.Publish(It.IsAny<SaveResourceMessage>())).Verifiable();

            _popupController.Setup(s => s.Show()).Returns(MessageBoxResult.Yes);
            var activetx = _shellViewModel.Items.ToList().First(i => i.WorkSurfaceViewModel.WorkSurfaceContext == WorkSurfaceContext.Workflow);

            //------------Execute Test---------------------------
            _shellViewModel.WorksurfaceContextManager.CloseWorkSurfaceContext(activetx, null);
            var pvt = new PrivateObject(_shellViewModel);
            //------------Assert Results-------------------------
            _eventAggregator.Verify(e => e.Publish(It.IsAny<SaveResourceMessage>()), Times.Never());
            _firstResource.Verify(r => r.Commit(), Times.Never(), "ResourceModel was committed when saved.");
            _firstResource.Verify(r => r.Rollback(), Times.Never(), "ResourceModel was rolled back when saved.");
            Assert.IsNull(pvt.GetField("_previousActive"));
        }


        #endregion

        #region Workspaces and init

        [TestMethod]
        public void OnImportsSatisfiedExpectsTwoItems()
        {
            CreateFullExportsAndVm();
            //One saved workspaceitem, one startpage
            Assert.AreEqual(2, _shellViewModel.Items.Count);
        }

        [TestMethod]
        public void OnImportsSatisfiedExpectsContextsAddedForSavedWorkspaces()
        {
            CreateFullExportsAndVm();
            var activetx =
                _shellViewModel.Items.ToList()
                    .First(i => i.WorkSurfaceViewModel.WorkSurfaceContext == WorkSurfaceContext.Workflow);
            var expectedKey = WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.Workflow, _firstResourceId,
                _serverId);
            Assert.IsTrue(expectedKey.ResourceID.Equals(activetx.WorkSurfaceKey.ResourceID) && expectedKey.ServerID.Equals(activetx.WorkSurfaceKey.ServerID));
        }

        [TestMethod]
        public void OnImportsSatisfiedExpectsDisplayNameSet()
        {
            CreateFullExportsAndVm();
            const string expected = "Warewolf";
            // flipping thing never passes locally... silly chickens ;(
            StringAssert.Contains(_shellViewModel.DisplayName, expected);
        }

        #endregion workspaces

        #region Commands

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("MainViewModel_ShowStartPageCommand")]
        public void MainViewModel_ShowStartPageCommand_ShowStartPageActive()
        {
            CreateFullExportsAndVm();
            //------------Execute Test---------------------------
            _shellViewModel.ShowStartPageCommand.Execute(null);
            var langHelpCtx = _shellViewModel.ActiveItem.WorkSurfaceViewModel as HelpViewModel;
            Assert.IsNotNull(langHelpCtx);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("MainViewModel_DebugCommand")]
        public void MainViewModel_DebugCommand_NotNull()
        {
            CreateFullExportsAndVm();
            //------------Execute Test---------------------------
            var authorizeCommand = _shellViewModel.DebugCommand;
            Assert.IsNotNull(authorizeCommand);
            Assert.AreEqual(_shellViewModel.ActiveItem.DebugCommand, authorizeCommand);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("MainViewModel_DebugCommand")]
        public void MainViewModel_DebugCommandNoActiveItem_NotNull()
        {
            CreateFullExportsAndVmWithEmptyRepo();
            _shellViewModel.DeactivateItem(_shellViewModel.ActiveItem, true);
            //------------Execute Test---------------------------
            var authorizeCommand = _shellViewModel.DebugCommand;
            Assert.IsNotNull(authorizeCommand);
            Assert.AreEqual(AuthorizationContext.None, authorizeCommand.AuthorizationContext);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("MainViewModel_QuickDebugCommand")]
        public void MainViewModel_QuickDebugCommand_NotNull()
        {
            CreateFullExportsAndVm();
            //------------Execute Test---------------------------
            var authorizeCommand = _shellViewModel.QuickDebugCommand;
            Assert.IsNotNull(authorizeCommand);
            Assert.AreEqual(_shellViewModel.ActiveItem.QuickDebugCommand, authorizeCommand);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("MainViewModel_QuickDebugCommand")]
        public void MainViewModel_QuickDebugCommandNoActiveItem_NotNull()
        {
            CreateFullExportsAndVmWithEmptyRepo();
            _shellViewModel.DeactivateItem(_shellViewModel.ActiveItem, true);
            //------------Execute Test---------------------------
            var authorizeCommand = _shellViewModel.QuickDebugCommand;
            Assert.IsNotNull(authorizeCommand);
            Assert.AreEqual(AuthorizationContext.None, authorizeCommand.AuthorizationContext);
        }



        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("MainViewModel_SaveCommand")]
        public void MainViewModel_SaveCommand_NotNull()
        {
            CreateFullExportsAndVm();
            //------------Execute Test---------------------------
            var authorizeCommand = _shellViewModel.SaveCommand;
            Assert.IsNotNull(authorizeCommand);
            Assert.AreEqual(_shellViewModel.ActiveItem.SaveCommand, authorizeCommand);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("MainViewModel_SaveCommand")]
        public void MainViewModel_SaveCommandNoActiveItem_NotNull()
        {
            CreateFullExportsAndVmWithEmptyRepo();
            _shellViewModel.DeactivateItem(_shellViewModel.ActiveItem, true);
            //------------Execute Test---------------------------
            var authorizeCommand = _shellViewModel.SaveCommand;
            Assert.IsNotNull(authorizeCommand);
            Assert.AreEqual(AuthorizationContext.None, authorizeCommand.AuthorizationContext);
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("MainViewModel_QuickViewInBrowserCommand")]
        public void MainViewModel_QuickViewInBrowserCommand_NotNull()
        {
            CreateFullExportsAndVm();
            //------------Execute Test---------------------------
            var authorizeCommand = _shellViewModel.QuickViewInBrowserCommand;
            Assert.IsNotNull(authorizeCommand);
            Assert.AreEqual(_shellViewModel.ActiveItem.QuickViewInBrowserCommand, authorizeCommand);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("MainViewModel_QuickViewInBrowserCommand")]
        public void MainViewModel_QuickViewInBrowserCommandNoActiveItem_NotNull()
        {
            CreateFullExportsAndVmWithEmptyRepo();
            _shellViewModel.DeactivateItem(_shellViewModel.ActiveItem, true);
            //------------Execute Test---------------------------
            var authorizeCommand = _shellViewModel.QuickViewInBrowserCommand;
            Assert.IsNotNull(authorizeCommand);
            Assert.AreEqual(AuthorizationContext.None, authorizeCommand.AuthorizationContext);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("MainViewModel_ViewInBrowserCommand")]
        public void MainViewModel_ViewInBrowserCommand_NotNull()
        {
            CreateFullExportsAndVm();
            //------------Execute Test---------------------------
            var authorizeCommand = _shellViewModel.ViewInBrowserCommand;
            Assert.IsNotNull(authorizeCommand);
            Assert.AreEqual(_shellViewModel.ActiveItem.ViewInBrowserCommand, authorizeCommand);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("MainViewModel_ViewInBrowserCommand")]
        public void MainViewModel_ViewInBrowserCommandNoActiveItem_NotNull()
        {
            CreateFullExportsAndVmWithEmptyRepo();
            _shellViewModel.DeactivateItem(_shellViewModel.ActiveItem, true);
            //------------Execute Test---------------------------
            var authorizeCommand = _shellViewModel.ViewInBrowserCommand;
            Assert.IsNotNull(authorizeCommand);
            Assert.AreEqual(AuthorizationContext.None, authorizeCommand.AuthorizationContext);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("MainViewModel_ShowCommunityPageCommand")]
        public void MainViewModel_ShowCommunityPageCommand_ShowShowCommunityPagActive()
        {
            CreateFullExportsAndVm();
            //------------Execute Test---------------------------
            _shellViewModel.ShowCommunityPageCommand.Execute(null);
            _browserPopupController.Verify(controller => controller.ShowPopup(It.IsAny<string>()));
        }

        [TestMethod]
        [Description("Makes sure that new workflow only calls TempSave, not save on the resource repository")]
        [Owner("Jurie Smit")]
        public void MainViewModel_Regression_NewWorkFlowCommand_DoesNotSaveRepository()
        {
            //Setup
            CustomContainer.Register(new Mock<Common.Interfaces.Studio.Controller.IPopupController>().Object);
            CreateFullExportsAndVmWithEmptyRepo();

            var environmentRepo = CreateMockEnvironment();

            var mockAuthService = new Mock<IAuthorizationService>();
            mockAuthService.Setup(c => c.GetResourcePermissions(It.IsAny<Guid>())).Returns(Permissions.Administrator);
            environmentRepo.Setup(c => c.AuthorizationService).Returns(mockAuthService.Object);

            var resourceRepo = new Mock<IResourceRepository>();
            resourceRepo.Setup(c => c.FetchResourceDefinition(It.IsAny<IServer>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>())).Returns(new ExecuteMessage());
            resourceRepo.Setup(r => r.Save(It.IsAny<IResourceModel>())).Verifiable();

            environmentRepo.Setup(e => e.ResourceRepository).Returns(resourceRepo.Object);
            environmentRepo.Setup(e => e.IsConnected).Returns(true);
            environmentRepo.Setup(e => e.IsLocalHost).Returns(true);

            EmptyEnvRepo.Setup(p => p.Get(It.IsAny<Guid>())).Returns(environmentRepo.Object);

            _shellViewModel.ActiveServer = environmentRepo.Object;
            _shellViewModel.NewServiceCommand.Execute("");
            //Assert
            resourceRepo.Verify(r => r.Save(It.IsAny<IResourceModel>()), Times.Never());
        }

        #endregion

        #region Delete

        [TestMethod]
        public void DeleteResourceConfirmedExpectContextRemoved()
        {
            CreateFullExportsAndVm();
            SetupForDelete();
            var msg = new DeleteResourcesMessage(new List<IContextualResourceModel> { _firstResource.Object }, "");
            _shellViewModel.Handle(msg);
            _resourceRepo.Verify(s => s.HasDependencies(_firstResource.Object), Times.Once());
        }

        [TestMethod]
        public void DeleteResourceConfirmedExpectContextRemoved2()
        {
            CreateFullExportsAndVm();
            SetupForDelete();
            var msg = new DeleteResourcesMessage(new List<IContextualResourceModel> { _firstResource.Object, _firstResource.Object }, "somefolder");
            _shellViewModel.Handle(msg);
            _resourceRepo.Verify(s => s.HasDependencies(_firstResource.Object), Times.Exactly(2));
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("MainViewModel_HandleDeleteResourceMessage")]
        public void MainViewModel_HandleDeleteResourceMessage_WhenHasActionDeclined_PerformsAction()
        {
            //------------Setup for test--------------------------
            CreateFullExportsAndVm();
            SetupForDelete();
            _popupController.Setup(s => s.Show()).Returns(MessageBoxResult.No);
            var _actionInvoked = false;
            var msg = new DeleteResourcesMessage(new List<IContextualResourceModel> { _firstResource.Object }, "", false, () =>
            {
                _actionInvoked = true;
            });
            //------------Execute Test---------------------------
            _shellViewModel.Handle(msg);
            //------------Assert Results-------------------------
            Assert.IsTrue(_actionInvoked);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("MainViewModel_HandleDeleteResourceMessage")]
        public void MainViewModel_HandleDeleteResourceMessage_WhenHasNullResource_PerformsAction()
        {
            //------------Setup for test--------------------------
            CreateFullExportsAndVm();
            SetupForDelete();
            _popupController.Setup(s => s.Show()).Returns(MessageBoxResult.Yes);
            var _actionInvoked = false;
            var msg = new DeleteResourcesMessage(null, "", true, () =>
            {
                _actionInvoked = true;
            });
            //------------Execute Test---------------------------
            _shellViewModel.Handle(msg);
            //------------Assert Results-------------------------
            Assert.IsFalse(_actionInvoked);
        }

        [TestMethod]
        public void DeleteResourceWithConfirmExpectsDependencyServiceCalled()
        {
            CreateFullExportsAndVm();
            SetupForDelete();
            _popupController.Setup(s => s.Show()).Returns(MessageBoxResult.Yes);
            var msg = new DeleteResourcesMessage(new List<IContextualResourceModel> { _firstResource.Object }, "");
            _shellViewModel.Handle(msg);
            _resourceRepo.Verify(s => s.HasDependencies(_firstResource.Object), Times.Once());
        }




        [TestMethod]
        public void DeleteResourceWithDeclineExpectsDependencyServiceCalled()
        {
            CreateFullExportsAndVm();
            SetupForDelete();
            _popupController.Setup(s => s.Show()).Returns(MessageBoxResult.No);
            var msg = new DeleteResourcesMessage(new List<IContextualResourceModel> { _firstResource.Object }, "", false);
            _shellViewModel.Handle(msg);
            _resourceRepo.Verify(s => s.HasDependencies(_firstResource.Object), Times.Never());
        }

        [TestMethod]
        public void DeleteResourceWithNullResourceExpectsNoPoupShown()
        {
            CreateFullExportsAndVm();
            SetupForDelete();
            var msg = new DeleteResourcesMessage(null, "", false);
            _shellViewModel.Handle(msg);

            _popupController.Verify(s => s.Show(), Times.Never());
        }

        [TestMethod]
        public void TestFromDebugExpectsNoPoupShown()
        {
            CreateFullExportsAndVm();
            var msg = new NewTestFromDebugMessage();
            try
            {
                _shellViewModel.Handle(msg);
            }
            catch (Exception)
            {
                //Suppress because of real calls happening                
            }

            _popupController.Verify(s => s.Show(), Times.Never());
        }

        [TestMethod]
        public void TestFromDebugAddAndActivateWorkSurface()
        {
            CreateFullExportsAndVm();
            var msg = new NewTestFromDebugMessage
            {
                ResourceModel = _firstResource.Object,
                ResourceID = _firstResourceId,
                RootItems = new List<IDebugTreeViewItemViewModel>()
            };
            try
            {
                _shellViewModel.Handle(msg);
            }
            catch (Exception)
            {
                //Suppress because of real calls happening                
            }

            _popupController.Verify(s => s.Show(), Times.Never());
        }

        [TestMethod]
        public void TestFromDebugExistingWorkSurface()
        {
            CreateFullExportsAndVm();
            var msg = new NewTestFromDebugMessage
            {
                ResourceModel = _firstResource.Object,
                ResourceID = _firstResourceId,
                RootItems = new List<IDebugTreeViewItemViewModel>()
            };

            var mockPopUp = Dev2MockFactory.CreateIPopup(MessageBoxResult.No);
            var workflowHelper = new Mock<IWorkflowHelper>();

            var workSurfaceKey = _shellViewModel.WorksurfaceContextManager.TryGetOrCreateWorkSurfaceKey(null, WorkSurfaceContext.ServiceTestsViewer, msg.ResourceID);
            var designerViewModel = new WorkflowDesignerViewModel(new Mock<IEventAggregator>().Object, msg.ResourceModel, workflowHelper.Object, mockPopUp.Object, new SynchronousAsyncWorker(), false);
            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(workSurfaceKey as WorkSurfaceKey, designerViewModel);
            _shellViewModel.Items.Add(workSurfaceContextViewModel);
            try
            {
                _shellViewModel.Handle(msg);
            }
            catch (Exception)
            {
                //Suppress because of real calls happening                
            }

            _popupController.Verify(s => s.Show(), Times.Never());
        }



        [TestMethod]
        [TestCategory("MainViewmodel_Delete")]
        [Description("Unassigned resources can be deleted")]
        [Owner("Ashley Lewis")]

        public void MainViewmodel_UnitTest_DeleteUnassignedResource_ResourceRepositoryDeleteResourceCalled()

        {
            //Isolate delete unassigned resource as a functional unit
            CreateFullExportsAndVm();
            SetupForDelete();
            var unassignedResource = new Mock<IContextualResourceModel>();
            var repo = new Mock<IResourceRepository>();
            var env = new Mock<IServer>();

            unassignedResource.Setup(res => res.Category).Returns(string.Empty);
            unassignedResource.Setup(resource => resource.Environment).Returns(env.Object);
            repo.Setup(repository => repository.DeleteResource(unassignedResource.Object)).Returns(MakeMsg("<DataList>Success</DataList>")).Verifiable();
            env.Setup(environment => environment.ResourceRepository).Returns(repo.Object);
            var msg = new DeleteResourcesMessage(new List<IContextualResourceModel> { unassignedResource.Object }, "", false, () => repo.Object.DeleteResource(unassignedResource.Object));

            //Run delete command
            _shellViewModel.Handle(msg);

            //Assert resource deleted from repository
            repo.Verify(repository => repository.DeleteResource(unassignedResource.Object), Times.Once(), "Deleting an unassigned resource does not delete from resource repository");
        }


        #endregion delete

        #region ShowStartPage

        // PBI 9512 - 2013.06.07 - TWR: added
        [TestMethod]
       
        [TestCategory("ShowStartPage")]
        public void MainViewModelShowStartPageExpectedGetsLatestFirst()
        {
            CreateFullExportsAndVm();
            var versionChecker = Mock.Get(_shellViewModel.Version);
            versionChecker.Setup(v => v.CommunityPageUri).Verifiable();
            _shellViewModel.ShowStartPageAsync();
            versionChecker.Verify(v => v.CommunityPageUri);
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("ShowStartPage")]
        public void MainViewModelShowStartPageExpectedTracking()
        {
            var _applicationTrackerMock = new Mock<IApplicationTracker>();
            _applicationTrackerMock.Setup(controller => controller.TrackEvent(It.IsAny<string>(), It.IsAny<string>()));
            CustomContainer.Register(_applicationTrackerMock.Object);
            CreateFullExportsAndVm();
            var versionChecker = Mock.Get(_shellViewModel.Version);
            versionChecker.Setup(v => v.CommunityPageUri).Verifiable();


            _shellViewModel.ShowStartPageAsync();
            versionChecker.Verify(v => v.CommunityPageUri);
            _applicationTrackerMock.Verify(controller => controller.TrackEvent(It.IsAny<string>(), It.IsAny<string>()), Times.AtLeastOnce());
        }
        #endregion

        #region ShowDependencies


        #endregion


        #region DeactivateItem

        [TestMethod]
        public void MainViewModelDeactivateItemWithPreviousItemNotOpenExpectedNoActiveItem()
        {
            var wsiRepo = new Mock<IWorkspaceItemRepository>();
            wsiRepo.Setup(r => r.WorkspaceItems).Returns(() => new List<IWorkspaceItem>());
            wsiRepo.Setup(r => r.Write()).Verifiable();

            #region Setup ImportService - GRRR!

            SetupImportServiceForPersistenceTests(wsiRepo);

            #endregion

            var resourceRepo = new Mock<IResourceRepository>();
            resourceRepo.Setup(r => r.Save(It.IsAny<IResourceModel>())).Verifiable();

            var envConn = SetupMockConnection();
            var env = new Mock<IServer>();
            envConn.Setup(conn => conn.ServerEvents).Returns(new Mock<IEventPublisher>().Object);
            env.Setup(e => e.ResourceRepository).Returns(resourceRepo.Object);
            env.Setup(e => e.Connection).Returns(envConn.Object);

            var envRepo = new Mock<IServerRepository>();
            envRepo.Setup(r => r.All()).Returns(new[] { env.Object });
            envRepo.Setup(r => r.Source).Returns(env.Object);
            envRepo.Setup(r => r.Get(It.IsAny<Guid>())).Returns(env.Object);
            var asyncWorker = AsyncWorkerTests.CreateSynchronousAsyncWorker();
            var mockMainViewModel = new ShellViewModelPersistenceMock(envRepo.Object, asyncWorker.Object, false);
            var resourceID = Guid.NewGuid();
            var serverID = Guid.NewGuid();

            #region Setup WorkSurfaceContextViewModel1

            var resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(m => m.Environment).Returns(env.Object);
            resourceModel.Setup(m => m.ID).Returns(resourceID);
            resourceModel.Setup(m => m.ResourceName).Returns("Some resource name 4");
            var mockPopUp = Dev2MockFactory.CreateIPopup(MessageBoxResult.No);
            var workflowHelper = new Mock<IWorkflowHelper>();
            var designerViewModel = new WorkflowDesignerViewModel(new Mock<IEventAggregator>().Object, resourceModel.Object, workflowHelper.Object, mockPopUp.Object, new SynchronousAsyncWorker(), false);
            var contextViewModel1 = new WorkSurfaceContextViewModel(
                new WorkSurfaceKey { ResourceID = resourceID, ServerID = serverID, WorkSurfaceContext = designerViewModel.WorkSurfaceContext },
                designerViewModel);

            #endregion

            mockMainViewModel.Items.Add(contextViewModel1);

            mockMainViewModel.PopupProvider = Dev2MockFactory.CreateIPopup(MessageBoxResult.OK).Object;

            mockMainViewModel.ActivateItem(mockMainViewModel.Items[1]);
            mockMainViewModel.ActivateItem(mockMainViewModel.Items[0]);
            mockMainViewModel.CallDeactivate(mockMainViewModel.Items[1]);
            mockMainViewModel.CallDeactivate(mockMainViewModel.Items[0]);
            Assert.AreEqual(null, mockMainViewModel.ActiveItem);
        }


        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("MainViewModel_UnsavedWorkflowDialog")]
        public void MainViewModel_UnsavedWorkflowDialog_WhenXPressed_WorkflowRemainsOpen()
        {
            var wsiRepo = new Mock<IWorkspaceItemRepository>();
            wsiRepo.Setup(r => r.WorkspaceItems).Returns(() => new List<IWorkspaceItem>());
            wsiRepo.Setup(r => r.Write()).Verifiable();

            #region Setup ImportService - GRRR!

            SetupImportServiceForPersistenceTests(wsiRepo);

            #endregion

            var envRepo = new Mock<IServerRepository>();
            var resourceRepo = new Mock<IResourceRepository>();
            resourceRepo.Setup(r => r.Save(It.IsAny<IResourceModel>())).Verifiable();

            var envConn = SetupMockConnection();
            var env = new Mock<IServer>();
            envConn.Setup(conn => conn.ServerEvents).Returns(new Mock<IEventPublisher>().Object);
            env.Setup(e => e.ResourceRepository).Returns(resourceRepo.Object);
            env.Setup(e => e.Connection).Returns(envConn.Object);
            envRepo.Setup(r => r.All()).Returns(new[] { env.Object });
            envRepo.Setup(e => e.Source).Returns(env.Object);
            envRepo.Setup(e => e.Get(It.IsAny<Guid>())).Returns(env.Object);
            var asyncWorker = AsyncWorkerTests.CreateSynchronousAsyncWorker();
            var mockMainViewModel = new ShellViewModelPersistenceMock(envRepo.Object, asyncWorker.Object, false);
            var resourceID = Guid.NewGuid();
            var serverID = Guid.NewGuid();

            CustomContainer.DeRegister<IServerRepository>();
            CustomContainer.Register(envRepo.Object);
            #region Setup WorkSurfaceContextViewModel1

            var resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(m => m.Environment).Returns(env.Object);
            resourceModel.Setup(m => m.ID).Returns(resourceID);
            resourceModel.Setup(r => r.IsAuthorized(AuthorizationContext.Contribute)).Returns(true);
            resourceModel.Setup(m => m.ResourceName).Returns("Some resource name 3");
            var mockPopUp = Dev2MockFactory.CreateIPopup(MessageBoxResult.No);
            mockPopUp.Setup(m => m.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Verifiable();
            var workflowHelper = new Mock<IWorkflowHelper>();
            var designerViewModel = new WorkflowDesignerViewModel(new Mock<IEventAggregator>().Object, resourceModel.Object, workflowHelper.Object, mockPopUp.Object, new SynchronousAsyncWorker(), false);
            var contextViewModel1 = new WorkSurfaceContextViewModel(
                new WorkSurfaceKey { ResourceID = resourceID, ServerID = serverID, WorkSurfaceContext = designerViewModel.WorkSurfaceContext },
                designerViewModel);

            #endregion

            mockMainViewModel.Items.Add(contextViewModel1);
            mockMainViewModel.PopupProvider = mockPopUp.Object;

            mockMainViewModel.ActivateItem(mockMainViewModel.Items[0]);
            mockMainViewModel.ActivateItem(mockMainViewModel.Items[1]);
            mockMainViewModel.CallDeactivate(mockMainViewModel.Items[1]);
            Assert.AreEqual(mockMainViewModel.Items[1], mockMainViewModel.ActiveItem);
            mockPopUp.Verify(m => m.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Once());
        }

        // PBI 9405 - 2013.06.13 - Massimo.Guerrera
        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("MainViewModel")]
        [Description("When closing a new workflow with nothing on it the pop up should not show")]
        public void MainViewModel_UnitTest_CloseNewWorkflowWithNoChanges_PopUpMustNotShow()
        {
            var wsiRepo = new Mock<IWorkspaceItemRepository>();
            wsiRepo.Setup(r => r.WorkspaceItems).Returns(() => new List<IWorkspaceItem>());
            wsiRepo.Setup(r => r.Write()).Verifiable();

            #region Setup ImportService - GRRR!

            SetupImportServiceForPersistenceTests(wsiRepo);

            #endregion

            var resourceRepo = new Mock<IResourceRepository>();
            resourceRepo.Setup(r => r.Save(It.IsAny<IResourceModel>())).Verifiable();
            var envRepo = new Mock<IServerRepository>();
            var envConn = SetupMockConnection();
            envConn.Setup(conn => conn.ServerEvents).Returns(new Mock<IEventPublisher>().Object);
            var env = new Mock<IServer>();
            env.Setup(e => e.ResourceRepository).Returns(resourceRepo.Object);
            env.Setup(e => e.Connection).Returns(envConn.Object);
            envRepo.Setup(r => r.All()).Returns(new List<IServer>(new[] { env.Object }));
            envRepo.Setup(r => r.Source).Returns(env.Object);
            envRepo.Setup(r => r.Get(It.IsAny<Guid>())).Returns(env.Object);

            var asyncWorker = AsyncWorkerTests.CreateSynchronousAsyncWorker();
            var mockMainViewModel = new ShellViewModelPersistenceMock(envRepo.Object, asyncWorker.Object, false);
            var resourceID = Guid.NewGuid();
            var serverID = Guid.NewGuid();

            #region Setup WorkSurfaceContextViewModel1

            var resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(m => m.Environment).Returns(env.Object);
            resourceModel.Setup(m => m.ID).Returns(resourceID);
            resourceModel.Setup(m => m.IsNewWorkflow).Returns(true);
            resourceModel.Setup(m => m.IsWorkflowSaved).Returns(true);
            resourceModel.Setup(m => m.ResourceName).Returns("Some resource name 2");
            var workflowHelper = new Mock<IWorkflowHelper>();
            var designerViewModel = new WorkflowDesignerViewModel(resourceModel.Object, workflowHelper.Object, false);
            var contextViewModel1 = new WorkSurfaceContextViewModel(
                new WorkSurfaceKey { ResourceID = resourceID, ServerID = serverID, WorkSurfaceContext = designerViewModel.WorkSurfaceContext },
                designerViewModel);

            #endregion

            mockMainViewModel.Items.Add(contextViewModel1);

            var mockPopUp = Dev2MockFactory.CreateIPopup(MessageBoxResult.OK);
            mockPopUp.Setup(m => m.Show()).Verifiable();

            mockMainViewModel.PopupProvider = mockPopUp.Object;

            mockMainViewModel.ActivateItem(mockMainViewModel.Items[0]);
            mockMainViewModel.ActivateItem(mockMainViewModel.Items[1]);
            mockMainViewModel.CallDeactivate(mockMainViewModel.Items[1]);
            Assert.AreEqual(mockMainViewModel.Items[0], mockMainViewModel.ActiveItem);
            mockPopUp.Verify(m => m.Show(), Times.Never());
        }

        // PBI 9405 - 2013.06.13 - Massimo.Guerrera
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("MainViewModel")]
        public void MainViewModel_RemoveResourceAndCloseTabMessage_PopUpMustNotShow()
        {
            var wsiRepo = new Mock<IWorkspaceItemRepository>();
            wsiRepo.Setup(r => r.WorkspaceItems).Returns(() => new List<IWorkspaceItem>());
            wsiRepo.Setup(r => r.Write()).Verifiable();

            #region Setup ImportService - GRRR!

            SetupImportServiceForPersistenceTests(wsiRepo);

            #endregion

            var resourceRepo = new Mock<IResourceRepository>();
            resourceRepo.Setup(r => r.Save(It.IsAny<IResourceModel>())).Verifiable();
            var envRepo = new Mock<IServerRepository>();
            var envConn = SetupMockConnection();
            envConn.Setup(conn => conn.ServerEvents).Returns(new Mock<IEventPublisher>().Object);

            var env = new Mock<IServer>();
            env.Setup(e => e.ResourceRepository).Returns(resourceRepo.Object);
            env.Setup(e => e.Connection).Returns(envConn.Object);
            envRepo.Setup(r => r.All()).Returns(new List<IServer>(new[] { env.Object }));
            envRepo.Setup(r => r.Source).Returns(env.Object);
            envRepo.Setup(r => r.Get(It.IsAny<Guid>())).Returns(env.Object);
            CustomContainer.DeRegister<IServerRepository>();
            CustomContainer.Register(envRepo.Object);
            var asyncWorker = AsyncWorkerTests.CreateSynchronousAsyncWorker();
            var mockMainViewModel = new ShellViewModelPersistenceMock(envRepo.Object, asyncWorker.Object, false);
            var resourceID = Guid.NewGuid();

            #region Setup WorkSurfaceContextViewModel1

            var resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(m => m.Environment).Returns(env.Object);
            resourceModel.Setup(m => m.ID).Returns(resourceID);
            resourceModel.Setup(m => m.IsNewWorkflow).Returns(true);
            resourceModel.Setup(m => m.IsWorkflowSaved).Returns(true);
            resourceModel.Setup(m => m.ResourceName).Returns("Some resource name 2");
            var workflowHelper = new Mock<IWorkflowHelper>();
            var designerViewModel = new WorkflowDesignerViewModel(resourceModel.Object, workflowHelper.Object, false);
            var contextViewModel1 = new WorkSurfaceContextViewModel(
                new WorkSurfaceKey { ResourceID = resourceID, ServerID = Guid.Empty, WorkSurfaceContext = designerViewModel.WorkSurfaceContext },
                designerViewModel);

            #endregion

            mockMainViewModel.Items.Add(contextViewModel1);

            var mockPopUp = Dev2MockFactory.CreateIPopup(MessageBoxResult.OK);
            mockPopUp.Setup(m => m.Show()).Verifiable();

            mockMainViewModel.PopupProvider = mockPopUp.Object;

            mockMainViewModel.ActivateItem(mockMainViewModel.Items[0]);
            mockMainViewModel.ActivateItem(mockMainViewModel.Items[1]);
            mockMainViewModel.Handle(new RemoveResourceAndCloseTabMessage(resourceModel.Object, false));
            Assert.AreEqual(mockMainViewModel.Items[0], mockMainViewModel.ActiveItem);
            mockPopUp.Verify(m => m.Show(), Times.Never());
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("MainViewModel")]
        public void MainViewModel_RemoveResourceAndCloseTabMessage_RemoveFromWorkspace_PopUpMustNotShow()
        {
            var wsiRepo = new Mock<IWorkspaceItemRepository>();
            wsiRepo.Setup(r => r.WorkspaceItems).Returns(() => new List<IWorkspaceItem>());
            wsiRepo.Setup(r => r.Write()).Verifiable();

            #region Setup ImportService - GRRR!

            SetupImportServiceForPersistenceTests(wsiRepo);

            #endregion

            var resourceRepo = new Mock<IResourceRepository>();
            resourceRepo.Setup(r => r.Save(It.IsAny<IResourceModel>())).Verifiable();
            var envRepo = new Mock<IServerRepository>();
            var envConn = SetupMockConnection();
            envConn.Setup(conn => conn.ServerEvents).Returns(new Mock<IEventPublisher>().Object);
            var env = new Mock<IServer>();
            env.Setup(e => e.ResourceRepository).Returns(resourceRepo.Object);
            env.Setup(e => e.Connection).Returns(envConn.Object);
            envRepo.Setup(r => r.All()).Returns(new List<IServer>(new[] { env.Object }));
            envRepo.Setup(r => r.Source).Returns(env.Object);
            envRepo.Setup(r => r.Get(It.IsAny<Guid>())).Returns(env.Object);

            var asyncWorker = AsyncWorkerTests.CreateSynchronousAsyncWorker();
            var mockMainViewModel = new ShellViewModelPersistenceMock(envRepo.Object, asyncWorker.Object);
            var resourceID = Guid.NewGuid();

            #region Setup WorkSurfaceContextViewModel1

            var resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(m => m.Environment).Returns(env.Object);
            resourceModel.Setup(m => m.ID).Returns(resourceID);
            resourceModel.Setup(m => m.IsNewWorkflow).Returns(true);
            resourceModel.Setup(m => m.IsWorkflowSaved).Returns(true);
            resourceModel.Setup(m => m.ResourceName).Returns("Some resource name 2");
            var workflowHelper = new Mock<IWorkflowHelper>();
            var designerViewModel = new WorkflowDesignerViewModel(resourceModel.Object, workflowHelper.Object, false);
            var contextViewModel1 = new WorkSurfaceContextViewModel(
                new WorkSurfaceKey { ResourceID = resourceID, ServerID = Guid.Empty, WorkSurfaceContext = designerViewModel.WorkSurfaceContext },
                designerViewModel);

            #endregion

            mockMainViewModel.Items.Add(contextViewModel1);

            var mockPopUp = Dev2MockFactory.CreateIPopup(MessageBoxResult.OK);
            mockPopUp.Setup(m => m.Show()).Verifiable();

            mockMainViewModel.PopupProvider = mockPopUp.Object;

            mockMainViewModel.ActivateItem(mockMainViewModel.Items[0]);
            mockMainViewModel.ActivateItem(mockMainViewModel.Items[1]);
            mockMainViewModel.Handle(new RemoveResourceAndCloseTabMessage(mockMainViewModel.Items[1].ContextualResourceModel, false));
            Assert.AreEqual(mockMainViewModel.Items[0], mockMainViewModel.ActiveItem);
            mockPopUp.Verify(m => m.Show(), Times.Never());
        }
        
        #endregion

        #region OnDeactivate

        // PBI 9397 - 2013.06.09 - TWR: added
        [TestMethod]
        public void MainViewModelOnDeactivateWithTrueExpectedSavesWorkspaceItems()
        {
            var wsiRepo = new Mock<IWorkspaceItemRepository>();
            wsiRepo.Setup(r => r.WorkspaceItems).Returns(() => new List<IWorkspaceItem>());
            wsiRepo.Setup(r => r.Write()).Verifiable();

            #region Setup ImportService - GRRR!

            SetupImportServiceForPersistenceTests(wsiRepo);

            #endregion

            var envRepo = new Mock<IServerRepository>();
            ICollection<IServer> envColletion = new List<IServer>();
            var env = Dev2MockFactory.SetupEnvironmentModel();
            env.Setup(mock => mock.IsConnected).Returns(true);
            envColletion.Add(env.Object);

            envRepo.Setup(mock => mock.All()).Returns(envColletion);
            envRepo.Setup(mock => mock.Source).Returns(env.Object);

            var asyncWorker = AsyncWorkerTests.CreateSynchronousAsyncWorker();
            var viewModel = new ShellViewModelPersistenceMock(envRepo.Object, asyncWorker.Object, false);

            viewModel.TestClose();
            wsiRepo.Verify(r => r.Write());
        }

        // PBI 9397 - 2013.06.09 - TWR: added
        [TestMethod]
        public void MainViewModelOnDeactivateWithTrueExpectedSavesResourceModels()
        {
            var wsiRepo = new Mock<IWorkspaceItemRepository>();
            wsiRepo.Setup(r => r.WorkspaceItems).Returns(() => new List<IWorkspaceItem>());
            SetupImportServiceForPersistenceTests(wsiRepo);

            var resourceID = Guid.NewGuid();
            var serverID = Guid.NewGuid();

            #region Setup resourceModel

            var resourceRepo = new Mock<IResourceRepository>();
            resourceRepo.Setup(r => r.Save(It.IsAny<IResourceModel>())).Verifiable();
            resourceRepo.Setup(r => r.FetchResourceDefinition(It.IsAny<IServer>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>())).Returns(new ExecuteMessage());

            var envConn = SetupMockConnection();
            var env = new Mock<IServer>();
            envConn.Setup(conn => conn.ServerEvents).Returns(new Mock<IEventPublisher>().Object);
            env.Setup(e => e.ResourceRepository).Returns(resourceRepo.Object);
            env.Setup(e => e.Connection).Returns(envConn.Object);
            env.Setup(e => e.IsConnected).Returns(true);

            var resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(m => m.Environment).Returns(env.Object);
            resourceModel.Setup(m => m.ID).Returns(resourceID);
            resourceModel.Setup(m => m.UserPermissions).Returns(Permissions.Contribute);
            resourceModel.Setup(m => m.ResourceName).Returns("Some resource name 4");
            #endregion

            var mockPopUp = Dev2MockFactory.CreateIPopup(MessageBoxResult.OK);
            var workflowHelper = new Mock<IWorkflowHelper>();
            var designerViewModel = new WorkflowDesignerViewModel(new Mock<IEventAggregator>().Object, resourceModel.Object, workflowHelper.Object, mockPopUp.Object, new SynchronousAsyncWorker(), false);
            var contextViewModel = new WorkSurfaceContextViewModel(
                new WorkSurfaceKey { ResourceID = resourceID, ServerID = serverID, WorkSurfaceContext = designerViewModel.WorkSurfaceContext },
                designerViewModel);

            var envRepo = new Mock<IServerRepository>();
            envRepo.Setup(r => r.All()).Returns(new[] { env.Object });
            envRepo.Setup(e => e.Source).Returns(env.Object);
            var asyncWorker = AsyncWorkerTests.CreateSynchronousAsyncWorker();
            var viewModel = new ShellViewModelPersistenceMock(envRepo.Object, asyncWorker.Object, false);
            viewModel.Items.Add(contextViewModel);

            viewModel.TestClose();

        }

        [TestMethod]
        public void MainViewModelOnDeactivateWithTrueExpectedSavesResourceModels_WhenEnvironmentNotConnectedDoesNotCallSave()
        {
            var wsiRepo = new Mock<IWorkspaceItemRepository>();
            wsiRepo.Setup(r => r.WorkspaceItems).Returns(() => new List<IWorkspaceItem>());

            SetupImportServiceForPersistenceTests(wsiRepo);

            var resourceID = Guid.NewGuid();
            var serverID = Guid.NewGuid();

            #region Setup resourceModel

            var resourceRepo = new Mock<IResourceRepository>();
            resourceRepo.Setup(r => r.Save(It.IsAny<IResourceModel>())).Verifiable();

            var envConn = SetupMockConnection();
            var env = new Mock<IServer>();
            envConn.Setup(conn => conn.ServerEvents).Returns(new Mock<IEventPublisher>().Object);
            env.Setup(e => e.ResourceRepository).Returns(resourceRepo.Object);
            env.Setup(e => e.Connection).Returns(envConn.Object);
            env.Setup(e => e.IsConnected).Returns(false);

            var resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(m => m.Environment).Returns(env.Object);
            resourceModel.Setup(m => m.ID).Returns(resourceID);
            resourceModel.Setup(m => m.ResourceName).Returns("Some resource name 5");
            #endregion

            var mockPopUp = Dev2MockFactory.CreateIPopup(MessageBoxResult.No);
            var workflowHelper = new Mock<IWorkflowHelper>();
            var designerViewModel = new WorkflowDesignerViewModel(new Mock<IEventAggregator>().Object, resourceModel.Object, workflowHelper.Object, mockPopUp.Object, new SynchronousAsyncWorker(), false);
            var contextViewModel = new WorkSurfaceContextViewModel(
                new WorkSurfaceKey { ResourceID = resourceID, ServerID = serverID, WorkSurfaceContext = designerViewModel.WorkSurfaceContext },
                designerViewModel);

            var envRepo = new Mock<IServerRepository>();
            envRepo.Setup(r => r.All()).Returns(new[] { env.Object });
            envRepo.Setup(e => e.Source).Returns(env.Object);
            var viewModel = new ShellViewModelPersistenceMock(envRepo.Object, false);
            viewModel.Items.Add(contextViewModel);

            viewModel.TestClose();
            
            resourceRepo.Verify(r => r.Save(It.IsAny<IResourceModel>()), Times.Never());
        }

        #endregion

        #region SetupImportServiceForPersistenceTests

        static void SetupImportServiceForPersistenceTests(Mock<IWorkspaceItemRepository> wsiRepo)
        {
            var workspaceItemRepository = new WorkspaceItemRepository(wsiRepo.Object);
            CustomContainer.Register<IWorkspaceItemRepository>(workspaceItemRepository);
            CustomContainer.Register(new Mock<Common.Interfaces.Studio.Controller.IPopupController>().Object);
        }

        #endregion

        #region BrowserPopupController

        [TestMethod]
        public void MainViewModelShowCommunityPageExpectedInvokesConstructorsBrowserPopupController()
        {
            var popupController = new Mock<IBrowserPopupController>();
            popupController.Setup(p => p.ShowPopup(It.IsAny<string>())).Verifiable();
            CustomContainer.Register(new Mock<IWindowManager>().Object);
            var envRepo = new Mock<IServerRepository>();
            envRepo.Setup(e => e.All()).Returns(new List<IServer>());

            var env = new Mock<IServer>();
            var mockEnvironmentConnection = SetupMockConnection();
            env.Setup(serv => serv.Connection).Returns(mockEnvironmentConnection.Object);

            envRepo.Setup(e => e.Source).Returns(env.Object);

            var vieFactory = new Mock<IViewFactory>();
            var viewMock = new Mock<IView>();
            CustomContainer.DeRegister<IServerRepository>();
            CustomContainer.Register(envRepo.Object);
            vieFactory.Setup(factory => factory.GetViewGivenServerResourceType(It.IsAny<string>())).Returns(viewMock.Object);
            var vm = new ShellViewModel(new Mock<IEventAggregator>().Object, new Mock<IAsyncWorker>().Object, envRepo.Object, new Mock<IVersionChecker>().Object, vieFactory.Object, false, popupController.Object);
            vm.ShowCommunityPage();

            popupController.Verify(p => p.ShowPopup(It.IsAny<string>()));
        }

        [TestMethod]
        public void MainViewModelConstructorWithNullBrowserPopupControllerExpectedCreatesExternalBrowserPopupController()
        {
            var mockEventAggregator = new Mock<IEventAggregator>();
            var envRepo = new Mock<IServerRepository>();
            CustomContainer.Register(new Mock<IWindowManager>().Object);
            envRepo.Setup(e => e.All()).Returns(new List<IServer>());

            var env = new Mock<IServer>();
            var mockEnvironmentConnection = SetupMockConnection();
            env.Setup(serv => serv.Connection).Returns(mockEnvironmentConnection.Object);

            envRepo.Setup(e => e.Source).Returns(env.Object);

            var vieFactory = new Mock<IViewFactory>();
            var viewMock = new Mock<IView>();
            vieFactory.Setup(factory => factory.GetViewGivenServerResourceType(It.IsAny<string>())).Returns(viewMock.Object);
            var vm = new ShellViewModel(mockEventAggregator.Object, new Mock<IAsyncWorker>().Object, envRepo.Object, new Mock<IVersionChecker>().Object, vieFactory.Object, false, null);
            Assert.IsInstanceOfType(vm.BrowserPopupController, typeof(ExternalBrowserPopupController));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void MainViewModelConstructorWithNullVersionCheckerExpectedThrowsArgumentNullException()
        {
            var mockEventAggregator = new Mock<IEventAggregator>();
            var envRepo = new Mock<IServerRepository>();
            envRepo.Setup(e => e.All()).Returns(new List<IServer>());
            envRepo.Setup(e => e.Source).Returns(new Mock<IServer>().Object);
            var vieFactory = new Mock<IViewFactory>();
            var viewMock = new Mock<IView>();
            vieFactory.Setup(factory => factory.GetViewGivenServerResourceType(It.IsAny<string>())).Returns(viewMock.Object);
            new ShellViewModel(mockEventAggregator.Object, new Mock<IAsyncWorker>().Object, envRepo.Object, null, vieFactory.Object);
        }

        #endregion

        #region ActiveEnvironment

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("MainViewModel_AuthorizeCommands")]
        public void MainViewModel_AuthorizeCommands_AuthorizationContextIsCorrect()
        {
            //------------Setup for test--------------------------    

            //------------Execute Test---------------------------
            CreateFullExportsAndVmWithEmptyRepo();

            //------------Assert Results-------------------------
            Assert.AreEqual(AuthorizationContext.Contribute, _shellViewModel.NewServiceCommand.AuthorizationContext);
            Assert.AreEqual(AuthorizationContext.Administrator, _shellViewModel.SettingsCommand.AuthorizationContext);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("MainViewModel_AuthorizeCommands")]
        public void MainViewModel_AuthorizeCommands_ActiveEnvironmentChanged_UpdateContextInvoked()
        {
            //------------Setup for test--------------------------            
            CreateFullExportsAndVmWithEmptyRepo();

            Assert.IsNull(_shellViewModel.NewServiceCommand.AuthorizationService);
            Assert.IsNull(_shellViewModel.SettingsCommand.AuthorizationService);

            var authService = new Mock<IAuthorizationService>();

            var env = new Mock<IServer>();
            env.Setup(e => e.AuthorizationService).Returns(authService.Object);
            env.Setup(e => e.IsConnected).Returns(true);
            var mockEnvironmentConnection = SetupMockConnection();
            env.SetupGet(it => it.Connection).Returns(mockEnvironmentConnection.Object);

            //------------Execute Test---------------------------
            _shellViewModel.ActiveServer = env.Object;

            //------------Assert Results-------------------------
            Assert.AreSame(authService.Object, _shellViewModel.NewServiceCommand.AuthorizationService);
            Assert.AreSame(authService.Object, _shellViewModel.SettingsCommand.AuthorizationService);
        }

        private static Mock<IEnvironmentConnection> SetupMockConnection()
        {
            var uri = new Uri("http://bravo.com/");
            var mockEnvironmentConnection = new Mock<IEnvironmentConnection>();
            mockEnvironmentConnection.Setup(a => a.AppServerUri).Returns(uri);
            mockEnvironmentConnection.Setup(a => a.AuthenticationType).Returns(Dev2.Runtime.ServiceModel.Data.AuthenticationType.Public);
            mockEnvironmentConnection.Setup(a => a.WebServerUri).Returns(uri);
            mockEnvironmentConnection.Setup(a => a.ID).Returns(Guid.Empty);
            return mockEnvironmentConnection;
        }

        [TestMethod]
        public void IsActiveEnvironmentConnectExpectFalseWithNullEnvironment()
        {
            CreateFullExportsAndVm();
            _shellViewModel.ActiveItem = _shellViewModel.Items.FirstOrDefault(c => c.WorkSurfaceViewModel.GetType() == typeof(HelpViewModel));
            var actual = _shellViewModel.IsActiveServerConnected();
            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void GetMenuPanelWidth()
        {
            CreateFullExportsAndVm();
            Assert.IsFalse(_shellViewModel.MenuExpanded);
            Assert.AreEqual(60, _shellViewModel.MenuPanelWidth);

            _shellViewModel.MenuExpanded = true;
            Assert.IsTrue(_shellViewModel.MenuExpanded);
            Assert.AreEqual(60, _shellViewModel.MenuPanelWidth);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("MainViewModel_SetActiveServer")]
        public void MainViewModel_SetActiveServer_Scenerio_Result()
        {
            //------------Setup for test--------------------------
            CreateFullExportsAndVm();

            var newSelectedConnection = new Mock<IServer>();
            var newSelectedConnectionEnvironmentId = Guid.NewGuid();
            newSelectedConnection.SetupGet(it => it.DisplayName).Returns("Nonlocalhost");
            newSelectedConnection.SetupGet(it => it.EnvironmentID).Returns(newSelectedConnectionEnvironmentId);
            newSelectedConnection.SetupGet(it => it.HasLoaded).Returns(true);
            newSelectedConnection.SetupGet(it => it.IsConnected).Returns(true);

            //------------Execute Test---------------------------
            _shellViewModel.SetActiveServer(newSelectedConnection.Object);

            //------------Assert Results-------------------------
            Assert.AreEqual(_shellViewModel.ActiveServer, newSelectedConnection.Object);
        }

        #endregion

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ShellViewModel))]
        public void MainViewModel_GetOutputsFromWorkflow()
        {
            //------------Setup for test--------------------------
            var toolboxViewModel = new Mock<IToolboxViewModel>().Object;
            CustomContainer.Register(toolboxViewModel);
            CreateFullExportsAndVm();

            var resourceId = Guid.NewGuid();
            var mockResourceModel = new Mock<IContextualResourceModel>();
            mockResourceModel.Setup(o => o.DataList).Returns("<DataList></DataList>");

            var mockResourceRepo = new Mock<IResourceRepository>();
            mockResourceRepo.Setup(o => o.LoadContextualResourceModel(resourceId)).Returns(mockResourceModel.Object);

            var env = SetupEnvironment();
            env.Setup(o => o.ResourceRepository).Returns(mockResourceRepo.Object);

            var mockServerRepository = new Mock<IServerRepository>();
            mockServerRepository.Setup(o => o.Source).Returns(env.Object);
            mockServerRepository.Setup(o => o.ActiveServer).Returns(env.Object);
            mockServerRepository.Setup(o => o.All()).Returns(new List<IServer>());

            //------------Execute Test---------------------------

            _shellViewModel = new ShellViewModelPersistenceMock(mockServerRepository.Object);

            //------------Assert Results-------------------------
            var outputs = _shellViewModel.GetOutputsFromWorkflow(resourceId);
            Assert.IsNotNull(outputs);
            Assert.AreEqual(0, outputs.Count);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("MainViewModel_ViewSwagger")]
        public void MainViewModel_ViewSwagger_Handle_Result()
        {
            //------------Setup for test--------------------------
            CreateFullExportsAndVm();

            var env = SetupEnvironment();

            //------------Execute Test---------------------------
            _shellViewModel.ActiveServer = env.Object;
            //------------Assert Results-------------------------
            Assert.IsNotNull(_shellViewModel.ActiveServer);
            Assert.IsTrue(_shellViewModel.ActiveServer.IsConnected);
            Assert.IsTrue(_shellViewModel.ActiveServer.CanStudioExecute);

            var source = new Mock<IExplorerItemViewModel>();
            source.Setup(a => a.ResourceId).Returns(Guid.NewGuid);
            source.Setup(a => a.ResourceName).Returns("TestResourceName");
            source.Setup(a => a.ResourceType).Returns("WorkflowService");
            source.Setup(a => a.IsService).Returns(true);
            source.Setup(a => a.IsFolder).Returns(false);

            var viewModel = new Mock<IShellViewModel>();
            var server = new Mock<IServer>();
            server.SetupGet(server1 => server1.IsConnected).Returns(true);
            viewModel.SetupGet(model => model.ActiveServer).Returns(server.Object);
            viewModel.SetupGet(model => model.LocalhostServer).Returns(server.Object);
            viewModel.SetupGet(model => model.ActiveServer.EnvironmentID).Returns(Guid.NewGuid);

            _shellViewModel.ViewSwagger(source.Object.ResourceId, viewModel.Object.ActiveServer);
        }


        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("MainViewModel_ViewViewApisJson")]
        public void MainViewModel_ViewViewApisJson_HandleFolder_Result()
        {
            //------------Setup for test--------------------------
            CreateFullExportsAndVm();

            var env = SetupEnvironment();

            //------------Execute Test---------------------------
            _shellViewModel.ActiveServer = env.Object;
            //------------Assert Results-------------------------
            Assert.IsNotNull(_shellViewModel.ActiveServer);
            Assert.IsTrue(_shellViewModel.ActiveServer.IsConnected);
            Assert.IsTrue(_shellViewModel.ActiveServer.CanStudioExecute);

            var source = new Mock<IExplorerItemViewModel>();
            source.Setup(a => a.ResourceId).Returns(Guid.NewGuid);
            source.Setup(a => a.ResourceName).Returns("TestResourceFolder");
            source.Setup(a => a.ResourceType).Returns("Folder");
            source.Setup(a => a.ResourcePath).Returns("Path");
            source.Setup(a => a.IsService).Returns(false);
            source.Setup(a => a.IsFolder).Returns(true);

            _shellViewModel.ViewApisJson(source.Object.ResourcePath, new Uri("http://localhost:3142"));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("MainViewModel_ViewViewApisJson")]
        public void MainViewModel_ViewViewApisJson_HandleServer_Result()
        {
            //------------Setup for test--------------------------
            CreateFullExportsAndVm();

            var env = SetupEnvironment();

            //------------Execute Test---------------------------
            _shellViewModel.ActiveServer = env.Object;
            //------------Assert Results-------------------------
            Assert.IsNotNull(_shellViewModel.ActiveServer);
            Assert.IsTrue(_shellViewModel.ActiveServer.IsConnected);
            Assert.IsTrue(_shellViewModel.ActiveServer.CanStudioExecute);

            var source = new Mock<IEnvironmentViewModel>();
            source.Setup(a => a.ResourceId).Returns(Guid.NewGuid);
            source.Setup(a => a.ResourceName).Returns("TestResourceFolder");
            source.Setup(a => a.ResourceType).Returns("Server");
            source.Setup(a => a.IsServer).Returns(true);

            _shellViewModel.ViewApisJson(source.Object.ResourcePath, new Uri("http://localhost:3142"));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("MainViewModel_OpenResource")]
        public void MainViewModel_OpenResource_HandleVersion_Result()
        {
            //------------Setup for test--------------------------
            CreateFullExportsAndVm();

            var env = SetupEnvironment();

            //------------Execute Test---------------------------
            _shellViewModel.ActiveServer = env.Object;
            //------------Assert Results-------------------------
            Assert.IsNotNull(_shellViewModel.ActiveServer);
            Assert.IsTrue(_shellViewModel.ActiveServer.IsConnected);
            Assert.IsTrue(_shellViewModel.ActiveServer.CanStudioExecute);

            var source = new Mock<IExplorerItemViewModel>();
            source.Setup(a => a.ResourceId).Returns(Guid.NewGuid);
            source.Setup(a => a.ResourceName).Returns("TestResourceName");
            source.Setup(a => a.ResourceType).Returns("Version");

            var viewModel = new Mock<IShellViewModel>();
            var server = new Mock<IServer>();
            server.SetupGet(server1 => server1.IsConnected).Returns(true);
            viewModel.SetupGet(model => model.ActiveServer).Returns(server.Object);
            viewModel.SetupGet(model => model.LocalhostServer).Returns(server.Object);
            viewModel.SetupGet(model => model.ActiveServer.EnvironmentID).Returns(Guid.NewGuid);

            _shellViewModel.OpenResource(source.Object.ResourceId, viewModel.Object.ActiveServer.EnvironmentID, viewModel.Object.ActiveServer);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("MainViewModel_OpenMergeConflictsView")]
        public void MainViewModel_OpenMergeConflictsView_HandleVersion_Result()
        {
            //------------Setup for test--------------------------
            CreateFullExportsAndVm();

            var env = SetupEnvironment();

            //------------Execute Test---------------------------
            _shellViewModel.ActiveServer = env.Object;
            //------------Assert Results-------------------------
            Assert.IsNotNull(_shellViewModel.ActiveServer);
            Assert.IsTrue(_shellViewModel.ActiveServer.IsConnected);
            Assert.IsTrue(_shellViewModel.ActiveServer.CanStudioExecute);
            var treeItem = new Mock<IExplorerTreeItem>();
            var popUpController = new Mock<Common.Interfaces.Studio.Controller.IPopupController>();
            var action = new Action<IExplorerItemViewModel>(p => { });
            var currentItem = new Mock<IContextualResourceModel>();
            currentItem.Setup(p => p.Environment).Returns(new Mock<IServer>().Object);
            var difItem = new Mock<IContextualResourceModel>();
            var resourceRepo = new Mock<IResourceRepository>();
            resourceRepo.Setup(p => p.LoadContextualResourceModel(It.IsAny<Guid>())).Returns(currentItem.Object);
            var viewModel = new Mock<IShellViewModel>();
            var server = new Mock<IServer>();
            var serverDef = new Mock<IServer>();
            serverDef.Setup(p => p.ResourceRepository.LoadContextualResourceModel(It.IsAny<Guid>())).Returns(difItem.Object);
            server.Setup(p => p.ResourceRepository).Returns(resourceRepo.Object);
            server.SetupGet(server1 => server1.IsConnected).Returns(true);
            viewModel.SetupGet(model => model.ActiveServer).Returns(server.Object);
            viewModel.SetupGet(model => model.LocalhostServer).Returns(server.Object);
            viewModel.SetupGet(model => model.ActiveServer.EnvironmentID).Returns(Guid.NewGuid);
            var source = new ExplorerItemViewModel(server.Object, treeItem.Object, action, _shellViewModel, popUpController.Object);
            var contextManager = new Mock<IWorksurfaceContextManager>();
            contextManager.Setup(p => p.ViewMergeConflictsService(currentItem.Object, difItem.Object, It.IsAny<bool>(), It.IsAny<IWorkSurfaceKey>()));
            var privateObject = new PrivateObject(_shellViewModel);
            var currentSurfaceManager = (IWorksurfaceContextManager)privateObject.GetField("_worksurfaceContextManager");
            privateObject.SetField("_worksurfaceContextManager", contextManager.Object);
            _shellViewModel.OpenMergeConflictsView(source, viewModel.Object.ActiveServer.EnvironmentID, serverDef.Object);
            contextManager.Verify(p => p.ViewMergeConflictsService(currentItem.Object, difItem.Object, It.IsAny<bool>(), It.IsAny<IWorkSurfaceKey>()));
            privateObject.SetField("_worksurfaceContextManager", currentSurfaceManager);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("MainViewModel_OpenResource")]
        public void MainViewModel_OpenResource_Handle_Result()
        {
            //------------Setup for test--------------------------
            CreateFullExportsAndVm();

            var env = SetupEnvironment();

            //------------Execute Test---------------------------
            _shellViewModel.ActiveServer = env.Object;
            //------------Assert Results-------------------------
            Assert.IsNotNull(_shellViewModel.ActiveServer);
            Assert.IsTrue(_shellViewModel.ActiveServer.IsConnected);
            Assert.IsTrue(_shellViewModel.ActiveServer.CanStudioExecute);

            var source = new Mock<IExplorerItemViewModel>();
            source.Setup(a => a.ResourceId).Returns(Guid.NewGuid);
            source.Setup(a => a.ResourceName).Returns("TestResourceName");
            source.Setup(a => a.ResourceType).Returns("WorkflowService");
            source.Setup(a => a.IsService).Returns(true);
            source.Setup(a => a.IsFolder).Returns(false);

            var viewModel = new Mock<IShellViewModel>();
            var server = new Mock<IServer>();
            server.SetupGet(server1 => server1.IsConnected).Returns(true);
            viewModel.SetupGet(model => model.ActiveServer).Returns(server.Object);
            viewModel.SetupGet(model => model.LocalhostServer).Returns(server.Object);
            viewModel.SetupGet(model => model.ActiveServer.EnvironmentID).Returns(Guid.NewGuid);

            _shellViewModel.OpenResource(source.Object.ResourceId, viewModel.Object.ActiveServer.EnvironmentID, viewModel.Object.ActiveServer);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("MainViewModel_EditSqlServerSource")]
        [DeploymentItem("Warewolf.Studio.Themes.Luna.dll")]
        [DeploymentItem("InfragisticsWPF4.DataPresenter.v15.1.dll")]
        public void MainViewModel_EditSqlServerSource_Handle_Result()
        {
            //------------Setup for test--------------------------
            CreateFullExportsAndVm();

            var env = SetupEnvironment();

            //------------Execute Test---------------------------
            _shellViewModel.ActiveServer = env.Object;
            //------------Assert Results-------------------------
            Assert.IsNotNull(_shellViewModel.ActiveServer);
            Assert.IsTrue(_shellViewModel.ActiveServer.IsConnected);
            Assert.IsTrue(_shellViewModel.ActiveServer.CanStudioExecute);

            var source = new Mock<IDbSource>();
            source.Setup(a => a.Name).Returns("TestDatabase");
            var view = new Mock<IView>();
            var mockWM = new Mock<IWorksurfaceContextManager>();
            mockWM.Setup(manager => manager.EditSqlServerResource(It.IsAny<IDbSource>(), view.Object)).Verifiable();
            _shellViewModel.WorksurfaceContextManager = mockWM.Object;
            _shellViewModel.WorksurfaceContextManager.EditSqlServerResource(source.Object, view.Object);
            mockWM.Verify(manager => manager.EditSqlServerResource(It.IsAny<IDbSource>(), view.Object));
            _shellViewModel.EditSqlServerResource(source.Object);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("MainViewModel_EditMySqlSource")]
        [DeploymentItem("Warewolf.Studio.Themes.Luna.dll")]
        [DeploymentItem("InfragisticsWPF4.Controls.Interactions.XamDialogWindow.v15.1.dll")]
        [DeploymentItem("InfragisticsWPF4.DataPresenter.v15.1.dll")]
        public void MainViewModel_EditMySqlSource_Handle_Result()
        {
            //------------Setup for test--------------------------
            CreateFullExportsAndVm();

            var env = SetupEnvironment();

            //------------Execute Test---------------------------
            _shellViewModel.ActiveServer = env.Object;
            //------------Assert Results-------------------------
            Assert.IsNotNull(_shellViewModel.ActiveServer);
            Assert.IsTrue(_shellViewModel.ActiveServer.IsConnected);
            Assert.IsTrue(_shellViewModel.ActiveServer.CanStudioExecute);

            var source = new Mock<IDbSource>();
            source.Setup(a => a.Name).Returns("TestDatabase");
            var view = new Mock<IView>();
            var mockWM = new Mock<IWorksurfaceContextManager>();
            mockWM.Setup(manager => manager.EditMySqlResource(It.IsAny<IDbSource>(), view.Object)).Verifiable();
            _shellViewModel.WorksurfaceContextManager = mockWM.Object;
            _shellViewModel.WorksurfaceContextManager.EditMySqlResource(source.Object, view.Object);
            mockWM.Verify(manager => manager.EditMySqlResource(It.IsAny<IDbSource>(), view.Object));
            _shellViewModel.EditMySqlResource(source.Object);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("MainViewModel_EditPostgreSqlSource")]
        [DeploymentItem("Warewolf.Studio.Themes.Luna.dll")]
        [DeploymentItem("InfragisticsWPF4.DataPresenter.v15.1.dll")]
        public void MainViewModel_EditPostgreSqlSource_Handle_Result()
        {
            //------------Setup for test--------------------------
            CreateFullExportsAndVm();

            var env = SetupEnvironment();

            //------------Execute Test---------------------------
            _shellViewModel.ActiveServer = env.Object;
            //------------Assert Results-------------------------
            Assert.IsNotNull(_shellViewModel.ActiveServer);
            Assert.IsTrue(_shellViewModel.ActiveServer.IsConnected);
            Assert.IsTrue(_shellViewModel.ActiveServer.CanStudioExecute);

            var source = new Mock<IDbSource>();
            source.Setup(a => a.Name).Returns("TestDatabase");
            var view = new Mock<IView>();
            var mockWM = new Mock<IWorksurfaceContextManager>();
            mockWM.Setup(manager => manager.EditPostgreSqlResource(It.IsAny<IDbSource>(), view.Object)).Verifiable();
            _shellViewModel.WorksurfaceContextManager = mockWM.Object;
            _shellViewModel.WorksurfaceContextManager.EditPostgreSqlResource(source.Object, view.Object);
            mockWM.Verify(manager => manager.EditPostgreSqlResource(It.IsAny<IDbSource>(), view.Object));
            _shellViewModel.EditPostgreSqlResource(source.Object);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("MainViewModel_EditOracleSource")]
        [DeploymentItem("Warewolf.Studio.Themes.Luna.dll")]
        [DeploymentItem("InfragisticsWPF4.DataPresenter.v15.1.dll")]
        public void MainViewModel_EditOracleSource_Handle_Result()
        {
            //------------Setup for test--------------------------
            CreateFullExportsAndVm();

            var env = SetupEnvironment();

            //------------Execute Test---------------------------
            _shellViewModel.ActiveServer = env.Object;
            //------------Assert Results-------------------------
            Assert.IsNotNull(_shellViewModel.ActiveServer);
            Assert.IsTrue(_shellViewModel.ActiveServer.IsConnected);
            Assert.IsTrue(_shellViewModel.ActiveServer.CanStudioExecute);

            var source = new Mock<IDbSource>();
            source.Setup(a => a.Name).Returns("TestDatabase");
            var view = new Mock<IView>();
            var mockWM = new Mock<IWorksurfaceContextManager>();
            mockWM.Setup(manager => manager.EditOracleResource(It.IsAny<IDbSource>(), view.Object)).Verifiable();
            _shellViewModel.WorksurfaceContextManager = mockWM.Object;
            _shellViewModel.WorksurfaceContextManager.EditOracleResource(source.Object, view.Object);
            mockWM.Verify(manager => manager.EditOracleResource(It.IsAny<IDbSource>(), view.Object));
            _shellViewModel.EditOracleResource(source.Object);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("MainViewModel_EditOdbcSource")]
        [DeploymentItem("Warewolf.Studio.Themes.Luna.dll")]
        [DeploymentItem("InfragisticsWPF4.DataPresenter.v15.1.dll")]
        public void MainViewModel_EditOdbcSource_Handle_Result()
        {
            //------------Setup for test--------------------------
            CreateFullExportsAndVm();

            var env = SetupEnvironment();

            //------------Execute Test---------------------------
            _shellViewModel.ActiveServer = env.Object;
            //------------Assert Results-------------------------
            Assert.IsNotNull(_shellViewModel.ActiveServer);
            Assert.IsTrue(_shellViewModel.ActiveServer.IsConnected);
            Assert.IsTrue(_shellViewModel.ActiveServer.CanStudioExecute);

            var source = new Mock<IDbSource>();
            source.Setup(a => a.Name).Returns("TestDatabase");
            var view = new Mock<IView>();
            var mockWM = new Mock<IWorksurfaceContextManager>();
            mockWM.Setup(manager => manager.EditOdbcResource(It.IsAny<IDbSource>(), view.Object)).Verifiable();
            _shellViewModel.WorksurfaceContextManager = mockWM.Object;
            _shellViewModel.WorksurfaceContextManager.EditOdbcResource(source.Object, view.Object);
            mockWM.Verify(manager => manager.EditOdbcResource(It.IsAny<IDbSource>(), view.Object));
            _shellViewModel.EditOdbcResource(source.Object);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("MainViewModel_EditDropboxSource")]
        public void MainViewModel_EditDropboxSource_Handle_Result()
        {
            //------------Setup for test--------------------------
            CreateFullExportsAndVm();

            var env = SetupEnvironment();

            //------------Execute Test---------------------------
            _shellViewModel.ActiveServer = env.Object;
            //------------Assert Results-------------------------
            Assert.IsNotNull(_shellViewModel.ActiveServer);
            Assert.IsTrue(_shellViewModel.ActiveServer.IsConnected);
            Assert.IsTrue(_shellViewModel.ActiveServer.CanStudioExecute);

            var source = new Mock<IOAuthSource>();
            source.Setup(a => a.ResourceName).Returns("TestDropbox");
            var view = new Mock<IView>();
            var mockWM = new Mock<IWorksurfaceContextManager>();
            mockWM.Setup(manager => manager.EditResource(It.IsAny<IOAuthSource>(), view.Object)).Verifiable();
            _shellViewModel.WorksurfaceContextManager = mockWM.Object;
            _shellViewModel.WorksurfaceContextManager.EditResource(source.Object, view.Object);
            mockWM.Verify(manager => manager.EditResource(It.IsAny<IOAuthSource>(), view.Object));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("MainViewModel_EditEmailSource")]
        public void MainViewModel_EditEmailSource_Handle_Result()
        {
            //------------Setup for test--------------------------
            CreateFullExportsAndVm();

            var env = SetupEnvironment();

            //------------Execute Test---------------------------
            _shellViewModel.ActiveServer = env.Object;
            //------------Assert Results-------------------------
            Assert.IsNotNull(_shellViewModel.ActiveServer);
            Assert.IsTrue(_shellViewModel.ActiveServer.IsConnected);
            Assert.IsTrue(_shellViewModel.ActiveServer.CanStudioExecute);

            var source = new Mock<IEmailServiceSource>();
            source.Setup(a => a.ResourceName).Returns("TestEmail");
            var view = new Mock<IView>();
            var mockWM = new Mock<IWorksurfaceContextManager>();
            mockWM.Setup(manager => manager.EditResource(It.IsAny<IEmailServiceSource>(), view.Object)).Verifiable();
            _shellViewModel.WorksurfaceContextManager = mockWM.Object;
            _shellViewModel.WorksurfaceContextManager.EditResource(source.Object, view.Object);
            mockWM.Verify(manager => manager.EditResource(It.IsAny<IEmailServiceSource>(), view.Object));
            _shellViewModel.EditResource(source.Object);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("MainViewModel_EditExchangeSource")]
        public void MainViewModel_EditExchangeSource_Handle_Result()
        {
            //------------Setup for test--------------------------
            CreateFullExportsAndVm();

            var env = SetupEnvironment();

            //------------Execute Test---------------------------
            _shellViewModel.ActiveServer = env.Object;
            //------------Assert Results-------------------------
            Assert.IsNotNull(_shellViewModel.ActiveServer);
            Assert.IsTrue(_shellViewModel.ActiveServer.IsConnected);
            Assert.IsTrue(_shellViewModel.ActiveServer.CanStudioExecute);
            var view = new Mock<IView>();
            var source = new Mock<IExchangeSource>();
            source.Setup(a => a.ResourceName).Returns("TestExchange");

            var mockWM = new Mock<IWorksurfaceContextManager>();
            mockWM.Setup(manager => manager.EditResource(It.IsAny<IExchangeSource>(), view.Object)).Verifiable();
            _shellViewModel.WorksurfaceContextManager = mockWM.Object;
            _shellViewModel.WorksurfaceContextManager.EditResource(source.Object, view.Object);
            mockWM.Verify(manager => manager.EditResource(It.IsAny<IExchangeSource>(), view.Object));
            _shellViewModel.EditResource(source.Object);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("MainViewModel_EditPluginSource")]
        public void MainViewModel_EditPluginSource_Handle_Result()
        {
            //------------Setup for test--------------------------
            CreateFullExportsAndVm();

            var env = SetupEnvironment();

            //------------Execute Test---------------------------
            _shellViewModel.ActiveServer = env.Object;
            //------------Assert Results-------------------------
            Assert.IsNotNull(_shellViewModel.ActiveServer);
            Assert.IsTrue(_shellViewModel.ActiveServer.IsConnected);
            Assert.IsTrue(_shellViewModel.ActiveServer.CanStudioExecute);

            var file = new Mock<IFileListing>();
            file.Setup(a => a.FullName).Returns("File");

            var source = new Mock<IPluginSource>();
            source.Setup(a => a.Name).Returns("TestPlugin");
            source.Setup(a => a.SelectedDll).Returns(file.Object);
            var view = new Mock<IView>();

            var mockWM = new Mock<IWorksurfaceContextManager>();
            mockWM.Setup(manager => manager.EditResource(It.IsAny<IPluginSource>(), view.Object)).Verifiable();
            _shellViewModel.WorksurfaceContextManager = mockWM.Object;
            _shellViewModel.WorksurfaceContextManager.EditResource(source.Object, view.Object);
            mockWM.Verify(manager => manager.EditResource(It.IsAny<IPluginSource>(), view.Object));
            _shellViewModel.EditResource(source.Object);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("MainViewModel_EditComPluginSource")]
        public void MainViewModel_EditComPluginSource_Handle_Result()
        {
            //------------Setup for test--------------------------
            CreateFullExportsAndVm();

            var env = SetupEnvironment();

            //------------Execute Test---------------------------
            _shellViewModel.ActiveServer = env.Object;
            //------------Assert Results-------------------------
            Assert.IsNotNull(_shellViewModel.ActiveServer);
            Assert.IsTrue(_shellViewModel.ActiveServer.IsConnected);
            Assert.IsTrue(_shellViewModel.ActiveServer.CanStudioExecute);

            var file = new Mock<IFileListing>();
            file.Setup(a => a.FullName).Returns("File");

            var source = new Mock<IComPluginSource>();
            source.Setup(a => a.ResourceName).Returns("TestPlugin");
            source.Setup(a => a.SelectedDll).Returns(file.Object);

            var view = new Mock<IView>();
            var mockWM = new Mock<IWorksurfaceContextManager>();
            mockWM.Setup(manager => manager.EditResource(It.IsAny<IComPluginSource>(), view.Object)).Verifiable();
            _shellViewModel.WorksurfaceContextManager = mockWM.Object;
            _shellViewModel.WorksurfaceContextManager.EditResource(source.Object, view.Object);
            mockWM.Verify(manager => manager.EditResource(It.IsAny<IComPluginSource>(), view.Object));
            _shellViewModel.EditResource(source.Object);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("MainViewModel_EditRabbitMQSource")]
        public void MainViewModel_EditRabbitMQSource_Handle_Result()
        {
            //------------Setup for test--------------------------
            CreateFullExportsAndVm();

            var env = SetupEnvironment();

            //------------Execute Test---------------------------
            _shellViewModel.ActiveServer = env.Object;
            //------------Assert Results-------------------------
            Assert.IsNotNull(_shellViewModel.ActiveServer);
            Assert.IsTrue(_shellViewModel.ActiveServer.IsConnected);
            Assert.IsTrue(_shellViewModel.ActiveServer.CanStudioExecute);

            var source = new Mock<IRabbitMQServiceSourceDefinition>();
            source.Setup(a => a.ResourceName).Returns("TestRabbitMQ");
            var view = new Mock<IView>();
            var mockWM = new Mock<IWorksurfaceContextManager>();
            mockWM.Setup(manager => manager.EditResource(It.IsAny<IRabbitMQServiceSourceDefinition>(), view.Object)).Verifiable();
            _shellViewModel.WorksurfaceContextManager = mockWM.Object;
            _shellViewModel.WorksurfaceContextManager.EditResource(source.Object, view.Object);
            mockWM.Verify(manager => manager.EditResource(It.IsAny<IRabbitMQServiceSourceDefinition>(), view.Object));
            _shellViewModel.EditResource(source.Object);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("MainViewModel_EditServerSource")]
        public void MainViewModel_EditServerSource_Handle_Result()
        {
            //------------Setup for test--------------------------
            CreateFullExportsAndVm();

            var env = SetupEnvironment();

            //------------Execute Test---------------------------
            _shellViewModel.ActiveServer = env.Object;
            //------------Assert Results-------------------------
            Assert.IsNotNull(_shellViewModel.ActiveServer);
            Assert.IsTrue(_shellViewModel.ActiveServer.IsConnected);
            Assert.IsTrue(_shellViewModel.ActiveServer.CanStudioExecute);

            var source = new Mock<IServerSource>();
            source.Setup(a => a.Name).Returns("TestServer");
            source.Setup(a => a.Address).Returns("https://someServerName:3143");

            var mockWM = new Mock<IWorksurfaceContextManager>();
            mockWM.Setup(manager => manager.EditServer(It.IsAny<IServerSource>(), It.IsAny<IServer>(), It.IsAny<IView>())).Verifiable();
            _shellViewModel.WorksurfaceContextManager = mockWM.Object;
            _shellViewModel.WorksurfaceContextManager.EditServer(source.Object, It.IsAny<IServer>(), It.IsAny<IView>());
            mockWM.Verify(manager => manager.EditServer(It.IsAny<IServerSource>(), It.IsAny<IServer>(), It.IsAny<IView>()));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("MainViewModel_EditSharepointSource")]
        public void MainViewModel_EditSharepointSource_Handle_Result()
        {
            //------------Setup for test--------------------------
            CreateFullExportsAndVm();

            var env = SetupEnvironment();

            //------------Execute Test---------------------------
            _shellViewModel.ActiveServer = env.Object;
            //------------Assert Results-------------------------
            Assert.IsNotNull(_shellViewModel.ActiveServer);
            Assert.IsTrue(_shellViewModel.ActiveServer.IsConnected);
            Assert.IsTrue(_shellViewModel.ActiveServer.CanStudioExecute);

            var source = new Mock<ISharepointServerSource>();
            source.Setup(a => a.Name).Returns("TestSharepoint");
            var view = new Mock<IView>();
            var mockWM = new Mock<IWorksurfaceContextManager>();
            mockWM.Setup(manager => manager.EditResource(It.IsAny<ISharepointServerSource>(), view.Object)).Verifiable();
            _shellViewModel.WorksurfaceContextManager = mockWM.Object;
            _shellViewModel.WorksurfaceContextManager.EditResource(source.Object, view.Object);
            mockWM.Verify(manager => manager.EditResource(It.IsAny<ISharepointServerSource>(), view.Object));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("MainViewModel_EditWcfSource")]
        public void MainViewModel_EditWcfSource_Handle_Result()
        {
            //------------Setup for test--------------------------
            CreateFullExportsAndVm();

            var env = SetupEnvironment();

            //------------Execute Test---------------------------
            _shellViewModel.ActiveServer = env.Object;
            //------------Assert Results-------------------------
            Assert.IsNotNull(_shellViewModel.ActiveServer);
            Assert.IsTrue(_shellViewModel.ActiveServer.IsConnected);
            Assert.IsTrue(_shellViewModel.ActiveServer.CanStudioExecute);

            var source = new Mock<IWcfServerSource>();
            source.Setup(a => a.Name).Returns("TestWcf");
            var view = new Mock<IView>();
            var mockWM = new Mock<IWorksurfaceContextManager>();
            mockWM.Setup(manager => manager.EditResource(It.IsAny<IWcfServerSource>(), view.Object)).Verifiable();
            _shellViewModel.WorksurfaceContextManager = mockWM.Object;
            _shellViewModel.WorksurfaceContextManager.EditResource(source.Object, view.Object);
            mockWM.Verify(manager => manager.EditResource(It.IsAny<IWcfServerSource>(), view.Object));
            _shellViewModel.EditResource(source.Object);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("MainViewModel_EditWebSource")]
        public void MainViewModel_EditWebSource_Handle_Result()
        {
            //------------Setup for test--------------------------
            CreateFullExportsAndVm();

            var env = SetupEnvironment();

            //------------Execute Test---------------------------
            _shellViewModel.ActiveServer = env.Object;
            //------------Assert Results-------------------------
            Assert.IsNotNull(_shellViewModel.ActiveServer);
            Assert.IsTrue(_shellViewModel.ActiveServer.IsConnected);
            Assert.IsTrue(_shellViewModel.ActiveServer.CanStudioExecute);

            var source = new Mock<IWebServiceSource>();
            source.Setup(a => a.Name).Returns("TestWeb");
            var view = new Mock<IView>();
            var mockWM = new Mock<IWorksurfaceContextManager>();
            mockWM.Setup(manager => manager.EditResource(It.IsAny<IWebServiceSource>(), view.Object)).Verifiable();
            _shellViewModel.WorksurfaceContextManager = mockWM.Object;
            _shellViewModel.WorksurfaceContextManager.EditResource(source.Object, view.Object);
            mockWM.Verify(manager => manager.EditResource(It.IsAny<IWebServiceSource>(), view.Object));
            _shellViewModel.EditResource(source.Object);
        }

        #region CommandTests

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("MainViewModel_NewSqlServerSource")]
        public void MainViewModel_NewSqlServerSource_Handle_Result()
        {
            //------------Setup for test--------------------------
            var toolboxViewModel = new Mock<IToolboxViewModel>().Object;
            CustomContainer.Register(toolboxViewModel);
            CreateFullExportsAndVm();

            var env = SetupEnvironment();

            //------------Execute Test---------------------------
            _shellViewModel.ActiveServer = env.Object;
            //------------Assert Results-------------------------
            Assert.IsNotNull(_shellViewModel.ActiveServer);
            Assert.IsTrue(_shellViewModel.ActiveServer.IsConnected);
            Assert.IsTrue(_shellViewModel.ActiveServer.CanStudioExecute);

            var canExecute = _shellViewModel.NewSqlServerSourceCommand.CanExecute(null);
            Assert.IsTrue(canExecute);


            var mockWM = new Mock<IWorksurfaceContextManager>();
            mockWM.Setup(manager => manager.NewSqlServerSource(It.IsAny<string>())).Verifiable();
            _shellViewModel.WorksurfaceContextManager = mockWM.Object;
            _shellViewModel.NewSqlServerSourceCommand.Execute(null);
            mockWM.Verify(manager => manager.NewSqlServerSource(It.IsAny<string>()));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("MainViewModel_NewMySqlSource")]
        public void MainViewModel_NewMySqlSource_Handle_Result()
        {
            //------------Setup for test--------------------------
            var toolboxViewModel = new Mock<IToolboxViewModel>().Object;
            CustomContainer.Register(toolboxViewModel);
            CreateFullExportsAndVm();

            var env = SetupEnvironment();

            //------------Execute Test---------------------------
            _shellViewModel.ActiveServer = env.Object;
            //------------Assert Results-------------------------
            Assert.IsNotNull(_shellViewModel.ActiveServer);
            Assert.IsTrue(_shellViewModel.ActiveServer.IsConnected);
            Assert.IsTrue(_shellViewModel.ActiveServer.CanStudioExecute);

            var canExecute = _shellViewModel.NewMySqlSourceCommand.CanExecute(null);
            Assert.IsTrue(canExecute);


            var mockWM = new Mock<IWorksurfaceContextManager>();
            mockWM.Setup(manager => manager.NewMySqlSource(It.IsAny<string>())).Verifiable();
            _shellViewModel.WorksurfaceContextManager = mockWM.Object;
            _shellViewModel.NewMySqlSourceCommand.Execute(null);
            mockWM.Verify(manager => manager.NewMySqlSource(It.IsAny<string>()));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("MainViewModel_NewPostgreSqlSource")]
        public void MainViewModel_NewPostgreSqlSource_Handle_Result()
        {
            //------------Setup for test--------------------------
            var toolboxViewModel = new Mock<IToolboxViewModel>().Object;
            CustomContainer.Register(toolboxViewModel);
            CreateFullExportsAndVm();

            var env = SetupEnvironment();

            //------------Execute Test---------------------------
            _shellViewModel.ActiveServer = env.Object;
            //------------Assert Results-------------------------
            Assert.IsNotNull(_shellViewModel.ActiveServer);
            Assert.IsTrue(_shellViewModel.ActiveServer.IsConnected);
            Assert.IsTrue(_shellViewModel.ActiveServer.CanStudioExecute);

            var canExecute = _shellViewModel.NewPostgreSqlSourceCommand.CanExecute(null);
            Assert.IsTrue(canExecute);


            var mockWM = new Mock<IWorksurfaceContextManager>();
            mockWM.Setup(manager => manager.NewPostgreSqlSource(It.IsAny<string>())).Verifiable();
            _shellViewModel.WorksurfaceContextManager = mockWM.Object;
            _shellViewModel.NewPostgreSqlSourceCommand.Execute(null);
            mockWM.Verify(manager => manager.NewPostgreSqlSource(It.IsAny<string>()));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("MainViewModel_NewOracleSource")]
        public void MainViewModel_NewOracleSource_Handle_Result()
        {
            //------------Setup for test--------------------------
            var toolboxViewModel = new Mock<IToolboxViewModel>().Object;
            CustomContainer.Register(toolboxViewModel);
            CreateFullExportsAndVm();

            var env = SetupEnvironment();

            //------------Execute Test---------------------------
            _shellViewModel.ActiveServer = env.Object;
            //------------Assert Results-------------------------
            Assert.IsNotNull(_shellViewModel.ActiveServer);
            Assert.IsTrue(_shellViewModel.ActiveServer.IsConnected);
            Assert.IsTrue(_shellViewModel.ActiveServer.CanStudioExecute);

            var canExecute = _shellViewModel.NewOracleSourceCommand.CanExecute(null);
            Assert.IsTrue(canExecute);


            var mockWM = new Mock<IWorksurfaceContextManager>();
            mockWM.Setup(manager => manager.NewOracleSource(It.IsAny<string>())).Verifiable();
            _shellViewModel.WorksurfaceContextManager = mockWM.Object;
            _shellViewModel.NewOracleSourceCommand.Execute(null);
            mockWM.Verify(manager => manager.NewOracleSource(It.IsAny<string>()));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("MainViewModel_NewOdbcSource")]
        public void MainViewModel_NewOdbcSource_Handle_Result()
        {
            //------------Setup for test--------------------------
            var toolboxViewModel = new Mock<IToolboxViewModel>().Object;
            CustomContainer.Register(toolboxViewModel);
            CreateFullExportsAndVm();

            var env = SetupEnvironment();

            //------------Execute Test---------------------------
            _shellViewModel.ActiveServer = env.Object;
            //------------Assert Results-------------------------
            Assert.IsNotNull(_shellViewModel.ActiveServer);
            Assert.IsTrue(_shellViewModel.ActiveServer.IsConnected);
            Assert.IsTrue(_shellViewModel.ActiveServer.CanStudioExecute);

            var canExecute = _shellViewModel.NewOdbcSourceCommand.CanExecute(null);
            Assert.IsTrue(canExecute);


            var mockWM = new Mock<IWorksurfaceContextManager>();
            mockWM.Setup(manager => manager.NewOdbcSource(It.IsAny<string>())).Verifiable();
            _shellViewModel.WorksurfaceContextManager = mockWM.Object;
            _shellViewModel.NewOdbcSourceCommand.Execute(null);
            mockWM.Verify(manager => manager.NewOdbcSource(It.IsAny<string>()));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("MainViewModel_NewDropboxSourceCommand")]
        public void MainViewModel_NewDropboxSourceCommand_Handle_Result()
        {
            //------------Setup for test--------------------------
            var toolboxViewModel = new Mock<IToolboxViewModel>().Object;
            CustomContainer.Register(toolboxViewModel);
            CreateFullExportsAndVm();

            var env = SetupEnvironment();

            //------------Execute Test---------------------------
            _shellViewModel.ActiveServer = env.Object;
            //------------Assert Results-------------------------
            Assert.IsNotNull(_shellViewModel.ActiveServer);
            Assert.IsTrue(_shellViewModel.ActiveServer.IsConnected);
            Assert.IsTrue(_shellViewModel.ActiveServer.CanStudioExecute);

            var canExecute = _shellViewModel.NewDropboxSourceCommand.CanExecute(null);
            Assert.IsTrue(canExecute);


            var mockWM = new Mock<IWorksurfaceContextManager>();
            mockWM.Setup(manager => manager.NewDropboxSource(It.IsAny<string>())).Verifiable();
            _shellViewModel.WorksurfaceContextManager = mockWM.Object;
            _shellViewModel.NewDropboxSourceCommand.Execute(null);
            mockWM.Verify(manager => manager.NewDropboxSource(It.IsAny<string>()));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("MainViewModel_NewEmailSourceCommand")]
        public void MainViewModel_NewEmailSourceCommand_Handle_Result()
        {
            //------------Setup for test--------------------------
            var toolboxViewModel = new Mock<IToolboxViewModel>().Object;
            CustomContainer.Register(toolboxViewModel);
            CreateFullExportsAndVm();

            var env = SetupEnvironment();

            //------------Execute Test---------------------------
            _shellViewModel.ActiveServer = env.Object;
            //------------Assert Results-------------------------
            Assert.IsNotNull(_shellViewModel.ActiveServer);
            Assert.IsTrue(_shellViewModel.ActiveServer.IsConnected);
            Assert.IsTrue(_shellViewModel.ActiveServer.CanStudioExecute);

            var canExecute = _shellViewModel.NewEmailSourceCommand.CanExecute(null);
            Assert.IsTrue(canExecute);


            var mockWM = new Mock<IWorksurfaceContextManager>();
            mockWM.Setup(manager => manager.NewEmailSource(It.IsAny<string>())).Verifiable();
            _shellViewModel.WorksurfaceContextManager = mockWM.Object;
            _shellViewModel.NewEmailSourceCommand.Execute(null);
            mockWM.Verify(manager => manager.NewEmailSource(It.IsAny<string>()));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("MainViewModel_NewExchangeSourceCommand")]
        public void MainViewModel_NewExchangeSourceCommand_Handle_Result()
        {
            //------------Setup for test--------------------------
            var toolboxViewModel = new Mock<IToolboxViewModel>().Object;
            CustomContainer.Register(toolboxViewModel);
            CreateFullExportsAndVm();

            var env = SetupEnvironment();

            //------------Execute Test---------------------------
            _shellViewModel.ActiveServer = env.Object;
            //------------Assert Results-------------------------
            Assert.IsNotNull(_shellViewModel.ActiveServer);
            Assert.IsTrue(_shellViewModel.ActiveServer.IsConnected);
            Assert.IsTrue(_shellViewModel.ActiveServer.CanStudioExecute);

            var canExecute = _shellViewModel.NewExchangeSourceCommand.CanExecute(null);
            Assert.IsTrue(canExecute);



            var mockWM = new Mock<IWorksurfaceContextManager>();
            mockWM.Setup(manager => manager.NewExchangeSource(It.IsAny<string>())).Verifiable();
            _shellViewModel.WorksurfaceContextManager = mockWM.Object;
            _shellViewModel.NewExchangeSourceCommand.Execute(null);
            mockWM.Verify(manager => manager.NewExchangeSource(It.IsAny<string>()));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("MainViewModel_NewPluginSourceCommand")]
        public void MainViewModel_NewPluginSourceCommand_Handle_Result()
        {
            //------------Setup for test--------------------------
            var toolboxViewModel = new Mock<IToolboxViewModel>().Object;
            CustomContainer.Register(toolboxViewModel);
            CreateFullExportsAndVm();

            var env = SetupEnvironment();

            //------------Execute Test---------------------------
            _shellViewModel.ActiveServer = env.Object;
            //------------Assert Results-------------------------
            Assert.IsNotNull(_shellViewModel.ActiveServer);
            Assert.IsTrue(_shellViewModel.ActiveServer.IsConnected);
            Assert.IsTrue(_shellViewModel.ActiveServer.CanStudioExecute);

            var canExecute = _shellViewModel.NewPluginSourceCommand.CanExecute(null);
            Assert.IsTrue(canExecute);


            var mockWM = new Mock<IWorksurfaceContextManager>();
            mockWM.Setup(manager => manager.NewPluginSource(It.IsAny<string>())).Verifiable();
            _shellViewModel.WorksurfaceContextManager = mockWM.Object;
            _shellViewModel.NewPluginSourceCommand.Execute(null);
            mockWM.Verify(manager => manager.NewPluginSource(It.IsAny<string>()));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("MainViewModel_NewRabbitMQSourceCommand")]
        public void MainViewModel_NewRabbitMQSourceCommand_Handle_Result()
        {
            //------------Setup for test--------------------------
            var toolboxViewModel = new Mock<IToolboxViewModel>().Object;
            CustomContainer.Register(toolboxViewModel);
            CreateFullExportsAndVm();

            var env = SetupEnvironment();

            //------------Execute Test---------------------------
            _shellViewModel.ActiveServer = env.Object;
            //------------Assert Results-------------------------
            Assert.IsNotNull(_shellViewModel.ActiveServer);
            Assert.IsTrue(_shellViewModel.ActiveServer.IsConnected);
            Assert.IsTrue(_shellViewModel.ActiveServer.CanStudioExecute);

            var canExecute = _shellViewModel.NewRabbitMQSourceCommand.CanExecute(null);
            Assert.IsTrue(canExecute);



            var mockWM = new Mock<IWorksurfaceContextManager>();
            mockWM.Setup(manager => manager.NewRabbitMQSource(It.IsAny<string>())).Verifiable();
            _shellViewModel.WorksurfaceContextManager = mockWM.Object;
            _shellViewModel.NewRabbitMQSourceCommand.Execute(null);
            mockWM.Verify(manager => manager.NewRabbitMQSource(It.IsAny<string>()));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("MainViewModel_NewSharepointSourceCommand")]
        public void MainViewModel_NewSharepointSourceCommand_Handle_Result()
        {
            //------------Setup for test--------------------------
            var toolboxViewModel = new Mock<IToolboxViewModel>().Object;
            CustomContainer.Register(toolboxViewModel);
            CreateFullExportsAndVm();

            var env = SetupEnvironment();

            //------------Execute Test---------------------------
            _shellViewModel.ActiveServer = env.Object;
            //------------Assert Results-------------------------
            Assert.IsNotNull(_shellViewModel.ActiveServer);
            Assert.IsTrue(_shellViewModel.ActiveServer.IsConnected);
            Assert.IsTrue(_shellViewModel.ActiveServer.CanStudioExecute);

            var canExecute = _shellViewModel.NewSharepointSourceCommand.CanExecute(null);
            Assert.IsTrue(canExecute);


            var mockWM = new Mock<IWorksurfaceContextManager>();
            mockWM.Setup(manager => manager.NewSharepointSource(It.IsAny<string>())).Verifiable();
            _shellViewModel.WorksurfaceContextManager = mockWM.Object;
            _shellViewModel.NewSharepointSourceCommand.Execute(null);
            mockWM.Verify(manager => manager.NewSharepointSource(It.IsAny<string>()));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("MainViewModel_NewWcfSourceCommand")]
        public void MainViewModel_NewWcfSourceCommand_Handle_Result()
        {
            //------------Setup for test--------------------------
            var toolboxViewModel = new Mock<IToolboxViewModel>().Object;
            CustomContainer.Register(toolboxViewModel);
            CreateFullExportsAndVm();

            var env = SetupEnvironment();

            //------------Execute Test---------------------------
            _shellViewModel.ActiveServer = env.Object;
            //------------Assert Results-------------------------
            Assert.IsNotNull(_shellViewModel.ActiveServer);
            Assert.IsTrue(_shellViewModel.ActiveServer.IsConnected);
            Assert.IsTrue(_shellViewModel.ActiveServer.CanStudioExecute);

            var canExecute = _shellViewModel.NewWcfSourceCommand.CanExecute(null);
            Assert.IsTrue(canExecute);


            var mockWM = new Mock<IWorksurfaceContextManager>();
            mockWM.Setup(manager => manager.NewWcfSource(It.IsAny<string>())).Verifiable();
            _shellViewModel.WorksurfaceContextManager = mockWM.Object;
            _shellViewModel.NewWcfSourceCommand.Execute(null);
            mockWM.Verify(manager => manager.NewWcfSource(It.IsAny<string>()));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("MainViewModel_NewWebSourceCommand")]
        public void MainViewModel_NewWebSourceCommand_Handle_Result()
        {
            //------------Setup for test--------------------------
            var toolboxViewModel = new Mock<IToolboxViewModel>().Object;
            CustomContainer.Register(toolboxViewModel);
            CreateFullExportsAndVm();

            var env = SetupEnvironment();

            //------------Execute Test---------------------------
            _shellViewModel.ActiveServer = env.Object;
            //------------Assert Results-------------------------
            Assert.IsNotNull(_shellViewModel.ActiveServer);
            Assert.IsTrue(_shellViewModel.ActiveServer.IsConnected);
            Assert.IsTrue(_shellViewModel.ActiveServer.CanStudioExecute);

            var canExecute = _shellViewModel.NewWebSourceCommand.CanExecute(null);
            Assert.IsTrue(canExecute);

            var mockWM = new Mock<IWorksurfaceContextManager>();
            mockWM.Setup(manager => manager.NewWebSource(It.IsAny<string>())).Verifiable();
            _shellViewModel.WorksurfaceContextManager = mockWM.Object;
            _shellViewModel.NewWebSourceCommand.Execute(null);
            mockWM.Verify(manager => manager.NewWebSource(It.IsAny<string>()));
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("MainViewModel_NewElasticsearchSourceCommand")]
        public void MainViewModel_NewElasticsearchSourceCommand_Handle_Result()
        {
            //------------Setup for test--------------------------
            var toolboxViewModel = new Mock<IToolboxViewModel>().Object;
            CustomContainer.Register(toolboxViewModel);
            CreateFullExportsAndVm();

            var env = SetupEnvironment();

            //------------Execute Test---------------------------
            _shellViewModel.ActiveServer = env.Object;
            //------------Assert Results-------------------------
            Assert.IsNotNull(_shellViewModel.ActiveServer);
            Assert.IsTrue(_shellViewModel.ActiveServer.IsConnected);
            Assert.IsTrue(_shellViewModel.ActiveServer.CanStudioExecute);

            var canExecute = _shellViewModel.NewElasticsearchSourceCommand.CanExecute(null);
            Assert.IsTrue(canExecute);

            var mockWM = new Mock<IWorksurfaceContextManager>();
            mockWM.Setup(manager => manager.NewElasticsearchSource(It.IsAny<string>())).Verifiable();
            _shellViewModel.WorksurfaceContextManager = mockWM.Object;
            _shellViewModel.NewElasticsearchSourceCommand.Execute(null);
            mockWM.Verify(manager => manager.NewElasticsearchSource(It.IsAny<string>()));
        }
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("MainViewModel_NewRedisSourceCommand")]
        public void MainViewModel_NewRedisSourceCommand_Handle_Result()
        {
            //------------Setup for test--------------------------
            var toolboxViewModel = new Mock<IToolboxViewModel>().Object;
            CustomContainer.Register(toolboxViewModel);
            CreateFullExportsAndVm();

            var env = SetupEnvironment();

            //------------Execute Test---------------------------
            _shellViewModel.ActiveServer = env.Object;
            //------------Assert Results-------------------------
            Assert.IsNotNull(_shellViewModel.ActiveServer);
            Assert.IsTrue(_shellViewModel.ActiveServer.IsConnected);
            Assert.IsTrue(_shellViewModel.ActiveServer.CanStudioExecute);

            var canExecute = _shellViewModel.NewRedisSourceCommand.CanExecute(null);
            Assert.IsTrue(canExecute);

            var mockWM = new Mock<IWorksurfaceContextManager>();
            mockWM.Setup(manager => manager.NewRedisSource(It.IsAny<string>())).Verifiable();
            _shellViewModel.WorksurfaceContextManager = mockWM.Object;
            _shellViewModel.NewRedisSourceCommand.Execute(null);
            mockWM.Verify(manager => manager.NewRedisSource(It.IsAny<string>()));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("MainViewModel_TasksCommand")]
        public void MainViewModel_TasksCommand_Handle_Result()
        {
            //------------Setup for test--------------------------
            var toolboxViewModel = new Mock<IToolboxViewModel>().Object;
            CustomContainer.Register(toolboxViewModel);
            CreateFullExportsAndVm();

            var env = SetupEnvironment();

            //------------Execute Test---------------------------
            _shellViewModel.ActiveServer = env.Object;
            //------------Assert Results-------------------------
            Assert.IsNotNull(_shellViewModel.ActiveServer);
            Assert.IsTrue(_shellViewModel.ActiveServer.IsConnected);
            Assert.IsTrue(_shellViewModel.ActiveServer.CanStudioExecute);

            var canExecute = _shellViewModel.TasksCommand.CanExecute(null);
            Assert.IsTrue(canExecute);

            var mockWM = new Mock<IWorksurfaceContextManager>();
            mockWM.Setup(manager => manager.AddTriggersWorkSurface()).Verifiable();
            _shellViewModel.WorksurfaceContextManager = mockWM.Object;
            _shellViewModel.TasksCommand.Execute(null);
            mockWM.Verify(manager => manager.AddTriggersWorkSurface());
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("MainViewModel_QueueEventsCommand")]
        public void MainViewModel_QueueEventsCommand_Handle_Result()
        {
            //------------Setup for test--------------------------
            var toolboxViewModel = new Mock<IToolboxViewModel>().Object;
            CustomContainer.Register(toolboxViewModel);
            CreateFullExportsAndVm();

            var env = SetupEnvironment();

            //------------Execute Test---------------------------
            _shellViewModel.ActiveServer = env.Object;
            //------------Assert Results-------------------------
            Assert.IsNotNull(_shellViewModel.ActiveServer);
            Assert.IsTrue(_shellViewModel.ActiveServer.IsConnected);
            Assert.IsTrue(_shellViewModel.ActiveServer.CanStudioExecute);

            var canExecute = _shellViewModel.QueueEventsCommand.CanExecute(null);
            Assert.IsTrue(canExecute);

            var mockWM = new Mock<IWorksurfaceContextManager>();
            mockWM.Setup(manager => manager.AddQueuesWorkSurface()).Verifiable();
            _shellViewModel.WorksurfaceContextManager = mockWM.Object;
            _shellViewModel.QueueEventsCommand.Execute(null);
            mockWM.Verify(manager => manager.AddQueuesWorkSurface());
        }


        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("MainViewModel_SchedulerCommand")]
        public void MainViewModel_SchedulerCommand_Handle_Result()
        {
            //------------Setup for test--------------------------
            var toolboxViewModel = new Mock<IToolboxViewModel>().Object;
            CustomContainer.Register(toolboxViewModel);
            CreateFullExportsAndVm();

            var env = SetupEnvironment();

            //------------Execute Test---------------------------
            _shellViewModel.ActiveServer = env.Object;
            //------------Assert Results-------------------------
            Assert.IsNotNull(_shellViewModel.ActiveServer);
            Assert.IsTrue(_shellViewModel.ActiveServer.IsConnected);
            Assert.IsTrue(_shellViewModel.ActiveServer.CanStudioExecute);

            var canExecute = _shellViewModel.SchedulerCommand.CanExecute(null);
            Assert.IsTrue(canExecute);

            var mockWM = new Mock<IWorksurfaceContextManager>();
            mockWM.Setup(manager => manager.AddSchedulerWorkSurface()).Verifiable();
            _shellViewModel.WorksurfaceContextManager = mockWM.Object;
            _shellViewModel.SchedulerCommand.Execute(null);
            mockWM.Verify(manager => manager.AddSchedulerWorkSurface());
        }

        static Mock<IServer> SetupEnvironment()
        {
            var newSelectedConnection = new Mock<IServer>();
            var newSelectedConnectionEnvironmentId = Guid.NewGuid();
            newSelectedConnection.SetupGet(it => it.DisplayName).Returns("Nonlocalhost");
            newSelectedConnection.SetupGet(it => it.EnvironmentID).Returns(newSelectedConnectionEnvironmentId);
            newSelectedConnection.SetupGet(it => it.HasLoaded).Returns(true);
            newSelectedConnection.SetupGet(it => it.IsConnected).Returns(true);
            newSelectedConnection.SetupGet(it => it.UpdateRepository).Returns(new Mock<IStudioUpdateManager>().Object);
            newSelectedConnection.SetupGet(it => it.QueryProxy).Returns(new Mock<IQueryManager>().Object);

            var env = new Mock<IServer>();
            env.Setup(a => a.IsLocalHost).Returns(true);
            env.Setup(a => a.IsConnected).Returns(true);
            env.Setup(a => a.CanStudioExecute).Returns(true);
            env.Setup(a => a.Name).Returns("TestEnvironment");
            env.Setup(a => a.DisplayName).Returns("TestEnvironment");
            return env;
        }

        #endregion

        #region OnStudioClosing
        [TestMethod]
        [TestCategory("MainViewModel_OnStudioClosing")]
        [Owner("Pieter Terblanche")]
        public void MainViewModel_OnStudioClosing_CallsTasksOnClosing()
        {
            var eventPublisher = new Mock<IEventAggregator>();
            var environmentRepository = new Mock<IServerRepository>();
            CustomContainer.Register(environmentRepository.Object);
            var environmentModel = new Mock<IServer>();

            var mockEnvironmentConnection = SetupMockConnection();
            environmentModel.SetupGet(it => it.Connection).Returns(mockEnvironmentConnection.Object);

            environmentRepository.Setup(repo => repo.Source).Returns(environmentModel.Object);

            var viewModel = new Mock<IShellViewModel>();
            var server = (IServer)CustomContainer.Get(typeof(IServer));
            viewModel.SetupGet(model => model.ActiveServer).Returns(server);

            CustomContainer.Register(viewModel.Object);

            var versionChecker = new Mock<IVersionChecker>();
            var asyncWorker = new Mock<IAsyncWorker>();
            asyncWorker.Setup(w => w.Start(It.IsAny<System.Action>(), It.IsAny<System.Action>())).Verifiable();
            var connected1 = new Mock<IServer>();
            var connected2 = new Mock<IServer>();
            var notConnected = new Mock<IServer>();
            connected1.Setup(a => a.IsConnected).Returns(true).Verifiable();
            connected1.Setup(a => a.Disconnect()).Verifiable();
            connected2.Setup(a => a.IsConnected).Returns(true).Verifiable();
            connected2.Setup(a => a.Disconnect()).Verifiable();
            IList<IServer> lst = new List<IServer> { connected1.Object, connected2.Object, notConnected.Object };
            environmentRepository.Setup(repo => repo.All()).Returns(lst);
            environmentRepository.Setup(repo => repo.Get(It.IsAny<Guid>())).Returns(connected1.Object);
            var vieFactory = new Mock<IViewFactory>();
            var viewMock = new Mock<IView>();
            vieFactory.Setup(factory => factory.GetViewGivenServerResourceType(It.IsAny<string>())).Returns(viewMock.Object);

            var popup = new Mock<IPopupController>();
            popup.Setup(a => a.Show(StringResources.Unsaved_Changes, StringResources.CloseHeader,
                               MessageBoxButton.YesNoCancel, MessageBoxImage.Information, @"", false, false, true, false, false, false)).Returns(MessageBoxResult.Cancel).Verifiable();

            var mvm = new ShellViewModel(eventPublisher.Object, asyncWorker.Object, environmentRepository.Object, versionChecker.Object, vieFactory.Object, false, null, popup.Object);

            var tasks = new TriggersViewModel(EventPublishers.Aggregator, popup.Object, new SynchronousAsyncWorker(), new Mock<IServer>().Object, a => environmentModel.Object) { WorkSurfaceContext = WorkSurfaceContext.Triggers };
            tasks.IsDirty = true;     
            var vm = new WorkSurfaceContextViewModel(new EventAggregator(), new WorkSurfaceKey(), tasks, new Mock<IPopupController>().Object, (a, b, c) => { });

            mvm.Items.Add(vm);
            Assert.IsFalse(mvm.OnStudioClosing());
        }

        [TestMethod]
        [TestCategory("MainViewModel_OnStudioClosing")]
        [Owner("Leon Rajindrapersadh")]
        public void MainViewModel_OnStudioClosing_ClosesRemoteEnvironmants()
        {
            var viewModel = new Mock<IShellViewModel>();
            var server = (IServer)CustomContainer.Get(typeof(IServer));
            var mockEnvironmentConnection = SetupMockConnection();
            server.Connection = mockEnvironmentConnection.Object;
            viewModel.SetupGet(model => model.ActiveServer).Returns(server);

            CustomContainer.Register(viewModel.Object);

            var eventPublisher = new Mock<IEventAggregator>();
            var environmentRepository = new Mock<IServerRepository>();
            var serverMock = new Mock<IServer>();
            serverMock.Setup(it => it.Connection).Returns(mockEnvironmentConnection.Object);
            environmentRepository.Setup(repo => repo.Source).Returns(serverMock.Object);
            CustomContainer.Register(environmentRepository.Object);
            var connected1 = new Mock<IServer>();
            var connected2 = new Mock<IServer>();
            var notConnected = new Mock<IServer>();
            notConnected.Setup(it => it.Connection).Returns(mockEnvironmentConnection.Object);
            connected1.Setup(a => a.IsConnected).Returns(true).Verifiable();
            connected1.Setup(a => a.Disconnect()).Verifiable();
            connected1.Setup(it => it.Connection).Returns(mockEnvironmentConnection.Object);
            connected2.Setup(a => a.IsConnected).Returns(true).Verifiable();
            connected2.Setup(a => a.Disconnect()).Verifiable();
            connected2.Setup(it => it.Connection).Returns(mockEnvironmentConnection.Object);
            notConnected.Setup(a => a.IsConnected).Returns(false).Verifiable();
            IList<IServer> lst = new List<IServer> { connected1.Object, connected2.Object, notConnected.Object };

            environmentRepository.Setup(repo => repo.All()).Returns(lst);
            environmentRepository.Setup(a => a.Get(It.IsAny<Guid>())).Returns(connected1.Object);
            var versionChecker = new Mock<IVersionChecker>();
            var asyncWorker = new Mock<IAsyncWorker>();
            asyncWorker.Setup(w => w.Start(It.IsAny<System.Action>(), It.IsAny<System.Action>())).Verifiable();
            var vieFactory = new Mock<IViewFactory>();
            var viewMock = new Mock<IView>();
            vieFactory.Setup(factory => factory.GetViewGivenServerResourceType(It.IsAny<string>())).Returns(viewMock.Object);

            var popup = new Mock<Common.Interfaces.Studio.Controller.IPopupController>();
            popup.Setup(a => a.Show(StringResources.Unsaved_Changes, StringResources.CloseHeader,
                               MessageBoxButton.YesNoCancel, MessageBoxImage.Information, @"", false, false, true, false, false, false)).Returns(MessageBoxResult.Yes).Verifiable();
            var mvm = new ShellViewModel(eventPublisher.Object, asyncWorker.Object, environmentRepository.Object, versionChecker.Object, vieFactory.Object, false, null, popup.Object);

            var scheduler = new SchedulerViewModel(EventPublishers.Aggregator, new DirectoryObjectPickerDialog(), popup.Object, new SynchronousAsyncWorker(), new Mock<IServer>().Object, a => new Mock<IServer>().Object) { WorkSurfaceContext = WorkSurfaceContext.Scheduler };
            var task = new Mock<IScheduledResource>();
            task.Setup(a => a.IsDirty).Returns(false);
            scheduler.SelectedTask = task.Object;
            var vm = new WorkSurfaceContextViewModel(new Mock<IEventAggregator>().Object, new WorkSurfaceKey(), scheduler, new Mock<Common.Interfaces.Studio.Controller.IPopupController>().Object, (a, b, c) => { });

            mvm.Items.Add(vm);
            Assert.IsTrue(mvm.OnStudioClosing());   // assert that the studio closes
        }

        [TestMethod]
        [TestCategory("MainViewModel_OnStudioClosing")]
        [Owner("Leon Rajindrapersadh")]
        public void MainViewModel_OnStudioClosing_CallsSettingsOnClosing()
        {
            var viewModel = new Mock<IShellViewModel>();
            var server = new Mock<IServer>();
            server.SetupGet(server1 => server1.IsConnected).Returns(true);
            viewModel.SetupGet(model => model.ActiveServer).Returns(server.Object);
            viewModel.SetupGet(model => model.ActiveServer).Returns(server.Object);
            viewModel.SetupGet(model => model.LocalhostServer).Returns(server.Object);
            CustomContainer.Register(viewModel.Object);

            var eventPublisher = new Mock<IEventAggregator>();
            var environmentRepository = new Mock<IServerRepository>();
            CustomContainer.Register(environmentRepository.Object);
            var environmentModel = new Mock<IServer>();
            var environmentConnection = new Mock<IEnvironmentConnection>().Object;
            environmentModel.SetupGet(a => a.Connection).Returns(environmentConnection);
            environmentModel.SetupGet(a => a.IsLocalHost).Returns(true);
            var mockAuthService = new Mock<IAuthorizationService>();
            var mockSecurityService = new Mock<ISecurityService>();
            mockSecurityService.Setup(a => a.Permissions).Returns(new List<WindowsGroupPermission>(new[] { WindowsGroupPermission.CreateEveryone(), }));
            mockAuthService.SetupGet(service => service.SecurityService).Returns(mockSecurityService.Object);
            environmentModel.Setup(c => c.AuthorizationService).Returns(mockAuthService.Object);
            environmentRepository.Setup(repo => repo.Source).Returns(environmentModel.Object);
            environmentModel.SetupGet(a => a.IsConnected).Returns(true);
            var versionChecker = new Mock<IVersionChecker>();
            var asyncWorker = new Mock<IAsyncWorker>();
            asyncWorker.Setup(w => w.Start(It.IsAny<System.Action>(), It.IsAny<System.Action>())).Verifiable();
            var connected1 = new Mock<IServer>();
            var connected2 = new Mock<IServer>();
            var notConnected = new Mock<IServer>();
            connected1.Setup(a => a.IsConnected).Returns(true).Verifiable();
            connected1.Setup(a => a.Disconnect()).Verifiable();
            connected2.Setup(a => a.IsConnected).Returns(true).Verifiable();
            connected2.Setup(a => a.Disconnect()).Verifiable();
            IList<IServer> lst = new List<IServer> { connected1.Object, connected2.Object, notConnected.Object };
            environmentRepository.Setup(repo => repo.All()).Returns(lst);
            var vieFactory = new Mock<IViewFactory>();
            var viewMock = new Mock<IView>();
            vieFactory.Setup(factory => factory.GetViewGivenServerResourceType(It.IsAny<string>())).Returns(viewMock.Object);

            var popup = new Mock<Common.Interfaces.Studio.Controller.IPopupController>();
            popup.Setup(a => a.Show(StringResources.Unsaved_Changes, StringResources.CloseHeader,
                               MessageBoxButton.YesNoCancel, MessageBoxImage.Information, @"", false, false, true, false, false, false)).Returns(MessageBoxResult.Yes).Verifiable();
            var mvm = new ShellViewModel(eventPublisher.Object, asyncWorker.Object, environmentRepository.Object, versionChecker.Object, vieFactory.Object, false, null, popup.Object);


            ServerRepository.Instance.ActiveServer = environmentModel.Object;

            var settings = new SettingsViewModelForTest(EventPublishers.Aggregator, popup.Object, new SynchronousAsyncWorker(), new NativeWindow()) { RetValue = false, WorkSurfaceContext = WorkSurfaceContext.Settings };
            var task = new Mock<IScheduledResource>();
            task.Setup(a => a.IsDirty).Returns(true);
            settings.IsDirty = true;
            var vm = new WorkSurfaceContextViewModel(new EventAggregator(), new WorkSurfaceKey(), settings, new Mock<Common.Interfaces.Studio.Controller.IPopupController>().Object, (a, b, c) => { });

            mvm.Items.Add(vm);
            Assert.IsFalse(mvm.OnStudioClosing());

        }

        [TestMethod]
        [TestCategory("MainViewModel_OnStudioClosing")]
        [Owner("Leon Rajindrapersadh")]
        public void MainViewModel_OnStudioClosing_CallsSettingsOnClosingDirty()
        {
            var viewModel = new Mock<IShellViewModel>();
            var server = (IServer)CustomContainer.Get(typeof(IServer));
            var mockEnvironmentConnection = SetupMockConnection();
            server.Connection = mockEnvironmentConnection.Object;
            viewModel.SetupGet(model => model.ActiveServer).Returns(server);
            CustomContainer.Register(viewModel.Object);

            var eventPublisher = new Mock<IEventAggregator>();
            var environmentRepository = new Mock<IServerRepository>();
            var serverMock = new Mock<IServer>();
            serverMock.Setup(it => it.Connection).Returns(mockEnvironmentConnection.Object);
            environmentRepository.Setup(repo => repo.Source).Returns(serverMock.Object);
            CustomContainer.Register(environmentRepository.Object);
            var versionChecker = new Mock<IVersionChecker>();
            var asyncWorker = new Mock<IAsyncWorker>();
            asyncWorker.Setup(w => w.Start(It.IsAny<System.Action>(), It.IsAny<System.Action>())).Verifiable();
            var connected1 = new Mock<IServer>();
            var connected2 = new Mock<IServer>();
            var notConnected = new Mock<IServer>();
            notConnected.Setup(it => it.Connection).Returns(mockEnvironmentConnection.Object);
            connected1.Setup(a => a.IsConnected).Returns(true).Verifiable();
            connected1.Setup(a => a.Disconnect()).Verifiable();
            connected1.Setup(it => it.Connection).Returns(mockEnvironmentConnection.Object);
            connected2.Setup(a => a.IsConnected).Returns(true).Verifiable();
            connected2.Setup(a => a.Disconnect()).Verifiable();
            connected2.Setup(it => it.Connection).Returns(mockEnvironmentConnection.Object);
            IList<IServer> lst = new List<IServer> { connected1.Object, connected2.Object, notConnected.Object };
            environmentRepository.Setup(repo => repo.All()).Returns(lst);
            var vieFactory = new Mock<IViewFactory>();
            var viewMock = new Mock<IView>();
            vieFactory.Setup(factory => factory.GetViewGivenServerResourceType(It.IsAny<string>())).Returns(viewMock.Object);

            var popup = new Mock<Common.Interfaces.Studio.Controller.IPopupController>();
            popup.Setup(a => a.Show(StringResources.Unsaved_Changes, StringResources.CloseHeader,
                               MessageBoxButton.YesNoCancel, MessageBoxImage.Information, @"", false, false, true, false, false, false)).Returns(MessageBoxResult.Cancel).Verifiable();

            var mvm = new ShellViewModel(eventPublisher.Object, asyncWorker.Object, environmentRepository.Object, versionChecker.Object, vieFactory.Object, false, null, popup.Object);
            var settings = new SettingsViewModelForTest(EventPublishers.Aggregator, popup.Object, new SynchronousAsyncWorker(), new NativeWindow()) { RetValue = true, WorkSurfaceContext = WorkSurfaceContext.Settings };
            var task = new Mock<IScheduledResource>();
            task.Setup(a => a.IsDirty).Returns(true);
            settings.IsDirty = true;
            var vm = new WorkSurfaceContextViewModel(new EventAggregator(), new WorkSurfaceKey(), settings, new Mock<Common.Interfaces.Studio.Controller.IPopupController>().Object, (a, b, c) => { });
            environmentRepository.Setup(repo => repo.All()).Returns(new List<IServer>());
            mvm.Items.Add(vm);
            Assert.IsFalse(mvm.OnStudioClosing());

        }

        [TestMethod]
        [TestCategory("MainViewModel_OnStudioClosing")]
        [Owner("Pieter Terblanche")]
        public void MainViewModel_OnStudioClosing_CallsWorkflowOnClosing()
        {
            var viewModel = new Mock<IShellViewModel>();
            var server = new Mock<IServer>();
            server.SetupGet(server1 => server1.IsConnected).Returns(true);
            viewModel.SetupGet(model => model.ActiveServer).Returns(server.Object);
            viewModel.SetupGet(model => model.ActiveServer).Returns(server.Object);
            viewModel.SetupGet(model => model.LocalhostServer).Returns(server.Object);
            CustomContainer.Register(viewModel.Object);

            var eventPublisher = new Mock<IEventAggregator>();
            var environmentRepository = new Mock<IServerRepository>();
            CustomContainer.Register(environmentRepository.Object);
            var environmentModel = new Mock<IServer>();
            var environmentConnection = new Mock<IEnvironmentConnection>().Object;
            environmentModel.SetupGet(a => a.Connection).Returns(environmentConnection);
            environmentModel.SetupGet(a => a.IsLocalHost).Returns(true);
            var mockAuthService = new Mock<IAuthorizationService>();
            var mockSecurityService = new Mock<ISecurityService>();
            mockSecurityService.Setup(a => a.Permissions).Returns(new List<WindowsGroupPermission>(new[] { WindowsGroupPermission.CreateEveryone(), }));
            mockAuthService.SetupGet(service => service.SecurityService).Returns(mockSecurityService.Object);
            environmentModel.Setup(c => c.AuthorizationService).Returns(mockAuthService.Object);
            environmentRepository.Setup(repo => repo.Source).Returns(environmentModel.Object);
            environmentModel.SetupGet(a => a.IsConnected).Returns(true);
            var versionChecker = new Mock<IVersionChecker>();
            var asyncWorker = new Mock<IAsyncWorker>();
            asyncWorker.Setup(w => w.Start(It.IsAny<System.Action>(), It.IsAny<System.Action>())).Verifiable();
            var connected1 = new Mock<IServer>();
            var connected2 = new Mock<IServer>();
            var notConnected = new Mock<IServer>();
            connected1.Setup(a => a.IsConnected).Returns(true).Verifiable();
            connected1.Setup(a => a.Disconnect()).Verifiable();
            connected2.Setup(a => a.IsConnected).Returns(true).Verifiable();
            connected2.Setup(a => a.Disconnect()).Verifiable();
            IList<IServer> lst = new List<IServer> { connected1.Object, connected2.Object, notConnected.Object };
            environmentRepository.Setup(repo => repo.All()).Returns(lst);
            var vieFactory = new Mock<IViewFactory>();
            var viewMock = new Mock<IView>();
            vieFactory.Setup(factory => factory.GetViewGivenServerResourceType(It.IsAny<string>())).Returns(viewMock.Object);

            var popup = new Mock<Common.Interfaces.Studio.Controller.IPopupController>();
            popup.Setup(a => a.Show(StringResources.Unsaved_Changes, StringResources.CloseHeader,
                               MessageBoxButton.YesNoCancel, MessageBoxImage.Information, @"", false, false, true, false, false, false)).Returns(MessageBoxResult.Cancel).Verifiable();

            var mvm = new ShellViewModel(eventPublisher.Object, asyncWorker.Object, environmentRepository.Object, versionChecker.Object, vieFactory.Object, false, null, popup.Object);

            CreateFullExportsAndVm();
            var surfaceViewModel = new Mock<IWorkSurfaceViewModel>();
            var workSurfaceKey = new WorkSurfaceKey()
            {
                WorkSurfaceContext = WorkSurfaceContext.Workflow
            };
            var surfaceContext = new Mock<WorkSurfaceContextViewModel>(workSurfaceKey, surfaceViewModel.Object);
            _shellViewModel.Items.Add(surfaceContext.Object);

            ServerRepository.Instance.ActiveServer = environmentModel.Object;
            var activetx = _shellViewModel.Items.ToList().First(i => i.WorkSurfaceViewModel.WorkSurfaceContext == WorkSurfaceContext.Workflow);

            var vm = new WorkSurfaceContextViewModel(new EventAggregator(), new WorkSurfaceKey(), activetx.WorkSurfaceViewModel, new Mock<Common.Interfaces.Studio.Controller.IPopupController>().Object, (a, b, c) => { });

            mvm.Items.Add(vm);
            Assert.IsFalse(mvm.OnStudioClosing());
        }

        void InitSourceViewModel(out Mock<IServer> environmentModel, out ShellViewModel mvm)
        {
            var viewModel = new Mock<IShellViewModel>();
            var server = new Mock<IServer>();
            server.SetupGet(server1 => server1.IsConnected).Returns(true);
            server.Setup(server1 => server1.UpdateRepository).Returns(new Mock<IStudioUpdateManager>().Object);
            server.Setup(server1 => server1.QueryProxy).Returns(new Mock<IQueryManager>().Object);
            server.Setup(server1 => server1.Name).Returns("localhost");
            viewModel.SetupGet(model => model.ActiveServer).Returns(server.Object);
            viewModel.SetupGet(model => model.LocalhostServer).Returns(server.Object);
            CustomContainer.Register(viewModel.Object);

            var eventPublisher = new Mock<IEventAggregator>();
            var environmentRepository = new Mock<IServerRepository>();

            environmentModel = new Mock<IServer>();
            var environmentConnection = new Mock<IEnvironmentConnection>().Object;
            environmentModel.SetupGet(a => a.Connection).Returns(environmentConnection);
            environmentModel.SetupGet(a => a.IsLocalHost).Returns(true);
            var mockAuthService = new Mock<IAuthorizationService>();
            var mockSecurityService = new Mock<ISecurityService>();
            mockSecurityService.Setup(a => a.Permissions).Returns(new List<WindowsGroupPermission>(new[] { WindowsGroupPermission.CreateEveryone(), }));
            mockAuthService.SetupGet(service => service.SecurityService).Returns(mockSecurityService.Object);
            environmentModel.Setup(c => c.AuthorizationService).Returns(mockAuthService.Object);
            environmentRepository.Setup(repo => repo.Source).Returns(environmentModel.Object);
            environmentModel.SetupGet(a => a.IsConnected).Returns(true);
            var versionChecker = new Mock<IVersionChecker>();
            var asyncWorker = new Mock<IAsyncWorker>();
            asyncWorker.Setup(w => w.Start(It.IsAny<System.Action>(), It.IsAny<System.Action>())).Verifiable();
            var connected1 = new Mock<IServer>();
            var connected2 = new Mock<IServer>();
            var notConnected = new Mock<IServer>();
            connected1.Setup(a => a.IsConnected).Returns(true).Verifiable();
            connected1.Setup(a => a.Disconnect()).Verifiable();
            connected2.Setup(a => a.IsConnected).Returns(true).Verifiable();
            connected2.Setup(a => a.Disconnect()).Verifiable();
            IList<IServer> lst = new List<IServer> { connected1.Object, connected2.Object, notConnected.Object };
            environmentRepository.Setup(repo => repo.All()).Returns(lst);
            var vieFactory = new Mock<IViewFactory>();
            var viewMock = new Mock<IView>();
            vieFactory.Setup(factory => factory.GetViewGivenServerResourceType(It.IsAny<string>())).Returns(viewMock.Object);

            var popup = new Mock<Common.Interfaces.Studio.Controller.IPopupController>();
            popup.Setup(a => a.Show(StringResources.Unsaved_Changes, StringResources.CloseHeader,
                               MessageBoxButton.YesNoCancel, MessageBoxImage.Information, @"", false, false, true, false, false, false)).Returns(MessageBoxResult.Cancel).Verifiable();
            CustomContainer.Register(environmentRepository.Object);
            mvm = new ShellViewModel(eventPublisher.Object, asyncWorker.Object, environmentRepository.Object, versionChecker.Object, vieFactory.Object, false, null, popup.Object);
            CreateFullExportsAndVm();
        }

        [TestMethod]
        [TestCategory("MainViewModel_OnStudioClosing")]
        [Owner("Pieter Terblanche")]
        public void MainViewModel_OnStudioClosing_CallsDatabaseOnClosing()
        {
            InitSourceViewModel(out Mock<IServer> environmentModel, out ShellViewModel mvm);
            var resourceModelMock = new Mock<IContextualResourceModel>();
            resourceModelMock.SetupGet(model => model.ServerResourceType).Returns("SqlDatabase");
            const string WorkFlowXaml = "<XamlDefinition>&lt;Activity x:Class=\"PBI 6690 - TEST\" xmlns=\"http://schemas.microsoft.com/netfx/2009/xaml/activities\" xmlns:av=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" xmlns:dc=\"clr-namespace:Dev2.Common;assembly=Dev2.CommonDataUtils\" xmlns:ddc=\"clr-namespace:Dev2.DataList.Contract;assembly=Dev2.Data\" xmlns:ddcb=\"clr-namespace:Dev2.DataList.Contract.Binary_Objects;assembly=Dev2.Data\" xmlns:ddd=\"clr-namespace:Dev2.Data.Decision;assembly=Dev2.Data\" xmlns:dddo=\"clr-namespace:Dev2.Data.Decisions.Operations;assembly=Dev2.Data\" xmlns:ddsm=\"clr-namespace:Dev2.Data.SystemTemplates.Models;assembly=Dev2.Data\" xmlns:mva=\"clr-namespace:Microsoft.VisualBasic.Activities;assembly=System.Activities\" xmlns:s=\"clr-namespace:System;assembly=mscorlib\" xmlns:sap=\"http://schemas.microsoft.com/netfx/2009/xaml/activities/presentation\" xmlns:scg=\"clr-namespace:System.Collections.Generic;assembly=mscorlib\" xmlns:sco=\"clr-namespace:System.Collections.ObjectModel;assembly=mscorlib\" xmlns:uaba=\"clr-namespace:Unlimited.Applications.BusinessDesignStudio.Activities;assembly=Dev2.Activities\" xmlns:uf=\"clr-namespace:Unlimited.Framework;assembly=Dev2.Core\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"&gt;&lt;x:Members&gt;&lt;x:Property Name=\"AmbientDataList\" Type=\"InOutArgument(scg:List(x:String))\" /&gt;&lt;x:Property Name=\"ParentWorkflowInstanceId\" Type=\"InOutArgument(s:Guid)\" /&gt;&lt;x:Property Name=\"ParentServiceName\" Type=\"InOutArgument(x:String)\" /&gt;&lt;/x:Members&gt;&lt;sap:VirtualizedContainerService.HintSize&gt;654,676&lt;/sap:VirtualizedContainerService.HintSize&gt;&lt;mva:VisualBasic.Settings&gt;Assembly references and imported namespaces serialized as XML namespaces&lt;/mva:VisualBasic.Settings&gt;&lt;TextExpression.NamespacesForImplementation&gt;&lt;scg:List x:TypeArguments=\"x:String\" Capacity=\"7\"&gt;&lt;x:String&gt;Dev2.CommonDataUtils&lt;/x:String&gt;&lt;x:String&gt;Dev2.Data.Decisions.Operations&lt;/x:String&gt;&lt;x:String&gt;Dev2.Data.SystemTemplates.Models&lt;/x:String&gt;&lt;x:String&gt;Dev2.DataList.Contract&lt;/x:String&gt;&lt;x:String&gt;Dev2.DataList.Contract.Binary_Objects&lt;/x:String&gt;&lt;x:String&gt;Unlimited.Framework&lt;/x:String&gt;&lt;x:String&gt;Unlimited.Applications.BusinessDesignStudio.Activities&lt;/x:String&gt;&lt;/scg:List&gt;&lt;/TextExpression.NamespacesForImplementation&gt;&lt;TextExpression.ReferencesForImplementation&gt;&lt;sco:Collection x:TypeArguments=\"AssemblyReference\"&gt;&lt;AssemblyReference&gt;Dev2.CommonDataUtils&lt;/AssemblyReference&gt;&lt;AssemblyReference&gt;Dev2.Data&lt;/AssemblyReference&gt;&lt;AssemblyReference&gt;Dev2.Core&lt;/AssemblyReference&gt;&lt;AssemblyReference&gt;Dev2.Activities&lt;/AssemblyReference&gt;&lt;/sco:Collection&gt;&lt;/TextExpression.ReferencesForImplementation&gt;&lt;Flowchart DisplayName=\"PBI 6690 - TEST\" sap:VirtualizedContainerService.HintSize=\"614,636\"&gt;&lt;Flowchart.Variables&gt;&lt;Variable x:TypeArguments=\"scg:List(x:String)\" Name=\"InstructionList\" /&gt;&lt;Variable x:TypeArguments=\"x:String\" Name=\"LastResult\" /&gt;&lt;Variable x:TypeArguments=\"x:Boolean\" Name=\"HasError\" /&gt;&lt;Variable x:TypeArguments=\"x:String\" Name=\"ExplicitDataList\" /&gt;&lt;Variable x:TypeArguments=\"x:Boolean\" Name=\"IsValid\" /&gt;&lt;Variable x:TypeArguments=\"uf:UnlimitedObject\" Name=\"d\" /&gt;&lt;Variable x:TypeArguments=\"uaba:Util\" Name=\"t\" /&gt;&lt;Variable x:TypeArguments=\"ddd:Dev2DataListDecisionHandler\" Name=\"Dev2DecisionHandler\" /&gt;&lt;/Flowchart.Variables&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=\"x:String, x:Object\"&gt;&lt;x:Boolean x:Key=\"IsExpanded\"&gt;False&lt;/x:Boolean&gt;&lt;av:Point x:Key=\"ShapeLocation\"&gt;270,2.5&lt;/av:Point&gt;&lt;av:Size x:Key=\"ShapeSize\"&gt;60,75&lt;/av:Size&gt;&lt;av:PointCollection x:Key=\"ConnectorLocation\"&gt;300,77.5 300,107.5 310,107.5 310,211&lt;/av:PointCollection&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;Flowchart.StartNode&gt;&lt;x:Reference&gt;__ReferenceID0&lt;/x:Reference&gt;&lt;/Flowchart.StartNode&gt;&lt;FlowStep x:Name=\"__ReferenceID0\"&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=\"x:String, x:Object\"&gt;&lt;av:Point x:Key=\"ShapeLocation\"&gt;185,211&lt;/av:Point&gt;&lt;av:Size x:Key=\"ShapeSize\"&gt;250,84&lt;/av:Size&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;uaba:DsfActivity ActionName=\"{x:Null}\" ActivityStateData=\"{x:Null}\" AuthorRoles=\"{x:Null}\" Category=\"{x:Null}\" Compiler=\"{x:Null}\" CurrentResult=\"{x:Null}\" DataObject=\"{x:Null}\" DataTags=\"{x:Null}\" ExplicitDataList=\"{x:Null}\" InputTransformation=\"{x:Null}\" OnResumeKeepList=\"{x:Null}\" ParentServiceID=\"{x:Null}\" ParentServiceName=\"{x:Null}\" ParentWorkflowInstanceId=\"{x:Null}\" ResultTransformation=\"{x:Null}\" ResultValidationExpression=\"{x:Null}\" ResultValidationRequiredTags=\"{x:Null}\" ScenarioID=\"{x:Null}\" ScopingObject=\"{x:Null}\" ServiceUri=\"{x:Null}\" SimulationOutput=\"{x:Null}\" Tags=\"{x:Null}\" AddMode=\"False\" DatabindRecursive=\"False\" DeferExecution=\"False\" DisplayName=\"PBI 6690 - TEST Inputs\" EnvironmentID=\"00000000-0000-0000-0000-000000000000\" FriendlySourceName=\"localhost\" HasError=\"[HasError]\" sap:VirtualizedContainerService.HintSize=\"250,84\" IconPath=\"pack://application:,,,/Warewolf Studio;component/images/Workflow-32.png\" InputMapping=\"&amp;lt;Inputs&amp;gt;&amp;lt;Input Name=&amp;quot;n1&amp;quot; Source=&amp;quot;&amp;quot; /&amp;gt;&amp;lt;Input Name=&amp;quot;n2&amp;quot; Source=&amp;quot;&amp;quot; /&amp;gt;&amp;lt;Input Name=&amp;quot;n3&amp;quot; Source=&amp;quot;&amp;quot; /&amp;gt;&amp;lt;/Inputs&amp;gt;\" InstructionList=\"[InstructionList]\" IsSimulationEnabled=\"False\" IsUIStep=\"False\" IsValid=\"[IsValid]\" IsWorkflow=\"True\" OnResumeClearAmbientDataList=\"False\" OnResumeClearTags=\"FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage\" OutputMapping=\"&amp;lt;Outputs&amp;gt;&amp;lt;Output Name=&amp;quot;result&amp;quot; MapsTo=&amp;quot;result&amp;quot; Value=&amp;quot;&amp;quot; /&amp;gt;&amp;lt;/Outputs&amp;gt;\" RemoveInputFromOutput=\"False\" ServiceName=\"PBI 6690 - TEST Inputs\" ServiceServer=\"00000000-0000-0000-0000-000000000000\" SimulationMode=\"OnDemand\" ToolboxFriendlyName=\"PBI 6690 - TEST Inputs\" Type=\"Workflow\" UniqueID=\"edadb62e-83f4-44bf-a260-7639d6b43169\"&gt;&lt;uaba:DsfActivity.AmbientDataList&gt;&lt;InOutArgument x:TypeArguments=\"scg:List(x:String)\" /&gt;&lt;/uaba:DsfActivity.AmbientDataList&gt;&lt;uaba:DsfActivity.HelpLink&gt;&lt;InArgument x:TypeArguments=\"x:String\"&gt;&lt;Literal x:TypeArguments=\"x:String\" Value=\"\" /&gt;&lt;/InArgument&gt;&lt;/uaba:DsfActivity.HelpLink&gt;&lt;uaba:DsfActivity.ParentInstanceID&gt;&lt;InOutArgument x:TypeArguments=\"x:String\" /&gt;&lt;/uaba:DsfActivity.ParentInstanceID&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=\"x:String, x:Object\"&gt;&lt;x:Boolean x:Key=\"IsExpanded\"&gt;True&lt;/x:Boolean&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;/uaba:DsfActivity&gt;&lt;/FlowStep&gt;&lt;FlowStep&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=\"x:String, x:Object\"&gt;&lt;av:Point x:Key=\"ShapeLocation\"&gt;145,390&lt;/av:Point&gt;&lt;av:Size x:Key=\"ShapeSize\"&gt;250,100&lt;/av:Size&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;uaba:DsfActivity ActivityStateData=\"{x:Null}\" AuthorRoles=\"{x:Null}\" Category=\"{x:Null}\" Compiler=\"{x:Null}\" CurrentResult=\"{x:Null}\" DataObject=\"{x:Null}\" DataTags=\"{x:Null}\" ExplicitDataList=\"{x:Null}\" HelpLink=\"{x:Null}\" InputTransformation=\"{x:Null}\" OnResumeKeepList=\"{x:Null}\" ParentServiceID=\"{x:Null}\" ParentServiceName=\"{x:Null}\" ParentWorkflowInstanceId=\"{x:Null}\" ResultTransformation=\"{x:Null}\" ResultValidationExpression=\"{x:Null}\" ResultValidationRequiredTags=\"{x:Null}\" ScenarioID=\"{x:Null}\" ScopingObject=\"{x:Null}\" ServiceUri=\"{x:Null}\" SimulationOutput=\"{x:Null}\" Tags=\"{x:Null}\" ActionName=\"dbo.spGetCountries\" AddMode=\"False\" DatabindRecursive=\"False\" DeferExecution=\"False\" DisplayName=\"Countries\" EnvironmentID=\"00000000-0000-0000-0000-000000000000\" FriendlySourceName=\"CitiesDb\" HasError=\"[HasError]\" sap:VirtualizedContainerService.HintSize=\"250,100\" IconPath=\"[Nothing]\" InputMapping=\"&amp;lt;Inputs&amp;gt;&amp;lt;Input Name=&amp;quot;Prefix&amp;quot; Source=&amp;quot;Prefix&amp;quot; /&amp;gt;&amp;lt;/Inputs&amp;gt;\" InstructionList=\"[InstructionList]\" IsSimulationEnabled=\"False\" IsUIStep=\"False\" IsValid=\"[IsValid]\" IsWorkflow=\"False\" OnResumeClearAmbientDataList=\"False\" OnResumeClearTags=\"FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage\" OutputMapping=\"&amp;lt;Outputs&amp;gt;&amp;lt;Output Name=&amp;quot;CountryID&amp;quot; MapsTo=&amp;quot;CountryID&amp;quot; Value=&amp;quot;[[GetCountries().CountryID]]&amp;quot; Recordset=&amp;quot;GetCountries&amp;quot; /&amp;gt;&amp;lt;Output Name=&amp;quot;Description&amp;quot; MapsTo=&amp;quot;Name&amp;quot; Value=&amp;quot;[[GetCountries().Name]]&amp;quot; Recordset=&amp;quot;GetCountries&amp;quot; /&amp;gt;&amp;lt;/Outputs&amp;gt;\" RemoveInputFromOutput=\"False\" ServiceName=\"Countries\" ServiceServer=\"00000000-0000-0000-0000-000000000000\" SimulationMode=\"OnDemand\" ToolboxFriendlyName=\"Countries\" Type=\"InvokeStoredProc\" UniqueID=\"33476822-9519-4ad5-8fee-1ae3b571115b\"&gt;&lt;uaba:DsfActivity.AmbientDataList&gt;&lt;InOutArgument x:TypeArguments=\"scg:List(x:String)\" /&gt;&lt;/uaba:DsfActivity.AmbientDataList&gt;&lt;uaba:DsfActivity.ParentInstanceID&gt;&lt;InOutArgument x:TypeArguments=\"x:String\" /&gt;&lt;/uaba:DsfActivity.ParentInstanceID&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=\"x:String, x:Object\"&gt;&lt;x:Boolean x:Key=\"IsExpanded\"&gt;True&lt;/x:Boolean&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;/uaba:DsfActivity&gt;&lt;/FlowStep&gt;&lt;/Flowchart&gt;&lt;/Activity&gt;</XamlDefinition>";
            resourceModelMock.SetupGet(model => model.WorkflowXaml).Returns(new StringBuilder(WorkFlowXaml));
            resourceModelMock.SetupGet(model => model.Environment.EnvironmentID).Returns(Guid.NewGuid);
            resourceModelMock.SetupGet(model => model.ID).Returns(Guid.NewGuid);
            resourceModelMock.SetupGet(model => model.ServerID).Returns(Guid.NewGuid);
            var newMockServer = new Mock<IServerRepository>();
            newMockServer.Setup(repository => repository.ActiveServer).Returns(environmentModel.Object);
            CustomContainer.DeRegister<IServerRepository>();
            CustomContainer.Register(newMockServer.Object);
            var worksurfaceContextManager = new WorksurfaceContextManager(false, _shellViewModel, newMockServer.Object, new Mock<IViewFactory>().Object);

            _activeEnvironment.Setup(model => model.Name).Returns("localhost");
            //---------------Execute Test ----------------------
            worksurfaceContextManager.DisplayResourceWizard(resourceModelMock.Object);


            var activetx = _shellViewModel.Items.ToList().First(i => i.WorkSurfaceViewModel.GetType().Name == "SourceViewModel`1");

            var vm = new WorkSurfaceContextViewModel(new EventAggregator(), new WorkSurfaceKey(), activetx.WorkSurfaceViewModel, new Mock<Common.Interfaces.Studio.Controller.IPopupController>().Object, (a, b, c) => { });
            var sourceVM = vm.WorkSurfaceViewModel as SourceViewModel<IDbSource>;
            sourceVM.ViewModel.Item = new Mock<IDbSource>().Object;
            mvm.Items.Add(vm);
            Assert.IsFalse(mvm.OnStudioClosing());
        }


        [TestMethod]
        [TestCategory("MainViewModel_OnStudioClosing")]
        [Owner("Pieter Terblanche")]
        public void MainViewModel_OnStudioClosing_CallsEmailOnClosing()
        {
            InitSourceViewModel(out Mock<IServer> environmentModel, out ShellViewModel mvm);
            var resourceModelMock = new Mock<IContextualResourceModel>();
            resourceModelMock.SetupGet(model => model.ServerResourceType).Returns("EmailSource");
            const string WorkFlowXaml = "<XamlDefinition>&lt;Activity x:Class=\"PBI 6690 - TEST\" xmlns=\"http://schemas.microsoft.com/netfx/2009/xaml/activities\" xmlns:av=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" xmlns:dc=\"clr-namespace:Dev2.Common;assembly=Dev2.CommonDataUtils\" xmlns:ddc=\"clr-namespace:Dev2.DataList.Contract;assembly=Dev2.Data\" xmlns:ddcb=\"clr-namespace:Dev2.DataList.Contract.Binary_Objects;assembly=Dev2.Data\" xmlns:ddd=\"clr-namespace:Dev2.Data.Decision;assembly=Dev2.Data\" xmlns:dddo=\"clr-namespace:Dev2.Data.Decisions.Operations;assembly=Dev2.Data\" xmlns:ddsm=\"clr-namespace:Dev2.Data.SystemTemplates.Models;assembly=Dev2.Data\" xmlns:mva=\"clr-namespace:Microsoft.VisualBasic.Activities;assembly=System.Activities\" xmlns:s=\"clr-namespace:System;assembly=mscorlib\" xmlns:sap=\"http://schemas.microsoft.com/netfx/2009/xaml/activities/presentation\" xmlns:scg=\"clr-namespace:System.Collections.Generic;assembly=mscorlib\" xmlns:sco=\"clr-namespace:System.Collections.ObjectModel;assembly=mscorlib\" xmlns:uaba=\"clr-namespace:Unlimited.Applications.BusinessDesignStudio.Activities;assembly=Dev2.Activities\" xmlns:uf=\"clr-namespace:Unlimited.Framework;assembly=Dev2.Core\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"&gt;&lt;x:Members&gt;&lt;x:Property Name=\"AmbientDataList\" Type=\"InOutArgument(scg:List(x:String))\" /&gt;&lt;x:Property Name=\"ParentWorkflowInstanceId\" Type=\"InOutArgument(s:Guid)\" /&gt;&lt;x:Property Name=\"ParentServiceName\" Type=\"InOutArgument(x:String)\" /&gt;&lt;/x:Members&gt;&lt;sap:VirtualizedContainerService.HintSize&gt;654,676&lt;/sap:VirtualizedContainerService.HintSize&gt;&lt;mva:VisualBasic.Settings&gt;Assembly references and imported namespaces serialized as XML namespaces&lt;/mva:VisualBasic.Settings&gt;&lt;TextExpression.NamespacesForImplementation&gt;&lt;scg:List x:TypeArguments=\"x:String\" Capacity=\"7\"&gt;&lt;x:String&gt;Dev2.CommonDataUtils&lt;/x:String&gt;&lt;x:String&gt;Dev2.Data.Decisions.Operations&lt;/x:String&gt;&lt;x:String&gt;Dev2.Data.SystemTemplates.Models&lt;/x:String&gt;&lt;x:String&gt;Dev2.DataList.Contract&lt;/x:String&gt;&lt;x:String&gt;Dev2.DataList.Contract.Binary_Objects&lt;/x:String&gt;&lt;x:String&gt;Unlimited.Framework&lt;/x:String&gt;&lt;x:String&gt;Unlimited.Applications.BusinessDesignStudio.Activities&lt;/x:String&gt;&lt;/scg:List&gt;&lt;/TextExpression.NamespacesForImplementation&gt;&lt;TextExpression.ReferencesForImplementation&gt;&lt;sco:Collection x:TypeArguments=\"AssemblyReference\"&gt;&lt;AssemblyReference&gt;Dev2.CommonDataUtils&lt;/AssemblyReference&gt;&lt;AssemblyReference&gt;Dev2.Data&lt;/AssemblyReference&gt;&lt;AssemblyReference&gt;Dev2.Core&lt;/AssemblyReference&gt;&lt;AssemblyReference&gt;Dev2.Activities&lt;/AssemblyReference&gt;&lt;/sco:Collection&gt;&lt;/TextExpression.ReferencesForImplementation&gt;&lt;Flowchart DisplayName=\"PBI 6690 - TEST\" sap:VirtualizedContainerService.HintSize=\"614,636\"&gt;&lt;Flowchart.Variables&gt;&lt;Variable x:TypeArguments=\"scg:List(x:String)\" Name=\"InstructionList\" /&gt;&lt;Variable x:TypeArguments=\"x:String\" Name=\"LastResult\" /&gt;&lt;Variable x:TypeArguments=\"x:Boolean\" Name=\"HasError\" /&gt;&lt;Variable x:TypeArguments=\"x:String\" Name=\"ExplicitDataList\" /&gt;&lt;Variable x:TypeArguments=\"x:Boolean\" Name=\"IsValid\" /&gt;&lt;Variable x:TypeArguments=\"uf:UnlimitedObject\" Name=\"d\" /&gt;&lt;Variable x:TypeArguments=\"uaba:Util\" Name=\"t\" /&gt;&lt;Variable x:TypeArguments=\"ddd:Dev2DataListDecisionHandler\" Name=\"Dev2DecisionHandler\" /&gt;&lt;/Flowchart.Variables&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=\"x:String, x:Object\"&gt;&lt;x:Boolean x:Key=\"IsExpanded\"&gt;False&lt;/x:Boolean&gt;&lt;av:Point x:Key=\"ShapeLocation\"&gt;270,2.5&lt;/av:Point&gt;&lt;av:Size x:Key=\"ShapeSize\"&gt;60,75&lt;/av:Size&gt;&lt;av:PointCollection x:Key=\"ConnectorLocation\"&gt;300,77.5 300,107.5 310,107.5 310,211&lt;/av:PointCollection&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;Flowchart.StartNode&gt;&lt;x:Reference&gt;__ReferenceID0&lt;/x:Reference&gt;&lt;/Flowchart.StartNode&gt;&lt;FlowStep x:Name=\"__ReferenceID0\"&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=\"x:String, x:Object\"&gt;&lt;av:Point x:Key=\"ShapeLocation\"&gt;185,211&lt;/av:Point&gt;&lt;av:Size x:Key=\"ShapeSize\"&gt;250,84&lt;/av:Size&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;uaba:DsfActivity ActionName=\"{x:Null}\" ActivityStateData=\"{x:Null}\" AuthorRoles=\"{x:Null}\" Category=\"{x:Null}\" Compiler=\"{x:Null}\" CurrentResult=\"{x:Null}\" DataObject=\"{x:Null}\" DataTags=\"{x:Null}\" ExplicitDataList=\"{x:Null}\" InputTransformation=\"{x:Null}\" OnResumeKeepList=\"{x:Null}\" ParentServiceID=\"{x:Null}\" ParentServiceName=\"{x:Null}\" ParentWorkflowInstanceId=\"{x:Null}\" ResultTransformation=\"{x:Null}\" ResultValidationExpression=\"{x:Null}\" ResultValidationRequiredTags=\"{x:Null}\" ScenarioID=\"{x:Null}\" ScopingObject=\"{x:Null}\" ServiceUri=\"{x:Null}\" SimulationOutput=\"{x:Null}\" Tags=\"{x:Null}\" AddMode=\"False\" DatabindRecursive=\"False\" DeferExecution=\"False\" DisplayName=\"PBI 6690 - TEST Inputs\" EnvironmentID=\"00000000-0000-0000-0000-000000000000\" FriendlySourceName=\"localhost\" HasError=\"[HasError]\" sap:VirtualizedContainerService.HintSize=\"250,84\" IconPath=\"pack://application:,,,/Warewolf Studio;component/images/Workflow-32.png\" InputMapping=\"&amp;lt;Inputs&amp;gt;&amp;lt;Input Name=&amp;quot;n1&amp;quot; Source=&amp;quot;&amp;quot; /&amp;gt;&amp;lt;Input Name=&amp;quot;n2&amp;quot; Source=&amp;quot;&amp;quot; /&amp;gt;&amp;lt;Input Name=&amp;quot;n3&amp;quot; Source=&amp;quot;&amp;quot; /&amp;gt;&amp;lt;/Inputs&amp;gt;\" InstructionList=\"[InstructionList]\" IsSimulationEnabled=\"False\" IsUIStep=\"False\" IsValid=\"[IsValid]\" IsWorkflow=\"True\" OnResumeClearAmbientDataList=\"False\" OnResumeClearTags=\"FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage\" OutputMapping=\"&amp;lt;Outputs&amp;gt;&amp;lt;Output Name=&amp;quot;result&amp;quot; MapsTo=&amp;quot;result&amp;quot; Value=&amp;quot;&amp;quot; /&amp;gt;&amp;lt;/Outputs&amp;gt;\" RemoveInputFromOutput=\"False\" ServiceName=\"PBI 6690 - TEST Inputs\" ServiceServer=\"00000000-0000-0000-0000-000000000000\" SimulationMode=\"OnDemand\" ToolboxFriendlyName=\"PBI 6690 - TEST Inputs\" Type=\"Workflow\" UniqueID=\"edadb62e-83f4-44bf-a260-7639d6b43169\"&gt;&lt;uaba:DsfActivity.AmbientDataList&gt;&lt;InOutArgument x:TypeArguments=\"scg:List(x:String)\" /&gt;&lt;/uaba:DsfActivity.AmbientDataList&gt;&lt;uaba:DsfActivity.HelpLink&gt;&lt;InArgument x:TypeArguments=\"x:String\"&gt;&lt;Literal x:TypeArguments=\"x:String\" Value=\"\" /&gt;&lt;/InArgument&gt;&lt;/uaba:DsfActivity.HelpLink&gt;&lt;uaba:DsfActivity.ParentInstanceID&gt;&lt;InOutArgument x:TypeArguments=\"x:String\" /&gt;&lt;/uaba:DsfActivity.ParentInstanceID&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=\"x:String, x:Object\"&gt;&lt;x:Boolean x:Key=\"IsExpanded\"&gt;True&lt;/x:Boolean&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;/uaba:DsfActivity&gt;&lt;/FlowStep&gt;&lt;FlowStep&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=\"x:String, x:Object\"&gt;&lt;av:Point x:Key=\"ShapeLocation\"&gt;145,390&lt;/av:Point&gt;&lt;av:Size x:Key=\"ShapeSize\"&gt;250,100&lt;/av:Size&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;uaba:DsfActivity ActivityStateData=\"{x:Null}\" AuthorRoles=\"{x:Null}\" Category=\"{x:Null}\" Compiler=\"{x:Null}\" CurrentResult=\"{x:Null}\" DataObject=\"{x:Null}\" DataTags=\"{x:Null}\" ExplicitDataList=\"{x:Null}\" HelpLink=\"{x:Null}\" InputTransformation=\"{x:Null}\" OnResumeKeepList=\"{x:Null}\" ParentServiceID=\"{x:Null}\" ParentServiceName=\"{x:Null}\" ParentWorkflowInstanceId=\"{x:Null}\" ResultTransformation=\"{x:Null}\" ResultValidationExpression=\"{x:Null}\" ResultValidationRequiredTags=\"{x:Null}\" ScenarioID=\"{x:Null}\" ScopingObject=\"{x:Null}\" ServiceUri=\"{x:Null}\" SimulationOutput=\"{x:Null}\" Tags=\"{x:Null}\" ActionName=\"dbo.spGetCountries\" AddMode=\"False\" DatabindRecursive=\"False\" DeferExecution=\"False\" DisplayName=\"Countries\" EnvironmentID=\"00000000-0000-0000-0000-000000000000\" FriendlySourceName=\"CitiesDb\" HasError=\"[HasError]\" sap:VirtualizedContainerService.HintSize=\"250,100\" IconPath=\"[Nothing]\" InputMapping=\"&amp;lt;Inputs&amp;gt;&amp;lt;Input Name=&amp;quot;Prefix&amp;quot; Source=&amp;quot;Prefix&amp;quot; /&amp;gt;&amp;lt;/Inputs&amp;gt;\" InstructionList=\"[InstructionList]\" IsSimulationEnabled=\"False\" IsUIStep=\"False\" IsValid=\"[IsValid]\" IsWorkflow=\"False\" OnResumeClearAmbientDataList=\"False\" OnResumeClearTags=\"FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage\" OutputMapping=\"&amp;lt;Outputs&amp;gt;&amp;lt;Output Name=&amp;quot;CountryID&amp;quot; MapsTo=&amp;quot;CountryID&amp;quot; Value=&amp;quot;[[GetCountries().CountryID]]&amp;quot; Recordset=&amp;quot;GetCountries&amp;quot; /&amp;gt;&amp;lt;Output Name=&amp;quot;Description&amp;quot; MapsTo=&amp;quot;Name&amp;quot; Value=&amp;quot;[[GetCountries().Name]]&amp;quot; Recordset=&amp;quot;GetCountries&amp;quot; /&amp;gt;&amp;lt;/Outputs&amp;gt;\" RemoveInputFromOutput=\"False\" ServiceName=\"Countries\" ServiceServer=\"00000000-0000-0000-0000-000000000000\" SimulationMode=\"OnDemand\" ToolboxFriendlyName=\"Countries\" Type=\"InvokeStoredProc\" UniqueID=\"33476822-9519-4ad5-8fee-1ae3b571115b\"&gt;&lt;uaba:DsfActivity.AmbientDataList&gt;&lt;InOutArgument x:TypeArguments=\"scg:List(x:String)\" /&gt;&lt;/uaba:DsfActivity.AmbientDataList&gt;&lt;uaba:DsfActivity.ParentInstanceID&gt;&lt;InOutArgument x:TypeArguments=\"x:String\" /&gt;&lt;/uaba:DsfActivity.ParentInstanceID&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=\"x:String, x:Object\"&gt;&lt;x:Boolean x:Key=\"IsExpanded\"&gt;True&lt;/x:Boolean&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;/uaba:DsfActivity&gt;&lt;/FlowStep&gt;&lt;/Flowchart&gt;&lt;/Activity&gt;</XamlDefinition>";
            resourceModelMock.SetupGet(model => model.WorkflowXaml).Returns(new StringBuilder(WorkFlowXaml));
            resourceModelMock.SetupGet(model => model.Environment.EnvironmentID).Returns(Guid.NewGuid);
            resourceModelMock.SetupGet(model => model.ID).Returns(Guid.NewGuid);
            resourceModelMock.SetupGet(model => model.ServerID).Returns(Guid.NewGuid);
            var mock = new Mock<IServerRepository>();
            mock.Setup(repository => repository.ActiveServer).Returns(environmentModel.Object);
            CustomContainer.Register(mock.Object);
            var worksurfaceContextManager = new WorksurfaceContextManager(false, _shellViewModel, mock.Object, new Mock<IViewFactory>().Object);

            _activeEnvironment.Setup(model => model.Name).Returns("localhost");
            //---------------Execute Test ----------------------
            worksurfaceContextManager.DisplayResourceWizard(resourceModelMock.Object);

            var activetx = _shellViewModel.Items.ToList().First(i => i.WorkSurfaceViewModel.GetType().Name == "SourceViewModel`1");

            var vm = new WorkSurfaceContextViewModel(new EventAggregator(), new WorkSurfaceKey(), activetx.WorkSurfaceViewModel, new Mock<Common.Interfaces.Studio.Controller.IPopupController>().Object, (a, b, c) => { });
            var sourceVM = vm.WorkSurfaceViewModel as SourceViewModel<IEmailServiceSource>;
            sourceVM.ViewModel.Item = new Mock<IEmailServiceSource>().Object;
            mvm.Items.Add(vm);
            Assert.IsFalse(mvm.OnStudioClosing());
        }

        [TestMethod]
        [TestCategory("MainViewModel_OnStudioClosing")]
        [Owner("Pieter Terblanche")]
        public void MainViewModel_OnStudioClosing_CallsWebSourceOnClosing()
        {
            InitSourceViewModel(out Mock<IServer> environmentModel, out ShellViewModel mvm);
            var resourceModelMock = new Mock<IContextualResourceModel>();
            resourceModelMock.SetupGet(model => model.ServerResourceType).Returns("WebSource");
            const string WorkFlowXaml = "<XamlDefinition>&lt;Activity x:Class=\"PBI 6690 - TEST\" xmlns=\"http://schemas.microsoft.com/netfx/2009/xaml/activities\" xmlns:av=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" xmlns:dc=\"clr-namespace:Dev2.Common;assembly=Dev2.CommonDataUtils\" xmlns:ddc=\"clr-namespace:Dev2.DataList.Contract;assembly=Dev2.Data\" xmlns:ddcb=\"clr-namespace:Dev2.DataList.Contract.Binary_Objects;assembly=Dev2.Data\" xmlns:ddd=\"clr-namespace:Dev2.Data.Decision;assembly=Dev2.Data\" xmlns:dddo=\"clr-namespace:Dev2.Data.Decisions.Operations;assembly=Dev2.Data\" xmlns:ddsm=\"clr-namespace:Dev2.Data.SystemTemplates.Models;assembly=Dev2.Data\" xmlns:mva=\"clr-namespace:Microsoft.VisualBasic.Activities;assembly=System.Activities\" xmlns:s=\"clr-namespace:System;assembly=mscorlib\" xmlns:sap=\"http://schemas.microsoft.com/netfx/2009/xaml/activities/presentation\" xmlns:scg=\"clr-namespace:System.Collections.Generic;assembly=mscorlib\" xmlns:sco=\"clr-namespace:System.Collections.ObjectModel;assembly=mscorlib\" xmlns:uaba=\"clr-namespace:Unlimited.Applications.BusinessDesignStudio.Activities;assembly=Dev2.Activities\" xmlns:uf=\"clr-namespace:Unlimited.Framework;assembly=Dev2.Core\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"&gt;&lt;x:Members&gt;&lt;x:Property Name=\"AmbientDataList\" Type=\"InOutArgument(scg:List(x:String))\" /&gt;&lt;x:Property Name=\"ParentWorkflowInstanceId\" Type=\"InOutArgument(s:Guid)\" /&gt;&lt;x:Property Name=\"ParentServiceName\" Type=\"InOutArgument(x:String)\" /&gt;&lt;/x:Members&gt;&lt;sap:VirtualizedContainerService.HintSize&gt;654,676&lt;/sap:VirtualizedContainerService.HintSize&gt;&lt;mva:VisualBasic.Settings&gt;Assembly references and imported namespaces serialized as XML namespaces&lt;/mva:VisualBasic.Settings&gt;&lt;TextExpression.NamespacesForImplementation&gt;&lt;scg:List x:TypeArguments=\"x:String\" Capacity=\"7\"&gt;&lt;x:String&gt;Dev2.CommonDataUtils&lt;/x:String&gt;&lt;x:String&gt;Dev2.Data.Decisions.Operations&lt;/x:String&gt;&lt;x:String&gt;Dev2.Data.SystemTemplates.Models&lt;/x:String&gt;&lt;x:String&gt;Dev2.DataList.Contract&lt;/x:String&gt;&lt;x:String&gt;Dev2.DataList.Contract.Binary_Objects&lt;/x:String&gt;&lt;x:String&gt;Unlimited.Framework&lt;/x:String&gt;&lt;x:String&gt;Unlimited.Applications.BusinessDesignStudio.Activities&lt;/x:String&gt;&lt;/scg:List&gt;&lt;/TextExpression.NamespacesForImplementation&gt;&lt;TextExpression.ReferencesForImplementation&gt;&lt;sco:Collection x:TypeArguments=\"AssemblyReference\"&gt;&lt;AssemblyReference&gt;Dev2.CommonDataUtils&lt;/AssemblyReference&gt;&lt;AssemblyReference&gt;Dev2.Data&lt;/AssemblyReference&gt;&lt;AssemblyReference&gt;Dev2.Core&lt;/AssemblyReference&gt;&lt;AssemblyReference&gt;Dev2.Activities&lt;/AssemblyReference&gt;&lt;/sco:Collection&gt;&lt;/TextExpression.ReferencesForImplementation&gt;&lt;Flowchart DisplayName=\"PBI 6690 - TEST\" sap:VirtualizedContainerService.HintSize=\"614,636\"&gt;&lt;Flowchart.Variables&gt;&lt;Variable x:TypeArguments=\"scg:List(x:String)\" Name=\"InstructionList\" /&gt;&lt;Variable x:TypeArguments=\"x:String\" Name=\"LastResult\" /&gt;&lt;Variable x:TypeArguments=\"x:Boolean\" Name=\"HasError\" /&gt;&lt;Variable x:TypeArguments=\"x:String\" Name=\"ExplicitDataList\" /&gt;&lt;Variable x:TypeArguments=\"x:Boolean\" Name=\"IsValid\" /&gt;&lt;Variable x:TypeArguments=\"uf:UnlimitedObject\" Name=\"d\" /&gt;&lt;Variable x:TypeArguments=\"uaba:Util\" Name=\"t\" /&gt;&lt;Variable x:TypeArguments=\"ddd:Dev2DataListDecisionHandler\" Name=\"Dev2DecisionHandler\" /&gt;&lt;/Flowchart.Variables&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=\"x:String, x:Object\"&gt;&lt;x:Boolean x:Key=\"IsExpanded\"&gt;False&lt;/x:Boolean&gt;&lt;av:Point x:Key=\"ShapeLocation\"&gt;270,2.5&lt;/av:Point&gt;&lt;av:Size x:Key=\"ShapeSize\"&gt;60,75&lt;/av:Size&gt;&lt;av:PointCollection x:Key=\"ConnectorLocation\"&gt;300,77.5 300,107.5 310,107.5 310,211&lt;/av:PointCollection&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;Flowchart.StartNode&gt;&lt;x:Reference&gt;__ReferenceID0&lt;/x:Reference&gt;&lt;/Flowchart.StartNode&gt;&lt;FlowStep x:Name=\"__ReferenceID0\"&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=\"x:String, x:Object\"&gt;&lt;av:Point x:Key=\"ShapeLocation\"&gt;185,211&lt;/av:Point&gt;&lt;av:Size x:Key=\"ShapeSize\"&gt;250,84&lt;/av:Size&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;uaba:DsfActivity ActionName=\"{x:Null}\" ActivityStateData=\"{x:Null}\" AuthorRoles=\"{x:Null}\" Category=\"{x:Null}\" Compiler=\"{x:Null}\" CurrentResult=\"{x:Null}\" DataObject=\"{x:Null}\" DataTags=\"{x:Null}\" ExplicitDataList=\"{x:Null}\" InputTransformation=\"{x:Null}\" OnResumeKeepList=\"{x:Null}\" ParentServiceID=\"{x:Null}\" ParentServiceName=\"{x:Null}\" ParentWorkflowInstanceId=\"{x:Null}\" ResultTransformation=\"{x:Null}\" ResultValidationExpression=\"{x:Null}\" ResultValidationRequiredTags=\"{x:Null}\" ScenarioID=\"{x:Null}\" ScopingObject=\"{x:Null}\" ServiceUri=\"{x:Null}\" SimulationOutput=\"{x:Null}\" Tags=\"{x:Null}\" AddMode=\"False\" DatabindRecursive=\"False\" DeferExecution=\"False\" DisplayName=\"PBI 6690 - TEST Inputs\" EnvironmentID=\"00000000-0000-0000-0000-000000000000\" FriendlySourceName=\"localhost\" HasError=\"[HasError]\" sap:VirtualizedContainerService.HintSize=\"250,84\" IconPath=\"pack://application:,,,/Warewolf Studio;component/images/Workflow-32.png\" InputMapping=\"&amp;lt;Inputs&amp;gt;&amp;lt;Input Name=&amp;quot;n1&amp;quot; Source=&amp;quot;&amp;quot; /&amp;gt;&amp;lt;Input Name=&amp;quot;n2&amp;quot; Source=&amp;quot;&amp;quot; /&amp;gt;&amp;lt;Input Name=&amp;quot;n3&amp;quot; Source=&amp;quot;&amp;quot; /&amp;gt;&amp;lt;/Inputs&amp;gt;\" InstructionList=\"[InstructionList]\" IsSimulationEnabled=\"False\" IsUIStep=\"False\" IsValid=\"[IsValid]\" IsWorkflow=\"True\" OnResumeClearAmbientDataList=\"False\" OnResumeClearTags=\"FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage\" OutputMapping=\"&amp;lt;Outputs&amp;gt;&amp;lt;Output Name=&amp;quot;result&amp;quot; MapsTo=&amp;quot;result&amp;quot; Value=&amp;quot;&amp;quot; /&amp;gt;&amp;lt;/Outputs&amp;gt;\" RemoveInputFromOutput=\"False\" ServiceName=\"PBI 6690 - TEST Inputs\" ServiceServer=\"00000000-0000-0000-0000-000000000000\" SimulationMode=\"OnDemand\" ToolboxFriendlyName=\"PBI 6690 - TEST Inputs\" Type=\"Workflow\" UniqueID=\"edadb62e-83f4-44bf-a260-7639d6b43169\"&gt;&lt;uaba:DsfActivity.AmbientDataList&gt;&lt;InOutArgument x:TypeArguments=\"scg:List(x:String)\" /&gt;&lt;/uaba:DsfActivity.AmbientDataList&gt;&lt;uaba:DsfActivity.HelpLink&gt;&lt;InArgument x:TypeArguments=\"x:String\"&gt;&lt;Literal x:TypeArguments=\"x:String\" Value=\"\" /&gt;&lt;/InArgument&gt;&lt;/uaba:DsfActivity.HelpLink&gt;&lt;uaba:DsfActivity.ParentInstanceID&gt;&lt;InOutArgument x:TypeArguments=\"x:String\" /&gt;&lt;/uaba:DsfActivity.ParentInstanceID&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=\"x:String, x:Object\"&gt;&lt;x:Boolean x:Key=\"IsExpanded\"&gt;True&lt;/x:Boolean&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;/uaba:DsfActivity&gt;&lt;/FlowStep&gt;&lt;FlowStep&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=\"x:String, x:Object\"&gt;&lt;av:Point x:Key=\"ShapeLocation\"&gt;145,390&lt;/av:Point&gt;&lt;av:Size x:Key=\"ShapeSize\"&gt;250,100&lt;/av:Size&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;uaba:DsfActivity ActivityStateData=\"{x:Null}\" AuthorRoles=\"{x:Null}\" Category=\"{x:Null}\" Compiler=\"{x:Null}\" CurrentResult=\"{x:Null}\" DataObject=\"{x:Null}\" DataTags=\"{x:Null}\" ExplicitDataList=\"{x:Null}\" HelpLink=\"{x:Null}\" InputTransformation=\"{x:Null}\" OnResumeKeepList=\"{x:Null}\" ParentServiceID=\"{x:Null}\" ParentServiceName=\"{x:Null}\" ParentWorkflowInstanceId=\"{x:Null}\" ResultTransformation=\"{x:Null}\" ResultValidationExpression=\"{x:Null}\" ResultValidationRequiredTags=\"{x:Null}\" ScenarioID=\"{x:Null}\" ScopingObject=\"{x:Null}\" ServiceUri=\"{x:Null}\" SimulationOutput=\"{x:Null}\" Tags=\"{x:Null}\" ActionName=\"dbo.spGetCountries\" AddMode=\"False\" DatabindRecursive=\"False\" DeferExecution=\"False\" DisplayName=\"Countries\" EnvironmentID=\"00000000-0000-0000-0000-000000000000\" FriendlySourceName=\"CitiesDb\" HasError=\"[HasError]\" sap:VirtualizedContainerService.HintSize=\"250,100\" IconPath=\"[Nothing]\" InputMapping=\"&amp;lt;Inputs&amp;gt;&amp;lt;Input Name=&amp;quot;Prefix&amp;quot; Source=&amp;quot;Prefix&amp;quot; /&amp;gt;&amp;lt;/Inputs&amp;gt;\" InstructionList=\"[InstructionList]\" IsSimulationEnabled=\"False\" IsUIStep=\"False\" IsValid=\"[IsValid]\" IsWorkflow=\"False\" OnResumeClearAmbientDataList=\"False\" OnResumeClearTags=\"FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage\" OutputMapping=\"&amp;lt;Outputs&amp;gt;&amp;lt;Output Name=&amp;quot;CountryID&amp;quot; MapsTo=&amp;quot;CountryID&amp;quot; Value=&amp;quot;[[GetCountries().CountryID]]&amp;quot; Recordset=&amp;quot;GetCountries&amp;quot; /&amp;gt;&amp;lt;Output Name=&amp;quot;Description&amp;quot; MapsTo=&amp;quot;Name&amp;quot; Value=&amp;quot;[[GetCountries().Name]]&amp;quot; Recordset=&amp;quot;GetCountries&amp;quot; /&amp;gt;&amp;lt;/Outputs&amp;gt;\" RemoveInputFromOutput=\"False\" ServiceName=\"Countries\" ServiceServer=\"00000000-0000-0000-0000-000000000000\" SimulationMode=\"OnDemand\" ToolboxFriendlyName=\"Countries\" Type=\"InvokeStoredProc\" UniqueID=\"33476822-9519-4ad5-8fee-1ae3b571115b\"&gt;&lt;uaba:DsfActivity.AmbientDataList&gt;&lt;InOutArgument x:TypeArguments=\"scg:List(x:String)\" /&gt;&lt;/uaba:DsfActivity.AmbientDataList&gt;&lt;uaba:DsfActivity.ParentInstanceID&gt;&lt;InOutArgument x:TypeArguments=\"x:String\" /&gt;&lt;/uaba:DsfActivity.ParentInstanceID&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=\"x:String, x:Object\"&gt;&lt;x:Boolean x:Key=\"IsExpanded\"&gt;True&lt;/x:Boolean&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;/uaba:DsfActivity&gt;&lt;/FlowStep&gt;&lt;/Flowchart&gt;&lt;/Activity&gt;</XamlDefinition>";
            resourceModelMock.SetupGet(model => model.WorkflowXaml).Returns(new StringBuilder(WorkFlowXaml));
            resourceModelMock.SetupGet(model => model.Environment.EnvironmentID).Returns(Guid.NewGuid);
            resourceModelMock.SetupGet(model => model.ID).Returns(Guid.NewGuid);
            resourceModelMock.SetupGet(model => model.ServerID).Returns(Guid.NewGuid);

            var mock = new Mock<IServerRepository>();
            mock.Setup(repository => repository.ActiveServer).Returns(environmentModel.Object);
            mock.Setup(repository => repository.All()).Returns(new List<IServer>() { environmentModel.Object });
            CustomContainer.Register(mock.Object);
            var worksurfaceContextManager = new WorksurfaceContextManager(false, _shellViewModel, mock.Object, new Mock<IViewFactory>().Object);

            _activeEnvironment.Setup(model => model.Name).Returns("localhost");
            //---------------Execute Test ----------------------
            worksurfaceContextManager.DisplayResourceWizard(resourceModelMock.Object);

            var activetx = _shellViewModel.Items.ToList().First(i => i.WorkSurfaceViewModel.GetType().Name == "SourceViewModel`1");

            var vm = new WorkSurfaceContextViewModel(new EventAggregator(), new WorkSurfaceKey(), activetx.WorkSurfaceViewModel, new Mock<Common.Interfaces.Studio.Controller.IPopupController>().Object, (a, b, c) => { });
            var sourceVM = vm.WorkSurfaceViewModel as SourceViewModel<IWebServiceSource>;
            sourceVM.ViewModel.Item = new Mock<IWebServiceSource>().Object;
            mvm.Items.Add(vm);
            Assert.IsFalse(mvm.OnStudioClosing());
        }

        [TestMethod]
        [TestCategory("MainViewModel_OnStudioClosing")]
        [Owner("Pieter Terblanche")]
        public void MainViewModel_OnStudioClosing_CallsComPluginSourceOnClosing()
        {
            InitSourceViewModel(out Mock<IServer> environmentModel, out ShellViewModel mvm);
            var resourceModelMock = new Mock<IContextualResourceModel>();
            resourceModelMock.SetupGet(model => model.ServerResourceType).Returns("ComPluginSource");
            const string WorkFlowXaml = "<XamlDefinition>&lt;Activity x:Class=\"PBI 6690 - TEST\" xmlns=\"http://schemas.microsoft.com/netfx/2009/xaml/activities\" xmlns:av=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" xmlns:dc=\"clr-namespace:Dev2.Common;assembly=Dev2.CommonDataUtils\" xmlns:ddc=\"clr-namespace:Dev2.DataList.Contract;assembly=Dev2.Data\" xmlns:ddcb=\"clr-namespace:Dev2.DataList.Contract.Binary_Objects;assembly=Dev2.Data\" xmlns:ddd=\"clr-namespace:Dev2.Data.Decision;assembly=Dev2.Data\" xmlns:dddo=\"clr-namespace:Dev2.Data.Decisions.Operations;assembly=Dev2.Data\" xmlns:ddsm=\"clr-namespace:Dev2.Data.SystemTemplates.Models;assembly=Dev2.Data\" xmlns:mva=\"clr-namespace:Microsoft.VisualBasic.Activities;assembly=System.Activities\" xmlns:s=\"clr-namespace:System;assembly=mscorlib\" xmlns:sap=\"http://schemas.microsoft.com/netfx/2009/xaml/activities/presentation\" xmlns:scg=\"clr-namespace:System.Collections.Generic;assembly=mscorlib\" xmlns:sco=\"clr-namespace:System.Collections.ObjectModel;assembly=mscorlib\" xmlns:uaba=\"clr-namespace:Unlimited.Applications.BusinessDesignStudio.Activities;assembly=Dev2.Activities\" xmlns:uf=\"clr-namespace:Unlimited.Framework;assembly=Dev2.Core\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"&gt;&lt;x:Members&gt;&lt;x:Property Name=\"AmbientDataList\" Type=\"InOutArgument(scg:List(x:String))\" /&gt;&lt;x:Property Name=\"ParentWorkflowInstanceId\" Type=\"InOutArgument(s:Guid)\" /&gt;&lt;x:Property Name=\"ParentServiceName\" Type=\"InOutArgument(x:String)\" /&gt;&lt;/x:Members&gt;&lt;sap:VirtualizedContainerService.HintSize&gt;654,676&lt;/sap:VirtualizedContainerService.HintSize&gt;&lt;mva:VisualBasic.Settings&gt;Assembly references and imported namespaces serialized as XML namespaces&lt;/mva:VisualBasic.Settings&gt;&lt;TextExpression.NamespacesForImplementation&gt;&lt;scg:List x:TypeArguments=\"x:String\" Capacity=\"7\"&gt;&lt;x:String&gt;Dev2.CommonDataUtils&lt;/x:String&gt;&lt;x:String&gt;Dev2.Data.Decisions.Operations&lt;/x:String&gt;&lt;x:String&gt;Dev2.Data.SystemTemplates.Models&lt;/x:String&gt;&lt;x:String&gt;Dev2.DataList.Contract&lt;/x:String&gt;&lt;x:String&gt;Dev2.DataList.Contract.Binary_Objects&lt;/x:String&gt;&lt;x:String&gt;Unlimited.Framework&lt;/x:String&gt;&lt;x:String&gt;Unlimited.Applications.BusinessDesignStudio.Activities&lt;/x:String&gt;&lt;/scg:List&gt;&lt;/TextExpression.NamespacesForImplementation&gt;&lt;TextExpression.ReferencesForImplementation&gt;&lt;sco:Collection x:TypeArguments=\"AssemblyReference\"&gt;&lt;AssemblyReference&gt;Dev2.CommonDataUtils&lt;/AssemblyReference&gt;&lt;AssemblyReference&gt;Dev2.Data&lt;/AssemblyReference&gt;&lt;AssemblyReference&gt;Dev2.Core&lt;/AssemblyReference&gt;&lt;AssemblyReference&gt;Dev2.Activities&lt;/AssemblyReference&gt;&lt;/sco:Collection&gt;&lt;/TextExpression.ReferencesForImplementation&gt;&lt;Flowchart DisplayName=\"PBI 6690 - TEST\" sap:VirtualizedContainerService.HintSize=\"614,636\"&gt;&lt;Flowchart.Variables&gt;&lt;Variable x:TypeArguments=\"scg:List(x:String)\" Name=\"InstructionList\" /&gt;&lt;Variable x:TypeArguments=\"x:String\" Name=\"LastResult\" /&gt;&lt;Variable x:TypeArguments=\"x:Boolean\" Name=\"HasError\" /&gt;&lt;Variable x:TypeArguments=\"x:String\" Name=\"ExplicitDataList\" /&gt;&lt;Variable x:TypeArguments=\"x:Boolean\" Name=\"IsValid\" /&gt;&lt;Variable x:TypeArguments=\"uf:UnlimitedObject\" Name=\"d\" /&gt;&lt;Variable x:TypeArguments=\"uaba:Util\" Name=\"t\" /&gt;&lt;Variable x:TypeArguments=\"ddd:Dev2DataListDecisionHandler\" Name=\"Dev2DecisionHandler\" /&gt;&lt;/Flowchart.Variables&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=\"x:String, x:Object\"&gt;&lt;x:Boolean x:Key=\"IsExpanded\"&gt;False&lt;/x:Boolean&gt;&lt;av:Point x:Key=\"ShapeLocation\"&gt;270,2.5&lt;/av:Point&gt;&lt;av:Size x:Key=\"ShapeSize\"&gt;60,75&lt;/av:Size&gt;&lt;av:PointCollection x:Key=\"ConnectorLocation\"&gt;300,77.5 300,107.5 310,107.5 310,211&lt;/av:PointCollection&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;Flowchart.StartNode&gt;&lt;x:Reference&gt;__ReferenceID0&lt;/x:Reference&gt;&lt;/Flowchart.StartNode&gt;&lt;FlowStep x:Name=\"__ReferenceID0\"&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=\"x:String, x:Object\"&gt;&lt;av:Point x:Key=\"ShapeLocation\"&gt;185,211&lt;/av:Point&gt;&lt;av:Size x:Key=\"ShapeSize\"&gt;250,84&lt;/av:Size&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;uaba:DsfActivity ActionName=\"{x:Null}\" ActivityStateData=\"{x:Null}\" AuthorRoles=\"{x:Null}\" Category=\"{x:Null}\" Compiler=\"{x:Null}\" CurrentResult=\"{x:Null}\" DataObject=\"{x:Null}\" DataTags=\"{x:Null}\" ExplicitDataList=\"{x:Null}\" InputTransformation=\"{x:Null}\" OnResumeKeepList=\"{x:Null}\" ParentServiceID=\"{x:Null}\" ParentServiceName=\"{x:Null}\" ParentWorkflowInstanceId=\"{x:Null}\" ResultTransformation=\"{x:Null}\" ResultValidationExpression=\"{x:Null}\" ResultValidationRequiredTags=\"{x:Null}\" ScenarioID=\"{x:Null}\" ScopingObject=\"{x:Null}\" ServiceUri=\"{x:Null}\" SimulationOutput=\"{x:Null}\" Tags=\"{x:Null}\" AddMode=\"False\" DatabindRecursive=\"False\" DeferExecution=\"False\" DisplayName=\"PBI 6690 - TEST Inputs\" EnvironmentID=\"00000000-0000-0000-0000-000000000000\" FriendlySourceName=\"localhost\" HasError=\"[HasError]\" sap:VirtualizedContainerService.HintSize=\"250,84\" IconPath=\"pack://application:,,,/Warewolf Studio;component/images/Workflow-32.png\" InputMapping=\"&amp;lt;Inputs&amp;gt;&amp;lt;Input Name=&amp;quot;n1&amp;quot; Source=&amp;quot;&amp;quot; /&amp;gt;&amp;lt;Input Name=&amp;quot;n2&amp;quot; Source=&amp;quot;&amp;quot; /&amp;gt;&amp;lt;Input Name=&amp;quot;n3&amp;quot; Source=&amp;quot;&amp;quot; /&amp;gt;&amp;lt;/Inputs&amp;gt;\" InstructionList=\"[InstructionList]\" IsSimulationEnabled=\"False\" IsUIStep=\"False\" IsValid=\"[IsValid]\" IsWorkflow=\"True\" OnResumeClearAmbientDataList=\"False\" OnResumeClearTags=\"FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage\" OutputMapping=\"&amp;lt;Outputs&amp;gt;&amp;lt;Output Name=&amp;quot;result&amp;quot; MapsTo=&amp;quot;result&amp;quot; Value=&amp;quot;&amp;quot; /&amp;gt;&amp;lt;/Outputs&amp;gt;\" RemoveInputFromOutput=\"False\" ServiceName=\"PBI 6690 - TEST Inputs\" ServiceServer=\"00000000-0000-0000-0000-000000000000\" SimulationMode=\"OnDemand\" ToolboxFriendlyName=\"PBI 6690 - TEST Inputs\" Type=\"Workflow\" UniqueID=\"edadb62e-83f4-44bf-a260-7639d6b43169\"&gt;&lt;uaba:DsfActivity.AmbientDataList&gt;&lt;InOutArgument x:TypeArguments=\"scg:List(x:String)\" /&gt;&lt;/uaba:DsfActivity.AmbientDataList&gt;&lt;uaba:DsfActivity.HelpLink&gt;&lt;InArgument x:TypeArguments=\"x:String\"&gt;&lt;Literal x:TypeArguments=\"x:String\" Value=\"\" /&gt;&lt;/InArgument&gt;&lt;/uaba:DsfActivity.HelpLink&gt;&lt;uaba:DsfActivity.ParentInstanceID&gt;&lt;InOutArgument x:TypeArguments=\"x:String\" /&gt;&lt;/uaba:DsfActivity.ParentInstanceID&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=\"x:String, x:Object\"&gt;&lt;x:Boolean x:Key=\"IsExpanded\"&gt;True&lt;/x:Boolean&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;/uaba:DsfActivity&gt;&lt;/FlowStep&gt;&lt;FlowStep&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=\"x:String, x:Object\"&gt;&lt;av:Point x:Key=\"ShapeLocation\"&gt;145,390&lt;/av:Point&gt;&lt;av:Size x:Key=\"ShapeSize\"&gt;250,100&lt;/av:Size&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;uaba:DsfActivity ActivityStateData=\"{x:Null}\" AuthorRoles=\"{x:Null}\" Category=\"{x:Null}\" Compiler=\"{x:Null}\" CurrentResult=\"{x:Null}\" DataObject=\"{x:Null}\" DataTags=\"{x:Null}\" ExplicitDataList=\"{x:Null}\" HelpLink=\"{x:Null}\" InputTransformation=\"{x:Null}\" OnResumeKeepList=\"{x:Null}\" ParentServiceID=\"{x:Null}\" ParentServiceName=\"{x:Null}\" ParentWorkflowInstanceId=\"{x:Null}\" ResultTransformation=\"{x:Null}\" ResultValidationExpression=\"{x:Null}\" ResultValidationRequiredTags=\"{x:Null}\" ScenarioID=\"{x:Null}\" ScopingObject=\"{x:Null}\" ServiceUri=\"{x:Null}\" SimulationOutput=\"{x:Null}\" Tags=\"{x:Null}\" ActionName=\"dbo.spGetCountries\" AddMode=\"False\" DatabindRecursive=\"False\" DeferExecution=\"False\" DisplayName=\"Countries\" EnvironmentID=\"00000000-0000-0000-0000-000000000000\" FriendlySourceName=\"CitiesDb\" HasError=\"[HasError]\" sap:VirtualizedContainerService.HintSize=\"250,100\" IconPath=\"[Nothing]\" InputMapping=\"&amp;lt;Inputs&amp;gt;&amp;lt;Input Name=&amp;quot;Prefix&amp;quot; Source=&amp;quot;Prefix&amp;quot; /&amp;gt;&amp;lt;/Inputs&amp;gt;\" InstructionList=\"[InstructionList]\" IsSimulationEnabled=\"False\" IsUIStep=\"False\" IsValid=\"[IsValid]\" IsWorkflow=\"False\" OnResumeClearAmbientDataList=\"False\" OnResumeClearTags=\"FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage\" OutputMapping=\"&amp;lt;Outputs&amp;gt;&amp;lt;Output Name=&amp;quot;CountryID&amp;quot; MapsTo=&amp;quot;CountryID&amp;quot; Value=&amp;quot;[[GetCountries().CountryID]]&amp;quot; Recordset=&amp;quot;GetCountries&amp;quot; /&amp;gt;&amp;lt;Output Name=&amp;quot;Description&amp;quot; MapsTo=&amp;quot;Name&amp;quot; Value=&amp;quot;[[GetCountries().Name]]&amp;quot; Recordset=&amp;quot;GetCountries&amp;quot; /&amp;gt;&amp;lt;/Outputs&amp;gt;\" RemoveInputFromOutput=\"False\" ServiceName=\"Countries\" ServiceServer=\"00000000-0000-0000-0000-000000000000\" SimulationMode=\"OnDemand\" ToolboxFriendlyName=\"Countries\" Type=\"InvokeStoredProc\" UniqueID=\"33476822-9519-4ad5-8fee-1ae3b571115b\"&gt;&lt;uaba:DsfActivity.AmbientDataList&gt;&lt;InOutArgument x:TypeArguments=\"scg:List(x:String)\" /&gt;&lt;/uaba:DsfActivity.AmbientDataList&gt;&lt;uaba:DsfActivity.ParentInstanceID&gt;&lt;InOutArgument x:TypeArguments=\"x:String\" /&gt;&lt;/uaba:DsfActivity.ParentInstanceID&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=\"x:String, x:Object\"&gt;&lt;x:Boolean x:Key=\"IsExpanded\"&gt;True&lt;/x:Boolean&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;/uaba:DsfActivity&gt;&lt;/FlowStep&gt;&lt;/Flowchart&gt;&lt;/Activity&gt;</XamlDefinition>";
            resourceModelMock.SetupGet(model => model.WorkflowXaml).Returns(new StringBuilder(WorkFlowXaml));
            resourceModelMock.SetupGet(model => model.Environment.EnvironmentID).Returns(Guid.NewGuid);
            resourceModelMock.SetupGet(model => model.ID).Returns(Guid.NewGuid);
            resourceModelMock.SetupGet(model => model.ServerID).Returns(Guid.NewGuid);
            var worksurfaceContextManager = new WorksurfaceContextManager(false, _shellViewModel, new Mock<IServerRepository>().Object, new Mock<IViewFactory>().Object);

            _activeEnvironment.Setup(model => model.Name).Returns("localhost");
            //---------------Execute Test ----------------------
            worksurfaceContextManager.DisplayResourceWizard(resourceModelMock.Object);

            ServerRepository.Instance.ActiveServer = environmentModel.Object;
            var activetx = _shellViewModel.Items.ToList().First(i => i.WorkSurfaceViewModel.GetType().Name == "SourceViewModel`1");

            var vm = new WorkSurfaceContextViewModel(new EventAggregator(), new WorkSurfaceKey(), activetx.WorkSurfaceViewModel, new Mock<Common.Interfaces.Studio.Controller.IPopupController>().Object, (a, b, c) => { });
            var sourceVM = vm.WorkSurfaceViewModel as SourceViewModel<IComPluginSource>;
            sourceVM.ViewModel.Item = new Mock<IComPluginSource>().Object;
            mvm.Items.Add(vm);
            Assert.IsFalse(mvm.OnStudioClosing());
        }

        [TestMethod]
        [TestCategory("MainViewModel_OnStudioClosing")]
        [Owner("Pieter Terblanche")]
        public void MainViewModel_OnStudioClosing_CallsPluginSourceOnClosing()
        {
            InitSourceViewModel(out Mock<IServer> environmentModel, out ShellViewModel mvm);
            var resourceModelMock = new Mock<IContextualResourceModel>();
            resourceModelMock.SetupGet(model => model.ServerResourceType).Returns("PluginSource");
            const string WorkFlowXaml = "<XamlDefinition>&lt;Activity x:Class=\"PBI 6690 - TEST\" xmlns=\"http://schemas.microsoft.com/netfx/2009/xaml/activities\" xmlns:av=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" xmlns:dc=\"clr-namespace:Dev2.Common;assembly=Dev2.CommonDataUtils\" xmlns:ddc=\"clr-namespace:Dev2.DataList.Contract;assembly=Dev2.Data\" xmlns:ddcb=\"clr-namespace:Dev2.DataList.Contract.Binary_Objects;assembly=Dev2.Data\" xmlns:ddd=\"clr-namespace:Dev2.Data.Decision;assembly=Dev2.Data\" xmlns:dddo=\"clr-namespace:Dev2.Data.Decisions.Operations;assembly=Dev2.Data\" xmlns:ddsm=\"clr-namespace:Dev2.Data.SystemTemplates.Models;assembly=Dev2.Data\" xmlns:mva=\"clr-namespace:Microsoft.VisualBasic.Activities;assembly=System.Activities\" xmlns:s=\"clr-namespace:System;assembly=mscorlib\" xmlns:sap=\"http://schemas.microsoft.com/netfx/2009/xaml/activities/presentation\" xmlns:scg=\"clr-namespace:System.Collections.Generic;assembly=mscorlib\" xmlns:sco=\"clr-namespace:System.Collections.ObjectModel;assembly=mscorlib\" xmlns:uaba=\"clr-namespace:Unlimited.Applications.BusinessDesignStudio.Activities;assembly=Dev2.Activities\" xmlns:uf=\"clr-namespace:Unlimited.Framework;assembly=Dev2.Core\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"&gt;&lt;x:Members&gt;&lt;x:Property Name=\"AmbientDataList\" Type=\"InOutArgument(scg:List(x:String))\" /&gt;&lt;x:Property Name=\"ParentWorkflowInstanceId\" Type=\"InOutArgument(s:Guid)\" /&gt;&lt;x:Property Name=\"ParentServiceName\" Type=\"InOutArgument(x:String)\" /&gt;&lt;/x:Members&gt;&lt;sap:VirtualizedContainerService.HintSize&gt;654,676&lt;/sap:VirtualizedContainerService.HintSize&gt;&lt;mva:VisualBasic.Settings&gt;Assembly references and imported namespaces serialized as XML namespaces&lt;/mva:VisualBasic.Settings&gt;&lt;TextExpression.NamespacesForImplementation&gt;&lt;scg:List x:TypeArguments=\"x:String\" Capacity=\"7\"&gt;&lt;x:String&gt;Dev2.CommonDataUtils&lt;/x:String&gt;&lt;x:String&gt;Dev2.Data.Decisions.Operations&lt;/x:String&gt;&lt;x:String&gt;Dev2.Data.SystemTemplates.Models&lt;/x:String&gt;&lt;x:String&gt;Dev2.DataList.Contract&lt;/x:String&gt;&lt;x:String&gt;Dev2.DataList.Contract.Binary_Objects&lt;/x:String&gt;&lt;x:String&gt;Unlimited.Framework&lt;/x:String&gt;&lt;x:String&gt;Unlimited.Applications.BusinessDesignStudio.Activities&lt;/x:String&gt;&lt;/scg:List&gt;&lt;/TextExpression.NamespacesForImplementation&gt;&lt;TextExpression.ReferencesForImplementation&gt;&lt;sco:Collection x:TypeArguments=\"AssemblyReference\"&gt;&lt;AssemblyReference&gt;Dev2.CommonDataUtils&lt;/AssemblyReference&gt;&lt;AssemblyReference&gt;Dev2.Data&lt;/AssemblyReference&gt;&lt;AssemblyReference&gt;Dev2.Core&lt;/AssemblyReference&gt;&lt;AssemblyReference&gt;Dev2.Activities&lt;/AssemblyReference&gt;&lt;/sco:Collection&gt;&lt;/TextExpression.ReferencesForImplementation&gt;&lt;Flowchart DisplayName=\"PBI 6690 - TEST\" sap:VirtualizedContainerService.HintSize=\"614,636\"&gt;&lt;Flowchart.Variables&gt;&lt;Variable x:TypeArguments=\"scg:List(x:String)\" Name=\"InstructionList\" /&gt;&lt;Variable x:TypeArguments=\"x:String\" Name=\"LastResult\" /&gt;&lt;Variable x:TypeArguments=\"x:Boolean\" Name=\"HasError\" /&gt;&lt;Variable x:TypeArguments=\"x:String\" Name=\"ExplicitDataList\" /&gt;&lt;Variable x:TypeArguments=\"x:Boolean\" Name=\"IsValid\" /&gt;&lt;Variable x:TypeArguments=\"uf:UnlimitedObject\" Name=\"d\" /&gt;&lt;Variable x:TypeArguments=\"uaba:Util\" Name=\"t\" /&gt;&lt;Variable x:TypeArguments=\"ddd:Dev2DataListDecisionHandler\" Name=\"Dev2DecisionHandler\" /&gt;&lt;/Flowchart.Variables&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=\"x:String, x:Object\"&gt;&lt;x:Boolean x:Key=\"IsExpanded\"&gt;False&lt;/x:Boolean&gt;&lt;av:Point x:Key=\"ShapeLocation\"&gt;270,2.5&lt;/av:Point&gt;&lt;av:Size x:Key=\"ShapeSize\"&gt;60,75&lt;/av:Size&gt;&lt;av:PointCollection x:Key=\"ConnectorLocation\"&gt;300,77.5 300,107.5 310,107.5 310,211&lt;/av:PointCollection&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;Flowchart.StartNode&gt;&lt;x:Reference&gt;__ReferenceID0&lt;/x:Reference&gt;&lt;/Flowchart.StartNode&gt;&lt;FlowStep x:Name=\"__ReferenceID0\"&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=\"x:String, x:Object\"&gt;&lt;av:Point x:Key=\"ShapeLocation\"&gt;185,211&lt;/av:Point&gt;&lt;av:Size x:Key=\"ShapeSize\"&gt;250,84&lt;/av:Size&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;uaba:DsfActivity ActionName=\"{x:Null}\" ActivityStateData=\"{x:Null}\" AuthorRoles=\"{x:Null}\" Category=\"{x:Null}\" Compiler=\"{x:Null}\" CurrentResult=\"{x:Null}\" DataObject=\"{x:Null}\" DataTags=\"{x:Null}\" ExplicitDataList=\"{x:Null}\" InputTransformation=\"{x:Null}\" OnResumeKeepList=\"{x:Null}\" ParentServiceID=\"{x:Null}\" ParentServiceName=\"{x:Null}\" ParentWorkflowInstanceId=\"{x:Null}\" ResultTransformation=\"{x:Null}\" ResultValidationExpression=\"{x:Null}\" ResultValidationRequiredTags=\"{x:Null}\" ScenarioID=\"{x:Null}\" ScopingObject=\"{x:Null}\" ServiceUri=\"{x:Null}\" SimulationOutput=\"{x:Null}\" Tags=\"{x:Null}\" AddMode=\"False\" DatabindRecursive=\"False\" DeferExecution=\"False\" DisplayName=\"PBI 6690 - TEST Inputs\" EnvironmentID=\"00000000-0000-0000-0000-000000000000\" FriendlySourceName=\"localhost\" HasError=\"[HasError]\" sap:VirtualizedContainerService.HintSize=\"250,84\" IconPath=\"pack://application:,,,/Warewolf Studio;component/images/Workflow-32.png\" InputMapping=\"&amp;lt;Inputs&amp;gt;&amp;lt;Input Name=&amp;quot;n1&amp;quot; Source=&amp;quot;&amp;quot; /&amp;gt;&amp;lt;Input Name=&amp;quot;n2&amp;quot; Source=&amp;quot;&amp;quot; /&amp;gt;&amp;lt;Input Name=&amp;quot;n3&amp;quot; Source=&amp;quot;&amp;quot; /&amp;gt;&amp;lt;/Inputs&amp;gt;\" InstructionList=\"[InstructionList]\" IsSimulationEnabled=\"False\" IsUIStep=\"False\" IsValid=\"[IsValid]\" IsWorkflow=\"True\" OnResumeClearAmbientDataList=\"False\" OnResumeClearTags=\"FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage\" OutputMapping=\"&amp;lt;Outputs&amp;gt;&amp;lt;Output Name=&amp;quot;result&amp;quot; MapsTo=&amp;quot;result&amp;quot; Value=&amp;quot;&amp;quot; /&amp;gt;&amp;lt;/Outputs&amp;gt;\" RemoveInputFromOutput=\"False\" ServiceName=\"PBI 6690 - TEST Inputs\" ServiceServer=\"00000000-0000-0000-0000-000000000000\" SimulationMode=\"OnDemand\" ToolboxFriendlyName=\"PBI 6690 - TEST Inputs\" Type=\"Workflow\" UniqueID=\"edadb62e-83f4-44bf-a260-7639d6b43169\"&gt;&lt;uaba:DsfActivity.AmbientDataList&gt;&lt;InOutArgument x:TypeArguments=\"scg:List(x:String)\" /&gt;&lt;/uaba:DsfActivity.AmbientDataList&gt;&lt;uaba:DsfActivity.HelpLink&gt;&lt;InArgument x:TypeArguments=\"x:String\"&gt;&lt;Literal x:TypeArguments=\"x:String\" Value=\"\" /&gt;&lt;/InArgument&gt;&lt;/uaba:DsfActivity.HelpLink&gt;&lt;uaba:DsfActivity.ParentInstanceID&gt;&lt;InOutArgument x:TypeArguments=\"x:String\" /&gt;&lt;/uaba:DsfActivity.ParentInstanceID&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=\"x:String, x:Object\"&gt;&lt;x:Boolean x:Key=\"IsExpanded\"&gt;True&lt;/x:Boolean&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;/uaba:DsfActivity&gt;&lt;/FlowStep&gt;&lt;FlowStep&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=\"x:String, x:Object\"&gt;&lt;av:Point x:Key=\"ShapeLocation\"&gt;145,390&lt;/av:Point&gt;&lt;av:Size x:Key=\"ShapeSize\"&gt;250,100&lt;/av:Size&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;uaba:DsfActivity ActivityStateData=\"{x:Null}\" AuthorRoles=\"{x:Null}\" Category=\"{x:Null}\" Compiler=\"{x:Null}\" CurrentResult=\"{x:Null}\" DataObject=\"{x:Null}\" DataTags=\"{x:Null}\" ExplicitDataList=\"{x:Null}\" HelpLink=\"{x:Null}\" InputTransformation=\"{x:Null}\" OnResumeKeepList=\"{x:Null}\" ParentServiceID=\"{x:Null}\" ParentServiceName=\"{x:Null}\" ParentWorkflowInstanceId=\"{x:Null}\" ResultTransformation=\"{x:Null}\" ResultValidationExpression=\"{x:Null}\" ResultValidationRequiredTags=\"{x:Null}\" ScenarioID=\"{x:Null}\" ScopingObject=\"{x:Null}\" ServiceUri=\"{x:Null}\" SimulationOutput=\"{x:Null}\" Tags=\"{x:Null}\" ActionName=\"dbo.spGetCountries\" AddMode=\"False\" DatabindRecursive=\"False\" DeferExecution=\"False\" DisplayName=\"Countries\" EnvironmentID=\"00000000-0000-0000-0000-000000000000\" FriendlySourceName=\"CitiesDb\" HasError=\"[HasError]\" sap:VirtualizedContainerService.HintSize=\"250,100\" IconPath=\"[Nothing]\" InputMapping=\"&amp;lt;Inputs&amp;gt;&amp;lt;Input Name=&amp;quot;Prefix&amp;quot; Source=&amp;quot;Prefix&amp;quot; /&amp;gt;&amp;lt;/Inputs&amp;gt;\" InstructionList=\"[InstructionList]\" IsSimulationEnabled=\"False\" IsUIStep=\"False\" IsValid=\"[IsValid]\" IsWorkflow=\"False\" OnResumeClearAmbientDataList=\"False\" OnResumeClearTags=\"FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage\" OutputMapping=\"&amp;lt;Outputs&amp;gt;&amp;lt;Output Name=&amp;quot;CountryID&amp;quot; MapsTo=&amp;quot;CountryID&amp;quot; Value=&amp;quot;[[GetCountries().CountryID]]&amp;quot; Recordset=&amp;quot;GetCountries&amp;quot; /&amp;gt;&amp;lt;Output Name=&amp;quot;Description&amp;quot; MapsTo=&amp;quot;Name&amp;quot; Value=&amp;quot;[[GetCountries().Name]]&amp;quot; Recordset=&amp;quot;GetCountries&amp;quot; /&amp;gt;&amp;lt;/Outputs&amp;gt;\" RemoveInputFromOutput=\"False\" ServiceName=\"Countries\" ServiceServer=\"00000000-0000-0000-0000-000000000000\" SimulationMode=\"OnDemand\" ToolboxFriendlyName=\"Countries\" Type=\"InvokeStoredProc\" UniqueID=\"33476822-9519-4ad5-8fee-1ae3b571115b\"&gt;&lt;uaba:DsfActivity.AmbientDataList&gt;&lt;InOutArgument x:TypeArguments=\"scg:List(x:String)\" /&gt;&lt;/uaba:DsfActivity.AmbientDataList&gt;&lt;uaba:DsfActivity.ParentInstanceID&gt;&lt;InOutArgument x:TypeArguments=\"x:String\" /&gt;&lt;/uaba:DsfActivity.ParentInstanceID&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=\"x:String, x:Object\"&gt;&lt;x:Boolean x:Key=\"IsExpanded\"&gt;True&lt;/x:Boolean&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;/uaba:DsfActivity&gt;&lt;/FlowStep&gt;&lt;/Flowchart&gt;&lt;/Activity&gt;</XamlDefinition>";
            resourceModelMock.SetupGet(model => model.WorkflowXaml).Returns(new StringBuilder(WorkFlowXaml));
            resourceModelMock.SetupGet(model => model.Environment.EnvironmentID).Returns(Guid.NewGuid);
            resourceModelMock.SetupGet(model => model.ID).Returns(Guid.NewGuid);
            resourceModelMock.SetupGet(model => model.ServerID).Returns(Guid.NewGuid);
            var mock = new Mock<IServerRepository>();
            mock.Setup(repository => repository.ActiveServer).Returns(environmentModel.Object);
            CustomContainer.Register(mock.Object);
            var worksurfaceContextManager = new WorksurfaceContextManager(false, _shellViewModel, mock.Object, new Mock<IViewFactory>().Object);

            _activeEnvironment.Setup(model => model.Name).Returns("localhost");
            //---------------Execute Test ----------------------
            worksurfaceContextManager.DisplayResourceWizard(resourceModelMock.Object);

            var activetx = _shellViewModel.Items.ToList().First(i => i.WorkSurfaceViewModel.GetType().Name == "SourceViewModel`1");

            var vm = new WorkSurfaceContextViewModel(new EventAggregator(), new WorkSurfaceKey(), activetx.WorkSurfaceViewModel, new Mock<Common.Interfaces.Studio.Controller.IPopupController>().Object, (a, b, c) => { });
            var sourceVM = vm.WorkSurfaceViewModel as SourceViewModel<IPluginSource>;
            sourceVM.ViewModel.Item = new Mock<IPluginSource>().Object;
            mvm.Items.Add(vm);
            Assert.IsFalse(mvm.OnStudioClosing());
        }

        [TestMethod]
        [TestCategory("MainViewModel_OnStudioClosing")]
        [Owner("Pieter Terblanche")]
        public void MainViewModel_OnStudioClosing_CallsExchangeSourceOnClosing()
        {
            InitSourceViewModel(out Mock<IServer> environmentModel, out ShellViewModel mvm);
            var resourceModelMock = new Mock<IContextualResourceModel>();
            resourceModelMock.SetupGet(model => model.ServerResourceType).Returns("ExchangeSource");
            const string WorkFlowXaml = "<XamlDefinition>&lt;Activity x:Class=\"PBI 6690 - TEST\" xmlns=\"http://schemas.microsoft.com/netfx/2009/xaml/activities\" xmlns:av=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" xmlns:dc=\"clr-namespace:Dev2.Common;assembly=Dev2.CommonDataUtils\" xmlns:ddc=\"clr-namespace:Dev2.DataList.Contract;assembly=Dev2.Data\" xmlns:ddcb=\"clr-namespace:Dev2.DataList.Contract.Binary_Objects;assembly=Dev2.Data\" xmlns:ddd=\"clr-namespace:Dev2.Data.Decision;assembly=Dev2.Data\" xmlns:dddo=\"clr-namespace:Dev2.Data.Decisions.Operations;assembly=Dev2.Data\" xmlns:ddsm=\"clr-namespace:Dev2.Data.SystemTemplates.Models;assembly=Dev2.Data\" xmlns:mva=\"clr-namespace:Microsoft.VisualBasic.Activities;assembly=System.Activities\" xmlns:s=\"clr-namespace:System;assembly=mscorlib\" xmlns:sap=\"http://schemas.microsoft.com/netfx/2009/xaml/activities/presentation\" xmlns:scg=\"clr-namespace:System.Collections.Generic;assembly=mscorlib\" xmlns:sco=\"clr-namespace:System.Collections.ObjectModel;assembly=mscorlib\" xmlns:uaba=\"clr-namespace:Unlimited.Applications.BusinessDesignStudio.Activities;assembly=Dev2.Activities\" xmlns:uf=\"clr-namespace:Unlimited.Framework;assembly=Dev2.Core\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"&gt;&lt;x:Members&gt;&lt;x:Property Name=\"AmbientDataList\" Type=\"InOutArgument(scg:List(x:String))\" /&gt;&lt;x:Property Name=\"ParentWorkflowInstanceId\" Type=\"InOutArgument(s:Guid)\" /&gt;&lt;x:Property Name=\"ParentServiceName\" Type=\"InOutArgument(x:String)\" /&gt;&lt;/x:Members&gt;&lt;sap:VirtualizedContainerService.HintSize&gt;654,676&lt;/sap:VirtualizedContainerService.HintSize&gt;&lt;mva:VisualBasic.Settings&gt;Assembly references and imported namespaces serialized as XML namespaces&lt;/mva:VisualBasic.Settings&gt;&lt;TextExpression.NamespacesForImplementation&gt;&lt;scg:List x:TypeArguments=\"x:String\" Capacity=\"7\"&gt;&lt;x:String&gt;Dev2.CommonDataUtils&lt;/x:String&gt;&lt;x:String&gt;Dev2.Data.Decisions.Operations&lt;/x:String&gt;&lt;x:String&gt;Dev2.Data.SystemTemplates.Models&lt;/x:String&gt;&lt;x:String&gt;Dev2.DataList.Contract&lt;/x:String&gt;&lt;x:String&gt;Dev2.DataList.Contract.Binary_Objects&lt;/x:String&gt;&lt;x:String&gt;Unlimited.Framework&lt;/x:String&gt;&lt;x:String&gt;Unlimited.Applications.BusinessDesignStudio.Activities&lt;/x:String&gt;&lt;/scg:List&gt;&lt;/TextExpression.NamespacesForImplementation&gt;&lt;TextExpression.ReferencesForImplementation&gt;&lt;sco:Collection x:TypeArguments=\"AssemblyReference\"&gt;&lt;AssemblyReference&gt;Dev2.CommonDataUtils&lt;/AssemblyReference&gt;&lt;AssemblyReference&gt;Dev2.Data&lt;/AssemblyReference&gt;&lt;AssemblyReference&gt;Dev2.Core&lt;/AssemblyReference&gt;&lt;AssemblyReference&gt;Dev2.Activities&lt;/AssemblyReference&gt;&lt;/sco:Collection&gt;&lt;/TextExpression.ReferencesForImplementation&gt;&lt;Flowchart DisplayName=\"PBI 6690 - TEST\" sap:VirtualizedContainerService.HintSize=\"614,636\"&gt;&lt;Flowchart.Variables&gt;&lt;Variable x:TypeArguments=\"scg:List(x:String)\" Name=\"InstructionList\" /&gt;&lt;Variable x:TypeArguments=\"x:String\" Name=\"LastResult\" /&gt;&lt;Variable x:TypeArguments=\"x:Boolean\" Name=\"HasError\" /&gt;&lt;Variable x:TypeArguments=\"x:String\" Name=\"ExplicitDataList\" /&gt;&lt;Variable x:TypeArguments=\"x:Boolean\" Name=\"IsValid\" /&gt;&lt;Variable x:TypeArguments=\"uf:UnlimitedObject\" Name=\"d\" /&gt;&lt;Variable x:TypeArguments=\"uaba:Util\" Name=\"t\" /&gt;&lt;Variable x:TypeArguments=\"ddd:Dev2DataListDecisionHandler\" Name=\"Dev2DecisionHandler\" /&gt;&lt;/Flowchart.Variables&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=\"x:String, x:Object\"&gt;&lt;x:Boolean x:Key=\"IsExpanded\"&gt;False&lt;/x:Boolean&gt;&lt;av:Point x:Key=\"ShapeLocation\"&gt;270,2.5&lt;/av:Point&gt;&lt;av:Size x:Key=\"ShapeSize\"&gt;60,75&lt;/av:Size&gt;&lt;av:PointCollection x:Key=\"ConnectorLocation\"&gt;300,77.5 300,107.5 310,107.5 310,211&lt;/av:PointCollection&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;Flowchart.StartNode&gt;&lt;x:Reference&gt;__ReferenceID0&lt;/x:Reference&gt;&lt;/Flowchart.StartNode&gt;&lt;FlowStep x:Name=\"__ReferenceID0\"&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=\"x:String, x:Object\"&gt;&lt;av:Point x:Key=\"ShapeLocation\"&gt;185,211&lt;/av:Point&gt;&lt;av:Size x:Key=\"ShapeSize\"&gt;250,84&lt;/av:Size&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;uaba:DsfActivity ActionName=\"{x:Null}\" ActivityStateData=\"{x:Null}\" AuthorRoles=\"{x:Null}\" Category=\"{x:Null}\" Compiler=\"{x:Null}\" CurrentResult=\"{x:Null}\" DataObject=\"{x:Null}\" DataTags=\"{x:Null}\" ExplicitDataList=\"{x:Null}\" InputTransformation=\"{x:Null}\" OnResumeKeepList=\"{x:Null}\" ParentServiceID=\"{x:Null}\" ParentServiceName=\"{x:Null}\" ParentWorkflowInstanceId=\"{x:Null}\" ResultTransformation=\"{x:Null}\" ResultValidationExpression=\"{x:Null}\" ResultValidationRequiredTags=\"{x:Null}\" ScenarioID=\"{x:Null}\" ScopingObject=\"{x:Null}\" ServiceUri=\"{x:Null}\" SimulationOutput=\"{x:Null}\" Tags=\"{x:Null}\" AddMode=\"False\" DatabindRecursive=\"False\" DeferExecution=\"False\" DisplayName=\"PBI 6690 - TEST Inputs\" EnvironmentID=\"00000000-0000-0000-0000-000000000000\" FriendlySourceName=\"localhost\" HasError=\"[HasError]\" sap:VirtualizedContainerService.HintSize=\"250,84\" IconPath=\"pack://application:,,,/Warewolf Studio;component/images/Workflow-32.png\" InputMapping=\"&amp;lt;Inputs&amp;gt;&amp;lt;Input Name=&amp;quot;n1&amp;quot; Source=&amp;quot;&amp;quot; /&amp;gt;&amp;lt;Input Name=&amp;quot;n2&amp;quot; Source=&amp;quot;&amp;quot; /&amp;gt;&amp;lt;Input Name=&amp;quot;n3&amp;quot; Source=&amp;quot;&amp;quot; /&amp;gt;&amp;lt;/Inputs&amp;gt;\" InstructionList=\"[InstructionList]\" IsSimulationEnabled=\"False\" IsUIStep=\"False\" IsValid=\"[IsValid]\" IsWorkflow=\"True\" OnResumeClearAmbientDataList=\"False\" OnResumeClearTags=\"FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage\" OutputMapping=\"&amp;lt;Outputs&amp;gt;&amp;lt;Output Name=&amp;quot;result&amp;quot; MapsTo=&amp;quot;result&amp;quot; Value=&amp;quot;&amp;quot; /&amp;gt;&amp;lt;/Outputs&amp;gt;\" RemoveInputFromOutput=\"False\" ServiceName=\"PBI 6690 - TEST Inputs\" ServiceServer=\"00000000-0000-0000-0000-000000000000\" SimulationMode=\"OnDemand\" ToolboxFriendlyName=\"PBI 6690 - TEST Inputs\" Type=\"Workflow\" UniqueID=\"edadb62e-83f4-44bf-a260-7639d6b43169\"&gt;&lt;uaba:DsfActivity.AmbientDataList&gt;&lt;InOutArgument x:TypeArguments=\"scg:List(x:String)\" /&gt;&lt;/uaba:DsfActivity.AmbientDataList&gt;&lt;uaba:DsfActivity.HelpLink&gt;&lt;InArgument x:TypeArguments=\"x:String\"&gt;&lt;Literal x:TypeArguments=\"x:String\" Value=\"\" /&gt;&lt;/InArgument&gt;&lt;/uaba:DsfActivity.HelpLink&gt;&lt;uaba:DsfActivity.ParentInstanceID&gt;&lt;InOutArgument x:TypeArguments=\"x:String\" /&gt;&lt;/uaba:DsfActivity.ParentInstanceID&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=\"x:String, x:Object\"&gt;&lt;x:Boolean x:Key=\"IsExpanded\"&gt;True&lt;/x:Boolean&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;/uaba:DsfActivity&gt;&lt;/FlowStep&gt;&lt;FlowStep&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=\"x:String, x:Object\"&gt;&lt;av:Point x:Key=\"ShapeLocation\"&gt;145,390&lt;/av:Point&gt;&lt;av:Size x:Key=\"ShapeSize\"&gt;250,100&lt;/av:Size&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;uaba:DsfActivity ActivityStateData=\"{x:Null}\" AuthorRoles=\"{x:Null}\" Category=\"{x:Null}\" Compiler=\"{x:Null}\" CurrentResult=\"{x:Null}\" DataObject=\"{x:Null}\" DataTags=\"{x:Null}\" ExplicitDataList=\"{x:Null}\" HelpLink=\"{x:Null}\" InputTransformation=\"{x:Null}\" OnResumeKeepList=\"{x:Null}\" ParentServiceID=\"{x:Null}\" ParentServiceName=\"{x:Null}\" ParentWorkflowInstanceId=\"{x:Null}\" ResultTransformation=\"{x:Null}\" ResultValidationExpression=\"{x:Null}\" ResultValidationRequiredTags=\"{x:Null}\" ScenarioID=\"{x:Null}\" ScopingObject=\"{x:Null}\" ServiceUri=\"{x:Null}\" SimulationOutput=\"{x:Null}\" Tags=\"{x:Null}\" ActionName=\"dbo.spGetCountries\" AddMode=\"False\" DatabindRecursive=\"False\" DeferExecution=\"False\" DisplayName=\"Countries\" EnvironmentID=\"00000000-0000-0000-0000-000000000000\" FriendlySourceName=\"CitiesDb\" HasError=\"[HasError]\" sap:VirtualizedContainerService.HintSize=\"250,100\" IconPath=\"[Nothing]\" InputMapping=\"&amp;lt;Inputs&amp;gt;&amp;lt;Input Name=&amp;quot;Prefix&amp;quot; Source=&amp;quot;Prefix&amp;quot; /&amp;gt;&amp;lt;/Inputs&amp;gt;\" InstructionList=\"[InstructionList]\" IsSimulationEnabled=\"False\" IsUIStep=\"False\" IsValid=\"[IsValid]\" IsWorkflow=\"False\" OnResumeClearAmbientDataList=\"False\" OnResumeClearTags=\"FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage\" OutputMapping=\"&amp;lt;Outputs&amp;gt;&amp;lt;Output Name=&amp;quot;CountryID&amp;quot; MapsTo=&amp;quot;CountryID&amp;quot; Value=&amp;quot;[[GetCountries().CountryID]]&amp;quot; Recordset=&amp;quot;GetCountries&amp;quot; /&amp;gt;&amp;lt;Output Name=&amp;quot;Description&amp;quot; MapsTo=&amp;quot;Name&amp;quot; Value=&amp;quot;[[GetCountries().Name]]&amp;quot; Recordset=&amp;quot;GetCountries&amp;quot; /&amp;gt;&amp;lt;/Outputs&amp;gt;\" RemoveInputFromOutput=\"False\" ServiceName=\"Countries\" ServiceServer=\"00000000-0000-0000-0000-000000000000\" SimulationMode=\"OnDemand\" ToolboxFriendlyName=\"Countries\" Type=\"InvokeStoredProc\" UniqueID=\"33476822-9519-4ad5-8fee-1ae3b571115b\"&gt;&lt;uaba:DsfActivity.AmbientDataList&gt;&lt;InOutArgument x:TypeArguments=\"scg:List(x:String)\" /&gt;&lt;/uaba:DsfActivity.AmbientDataList&gt;&lt;uaba:DsfActivity.ParentInstanceID&gt;&lt;InOutArgument x:TypeArguments=\"x:String\" /&gt;&lt;/uaba:DsfActivity.ParentInstanceID&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=\"x:String, x:Object\"&gt;&lt;x:Boolean x:Key=\"IsExpanded\"&gt;True&lt;/x:Boolean&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;/uaba:DsfActivity&gt;&lt;/FlowStep&gt;&lt;/Flowchart&gt;&lt;/Activity&gt;</XamlDefinition>";
            resourceModelMock.SetupGet(model => model.WorkflowXaml).Returns(new StringBuilder(WorkFlowXaml));
            resourceModelMock.SetupGet(model => model.Environment.EnvironmentID).Returns(Guid.NewGuid);
            resourceModelMock.SetupGet(model => model.ID).Returns(Guid.NewGuid);
            resourceModelMock.SetupGet(model => model.ServerID).Returns(Guid.NewGuid);
            var mock = new Mock<IServerRepository>();
            mock.Setup(repository => repository.ActiveServer).Returns(environmentModel.Object);
            CustomContainer.Register(mock.Object);
            var worksurfaceContextManager = new WorksurfaceContextManager(false, _shellViewModel, mock.Object, new Mock<IViewFactory>().Object);

            _activeEnvironment.Setup(model => model.Name).Returns("localhost");
            //---------------Execute Test ----------------------
            worksurfaceContextManager.DisplayResourceWizard(resourceModelMock.Object);

            var activetx = _shellViewModel.Items.ToList().First(i => i.WorkSurfaceViewModel.GetType().Name == "SourceViewModel`1");

            var vm = new WorkSurfaceContextViewModel(new EventAggregator(), new WorkSurfaceKey(), activetx.WorkSurfaceViewModel, new Mock<Common.Interfaces.Studio.Controller.IPopupController>().Object, (a, b, c) => { });
            var sourceVM = vm.WorkSurfaceViewModel as SourceViewModel<IExchangeSource>;
            sourceVM.ViewModel.Item = new Mock<IExchangeSource>().Object;
            mvm.Items.Add(vm);
            Assert.IsFalse(mvm.OnStudioClosing());
        }

        [TestMethod]
        [TestCategory("MainViewModel_OnStudioClosing")]
        [Owner("Pieter Terblanche")]
        public void MainViewModel_OnStudioClosing_CallsOAuthSourceOnClosing()
        {
            InitSourceViewModel(out Mock<IServer> environmentModel, out ShellViewModel mvm);
            var resourceModelMock = new Mock<IContextualResourceModel>();
            resourceModelMock.SetupGet(model => model.ServerResourceType).Returns("OauthSource");
            const string WorkFlowXaml = "<XamlDefinition>&lt;Activity x:Class=\"PBI 6690 - TEST\" xmlns=\"http://schemas.microsoft.com/netfx/2009/xaml/activities\" xmlns:av=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" xmlns:dc=\"clr-namespace:Dev2.Common;assembly=Dev2.CommonDataUtils\" xmlns:ddc=\"clr-namespace:Dev2.DataList.Contract;assembly=Dev2.Data\" xmlns:ddcb=\"clr-namespace:Dev2.DataList.Contract.Binary_Objects;assembly=Dev2.Data\" xmlns:ddd=\"clr-namespace:Dev2.Data.Decision;assembly=Dev2.Data\" xmlns:dddo=\"clr-namespace:Dev2.Data.Decisions.Operations;assembly=Dev2.Data\" xmlns:ddsm=\"clr-namespace:Dev2.Data.SystemTemplates.Models;assembly=Dev2.Data\" xmlns:mva=\"clr-namespace:Microsoft.VisualBasic.Activities;assembly=System.Activities\" xmlns:s=\"clr-namespace:System;assembly=mscorlib\" xmlns:sap=\"http://schemas.microsoft.com/netfx/2009/xaml/activities/presentation\" xmlns:scg=\"clr-namespace:System.Collections.Generic;assembly=mscorlib\" xmlns:sco=\"clr-namespace:System.Collections.ObjectModel;assembly=mscorlib\" xmlns:uaba=\"clr-namespace:Unlimited.Applications.BusinessDesignStudio.Activities;assembly=Dev2.Activities\" xmlns:uf=\"clr-namespace:Unlimited.Framework;assembly=Dev2.Core\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"&gt;&lt;x:Members&gt;&lt;x:Property Name=\"AmbientDataList\" Type=\"InOutArgument(scg:List(x:String))\" /&gt;&lt;x:Property Name=\"ParentWorkflowInstanceId\" Type=\"InOutArgument(s:Guid)\" /&gt;&lt;x:Property Name=\"ParentServiceName\" Type=\"InOutArgument(x:String)\" /&gt;&lt;/x:Members&gt;&lt;sap:VirtualizedContainerService.HintSize&gt;654,676&lt;/sap:VirtualizedContainerService.HintSize&gt;&lt;mva:VisualBasic.Settings&gt;Assembly references and imported namespaces serialized as XML namespaces&lt;/mva:VisualBasic.Settings&gt;&lt;TextExpression.NamespacesForImplementation&gt;&lt;scg:List x:TypeArguments=\"x:String\" Capacity=\"7\"&gt;&lt;x:String&gt;Dev2.CommonDataUtils&lt;/x:String&gt;&lt;x:String&gt;Dev2.Data.Decisions.Operations&lt;/x:String&gt;&lt;x:String&gt;Dev2.Data.SystemTemplates.Models&lt;/x:String&gt;&lt;x:String&gt;Dev2.DataList.Contract&lt;/x:String&gt;&lt;x:String&gt;Dev2.DataList.Contract.Binary_Objects&lt;/x:String&gt;&lt;x:String&gt;Unlimited.Framework&lt;/x:String&gt;&lt;x:String&gt;Unlimited.Applications.BusinessDesignStudio.Activities&lt;/x:String&gt;&lt;/scg:List&gt;&lt;/TextExpression.NamespacesForImplementation&gt;&lt;TextExpression.ReferencesForImplementation&gt;&lt;sco:Collection x:TypeArguments=\"AssemblyReference\"&gt;&lt;AssemblyReference&gt;Dev2.CommonDataUtils&lt;/AssemblyReference&gt;&lt;AssemblyReference&gt;Dev2.Data&lt;/AssemblyReference&gt;&lt;AssemblyReference&gt;Dev2.Core&lt;/AssemblyReference&gt;&lt;AssemblyReference&gt;Dev2.Activities&lt;/AssemblyReference&gt;&lt;/sco:Collection&gt;&lt;/TextExpression.ReferencesForImplementation&gt;&lt;Flowchart DisplayName=\"PBI 6690 - TEST\" sap:VirtualizedContainerService.HintSize=\"614,636\"&gt;&lt;Flowchart.Variables&gt;&lt;Variable x:TypeArguments=\"scg:List(x:String)\" Name=\"InstructionList\" /&gt;&lt;Variable x:TypeArguments=\"x:String\" Name=\"LastResult\" /&gt;&lt;Variable x:TypeArguments=\"x:Boolean\" Name=\"HasError\" /&gt;&lt;Variable x:TypeArguments=\"x:String\" Name=\"ExplicitDataList\" /&gt;&lt;Variable x:TypeArguments=\"x:Boolean\" Name=\"IsValid\" /&gt;&lt;Variable x:TypeArguments=\"uf:UnlimitedObject\" Name=\"d\" /&gt;&lt;Variable x:TypeArguments=\"uaba:Util\" Name=\"t\" /&gt;&lt;Variable x:TypeArguments=\"ddd:Dev2DataListDecisionHandler\" Name=\"Dev2DecisionHandler\" /&gt;&lt;/Flowchart.Variables&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=\"x:String, x:Object\"&gt;&lt;x:Boolean x:Key=\"IsExpanded\"&gt;False&lt;/x:Boolean&gt;&lt;av:Point x:Key=\"ShapeLocation\"&gt;270,2.5&lt;/av:Point&gt;&lt;av:Size x:Key=\"ShapeSize\"&gt;60,75&lt;/av:Size&gt;&lt;av:PointCollection x:Key=\"ConnectorLocation\"&gt;300,77.5 300,107.5 310,107.5 310,211&lt;/av:PointCollection&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;Flowchart.StartNode&gt;&lt;x:Reference&gt;__ReferenceID0&lt;/x:Reference&gt;&lt;/Flowchart.StartNode&gt;&lt;FlowStep x:Name=\"__ReferenceID0\"&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=\"x:String, x:Object\"&gt;&lt;av:Point x:Key=\"ShapeLocation\"&gt;185,211&lt;/av:Point&gt;&lt;av:Size x:Key=\"ShapeSize\"&gt;250,84&lt;/av:Size&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;uaba:DsfActivity ActionName=\"{x:Null}\" ActivityStateData=\"{x:Null}\" AuthorRoles=\"{x:Null}\" Category=\"{x:Null}\" Compiler=\"{x:Null}\" CurrentResult=\"{x:Null}\" DataObject=\"{x:Null}\" DataTags=\"{x:Null}\" ExplicitDataList=\"{x:Null}\" InputTransformation=\"{x:Null}\" OnResumeKeepList=\"{x:Null}\" ParentServiceID=\"{x:Null}\" ParentServiceName=\"{x:Null}\" ParentWorkflowInstanceId=\"{x:Null}\" ResultTransformation=\"{x:Null}\" ResultValidationExpression=\"{x:Null}\" ResultValidationRequiredTags=\"{x:Null}\" ScenarioID=\"{x:Null}\" ScopingObject=\"{x:Null}\" ServiceUri=\"{x:Null}\" SimulationOutput=\"{x:Null}\" Tags=\"{x:Null}\" AddMode=\"False\" DatabindRecursive=\"False\" DeferExecution=\"False\" DisplayName=\"PBI 6690 - TEST Inputs\" EnvironmentID=\"00000000-0000-0000-0000-000000000000\" FriendlySourceName=\"localhost\" HasError=\"[HasError]\" sap:VirtualizedContainerService.HintSize=\"250,84\" IconPath=\"pack://application:,,,/Warewolf Studio;component/images/Workflow-32.png\" InputMapping=\"&amp;lt;Inputs&amp;gt;&amp;lt;Input Name=&amp;quot;n1&amp;quot; Source=&amp;quot;&amp;quot; /&amp;gt;&amp;lt;Input Name=&amp;quot;n2&amp;quot; Source=&amp;quot;&amp;quot; /&amp;gt;&amp;lt;Input Name=&amp;quot;n3&amp;quot; Source=&amp;quot;&amp;quot; /&amp;gt;&amp;lt;/Inputs&amp;gt;\" InstructionList=\"[InstructionList]\" IsSimulationEnabled=\"False\" IsUIStep=\"False\" IsValid=\"[IsValid]\" IsWorkflow=\"True\" OnResumeClearAmbientDataList=\"False\" OnResumeClearTags=\"FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage\" OutputMapping=\"&amp;lt;Outputs&amp;gt;&amp;lt;Output Name=&amp;quot;result&amp;quot; MapsTo=&amp;quot;result&amp;quot; Value=&amp;quot;&amp;quot; /&amp;gt;&amp;lt;/Outputs&amp;gt;\" RemoveInputFromOutput=\"False\" ServiceName=\"PBI 6690 - TEST Inputs\" ServiceServer=\"00000000-0000-0000-0000-000000000000\" SimulationMode=\"OnDemand\" ToolboxFriendlyName=\"PBI 6690 - TEST Inputs\" Type=\"Workflow\" UniqueID=\"edadb62e-83f4-44bf-a260-7639d6b43169\"&gt;&lt;uaba:DsfActivity.AmbientDataList&gt;&lt;InOutArgument x:TypeArguments=\"scg:List(x:String)\" /&gt;&lt;/uaba:DsfActivity.AmbientDataList&gt;&lt;uaba:DsfActivity.HelpLink&gt;&lt;InArgument x:TypeArguments=\"x:String\"&gt;&lt;Literal x:TypeArguments=\"x:String\" Value=\"\" /&gt;&lt;/InArgument&gt;&lt;/uaba:DsfActivity.HelpLink&gt;&lt;uaba:DsfActivity.ParentInstanceID&gt;&lt;InOutArgument x:TypeArguments=\"x:String\" /&gt;&lt;/uaba:DsfActivity.ParentInstanceID&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=\"x:String, x:Object\"&gt;&lt;x:Boolean x:Key=\"IsExpanded\"&gt;True&lt;/x:Boolean&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;/uaba:DsfActivity&gt;&lt;/FlowStep&gt;&lt;FlowStep&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=\"x:String, x:Object\"&gt;&lt;av:Point x:Key=\"ShapeLocation\"&gt;145,390&lt;/av:Point&gt;&lt;av:Size x:Key=\"ShapeSize\"&gt;250,100&lt;/av:Size&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;uaba:DsfActivity ActivityStateData=\"{x:Null}\" AuthorRoles=\"{x:Null}\" Category=\"{x:Null}\" Compiler=\"{x:Null}\" CurrentResult=\"{x:Null}\" DataObject=\"{x:Null}\" DataTags=\"{x:Null}\" ExplicitDataList=\"{x:Null}\" HelpLink=\"{x:Null}\" InputTransformation=\"{x:Null}\" OnResumeKeepList=\"{x:Null}\" ParentServiceID=\"{x:Null}\" ParentServiceName=\"{x:Null}\" ParentWorkflowInstanceId=\"{x:Null}\" ResultTransformation=\"{x:Null}\" ResultValidationExpression=\"{x:Null}\" ResultValidationRequiredTags=\"{x:Null}\" ScenarioID=\"{x:Null}\" ScopingObject=\"{x:Null}\" ServiceUri=\"{x:Null}\" SimulationOutput=\"{x:Null}\" Tags=\"{x:Null}\" ActionName=\"dbo.spGetCountries\" AddMode=\"False\" DatabindRecursive=\"False\" DeferExecution=\"False\" DisplayName=\"Countries\" EnvironmentID=\"00000000-0000-0000-0000-000000000000\" FriendlySourceName=\"CitiesDb\" HasError=\"[HasError]\" sap:VirtualizedContainerService.HintSize=\"250,100\" IconPath=\"[Nothing]\" InputMapping=\"&amp;lt;Inputs&amp;gt;&amp;lt;Input Name=&amp;quot;Prefix&amp;quot; Source=&amp;quot;Prefix&amp;quot; /&amp;gt;&amp;lt;/Inputs&amp;gt;\" InstructionList=\"[InstructionList]\" IsSimulationEnabled=\"False\" IsUIStep=\"False\" IsValid=\"[IsValid]\" IsWorkflow=\"False\" OnResumeClearAmbientDataList=\"False\" OnResumeClearTags=\"FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage\" OutputMapping=\"&amp;lt;Outputs&amp;gt;&amp;lt;Output Name=&amp;quot;CountryID&amp;quot; MapsTo=&amp;quot;CountryID&amp;quot; Value=&amp;quot;[[GetCountries().CountryID]]&amp;quot; Recordset=&amp;quot;GetCountries&amp;quot; /&amp;gt;&amp;lt;Output Name=&amp;quot;Description&amp;quot; MapsTo=&amp;quot;Name&amp;quot; Value=&amp;quot;[[GetCountries().Name]]&amp;quot; Recordset=&amp;quot;GetCountries&amp;quot; /&amp;gt;&amp;lt;/Outputs&amp;gt;\" RemoveInputFromOutput=\"False\" ServiceName=\"Countries\" ServiceServer=\"00000000-0000-0000-0000-000000000000\" SimulationMode=\"OnDemand\" ToolboxFriendlyName=\"Countries\" Type=\"InvokeStoredProc\" UniqueID=\"33476822-9519-4ad5-8fee-1ae3b571115b\"&gt;&lt;uaba:DsfActivity.AmbientDataList&gt;&lt;InOutArgument x:TypeArguments=\"scg:List(x:String)\" /&gt;&lt;/uaba:DsfActivity.AmbientDataList&gt;&lt;uaba:DsfActivity.ParentInstanceID&gt;&lt;InOutArgument x:TypeArguments=\"x:String\" /&gt;&lt;/uaba:DsfActivity.ParentInstanceID&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=\"x:String, x:Object\"&gt;&lt;x:Boolean x:Key=\"IsExpanded\"&gt;True&lt;/x:Boolean&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;/uaba:DsfActivity&gt;&lt;/FlowStep&gt;&lt;/Flowchart&gt;&lt;/Activity&gt;</XamlDefinition>";
            resourceModelMock.SetupGet(model => model.WorkflowXaml).Returns(new StringBuilder(WorkFlowXaml));
            resourceModelMock.SetupGet(model => model.Environment.EnvironmentID).Returns(Guid.NewGuid);
            resourceModelMock.SetupGet(model => model.ID).Returns(Guid.NewGuid);
            resourceModelMock.SetupGet(model => model.ServerID).Returns(Guid.NewGuid);
            var mock = new Mock<IServerRepository>();
            mock.Setup(repository => repository.ActiveServer).Returns(environmentModel.Object);
            CustomContainer.DeRegister<IServerRepository>();
            CustomContainer.Register(mock.Object);
            var worksurfaceContextManager = new WorksurfaceContextManager(false, _shellViewModel, mock.Object, new Mock<IViewFactory>().Object);

            _activeEnvironment.Setup(model => model.Name).Returns("localhost");
            //---------------Execute Test ----------------------
            worksurfaceContextManager.DisplayResourceWizard(resourceModelMock.Object);

            var activetx = _shellViewModel.Items.ToList().First(i => i.WorkSurfaceViewModel.GetType().Name == "SourceViewModel`1");

            var vm = new WorkSurfaceContextViewModel(new EventAggregator(), new WorkSurfaceKey(), activetx.WorkSurfaceViewModel, new Mock<Common.Interfaces.Studio.Controller.IPopupController>().Object, (a, b, c) => { });
            var sourceVM = vm.WorkSurfaceViewModel as SourceViewModel<IOAuthSource>;
            sourceVM.ViewModel.Item = new Mock<IOAuthSource>().Object;
            mvm.Items.Add(vm);
            Assert.IsFalse(mvm.OnStudioClosing());
        }

        [TestMethod]
        [TestCategory("MainViewModel_OnStudioClosing")]
        [Owner("Pieter Terblanche")]
        public void MainViewModel_OnStudioClosing_CallsSharepointServerSourceOnClosing()
        {
            InitSourceViewModel(out Mock<IServer> environmentModel, out ShellViewModel mvm);
            var resourceModelMock = new Mock<IContextualResourceModel>();
            resourceModelMock.SetupGet(model => model.ServerResourceType).Returns("SharepointServerSource");
            const string WorkFlowXaml = "<XamlDefinition>&lt;Activity x:Class=\"PBI 6690 - TEST\" xmlns=\"http://schemas.microsoft.com/netfx/2009/xaml/activities\" xmlns:av=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" xmlns:dc=\"clr-namespace:Dev2.Common;assembly=Dev2.CommonDataUtils\" xmlns:ddc=\"clr-namespace:Dev2.DataList.Contract;assembly=Dev2.Data\" xmlns:ddcb=\"clr-namespace:Dev2.DataList.Contract.Binary_Objects;assembly=Dev2.Data\" xmlns:ddd=\"clr-namespace:Dev2.Data.Decision;assembly=Dev2.Data\" xmlns:dddo=\"clr-namespace:Dev2.Data.Decisions.Operations;assembly=Dev2.Data\" xmlns:ddsm=\"clr-namespace:Dev2.Data.SystemTemplates.Models;assembly=Dev2.Data\" xmlns:mva=\"clr-namespace:Microsoft.VisualBasic.Activities;assembly=System.Activities\" xmlns:s=\"clr-namespace:System;assembly=mscorlib\" xmlns:sap=\"http://schemas.microsoft.com/netfx/2009/xaml/activities/presentation\" xmlns:scg=\"clr-namespace:System.Collections.Generic;assembly=mscorlib\" xmlns:sco=\"clr-namespace:System.Collections.ObjectModel;assembly=mscorlib\" xmlns:uaba=\"clr-namespace:Unlimited.Applications.BusinessDesignStudio.Activities;assembly=Dev2.Activities\" xmlns:uf=\"clr-namespace:Unlimited.Framework;assembly=Dev2.Core\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"&gt;&lt;x:Members&gt;&lt;x:Property Name=\"AmbientDataList\" Type=\"InOutArgument(scg:List(x:String))\" /&gt;&lt;x:Property Name=\"ParentWorkflowInstanceId\" Type=\"InOutArgument(s:Guid)\" /&gt;&lt;x:Property Name=\"ParentServiceName\" Type=\"InOutArgument(x:String)\" /&gt;&lt;/x:Members&gt;&lt;sap:VirtualizedContainerService.HintSize&gt;654,676&lt;/sap:VirtualizedContainerService.HintSize&gt;&lt;mva:VisualBasic.Settings&gt;Assembly references and imported namespaces serialized as XML namespaces&lt;/mva:VisualBasic.Settings&gt;&lt;TextExpression.NamespacesForImplementation&gt;&lt;scg:List x:TypeArguments=\"x:String\" Capacity=\"7\"&gt;&lt;x:String&gt;Dev2.CommonDataUtils&lt;/x:String&gt;&lt;x:String&gt;Dev2.Data.Decisions.Operations&lt;/x:String&gt;&lt;x:String&gt;Dev2.Data.SystemTemplates.Models&lt;/x:String&gt;&lt;x:String&gt;Dev2.DataList.Contract&lt;/x:String&gt;&lt;x:String&gt;Dev2.DataList.Contract.Binary_Objects&lt;/x:String&gt;&lt;x:String&gt;Unlimited.Framework&lt;/x:String&gt;&lt;x:String&gt;Unlimited.Applications.BusinessDesignStudio.Activities&lt;/x:String&gt;&lt;/scg:List&gt;&lt;/TextExpression.NamespacesForImplementation&gt;&lt;TextExpression.ReferencesForImplementation&gt;&lt;sco:Collection x:TypeArguments=\"AssemblyReference\"&gt;&lt;AssemblyReference&gt;Dev2.CommonDataUtils&lt;/AssemblyReference&gt;&lt;AssemblyReference&gt;Dev2.Data&lt;/AssemblyReference&gt;&lt;AssemblyReference&gt;Dev2.Core&lt;/AssemblyReference&gt;&lt;AssemblyReference&gt;Dev2.Activities&lt;/AssemblyReference&gt;&lt;/sco:Collection&gt;&lt;/TextExpression.ReferencesForImplementation&gt;&lt;Flowchart DisplayName=\"PBI 6690 - TEST\" sap:VirtualizedContainerService.HintSize=\"614,636\"&gt;&lt;Flowchart.Variables&gt;&lt;Variable x:TypeArguments=\"scg:List(x:String)\" Name=\"InstructionList\" /&gt;&lt;Variable x:TypeArguments=\"x:String\" Name=\"LastResult\" /&gt;&lt;Variable x:TypeArguments=\"x:Boolean\" Name=\"HasError\" /&gt;&lt;Variable x:TypeArguments=\"x:String\" Name=\"ExplicitDataList\" /&gt;&lt;Variable x:TypeArguments=\"x:Boolean\" Name=\"IsValid\" /&gt;&lt;Variable x:TypeArguments=\"uf:UnlimitedObject\" Name=\"d\" /&gt;&lt;Variable x:TypeArguments=\"uaba:Util\" Name=\"t\" /&gt;&lt;Variable x:TypeArguments=\"ddd:Dev2DataListDecisionHandler\" Name=\"Dev2DecisionHandler\" /&gt;&lt;/Flowchart.Variables&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=\"x:String, x:Object\"&gt;&lt;x:Boolean x:Key=\"IsExpanded\"&gt;False&lt;/x:Boolean&gt;&lt;av:Point x:Key=\"ShapeLocation\"&gt;270,2.5&lt;/av:Point&gt;&lt;av:Size x:Key=\"ShapeSize\"&gt;60,75&lt;/av:Size&gt;&lt;av:PointCollection x:Key=\"ConnectorLocation\"&gt;300,77.5 300,107.5 310,107.5 310,211&lt;/av:PointCollection&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;Flowchart.StartNode&gt;&lt;x:Reference&gt;__ReferenceID0&lt;/x:Reference&gt;&lt;/Flowchart.StartNode&gt;&lt;FlowStep x:Name=\"__ReferenceID0\"&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=\"x:String, x:Object\"&gt;&lt;av:Point x:Key=\"ShapeLocation\"&gt;185,211&lt;/av:Point&gt;&lt;av:Size x:Key=\"ShapeSize\"&gt;250,84&lt;/av:Size&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;uaba:DsfActivity ActionName=\"{x:Null}\" ActivityStateData=\"{x:Null}\" AuthorRoles=\"{x:Null}\" Category=\"{x:Null}\" Compiler=\"{x:Null}\" CurrentResult=\"{x:Null}\" DataObject=\"{x:Null}\" DataTags=\"{x:Null}\" ExplicitDataList=\"{x:Null}\" InputTransformation=\"{x:Null}\" OnResumeKeepList=\"{x:Null}\" ParentServiceID=\"{x:Null}\" ParentServiceName=\"{x:Null}\" ParentWorkflowInstanceId=\"{x:Null}\" ResultTransformation=\"{x:Null}\" ResultValidationExpression=\"{x:Null}\" ResultValidationRequiredTags=\"{x:Null}\" ScenarioID=\"{x:Null}\" ScopingObject=\"{x:Null}\" ServiceUri=\"{x:Null}\" SimulationOutput=\"{x:Null}\" Tags=\"{x:Null}\" AddMode=\"False\" DatabindRecursive=\"False\" DeferExecution=\"False\" DisplayName=\"PBI 6690 - TEST Inputs\" EnvironmentID=\"00000000-0000-0000-0000-000000000000\" FriendlySourceName=\"localhost\" HasError=\"[HasError]\" sap:VirtualizedContainerService.HintSize=\"250,84\" IconPath=\"pack://application:,,,/Warewolf Studio;component/images/Workflow-32.png\" InputMapping=\"&amp;lt;Inputs&amp;gt;&amp;lt;Input Name=&amp;quot;n1&amp;quot; Source=&amp;quot;&amp;quot; /&amp;gt;&amp;lt;Input Name=&amp;quot;n2&amp;quot; Source=&amp;quot;&amp;quot; /&amp;gt;&amp;lt;Input Name=&amp;quot;n3&amp;quot; Source=&amp;quot;&amp;quot; /&amp;gt;&amp;lt;/Inputs&amp;gt;\" InstructionList=\"[InstructionList]\" IsSimulationEnabled=\"False\" IsUIStep=\"False\" IsValid=\"[IsValid]\" IsWorkflow=\"True\" OnResumeClearAmbientDataList=\"False\" OnResumeClearTags=\"FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage\" OutputMapping=\"&amp;lt;Outputs&amp;gt;&amp;lt;Output Name=&amp;quot;result&amp;quot; MapsTo=&amp;quot;result&amp;quot; Value=&amp;quot;&amp;quot; /&amp;gt;&amp;lt;/Outputs&amp;gt;\" RemoveInputFromOutput=\"False\" ServiceName=\"PBI 6690 - TEST Inputs\" ServiceServer=\"00000000-0000-0000-0000-000000000000\" SimulationMode=\"OnDemand\" ToolboxFriendlyName=\"PBI 6690 - TEST Inputs\" Type=\"Workflow\" UniqueID=\"edadb62e-83f4-44bf-a260-7639d6b43169\"&gt;&lt;uaba:DsfActivity.AmbientDataList&gt;&lt;InOutArgument x:TypeArguments=\"scg:List(x:String)\" /&gt;&lt;/uaba:DsfActivity.AmbientDataList&gt;&lt;uaba:DsfActivity.HelpLink&gt;&lt;InArgument x:TypeArguments=\"x:String\"&gt;&lt;Literal x:TypeArguments=\"x:String\" Value=\"\" /&gt;&lt;/InArgument&gt;&lt;/uaba:DsfActivity.HelpLink&gt;&lt;uaba:DsfActivity.ParentInstanceID&gt;&lt;InOutArgument x:TypeArguments=\"x:String\" /&gt;&lt;/uaba:DsfActivity.ParentInstanceID&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=\"x:String, x:Object\"&gt;&lt;x:Boolean x:Key=\"IsExpanded\"&gt;True&lt;/x:Boolean&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;/uaba:DsfActivity&gt;&lt;/FlowStep&gt;&lt;FlowStep&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=\"x:String, x:Object\"&gt;&lt;av:Point x:Key=\"ShapeLocation\"&gt;145,390&lt;/av:Point&gt;&lt;av:Size x:Key=\"ShapeSize\"&gt;250,100&lt;/av:Size&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;uaba:DsfActivity ActivityStateData=\"{x:Null}\" AuthorRoles=\"{x:Null}\" Category=\"{x:Null}\" Compiler=\"{x:Null}\" CurrentResult=\"{x:Null}\" DataObject=\"{x:Null}\" DataTags=\"{x:Null}\" ExplicitDataList=\"{x:Null}\" HelpLink=\"{x:Null}\" InputTransformation=\"{x:Null}\" OnResumeKeepList=\"{x:Null}\" ParentServiceID=\"{x:Null}\" ParentServiceName=\"{x:Null}\" ParentWorkflowInstanceId=\"{x:Null}\" ResultTransformation=\"{x:Null}\" ResultValidationExpression=\"{x:Null}\" ResultValidationRequiredTags=\"{x:Null}\" ScenarioID=\"{x:Null}\" ScopingObject=\"{x:Null}\" ServiceUri=\"{x:Null}\" SimulationOutput=\"{x:Null}\" Tags=\"{x:Null}\" ActionName=\"dbo.spGetCountries\" AddMode=\"False\" DatabindRecursive=\"False\" DeferExecution=\"False\" DisplayName=\"Countries\" EnvironmentID=\"00000000-0000-0000-0000-000000000000\" FriendlySourceName=\"CitiesDb\" HasError=\"[HasError]\" sap:VirtualizedContainerService.HintSize=\"250,100\" IconPath=\"[Nothing]\" InputMapping=\"&amp;lt;Inputs&amp;gt;&amp;lt;Input Name=&amp;quot;Prefix&amp;quot; Source=&amp;quot;Prefix&amp;quot; /&amp;gt;&amp;lt;/Inputs&amp;gt;\" InstructionList=\"[InstructionList]\" IsSimulationEnabled=\"False\" IsUIStep=\"False\" IsValid=\"[IsValid]\" IsWorkflow=\"False\" OnResumeClearAmbientDataList=\"False\" OnResumeClearTags=\"FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage\" OutputMapping=\"&amp;lt;Outputs&amp;gt;&amp;lt;Output Name=&amp;quot;CountryID&amp;quot; MapsTo=&amp;quot;CountryID&amp;quot; Value=&amp;quot;[[GetCountries().CountryID]]&amp;quot; Recordset=&amp;quot;GetCountries&amp;quot; /&amp;gt;&amp;lt;Output Name=&amp;quot;Description&amp;quot; MapsTo=&amp;quot;Name&amp;quot; Value=&amp;quot;[[GetCountries().Name]]&amp;quot; Recordset=&amp;quot;GetCountries&amp;quot; /&amp;gt;&amp;lt;/Outputs&amp;gt;\" RemoveInputFromOutput=\"False\" ServiceName=\"Countries\" ServiceServer=\"00000000-0000-0000-0000-000000000000\" SimulationMode=\"OnDemand\" ToolboxFriendlyName=\"Countries\" Type=\"InvokeStoredProc\" UniqueID=\"33476822-9519-4ad5-8fee-1ae3b571115b\"&gt;&lt;uaba:DsfActivity.AmbientDataList&gt;&lt;InOutArgument x:TypeArguments=\"scg:List(x:String)\" /&gt;&lt;/uaba:DsfActivity.AmbientDataList&gt;&lt;uaba:DsfActivity.ParentInstanceID&gt;&lt;InOutArgument x:TypeArguments=\"x:String\" /&gt;&lt;/uaba:DsfActivity.ParentInstanceID&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=\"x:String, x:Object\"&gt;&lt;x:Boolean x:Key=\"IsExpanded\"&gt;True&lt;/x:Boolean&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;/uaba:DsfActivity&gt;&lt;/FlowStep&gt;&lt;/Flowchart&gt;&lt;/Activity&gt;</XamlDefinition>";
            resourceModelMock.SetupGet(model => model.WorkflowXaml).Returns(new StringBuilder(WorkFlowXaml));
            resourceModelMock.SetupGet(model => model.Environment.EnvironmentID).Returns(Guid.NewGuid);
            resourceModelMock.SetupGet(model => model.ID).Returns(Guid.NewGuid);
            resourceModelMock.SetupGet(model => model.ServerID).Returns(Guid.NewGuid);
            var newMockServer = new Mock<IServerRepository>();
            newMockServer.Setup(repository => repository.ActiveServer).Returns(environmentModel.Object);
            CustomContainer.DeRegister<IServerRepository>();
            CustomContainer.Register(newMockServer.Object);
            var worksurfaceContextManager = new WorksurfaceContextManager(false, _shellViewModel, newMockServer.Object, new Mock<IViewFactory>().Object);

            _activeEnvironment.Setup(model => model.Name).Returns("localhost");
            //---------------Execute Test ----------------------
            worksurfaceContextManager.DisplayResourceWizard(resourceModelMock.Object);


            var activetx = _shellViewModel.Items.ToList().First(i => i.WorkSurfaceViewModel.GetType().Name == "SourceViewModel`1");

            var vm = new WorkSurfaceContextViewModel(new EventAggregator(), new WorkSurfaceKey(), activetx.WorkSurfaceViewModel, new Mock<Common.Interfaces.Studio.Controller.IPopupController>().Object, (a, b, c) => { });
            var sourceVM = vm.WorkSurfaceViewModel as SourceViewModel<ISharepointServerSource>;
            sourceVM.ViewModel.Item = new Mock<ISharepointServerSource>().Object;
            mvm.Items.Add(vm);
            Assert.IsFalse(mvm.OnStudioClosing());
        }

        [TestMethod]
        [TestCategory("MainViewModel_OnStudioClosing")]
        [Owner("Pieter Terblanche")]
        public void MainViewModel_OnStudioClosing_CallsRabbitMQSourceOnClosing()
        {
            InitSourceViewModel(out Mock<IServer> environmentModel, out ShellViewModel mvm);
            var resourceModelMock = new Mock<IContextualResourceModel>();
            resourceModelMock.SetupGet(model => model.ServerResourceType).Returns("RabbitMQSource");
            const string WorkFlowXaml = "<XamlDefinition>&lt;Activity x:Class=\"PBI 6690 - TEST\" xmlns=\"http://schemas.microsoft.com/netfx/2009/xaml/activities\" xmlns:av=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" xmlns:dc=\"clr-namespace:Dev2.Common;assembly=Dev2.CommonDataUtils\" xmlns:ddc=\"clr-namespace:Dev2.DataList.Contract;assembly=Dev2.Data\" xmlns:ddcb=\"clr-namespace:Dev2.DataList.Contract.Binary_Objects;assembly=Dev2.Data\" xmlns:ddd=\"clr-namespace:Dev2.Data.Decision;assembly=Dev2.Data\" xmlns:dddo=\"clr-namespace:Dev2.Data.Decisions.Operations;assembly=Dev2.Data\" xmlns:ddsm=\"clr-namespace:Dev2.Data.SystemTemplates.Models;assembly=Dev2.Data\" xmlns:mva=\"clr-namespace:Microsoft.VisualBasic.Activities;assembly=System.Activities\" xmlns:s=\"clr-namespace:System;assembly=mscorlib\" xmlns:sap=\"http://schemas.microsoft.com/netfx/2009/xaml/activities/presentation\" xmlns:scg=\"clr-namespace:System.Collections.Generic;assembly=mscorlib\" xmlns:sco=\"clr-namespace:System.Collections.ObjectModel;assembly=mscorlib\" xmlns:uaba=\"clr-namespace:Unlimited.Applications.BusinessDesignStudio.Activities;assembly=Dev2.Activities\" xmlns:uf=\"clr-namespace:Unlimited.Framework;assembly=Dev2.Core\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"&gt;&lt;x:Members&gt;&lt;x:Property Name=\"AmbientDataList\" Type=\"InOutArgument(scg:List(x:String))\" /&gt;&lt;x:Property Name=\"ParentWorkflowInstanceId\" Type=\"InOutArgument(s:Guid)\" /&gt;&lt;x:Property Name=\"ParentServiceName\" Type=\"InOutArgument(x:String)\" /&gt;&lt;/x:Members&gt;&lt;sap:VirtualizedContainerService.HintSize&gt;654,676&lt;/sap:VirtualizedContainerService.HintSize&gt;&lt;mva:VisualBasic.Settings&gt;Assembly references and imported namespaces serialized as XML namespaces&lt;/mva:VisualBasic.Settings&gt;&lt;TextExpression.NamespacesForImplementation&gt;&lt;scg:List x:TypeArguments=\"x:String\" Capacity=\"7\"&gt;&lt;x:String&gt;Dev2.CommonDataUtils&lt;/x:String&gt;&lt;x:String&gt;Dev2.Data.Decisions.Operations&lt;/x:String&gt;&lt;x:String&gt;Dev2.Data.SystemTemplates.Models&lt;/x:String&gt;&lt;x:String&gt;Dev2.DataList.Contract&lt;/x:String&gt;&lt;x:String&gt;Dev2.DataList.Contract.Binary_Objects&lt;/x:String&gt;&lt;x:String&gt;Unlimited.Framework&lt;/x:String&gt;&lt;x:String&gt;Unlimited.Applications.BusinessDesignStudio.Activities&lt;/x:String&gt;&lt;/scg:List&gt;&lt;/TextExpression.NamespacesForImplementation&gt;&lt;TextExpression.ReferencesForImplementation&gt;&lt;sco:Collection x:TypeArguments=\"AssemblyReference\"&gt;&lt;AssemblyReference&gt;Dev2.CommonDataUtils&lt;/AssemblyReference&gt;&lt;AssemblyReference&gt;Dev2.Data&lt;/AssemblyReference&gt;&lt;AssemblyReference&gt;Dev2.Core&lt;/AssemblyReference&gt;&lt;AssemblyReference&gt;Dev2.Activities&lt;/AssemblyReference&gt;&lt;/sco:Collection&gt;&lt;/TextExpression.ReferencesForImplementation&gt;&lt;Flowchart DisplayName=\"PBI 6690 - TEST\" sap:VirtualizedContainerService.HintSize=\"614,636\"&gt;&lt;Flowchart.Variables&gt;&lt;Variable x:TypeArguments=\"scg:List(x:String)\" Name=\"InstructionList\" /&gt;&lt;Variable x:TypeArguments=\"x:String\" Name=\"LastResult\" /&gt;&lt;Variable x:TypeArguments=\"x:Boolean\" Name=\"HasError\" /&gt;&lt;Variable x:TypeArguments=\"x:String\" Name=\"ExplicitDataList\" /&gt;&lt;Variable x:TypeArguments=\"x:Boolean\" Name=\"IsValid\" /&gt;&lt;Variable x:TypeArguments=\"uf:UnlimitedObject\" Name=\"d\" /&gt;&lt;Variable x:TypeArguments=\"uaba:Util\" Name=\"t\" /&gt;&lt;Variable x:TypeArguments=\"ddd:Dev2DataListDecisionHandler\" Name=\"Dev2DecisionHandler\" /&gt;&lt;/Flowchart.Variables&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=\"x:String, x:Object\"&gt;&lt;x:Boolean x:Key=\"IsExpanded\"&gt;False&lt;/x:Boolean&gt;&lt;av:Point x:Key=\"ShapeLocation\"&gt;270,2.5&lt;/av:Point&gt;&lt;av:Size x:Key=\"ShapeSize\"&gt;60,75&lt;/av:Size&gt;&lt;av:PointCollection x:Key=\"ConnectorLocation\"&gt;300,77.5 300,107.5 310,107.5 310,211&lt;/av:PointCollection&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;Flowchart.StartNode&gt;&lt;x:Reference&gt;__ReferenceID0&lt;/x:Reference&gt;&lt;/Flowchart.StartNode&gt;&lt;FlowStep x:Name=\"__ReferenceID0\"&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=\"x:String, x:Object\"&gt;&lt;av:Point x:Key=\"ShapeLocation\"&gt;185,211&lt;/av:Point&gt;&lt;av:Size x:Key=\"ShapeSize\"&gt;250,84&lt;/av:Size&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;uaba:DsfActivity ActionName=\"{x:Null}\" ActivityStateData=\"{x:Null}\" AuthorRoles=\"{x:Null}\" Category=\"{x:Null}\" Compiler=\"{x:Null}\" CurrentResult=\"{x:Null}\" DataObject=\"{x:Null}\" DataTags=\"{x:Null}\" ExplicitDataList=\"{x:Null}\" InputTransformation=\"{x:Null}\" OnResumeKeepList=\"{x:Null}\" ParentServiceID=\"{x:Null}\" ParentServiceName=\"{x:Null}\" ParentWorkflowInstanceId=\"{x:Null}\" ResultTransformation=\"{x:Null}\" ResultValidationExpression=\"{x:Null}\" ResultValidationRequiredTags=\"{x:Null}\" ScenarioID=\"{x:Null}\" ScopingObject=\"{x:Null}\" ServiceUri=\"{x:Null}\" SimulationOutput=\"{x:Null}\" Tags=\"{x:Null}\" AddMode=\"False\" DatabindRecursive=\"False\" DeferExecution=\"False\" DisplayName=\"PBI 6690 - TEST Inputs\" EnvironmentID=\"00000000-0000-0000-0000-000000000000\" FriendlySourceName=\"localhost\" HasError=\"[HasError]\" sap:VirtualizedContainerService.HintSize=\"250,84\" IconPath=\"pack://application:,,,/Warewolf Studio;component/images/Workflow-32.png\" InputMapping=\"&amp;lt;Inputs&amp;gt;&amp;lt;Input Name=&amp;quot;n1&amp;quot; Source=&amp;quot;&amp;quot; /&amp;gt;&amp;lt;Input Name=&amp;quot;n2&amp;quot; Source=&amp;quot;&amp;quot; /&amp;gt;&amp;lt;Input Name=&amp;quot;n3&amp;quot; Source=&amp;quot;&amp;quot; /&amp;gt;&amp;lt;/Inputs&amp;gt;\" InstructionList=\"[InstructionList]\" IsSimulationEnabled=\"False\" IsUIStep=\"False\" IsValid=\"[IsValid]\" IsWorkflow=\"True\" OnResumeClearAmbientDataList=\"False\" OnResumeClearTags=\"FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage\" OutputMapping=\"&amp;lt;Outputs&amp;gt;&amp;lt;Output Name=&amp;quot;result&amp;quot; MapsTo=&amp;quot;result&amp;quot; Value=&amp;quot;&amp;quot; /&amp;gt;&amp;lt;/Outputs&amp;gt;\" RemoveInputFromOutput=\"False\" ServiceName=\"PBI 6690 - TEST Inputs\" ServiceServer=\"00000000-0000-0000-0000-000000000000\" SimulationMode=\"OnDemand\" ToolboxFriendlyName=\"PBI 6690 - TEST Inputs\" Type=\"Workflow\" UniqueID=\"edadb62e-83f4-44bf-a260-7639d6b43169\"&gt;&lt;uaba:DsfActivity.AmbientDataList&gt;&lt;InOutArgument x:TypeArguments=\"scg:List(x:String)\" /&gt;&lt;/uaba:DsfActivity.AmbientDataList&gt;&lt;uaba:DsfActivity.HelpLink&gt;&lt;InArgument x:TypeArguments=\"x:String\"&gt;&lt;Literal x:TypeArguments=\"x:String\" Value=\"\" /&gt;&lt;/InArgument&gt;&lt;/uaba:DsfActivity.HelpLink&gt;&lt;uaba:DsfActivity.ParentInstanceID&gt;&lt;InOutArgument x:TypeArguments=\"x:String\" /&gt;&lt;/uaba:DsfActivity.ParentInstanceID&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=\"x:String, x:Object\"&gt;&lt;x:Boolean x:Key=\"IsExpanded\"&gt;True&lt;/x:Boolean&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;/uaba:DsfActivity&gt;&lt;/FlowStep&gt;&lt;FlowStep&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=\"x:String, x:Object\"&gt;&lt;av:Point x:Key=\"ShapeLocation\"&gt;145,390&lt;/av:Point&gt;&lt;av:Size x:Key=\"ShapeSize\"&gt;250,100&lt;/av:Size&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;uaba:DsfActivity ActivityStateData=\"{x:Null}\" AuthorRoles=\"{x:Null}\" Category=\"{x:Null}\" Compiler=\"{x:Null}\" CurrentResult=\"{x:Null}\" DataObject=\"{x:Null}\" DataTags=\"{x:Null}\" ExplicitDataList=\"{x:Null}\" HelpLink=\"{x:Null}\" InputTransformation=\"{x:Null}\" OnResumeKeepList=\"{x:Null}\" ParentServiceID=\"{x:Null}\" ParentServiceName=\"{x:Null}\" ParentWorkflowInstanceId=\"{x:Null}\" ResultTransformation=\"{x:Null}\" ResultValidationExpression=\"{x:Null}\" ResultValidationRequiredTags=\"{x:Null}\" ScenarioID=\"{x:Null}\" ScopingObject=\"{x:Null}\" ServiceUri=\"{x:Null}\" SimulationOutput=\"{x:Null}\" Tags=\"{x:Null}\" ActionName=\"dbo.spGetCountries\" AddMode=\"False\" DatabindRecursive=\"False\" DeferExecution=\"False\" DisplayName=\"Countries\" EnvironmentID=\"00000000-0000-0000-0000-000000000000\" FriendlySourceName=\"CitiesDb\" HasError=\"[HasError]\" sap:VirtualizedContainerService.HintSize=\"250,100\" IconPath=\"[Nothing]\" InputMapping=\"&amp;lt;Inputs&amp;gt;&amp;lt;Input Name=&amp;quot;Prefix&amp;quot; Source=&amp;quot;Prefix&amp;quot; /&amp;gt;&amp;lt;/Inputs&amp;gt;\" InstructionList=\"[InstructionList]\" IsSimulationEnabled=\"False\" IsUIStep=\"False\" IsValid=\"[IsValid]\" IsWorkflow=\"False\" OnResumeClearAmbientDataList=\"False\" OnResumeClearTags=\"FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage\" OutputMapping=\"&amp;lt;Outputs&amp;gt;&amp;lt;Output Name=&amp;quot;CountryID&amp;quot; MapsTo=&amp;quot;CountryID&amp;quot; Value=&amp;quot;[[GetCountries().CountryID]]&amp;quot; Recordset=&amp;quot;GetCountries&amp;quot; /&amp;gt;&amp;lt;Output Name=&amp;quot;Description&amp;quot; MapsTo=&amp;quot;Name&amp;quot; Value=&amp;quot;[[GetCountries().Name]]&amp;quot; Recordset=&amp;quot;GetCountries&amp;quot; /&amp;gt;&amp;lt;/Outputs&amp;gt;\" RemoveInputFromOutput=\"False\" ServiceName=\"Countries\" ServiceServer=\"00000000-0000-0000-0000-000000000000\" SimulationMode=\"OnDemand\" ToolboxFriendlyName=\"Countries\" Type=\"InvokeStoredProc\" UniqueID=\"33476822-9519-4ad5-8fee-1ae3b571115b\"&gt;&lt;uaba:DsfActivity.AmbientDataList&gt;&lt;InOutArgument x:TypeArguments=\"scg:List(x:String)\" /&gt;&lt;/uaba:DsfActivity.AmbientDataList&gt;&lt;uaba:DsfActivity.ParentInstanceID&gt;&lt;InOutArgument x:TypeArguments=\"x:String\" /&gt;&lt;/uaba:DsfActivity.ParentInstanceID&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=\"x:String, x:Object\"&gt;&lt;x:Boolean x:Key=\"IsExpanded\"&gt;True&lt;/x:Boolean&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;/uaba:DsfActivity&gt;&lt;/FlowStep&gt;&lt;/Flowchart&gt;&lt;/Activity&gt;</XamlDefinition>";
            resourceModelMock.SetupGet(model => model.WorkflowXaml).Returns(new StringBuilder(WorkFlowXaml));
            resourceModelMock.SetupGet(model => model.Environment.EnvironmentID).Returns(Guid.NewGuid);
            resourceModelMock.SetupGet(model => model.ID).Returns(Guid.NewGuid);
            resourceModelMock.SetupGet(model => model.ServerID).Returns(Guid.NewGuid);
            var mock = new Mock<IServerRepository>();
            mock.Setup(repository => repository.ActiveServer).Returns(environmentModel.Object);
            CustomContainer.Register(mock.Object);

            var worksurfaceContextManager = new WorksurfaceContextManager(false, _shellViewModel, mock.Object, new Mock<IViewFactory>().Object);

            _activeEnvironment.Setup(model => model.Name).Returns("localhost");
            //---------------Execute Test ----------------------
            worksurfaceContextManager.DisplayResourceWizard(resourceModelMock.Object);

            var activetx = _shellViewModel.Items.ToList().First(i => i.WorkSurfaceViewModel.GetType().Name == "SourceViewModel`1");

            var vm = new WorkSurfaceContextViewModel(new EventAggregator(), new WorkSurfaceKey(), activetx.WorkSurfaceViewModel, new Mock<Common.Interfaces.Studio.Controller.IPopupController>().Object, (a, b, c) => { });
            var sourceVM = vm.WorkSurfaceViewModel as SourceViewModel<IRabbitMQServiceSourceDefinition>;
            sourceVM.ViewModel.Item = new Mock<IRabbitMQServiceSourceDefinition>().Object;
            mvm.Items.Add(vm);
            Assert.IsFalse(mvm.OnStudioClosing());
        }

        [TestMethod]
        [TestCategory("MainViewModel_OnStudioClosing")]
        [Owner("Pieter Terblanche")]
        public void MainViewModel_OnStudioClosing_CallsWcfSourceOnClosing()
        {
            InitSourceViewModel(out Mock<IServer> environmentModel, out ShellViewModel mvm);
            var resourceModelMock = new Mock<IContextualResourceModel>();
            resourceModelMock.SetupGet(model => model.ServerResourceType).Returns("WcfSource");
            const string WorkFlowXaml = "<XamlDefinition>&lt;Activity x:Class=\"PBI 6690 - TEST\" xmlns=\"http://schemas.microsoft.com/netfx/2009/xaml/activities\" xmlns:av=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" xmlns:dc=\"clr-namespace:Dev2.Common;assembly=Dev2.CommonDataUtils\" xmlns:ddc=\"clr-namespace:Dev2.DataList.Contract;assembly=Dev2.Data\" xmlns:ddcb=\"clr-namespace:Dev2.DataList.Contract.Binary_Objects;assembly=Dev2.Data\" xmlns:ddd=\"clr-namespace:Dev2.Data.Decision;assembly=Dev2.Data\" xmlns:dddo=\"clr-namespace:Dev2.Data.Decisions.Operations;assembly=Dev2.Data\" xmlns:ddsm=\"clr-namespace:Dev2.Data.SystemTemplates.Models;assembly=Dev2.Data\" xmlns:mva=\"clr-namespace:Microsoft.VisualBasic.Activities;assembly=System.Activities\" xmlns:s=\"clr-namespace:System;assembly=mscorlib\" xmlns:sap=\"http://schemas.microsoft.com/netfx/2009/xaml/activities/presentation\" xmlns:scg=\"clr-namespace:System.Collections.Generic;assembly=mscorlib\" xmlns:sco=\"clr-namespace:System.Collections.ObjectModel;assembly=mscorlib\" xmlns:uaba=\"clr-namespace:Unlimited.Applications.BusinessDesignStudio.Activities;assembly=Dev2.Activities\" xmlns:uf=\"clr-namespace:Unlimited.Framework;assembly=Dev2.Core\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"&gt;&lt;x:Members&gt;&lt;x:Property Name=\"AmbientDataList\" Type=\"InOutArgument(scg:List(x:String))\" /&gt;&lt;x:Property Name=\"ParentWorkflowInstanceId\" Type=\"InOutArgument(s:Guid)\" /&gt;&lt;x:Property Name=\"ParentServiceName\" Type=\"InOutArgument(x:String)\" /&gt;&lt;/x:Members&gt;&lt;sap:VirtualizedContainerService.HintSize&gt;654,676&lt;/sap:VirtualizedContainerService.HintSize&gt;&lt;mva:VisualBasic.Settings&gt;Assembly references and imported namespaces serialized as XML namespaces&lt;/mva:VisualBasic.Settings&gt;&lt;TextExpression.NamespacesForImplementation&gt;&lt;scg:List x:TypeArguments=\"x:String\" Capacity=\"7\"&gt;&lt;x:String&gt;Dev2.CommonDataUtils&lt;/x:String&gt;&lt;x:String&gt;Dev2.Data.Decisions.Operations&lt;/x:String&gt;&lt;x:String&gt;Dev2.Data.SystemTemplates.Models&lt;/x:String&gt;&lt;x:String&gt;Dev2.DataList.Contract&lt;/x:String&gt;&lt;x:String&gt;Dev2.DataList.Contract.Binary_Objects&lt;/x:String&gt;&lt;x:String&gt;Unlimited.Framework&lt;/x:String&gt;&lt;x:String&gt;Unlimited.Applications.BusinessDesignStudio.Activities&lt;/x:String&gt;&lt;/scg:List&gt;&lt;/TextExpression.NamespacesForImplementation&gt;&lt;TextExpression.ReferencesForImplementation&gt;&lt;sco:Collection x:TypeArguments=\"AssemblyReference\"&gt;&lt;AssemblyReference&gt;Dev2.CommonDataUtils&lt;/AssemblyReference&gt;&lt;AssemblyReference&gt;Dev2.Data&lt;/AssemblyReference&gt;&lt;AssemblyReference&gt;Dev2.Core&lt;/AssemblyReference&gt;&lt;AssemblyReference&gt;Dev2.Activities&lt;/AssemblyReference&gt;&lt;/sco:Collection&gt;&lt;/TextExpression.ReferencesForImplementation&gt;&lt;Flowchart DisplayName=\"PBI 6690 - TEST\" sap:VirtualizedContainerService.HintSize=\"614,636\"&gt;&lt;Flowchart.Variables&gt;&lt;Variable x:TypeArguments=\"scg:List(x:String)\" Name=\"InstructionList\" /&gt;&lt;Variable x:TypeArguments=\"x:String\" Name=\"LastResult\" /&gt;&lt;Variable x:TypeArguments=\"x:Boolean\" Name=\"HasError\" /&gt;&lt;Variable x:TypeArguments=\"x:String\" Name=\"ExplicitDataList\" /&gt;&lt;Variable x:TypeArguments=\"x:Boolean\" Name=\"IsValid\" /&gt;&lt;Variable x:TypeArguments=\"uf:UnlimitedObject\" Name=\"d\" /&gt;&lt;Variable x:TypeArguments=\"uaba:Util\" Name=\"t\" /&gt;&lt;Variable x:TypeArguments=\"ddd:Dev2DataListDecisionHandler\" Name=\"Dev2DecisionHandler\" /&gt;&lt;/Flowchart.Variables&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=\"x:String, x:Object\"&gt;&lt;x:Boolean x:Key=\"IsExpanded\"&gt;False&lt;/x:Boolean&gt;&lt;av:Point x:Key=\"ShapeLocation\"&gt;270,2.5&lt;/av:Point&gt;&lt;av:Size x:Key=\"ShapeSize\"&gt;60,75&lt;/av:Size&gt;&lt;av:PointCollection x:Key=\"ConnectorLocation\"&gt;300,77.5 300,107.5 310,107.5 310,211&lt;/av:PointCollection&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;Flowchart.StartNode&gt;&lt;x:Reference&gt;__ReferenceID0&lt;/x:Reference&gt;&lt;/Flowchart.StartNode&gt;&lt;FlowStep x:Name=\"__ReferenceID0\"&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=\"x:String, x:Object\"&gt;&lt;av:Point x:Key=\"ShapeLocation\"&gt;185,211&lt;/av:Point&gt;&lt;av:Size x:Key=\"ShapeSize\"&gt;250,84&lt;/av:Size&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;uaba:DsfActivity ActionName=\"{x:Null}\" ActivityStateData=\"{x:Null}\" AuthorRoles=\"{x:Null}\" Category=\"{x:Null}\" Compiler=\"{x:Null}\" CurrentResult=\"{x:Null}\" DataObject=\"{x:Null}\" DataTags=\"{x:Null}\" ExplicitDataList=\"{x:Null}\" InputTransformation=\"{x:Null}\" OnResumeKeepList=\"{x:Null}\" ParentServiceID=\"{x:Null}\" ParentServiceName=\"{x:Null}\" ParentWorkflowInstanceId=\"{x:Null}\" ResultTransformation=\"{x:Null}\" ResultValidationExpression=\"{x:Null}\" ResultValidationRequiredTags=\"{x:Null}\" ScenarioID=\"{x:Null}\" ScopingObject=\"{x:Null}\" ServiceUri=\"{x:Null}\" SimulationOutput=\"{x:Null}\" Tags=\"{x:Null}\" AddMode=\"False\" DatabindRecursive=\"False\" DeferExecution=\"False\" DisplayName=\"PBI 6690 - TEST Inputs\" EnvironmentID=\"00000000-0000-0000-0000-000000000000\" FriendlySourceName=\"localhost\" HasError=\"[HasError]\" sap:VirtualizedContainerService.HintSize=\"250,84\" IconPath=\"pack://application:,,,/Warewolf Studio;component/images/Workflow-32.png\" InputMapping=\"&amp;lt;Inputs&amp;gt;&amp;lt;Input Name=&amp;quot;n1&amp;quot; Source=&amp;quot;&amp;quot; /&amp;gt;&amp;lt;Input Name=&amp;quot;n2&amp;quot; Source=&amp;quot;&amp;quot; /&amp;gt;&amp;lt;Input Name=&amp;quot;n3&amp;quot; Source=&amp;quot;&amp;quot; /&amp;gt;&amp;lt;/Inputs&amp;gt;\" InstructionList=\"[InstructionList]\" IsSimulationEnabled=\"False\" IsUIStep=\"False\" IsValid=\"[IsValid]\" IsWorkflow=\"True\" OnResumeClearAmbientDataList=\"False\" OnResumeClearTags=\"FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage\" OutputMapping=\"&amp;lt;Outputs&amp;gt;&amp;lt;Output Name=&amp;quot;result&amp;quot; MapsTo=&amp;quot;result&amp;quot; Value=&amp;quot;&amp;quot; /&amp;gt;&amp;lt;/Outputs&amp;gt;\" RemoveInputFromOutput=\"False\" ServiceName=\"PBI 6690 - TEST Inputs\" ServiceServer=\"00000000-0000-0000-0000-000000000000\" SimulationMode=\"OnDemand\" ToolboxFriendlyName=\"PBI 6690 - TEST Inputs\" Type=\"Workflow\" UniqueID=\"edadb62e-83f4-44bf-a260-7639d6b43169\"&gt;&lt;uaba:DsfActivity.AmbientDataList&gt;&lt;InOutArgument x:TypeArguments=\"scg:List(x:String)\" /&gt;&lt;/uaba:DsfActivity.AmbientDataList&gt;&lt;uaba:DsfActivity.HelpLink&gt;&lt;InArgument x:TypeArguments=\"x:String\"&gt;&lt;Literal x:TypeArguments=\"x:String\" Value=\"\" /&gt;&lt;/InArgument&gt;&lt;/uaba:DsfActivity.HelpLink&gt;&lt;uaba:DsfActivity.ParentInstanceID&gt;&lt;InOutArgument x:TypeArguments=\"x:String\" /&gt;&lt;/uaba:DsfActivity.ParentInstanceID&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=\"x:String, x:Object\"&gt;&lt;x:Boolean x:Key=\"IsExpanded\"&gt;True&lt;/x:Boolean&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;/uaba:DsfActivity&gt;&lt;/FlowStep&gt;&lt;FlowStep&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=\"x:String, x:Object\"&gt;&lt;av:Point x:Key=\"ShapeLocation\"&gt;145,390&lt;/av:Point&gt;&lt;av:Size x:Key=\"ShapeSize\"&gt;250,100&lt;/av:Size&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;uaba:DsfActivity ActivityStateData=\"{x:Null}\" AuthorRoles=\"{x:Null}\" Category=\"{x:Null}\" Compiler=\"{x:Null}\" CurrentResult=\"{x:Null}\" DataObject=\"{x:Null}\" DataTags=\"{x:Null}\" ExplicitDataList=\"{x:Null}\" HelpLink=\"{x:Null}\" InputTransformation=\"{x:Null}\" OnResumeKeepList=\"{x:Null}\" ParentServiceID=\"{x:Null}\" ParentServiceName=\"{x:Null}\" ParentWorkflowInstanceId=\"{x:Null}\" ResultTransformation=\"{x:Null}\" ResultValidationExpression=\"{x:Null}\" ResultValidationRequiredTags=\"{x:Null}\" ScenarioID=\"{x:Null}\" ScopingObject=\"{x:Null}\" ServiceUri=\"{x:Null}\" SimulationOutput=\"{x:Null}\" Tags=\"{x:Null}\" ActionName=\"dbo.spGetCountries\" AddMode=\"False\" DatabindRecursive=\"False\" DeferExecution=\"False\" DisplayName=\"Countries\" EnvironmentID=\"00000000-0000-0000-0000-000000000000\" FriendlySourceName=\"CitiesDb\" HasError=\"[HasError]\" sap:VirtualizedContainerService.HintSize=\"250,100\" IconPath=\"[Nothing]\" InputMapping=\"&amp;lt;Inputs&amp;gt;&amp;lt;Input Name=&amp;quot;Prefix&amp;quot; Source=&amp;quot;Prefix&amp;quot; /&amp;gt;&amp;lt;/Inputs&amp;gt;\" InstructionList=\"[InstructionList]\" IsSimulationEnabled=\"False\" IsUIStep=\"False\" IsValid=\"[IsValid]\" IsWorkflow=\"False\" OnResumeClearAmbientDataList=\"False\" OnResumeClearTags=\"FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage\" OutputMapping=\"&amp;lt;Outputs&amp;gt;&amp;lt;Output Name=&amp;quot;CountryID&amp;quot; MapsTo=&amp;quot;CountryID&amp;quot; Value=&amp;quot;[[GetCountries().CountryID]]&amp;quot; Recordset=&amp;quot;GetCountries&amp;quot; /&amp;gt;&amp;lt;Output Name=&amp;quot;Description&amp;quot; MapsTo=&amp;quot;Name&amp;quot; Value=&amp;quot;[[GetCountries().Name]]&amp;quot; Recordset=&amp;quot;GetCountries&amp;quot; /&amp;gt;&amp;lt;/Outputs&amp;gt;\" RemoveInputFromOutput=\"False\" ServiceName=\"Countries\" ServiceServer=\"00000000-0000-0000-0000-000000000000\" SimulationMode=\"OnDemand\" ToolboxFriendlyName=\"Countries\" Type=\"InvokeStoredProc\" UniqueID=\"33476822-9519-4ad5-8fee-1ae3b571115b\"&gt;&lt;uaba:DsfActivity.AmbientDataList&gt;&lt;InOutArgument x:TypeArguments=\"scg:List(x:String)\" /&gt;&lt;/uaba:DsfActivity.AmbientDataList&gt;&lt;uaba:DsfActivity.ParentInstanceID&gt;&lt;InOutArgument x:TypeArguments=\"x:String\" /&gt;&lt;/uaba:DsfActivity.ParentInstanceID&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=\"x:String, x:Object\"&gt;&lt;x:Boolean x:Key=\"IsExpanded\"&gt;True&lt;/x:Boolean&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;/uaba:DsfActivity&gt;&lt;/FlowStep&gt;&lt;/Flowchart&gt;&lt;/Activity&gt;</XamlDefinition>";
            resourceModelMock.SetupGet(model => model.WorkflowXaml).Returns(new StringBuilder(WorkFlowXaml));
            resourceModelMock.SetupGet(model => model.Environment.EnvironmentID).Returns(Guid.NewGuid);
            resourceModelMock.SetupGet(model => model.ID).Returns(Guid.NewGuid);
            resourceModelMock.SetupGet(model => model.ServerID).Returns(Guid.NewGuid);
            var worksurfaceContextManager = new WorksurfaceContextManager(false, _shellViewModel, new Mock<IServerRepository>().Object, new Mock<IViewFactory>().Object);

            _activeEnvironment.Setup(model => model.Name).Returns("localhost");
            //---------------Execute Test ----------------------
            worksurfaceContextManager.DisplayResourceWizard(resourceModelMock.Object);

            ServerRepository.Instance.ActiveServer = environmentModel.Object;
            var activetx = _shellViewModel.Items.ToList().First(i => i.WorkSurfaceViewModel.GetType().Name == "SourceViewModel`1");

            var vm = new WorkSurfaceContextViewModel(new EventAggregator(), new WorkSurfaceKey(), activetx.WorkSurfaceViewModel, new Mock<Common.Interfaces.Studio.Controller.IPopupController>().Object, (a, b, c) => { });
            var sourceVM = vm.WorkSurfaceViewModel as SourceViewModel<IWcfServerSource>;
            sourceVM.ViewModel.Item = new Mock<IWcfServerSource>().Object;
            mvm.Items.Add(vm);
            Assert.IsFalse(mvm.OnStudioClosing());
        }

        [TestMethod]
        [TestCategory("MainViewModel_OnStudioClosing")]
        [Owner("Pieter Terblanche")]
        public void MainViewModel_OnStudioClosing_CallsServerSourceOnClosing()
        {
            InitSourceViewModel(out Mock<IServer> environmentModel, out ShellViewModel mvm);
            var resourceModelMock = new Mock<IContextualResourceModel>();
            resourceModelMock.SetupGet(model => model.ServerResourceType).Returns("Dev2Server");
            const string WorkFlowXaml = "<XamlDefinition>&lt;Activity x:Class=\"PBI 6690 - TEST\" xmlns=\"http://schemas.microsoft.com/netfx/2009/xaml/activities\" xmlns:av=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" xmlns:dc=\"clr-namespace:Dev2.Common;assembly=Dev2.CommonDataUtils\" xmlns:ddc=\"clr-namespace:Dev2.DataList.Contract;assembly=Dev2.Data\" xmlns:ddcb=\"clr-namespace:Dev2.DataList.Contract.Binary_Objects;assembly=Dev2.Data\" xmlns:ddd=\"clr-namespace:Dev2.Data.Decision;assembly=Dev2.Data\" xmlns:dddo=\"clr-namespace:Dev2.Data.Decisions.Operations;assembly=Dev2.Data\" xmlns:ddsm=\"clr-namespace:Dev2.Data.SystemTemplates.Models;assembly=Dev2.Data\" xmlns:mva=\"clr-namespace:Microsoft.VisualBasic.Activities;assembly=System.Activities\" xmlns:s=\"clr-namespace:System;assembly=mscorlib\" xmlns:sap=\"http://schemas.microsoft.com/netfx/2009/xaml/activities/presentation\" xmlns:scg=\"clr-namespace:System.Collections.Generic;assembly=mscorlib\" xmlns:sco=\"clr-namespace:System.Collections.ObjectModel;assembly=mscorlib\" xmlns:uaba=\"clr-namespace:Unlimited.Applications.BusinessDesignStudio.Activities;assembly=Dev2.Activities\" xmlns:uf=\"clr-namespace:Unlimited.Framework;assembly=Dev2.Core\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"&gt;&lt;x:Members&gt;&lt;x:Property Name=\"AmbientDataList\" Type=\"InOutArgument(scg:List(x:String))\" /&gt;&lt;x:Property Name=\"ParentWorkflowInstanceId\" Type=\"InOutArgument(s:Guid)\" /&gt;&lt;x:Property Name=\"ParentServiceName\" Type=\"InOutArgument(x:String)\" /&gt;&lt;/x:Members&gt;&lt;sap:VirtualizedContainerService.HintSize&gt;654,676&lt;/sap:VirtualizedContainerService.HintSize&gt;&lt;mva:VisualBasic.Settings&gt;Assembly references and imported namespaces serialized as XML namespaces&lt;/mva:VisualBasic.Settings&gt;&lt;TextExpression.NamespacesForImplementation&gt;&lt;scg:List x:TypeArguments=\"x:String\" Capacity=\"7\"&gt;&lt;x:String&gt;Dev2.CommonDataUtils&lt;/x:String&gt;&lt;x:String&gt;Dev2.Data.Decisions.Operations&lt;/x:String&gt;&lt;x:String&gt;Dev2.Data.SystemTemplates.Models&lt;/x:String&gt;&lt;x:String&gt;Dev2.DataList.Contract&lt;/x:String&gt;&lt;x:String&gt;Dev2.DataList.Contract.Binary_Objects&lt;/x:String&gt;&lt;x:String&gt;Unlimited.Framework&lt;/x:String&gt;&lt;x:String&gt;Unlimited.Applications.BusinessDesignStudio.Activities&lt;/x:String&gt;&lt;/scg:List&gt;&lt;/TextExpression.NamespacesForImplementation&gt;&lt;TextExpression.ReferencesForImplementation&gt;&lt;sco:Collection x:TypeArguments=\"AssemblyReference\"&gt;&lt;AssemblyReference&gt;Dev2.CommonDataUtils&lt;/AssemblyReference&gt;&lt;AssemblyReference&gt;Dev2.Data&lt;/AssemblyReference&gt;&lt;AssemblyReference&gt;Dev2.Core&lt;/AssemblyReference&gt;&lt;AssemblyReference&gt;Dev2.Activities&lt;/AssemblyReference&gt;&lt;/sco:Collection&gt;&lt;/TextExpression.ReferencesForImplementation&gt;&lt;Flowchart DisplayName=\"PBI 6690 - TEST\" sap:VirtualizedContainerService.HintSize=\"614,636\"&gt;&lt;Flowchart.Variables&gt;&lt;Variable x:TypeArguments=\"scg:List(x:String)\" Name=\"InstructionList\" /&gt;&lt;Variable x:TypeArguments=\"x:String\" Name=\"LastResult\" /&gt;&lt;Variable x:TypeArguments=\"x:Boolean\" Name=\"HasError\" /&gt;&lt;Variable x:TypeArguments=\"x:String\" Name=\"ExplicitDataList\" /&gt;&lt;Variable x:TypeArguments=\"x:Boolean\" Name=\"IsValid\" /&gt;&lt;Variable x:TypeArguments=\"uf:UnlimitedObject\" Name=\"d\" /&gt;&lt;Variable x:TypeArguments=\"uaba:Util\" Name=\"t\" /&gt;&lt;Variable x:TypeArguments=\"ddd:Dev2DataListDecisionHandler\" Name=\"Dev2DecisionHandler\" /&gt;&lt;/Flowchart.Variables&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=\"x:String, x:Object\"&gt;&lt;x:Boolean x:Key=\"IsExpanded\"&gt;False&lt;/x:Boolean&gt;&lt;av:Point x:Key=\"ShapeLocation\"&gt;270,2.5&lt;/av:Point&gt;&lt;av:Size x:Key=\"ShapeSize\"&gt;60,75&lt;/av:Size&gt;&lt;av:PointCollection x:Key=\"ConnectorLocation\"&gt;300,77.5 300,107.5 310,107.5 310,211&lt;/av:PointCollection&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;Flowchart.StartNode&gt;&lt;x:Reference&gt;__ReferenceID0&lt;/x:Reference&gt;&lt;/Flowchart.StartNode&gt;&lt;FlowStep x:Name=\"__ReferenceID0\"&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=\"x:String, x:Object\"&gt;&lt;av:Point x:Key=\"ShapeLocation\"&gt;185,211&lt;/av:Point&gt;&lt;av:Size x:Key=\"ShapeSize\"&gt;250,84&lt;/av:Size&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;uaba:DsfActivity ActionName=\"{x:Null}\" ActivityStateData=\"{x:Null}\" AuthorRoles=\"{x:Null}\" Category=\"{x:Null}\" Compiler=\"{x:Null}\" CurrentResult=\"{x:Null}\" DataObject=\"{x:Null}\" DataTags=\"{x:Null}\" ExplicitDataList=\"{x:Null}\" InputTransformation=\"{x:Null}\" OnResumeKeepList=\"{x:Null}\" ParentServiceID=\"{x:Null}\" ParentServiceName=\"{x:Null}\" ParentWorkflowInstanceId=\"{x:Null}\" ResultTransformation=\"{x:Null}\" ResultValidationExpression=\"{x:Null}\" ResultValidationRequiredTags=\"{x:Null}\" ScenarioID=\"{x:Null}\" ScopingObject=\"{x:Null}\" ServiceUri=\"{x:Null}\" SimulationOutput=\"{x:Null}\" Tags=\"{x:Null}\" AddMode=\"False\" DatabindRecursive=\"False\" DeferExecution=\"False\" DisplayName=\"PBI 6690 - TEST Inputs\" EnvironmentID=\"00000000-0000-0000-0000-000000000000\" FriendlySourceName=\"localhost\" HasError=\"[HasError]\" sap:VirtualizedContainerService.HintSize=\"250,84\" IconPath=\"pack://application:,,,/Warewolf Studio;component/images/Workflow-32.png\" InputMapping=\"&amp;lt;Inputs&amp;gt;&amp;lt;Input Name=&amp;quot;n1&amp;quot; Source=&amp;quot;&amp;quot; /&amp;gt;&amp;lt;Input Name=&amp;quot;n2&amp;quot; Source=&amp;quot;&amp;quot; /&amp;gt;&amp;lt;Input Name=&amp;quot;n3&amp;quot; Source=&amp;quot;&amp;quot; /&amp;gt;&amp;lt;/Inputs&amp;gt;\" InstructionList=\"[InstructionList]\" IsSimulationEnabled=\"False\" IsUIStep=\"False\" IsValid=\"[IsValid]\" IsWorkflow=\"True\" OnResumeClearAmbientDataList=\"False\" OnResumeClearTags=\"FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage\" OutputMapping=\"&amp;lt;Outputs&amp;gt;&amp;lt;Output Name=&amp;quot;result&amp;quot; MapsTo=&amp;quot;result&amp;quot; Value=&amp;quot;&amp;quot; /&amp;gt;&amp;lt;/Outputs&amp;gt;\" RemoveInputFromOutput=\"False\" ServiceName=\"PBI 6690 - TEST Inputs\" ServiceServer=\"00000000-0000-0000-0000-000000000000\" SimulationMode=\"OnDemand\" ToolboxFriendlyName=\"PBI 6690 - TEST Inputs\" Type=\"Workflow\" UniqueID=\"edadb62e-83f4-44bf-a260-7639d6b43169\"&gt;&lt;uaba:DsfActivity.AmbientDataList&gt;&lt;InOutArgument x:TypeArguments=\"scg:List(x:String)\" /&gt;&lt;/uaba:DsfActivity.AmbientDataList&gt;&lt;uaba:DsfActivity.HelpLink&gt;&lt;InArgument x:TypeArguments=\"x:String\"&gt;&lt;Literal x:TypeArguments=\"x:String\" Value=\"\" /&gt;&lt;/InArgument&gt;&lt;/uaba:DsfActivity.HelpLink&gt;&lt;uaba:DsfActivity.ParentInstanceID&gt;&lt;InOutArgument x:TypeArguments=\"x:String\" /&gt;&lt;/uaba:DsfActivity.ParentInstanceID&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=\"x:String, x:Object\"&gt;&lt;x:Boolean x:Key=\"IsExpanded\"&gt;True&lt;/x:Boolean&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;/uaba:DsfActivity&gt;&lt;/FlowStep&gt;&lt;FlowStep&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=\"x:String, x:Object\"&gt;&lt;av:Point x:Key=\"ShapeLocation\"&gt;145,390&lt;/av:Point&gt;&lt;av:Size x:Key=\"ShapeSize\"&gt;250,100&lt;/av:Size&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;uaba:DsfActivity ActivityStateData=\"{x:Null}\" AuthorRoles=\"{x:Null}\" Category=\"{x:Null}\" Compiler=\"{x:Null}\" CurrentResult=\"{x:Null}\" DataObject=\"{x:Null}\" DataTags=\"{x:Null}\" ExplicitDataList=\"{x:Null}\" HelpLink=\"{x:Null}\" InputTransformation=\"{x:Null}\" OnResumeKeepList=\"{x:Null}\" ParentServiceID=\"{x:Null}\" ParentServiceName=\"{x:Null}\" ParentWorkflowInstanceId=\"{x:Null}\" ResultTransformation=\"{x:Null}\" ResultValidationExpression=\"{x:Null}\" ResultValidationRequiredTags=\"{x:Null}\" ScenarioID=\"{x:Null}\" ScopingObject=\"{x:Null}\" ServiceUri=\"{x:Null}\" SimulationOutput=\"{x:Null}\" Tags=\"{x:Null}\" ActionName=\"dbo.spGetCountries\" AddMode=\"False\" DatabindRecursive=\"False\" DeferExecution=\"False\" DisplayName=\"Countries\" EnvironmentID=\"00000000-0000-0000-0000-000000000000\" FriendlySourceName=\"CitiesDb\" HasError=\"[HasError]\" sap:VirtualizedContainerService.HintSize=\"250,100\" IconPath=\"[Nothing]\" InputMapping=\"&amp;lt;Inputs&amp;gt;&amp;lt;Input Name=&amp;quot;Prefix&amp;quot; Source=&amp;quot;Prefix&amp;quot; /&amp;gt;&amp;lt;/Inputs&amp;gt;\" InstructionList=\"[InstructionList]\" IsSimulationEnabled=\"False\" IsUIStep=\"False\" IsValid=\"[IsValid]\" IsWorkflow=\"False\" OnResumeClearAmbientDataList=\"False\" OnResumeClearTags=\"FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage\" OutputMapping=\"&amp;lt;Outputs&amp;gt;&amp;lt;Output Name=&amp;quot;CountryID&amp;quot; MapsTo=&amp;quot;CountryID&amp;quot; Value=&amp;quot;[[GetCountries().CountryID]]&amp;quot; Recordset=&amp;quot;GetCountries&amp;quot; /&amp;gt;&amp;lt;Output Name=&amp;quot;Description&amp;quot; MapsTo=&amp;quot;Name&amp;quot; Value=&amp;quot;[[GetCountries().Name]]&amp;quot; Recordset=&amp;quot;GetCountries&amp;quot; /&amp;gt;&amp;lt;/Outputs&amp;gt;\" RemoveInputFromOutput=\"False\" ServiceName=\"Countries\" ServiceServer=\"00000000-0000-0000-0000-000000000000\" SimulationMode=\"OnDemand\" ToolboxFriendlyName=\"Countries\" Type=\"InvokeStoredProc\" UniqueID=\"33476822-9519-4ad5-8fee-1ae3b571115b\"&gt;&lt;uaba:DsfActivity.AmbientDataList&gt;&lt;InOutArgument x:TypeArguments=\"scg:List(x:String)\" /&gt;&lt;/uaba:DsfActivity.AmbientDataList&gt;&lt;uaba:DsfActivity.ParentInstanceID&gt;&lt;InOutArgument x:TypeArguments=\"x:String\" /&gt;&lt;/uaba:DsfActivity.ParentInstanceID&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=\"x:String, x:Object\"&gt;&lt;x:Boolean x:Key=\"IsExpanded\"&gt;True&lt;/x:Boolean&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;/uaba:DsfActivity&gt;&lt;/FlowStep&gt;&lt;/Flowchart&gt;&lt;/Activity&gt;</XamlDefinition>";
            resourceModelMock.SetupGet(model => model.WorkflowXaml).Returns(new StringBuilder(WorkFlowXaml));
            resourceModelMock.SetupGet(model => model.Environment.EnvironmentID).Returns(Guid.NewGuid);
            resourceModelMock.SetupGet(model => model.ID).Returns(Guid.NewGuid);
            resourceModelMock.SetupGet(model => model.ServerID).Returns(Guid.NewGuid);
            var mock = new Mock<IServerRepository>();
            mock.Setup(repository => repository.ActiveServer).Returns(environmentModel.Object);
            CustomContainer.Register(mock.Object);
            var worksurfaceContextManager = new WorksurfaceContextManager(false, _shellViewModel, mock.Object, new Mock<IViewFactory>().Object);

            _activeEnvironment.Setup(model => model.Name).Returns("localhost");
            //---------------Execute Test ----------------------
            worksurfaceContextManager.DisplayResourceWizard(resourceModelMock.Object);

            var activetx = _shellViewModel.Items.ToList().First(i => i.WorkSurfaceViewModel.GetType().Name == "SourceViewModel`1");

            var vm = new WorkSurfaceContextViewModel(new EventAggregator(), new WorkSurfaceKey(), activetx.WorkSurfaceViewModel, new Mock<Common.Interfaces.Studio.Controller.IPopupController>().Object, (a, b, c) => { });
            var sourceVM = vm.WorkSurfaceViewModel as SourceViewModel<IServerSource>;
            sourceVM.ViewModel.Item = new Mock<IServerSource>().Object;
            mvm.Items.Add(vm);
            Assert.IsFalse(mvm.OnStudioClosing());
        }

        #endregion

        static ExecuteMessage MakeMsg(string msg)
        {
            var result = new ExecuteMessage { HasError = false };
            result.SetMessage(msg);
            return result;
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("MainViewModel_HandleAddWorkSurfaceMessage")]
        public void MainViewModel_HandleAddWorkSurfaceMessage_ShowDebugWindowOnLoadIsTrue_DoesExecuteDebugCommand()
        {
            CustomContainer.Register(new Mock<IShellViewModel>().Object);
            Verify_HandleAddWorkSurfaceMessage_ShowDebugWindowOnLoad(true);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("MainViewModel_HandleAddWorkSurfaceMessage")]
        public void MainViewModel_HandleAddWorkSurfaceMessage_ShowDebugWindowOnLoadIsFalse_DoesNotExecuteDebugCommand()
        {
            CustomContainer.Register(new Mock<IShellViewModel>().Object);
            Verify_HandleAddWorkSurfaceMessage_ShowDebugWindowOnLoad(false);
        }

        static void Verify_HandleAddWorkSurfaceMessage_ShowDebugWindowOnLoad(bool showDebugWindowOnLoad)
        {
            //------------Setup for test--------------------------
            var mockEnvironmentConnection = SetupMockConnection();
            var eventAggregator = new Mock<IEventAggregator>();

            var envRepo = new Mock<IServerRepository>();
            var serverMock = new Mock<IServer>();
            serverMock.Setup(it => it.Connection).Returns(mockEnvironmentConnection.Object);
            envRepo.Setup(e => e.All()).Returns(new List<IServer>());
            envRepo.Setup(e => e.Source).Returns(serverMock.Object);

            var vieFactory = new Mock<IViewFactory>();
            var viewMock = new Mock<IView>();
            vieFactory.Setup(factory => factory.GetViewGivenServerResourceType(It.IsAny<string>())).Returns(viewMock.Object);

            var vm = new ShellViewModel(eventAggregator.Object, new SynchronousAsyncWorker(), envRepo.Object, new Mock<IVersionChecker>().Object, vieFactory.Object, false, new Mock<IBrowserPopupController>().Object);

            var workSurfaceContextViewModel = new Mock<WorkSurfaceContextViewModel>(eventAggregator.Object, new WorkSurfaceKey(), new Mock<IWorkSurfaceViewModel>().Object, new Mock<Common.Interfaces.Studio.Controller.IPopupController>().Object, new Action<IContextualResourceModel, bool, System.Action>(
                (a, b, c) => { }));
            workSurfaceContextViewModel.Setup(v => v.Debug()).Verifiable();

            vm.ActiveItem = workSurfaceContextViewModel.Object;

            //------------Execute Test---------------------------
            vm.Handle(new AddWorkSurfaceMessage(new Mock<IWorkSurfaceObject>().Object) { ShowDebugWindowOnLoad = showDebugWindowOnLoad });

            //------------Assert Results-------------------------
            workSurfaceContextViewModel.Verify(v => v.Debug(), showDebugWindowOnLoad ? Times.Once() : Times.Never());

        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("MainViewModel_AddWorkSurfaceContext")]
        public void MainViewModel_AddWorkSurfaceContext_AddResourceFromServer_ExpectIsSavedTrue()
        {
            var resourceId = Guid.NewGuid();
            var serverId = Guid.NewGuid();
            var environmentId = Guid.NewGuid();
            var workspaceId = Guid.NewGuid();

            var resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(r => r.ResourceType).Returns(ResourceType.WorkflowService);
            resourceModel.SetupGet(r => r.ID).Returns(resourceId);
            resourceModel.SetupGet(r => r.ServerID).Returns(serverId);
            resourceModel.SetupGet(r => r.ResourceName).Returns("My_Resource_Name");

            var environmentModel = new Mock<IServer>();
            environmentModel.SetupGet(e => e.EnvironmentID).Returns(environmentId);

            var environmentConnection = new Mock<IEnvironmentConnection>();
            environmentConnection.SetupGet(env => env.WorkspaceID).Returns(workspaceId);

            environmentModel.SetupGet(e => e.Connection).Returns(environmentConnection.Object);

            var resourceRepository = new Mock<IResourceRepository>();

            environmentModel.SetupGet(e => e.ResourceRepository).Returns(resourceRepository.Object);
            resourceModel.SetupGet(r => r.Environment).Returns(environmentModel.Object);

            var environmentRepository = new Mock<IServerRepository>();
            environmentModel.Setup(c => c.CanStudioExecute).Returns(false);
            environmentRepository.Setup(c => c.Source).Returns(environmentModel.Object);
            environmentRepository.Setup(c => c.All()).Returns(new[] { environmentModel.Object });

        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("MainViewModel_AddWorkSurfaceContext")]
        public void MainViewModel_AddWorkSurfaceContext_AddResourceFromWorkspace_ExpectIsSavedValueSameAsWhenPassedIn()
        {
            var resourceId = Guid.NewGuid();
            var serverId = Guid.NewGuid();
            var environmentId = Guid.NewGuid();
            var workspaceId = Guid.NewGuid();

            var resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(r => r.ResourceType).Returns(ResourceType.WorkflowService);
            resourceModel.SetupGet(r => r.ID).Returns(resourceId);
            resourceModel.SetupGet(r => r.ServerID).Returns(serverId);
            resourceModel.SetupGet(r => r.ResourceName).Returns("My_Resource_Name");

            var environmentModel = new Mock<IServer>();
            environmentModel.SetupGet(e => e.EnvironmentID).Returns(environmentId);

            var environmentConnection = new Mock<IEnvironmentConnection>();
            environmentConnection.SetupGet(env => env.WorkspaceID).Returns(workspaceId);

            environmentModel.SetupGet(e => e.Connection).Returns(environmentConnection.Object);

            var resourceRepository = new Mock<IResourceRepository>();

            environmentModel.SetupGet(e => e.ResourceRepository).Returns(resourceRepository.Object);
            resourceModel.SetupGet(r => r.Environment).Returns(environmentModel.Object);

            var environmentRepository = new Mock<IServerRepository>();
            environmentModel.Setup(c => c.CanStudioExecute).Returns(false);
            environmentRepository.Setup(c => c.Source).Returns(environmentModel.Object);
            environmentRepository.Setup(c => c.All()).Returns(new[] { environmentModel.Object });

        }

        [TestMethod]
        [TestCategory("MainViewModel_IsDownloading")]
        [Owner("Tshepo Ntlhokoa")]
        public void MainViewModel_IsDownloading_IsBusyDownloadingInstallerIsNull_False()
        {
            //------------Setup for test--------------------------
            var localhost = new Mock<IServer>();
            localhost.Setup(e => e.EnvironmentID).Returns(Guid.Empty);
            localhost.Setup(e => e.IsConnected).Returns(true); // so that we load resources

            var mockEnvironmentConnection = SetupMockConnection();
            localhost.SetupGet(it => it.Connection).Returns(mockEnvironmentConnection.Object);

            var environmentRepository = new Mock<IServerRepository>();
            CustomContainer.DeRegister<IServerRepository>();
            CustomContainer.Register(environmentRepository.Object);
            environmentRepository.Setup(c => c.All()).Returns(new[] { localhost.Object });
            environmentRepository.Setup(c => c.Source).Returns(localhost.Object);
            var eventPublisher = new Mock<IEventAggregator>();
            var asyncWorker = AsyncWorkerTests.CreateSynchronousAsyncWorker();
            var versionChecker = new Mock<IVersionChecker>();
            var browserPopupController = new Mock<IBrowserPopupController>();
            var vieFactory = new Mock<IViewFactory>();
            var viewMock = new Mock<IView>();
            vieFactory.Setup(factory => factory.GetViewGivenServerResourceType(It.IsAny<string>())).Returns(viewMock.Object);
            var viewModel = new ShellViewModelMock(eventPublisher.Object, asyncWorker.Object, environmentRepository.Object, versionChecker.Object, vieFactory.Object, false, browserPopupController.Object);

            //------------Execute Test---------------------------
            var isDownloading = ShellViewModel.IsDownloading();
            //------------Assert Results-------------------------
            Assert.IsFalse(isDownloading);
        }

        [TestMethod]
        [TestCategory("MainViewModel_IsDownloading")]
        [Owner("Tshepo Ntlhokoa")]
        public void MainViewModel_IsDownloading_IsBusyDownloadingInstallerReturnsFalse_False()
        {
            //------------Setup for test--------------------------
            var localhost = new Mock<IServer>();
            localhost.Setup(e => e.EnvironmentID).Returns(Guid.Empty);
            localhost.Setup(e => e.IsConnected).Returns(true); // so that we load resources

            var mockEnvironmentConnection = SetupMockConnection();
            localhost.SetupGet(it => it.Connection).Returns(mockEnvironmentConnection.Object);

            var environmentRepository = new Mock<IServerRepository>();
            environmentRepository.Setup(c => c.All()).Returns(new[] { localhost.Object });
            environmentRepository.Setup(c => c.Source).Returns(localhost.Object);
            var eventPublisher = new Mock<IEventAggregator>();
            var asyncWorker = AsyncWorkerTests.CreateSynchronousAsyncWorker();
            var versionChecker = new Mock<IVersionChecker>();
            var browserPopupController = new Mock<IBrowserPopupController>();
            var vieFactory = new Mock<IViewFactory>();
            var viewMock = new Mock<IView>();
            vieFactory.Setup(factory => factory.GetViewGivenServerResourceType(It.IsAny<string>())).Returns(viewMock.Object);
            var viewModel = new ShellViewModelMock(eventPublisher.Object, asyncWorker.Object, environmentRepository.Object, versionChecker.Object, vieFactory.Object, false, browserPopupController.Object);
            //------------Execute Test---------------------------
            var isDownloading = ShellViewModel.IsDownloading();
            //------------Assert Results-------------------------
            Assert.IsFalse(isDownloading);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CopyUrlLink_GivenresourceIdAndServer_ShouldLoadResourceModel()
        {
            //---------------Set up test pack-------------------

            CreateFullExportsAndVm();
            _environmentModel.Setup(model => model.ResourceRepository.LoadContextualResourceModel(It.IsAny<Guid>()));
            //---------------Assert Precondition----------------
            Assert.IsNotNull(_shellViewModel);
            //---------------Execute Test ----------------------
            _shellViewModel.CopyUrlLink(Guid.Empty, _shellViewModel.ActiveServer);
            //---------------Test Result -----------------------
            _environmentModel.Verify(model => model.ResourceRepository.LoadContextualResourceModel(It.IsAny<Guid>()));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CreateNewSchedule_GivenresourceIdAndServer_ShouldLoadResourceModel()
        {
            //---------------Set up test pack-------------------

            CreateFullExportsAndVm();
            _environmentModel.Setup(model => model.ResourceRepository.LoadContextualResourceModel(It.IsAny<Guid>()));
            //---------------Assert Precondition----------------
            Assert.IsNotNull(_shellViewModel);
            //---------------Execute Test ----------------------
            _shellViewModel.CreateNewSchedule(Guid.Empty);
            //---------------Test Result -----------------------
            _environmentModel.Verify(model => model.ResourceRepository.LoadContextualResourceModel(It.IsAny<Guid>()));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        public void MainViewModel_CreateNewQueueEvent_GivenresourceIdAndServer_ShouldLoadResourceModel()
        {
            //---------------Set up test pack-------------------

            CreateFullExportsAndVm();
            _environmentModel.Setup(model => model.ResourceRepository.LoadContextualResourceModel(It.IsAny<Guid>()));
            //---------------Assert Precondition----------------
            Assert.IsNotNull(_shellViewModel);
            //---------------Execute Test ----------------------
            _shellViewModel.CreateNewQueueEvent(Guid.Empty);
            //---------------Test Result -----------------------
            _environmentModel.Verify(model => model.ResourceRepository.LoadContextualResourceModel(It.IsAny<Guid>()));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void SetRefreshExplorerState_GivenTrue_ShouldSetExplorerStateCorrectly()
        {
            //---------------Set up test pack-------------------
            var explorer = new Mock<IExplorerViewModel>();
            explorer.SetupProperty(model => model.IsRefreshing);
            CreateFullExportsAndVm(explorer.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(_shellViewModel);
            Assert.IsNotNull(_shellViewModel.ExplorerViewModel);
            //---------------Execute Test ----------------------
            _shellViewModel.SetRefreshExplorerState(true);
            //---------------Test Result -----------------------
            Assert.IsTrue(_shellViewModel.ExplorerViewModel.IsRefreshing);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void BrowserDebug_GivenId_ShouldLoadResourceModel()
        {
            //---------------Set up test pack-------------------

            CreateFullExportsAndVm();
            var pv = new PrivateObject(_shellViewModel);
            var resourceModel = new Mock<IContextualResourceModel>();

            var wcm = new Mock<IWorksurfaceContextManager>();
            wcm.Setup(manager => manager.DisplayResourceWizard(resourceModel.Object));
            _environmentModel.Setup(model => model.ResourceRepository.LoadContextualResourceModel(It.IsAny<Guid>()))
                .Returns(resourceModel.Object);
            pv.SetField("_worksurfaceContextManager", BindingFlags.Instance | BindingFlags.NonPublic, wcm.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(_shellViewModel);
            try
            {
                //---------------Execute Test ----------------------
                _shellViewModel.BrowserDebug(Guid.Empty, _shellViewModel.ActiveServer);
                Assert.Fail();
            }
            catch (NullReferenceException)//Actual Quick debug fails
            {
                //---------------Test Result -----------------------
                _environmentModel.Verify(model => model.ResourceRepository.LoadContextualResourceModel(It.IsAny<Guid>()));
                wcm.Verify(manager => manager.DisplayResourceWizard(resourceModel.Object));
            }
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void NewComPluginSource_GivenPath_ShouldOpenSurfaceWithCorrectPath()
        {
            //---------------Set up test pack-------------------

            CreateFullExportsAndVm();
            var pv = new PrivateObject(_shellViewModel);
            var resourceModel = new Mock<IContextualResourceModel>();

            var wcm = new Mock<IWorksurfaceContextManager>();
            wcm.Setup(manager => manager.NewComPluginSource("path"));
            pv.SetField("_worksurfaceContextManager", BindingFlags.Instance | BindingFlags.NonPublic, wcm.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(_shellViewModel);

            //---------------Execute Test ----------------------
            _shellViewModel.NewComPluginSource("path");
            //---------------Test Result -----------------------
            wcm.VerifyAll();
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void AddDeploySurface_GivenExplorereItems_ShouldOpenSurfaceWithCorrectExplorereItems()
        {
            //---------------Set up test pack-------------------

            CreateFullExportsAndVm();
            var pv = new PrivateObject(_shellViewModel);

            var wcm = new Mock<IWorksurfaceContextManager>();
            IEnumerable<IExplorerTreeItem> enumerable = new List<IExplorerTreeItem>();
            wcm.Setup(manager => manager.AddDeploySurface(enumerable));
            pv.SetField("_worksurfaceContextManager", BindingFlags.Instance | BindingFlags.NonPublic, wcm.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(_shellViewModel);

            //---------------Execute Test ----------------------
            _shellViewModel.AddDeploySurface(enumerable);
            //---------------Test Result -----------------------
            wcm.VerifyAll();
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void OpenVersion_GivenVersionInfo_ShouldOpenSurfaceWithCorrectVersion()
        {
            //---------------Set up test pack-------------------
            CreateFullExportsAndVm();
            var pv = new PrivateObject(_shellViewModel);

            var wcm = new Mock<IWorksurfaceContextManager>();
            IVersionInfo version = new VersionInfo();
            wcm.Setup(manager => manager.OpenVersion(Guid.Empty, version));
            pv.SetField("_worksurfaceContextManager", BindingFlags.Instance | BindingFlags.NonPublic, wcm.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(_shellViewModel);

            //---------------Execute Test ----------------------
            _shellViewModel.OpenVersion(Guid.Empty, version);
            //---------------Test Result -----------------------
            wcm.VerifyAll();
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void StudioDebug_GivenId_ShouldLoadResourceModel()
        {
            //---------------Set up test pack-------------------
            CreateFullExportsAndVm();
            var pv = new PrivateObject(_shellViewModel);
            var resourceModel = new Mock<IContextualResourceModel>();

            var wcm = new Mock<IWorksurfaceContextManager>();
            wcm.Setup(manager => manager.DisplayResourceWizard(resourceModel.Object));
            _environmentModel.Setup(model => model.ResourceRepository.LoadContextualResourceModel(It.IsAny<Guid>()))
                .Returns(resourceModel.Object);
            pv.SetField("_worksurfaceContextManager", BindingFlags.Instance | BindingFlags.NonPublic, wcm.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(_shellViewModel);
            try
            {
                //---------------Execute Test ----------------------
                _shellViewModel.StudioDebug(Guid.Empty, _shellViewModel.ActiveServer);
                Assert.Fail();
            }
            catch (NullReferenceException)//Actual Quick debug fails
            {
                //---------------Test Result -----------------------
                _environmentModel.Verify(model => model.ResourceRepository.LoadContextualResourceModel(It.IsAny<Guid>()));
                wcm.Verify(manager => manager.DisplayResourceWizard(resourceModel.Object));
            }
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void NewSchedule_GivenId_ShouldLoadResourceModel()
        {
            //---------------Set up test pack-------------------

            CreateFullExportsAndVm();
            var pv = new PrivateObject(_shellViewModel);
            var resourceModel = new Mock<IContextualResourceModel>();

            var wcm = new Mock<IWorksurfaceContextManager>();
            wcm.Setup(manager => manager.TryCreateNewScheduleWorkSurface(resourceModel.Object));
            _environmentModel.Setup(model => model.ResourceRepository.LoadContextualResourceModel(It.IsAny<Guid>()))
                .Returns(resourceModel.Object);
            pv.SetField("_worksurfaceContextManager", BindingFlags.Instance | BindingFlags.NonPublic, wcm.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(_shellViewModel);

            //---------------Execute Test ----------------------
            _shellViewModel.NewSchedule(Guid.Empty);
            //---------------Test Result -----------------------
            _environmentModel.Verify(model => model.ResourceRepository.LoadContextualResourceModel(It.IsAny<Guid>()));
            wcm.VerifyAll();
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        public void MainViewModel_NewQueueEvent_GivenId_ShouldLoadResourceModel()
        {
            //---------------Set up test pack-------------------

            CreateFullExportsAndVm();
            var pv = new PrivateObject(_shellViewModel);
            var resourceModel = new Mock<IContextualResourceModel>();

            var wcm = new Mock<IWorksurfaceContextManager>();
            wcm.Setup(manager => manager.TryCreateNewQueueEventWorkSurface(resourceModel.Object));
            _environmentModel.Setup(model => model.ResourceRepository.LoadContextualResourceModel(It.IsAny<Guid>()))
                .Returns(resourceModel.Object);
            pv.SetField("_worksurfaceContextManager", BindingFlags.Instance | BindingFlags.NonPublic, wcm.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(_shellViewModel);

            //---------------Execute Test ----------------------
            _shellViewModel.NewQueueEvent(Guid.Empty);
            //---------------Test Result -----------------------
            _environmentModel.Verify(model => model.ResourceRepository.LoadContextualResourceModel(It.IsAny<Guid>()));
            wcm.VerifyAll();
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void OpenResourceAsync_GivenresourceIdAndServer_ShouldLoadResourceModel()
        {
            //---------------Set up test pack-------------------

            CreateFullExportsAndVm();
            var resource = CreateResource(ResourceType.WorkflowService);
            _environmentModel
                .Setup(server => server.ResourceRepository.LoadContextualResourceModelAsync(It.IsAny<Guid>()))
                .ReturnsAsync(resource.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(_shellViewModel);
            //---------------Execute Test ----------------------

            var task = Task.Run(() => { _shellViewModel.OpenResourceAsync(Guid.Empty, _shellViewModel.ActiveServer); });
            task.ContinueWith(task1 =>
            {
                //---------------Test Result -----------------------
                _environmentModel.Verify(model => model.ResourceRepository.LoadContextualResourceModelAsync(It.IsAny<Guid>()));
            });
            task.Wait();
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ShowServerDisconnectedPopup_GivenresourceIdAndServer_ShouldLoadResourceModel()
        {
            //---------------Set up test pack-------------------

            CreateFullExportsAndVm();
            var mock = new Mock<Common.Interfaces.Studio.Controller.IPopupController>();
            mock.Setup(controller => controller.Show(It.IsAny<string>(), Warewolf.Studio.Resources.Languages.Core.ServerDisconnectedHeader, MessageBoxButton.OK, MessageBoxImage.Error, "", false, true, false, false, false, false));

            CustomContainer.Register(mock.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(_shellViewModel);
            //---------------Execute Test ----------------------
            var mock1 = new Mock<IServer>();
            mock1.Setup(se => se.Name).Returns("a");
            mock1.Setup(se => se.DisplayName).Returns("a");
            _shellViewModel.ActiveServer = mock1.Object;
            var po = new PrivateObject(_shellViewModel);
            po.Invoke("ShowServerDisconnectedPopup");
            //---------------Test Result -----------------------
            mock.VerifyAll();
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DuplicateResource_GivenNotConnected_ShouldPopup()
        {
            //---------------Set up test pack-------------------
            CreateFullExportsAndVm();
            var mock = new Mock<Common.Interfaces.Studio.Controller.IPopupController>();
            mock.Setup(controller => controller.Show(It.IsAny<string>(), Warewolf.Studio.Resources.Languages.Core.ServerDisconnectedHeader, MessageBoxButton.OK, MessageBoxImage.Error, "", false, true, false, false, false, false));
            CustomContainer.Register(mock.Object);
            var explorerVm = new Mock<IExplorerItemViewModel>();

            //---------------Assert Precondition----------------
            Assert.IsNotNull(_shellViewModel);
            var mock1 = new Mock<IServer>();
            mock1.Setup(se => se.Name).Returns("a");
            mock1.Setup(se => se.DisplayName).Returns("a");
            _shellViewModel.ActiveServer = mock1.Object;

            //---------------Execute Test ----------------------
            _shellViewModel.DuplicateResource(explorerVm.Object);

            //---------------Test Result -----------------------
            mock.VerifyAll();
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void SaveAllCommand_GivenWorkflowItemAndCanSav_ShouldSaveItem()
        {
            //---------------Set up test pack-------------------

            CreateFullExportsAndVm();
            var surfaeViewModel = new Mock<IWorkSurfaceViewModel>();
            var workSurfaceKey = new WorkSurfaceKey()
            {
                WorkSurfaceContext = WorkSurfaceContext.Workflow
            };
            var surfaceContext = new Mock<WorkSurfaceContextViewModel>(workSurfaceKey, surfaeViewModel.Object);
            _shellViewModel.Items.Add(surfaceContext.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(_shellViewModel);
            Assert.IsNotNull(_shellViewModel.SaveAllAndCloseCommand);
            Assert.IsNotNull(_shellViewModel.Items);

            //---------------Execute Test ----------------------
            _shellViewModel.SaveAllAndCloseCommand.Execute(null);
            //---------------Test Result -----------------------
            surfaceContext.VerifyAll();
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        public void ShellViewModel_AddWorkflowCommand_OpenResourcePicker()
        {
            var resourceId = Guid.NewGuid();
            var resourceName = "ResourceName";

            var mockResourceModel = new Mock<IContextualResourceModel>();
            mockResourceModel.Setup(resourceModel => resourceModel.ResourceName).Returns(resourceName);
            mockResourceModel.Setup(resourceModel => resourceModel.ID).Returns(resourceId);
            mockResourceModel.Setup(resourceModel => resourceModel.ResourceType).Returns(ResourceType.WorkflowService);
            mockResourceModel.Setup(resourceModel => resourceModel.DataList).Returns("<DataList><scalar1 Description=\"\" IsEditable=\"True\" " +
                                                                          "ColumnIODirection=\"Input\" /><scalar2 Description=\"\" IsEditable=\"True\" " +
                                                                          "ColumnIODirection=\"Input\" /></DataList>");

            var mockResourceRepository = new Mock<IResourceRepository>();
            mockResourceRepository.Setup(resourceRepo => resourceRepo.LoadContextualResourceModel(resourceId)).Returns(mockResourceModel.Object);
            var mockServer = new Mock<IServer>();
            mockServer.Setup(server => server.DisplayName).Returns("Localhost");
            mockServer.Setup(server => server.Name).Returns("Localhost");
            mockServer.Setup(server => server.ResourceRepository).Returns(mockResourceRepository.Object);
            var mockEnvironmentConnection = SetupMockConnection();
            mockServer.SetupGet(server => server.Connection).Returns(mockEnvironmentConnection.Object);

            var mockVersionChecker = new Mock<IVersionChecker>();
            var mockPopupController = new Mock<IBrowserPopupController>();
            var mockViewFactory = new Mock<IViewFactory>();

            var mockExplorerTreeItem = new Mock<IExplorerTreeItem>();
            mockExplorerTreeItem.Setup(explorerTreeItem => explorerTreeItem.ResourcePath).Returns(resourceName);
            mockExplorerTreeItem.Setup(explorerTreeItem => explorerTreeItem.ResourceId).Returns(resourceId);

            var mockResourcePicker = new Mock<IResourcePickerDialog>();
            mockResourcePicker.Setup(resourcePicker => resourcePicker.ShowDialog(mockServer.Object)).Returns(true);
            mockResourcePicker.Setup(resourcePicker => resourcePicker.SelectedResource).Returns(mockExplorerTreeItem.Object);

            var mockServerRepository = new Mock<IServerRepository>();
            mockServerRepository.Setup(repository => repository.ActiveServer).Returns(mockServer.Object);
            mockServerRepository.Setup(repository => repository.Source).Returns(mockServer.Object);
            mockServerRepository.Setup(repository => repository.All()).Returns(new List<IServer>());
            CustomContainer.Register(mockServerRepository.Object);

            var shellViewModel = new ShellViewModel(new Mock<IEventAggregator>().Object, new Mock<IAsyncWorker>().Object, mockServerRepository.Object, mockVersionChecker.Object, mockViewFactory.Object, false, mockPopupController.Object, new Mock<IPopupController>().Object, null, mockResourcePicker.Object);

            var optionWorkflow = new OptionWorkflow();
            var optionView = new Warewolf.UI.OptionView(optionWorkflow, () => { });
            shellViewModel.AddWorkflowCommand.Execute(optionView);

            mockResourcePicker.Verify(resourcePicker => resourcePicker.ShowDialog(mockServer.Object), Times.Once());
            mockResourceRepository.Verify(resourceRepo => resourceRepo.LoadContextualResourceModel(resourceId), Times.Once());
            Assert.AreEqual(resourceId, optionWorkflow.Workflow.Value);
            Assert.AreEqual(resourceId, optionWorkflow.Workflow.Value);
            Assert.AreEqual(resourceName, optionWorkflow.Workflow.Name);
            Assert.AreEqual(2, optionWorkflow.Workflow.Inputs.Count);
            Assert.AreEqual("scalar1", optionWorkflow.Workflow.Inputs.ToList()[0].Name);
            Assert.AreEqual("scalar2", optionWorkflow.Workflow.Inputs.ToList()[1].Name);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        public void ShellViewModel_RunCoverageCommand_ExplorerItemViewModel()
        {
            var resourceId = Guid.NewGuid();
            var resourceName = "ResourceName";

            var mockResourceModel = new Mock<IContextualResourceModel>();
            mockResourceModel.Setup(o => o.ResourceName).Returns(resourceName);
            mockResourceModel.Setup(o => o.ID).Returns(resourceId);
            mockResourceModel.Setup(o => o.ResourceType).Returns(ResourceType.WorkflowService);
            mockResourceModel.Setup(o => o.DataList).Returns("<DataList><scalar1 Description=\"\" IsEditable=\"True\" " +
                                                                          "ColumnIODirection=\"Input\" /><scalar2 Description=\"\" IsEditable=\"True\" " +
                                                                          "ColumnIODirection=\"Input\" /></DataList>");

            var environmentId = Guid.NewGuid();
            
            var mockResourceRepository = new Mock<IResourceRepository>();
            mockResourceRepository.Setup(o => o.LoadContextualResourceModel(resourceId)).Returns(mockResourceModel.Object);
            var mockServer = new Mock<IServer>();
            mockServer.Setup(o => o.DisplayName).Returns("Localhost");
            mockServer.Setup(o => o.Name).Returns("Localhost");
            mockServer.Setup(o => o.EnvironmentID).Returns(environmentId);
            mockServer.Setup(o => o.ResourceRepository).Returns(mockResourceRepository.Object);
            var mockEnvironmentConnection = SetupMockConnection();
            mockServer.SetupGet(o => o.Connection).Returns(mockEnvironmentConnection.Object);

            var mockVersionChecker = new Mock<IVersionChecker>();
            var mockBrowserPopupController = new Mock<IBrowserPopupController>();
            var mockPopupController = new Mock<IPopupController>();
            var mockViewFactory = new Mock<IViewFactory>();

            var mockExplorerTreeItem = new Mock<IExplorerTreeItem>();
            
            var mockExplorerItemViewModel = new Mock<IExplorerItemViewModel>();
            mockExplorerItemViewModel.Setup(o => o.ResourcePath).Returns(resourceName);
            mockExplorerItemViewModel.Setup(o => o.ResourceId).Returns(resourceId);
            
            var mockResourcePicker = new Mock<IResourcePickerDialog>();
            
            var mockServerRepository = new Mock<IServerRepository>();
            mockServerRepository.Setup(o => o.ActiveServer).Returns(mockServer.Object);
            mockServerRepository.Setup(o => o.Get(environmentId)).Returns(mockServer.Object);
            mockServerRepository.Setup(o => o.Source).Returns(mockServer.Object);
            mockServerRepository.Setup(o => o.All()).Returns(new List<IServer>());
            CustomContainer.Register(mockServerRepository.Object);

            var mockWorksurfaceContext = new Mock<IWorksurfaceContextManager>();
            mockWorksurfaceContext.Setup(o => o.RunAllTestCoverageForService(mockResourceModel.Object)).Verifiable();
            
            var shellViewModel = new ShellViewModel(new Mock<IEventAggregator>().Object, new Mock<IAsyncWorker>().Object, mockServerRepository.Object, mockVersionChecker.Object, mockViewFactory.Object, false, mockBrowserPopupController.Object, new Mock<IPopupController>().Object, null, mockResourcePicker.Object);

            shellViewModel.WorksurfaceContextManager = mockWorksurfaceContext.Object;
            
            var explorerItemViewModel = new ExplorerItemViewModel(mockServer.Object, mockExplorerTreeItem.Object, null, shellViewModel, mockPopupController.Object);
            explorerItemViewModel.ResourcePath = resourceName;
            explorerItemViewModel.ResourceId = resourceId;
            
            Assert.IsTrue((shellViewModel.RunCoverageCommand.CanExecute(null)));
            shellViewModel.RunCoverageCommand.Execute(explorerItemViewModel);
            
            mockResourceRepository.Verify(o => o.LoadContextualResourceModel(resourceId), Times.Once());
            mockWorksurfaceContext.Verify(o => o.RunAllTestCoverageForService(mockResourceModel.Object), Times.Once());
        }
        
        [TestMethod]
        [Owner("Pieter Terblanche")]
        public void ShellViewModel_RunCoverageCommand_EnvironmentViewModel()
        {
            var resourceId = Guid.NewGuid();
            var resourceName = "ResourceName";

            var mockResourceModel = new Mock<IContextualResourceModel>();
            mockResourceModel.Setup(o => o.ResourceName).Returns(resourceName);
            mockResourceModel.Setup(o => o.ID).Returns(resourceId);
            mockResourceModel.Setup(o => o.ResourceType).Returns(ResourceType.Server);
            mockResourceModel.Setup(o => o.DataList).Returns("<DataList><scalar1 Description=\"\" IsEditable=\"True\" " +
                                                                          "ColumnIODirection=\"Input\" /><scalar2 Description=\"\" IsEditable=\"True\" " +
                                                                          "ColumnIODirection=\"Input\" /></DataList>");

            var environmentId = Guid.NewGuid();
            
            var mockResourceRepository = new Mock<IResourceRepository>();
            var mockServer = new Mock<IServer>();
            mockServer.Setup(o => o.DisplayName).Returns("Localhost");
            mockServer.Setup(o => o.Name).Returns("Localhost");
            mockServer.Setup(o => o.EnvironmentID).Returns(environmentId);
            mockServer.Setup(o => o.ResourceRepository).Returns(mockResourceRepository.Object);
            var mockEnvironmentConnection = SetupMockConnection();
            mockServer.SetupGet(o => o.Connection).Returns(mockEnvironmentConnection.Object);

            var mockVersionChecker = new Mock<IVersionChecker>();
            var mockBrowserPopupController = new Mock<IBrowserPopupController>();
            var mockPopupController = new Mock<IPopupController>();
            var mockViewFactory = new Mock<IViewFactory>();

            var mockExplorerTreeItem = new Mock<IExplorerTreeItem>();
            
            var mockExplorerItemViewModel = new Mock<IExplorerItemViewModel>();
            mockExplorerItemViewModel.Setup(o => o.ResourcePath).Returns(resourceName);
            mockExplorerItemViewModel.Setup(o => o.ResourceId).Returns(resourceId);
            
            var mockResourcePicker = new Mock<IResourcePickerDialog>();
            
            var mockServerRepository = new Mock<IServerRepository>();
            mockServerRepository.Setup(o => o.ActiveServer).Returns(mockServer.Object);
            mockServerRepository.Setup(o => o.Get(environmentId)).Returns(mockServer.Object);
            mockServerRepository.Setup(o => o.Source).Returns(mockServer.Object);
            mockServerRepository.Setup(o => o.All()).Returns(new List<IServer>());
            CustomContainer.Register(mockServerRepository.Object);

            var mockWorksurfaceContext = new Mock<IWorksurfaceContextManager>();
            mockWorksurfaceContext.Setup(o => o.RunAllTestCoverageForFolder(It.IsAny<string>(), It.IsAny<IExternalProcessExecutor>())).Verifiable();
            
            var shellViewModel = new ShellViewModel(new Mock<IEventAggregator>().Object, new Mock<IAsyncWorker>().Object, mockServerRepository.Object, mockVersionChecker.Object, mockViewFactory.Object, false, mockBrowserPopupController.Object, new Mock<IPopupController>().Object, null, mockResourcePicker.Object);

            shellViewModel.WorksurfaceContextManager = mockWorksurfaceContext.Object;
            
            var environmentViewModel = new EnvironmentViewModel(mockServer.Object, shellViewModel);
            environmentViewModel.ResourcePath = resourceName;
            environmentViewModel.ResourceId = resourceId;
            
            Assert.IsTrue((shellViewModel.RunCoverageCommand.CanExecute(null)));
            shellViewModel.RunCoverageCommand.Execute(environmentViewModel);
            
            mockResourceRepository.Verify(o => o.LoadContextualResourceModel(resourceId), Times.Once());
            mockWorksurfaceContext.Verify(o => o.RunAllTestCoverageForFolder(It.IsAny<string>(), It.IsAny<IExternalProcessExecutor>()), Times.Once());
        }
        
        [TestMethod]
        [Owner("Pieter Terblanche")]
        public void ShellViewModel_RunCoverageCommand_WorkflowDesignerViewModel()
        {
            var resourceId = Guid.NewGuid();
            var resourceName = "ResourceName";

            var mockResourceModel = new Mock<IContextualResourceModel>();
            mockResourceModel.Setup(o => o.ResourceName).Returns(resourceName);
            
            mockResourceModel.Setup(o => o.ID).Returns(resourceId);
            mockResourceModel.Setup(o => o.ResourceType).Returns(ResourceType.Server);
            mockResourceModel.Setup(o => o.DataList).Returns("<DataList><scalar1 Description=\"\" IsEditable=\"True\" " +
                                                                          "ColumnIODirection=\"Input\" /><scalar2 Description=\"\" IsEditable=\"True\" " +
                                                                          "ColumnIODirection=\"Input\" /></DataList>");

            var environmentId = Guid.NewGuid();
            
            var mockResourceRepository = new Mock<IResourceRepository>();
            var mockServer = new Mock<IServer>();
            mockServer.Setup(o => o.DisplayName).Returns("Localhost");
            mockServer.Setup(o => o.Name).Returns("Localhost");
            mockServer.Setup(o => o.EnvironmentID).Returns(environmentId);
            mockServer.Setup(o => o.ResourceRepository).Returns(mockResourceRepository.Object);
            var mockEnvironmentConnection = SetupMockConnection();
            mockServer.SetupGet(o => o.Connection).Returns(mockEnvironmentConnection.Object);
            var mockEventPublisher = new Mock<IEventPublisher>();
            mockEnvironmentConnection.Setup(m => m.ServerEvents).Returns(mockEventPublisher.Object);

            mockResourceModel.Setup(o => o.Environment).Returns(mockServer.Object);
            
            var mockVersionChecker = new Mock<IVersionChecker>();
            var mockBrowserPopupController = new Mock<IBrowserPopupController>();
            var mockPopupController = new Mock<IPopupController>();
            var mockViewFactory = new Mock<IViewFactory>();

            var mockExplorerTreeItem = new Mock<IExplorerTreeItem>();
            
            var mockExplorerItemViewModel = new Mock<IExplorerItemViewModel>();
            mockExplorerItemViewModel.Setup(o => o.ResourcePath).Returns(resourceName);
            mockExplorerItemViewModel.Setup(o => o.ResourceId).Returns(resourceId);
            
            var mockResourcePicker = new Mock<IResourcePickerDialog>();
            
            var mockServerRepository = new Mock<IServerRepository>();
            mockServerRepository.Setup(o => o.ActiveServer).Returns(mockServer.Object);
            mockServerRepository.Setup(o => o.Get(environmentId)).Returns(mockServer.Object);
            mockServerRepository.Setup(o => o.Source).Returns(mockServer.Object);
            mockServerRepository.Setup(o => o.All()).Returns(new List<IServer>());
            CustomContainer.Register(mockServerRepository.Object);

            var mockWorksurfaceContext = new Mock<IWorksurfaceContextManager>();
            mockWorksurfaceContext.Setup(o => o.RunAllTestCoverageForFolder(It.IsAny<string>(), It.IsAny<IExternalProcessExecutor>())).Verifiable();

            var mockAsyncWorker = new Mock<IAsyncWorker>();

            var mockEventAggregator = new Mock<IEventAggregator>();
            var shellViewModel = new ShellViewModel(mockEventAggregator.Object, mockAsyncWorker.Object, mockServerRepository.Object, mockVersionChecker.Object, mockViewFactory.Object, false, mockBrowserPopupController.Object, new Mock<IPopupController>().Object, null, mockResourcePicker.Object);

            shellViewModel.WorksurfaceContextManager = mockWorksurfaceContext.Object;
            
            var mockWorkflowHelper = new Mock<IWorkflowHelper>();
            
            var workflowDesignerViewModel = new WorkflowDesignerViewModel(mockEventAggregator.Object, mockResourceModel.Object, mockWorkflowHelper.Object, mockPopupController.Object, mockAsyncWorker.Object, false, false);

            Assert.IsTrue((shellViewModel.RunCoverageCommand.CanExecute(null)));
            shellViewModel.RunCoverageCommand.Execute(workflowDesignerViewModel);
            
            mockResourceRepository.Verify(o => o.LoadContextualResourceModel(resourceId), Times.Once());
            mockWorksurfaceContext.Verify(o => o.RunAllTestCoverageForFolder(It.IsAny<string>(), It.IsAny<IExternalProcessExecutor>()), Times.Once());
        }
        
        [TestMethod]
        [Owner("Pieter Terblanche")]
        public void ShellViewModel_RunAllTestsCommand_ExplorerItemViewModel()
        {
            var resourceId = Guid.NewGuid();
            var resourceName = "ResourceName";

            var mockResourceModel = new Mock<IContextualResourceModel>();
            mockResourceModel.Setup(o => o.ResourceName).Returns(resourceName);
            mockResourceModel.Setup(o => o.ID).Returns(resourceId);
            mockResourceModel.Setup(o => o.ResourceType).Returns(ResourceType.WorkflowService);
            mockResourceModel.Setup(o => o.DataList).Returns("<DataList><scalar1 Description=\"\" IsEditable=\"True\" " +
                                                                          "ColumnIODirection=\"Input\" /><scalar2 Description=\"\" IsEditable=\"True\" " +
                                                                          "ColumnIODirection=\"Input\" /></DataList>");

            var environmentId = Guid.NewGuid();
            
            var mockResourceRepository = new Mock<IResourceRepository>();
            mockResourceRepository.Setup(o => o.LoadContextualResourceModel(resourceId)).Returns(mockResourceModel.Object);
            var mockServer = new Mock<IServer>();
            mockServer.Setup(o => o.DisplayName).Returns("Localhost");
            mockServer.Setup(o => o.Name).Returns("Localhost");
            mockServer.Setup(o => o.EnvironmentID).Returns(environmentId);
            mockServer.Setup(o => o.ResourceRepository).Returns(mockResourceRepository.Object);
            var mockEnvironmentConnection = SetupMockConnection();
            mockServer.SetupGet(o => o.Connection).Returns(mockEnvironmentConnection.Object);

            var mockVersionChecker = new Mock<IVersionChecker>();
            var mockBrowserPopupController = new Mock<IBrowserPopupController>();
            var mockPopupController = new Mock<IPopupController>();
            var mockViewFactory = new Mock<IViewFactory>();

            var mockExplorerTreeItem = new Mock<IExplorerTreeItem>();
            
            var mockExplorerItemViewModel = new Mock<IExplorerItemViewModel>();
            mockExplorerItemViewModel.Setup(o => o.ResourcePath).Returns(resourceName);
            mockExplorerItemViewModel.Setup(o => o.ResourceId).Returns(resourceId);
            
            var mockResourcePicker = new Mock<IResourcePickerDialog>();
            
            var mockServerRepository = new Mock<IServerRepository>();
            mockServerRepository.Setup(o => o.ActiveServer).Returns(mockServer.Object);
            mockServerRepository.Setup(o => o.Get(environmentId)).Returns(mockServer.Object);
            mockServerRepository.Setup(o => o.Source).Returns(mockServer.Object);
            mockServerRepository.Setup(o => o.All()).Returns(new List<IServer>());
            CustomContainer.Register(mockServerRepository.Object);

            var mockWorksurfaceContext = new Mock<IWorksurfaceContextManager>();
            mockWorksurfaceContext.Setup(o => o.RunAllTestsForService(mockResourceModel.Object)).Verifiable();
            
            var shellViewModel = new ShellViewModel(new Mock<IEventAggregator>().Object, new Mock<IAsyncWorker>().Object, mockServerRepository.Object, mockVersionChecker.Object, mockViewFactory.Object, false, mockBrowserPopupController.Object, new Mock<IPopupController>().Object, null, mockResourcePicker.Object);

            shellViewModel.WorksurfaceContextManager = mockWorksurfaceContext.Object;
            
            var explorerItemViewModel = new ExplorerItemViewModel(mockServer.Object, mockExplorerTreeItem.Object, null, shellViewModel, mockPopupController.Object);
            explorerItemViewModel.ResourcePath = resourceName;
            explorerItemViewModel.ResourceId = resourceId;
            
            Assert.IsTrue((shellViewModel.RunAllTestsCommand.CanExecute(null)));
            shellViewModel.RunAllTestsCommand.Execute(explorerItemViewModel);
            
            mockResourceRepository.Verify(o => o.LoadContextualResourceModel(resourceId), Times.Once());
            mockWorksurfaceContext.Verify(o => o.RunAllTestsForService(mockResourceModel.Object), Times.Once());
        }
        
        [TestMethod]
        [Owner("Pieter Terblanche")]
        public void ShellViewModel_RunAllTestsCommand_EnvironmentViewModel()
        {
            var resourceId = Guid.NewGuid();
            var resourceName = "ResourceName";

            var mockResourceModel = new Mock<IContextualResourceModel>();
            mockResourceModel.Setup(o => o.ResourceName).Returns(resourceName);
            mockResourceModel.Setup(o => o.ID).Returns(resourceId);
            mockResourceModel.Setup(o => o.ResourceType).Returns(ResourceType.Server);
            mockResourceModel.Setup(o => o.DataList).Returns("<DataList><scalar1 Description=\"\" IsEditable=\"True\" " +
                                                                          "ColumnIODirection=\"Input\" /><scalar2 Description=\"\" IsEditable=\"True\" " +
                                                                          "ColumnIODirection=\"Input\" /></DataList>");

            var environmentId = Guid.NewGuid();
            
            var mockResourceRepository = new Mock<IResourceRepository>();
            var mockServer = new Mock<IServer>();
            mockServer.Setup(o => o.DisplayName).Returns("Localhost");
            mockServer.Setup(o => o.Name).Returns("Localhost");
            mockServer.Setup(o => o.EnvironmentID).Returns(environmentId);
            mockServer.Setup(o => o.ResourceRepository).Returns(mockResourceRepository.Object);
            var mockEnvironmentConnection = SetupMockConnection();
            mockServer.SetupGet(o => o.Connection).Returns(mockEnvironmentConnection.Object);

            var mockVersionChecker = new Mock<IVersionChecker>();
            var mockBrowserPopupController = new Mock<IBrowserPopupController>();
            var mockPopupController = new Mock<IPopupController>();
            var mockViewFactory = new Mock<IViewFactory>();

            var mockExplorerTreeItem = new Mock<IExplorerTreeItem>();
            
            var mockExplorerItemViewModel = new Mock<IExplorerItemViewModel>();
            mockExplorerItemViewModel.Setup(o => o.ResourcePath).Returns(resourceName);
            mockExplorerItemViewModel.Setup(o => o.ResourceId).Returns(resourceId);
            
            var mockResourcePicker = new Mock<IResourcePickerDialog>();
            
            var mockServerRepository = new Mock<IServerRepository>();
            mockServerRepository.Setup(o => o.ActiveServer).Returns(mockServer.Object);
            mockServerRepository.Setup(o => o.Get(environmentId)).Returns(mockServer.Object);
            mockServerRepository.Setup(o => o.Source).Returns(mockServer.Object);
            mockServerRepository.Setup(o => o.All()).Returns(new List<IServer>());
            CustomContainer.Register(mockServerRepository.Object);

            var mockWorksurfaceContext = new Mock<IWorksurfaceContextManager>();
            mockWorksurfaceContext.Setup(o => o.RunAllTestsForFolder(It.IsAny<string>(), It.IsAny<IExternalProcessExecutor>())).Verifiable();
            
            var shellViewModel = new ShellViewModel(new Mock<IEventAggregator>().Object, new Mock<IAsyncWorker>().Object, mockServerRepository.Object, mockVersionChecker.Object, mockViewFactory.Object, false, mockBrowserPopupController.Object, new Mock<IPopupController>().Object, null, mockResourcePicker.Object);

            shellViewModel.WorksurfaceContextManager = mockWorksurfaceContext.Object;
            
            var environmentViewModel = new EnvironmentViewModel(mockServer.Object, shellViewModel);
            environmentViewModel.ResourcePath = resourceName;
            environmentViewModel.ResourceId = resourceId;
            
            Assert.IsTrue((shellViewModel.RunAllTestsCommand.CanExecute(null)));
            shellViewModel.RunAllTestsCommand.Execute(environmentViewModel);
            
            mockResourceRepository.Verify(o => o.LoadContextualResourceModel(resourceId), Times.Once());
            mockWorksurfaceContext.Verify(o => o.RunAllTestsForFolder(It.IsAny<string>(), It.IsAny<IExternalProcessExecutor>()), Times.Once());
        }
        
        [TestMethod]
        [Owner("Pieter Terblanche")]
        public void ShellViewModel_RunAllTestsCommand_WorkflowDesignerViewModel()
        {
            var resourceId = Guid.NewGuid();
            var resourceName = "ResourceName";

            var mockResourceModel = new Mock<IContextualResourceModel>();
            mockResourceModel.Setup(o => o.ResourceName).Returns(resourceName);
            
            mockResourceModel.Setup(o => o.ID).Returns(resourceId);
            mockResourceModel.Setup(o => o.ResourceType).Returns(ResourceType.Server);
            mockResourceModel.Setup(o => o.DataList).Returns("<DataList><scalar1 Description=\"\" IsEditable=\"True\" " +
                                                                          "ColumnIODirection=\"Input\" /><scalar2 Description=\"\" IsEditable=\"True\" " +
                                                                          "ColumnIODirection=\"Input\" /></DataList>");

            var environmentId = Guid.NewGuid();
            
            var mockResourceRepository = new Mock<IResourceRepository>();
            var mockServer = new Mock<IServer>();
            mockServer.Setup(o => o.DisplayName).Returns("Localhost");
            mockServer.Setup(o => o.Name).Returns("Localhost");
            mockServer.Setup(o => o.EnvironmentID).Returns(environmentId);
            mockServer.Setup(o => o.ResourceRepository).Returns(mockResourceRepository.Object);
            var mockEnvironmentConnection = SetupMockConnection();
            mockServer.SetupGet(o => o.Connection).Returns(mockEnvironmentConnection.Object);
            var mockEventPublisher = new Mock<IEventPublisher>();
            mockEnvironmentConnection.Setup(m => m.ServerEvents).Returns(mockEventPublisher.Object);

            mockResourceModel.Setup(o => o.Environment).Returns(mockServer.Object);
            
            var mockVersionChecker = new Mock<IVersionChecker>();
            var mockBrowserPopupController = new Mock<IBrowserPopupController>();
            var mockPopupController = new Mock<IPopupController>();
            var mockViewFactory = new Mock<IViewFactory>();

            var mockExplorerTreeItem = new Mock<IExplorerTreeItem>();
            
            var mockExplorerItemViewModel = new Mock<IExplorerItemViewModel>();
            mockExplorerItemViewModel.Setup(o => o.ResourcePath).Returns(resourceName);
            mockExplorerItemViewModel.Setup(o => o.ResourceId).Returns(resourceId);
            
            var mockResourcePicker = new Mock<IResourcePickerDialog>();
            
            var mockServerRepository = new Mock<IServerRepository>();
            mockServerRepository.Setup(o => o.ActiveServer).Returns(mockServer.Object);
            mockServerRepository.Setup(o => o.Get(environmentId)).Returns(mockServer.Object);
            mockServerRepository.Setup(o => o.Source).Returns(mockServer.Object);
            mockServerRepository.Setup(o => o.All()).Returns(new List<IServer>());
            CustomContainer.Register(mockServerRepository.Object);

            var mockWorksurfaceContext = new Mock<IWorksurfaceContextManager>();
            mockWorksurfaceContext.Setup(o => o.RunAllTestsForFolder(It.IsAny<string>(), It.IsAny<IExternalProcessExecutor>())).Verifiable();

            var mockAsyncWorker = new Mock<IAsyncWorker>();

            var mockEventAggregator = new Mock<IEventAggregator>();
            var shellViewModel = new ShellViewModel(mockEventAggregator.Object, mockAsyncWorker.Object, mockServerRepository.Object, mockVersionChecker.Object, mockViewFactory.Object, false, mockBrowserPopupController.Object, new Mock<IPopupController>().Object, null, mockResourcePicker.Object);

            shellViewModel.WorksurfaceContextManager = mockWorksurfaceContext.Object;
            
            var mockWorkflowHelper = new Mock<IWorkflowHelper>();
            
            var workflowDesignerViewModel = new WorkflowDesignerViewModel(mockEventAggregator.Object, mockResourceModel.Object, mockWorkflowHelper.Object, mockPopupController.Object, mockAsyncWorker.Object, false, false);

            Assert.IsTrue((shellViewModel.RunAllTestsCommand.CanExecute(null)));
            shellViewModel.RunAllTestsCommand.Execute(workflowDesignerViewModel);
            
            mockResourceRepository.Verify(o => o.LoadContextualResourceModel(resourceId), Times.Once());
            mockWorksurfaceContext.Verify(o => o.RunAllTestsForFolder(It.IsAny<string>(), It.IsAny<IExternalProcessExecutor>()), Times.Once());
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DeployResource_GivenIsNew_ShouldReturnNull()
        {
            //---------------Set up test pack-------------------

            CreateFullExportsAndVm();

            //---------------Assert Precondition----------------
            Assert.IsNull(_shellViewModel.DeployResource);

            //---------------Execute Test ----------------------
            _shellViewModel.DeployResource = new Mock<IContextualResourceModel>().Object;
            //---------------Test Result -----------------------
            Assert.IsNotNull(_shellViewModel.DeployResource);
        }



        [TestMethod]
        public void MainViewModel_HasNewVersion_ShouldCallBrowserPopupContollerToLatestVersionPage()
        {
            var popupController = new Mock<IBrowserPopupController>();
            popupController.Setup(p => p.ShowPopup(It.IsAny<string>())).Verifiable();
            CustomContainer.Register(new Mock<IWindowManager>().Object);
            var envRepo = new Mock<IServerRepository>();
            envRepo.Setup(e => e.All()).Returns(new List<IServer>());
            var environmentModel = new Mock<IServer>();
            var mockEnvironmentConnection = SetupMockConnection();
            environmentModel.SetupGet(it => it.Connection).Returns(mockEnvironmentConnection.Object);
            envRepo.Setup(e => e.Source).Returns(environmentModel.Object);

            var mockVersionChecker = new Mock<IVersionChecker>();
            CustomContainer.DeRegister<IServerRepository>();
            CustomContainer.Register(envRepo.Object);
            mockVersionChecker.Setup(checker => checker.GetNewerVersionAsync()).Returns(Task.FromResult(true));
            var vieFactory = new Mock<IViewFactory>();
            var viewMock = new Mock<IView>();
            vieFactory.Setup(factory => factory.GetViewGivenServerResourceType(It.IsAny<string>())).Returns(viewMock.Object);
            var vm = new ShellViewModel(new Mock<IEventAggregator>().Object, new Mock<IAsyncWorker>().Object, envRepo.Object, mockVersionChecker.Object, vieFactory.Object, false, popupController.Object);
            vm.DisplayDialogForNewVersion();

            popupController.Verify(p => p.ShowPopup(Warewolf.Studio.Resources.Languages.Core.WarewolfLatestDownloadUrl));
        }

        public class IEnvironmentRepository
        {
            public object Source { get; set; }
            public object All() { return null; }
            public object Get(Guid guid) { return Guid.Empty; }
        }

        public class IEnvironmentModel
        {
            public object ResourceRepository { get; set; }
            public object Connection { get; set; }
        }

        public class MainViewModelPersistenceMock
        {
            private IEnvironmentRepository _object1;
            private IAsyncWorker _object2;

            public MainViewModelPersistenceMock(IEnvironmentRepository object1, IAsyncWorker object2)
            {
                this._object1 = object1;
                this._object2 = object2;
            }

            public IList<WorkSurfaceContextViewModel> Items { get; internal set; }
            public Common.Interfaces.Studio.Controller.IPopupController PopupProvider { get; internal set; }
            public SpecialContext WorksurfaceContextManager { get; internal set; }
            public object ActiveItem { get; internal set; }

            internal void ActivateItem(object p) => throw new NotImplementedException();

            public class SpecialContext
            {
                internal void TryRemoveContext(object contextualResourceModel) => throw new NotImplementedException();
            }
        }
    }

    public class SchedulerViewModelForTesting : SchedulerViewModel
    {
        public SchedulerViewModelForTesting()
        {

        }

        public SchedulerViewModelForTesting(IEventAggregator eventPublisher, DirectoryObjectPickerDialog directoryObjectPicker, Common.Interfaces.Studio.Controller.IPopupController popupController, IAsyncWorker asyncWorker)

            : base(eventPublisher, directoryObjectPicker, popupController, asyncWorker, new Mock<IServer>().Object, a => new Mock<IServer>().Object)
        {

        }

        public override bool DoDeactivate(bool showMessage)
        {
            return RetValue;
        }

        public bool RetValue { get; set; }
    }

    public class SettingsViewModelForTest : SettingsViewModel
    {

        public SettingsViewModelForTest()
        {

        }


        public SettingsViewModelForTest(IEventAggregator eventPublisher, Common.Interfaces.Studio.Controller.IPopupController popupController,

                                       IAsyncWorker asyncWorker, IWin32Window parentWindow)
            : base(eventPublisher, popupController, asyncWorker, parentWindow, new Mock<IServer>().Object, a => new Mock<IServer>().Object)
        {
        }


        public override bool DoDeactivate(bool showMessage)
        {
            return RetValue;
        }

        public bool RetValue { get; set; }
    }

}
