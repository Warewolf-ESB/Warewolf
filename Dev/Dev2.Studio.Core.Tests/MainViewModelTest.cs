
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using Caliburn.Micro;
using CubicOrange.Windows.Forms.ActiveDirectory;
using Dev2.AppResources.Repositories;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Infrastructure.Events;
using Dev2.Common.Interfaces.Scheduler.Interfaces;
using Dev2.Common.Interfaces.Security;
using Dev2.Common.Interfaces.Studio;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Common.Interfaces.Threading;
using Dev2.Communication;
using Dev2.ConnectionHelpers;
using Dev2.Core.Tests.Utils;
using Dev2.Factory;
using Dev2.Services.Events;
using Dev2.Services.Security;
using Dev2.Settings;
using Dev2.Settings.Scheduler;
using Dev2.Studio.AppResources.Comparers;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.Browsers;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Helpers;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Interfaces.DataList;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Core.Workspaces;
using Dev2.Studio.Factory;
using Dev2.Studio.ViewModels;
using Dev2.Studio.ViewModels.Help;
using Dev2.Studio.ViewModels.Workflow;
using Dev2.Studio.ViewModels.WorkSurface;
using Dev2.Threading;
using Dev2.Util;
using Dev2.Utilities;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

//using System.Windows.Media.Imaging;
// ReSharper disable InconsistentNaming
// ReSharper disable ObjectCreationAsStatement
namespace Dev2.Core.Tests
{
    /// <summary>
    ///     This is a result class for MainViewModelTest and is intended
    ///     to contain all MainViewModelTest Unit Tests
    /// </summary>
    [TestClass]
    public class MainViewModelTest : MainViewModelBase
    {
        [TestInitialize]
        public void Initialize()
        {
            AppSettings.LocalHost = "http://localhost:3142";
            var svr = new Mock<IServer>();
            svr.Setup(a => a.ResourceName).Returns("Localhost");
            
            Task<IExplorerItem> ac = new Task<IExplorerItem>(()=> new Mock<IExplorerItem>().Object);
            svr.Setup(a => a.LoadExplorer()).Returns(()=>ac);
            CustomContainer.Register(svr.Object);
            CustomContainer.Register(new Mock<Microsoft.Practices.Prism.PubSubEvents.IEventAggregator>().Object);
        }

        [TestMethod]
        public void DeployCommandCanExecuteIrrespectiveOfEnvironments()
        {
            CreateFullExportsAndVm();
            Assert.IsTrue(MainViewModel.DeployCommand.CanExecute(null));
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
            ActiveEnvironment.Setup(e => e.IsConnected).Returns(isConnected);
            ActiveEnvironment.Setup(e => e.CanStudioExecute).Returns(canStudioExecute);
            // ReSharper disable MaximumChainedReferences
            AuthorizationService.Setup(a => a.IsAuthorized(AuthorizationContext.Administrator, It.IsAny<string>())).Returns(isAuthorized);


            var actual = MainViewModel.SettingsCommand.CanExecute(null);
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
            resourceRepo.Setup(r => r.ReloadResource(It.IsAny<Guid>(), It.IsAny<ResourceType>(), It.IsAny<IEqualityComparer<IResourceModel>>(), true)).Verifiable();
            resourceRepo.Setup(r => r.FetchResourceDefinition(It.IsAny<IEnvironmentModel>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>())).Returns(new ExecuteMessage());


            var envConn = new Mock<IEnvironmentConnection>();
            envConn.Setup(conn => conn.ServerEvents).Returns(new Mock<IEventPublisher>().Object);
            envConn.Setup(conn => conn.WorkspaceID).Returns(workspaceID);
            envConn.Setup(conn => conn.ServerID).Returns(serverID);
            envConn.Setup(conn => conn.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>())).Returns(new StringBuilder());

            var env = new Mock<IEnvironmentModel>();
            env.Setup(e => e.Connection).Returns(envConn.Object);
            env.Setup(e => e.IsConnected).Returns(true);
            env.Setup(e => e.ResourceRepository).Returns(resourceRepo.Object);
            
            resourceModel.Setup(m => m.Environment).Returns(env.Object);

            var envRepo = new Mock<IEnvironmentRepository>();
            envRepo.Setup(r => r.All()).Returns(new List<IEnvironmentModel>(new[] { env.Object }));
            envRepo.Setup(r => r.Get(It.IsAny<Guid>())).Returns(env.Object);
            envRepo.Setup(r => r.Source).Returns(env.Object);

            

            Mock<IAsyncWorker> asyncWorker = AsyncWorkerTests.CreateSynchronousAsyncWorker();


            // FetchResourceDefinitionService
            var viewModel = new MainViewModelPersistenceMock(envRepo.Object, asyncWorker.Object, false);

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
            resourceRepo.Setup(r => r.ReloadResource(It.IsAny<Guid>(), It.IsAny<ResourceType>(), It.IsAny<IEqualityComparer<IResourceModel>>(), true)).Verifiable();

            var envConn = new Mock<IEnvironmentConnection>();
            envConn.Setup(conn => conn.WorkspaceID).Returns(workspaceID);
            envConn.Setup(conn => conn.ServerID).Returns(serverID);
            var env = new Mock<IEnvironmentModel>();
            env.Setup(e => e.Connection).Returns(envConn.Object);
            env.Setup(e => e.IsConnected).Returns(true);
            env.Setup(e => e.ResourceRepository).Returns(resourceRepo.Object);

            resourceModel.Setup(m => m.Environment).Returns(env.Object);

            var envRepo = new Mock<IEnvironmentRepository>();
            envRepo.Setup(r => r.All()).Returns(new List<IEnvironmentModel>(new[] { env.Object }));
            envRepo.Setup(r => r.Source).Returns(env.Object);
            Mock<IAsyncWorker> asyncWorker = AsyncWorkerTests.CreateSynchronousAsyncWorker();
            var viewModel = new MainViewModelPersistenceMock(envRepo.Object, asyncWorker.Object, false);

            resourceRepo.Verify(r => r.ReloadResource(It.IsAny<Guid>(), It.IsAny<ResourceType>(), It.IsAny<IEqualityComparer<IResourceModel>>(), true), Times.Never());
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

            Guid environmentID = Guid.NewGuid();
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
            resourceRepo.Setup(r => r.ReloadResource(It.IsAny<Guid>(), It.IsAny<ResourceType>(), It.IsAny<IEqualityComparer<IResourceModel>>(), true)).Verifiable();
            resourceRepo.Setup(r => r.FetchResourceDefinition(It.IsAny<IEnvironmentModel>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>())).Returns(new ExecuteMessage());

            var envConn = new Mock<IEnvironmentConnection>();
            envConn.Setup(conn => conn.WorkspaceID).Returns(workspaceID);
            envConn.Setup(conn => conn.ServerID).Returns(serverID);
            envConn.Setup(conn => conn.ServerEvents).Returns(new Mock<IEventPublisher>().Object);

            var env = new Mock<IEnvironmentModel>();
            env.Setup(e => e.Connection).Returns(envConn.Object);
            env.Setup(e => e.IsConnected).Returns(true);
            env.Setup(e => e.ResourceRepository).Returns(resourceRepo.Object);
            env.Setup(e => e.ID).Returns(environmentID);

            resourceModel.Setup(m => m.Environment).Returns(env.Object);

            var envRepo = new Mock<IEnvironmentRepository>();
            envRepo.Setup(r => r.All()).Returns(new List<IEnvironmentModel>(new[] { env.Object }));
            envRepo.Setup(r => r.Source).Returns(env.Object);
            envRepo.Setup(r => r.Get(It.IsAny<Guid>())).Returns(env.Object);
            Mock<IAsyncWorker> asyncWorker = AsyncWorkerTests.CreateSynchronousAsyncWorker();
            var viewModel = new MainViewModelPersistenceMock(envRepo.Object, asyncWorker.Object, false);

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
            var environmentRepository = new Mock<IEnvironmentRepository>();
            var versionChecker = new Mock<IVersionChecker>();
            var mvm = new MainViewModel(eventPublisher.Object, null, environmentRepository.Object, versionChecker.Object, false);
            Assert.IsNull(mvm);
        }


        #endregion

        #region Show Help Tab

        [TestMethod]
        public void ShowHelpTabMessageExpectHelpTabWithUriActive()
        {
            CreateFullExportsAndVm();
            var msg = new ShowHelpTabMessage("testuri");
            MainViewModel.Handle(msg);
            var helpctx = MainViewModel.ActiveItem.WorkSurfaceViewModel as HelpViewModel;
            Assert.IsNotNull(helpctx);
            // ReSharper disable PossibleNullReferenceException
            Assert.IsTrue(helpctx.Uri == "testuri");
            // ReSharper restore PossibleNullReferenceException
        }

        #endregion

        #region AddMode Work Surface


        #endregion

        #region Close Context

        [TestMethod]
        public void MainViewModel_CloseWorkSurfaceContext_CloseTrueAndResourceSaved_RemoveWorkspaceItemRemoveCalledAndTabClosedMessageAndContextRemoved()
        {
            CreateFullExportsAndVm();

            Assert.AreEqual(2, MainViewModel.Items.Count);

            FirstResource.Setup(r => r.IsWorkflowSaved).Returns(true);
            FirstResource.Setup(r => r.IsAuthorized(AuthorizationContext.Contribute)).Returns(true);
            var activetx =
                MainViewModel.Items.ToList()
                    .First(i => i.WorkSurfaceViewModel.WorkSurfaceContext == WorkSurfaceContext.Workflow);

            EventAggregator.Setup(e => e.Publish(It.IsAny<TabClosedMessage>()))
                .Callback<object>(o =>
                {
                    var msg = (TabClosedMessage)o;
                    Assert.IsTrue(msg.Context.Equals(activetx));
                });

            MainViewModel.DeactivateItem(activetx, true);
            MockWorkspaceRepo.Verify(c => c.Remove(FirstResource.Object), Times.Once());
            Assert.IsTrue(MainViewModel.Items.Count == 1);
            EventAggregator.Verify(e => e.Publish(It.IsAny<TabClosedMessage>()), Times.Once());
        }

        [TestMethod]
        public void MainViewModel_CloseWorkSurfaceContext_CloseTrueAndResourceNotSavedPopupOk_RemoveWorkspaceItemCalledAndContextRemovedAndSaveResourceEventAggregatorMessage()
        {
            CreateFullExportsAndVm();

            Assert.AreEqual(2, MainViewModel.Items.Count);
            FirstResource.Setup(r => r.IsWorkflowSaved).Returns(false);
            FirstResource.Setup(r => r.IsAuthorized(AuthorizationContext.Contribute)).Returns(true);
            PopupController.Setup(s => s.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(MessageBoxResult.Yes);

            var activetx =
                MainViewModel.Items.ToList()
                    .First(i => i.WorkSurfaceViewModel.WorkSurfaceContext == WorkSurfaceContext.Workflow);

            EventAggregator.Setup(e => e.Publish(It.IsAny<TabClosedMessage>()))
                .Callback<object>(o =>
                {
                    var msg = (TabClosedMessage)o;
                    Assert.IsTrue(msg.Context.Equals(activetx));
                });

            EventAggregator.Setup(e => e.Publish(It.IsAny<SaveResourceMessage>()))
                .Callback<object>(o =>
                {
                    var msg = (SaveResourceMessage)o;
                    Assert.IsTrue(msg.Resource.Equals(FirstResource.Object));
                });

            MainViewModel.DeactivateItem(activetx, true);
            MockWorkspaceRepo.Verify(c => c.Remove(FirstResource.Object), Times.Once());
            Assert.IsTrue(MainViewModel.Items.Count == 1);
            EventAggregator.Verify(e => e.Publish(It.IsAny<TabClosedMessage>()), Times.Once());
            EventAggregator.Verify(e => e.Publish(It.IsAny<SaveResourceMessage>()), Times.Once());
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("MainViewModel_DeactivateItem")]
        public void MainViewModel_DeactivateItem_WorkSurfaceContextViewModelIsNull_RemoveIsNotCalledOnTheRepo()
        {
            CreateFullExportsAndVm();
            Assert.AreEqual(2, MainViewModel.Items.Count);
            FirstResource.Setup(r => r.IsWorkflowSaved).Returns(false);
            FirstResource.Setup(r => r.IsAuthorized(AuthorizationContext.Contribute)).Returns(true);
            PopupController.Setup(s => s.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(MessageBoxResult.Yes);
            MainViewModel.DeactivateItem(null, true);
            MockWorkspaceRepo.Verify(c => c.Remove(FirstResource.Object), Times.Never());
        }

        [TestMethod]
        public void MainViewModel_CloseWorkSurfaceContext_CloseTrueAndResourceNotSavedPopupNotOk_WorkspaceItemNotRemoved()
        {
            CreateFullExportsAndVm();
            Assert.AreEqual(2, MainViewModel.Items.Count);
            FirstResource.Setup(r => r.IsWorkflowSaved).Returns(false);
            FirstResource.Setup(r => r.IsAuthorized(AuthorizationContext.Contribute)).Returns(true);

            PopupController.Setup(s => s.Show()).Returns(MessageBoxResult.No);
            var activetx =
                MainViewModel.Items.ToList()
                    .First(i => i.WorkSurfaceViewModel.WorkSurfaceContext == WorkSurfaceContext.Workflow);
            MainViewModel.DeactivateItem(activetx, false);
            MockWorkspaceRepo.Verify(c => c.Remove(FirstResource.Object), Times.Never());
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("MainViewModel_ChangeActiveItem")]
        public void MainViewModel_ChangeActiveItem_WhenHasContextWithNoDataListViewModel_ClearsCollectionsOnNewItem()
        {
            //------------Setup for test--------------------------
            string errorString;
            CreateFullExportsAndVm();
            FirstResource.Setup(r => r.IsAuthorized(AuthorizationContext.Contribute)).Returns(true);
            var firstCtx = MainViewModel.FindWorkSurfaceContextViewModel(FirstResource.Object);
            var mockDataListViewModel = new Mock<IDataListViewModel>();
            firstCtx.DataListViewModel = mockDataListViewModel.Object;
            MainViewModel.ActiveItem = MainViewModel.Items.FirstOrDefault(c => c.WorkSurfaceViewModel.GetType() == typeof(HelpViewModel));
            //------------Execute Test---------------------------
            MainViewModel.ActivateItem(firstCtx);
            //------------Assert Results-------------------------
            mockDataListViewModel.Verify(model => model.ClearCollections(), Times.Once());
            mockDataListViewModel.Verify(model => model.CreateListsOfIDataListItemModelToBindTo(out errorString), Times.Once());
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("MainViewModel_IsWorkFlowOpened")]
        public void MainViewModel_IsWorkFlowOpened_ResourceIsOpened_True()
        {
            //------------Setup for test--------------------------
            CreateFullExportsAndVm();
            FirstResource.Setup(r => r.IsAuthorized(AuthorizationContext.Contribute)).Returns(true);
            //------------Execute Test---------------------------
            var isWorkflowOpened = MainViewModel.IsWorkFlowOpened(FirstResource.Object);
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
            var environmentModel = new Mock<IEnvironmentModel>();
            environmentModel.Setup(e => e.ID).Returns(Guid.NewGuid);
            resource.Setup(r => r.Environment).Returns(environmentModel.Object);
            resource.Setup(r => r.IsAuthorized(AuthorizationContext.Contribute)).Returns(true);
            var isWorkflowOpened = MainViewModel.IsWorkFlowOpened(resource.Object);
            //------------Execute Test---------------------------
            Assert.IsFalse(isWorkflowOpened);
        }

        [TestMethod]
        public void MainViewModel_CloseWorkSurfaceContext_CloseFalse_PreviousItemActivatedAndAllItemsPResent()
        {
            CreateFullExportsAndVm();
            AddAdditionalContext();
            Assert.AreEqual(3, MainViewModel.Items.Count);

            FirstResource.Setup(r => r.IsAuthorized(AuthorizationContext.Contribute)).Returns(true);
            SecondResource.Setup(r => r.IsAuthorized(AuthorizationContext.Contribute)).Returns(true);

            var firstCtx = MainViewModel.FindWorkSurfaceContextViewModel(FirstResource.Object);
            var secondCtx = MainViewModel.FindWorkSurfaceContextViewModel(SecondResource.Object);

            MainViewModel.ActivateItem(firstCtx);
            MainViewModel.DeactivateItem(secondCtx, false);

            Assert.AreEqual(3, MainViewModel.Items.Count);
            Assert.IsTrue(MainViewModel.ActiveItem.Equals(firstCtx));
        }

        [TestMethod]
        public void MainViewModel_CloseWorkSurfaceContext_CloseTrue_PreviousItemActivatedAndOneLessItem()
        {
            CreateFullExportsAndVm();
            AddAdditionalContext();
            Assert.AreEqual(3, MainViewModel.Items.Count);

            FirstResource.Setup(r => r.IsAuthorized(AuthorizationContext.Contribute)).Returns(true);
            SecondResource.Setup(r => r.IsAuthorized(AuthorizationContext.Contribute)).Returns(true);

            var firstCtx = MainViewModel.FindWorkSurfaceContextViewModel(FirstResource.Object);
            var secondCtx = MainViewModel.FindWorkSurfaceContextViewModel(SecondResource.Object);

            MainViewModel.ActivateItem(firstCtx);
            MainViewModel.DeactivateItem(firstCtx, true);

            Assert.AreEqual(3, MainViewModel.Items.Count);
            Assert.IsFalse(MainViewModel.ActiveItem.Equals(secondCtx));
        }

        [TestMethod]
        public void MainViewModel_CloseWorkSurfaceContext_CloseFalse_ContextNotRemoved()
        {
            CreateFullExportsAndVm();
            FirstResource.Setup(r => r.IsAuthorized(AuthorizationContext.Contribute)).Returns(true);
            var activetx =
                MainViewModel.Items.ToList()
                    .First(i => i.WorkSurfaceViewModel.WorkSurfaceContext == WorkSurfaceContext.Workflow);
            MainViewModel.DeactivateItem(activetx, false);
            MockWorkspaceRepo.Verify(c => c.Remove(FirstResource.Object), Times.Never());
        }

        [TestMethod]
        [TestCategory("MainViewModel_CloseWorkSurfaceContext")]
        [Description("An exisiting workflow with unsaved changes that is not saved, must rollback the resource model.")]
        [Owner("Trevor Williams-Ros")]
        public void MainViewModel_CloseWorkSurfaceContext_ExistingUnsavedWorkflowNotSaved_ResourceModelRolledback()
        {
            CreateFullExportsAndVm();
            Assert.IsTrue(MainViewModel.Items.Count == 2);
            FirstResource.Setup(r => r.IsWorkflowSaved).Returns(false);
            FirstResource.Setup(r => r.Commit()).Verifiable();
            FirstResource.Setup(r => r.Rollback()).Verifiable();
            FirstResource.Setup(r => r.IsAuthorized(AuthorizationContext.Contribute)).Returns(true);

            PopupController.Setup(s => s.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(MessageBoxResult.No);
            var activetx = MainViewModel.Items.ToList().First(i => i.WorkSurfaceViewModel.WorkSurfaceContext == WorkSurfaceContext.Workflow);
            MainViewModel.CloseWorkSurfaceContext(activetx, null);
            FirstResource.Verify(r => r.Commit(), Times.Never(), "ResourceModel was committed when not saved.");
            FirstResource.Verify(r => r.Rollback(), Times.Once(), "ResourceModel was not rolled back when not saved.");
        }

        [TestMethod]
        [TestCategory("MainViewModel_CloseWorkSurfaceContext")]
        [Description("An exisiting workflow with unsaved changes that is saved, must commit the resource model.")]
        [Owner("Trevor Williams-Ros")]
        public void MainViewModel_CloseWorkSurfaceContext_ExistingUnsavedWorkflowSaved_ResourceModelCommitted()
        {
            CreateFullExportsAndVm();
            Assert.IsTrue(MainViewModel.Items.Count == 2);
            FirstResource.Setup(r => r.IsWorkflowSaved).Returns(false);
            FirstResource.Setup(r => r.IsAuthorized(AuthorizationContext.Contribute)).Returns(true);
            FirstResource.Setup(r => r.Commit()).Verifiable();
            FirstResource.Setup(r => r.Rollback()).Verifiable();

            PopupController.Setup(s => s.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(MessageBoxResult.Yes);
            var activetx = MainViewModel.Items.ToList().First(i => i.WorkSurfaceViewModel.WorkSurfaceContext == WorkSurfaceContext.Workflow);
            MainViewModel.CloseWorkSurfaceContext(activetx, null);
            FirstResource.Verify(r => r.Commit(), Times.Once(), "ResourceModel was not committed when saved.");
            FirstResource.Verify(r => r.Rollback(), Times.Never(), "ResourceModel was rolled back when saved.");
        }


        [TestMethod]
        [TestCategory("MainViewModel_CloseWorkSurfaceContext")]
        [Description("An exisiting workflow with unsaved changes that is saved, must commit the resource model.")]
        [Owner("Leon Rajindrapersadh")]
        public void MainViewModel_CloseWorkSurfaceContext_ExistingUnsavedWorkflowSaved_WhenDeletedNoPopup()
        {
            CreateFullExportsAndVm();
            Assert.IsTrue(MainViewModel.Items.Count == 2);
            FirstResource.Setup(r => r.IsWorkflowSaved).Returns(false);
            FirstResource.Setup(r => r.IsAuthorized(AuthorizationContext.Contribute)).Returns(true);
            FirstResource.Setup(r => r.Commit()).Verifiable();
            FirstResource.Setup(r => r.Rollback()).Verifiable();

            PopupController.Setup(s => s.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(MessageBoxResult.Yes);
            var activetx = MainViewModel.Items.ToList().First(i => i.WorkSurfaceViewModel.WorkSurfaceContext == WorkSurfaceContext.Workflow);
            MainViewModel.CloseWorkSurfaceContext(activetx, null,true);
            PopupController.Verify(
                s =>
                    s.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(),
                        It.IsAny<MessageBoxImage>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(),
                        It.IsAny<bool>(), It.IsAny<bool>()), Times.Never);


        }

        [TestMethod]
        [TestCategory("MainViewModel_CloseWorkSurfaceContext")]
        [Description("An exisiting workflow with unsaved changes that is saved, must commit the resource model.")]
        [Owner("Leon Rajindrapersadh")]
        public void MainViewModel_CloseResource()
        {
            CreateFullExportsAndVm();
            Assert.IsTrue(MainViewModel.Items.Count == 2);
            FirstResource.Setup(r => r.IsWorkflowSaved).Returns(false);
            FirstResource.Setup(r => r.IsAuthorized(AuthorizationContext.Contribute)).Returns(true);
            FirstResource.Setup(r => r.Commit()).Verifiable();
            FirstResource.Setup(r => r.Rollback()).Verifiable();
            var gu = Guid.NewGuid();
            FirstResource.Setup(a => a.ID).Returns(gu);
            PopupController.Setup(s => s.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(MessageBoxResult.Yes);
            var mckEnv = new Mock<IEnvironmentRepository>();
            var mockEnv = new Mock<IEnvironmentModel>();
            PrivateObject p = new PrivateObject(MainViewModel);
            p.SetProperty("EnvironmentRepository",mckEnv.Object);
            mckEnv.Setup(a => a.Get(It.IsAny<Guid>()))
                .Returns(mockEnv.Object);
            var res = new Mock<IResourceRepository>();
            mockEnv.Setup(a => a.ResourceRepository).Returns(res.Object);
            res.Setup(a => a.LoadContextualResourceModel(It.IsAny<Guid>())).Returns(FirstResource.Object);
            MainViewModel.CloseResource(gu,Guid.Empty);
            PopupController.Verify(
                s =>
                    s.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(),
                        It.IsAny<MessageBoxImage>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(),
                        It.IsAny<bool>(), It.IsAny<bool>()), Times.Never);


        }



        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("MainViewModel_CloseWorkSurfaceContext")]
        public void MainViewModel_CloseWorkSurfaceContext_UnsavedWorkflowAndResourceCanSaveIsFalse_ResourceModelIsNotSaved()
        {
            //------------Setup for test--------------------------
            CreateFullExportsAndVm();
            Assert.IsTrue(MainViewModel.Items.Count == 2);

            FirstResource.Setup(r => r.Commit()).Verifiable();
            FirstResource.Setup(r => r.Rollback()).Verifiable();
            FirstResource.Setup(r => r.IsWorkflowSaved).Returns(false);
            FirstResource.Setup(r => r.IsAuthorized(AuthorizationContext.Contribute)).Returns(false);

            EventAggregator.Setup(e => e.Publish(It.IsAny<SaveResourceMessage>())).Verifiable();

            PopupController.Setup(s => s.Show()).Returns(MessageBoxResult.Yes);
            var activetx = MainViewModel.Items.ToList().First(i => i.WorkSurfaceViewModel.WorkSurfaceContext == WorkSurfaceContext.Workflow);

            //------------Execute Test---------------------------
            MainViewModel.CloseWorkSurfaceContext(activetx, null);
            PrivateObject pvt = new PrivateObject(MainViewModel);
            //------------Assert Results-------------------------
            EventAggregator.Verify(e => e.Publish(It.IsAny<SaveResourceMessage>()), Times.Never());
            FirstResource.Verify(r => r.Commit(), Times.Never(), "ResourceModel was committed when saved.");
            FirstResource.Verify(r => r.Rollback(), Times.Never(), "ResourceModel was rolled back when saved.");
            Assert.IsNull(pvt.GetField("_previousActive"));
        }


        #endregion

        #region Workspaces and init

        [TestMethod]
        public void OnImportsSatisfiedExpectsTwoItems()
        {
            CreateFullExportsAndVm();
            //One saved workspaceitem, one startpage
            Assert.AreEqual(2, MainViewModel.Items.Count);
        }

        [TestMethod]
        public void OnImportsSatisfiedExpectsContextsAddedForSavedWorkspaces()
        {
            CreateFullExportsAndVm();
            var activetx =
                MainViewModel.Items.ToList()
                    .First(i => i.WorkSurfaceViewModel.WorkSurfaceContext == WorkSurfaceContext.Workflow);
            var expectedKey = WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.Workflow, FirstResourceId,
                ServerId);
            Assert.IsTrue(expectedKey.ResourceID.Equals(activetx.WorkSurfaceKey.ResourceID) && expectedKey.ServerID.Equals(activetx.WorkSurfaceKey.ServerID));
        }

        [TestMethod]
        public void OnImportsSatisfiedExpectsDisplayNameSet()
        {
            CreateFullExportsAndVm();
            const string expected = "Warewolf";
            // flipping thing never passes locally... silly chickens ;(
            StringAssert.Contains(MainViewModel.DisplayName, expected);
        }

        #endregion workspaces
        #region Commands

        [TestMethod]
        public void DisplayAboutDialogueCommandExpectsWindowManagerShowingIDialogueViewModel()
        {
            CreateFullExportsAndVm();
            WindowManager.Setup(w => w.ShowDialog(It.IsAny<IDialogueViewModel>(), null, null)).Verifiable();
            CustomContainer.Register(new DialogViewModelFactory() as IDialogViewModelFactory);
            MainViewModel.DisplayAboutDialogueCommand.Execute(null);
            WindowManager.Verify(w => w.ShowDialog(It.IsAny<IDialogueViewModel>(), null, null), Times.Once());
        }


        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("MainViewModel_AddLanguageHelpPageCommand")]
        public void MainViewModel_AddLanguageHelpPageCommand_LanguageHelpActive()
        {
            CreateFullExportsAndVm();
            //------------Execute Test---------------------------
            MainViewModel.AddLanguageHelpPageCommand.Execute(null);

            // Assert LanguageHelp is active
            var languageHelpUri = FileHelper.GetFullPath(StringResources.Uri_Studio_Language_Reference_Document);
            var langHelpCtx = MainViewModel.ActiveItem.WorkSurfaceViewModel as HelpViewModel;
            Assert.IsNotNull(langHelpCtx);
            // ReSharper disable PossibleNullReferenceException
            Assert.IsTrue(langHelpCtx.Uri == languageHelpUri);
            // ReSharper restore PossibleNullReferenceException
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("MainViewModel_ShowStartPageCommand")]
        public void MainViewModel_ShowStartPageCommand_ShowStartPageActive()
        {
            CreateFullExportsAndVm();
            //------------Execute Test---------------------------
            MainViewModel.ShowStartPageCommand.Execute(null);
            var langHelpCtx = MainViewModel.ActiveItem.WorkSurfaceViewModel as HelpViewModel;
            Assert.IsNotNull(langHelpCtx);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("MainViewModel_DebugCommand")]
        public void MainViewModel_DebugCommand_NotNull()
        {
            CreateFullExportsAndVm();
            //------------Execute Test---------------------------
            var authorizeCommand = MainViewModel.DebugCommand;
            Assert.IsNotNull(authorizeCommand);
            Assert.AreEqual(MainViewModel.ActiveItem.DebugCommand, authorizeCommand);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("MainViewModel_DebugCommand")]
        public void MainViewModel_DebugCommandNoActiveItem_NotNull()
        {
            CreateFullExportsAndVmWithEmptyRepo();
            MainViewModel.DeactivateItem(MainViewModel.ActiveItem, true);
            //------------Execute Test---------------------------
            var authorizeCommand = MainViewModel.DebugCommand;
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
            var authorizeCommand = MainViewModel.QuickDebugCommand;
            Assert.IsNotNull(authorizeCommand);
            Assert.AreEqual(MainViewModel.ActiveItem.QuickDebugCommand, authorizeCommand);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("MainViewModel_QuickDebugCommand")]
        public void MainViewModel_QuickDebugCommandNoActiveItem_NotNull()
        {
            CreateFullExportsAndVmWithEmptyRepo();
            MainViewModel.DeactivateItem(MainViewModel.ActiveItem, true);
            //------------Execute Test---------------------------
            var authorizeCommand = MainViewModel.QuickDebugCommand;
            Assert.IsNotNull(authorizeCommand);
            Assert.AreEqual(AuthorizationContext.None, authorizeCommand.AuthorizationContext);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("MainViewModel_EditCommand")]
        public void MainViewModel_EditCommand_NotNull()
        {
            CreateFullExportsAndVm();
            //------------Execute Test---------------------------
            var authorizeCommand = MainViewModel.EditCommand;
            Assert.IsNotNull(authorizeCommand);
            Assert.AreEqual(MainViewModel.ActiveItem.EditCommand, authorizeCommand);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("MainViewModel_EditCommand")]
        public void MainViewModel_EditCommandNoActiveItem_NotNull()
        {
            CreateFullExportsAndVmWithEmptyRepo();
            MainViewModel.DeactivateItem(MainViewModel.ActiveItem, true);
            //------------Execute Test---------------------------
            var authorizeCommand = MainViewModel.EditCommand;
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
            var authorizeCommand = MainViewModel.SaveCommand;
            Assert.IsNotNull(authorizeCommand);
            Assert.AreEqual(MainViewModel.ActiveItem.SaveCommand, authorizeCommand);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("MainViewModel_SaveCommand")]
        public void MainViewModel_SaveCommandNoActiveItem_NotNull()
        {
            CreateFullExportsAndVmWithEmptyRepo();
            MainViewModel.DeactivateItem(MainViewModel.ActiveItem, true);
            //------------Execute Test---------------------------
            var authorizeCommand = MainViewModel.SaveCommand;
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
            var authorizeCommand = MainViewModel.QuickViewInBrowserCommand;
            Assert.IsNotNull(authorizeCommand);
            Assert.AreEqual(MainViewModel.ActiveItem.QuickViewInBrowserCommand, authorizeCommand);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("MainViewModel_QuickViewInBrowserCommand")]
        public void MainViewModel_QuickViewInBrowserCommandNoActiveItem_NotNull()
        {
            CreateFullExportsAndVmWithEmptyRepo();
            MainViewModel.DeactivateItem(MainViewModel.ActiveItem, true);
            //------------Execute Test---------------------------
            var authorizeCommand = MainViewModel.QuickViewInBrowserCommand;
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
            var authorizeCommand = MainViewModel.ViewInBrowserCommand;
            Assert.IsNotNull(authorizeCommand);
            Assert.AreEqual(MainViewModel.ActiveItem.ViewInBrowserCommand, authorizeCommand);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("MainViewModel_ViewInBrowserCommand")]
        public void MainViewModel_ViewInBrowserCommandNoActiveItem_NotNull()
        {
            CreateFullExportsAndVmWithEmptyRepo();
            MainViewModel.DeactivateItem(MainViewModel.ActiveItem, true);
            //------------Execute Test---------------------------
            var authorizeCommand = MainViewModel.ViewInBrowserCommand;
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
            MainViewModel.ShowCommunityPageCommand.Execute(null);
            BrowserPopupController.Verify(controller => controller.ShowPopup(It.IsAny<string>()));
        }

        [TestMethod]
        [Description("Makes sure that new workflow only calls TempSave, not save on the resource repository")]
        [Owner("Jurie Smit")]
        public void MainViewModel_Regression_NewWorkFlowCommand_DoesNotSaveRepository()
        {
            //Setup
            CustomContainer.Register(new Mock<IPopupController>().Object);
            CreateFullExportsAndVmWithEmptyRepo();
            MockStudioResourceRepository.Setup(repository => repository.AddResouceItem(It.IsAny<IContextualResourceModel>()));
            // ReSharper disable once SuggestVarOrType_Elsewhere
            Mock<IEnvironmentModel> environmentRepo = CreateMockEnvironment();
            // ReSharper disable once SuggestVarOrType_Elsewhere
            Mock<IAuthorizationService> mockAuthService = new Mock<IAuthorizationService>();
            mockAuthService.Setup(c => c.GetResourcePermissions(It.IsAny<Guid>())).Returns(Permissions.Administrator);
            environmentRepo.Setup(c => c.AuthorizationService).Returns(mockAuthService.Object);
            // ReSharper disable once SuggestVarOrType_Elsewhere
            Mock<IResourceRepository> resourceRepo = new Mock<IResourceRepository>();
            resourceRepo.Setup(c => c.FetchResourceDefinition(It.IsAny<IEnvironmentModel>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>())).Returns(new ExecuteMessage());
            resourceRepo.Setup(r => r.Save(It.IsAny<IResourceModel>())).Verifiable();
           
            environmentRepo.Setup(e => e.ResourceRepository).Returns(resourceRepo.Object);
            environmentRepo.Setup(e => e.IsConnected).Returns(true);
            environmentRepo.Setup(e => e.IsLocalHost).Returns(true);

            EmptyEnvRepo.Setup(p => p.Get(It.IsAny<Guid>())).Returns(environmentRepo.Object);

            MainViewModel.ActiveEnvironment = environmentRepo.Object;
            MainViewModel.NewResourceCommand.Execute("Workflow");
            //Assert
            resourceRepo.Verify(r => r.Save(It.IsAny<IResourceModel>()), Times.Never());
        }

        #endregion

        #region Delete
        //
        //        [TestMethod]
        //        public void DeleteServerResourceOnLocalHostAlsoDeletesFromEnvironmentRepoAndExplorerTree()
        //        {
        //            //---------Setup------
        //            var mock = SetupForDeleteServer();
        //            EnvironmentModel.Setup(s => s.IsLocalHost).Returns(true);
        //            //---------Execute------
        //            var msg = new DeleteResourcesMessage(new List<IContextualResourceModel> { FirstResource.Object }, "", false);
        //            MainViewModel.Handle(msg);
        //
        //            //---------Verify------
        //            mock.Verify(s => s.Remove(It.IsAny<IEnvironmentModel>()), Times.Once());
        //            EventAggregator.Verify(e => e.Publish(It.IsAny<EnvironmentDeletedMessage>()), Times.Once());
        //        }
        //
        //        [TestMethod]
        //        public void DeleteServerResourceOnOtherServerDoesntDeleteFromEnvironmentRepoAndExplorerTree()
        //        {
        //            //---------Setup------
        //            var mock = SetupForDeleteServer();
        //            EnvironmentConnection.Setup(c => c.DisplayName).Returns("NotLocalHost");
        //            EventAggregator = new Mock<IEventAggregator>();
        //            EventAggregator.Setup(e => e.Publish(It.IsAny<EnvironmentDeletedMessage>())).Verifiable();
        //
        //            //---------Execute------
        //            var msg = new DeleteResourcesMessage(new List<IContextualResourceModel> { FirstResource.Object }, "", false);
        //            MainViewModel.Handle(msg);
        //
        //            //---------Verify------
        //            mock.Verify(s => s.Remove(It.IsAny<IEnvironmentModel>()), Times.Never());
        //            EventAggregator.Verify(e => e.Publish(It.IsAny<EnvironmentDeletedMessage>()), Times.Never());
        //        }

        [TestMethod]
        public void DeleteResourceConfirmedExpectContextRemoved()
        {
            CreateFullExportsAndVm();
            SetupForDelete();
            var msg = new DeleteResourcesMessage(new List<IContextualResourceModel> { FirstResource.Object }, "");
            MainViewModel.Handle(msg);
            ResourceRepo.Verify(s => s.HasDependencies(FirstResource.Object), Times.Once());
        }

  
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("MainViewModel_HandleDeleteResourceMessage")]
        public void MainViewModel_HandleDeleteResourceMessage_WhenHasActionDeclined_PerformsAction()
        {
            //------------Setup for test--------------------------
            CreateFullExportsAndVm();
            SetupForDelete();
            PopupController.Setup(s => s.Show()).Returns(MessageBoxResult.No);
            var _actionInvoked = false;
            var msg = new DeleteResourcesMessage(new List<IContextualResourceModel> { FirstResource.Object }, "", false, () =>
            {
                _actionInvoked = true;
            });
            //------------Execute Test---------------------------
            MainViewModel.Handle(msg);
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
            PopupController.Setup(s => s.Show()).Returns(MessageBoxResult.Yes);
            var _actionInvoked = false;
            var msg = new DeleteResourcesMessage(null, "", true, () =>
            {
                _actionInvoked = true;
            });
            //------------Execute Test---------------------------
            MainViewModel.Handle(msg);
            //------------Assert Results-------------------------
            Assert.IsFalse(_actionInvoked);
        }

        [TestMethod]
        public void DeleteResourceWithConfirmExpectsDependencyServiceCalled()
        {
            CreateFullExportsAndVm();
            SetupForDelete();
            PopupController.Setup(s => s.Show()).Returns(MessageBoxResult.Yes);
            var msg = new DeleteResourcesMessage(new List<IContextualResourceModel> { FirstResource.Object }, "");
            MainViewModel.Handle(msg);
            ResourceRepo.Verify(s => s.HasDependencies(FirstResource.Object), Times.Once());
        }




        [TestMethod]
        public void DeleteResourceWithDeclineExpectsDependencyServiceCalled()
        {
            CreateFullExportsAndVm();
            SetupForDelete();
            PopupController.Setup(s => s.Show()).Returns(MessageBoxResult.No);
            var msg = new DeleteResourcesMessage(new List<IContextualResourceModel> { FirstResource.Object }, "", false);
            MainViewModel.Handle(msg);
            ResourceRepo.Verify(s => s.HasDependencies(FirstResource.Object), Times.Never());
        }

        [TestMethod]
        public void DeleteResourceWithNullResourceExpectsNoPoupShown()
        {
            CreateFullExportsAndVm();
            SetupForDelete();
            var msg = new DeleteResourcesMessage(null, "", false);
            MainViewModel.Handle(msg);
            PopupController.Verify(s => s.Show(), Times.Never());
        }

        [TestMethod]
        [TestCategory("MainViewmodel_Delete")]
        [Description("Unassigned resources can be deleted")]
        [Owner("Ashley Lewis")]
        // ReSharper disable InconsistentNaming
        public void MainViewmodel_UnitTest_DeleteUnassignedResource_ResourceRepositoryDeleteResourceCalled()
        // ReSharper restore InconsistentNaming
        {
            //Isolate delete unassigned resource as a functional unit
            CreateFullExportsAndVm();
            SetupForDelete();
            var unassignedResource = new Mock<IContextualResourceModel>();
            var repo = new Mock<IResourceRepository>();
            var env = new Mock<IEnvironmentModel>();

            unassignedResource.Setup(res => res.Category).Returns(string.Empty);
            unassignedResource.Setup(resource => resource.Environment).Returns(env.Object);
            repo.Setup(repository => repository.DeleteResource(unassignedResource.Object)).Returns(MakeMsg("<DataList>Success</DataList>")).Verifiable();
            env.Setup(environment => environment.ResourceRepository).Returns(repo.Object);
            var msg = new DeleteResourcesMessage(new List<IContextualResourceModel> { unassignedResource.Object }, "", false,()=>repo.Object.DeleteResource(unassignedResource.Object));

            //Run delete command
            MainViewModel.Handle(msg);

            //Assert resource deleted from repository
            repo.Verify(repository => repository.DeleteResource(unassignedResource.Object), Times.Once(), "Deleting an unassigned resource does not delete from resource repository");
        }
      

        #endregion delete

        #region ShowStartPage

        // PBI 9512 - 2013.06.07 - TWR: added
        [TestMethod]
        public void MainViewModelShowStartPageExpectedGetsLatestFirst()
        {
            CreateFullExportsAndVm();
            var versionChecker = Mock.Get(MainViewModel.Version);
            versionChecker.Setup(v => v.CommunityPageUri).Verifiable();
            MainViewModel.ShowStartPage();
            versionChecker.Verify(v => v.CommunityPageUri);
        }

        #endregion

        #region ShowDependencies

      
        #endregion

        #region IHandle

        #endregion

        #region DeactivateItem

        // PBI 9405 - 2013.06.13 - Massimo.Guerrera
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

            var envConn = new Mock<IEnvironmentConnection>();
            var env = new Mock<IEnvironmentModel>();
            envConn.Setup(conn => conn.ServerEvents).Returns(new Mock<IEventPublisher>().Object);
            env.Setup(e => e.ResourceRepository).Returns(resourceRepo.Object);
            env.Setup(e => e.Connection).Returns(envConn.Object);

            var envRepo = new Mock<IEnvironmentRepository>();
            envRepo.Setup(r => r.All()).Returns(new[] { env.Object });
            envRepo.Setup(r => r.Source).Returns(env.Object);
            envRepo.Setup(r => r.Get(It.IsAny<Guid>())).Returns(env.Object);
            Mock<IAsyncWorker> asyncWorker = AsyncWorkerTests.CreateSynchronousAsyncWorker();
            var mockMainViewModel = new MainViewModelPersistenceMock(envRepo.Object, asyncWorker.Object, false);
            var resourceID = Guid.NewGuid();
            var serverID = Guid.NewGuid();

            #region Setup WorkSurfaceContextViewModel1

            var resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(m => m.Environment).Returns(env.Object);
            resourceModel.Setup(m => m.ID).Returns(resourceID);
            resourceModel.Setup(m => m.ResourceName).Returns("Some resource name 4");
            Mock<IPopupController> mockPopUp = Dev2MockFactory.CreateIPopup(MessageBoxResult.No);
            var workflowHelper = new Mock<IWorkflowHelper>();
            var designerViewModel = new WorkflowDesignerViewModel(new Mock<IEventAggregator>().Object, resourceModel.Object, workflowHelper.Object, mockPopUp.Object, new SynchronousAsyncWorker(), new Mock<IExternalProcessExecutor>().Object, false);
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

            var envRepo = new Mock<IEnvironmentRepository>();
            var resourceRepo = new Mock<IResourceRepository>();
            resourceRepo.Setup(r => r.Save(It.IsAny<IResourceModel>())).Verifiable();

            var envConn = new Mock<IEnvironmentConnection>();
            var env = new Mock<IEnvironmentModel>();
            envConn.Setup(conn => conn.ServerEvents).Returns(new Mock<IEventPublisher>().Object);
            env.Setup(e => e.ResourceRepository).Returns(resourceRepo.Object);
            env.Setup(e => e.Connection).Returns(envConn.Object);
            envRepo.Setup(r => r.All()).Returns(new[] { env.Object });
            envRepo.Setup(e => e.Source).Returns(env.Object);
            envRepo.Setup(e => e.Get(It.IsAny<Guid>())).Returns(env.Object);
            Mock<IAsyncWorker> asyncWorker = AsyncWorkerTests.CreateSynchronousAsyncWorker();
            var mockMainViewModel = new MainViewModelPersistenceMock(envRepo.Object, asyncWorker.Object, false);
            var resourceID = Guid.NewGuid();
            var serverID = Guid.NewGuid();

            #region Setup WorkSurfaceContextViewModel1

            var resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(m => m.Environment).Returns(env.Object);
            resourceModel.Setup(m => m.ID).Returns(resourceID);
            resourceModel.Setup(r => r.IsAuthorized(AuthorizationContext.Contribute)).Returns(true);
            resourceModel.Setup(m => m.ResourceName).Returns("Some resource name 3");
            Mock<IPopupController> mockPopUp = Dev2MockFactory.CreateIPopup(MessageBoxResult.No);
            mockPopUp.Setup(m => m.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Verifiable();
            var workflowHelper = new Mock<IWorkflowHelper>();
            var designerViewModel = new WorkflowDesignerViewModel(new Mock<IEventAggregator>().Object, resourceModel.Object, workflowHelper.Object, mockPopUp.Object, new SynchronousAsyncWorker(), new Mock<IExternalProcessExecutor>().Object, false);
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
            mockPopUp.Verify(m => m.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Once());
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
            var envRepo = new Mock<IEnvironmentRepository>();
            var envConn = new Mock<IEnvironmentConnection>();
            envConn.Setup(conn => conn.ServerEvents).Returns(new Mock<IEventPublisher>().Object);
            var env = new Mock<IEnvironmentModel>();
            env.Setup(e => e.ResourceRepository).Returns(resourceRepo.Object);
            env.Setup(e => e.Connection).Returns(envConn.Object);
            envRepo.Setup(r => r.All()).Returns(new List<IEnvironmentModel>(new[] { env.Object }));
            envRepo.Setup(r => r.Source).Returns(env.Object);
            envRepo.Setup(r => r.Get(It.IsAny<Guid>())).Returns(env.Object);

            Mock<IAsyncWorker> asyncWorker = AsyncWorkerTests.CreateSynchronousAsyncWorker();
            var mockMainViewModel = new MainViewModelPersistenceMock(envRepo.Object, asyncWorker.Object, false);
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

            Mock<IPopupController> mockPopUp = Dev2MockFactory.CreateIPopup(MessageBoxResult.OK);
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
            var envRepo = new Mock<IEnvironmentRepository>();
            var envConn = new Mock<IEnvironmentConnection>();
            envConn.Setup(conn => conn.ServerEvents).Returns(new Mock<IEventPublisher>().Object);
            var env = new Mock<IEnvironmentModel>();
            env.Setup(e => e.ResourceRepository).Returns(resourceRepo.Object);
            env.Setup(e => e.Connection).Returns(envConn.Object);
            envRepo.Setup(r => r.All()).Returns(new List<IEnvironmentModel>(new[] { env.Object }));
            envRepo.Setup(r => r.Source).Returns(env.Object);
            envRepo.Setup(r => r.Get(It.IsAny<Guid>())).Returns(env.Object);

            Mock<IAsyncWorker> asyncWorker = AsyncWorkerTests.CreateSynchronousAsyncWorker();
            var mockMainViewModel = new MainViewModelPersistenceMock(envRepo.Object, asyncWorker.Object, false);
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

            Mock<IPopupController> mockPopUp = Dev2MockFactory.CreateIPopup(MessageBoxResult.No);
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
            var envRepo = new Mock<IEnvironmentRepository>();
            var envConn = new Mock<IEnvironmentConnection>();
            envConn.Setup(conn => conn.ServerEvents).Returns(new Mock<IEventPublisher>().Object);
            var env = new Mock<IEnvironmentModel>();
            env.Setup(e => e.ResourceRepository).Returns(resourceRepo.Object);
            env.Setup(e => e.Connection).Returns(envConn.Object);
            envRepo.Setup(r => r.All()).Returns(new List<IEnvironmentModel>(new[] { env.Object }));
            envRepo.Setup(r => r.Source).Returns(env.Object);
            envRepo.Setup(r => r.Get(It.IsAny<Guid>())).Returns(env.Object);

            Mock<IAsyncWorker> asyncWorker = AsyncWorkerTests.CreateSynchronousAsyncWorker();
            var mockMainViewModel = new MainViewModelPersistenceMock(envRepo.Object, asyncWorker.Object);
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

            Mock<IPopupController> mockPopUp = Dev2MockFactory.CreateIPopup(MessageBoxResult.No);
            mockPopUp.Setup(m => m.Show()).Verifiable();

            mockMainViewModel.PopupProvider = mockPopUp.Object;

            mockMainViewModel.ActivateItem(mockMainViewModel.Items[0]);
            mockMainViewModel.ActivateItem(mockMainViewModel.Items[1]);
            mockMainViewModel.Handle(new RemoveResourceAndCloseTabMessage(mockMainViewModel.Items[1].ContextualResourceModel, false));
            Assert.AreEqual(mockMainViewModel.Items[0], mockMainViewModel.ActiveItem);
            mockPopUp.Verify(m => m.Show(), Times.Never());
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("MainViewModel")]
        public void MainViewModel_TryRemoveContext_Removes()
        {
            var wsiRepo = new Mock<IWorkspaceItemRepository>();
            wsiRepo.Setup(r => r.WorkspaceItems).Returns(() => new List<IWorkspaceItem>());
            wsiRepo.Setup(r => r.Write()).Verifiable();

            #region Setup ImportService - GRRR!

            SetupImportServiceForPersistenceTests(wsiRepo);

            #endregion

            var resourceRepo = new Mock<IResourceRepository>();
            resourceRepo.Setup(r => r.Save(It.IsAny<IResourceModel>())).Verifiable();
            var envRepo = new Mock<IEnvironmentRepository>();
            var envConn = new Mock<IEnvironmentConnection>();
            envConn.Setup(conn => conn.ServerEvents).Returns(new Mock<IEventPublisher>().Object);
            var env = new Mock<IEnvironmentModel>();
            env.Setup(e => e.ResourceRepository).Returns(resourceRepo.Object);
            env.Setup(e => e.Connection).Returns(envConn.Object);
            envRepo.Setup(r => r.All()).Returns(new List<IEnvironmentModel>(new[] { env.Object }));
            envRepo.Setup(r => r.Source).Returns(env.Object);
            envRepo.Setup(r => r.Get(It.IsAny<Guid>())).Returns(env.Object);

            Mock<IAsyncWorker> asyncWorker = AsyncWorkerTests.CreateSynchronousAsyncWorker();
            var mockMainViewModel = new MainViewModelPersistenceMock(envRepo.Object, asyncWorker.Object);
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

            Mock<IPopupController> mockPopUp = Dev2MockFactory.CreateIPopup(MessageBoxResult.No);
            mockPopUp.Setup(m => m.Show()).Verifiable();

            mockMainViewModel.PopupProvider = mockPopUp.Object;

            mockMainViewModel.ActivateItem(mockMainViewModel.Items[0]);
            mockMainViewModel.ActivateItem(mockMainViewModel.Items[1]);
            mockMainViewModel.TryRemoveContext(mockMainViewModel.Items[1].ContextualResourceModel);
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

            var envRepo = new Mock<IEnvironmentRepository>();
            ICollection<IEnvironmentModel> envColletion = new List<IEnvironmentModel>();
            var env = Dev2MockFactory.SetupEnvironmentModel();
            env.Setup(mock => mock.IsConnected).Returns(true);
            envColletion.Add(env.Object);

            envRepo.Setup(mock => mock.All()).Returns(envColletion);
            envRepo.Setup(mock => mock.Source).Returns(env.Object);

            Mock<IAsyncWorker> asyncWorker = AsyncWorkerTests.CreateSynchronousAsyncWorker();
            var viewModel = new MainViewModelPersistenceMock(envRepo.Object, asyncWorker.Object, false);

            viewModel.TestClose();
            wsiRepo.Verify(r => r.Write());
        }

        // PBI 9397 - 2013.06.09 - TWR: added
        [TestMethod]
        public void MainViewModelOnDeactivateWithTrueExpectedSavesResourceModels()
        {
            var wsiRepo = new Mock<IWorkspaceItemRepository>();
            wsiRepo.Setup(r => r.WorkspaceItems).Returns(() => new List<IWorkspaceItem>());
            wsiRepo.Setup(r => r.UpdateWorkspaceItem(It.IsAny<IContextualResourceModel>(), It.Is<bool>(b => b))).Returns(new ExecuteMessage()).Verifiable();
            SetupImportServiceForPersistenceTests(wsiRepo);

            var resourceID = Guid.NewGuid();
            var serverID = Guid.NewGuid();

            #region Setup resourceModel

            var resourceRepo = new Mock<IResourceRepository>();
            resourceRepo.Setup(r => r.Save(It.IsAny<IResourceModel>())).Verifiable();
            resourceRepo.Setup(r => r.FetchResourceDefinition(It.IsAny<IEnvironmentModel>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>())).Returns(new ExecuteMessage());

            var envConn = new Mock<IEnvironmentConnection>();
            var env = new Mock<IEnvironmentModel>();
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

            Mock<IPopupController> mockPopUp = Dev2MockFactory.CreateIPopup(MessageBoxResult.OK);
            var workflowHelper = new Mock<IWorkflowHelper>();
            var designerViewModel = new WorkflowDesignerViewModel(new Mock<IEventAggregator>().Object, resourceModel.Object, workflowHelper.Object, mockPopUp.Object, new SynchronousAsyncWorker(), new Mock<IExternalProcessExecutor>().Object, false);
            var contextViewModel = new WorkSurfaceContextViewModel(
                new WorkSurfaceKey { ResourceID = resourceID, ServerID = serverID, WorkSurfaceContext = designerViewModel.WorkSurfaceContext },
                designerViewModel);

            var envRepo = new Mock<IEnvironmentRepository>();
            envRepo.Setup(r => r.All()).Returns(new[] { env.Object });
            envRepo.Setup(e => e.Source).Returns(env.Object);
            Mock<IAsyncWorker> asyncWorker = AsyncWorkerTests.CreateSynchronousAsyncWorker();
            var viewModel = new MainViewModelPersistenceMock(envRepo.Object, asyncWorker.Object, false);
            viewModel.Items.Add(contextViewModel);

            viewModel.TestClose();

        }

        [TestMethod]
        public void MainViewModelOnDeactivateWithTrueExpectedSavesResourceModels_WhenEnvironmentNotConnectedDoesNotCallSave()
        {
            var wsiRepo = new Mock<IWorkspaceItemRepository>();
            wsiRepo.Setup(r => r.WorkspaceItems).Returns(() => new List<IWorkspaceItem>());
            wsiRepo.Setup(r => r.UpdateWorkspaceItem(It.IsAny<IContextualResourceModel>(), It.Is<bool>(b => b))).Verifiable();

            SetupImportServiceForPersistenceTests(wsiRepo);

            var resourceID = Guid.NewGuid();
            var serverID = Guid.NewGuid();

            #region Setup resourceModel

            var resourceRepo = new Mock<IResourceRepository>();
            resourceRepo.Setup(r => r.Save(It.IsAny<IResourceModel>())).Verifiable();

            var envConn = new Mock<IEnvironmentConnection>();
            var env = new Mock<IEnvironmentModel>();
            envConn.Setup(conn => conn.ServerEvents).Returns(new Mock<IEventPublisher>().Object);
            env.Setup(e => e.ResourceRepository).Returns(resourceRepo.Object);
            env.Setup(e => e.Connection).Returns(envConn.Object);
            env.Setup(e => e.IsConnected).Returns(false);

            var resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(m => m.Environment).Returns(env.Object);
            resourceModel.Setup(m => m.ID).Returns(resourceID);
            resourceModel.Setup(m => m.ResourceName).Returns("Some resource name 5");
            #endregion

            Mock<IPopupController> mockPopUp = Dev2MockFactory.CreateIPopup(MessageBoxResult.No);
            var workflowHelper = new Mock<IWorkflowHelper>();
            var designerViewModel = new WorkflowDesignerViewModel(new Mock<IEventAggregator>().Object, resourceModel.Object, workflowHelper.Object, mockPopUp.Object, new SynchronousAsyncWorker(), new Mock<IExternalProcessExecutor>().Object, false);
            var contextViewModel = new WorkSurfaceContextViewModel(
                new WorkSurfaceKey { ResourceID = resourceID, ServerID = serverID, WorkSurfaceContext = designerViewModel.WorkSurfaceContext },
                designerViewModel);

            var envRepo = new Mock<IEnvironmentRepository>();
            envRepo.Setup(r => r.All()).Returns(new[] { env.Object });
            envRepo.Setup(e => e.Source).Returns(env.Object);
            var viewModel = new MainViewModelPersistenceMock(envRepo.Object, false);
            viewModel.Items.Add(contextViewModel);

            viewModel.TestClose();

            wsiRepo.Verify(r => r.UpdateWorkspaceItem(It.IsAny<IContextualResourceModel>(), It.Is<bool>(b => b)), Times.Never());
            resourceRepo.Verify(r => r.Save(It.IsAny<IResourceModel>()), Times.Never());
        }

        #endregion

        #region SetupImportServiceForPersistenceTests

        static void SetupImportServiceForPersistenceTests(Mock<IWorkspaceItemRepository> wsiRepo)
        {
            var workspaceItemRepository = new WorkspaceItemRepository(wsiRepo.Object);
            CustomContainer.Register<IWorkspaceItemRepository>(workspaceItemRepository);
            CustomContainer.Register(new Mock<IPopupController>().Object);
        }

        #endregion

        #region BrowserPopupController

        [TestMethod]
        public void MainViewModelShowCommunityPageExpectedInvokesConstructorsBrowserPopupController()
        {
            var popupController = new Mock<IBrowserPopupController>();
            popupController.Setup(p => p.ShowPopup(It.IsAny<string>())).Verifiable();
            CustomContainer.Register(new Mock<IWindowManager>().Object);
            var envRepo = new Mock<IEnvironmentRepository>();
            envRepo.Setup(e => e.All()).Returns(new List<IEnvironmentModel>());
            envRepo.Setup(e => e.Source).Returns(new Mock<IEnvironmentModel>().Object);
            var vm = new MainViewModel(new Mock<IEventAggregator>().Object, new Mock<IAsyncWorker>().Object, envRepo.Object, new Mock<IVersionChecker>().Object, false, popupController.Object);
            vm.ShowCommunityPage();

            popupController.Verify(p => p.ShowPopup(It.IsAny<string>()));
        }

        [TestMethod]
        public void MainViewModelConstructorWithNullBrowserPopupControllerExpectedCreatesExternalBrowserPopupController()
        {
            var mockEventAggregator = new Mock<IEventAggregator>();
            var envRepo = new Mock<IEnvironmentRepository>();
            CustomContainer.Register(new Mock<IWindowManager>().Object);
            envRepo.Setup(e => e.All()).Returns(new List<IEnvironmentModel>());
            envRepo.Setup(e => e.Source).Returns(new Mock<IEnvironmentModel>().Object);
            // ReSharper disable once RedundantArgumentDefaultValue
            var vm = new MainViewModel(mockEventAggregator.Object, new Mock<IAsyncWorker>().Object, envRepo.Object, new Mock<IVersionChecker>().Object, false, null);
            Assert.IsInstanceOfType(vm.BrowserPopupController, typeof(ExternalBrowserPopupController));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void MainViewModelConstructorWithNullVersionCheckerExpectedThrowsArgumentNullException()
        {
            var mockEventAggregator = new Mock<IEventAggregator>();
            var envRepo = new Mock<IEnvironmentRepository>();
            envRepo.Setup(e => e.All()).Returns(new List<IEnvironmentModel>());
            envRepo.Setup(e => e.Source).Returns(new Mock<IEnvironmentModel>().Object);

            new MainViewModel(mockEventAggregator.Object, new Mock<IAsyncWorker>().Object, envRepo.Object, null);
        }

        #endregion

        #region ActiveEnvironment

        [TestMethod]
        [TestCategory("MainViewModel_SetActiveEnvironmentMessage")]
        [Owner("Trevor Williams-Ros")]
        public void MainViewModel_SetActiveEnvironmentMessage_SetsActiveEnvironmentOnEnvironmentRepository()
        {
            Mock<IEnvironmentModel> mockEnv = Dev2MockFactory.SetupEnvironmentModel();
            CreateFullExportsAndVmWithEmptyRepo();
            EmptyEnvRepo.Setup(r => r.Get(It.IsAny<Guid>())).Returns(mockEnv.Object);
            MainViewModel.Handle(new SetActiveEnvironmentMessage(mockEnv.Object));
            
            Assert.AreSame(mockEnv.Object, MainViewModel.EnvironmentRepository.ActiveEnvironment);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("MainViewModel_AuthorizeCommands")]
        public void MainViewModel_AuthorizeCommands_AuthorizationContextIsCorrect()
        {
            //------------Setup for test--------------------------    

            //------------Execute Test---------------------------
            CreateFullExportsAndVmWithEmptyRepo();

            //------------Assert Results-------------------------
            Assert.AreEqual(AuthorizationContext.Contribute, MainViewModel.NewResourceCommand.AuthorizationContext);
            Assert.AreEqual(AuthorizationContext.Administrator, MainViewModel.SettingsCommand.AuthorizationContext);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("MainViewModel_AuthorizeCommands")]
        public void MainViewModel_AuthorizeCommands_ActiveEnvironmentChanged_UpdateContextInvoked()
        {
            //------------Setup for test--------------------------            
            CreateFullExportsAndVmWithEmptyRepo();

            Assert.IsNull(MainViewModel.NewResourceCommand.AuthorizationService);
            Assert.IsNull(MainViewModel.SettingsCommand.AuthorizationService);

            var authService = new Mock<IAuthorizationService>();

            var env = new Mock<IEnvironmentModel>();
            env.Setup(e => e.AuthorizationService).Returns(authService.Object);

            //------------Execute Test---------------------------
            MainViewModel.ActiveEnvironment = env.Object;

            //------------Assert Results-------------------------
            Assert.AreSame(authService.Object, MainViewModel.NewResourceCommand.AuthorizationService);
            Assert.AreSame(authService.Object, MainViewModel.SettingsCommand.AuthorizationService);
        }

        [TestMethod]
        public void IsActiveEnvironmentConnectExpectFalseWithNullEnvironment()
        {
            CreateFullExportsAndVm();
            MainViewModel.ActiveItem = MainViewModel.Items.FirstOrDefault(c => c.WorkSurfaceViewModel.GetType() == typeof(HelpViewModel));
            var actual = MainViewModel.IsActiveEnvironmentConnected();
            Assert.IsFalse(actual);
            Assert.IsFalse(MainViewModel.HasActiveConnection);
        }

        #endregion

        #region OnStudioClosing

        [TestMethod]
        [TestCategory("MainViewModel_OnStudioClosing")]
        [Owner("Leon Rajindrapersadh")]
        public void MainViewModel_OnStudioClosing_CallsSchedulerOnClosing()
        {
            //Barney, commented out when I removed the feedback stuff from the studio
            //SetupDefaultMef();
            
            var eventPublisher = new Mock<IEventAggregator>();
            var environmentRepository = new Mock<IEnvironmentRepository>();
            var environmentModel = new Mock<IEnvironmentModel>().Object;
            environmentRepository.Setup(repo => repo.Source).Returns(environmentModel);

            var viewModel = new Mock<IShellViewModel>();
            IServer server = (IServer)CustomContainer.Get(typeof(IServer));
            viewModel.SetupGet(model => model.ActiveServer).Returns(server);

            CustomContainer.Register(viewModel.Object);

            var versionChecker = new Mock<IVersionChecker>();
            var asyncWorker = new Mock<IAsyncWorker>();
            asyncWorker.Setup(w => w.Start(It.IsAny<System.Action>(), It.IsAny<System.Action>())).Verifiable();
            var connected1 = new Mock<IEnvironmentModel>();
            var connected2 = new Mock<IEnvironmentModel>();
            var notConnected = new Mock<IEnvironmentModel>();
            connected1.Setup(a => a.IsConnected).Returns(true).Verifiable();
            connected1.Setup(a => a.Disconnect()).Verifiable();
            connected2.Setup(a => a.IsConnected).Returns(true).Verifiable();
            connected2.Setup(a => a.Disconnect()).Verifiable();
            IList<IEnvironmentModel> lst = new List<IEnvironmentModel> { connected1.Object, connected2.Object, notConnected.Object };
            environmentRepository.Setup(repo => repo.All()).Returns(lst);
              environmentRepository.Setup(repo => repo.Get(It.IsAny<Guid>())).Returns(connected1.Object);
            var mvm = new MainViewModel(eventPublisher.Object, asyncWorker.Object, environmentRepository.Object, versionChecker.Object, false);
            var popup = new Mock<IPopupController>();
            popup.Setup(a => a.ShowSchedulerCloseConfirmation()).Returns(MessageBoxResult.Cancel).Verifiable();
            var scheduler = new SchedulerViewModel(EventPublishers.Aggregator, new DirectoryObjectPickerDialog(), popup.Object, new SynchronousAsyncWorker(), new Mock<IServer>().Object, a => environmentModel) { WorkSurfaceContext = WorkSurfaceContext.Scheduler };
            var task = new Mock<IScheduledResource>();
            task.Setup(a => a.IsDirty).Returns(true);
            scheduler.SelectedTask = task.Object;
            var vm = new WorkSurfaceContextViewModel(new EventAggregator(), new WorkSurfaceKey(), scheduler, new Mock<IPopupController>().Object, (a, b,c) => { });

            mvm.Items.Add(vm);
            Assert.IsFalse(mvm.OnStudioClosing());

        }

        [TestMethod]
        [TestCategory("MainViewModel_OnStudioClosing")]
        [Owner("Leon Rajindrapersadh")]
        public void MainViewModel_OnStudioClosing_ClosesRemoteEnvironmants()
        {
            //Barney, commented out when I removed the feedback stuff from the studio
            //SetupDefaultMef();
            var viewModel = new Mock<IShellViewModel>();
            IServer server = (IServer)CustomContainer.Get(typeof(IServer));
            viewModel.SetupGet(model => model.ActiveServer).Returns(server);

            CustomContainer.Register(viewModel.Object);

            var eventPublisher = new Mock<IEventAggregator>();
            var environmentRepository = new Mock<IEnvironmentRepository>();
            var connected1 = new Mock<IEnvironmentModel>();
            var connected2 = new Mock<IEnvironmentModel>();
            var notConnected = new Mock<IEnvironmentModel>();
            connected1.Setup(a => a.IsConnected).Returns(true).Verifiable();
            connected1.Setup(a => a.Disconnect()).Verifiable();
            connected2.Setup(a => a.IsConnected).Returns(true).Verifiable();
            connected2.Setup(a => a.Disconnect()).Verifiable();
            notConnected.Setup(a => a.IsConnected).Returns(false).Verifiable();
            IList<IEnvironmentModel> lst = new List<IEnvironmentModel> { connected1.Object, connected2.Object, notConnected.Object };

            environmentRepository.Setup(repo => repo.Source).Returns(new Mock<IEnvironmentModel>().Object);
            environmentRepository.Setup(repo => repo.All()).Returns(lst);
                    environmentRepository.Setup(a => a.Get(It.IsAny<Guid>())).Returns(connected1.Object);
            var versionChecker = new Mock<IVersionChecker>();
            var asyncWorker = new Mock<IAsyncWorker>();
            asyncWorker.Setup(w => w.Start(It.IsAny<System.Action>(), It.IsAny<System.Action>())).Verifiable();
            var mvm = new MainViewModel(eventPublisher.Object, asyncWorker.Object, environmentRepository.Object, versionChecker.Object, false);
            var popup = new Mock<IPopupController>();
            popup.Setup(a => a.ShowSchedulerCloseConfirmation()).Returns(MessageBoxResult.Cancel).Verifiable();
            var scheduler = new SchedulerViewModel(EventPublishers.Aggregator, new DirectoryObjectPickerDialog(), popup.Object, new SynchronousAsyncWorker(), new Mock<IServer>().Object, a => new Mock<IEnvironmentModel>().Object) { WorkSurfaceContext = WorkSurfaceContext.Scheduler };
            var task = new Mock<IScheduledResource>();
            task.Setup(a => a.IsDirty).Returns(false);
            scheduler.SelectedTask = task.Object;
            var vm = new WorkSurfaceContextViewModel(new EventAggregator(), new WorkSurfaceKey(), scheduler, new Mock<IPopupController>().Object, (a, b,c) => { });

            mvm.Items.Add(vm);
            Assert.IsTrue(mvm.OnStudioClosing());   // assert that the studio closes
            connected1.Verify(a => a.IsConnected);
            connected1.Verify(a => a.Disconnect());
            connected2.Verify(a => a.IsConnected);
            connected2.Verify(a => a.Disconnect());
            notConnected.Verify(a => a.IsConnected);

        }


        [TestMethod]
        [TestCategory("MainViewModel_OnStudioClosing")]
        [Owner("Leon Rajindrapersadh")]
        public void MainViewModel_OnStudioClosing_CallsSettingsOnClosing()
        {
            //Barney, commented out when I removed the feedback stuff from the studio
            //SetupDefaultMef();

            var viewModel = new Mock<IShellViewModel>();
            var server = new Mock<IServer>();
            server.SetupGet(server1 => server1.IsConnected).Returns(true);
            viewModel.SetupGet(model => model.ActiveServer).Returns(server.Object);
            viewModel.SetupGet(model => model.ActiveServer).Returns(server.Object);
            viewModel.SetupGet(model => model.LocalhostServer).Returns(server.Object);
            CustomContainer.Register(viewModel.Object);

            var eventPublisher = new Mock<IEventAggregator>();
            var environmentRepository = new Mock<IEnvironmentRepository>();
            
            var environmentModel = new Mock<IEnvironmentModel>();
            var environmentConnection = new Mock<IEnvironmentConnection>().Object;
            environmentModel.SetupGet(a => a.Connection).Returns(environmentConnection);
            environmentModel.SetupGet(a => a.IsLocalHost).Returns(true);
            Mock<IAuthorizationService> mockAuthService = new Mock<IAuthorizationService>();
            var mockSecurityService = new Mock<ISecurityService>();
            mockSecurityService.Setup(a => a.Permissions).Returns(new List<WindowsGroupPermission>(new[] { WindowsGroupPermission.CreateEveryone(), }));
            mockAuthService.SetupGet(service => service.SecurityService).Returns(mockSecurityService.Object);
            environmentModel.Setup(c => c.AuthorizationService).Returns(mockAuthService.Object);
            environmentRepository.Setup(repo => repo.Source).Returns(environmentModel.Object);
            environmentModel.SetupGet(a => a.IsConnected).Returns(true);
            var versionChecker = new Mock<IVersionChecker>();
            var asyncWorker = new Mock<IAsyncWorker>();
            asyncWorker.Setup(w => w.Start(It.IsAny<System.Action>(), It.IsAny<System.Action>())).Verifiable();
            var connected1 = new Mock<IEnvironmentModel>();
            var connected2 = new Mock<IEnvironmentModel>();
            var notConnected = new Mock<IEnvironmentModel>();
            connected1.Setup(a => a.IsConnected).Returns(true).Verifiable();
            connected1.Setup(a => a.Disconnect()).Verifiable();
            connected2.Setup(a => a.IsConnected).Returns(true).Verifiable();
            connected2.Setup(a => a.Disconnect()).Verifiable();
            IList<IEnvironmentModel> lst = new List<IEnvironmentModel> { connected1.Object, connected2.Object, notConnected.Object };
            environmentRepository.Setup(repo => repo.All()).Returns(lst);
                
            var mvm = new MainViewModel(eventPublisher.Object, asyncWorker.Object, environmentRepository.Object, versionChecker.Object, false);
            var popup = new Mock<IPopupController>();
            popup.Setup(a => a.ShowSchedulerCloseConfirmation()).Returns(MessageBoxResult.Cancel).Verifiable();

            EnvironmentRepository.Instance.ActiveEnvironment = environmentModel.Object;
            
            var settings = new SettingsViewModelForTest(EventPublishers.Aggregator, popup.Object, new SynchronousAsyncWorker(), new NativeWindow()) { RetValue = false, WorkSurfaceContext = WorkSurfaceContext.Settings };
            var task = new Mock<IScheduledResource>();
            task.Setup(a => a.IsDirty).Returns(true);
            settings.IsDirty = true;
            var vm = new WorkSurfaceContextViewModel(new EventAggregator(), new WorkSurfaceKey(), settings, new Mock<IPopupController>().Object, (a, b,c) => { });

            mvm.Items.Add(vm);
            Assert.IsFalse(mvm.OnStudioClosing());

        }


        [TestMethod]
        [TestCategory("MainViewModel_OnStudioClosing")]
        [Owner("Leon Rajindrapersadh")]
        public void MainViewModel_OnStudioClosing_CallsSettingsOnClosingDirty()
        {
            //Barney, commented out when I removed the feedback stuff from the studio
            //SetupDefaultMef();
            var viewModel = new Mock<IShellViewModel>();
            IServer server = (IServer)CustomContainer.Get(typeof(IServer));
            viewModel.SetupGet(model => model.ActiveServer).Returns(server);
            CustomContainer.Register(viewModel.Object);

            var eventPublisher = new Mock<IEventAggregator>();
            var environmentRepository = new Mock<IEnvironmentRepository>();
            environmentRepository.Setup(repo => repo.Source).Returns(new Mock<IEnvironmentModel>().Object);
            var versionChecker = new Mock<IVersionChecker>();
            var asyncWorker = new Mock<IAsyncWorker>();
            asyncWorker.Setup(w => w.Start(It.IsAny<System.Action>(), It.IsAny<System.Action>())).Verifiable();
            var connected1 = new Mock<IEnvironmentModel>();
            var connected2 = new Mock<IEnvironmentModel>();
            var notConnected = new Mock<IEnvironmentModel>();
            connected1.Setup(a => a.IsConnected).Returns(true).Verifiable();
            connected1.Setup(a => a.Disconnect()).Verifiable();
            connected2.Setup(a => a.IsConnected).Returns(true).Verifiable();
            connected2.Setup(a => a.Disconnect()).Verifiable();
            IList<IEnvironmentModel> lst = new List<IEnvironmentModel> { connected1.Object, connected2.Object, notConnected.Object };
            environmentRepository.Setup(repo => repo.All()).Returns(lst);
            var mvm = new MainViewModel(eventPublisher.Object, asyncWorker.Object, environmentRepository.Object, versionChecker.Object, false);
            var popup = new Mock<IPopupController>();

            var settings = new SettingsViewModelForTest(EventPublishers.Aggregator, popup.Object, new SynchronousAsyncWorker(), new NativeWindow()) { RetValue = true, WorkSurfaceContext = WorkSurfaceContext.Settings };
            var task = new Mock<IScheduledResource>();
            task.Setup(a => a.IsDirty).Returns(true);
            settings.IsDirty = true;
            var vm = new WorkSurfaceContextViewModel(new EventAggregator(), new WorkSurfaceKey(), settings, new Mock<IPopupController>().Object, (a, b,c) => { });
            environmentRepository.Setup(repo => repo.All()).Returns(new List<IEnvironmentModel>());
            mvm.Items.Add(vm);
            Assert.IsTrue(mvm.OnStudioClosing());

        }

        [TestMethod]
        [TestCategory("MainViewModel_OnStudioClosing")]
        [Owner("Leon Rajindrapersadh")]
        public void MainViewModel_OnStudioClosing_CallsSchedulerOnClosingClosesSuccessfully()
        {
            //Barney, commented out when I removed the feedback stuff from the studio
            //SetupDefaultMef();

            var viewModel = new Mock<IShellViewModel>();
            IServer server = (IServer)CustomContainer.Get(typeof(IServer));
            viewModel.SetupGet(model => model.ActiveServer).Returns(server);

            CustomContainer.Register(viewModel.Object);

            var eventPublisher = new Mock<IEventAggregator>();
            var environmentRepository = new Mock<IEnvironmentRepository>();
            environmentRepository.Setup(repo => repo.Source).Returns(new Mock<IEnvironmentModel>().Object);
            var versionChecker = new Mock<IVersionChecker>();
            var asyncWorker = new Mock<IAsyncWorker>();
            asyncWorker.Setup(w => w.Start(It.IsAny<System.Action>(), It.IsAny<System.Action>())).Verifiable();
            var connected1 = new Mock<IEnvironmentModel>();
            var connected2 = new Mock<IEnvironmentModel>();
            var notConnected = new Mock<IEnvironmentModel>();
            connected1.Setup(a => a.IsConnected).Returns(true).Verifiable();
            connected1.Setup(a => a.Disconnect()).Verifiable();
            connected2.Setup(a => a.IsConnected).Returns(true).Verifiable();
            connected2.Setup(a => a.Disconnect()).Verifiable();
            IList<IEnvironmentModel> lst = new List<IEnvironmentModel> { connected1.Object, connected2.Object, notConnected.Object };
            environmentRepository.Setup(repo => repo.All()).Returns(lst);
            var mvm = new MainViewModel(eventPublisher.Object, asyncWorker.Object, environmentRepository.Object, versionChecker.Object, false);
            var popup = new Mock<IPopupController>();
            popup.Setup(a => a.ShowSchedulerCloseConfirmation()).Returns(MessageBoxResult.Yes).Verifiable();
            var scheduler = new SchedulerViewModelForTesting(EventPublishers.Aggregator, new DirectoryObjectPickerDialog(), popup.Object, new SynchronousAsyncWorker()) { RetValue = true, WorkSurfaceContext = WorkSurfaceContext.Scheduler };
            var task = new Mock<IScheduledResource>();
            task.Setup(a => a.IsDirty).Returns(true);
            scheduler.SelectedTask = task.Object;
            var vm = new WorkSurfaceContextViewModel(new EventAggregator(), new WorkSurfaceKey(), scheduler, new Mock<IPopupController>().Object, (a, b,c) => { });
            environmentRepository.Setup(repo => repo.All()).Returns(new List<IEnvironmentModel>());
            mvm.Items.Add(vm);
            Assert.IsTrue(mvm.OnStudioClosing());

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
            var eventAggregator = new Mock<IEventAggregator>();

            var envRepo = new Mock<IEnvironmentRepository>();
            envRepo.Setup(e => e.All()).Returns(new List<IEnvironmentModel>());
            var environmentModel = new Mock<IEnvironmentModel>().Object;
            envRepo.Setup(e => e.Source).Returns(environmentModel);
            envRepo.Setup(e => e.Source.IsConnected).Returns(false);
            envRepo.Setup(e => e.Source.Connection.IsConnected).Returns(false);

            var vm = new MainViewModel(eventAggregator.Object, new SynchronousAsyncWorker(), envRepo.Object, new Mock<IVersionChecker>().Object, false, new Mock<IBrowserPopupController>().Object);

            var workSurfaceContextViewModel = new Mock<WorkSurfaceContextViewModel>(eventAggregator.Object, new WorkSurfaceKey(), new Mock<IWorkSurfaceViewModel>().Object, new Mock<IPopupController>().Object, new Action<IContextualResourceModel, bool, System.Action>(
                (a, b,c) => { }));
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

            var environmentModel = new Mock<IEnvironmentModel>();
            environmentModel.SetupGet(e => e.ID).Returns(environmentId);

            var environmentConnection = new Mock<IEnvironmentConnection>();
            environmentConnection.SetupGet(env => env.WorkspaceID).Returns(workspaceId);

            environmentModel.SetupGet(e => e.Connection).Returns(environmentConnection.Object);

            var resourceRepository = new Mock<IResourceRepository>();
            resourceRepository.Setup(repository => repository.ReloadResource(It.IsAny<Guid>(), It.IsAny<ResourceType>(), It.IsAny<IEqualityComparer<IResourceModel>>(), It.IsAny<bool>())).Returns(() =>
            {
                Thread.Sleep(100);
                return new List<IResourceModel>();
            });
            environmentModel.SetupGet(e => e.ResourceRepository).Returns(resourceRepository.Object);
            resourceModel.SetupGet(r => r.Environment).Returns(environmentModel.Object);

            var environmentRepository = new Mock<IEnvironmentRepository>();
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

            var environmentModel = new Mock<IEnvironmentModel>();
            environmentModel.SetupGet(e => e.ID).Returns(environmentId);

            var environmentConnection = new Mock<IEnvironmentConnection>();
            environmentConnection.SetupGet(env => env.WorkspaceID).Returns(workspaceId);

            environmentModel.SetupGet(e => e.Connection).Returns(environmentConnection.Object);

            var resourceRepository = new Mock<IResourceRepository>();
            resourceRepository.Setup(repository => repository.ReloadResource(It.IsAny<Guid>(), It.IsAny<ResourceType>(), It.IsAny<IEqualityComparer<IResourceModel>>(), It.IsAny<bool>())).Returns(() =>
            {
                Thread.Sleep(100);
                return new List<IResourceModel>();
            });
            environmentModel.SetupGet(e => e.ResourceRepository).Returns(resourceRepository.Object);
            resourceModel.SetupGet(r => r.Environment).Returns(environmentModel.Object);

            var environmentRepository = new Mock<IEnvironmentRepository>();
            environmentModel.Setup(c => c.CanStudioExecute).Returns(false);
            environmentRepository.Setup(c => c.Source).Returns(environmentModel.Object);
            environmentRepository.Setup(c => c.All()).Returns(new[] { environmentModel.Object });
            //            new Thread(() =>
            //            {
            //                var vm = new MainViewModel(new Mock<IEventAggregator>().Object,
            //                                     new SynchronousAsyncWorker(),
            //                                     environmentRepository.Object,
            //                                     new Mock<IVersionChecker>().Object,
            //                                     false,
            //                                     new Mock<IBrowserPopupController>().Object,
            //                                     new Mock<IPopupController>().Object,
            //                                     new Mock<IWindowManager>().Object,
            //                                     new Mock<IWebController>().Object,
            //                                     new Mock<IFeedbackInvoker>().Object);
            //
            //                var workspaceItemRepository = new Mock<IWorkspaceItemRepository>();
            //                workspaceItemRepository.SetupGet(p => p.WorkspaceItems).Returns(new List<IWorkspaceItem>());
            //
            //                vm.GetWorkspaceItemRepository = () => workspaceItemRepository.Object;
            //
            //                var tasks = new List<Task>
            //                {
            //                    Task.Factory.StartNew(() => vm.AddWorkSurfaceContextFromWorkspace(resourceModel.Object))
            //                };
            //
            //                Task.WaitAll(tasks.ToArray());
            //
            //                Assert.IsFalse(resourceModel.Object.IsWorkflowSaved);
            //            });

        }

        [TestMethod]
        [TestCategory("MainViewModel_IsDownloading")]
        [Owner("Tshepo Ntlhokoa")]
        public void MainViewModel_IsDownloading_IsBusyDownloadingInstallerIsNull_False()
        {
            //------------Setup for test--------------------------
            var localhost = new Mock<IEnvironmentModel>();
            localhost.Setup(e => e.ID).Returns(Guid.Empty);
            localhost.Setup(e => e.IsConnected).Returns(true); // so that we load resources
            var environmentRepository = new Mock<IEnvironmentRepository>();
            //environmentRepository.Setup(c => c.ReadSession()).Returns(new[] { Guid.NewGuid() });
            environmentRepository.Setup(c => c.All()).Returns(new[] { localhost.Object });
            environmentRepository.Setup(c => c.Source).Returns(localhost.Object);
            var eventPublisher = new Mock<IEventAggregator>();
            var asyncWorker = AsyncWorkerTests.CreateSynchronousAsyncWorker();
            var versionChecker = new Mock<IVersionChecker>();
            var browserPopupController = new Mock<IBrowserPopupController>();

            var viewModel = new MainViewModelMock(eventPublisher.Object, asyncWorker.Object, environmentRepository.Object, versionChecker.Object, new Mock<IStudioResourceRepository>().Object, new Mock<IConnectControlSingleton>().Object, new Mock<CustomControls.Connections.IConnectControlViewModel>().Object, false, browserPopupController.Object) { IsBusyDownloadingInstaller = null };

            //------------Execute Test---------------------------
            var isDownloading = viewModel.IsDownloading();
            //------------Assert Results-------------------------
            Assert.IsFalse(isDownloading);
        }

        [TestMethod]
        [TestCategory("MainViewModel_Handle_DisplayMessageBoxMessage")]
        [Owner("Hagashen Naidu")]
        public void MainViewModel_HandleMessageBoxMessage_CallsPopupShow()
        {
            //------------Setup for test--------------------------
            var localhost = new Mock<IEnvironmentModel>();
            localhost.Setup(e => e.ID).Returns(Guid.Empty);
            localhost.Setup(e => e.IsConnected).Returns(true); // so that we load resources
            var environmentRepository = new Mock<IEnvironmentRepository>();
            environmentRepository.Setup(c => c.All()).Returns(new[] { localhost.Object });
            environmentRepository.Setup(c => c.Source).Returns(localhost.Object);
            var eventPublisher = new Mock<IEventAggregator>();
            var asyncWorker = AsyncWorkerTests.CreateSynchronousAsyncWorker();
            var versionChecker = new Mock<IVersionChecker>();
            var browserPopupController = new Mock<IBrowserPopupController>();
            var viewModel = new MainViewModelMock(eventPublisher.Object, asyncWorker.Object, environmentRepository.Object, versionChecker.Object, new Mock<IStudioResourceRepository>().Object, new Mock<IConnectControlSingleton>().Object, new Mock<CustomControls.Connections.IConnectControlViewModel>().Object, false, browserPopupController.Object) { IsBusyDownloadingInstaller = null };
            Mock<IPopupController> mockPopupController = new Mock<IPopupController>();
            mockPopupController.Setup(controller => controller.Show("Some message", "Some heading", MessageBoxButton.OK, MessageBoxImage.Warning, "", false, false, true, false)).Verifiable();
            viewModel.PopupProvider = mockPopupController.Object;
            //------------Execute Test---------------------------
            viewModel.Handle(new DisplayMessageBoxMessage("Some heading", "Some message", MessageBoxImage.Warning));
            //------------Assert Results-------------------------
            mockPopupController.Verify(controller => controller.Show("Some message", "Some heading", MessageBoxButton.OK, MessageBoxImage.Warning, "", false, false, true, false));
        }

        [TestMethod]
        [TestCategory("MainViewModel_IsDownloading")]
        [Owner("Tshepo Ntlhokoa")]
        public void MainViewModel_IsDownloading_IsBusyDownloadingInstallerReturnsFalse_False()
        {
            //------------Setup for test--------------------------
            var localhost = new Mock<IEnvironmentModel>();
            localhost.Setup(e => e.ID).Returns(Guid.Empty);
            localhost.Setup(e => e.IsConnected).Returns(true); // so that we load resources
            var environmentRepository = new Mock<IEnvironmentRepository>();
            environmentRepository.Setup(c => c.All()).Returns(new[] { localhost.Object });
            environmentRepository.Setup(c => c.Source).Returns(localhost.Object);
            var eventPublisher = new Mock<IEventAggregator>();
            var asyncWorker = AsyncWorkerTests.CreateSynchronousAsyncWorker();
            var versionChecker = new Mock<IVersionChecker>();
            var browserPopupController = new Mock<IBrowserPopupController>();
            var viewModel = new MainViewModelMock(eventPublisher.Object, asyncWorker.Object, environmentRepository.Object, versionChecker.Object, new Mock<IStudioResourceRepository>().Object, new Mock<IConnectControlSingleton>().Object, new Mock<CustomControls.Connections.IConnectControlViewModel>().Object, false, browserPopupController.Object) { IsBusyDownloadingInstaller = () => false };
            //------------Execute Test---------------------------
            var isDownloading = viewModel.IsDownloading();
            //------------Assert Results-------------------------
            Assert.IsFalse(isDownloading);
        }
    }

    public class SchedulerViewModelForTesting : SchedulerViewModel
    {
        public SchedulerViewModelForTesting()
        {

        }
        // ReSharper disable TooManyDependencies
        public SchedulerViewModelForTesting(IEventAggregator eventPublisher, DirectoryObjectPickerDialog directoryObjectPicker, IPopupController popupController, IAsyncWorker asyncWorker)
            // ReSharper restore TooManyDependencies
            : base(eventPublisher, directoryObjectPicker, popupController, asyncWorker, new Mock<IServer>().Object, a => new Mock<IEnvironmentModel>().Object)
        {

        }

        public override bool DoDeactivate(bool b)
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

        // ReSharper disable TooManyDependencies
        public SettingsViewModelForTest(IEventAggregator eventPublisher, IPopupController popupController,
            // ReSharper restore TooManyDependencies
                                       IAsyncWorker asyncWorker, IWin32Window parentWindow)
            : base(eventPublisher, popupController, asyncWorker, parentWindow, new Mock<IServer>().Object, a => new Mock<IEnvironmentModel>().Object)
        {
        }


        public override bool DoDeactivate(bool b)
        {
            return RetValue;
        }

        public bool RetValue { get; set; }
    }
    // ReSharper restore MaximumChainedReferences
}
