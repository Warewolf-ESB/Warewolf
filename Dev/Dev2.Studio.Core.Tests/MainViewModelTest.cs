using Caliburn.Micro;
using CubicOrange.Windows.Forms.ActiveDirectory;
using Dev2.AppResources.Repositories;
using Dev2.Common.Interfaces.Infrastructure.Events;
using Dev2.Common.Interfaces.Scheduler.Interfaces;
using Dev2.Common.Interfaces.Security;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Communication;
using Dev2.ConnectionHelpers;
using Dev2.Core.Tests.Utils;
using Dev2.CustomControls.Connections;
using Dev2.Factory;
using Dev2.Models;
using Dev2.Services.Events;
using Dev2.Services.Security;
using Dev2.Settings;
using Dev2.Settings.Scheduler;
using Dev2.Studio.AppResources.Comparers;
using Dev2.Studio.Core.AppResources.Browsers;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Helpers;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Interfaces.DataList;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Core.Workspaces;
using Dev2.Studio.Feedback;
using Dev2.Studio.Feedback.Actions;
using Dev2.Studio.ViewModels;
using Dev2.Studio.ViewModels.DependencyVisualization;
using Dev2.Studio.ViewModels.Help;
using Dev2.Studio.ViewModels.Workflow;
using Dev2.Studio.ViewModels.WorkSurface;
using Dev2.Threading;
using Dev2.Util;
using Dev2.Utilities;
using Dev2.Webs;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using Action = System.Action;
using FileHelper = Dev2.Studio.Core.Helpers.FileHelper;

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
            AuthorizationService.Setup(a => a.IsAuthorized(AuthorizationContext.Administrator, It.IsAny<string>())).Returns(isAuthorized);

            var actual = MainViewModel.SettingsCommand.CanExecute(null);
            Assert.AreEqual(expected, actual);
        }

        #region Constructor

        [TestMethod]
        [TestCategory("MainViewModel_Constructor")]
        [Description("MainViewModel constructor must show start page")]
        [Owner("Ashley Lewis")]
        public void MainViewModel_UnitTest_Constructor_ShowStartPage()
        {
            //isolate unit
            var eventPublisher = new Mock<IEventAggregator>();
            var environmentRepository = new Mock<IEnvironmentRepository>();
            var environmentModel = new Mock<IEnvironmentModel>();
            environmentModel.Setup(c => c.CanStudioExecute).Returns(false);
            environmentRepository.Setup(c => c.Source).Returns(environmentModel.Object);
            environmentRepository.Setup(c => c.ReadSession()).Returns(new[] { Guid.NewGuid() });
            environmentRepository.Setup(c => c.All()).Returns(new[] { environmentModel.Object });
            var versionChecker = new Mock<IVersionChecker>();
            var asyncWorker = new Mock<IAsyncWorker>();

            var mvm = new Mock<MainViewModel>(eventPublisher.Object, asyncWorker.Object, environmentRepository.Object, versionChecker.Object, false, null, null, null, null, null, new Mock<IStudioResourceRepository>().Object, new Mock<IConnectControlSingleton>().Object, new Mock<IConnectControlViewModel>().Object);
            mvm.Setup(c => c.ShowStartPage()).Verifiable();

            //construct
            var concreteMvm = mvm.Object;

            //test result
            Assert.IsNotNull(concreteMvm);
            mvm.Verify(c => c.ShowStartPage(), Times.Once());
        }

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
            resourceRepo.Setup(r => r.FetchResourceDefinition(It.IsAny<IEnvironmentModel>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new ExecuteMessage());


            var envConn = new Mock<IEnvironmentConnection>();
            envConn.Setup(conn => conn.ServerEvents).Returns(new Mock<IEventPublisher>().Object);
            envConn.Setup(conn => conn.WorkspaceID).Returns(workspaceID);
            envConn.Setup(conn => conn.ServerID).Returns(serverID);
            envConn.Setup(conn => conn.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new StringBuilder());

            var env = new Mock<IEnvironmentModel>();
            env.Setup(e => e.Connection).Returns(envConn.Object);
            env.Setup(e => e.IsConnected).Returns(true);
            env.Setup(e => e.ResourceRepository).Returns(resourceRepo.Object);

            resourceModel.Setup(m => m.Environment).Returns(env.Object);

            var envRepo = new Mock<IEnvironmentRepository>();
            envRepo.Setup(r => r.All()).Returns(new List<IEnvironmentModel>(new[] { env.Object }));
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
            resourceRepo.Setup(r => r.FetchResourceDefinition(It.IsAny<IEnvironmentModel>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new ExecuteMessage());

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


        [TestMethod]
        [TestCategory("MainViewModel_Constructor")]
        [Description("MainViewModel constructor must show start page")]
        [Owner("Ashley Lewis")]
        public void MainViewModel_UnitTest_ConstructorWithNoNullParams_ShowStartPage()
        {
            //Isolate MainViewModel Constructor as a functional unit
            var mvm = Dev2MockFactory.MainViewModel;
            mvm.Setup(c => c.ShowStartPage()).Verifiable();

            //Run constructor
            var concreteMvm = mvm.Object;

            //assert ShowStartPage is called
            Assert.IsNotNull(concreteMvm);
            mvm.Verify(c => c.ShowStartPage(), Times.Once());
        }

        #endregion

        #region Show Dependencies

        [TestMethod]
        public void ShowDependenciesMessageExpectsDependencyVisualizerWithResource()
        {
            CreateFullExportsAndVm();
            var msg = new ShowDependenciesMessage(FirstResource.Object);
            MainViewModel.Handle(msg);
            var ctx = MainViewModel.ActiveItem;
            var vm = ctx.WorkSurfaceViewModel as DependencyVisualiserViewModel;
            Assert.IsNotNull(vm);
            // ReSharper disable PossibleNullReferenceException
            Assert.IsTrue(vm.ResourceModel.Equals(FirstResource.Object));
            // ReSharper restore PossibleNullReferenceException
        }

        [TestMethod]
        public void ShowDependenciesMessageExpectsNothingWithNullResource()
        {
            CreateFullExportsAndVm();
            var msg = new ShowDependenciesMessage(null);
            MainViewModel.Handle(msg);
            Assert.IsTrue(
                MainViewModel.Items.All(
                    i => i.WorkSurfaceKey.WorkSurfaceContext != WorkSurfaceContext.DependencyVisualiser));
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

        #region Deactivate

        [TestMethod]
        public void DeactivateWithCloseExpectBuildWithEmptyDebugWriterWriteMessage()
        {
            CreateFullExportsAndVm();
            EventAggregator.Setup(e => e.Publish(It.IsAny<UpdateDeployMessage>()))
                .Verifiable();

            MainViewModel.Dispose();

            EventAggregator.Verify(e => e.Publish(It.IsAny<UpdateDeployMessage>()), Times.Exactly(1));
        }

        [TestMethod]
        public void DeactivateWithCloseAndTwoTabsExpectBuildTwiceWithEmptyDebugWriterWriteMessage()
        {
            CreateFullExportsAndVm();
            EventAggregator.Setup(e => e.Publish(It.IsAny<UpdateDeployMessage>()))
                .Verifiable();
            AddAdditionalContext();

            MainViewModel.Dispose();

            EventAggregator.Verify(e => e.Publish(It.IsAny<UpdateDeployMessage>()), Times.Exactly(2));
        }

        #endregion

        #region Add Work Surface

        [TestMethod]
        public void AdditionalWorksurfaceAddedExpectsLAstAddedTOBeActive()
        {
            CreateFullExportsAndVm();
            AddAdditionalContext();
            Assert.AreEqual(3, MainViewModel.Items.Count);
            var activeItem = MainViewModel.ActiveItem;
            var secondKey = WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.Workflow, SecondResource.Object.ID,
                SecondResource.Object.ServerID);
            Assert.IsTrue(activeItem.WorkSurfaceKey.ResourceID.Equals(secondKey.ResourceID) && activeItem.WorkSurfaceKey.ServerID.Equals(secondKey.ServerID));
        }

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
                .Callback<object>((o =>
                {
                    var msg = (TabClosedMessage)o;
                    Assert.IsTrue(msg.Context.Equals(activetx));
                }));

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
            PopupController.Setup(s => s.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>(), It.IsAny<string>())).Returns(MessageBoxResult.Yes);

            var activetx =
                MainViewModel.Items.ToList()
                    .First(i => i.WorkSurfaceViewModel.WorkSurfaceContext == WorkSurfaceContext.Workflow);

            EventAggregator.Setup(e => e.Publish(It.IsAny<TabClosedMessage>()))
                .Callback<object>((o =>
                {
                    var msg = (TabClosedMessage)o;
                    Assert.IsTrue(msg.Context.Equals(activetx));
                }));

            EventAggregator.Setup(e => e.Publish(It.IsAny<SaveResourceMessage>()))
                .Callback<object>((o =>
                {
                    var msg = (SaveResourceMessage)o;
                    Assert.IsTrue(msg.Resource.Equals(FirstResource.Object));
                }));

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
            PopupController.Setup(s => s.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>(), It.IsAny<string>())).Returns(MessageBoxResult.Yes);
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
        [Owner("Jai Holloway")]
        [TestCategory("MainViewModel_ChangeActiveItem")]
        public void MainViewModel_CloseWorkSurfaceContext_PreviousItemActivatedAndCorrectlySet()
        {
            CreateFullExportsAndVm();
            AddAdditionalContext();
            AddAdditionalContext();
            Assert.AreEqual(3, MainViewModel.Items.Count);

            FirstResource.Setup(r => r.IsAuthorized(AuthorizationContext.Contribute)).Returns(true);
            SecondResource.Setup(r => r.IsAuthorized(AuthorizationContext.Contribute)).Returns(true);

            var firstCtx = MainViewModel.FindWorkSurfaceContextViewModel(FirstResource.Object);
            var secondCtx = MainViewModel.FindWorkSurfaceContextViewModel(SecondResource.Object);

            MainViewModel.ActivateItem(firstCtx);
            MainViewModel.ActivateItem(secondCtx);
            MainViewModel.ActivateItem(firstCtx);
            var msg = new ShowDependenciesMessage(FirstResource.Object);
            MainViewModel.Handle(msg);
            var dependencyCtx = MainViewModel.ActiveItem;
            var vm = dependencyCtx.WorkSurfaceViewModel as DependencyVisualiserViewModel;
            Assert.IsNotNull(vm);
            //Assert.IsTrue(vm.ResourceModel.Equals(_firstResource.Object));

            MainViewModel.DeactivateItem(dependencyCtx, false);

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

            PopupController.Setup(s => s.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>(), It.IsAny<string>())).Returns(MessageBoxResult.No);
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

            PopupController.Setup(s => s.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>(), It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            var activetx = MainViewModel.Items.ToList().First(i => i.WorkSurfaceViewModel.WorkSurfaceContext == WorkSurfaceContext.Workflow);
            MainViewModel.CloseWorkSurfaceContext(activetx, null);
            FirstResource.Verify(r => r.Commit(), Times.Once(), "ResourceModel was not committed when saved.");
            FirstResource.Verify(r => r.Rollback(), Times.Never(), "ResourceModel was rolled back when saved.");
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

            //------------Assert Results-------------------------
            EventAggregator.Verify(e => e.Publish(It.IsAny<SaveResourceMessage>()), Times.Never());
            FirstResource.Verify(r => r.Commit(), Times.Never(), "ResourceModel was committed when saved.");
            FirstResource.Verify(r => r.Rollback(), Times.Never(), "ResourceModel was rolled back when saved.");
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
            MainViewModel.DisplayAboutDialogueCommand.Execute(null);
            WindowManager.Verify(w => w.ShowDialog(It.IsAny<IDialogueViewModel>(), null, null), Times.Once());
        }

        [TestMethod]
        public void AddStudioShortcutsPageCommandExpectsShortKeysActive()
        {
            CreateFullExportsAndVm();
            MainViewModel.AddStudioShortcutsPageCommand.Execute(null);
            var shortkeyUri = FileHelper.GetFullPath(StringResources.Uri_Studio_Shortcut_Keys_Document);
            var helpctx = MainViewModel.ActiveItem.WorkSurfaceViewModel as HelpViewModel;
            Assert.IsNotNull(helpctx);
            // ReSharper disable PossibleNullReferenceException
            Assert.IsTrue(helpctx.Uri == shortkeyUri);
            // ReSharper restore PossibleNullReferenceException
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
        public void NewResourceCommandExpectsWebControllerDisplayDialogue()
        {
            CreateFullExportsAndVm();
            int HitCounter = 0;
            IContextualResourceModel payloadResourceModel = null;
            MockStudioResourceRepository.Setup(repository => repository.AddResouceItem(It.IsAny<IContextualResourceModel>()));
            WebController.Setup(w => w.DisplayDialogue(It.IsAny<IContextualResourceModel>(), false)).Callback((IContextualResourceModel c, bool b1) =>
                {
                    HitCounter++;
                    payloadResourceModel = c;
                });

            Mock<IAuthorizationService> mockAuthService = new Mock<IAuthorizationService>();
            mockAuthService.Setup(c => c.GetResourcePermissions(It.IsAny<Guid>())).Returns(Permissions.Administrator);
            EnvironmentModel.Setup(c => c.AuthorizationService).Returns(mockAuthService.Object);
            MainViewModel.Handle(new SetActiveEnvironmentMessage(EnvironmentModel.Object));
            MainViewModel.NewResourceCommand.Execute("Service");
            Assert.AreEqual(1, HitCounter);
            if(payloadResourceModel != null)
            {
                Assert.AreEqual(Guid.Empty, payloadResourceModel.ID);
            }
            else
            {
                Assert.Fail("The resource passed in was null");
            }
            MockStudioResourceRepository.Verify(repository => repository.AddResouceItem(It.IsAny<IContextualResourceModel>()), Times.Never());
        }

        [TestMethod]
        public void NewResourceMessageExpectsWebControllerDisplayDialogue()
        {
            CreateFullExportsAndVm();
            int HitCounter = 0;
            IContextualResourceModel payloadResourceModel = null;
            MockStudioResourceRepository.Setup(repository => repository.AddResouceItem(It.IsAny<IContextualResourceModel>()));
            WebController.Setup(w => w.DisplayDialogue(It.IsAny<IContextualResourceModel>(), false)).Callback((IContextualResourceModel c, bool b1) =>
                {
                    HitCounter++;
                    payloadResourceModel = c;
                });

            Mock<IAuthorizationService> mockAuthService = new Mock<IAuthorizationService>();
            mockAuthService.Setup(c => c.GetResourcePermissions(It.IsAny<Guid>())).Returns(Permissions.Administrator);
            EnvironmentModel.Setup(c => c.AuthorizationService).Returns(mockAuthService.Object);
            MainViewModel.Handle(new SetActiveEnvironmentMessage(EnvironmentModel.Object));
            MainViewModel.Handle(new ShowNewResourceWizard("Service"));
            Assert.AreEqual(1, HitCounter);
            if(payloadResourceModel != null)
            {
                Assert.AreEqual(Guid.Empty, payloadResourceModel.ID);
            }
            else
            {
                Assert.Fail("The resource passed in was null");
            }
            MockStudioResourceRepository.Verify(repository => repository.AddResouceItem(It.IsAny<IContextualResourceModel>()), Times.Never());
        }

        [TestMethod]
        public void ShowEditResourceWizardMessageExpectsWebControllerDisplayDialogue()
        {
            CreateFullExportsAndVm();
            int HitCounter = 0;
            IContextualResourceModel payloadResourceModel = null;
            MockStudioResourceRepository.Setup(repository => repository.AddResouceItem(It.IsAny<IContextualResourceModel>()));
            WebController.Setup(w => w.DisplayDialogue(It.IsAny<IContextualResourceModel>(), true)).Callback((IContextualResourceModel c, bool b1) =>
                {
                    HitCounter++;
                    payloadResourceModel = c;
                });

            Mock<IAuthorizationService> mockAuthService = new Mock<IAuthorizationService>();
            mockAuthService.Setup(c => c.GetResourcePermissions(It.IsAny<Guid>())).Returns(Permissions.Administrator);
            EnvironmentModel.Setup(c => c.AuthorizationService).Returns(mockAuthService.Object);
            MainViewModel.Handle(new SetActiveEnvironmentMessage(EnvironmentModel.Object));
            var mockResourceModel = new Mock<IContextualResourceModel>();
            mockResourceModel.Setup(model => model.WorkflowXaml).Returns(new StringBuilder());
            MainViewModel.Handle(new ShowEditResourceWizardMessage(mockResourceModel.Object));
            Assert.AreEqual(1, HitCounter);
            if(payloadResourceModel != null)
            {
                Assert.AreEqual(Guid.Empty, payloadResourceModel.ID);
            }
            else
            {
                Assert.Fail("The resource passed in was null");
            }
            MockStudioResourceRepository.Verify(repository => repository.AddResouceItem(It.IsAny<IContextualResourceModel>()), Times.Never());
        }

        [TestMethod]
        public void StartFeedbackCommandCommandExpectsFeedbackInvoked()
        {
            CreateFullExportsAndVm();
            FeedbackInvoker.Setup(
                i => i.InvokeFeedback(It.IsAny<EmailFeedbackAction>(), It.IsAny<RecorderFeedbackAction>()))
                .Verifiable();
            MainViewModel.StartFeedbackCommand.Execute(null);
            FeedbackInvoker.Verify(
                i => i.InvokeFeedback(It.IsAny<EmailFeedbackAction>(), It.IsAny<RecorderFeedbackAction>()),
                Times.Once());
        }

        [TestMethod]
        public void StartStopRecordedFeedbackCommandExpectsFeedbackStartedWhenNotInProgress()
        {
            CreateFullExportsAndVm();
            FeedbackInvoker.Setup(i => i.InvokeFeedback(It.IsAny<RecorderFeedbackAction>())).Verifiable();
            MainViewModel.StartStopRecordedFeedbackCommand.Execute(null);
            FeedbackInvoker.Verify(i => i.InvokeFeedback(It.IsAny<RecorderFeedbackAction>()), Times.Once());
        }

        [TestMethod]
        public void StartStopRecordedFeedbackCommandExpectsFeedbackStppedtInProgress()
        {
            CreateFullExportsAndVm();
            var mockAction = new Mock<IAsyncFeedbackAction>();
            mockAction.Setup(a => a.StartFeedback()).Verifiable();
            FeedbackInvoker.SetupGet(i => i.CurrentAction).Returns(mockAction.Object);
            MainViewModel.StartStopRecordedFeedbackCommand.Execute(null);
            FeedbackInvoker.Verify(i => i.InvokeFeedback(It.IsAny<RecorderFeedbackAction>()), Times.Never());

            // PBI 9598 - 2013.06.10 - TWR : added null parameter
            mockAction.Verify(a => a.FinishFeedBack(It.IsAny<IEnvironmentModel>()), Times.Once());
        }

        [TestMethod]
        public void DeployAllCommandWithoutCurrentResourceExpectsDeplouViewModelActive()
        {
            CreateFullExportsAndVmWithEmptyRepo();
            MainViewModel.Handle(new SetActiveEnvironmentMessage(EnvironmentModel.Object));
            MainViewModel.DeployAllCommand.Execute(null);
            var activectx = MainViewModel.ActiveItem;
            Assert.IsTrue(activectx.WorkSurfaceKey.Equals(
                WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.DeployResources)));
        }

        [TestMethod]
        [Description("Makes sure that new workflow only calls TempSave, not save on the resource repository")]
        [Owner("Jurie Smit")]
        public void MainViewModel_Regression_NewWorkFlowCommand_DoesNotSaveRepository()
        {
            //Setup
            CreateFullExportsAndVmWithEmptyRepo();
            MockStudioResourceRepository.Setup(repository => repository.AddResouceItem(It.IsAny<IContextualResourceModel>()));
            var environmentRepo = CreateMockEnvironment();
            Mock<IAuthorizationService> mockAuthService = new Mock<IAuthorizationService>();
            mockAuthService.Setup(c => c.GetResourcePermissions(It.IsAny<Guid>())).Returns(Permissions.Administrator);
            environmentRepo.Setup(c => c.AuthorizationService).Returns(mockAuthService.Object);
            var resourceRepo = new Mock<IResourceRepository>();
            resourceRepo.Setup(c => c.FetchResourceDefinition(It.IsAny<IEnvironmentModel>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new ExecuteMessage());
            resourceRepo.Setup(r => r.Save(It.IsAny<IResourceModel>())).Verifiable();
            environmentRepo.Setup(e => e.ResourceRepository).Returns(resourceRepo.Object);
            environmentRepo.Setup(e => e.IsConnected).Returns(true);
            MainViewModel.ActiveEnvironment = environmentRepo.Object;
            MainViewModel.NewResourceCommand.Execute("Workflow");
            //Assert
            resourceRepo.Verify(r => r.Save(It.IsAny<IResourceModel>()), Times.Never());
            MockStudioResourceRepository.Verify(repository => repository.AddResouceItem(It.IsAny<IContextualResourceModel>()), Times.Once());
        }

        #endregion

        #region Delete

        [TestMethod]
        public void DeleteServerResourceOnLocalHostAlsoDeletesFromEnvironmentRepoAndExplorerTree()
        {
            //---------Setup------
            var mock = SetupForDeleteServer();
            EnvironmentModel.Setup(s => s.IsLocalHost).Returns(true);
            //---------Execute------
            var msg = new DeleteResourcesMessage(new List<IContextualResourceModel> { FirstResource.Object }, "", false);
            MainViewModel.Handle(msg);

            //---------Verify------
            mock.Verify(s => s.Remove(It.IsAny<IEnvironmentModel>()), Times.Once());
            EventAggregator.Verify(e => e.Publish(It.IsAny<EnvironmentDeletedMessage>()), Times.Once());
        }

        [TestMethod]
        public void DeleteServerResourceOnOtherServerDoesntDeleteFromEnvironmentRepoAndExplorerTree()
        {
            //---------Setup------
            var mock = SetupForDeleteServer();
            EnvironmentConnection.Setup(c => c.DisplayName).Returns("NotLocalHost");
            EventAggregator = new Mock<IEventAggregator>();
            EventAggregator.Setup(e => e.Publish(It.IsAny<EnvironmentDeletedMessage>())).Verifiable();

            //---------Execute------
            var msg = new DeleteResourcesMessage(new List<IContextualResourceModel> { FirstResource.Object }, "", false);
            MainViewModel.Handle(msg);

            //---------Verify------
            mock.Verify(s => s.Remove(It.IsAny<IEnvironmentModel>()), Times.Never());
            EventAggregator.Verify(e => e.Publish(It.IsAny<EnvironmentDeletedMessage>()), Times.Never());
        }

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
        public void MainViewModel_HandleDeleteResourceMessage_WhenHasActionNotDeclined_PerformsAction()
        {
            //------------Setup for test--------------------------
            CreateFullExportsAndVm();
            SetupForDelete();
            var _actionInvoked = false;
            var msg = new DeleteResourcesMessage(new List<IContextualResourceModel> { FirstResource.Object }, "", true, () =>
            {
                _actionInvoked = true;
            });
            //------------Execute Test---------------------------
            MainViewModel.Handle(msg);
            //------------Assert Results-------------------------
            Assert.IsFalse(_actionInvoked);
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
            var msg = new DeleteResourcesMessage(new List<IContextualResourceModel> { unassignedResource.Object }, "", false);

            //Run delete command
            MainViewModel.Handle(msg);

            //Assert resource deleted from repository
            repo.Verify(repository => repository.DeleteResource(unassignedResource.Object), Times.Once(), "Deleting an unassigned resource does not delete from resource repository");
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("MainViewmodel_HandleDeleteResourceMessage")]
        public void MainViewmodel_HandleDeleteResourceMessage_NullModelInList_NoUnhandledExeptions()
        {
            //---------Setup------
            var mock = SetupForDeleteServer();
            EnvironmentModel.Setup(s => s.IsLocalHost).Returns(true);

            //---------Execute------
            var msg = new DeleteResourcesMessage(new List<IContextualResourceModel> { null }, "", false);
            MainViewModel.Handle(msg);

            //---------Verify------
            mock.Verify(s => s.Remove(It.IsAny<IEnvironmentModel>()), Times.Never());
            EventAggregator.Verify(e => e.Publish(It.IsAny<EnvironmentDeletedMessage>()), Times.Never());
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

        [TestMethod]
        public void IHandleShowDependenciesActivatesDependecies()
        {
            CreateFullExportsAndVm();
            var msg = new ShowDependenciesMessage(FirstResource.Object);
            MainViewModel.Handle(msg);

            Assert.AreEqual(MainViewModel.ActiveItem.WorkSurfaceKey.WorkSurfaceContext, WorkSurfaceContext.DependencyVisualiser);
        }

        [TestMethod]
        public void IHandleShowDependenciesActivatesReverseDependecies()
        {
            CreateFullExportsAndVm();
            var msg = new ShowDependenciesMessage(FirstResource.Object, true);
            MainViewModel.Handle(msg);
            Assert.AreEqual(MainViewModel.ActiveItem.WorkSurfaceKey.WorkSurfaceContext, WorkSurfaceContext.ReverseDependencyVisualiser);
        }

        #endregion

        #region IHandle

        [TestMethod]
        public void IHandleShowDependencies()
        {
            CreateFullExportsAndVm();
            Assert.IsInstanceOfType(MainViewModel, typeof(IHandle<ShowDependenciesMessage>));
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("MainViewModel_Handle")]
        public void MainViewModel_Handle_FileChooserMessage_True()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            CreateFullExportsAndVm();

            //------------Assert Results-------------------------
            Assert.IsInstanceOfType(MainViewModel, typeof(IHandle<FileChooserMessage>));
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("MainViewModel_Handle")]
        public void MainViewModel_Handle_DisplayMessageBoxMessage_True()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            CreateFullExportsAndVm();

            //------------Assert Results-------------------------
            Assert.IsInstanceOfType(MainViewModel, typeof(IHandle<DisplayMessageBoxMessage>));
        }

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
            envRepo.Setup(r => r.ReadSession()).Returns(new[] { env.Object.ID });
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
            var designerViewModel = new WorkflowDesignerViewModel(new Mock<IEventAggregator>().Object, resourceModel.Object, workflowHelper.Object, mockPopUp.Object, false);
            var contextViewModel1 = new WorkSurfaceContextViewModel(
                new WorkSurfaceKey { ResourceID = resourceID, ServerID = serverID, WorkSurfaceContext = designerViewModel.WorkSurfaceContext },
                designerViewModel);

            #endregion

            mockMainViewModel.Items.Add(contextViewModel1);

            mockMainViewModel.PopupProvider = Dev2MockFactory.CreateIPopup(MessageBoxResult.No).Object;

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
            envRepo.Setup(r => r.ReadSession()).Returns(new[] { env.Object.ID });
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
            mockPopUp.Setup(m => m.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>(), It.IsAny<string>())).Verifiable();
            var workflowHelper = new Mock<IWorkflowHelper>();
            var designerViewModel = new WorkflowDesignerViewModel(new Mock<IEventAggregator>().Object, resourceModel.Object, workflowHelper.Object, mockPopUp.Object, false);
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
            mockPopUp.Verify(m => m.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>(), It.IsAny<string>()), Times.Once());
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

            Mock<IPopupController> mockPopUp = Dev2MockFactory.CreateIPopup(MessageBoxResult.No);
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
            envRepo.Setup(mock => mock.ReadSession()).Returns(new[] { env.Object.ID });

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
            resourceRepo.Setup(r => r.FetchResourceDefinition(It.IsAny<IEnvironmentModel>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new ExecuteMessage());

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

            Mock<IPopupController> mockPopUp = Dev2MockFactory.CreateIPopup(MessageBoxResult.No);
            var workflowHelper = new Mock<IWorkflowHelper>();
            var designerViewModel = new WorkflowDesignerViewModel(new Mock<IEventAggregator>().Object, resourceModel.Object, workflowHelper.Object, mockPopUp.Object, false);
            var contextViewModel = new WorkSurfaceContextViewModel(
                new WorkSurfaceKey { ResourceID = resourceID, ServerID = serverID, WorkSurfaceContext = designerViewModel.WorkSurfaceContext },
                designerViewModel);

            var envRepo = new Mock<IEnvironmentRepository>();
            envRepo.Setup(r => r.All()).Returns(new[] { env.Object });
            envRepo.Setup(e => e.Source).Returns(env.Object);
            envRepo.Setup(r => r.ReadSession()).Returns(new[] { env.Object.ID });
            Mock<IAsyncWorker> asyncWorker = AsyncWorkerTests.CreateSynchronousAsyncWorker();
            var viewModel = new MainViewModelPersistenceMock(envRepo.Object, asyncWorker.Object, false);
            viewModel.Items.Add(contextViewModel);

            viewModel.TestClose();

            wsiRepo.Verify(r => r.UpdateWorkspaceItem(It.IsAny<IContextualResourceModel>(), It.Is<bool>(b => b)));
            resourceRepo.Verify(r => r.Save(It.IsAny<IResourceModel>()));
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
            var designerViewModel = new WorkflowDesignerViewModel(new Mock<IEventAggregator>().Object, resourceModel.Object, workflowHelper.Object, mockPopUp.Object, false);
            var contextViewModel = new WorkSurfaceContextViewModel(
                new WorkSurfaceKey { ResourceID = resourceID, ServerID = serverID, WorkSurfaceContext = designerViewModel.WorkSurfaceContext },
                designerViewModel);

            var envRepo = new Mock<IEnvironmentRepository>();
            envRepo.Setup(r => r.All()).Returns(new[] { env.Object });
            envRepo.Setup(e => e.Source).Returns(env.Object);
            envRepo.Setup(r => r.ReadSession()).Returns(new[] { env.Object.ID });
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
            envRepo.Setup(e => e.ReadSession()).Returns(new[] { Guid.NewGuid() });
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
            var vm = new MainViewModel(mockEventAggregator.Object, new Mock<IAsyncWorker>().Object, envRepo.Object, new Mock<IVersionChecker>().Object, false);
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
            SetupDefaultMef();
            var eventPublisher = new Mock<IEventAggregator>();
            var environmentRepository = new Mock<IEnvironmentRepository>();
            environmentRepository.Setup(repo => repo.Source).Returns(new Mock<IEnvironmentModel>().Object);
            var versionChecker = new Mock<IVersionChecker>();
            var asyncWorker = new Mock<IAsyncWorker>();
            asyncWorker.Setup(w => w.Start(It.IsAny<Action>(), It.IsAny<Action>())).Verifiable();
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
            var scheduler = new SchedulerViewModel(EventPublishers.Aggregator, new DirectoryObjectPickerDialog(), popup.Object, new AsyncWorker(), new Mock<IConnectControlViewModel>().Object) { WorkSurfaceContext = WorkSurfaceContext.Scheduler };
            var task = new Mock<IScheduledResource>();
            task.Setup(a => a.IsDirty).Returns(true);
            scheduler.SelectedTask = task.Object;
            var vm = new WorkSurfaceContextViewModel(new EventAggregator(), new WorkSurfaceKey(), scheduler, new Mock<IPopupController>().Object, (a, b) => { });

            mvm.Items.Add(vm);
            Assert.IsFalse(mvm.OnStudioClosing());

        }

        [TestMethod]
        [TestCategory("MainViewModel_OnStudioClosing")]
        [Owner("Leon Rajindrapersadh")]
        public void MainViewModel_OnStudioClosing_ClosesRemoteEnvironmants()
        {
            SetupDefaultMef();
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
            var versionChecker = new Mock<IVersionChecker>();
            var asyncWorker = new Mock<IAsyncWorker>();
            asyncWorker.Setup(w => w.Start(It.IsAny<Action>(), It.IsAny<Action>())).Verifiable();
            var mvm = new MainViewModel(eventPublisher.Object, asyncWorker.Object, environmentRepository.Object, versionChecker.Object, false);
            var popup = new Mock<IPopupController>();
            popup.Setup(a => a.ShowSchedulerCloseConfirmation()).Returns(MessageBoxResult.Cancel).Verifiable();
            var scheduler = new SchedulerViewModel(EventPublishers.Aggregator, new DirectoryObjectPickerDialog(), popup.Object, new AsyncWorker(), new Mock<IConnectControlViewModel>().Object) { WorkSurfaceContext = WorkSurfaceContext.Scheduler };
            var task = new Mock<IScheduledResource>();
            task.Setup(a => a.IsDirty).Returns(false);
            scheduler.SelectedTask = task.Object;
            var vm = new WorkSurfaceContextViewModel(new EventAggregator(), new WorkSurfaceKey(), scheduler, new Mock<IPopupController>().Object, (a, b) => { });

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
            SetupDefaultMef();
            var eventPublisher = new Mock<IEventAggregator>();
            var environmentRepository = new Mock<IEnvironmentRepository>();
            environmentRepository.Setup(repo => repo.Source).Returns(new Mock<IEnvironmentModel>().Object);
            var versionChecker = new Mock<IVersionChecker>();
            var asyncWorker = new Mock<IAsyncWorker>();
            asyncWorker.Setup(w => w.Start(It.IsAny<Action>(), It.IsAny<Action>())).Verifiable();
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
            var settings = new SettingsViewModelForTest(EventPublishers.Aggregator, popup.Object, new AsyncWorker(), new NativeWindow()) { RetValue = false, WorkSurfaceContext = WorkSurfaceContext.Settings };
            var task = new Mock<IScheduledResource>();
            task.Setup(a => a.IsDirty).Returns(true);
            settings.IsDirty = true;
            var vm = new WorkSurfaceContextViewModel(new EventAggregator(), new WorkSurfaceKey(), settings, new Mock<IPopupController>().Object, (a, b) => { });

            mvm.Items.Add(vm);
            Assert.IsFalse(mvm.OnStudioClosing());

        }


        [TestMethod]
        [TestCategory("MainViewModel_OnStudioClosing")]
        [Owner("Leon Rajindrapersadh")]
        public void MainViewModel_OnStudioClosing_CallsSettingsOnClosingDirty()
        {
            SetupDefaultMef();
            var eventPublisher = new Mock<IEventAggregator>();
            var environmentRepository = new Mock<IEnvironmentRepository>();
            environmentRepository.Setup(repo => repo.Source).Returns(new Mock<IEnvironmentModel>().Object);
            var versionChecker = new Mock<IVersionChecker>();
            var asyncWorker = new Mock<IAsyncWorker>();
            asyncWorker.Setup(w => w.Start(It.IsAny<Action>(), It.IsAny<Action>())).Verifiable();
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

            var settings = new SettingsViewModelForTest(EventPublishers.Aggregator, popup.Object, new AsyncWorker(), new NativeWindow()) { RetValue = true, WorkSurfaceContext = WorkSurfaceContext.Settings };
            var task = new Mock<IScheduledResource>();
            task.Setup(a => a.IsDirty).Returns(true);
            settings.IsDirty = true;
            var vm = new WorkSurfaceContextViewModel(new EventAggregator(), new WorkSurfaceKey(), settings, new Mock<IPopupController>().Object, (a, b) => { });
            environmentRepository.Setup(repo => repo.All()).Returns(new List<IEnvironmentModel>());
            mvm.Items.Add(vm);
            Assert.IsTrue(mvm.OnStudioClosing());

        }

        [TestMethod]
        [TestCategory("MainViewModel_OnStudioClosing")]
        [Owner("Leon Rajindrapersadh")]
        public void MainViewModel_OnStudioClosing_CallsSchedulerOnClosingClosesSuccessfully()
        {
            SetupDefaultMef();
            var eventPublisher = new Mock<IEventAggregator>();
            var environmentRepository = new Mock<IEnvironmentRepository>();
            environmentRepository.Setup(repo => repo.Source).Returns(new Mock<IEnvironmentModel>().Object);
            var versionChecker = new Mock<IVersionChecker>();
            var asyncWorker = new Mock<IAsyncWorker>();
            asyncWorker.Setup(w => w.Start(It.IsAny<Action>(), It.IsAny<Action>())).Verifiable();
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
            var scheduler = new SchedulerViewModelForTesting(EventPublishers.Aggregator, new DirectoryObjectPickerDialog(), popup.Object, new AsyncWorker()) { RetValue = true, WorkSurfaceContext = WorkSurfaceContext.Scheduler };
            var task = new Mock<IScheduledResource>();
            task.Setup(a => a.IsDirty).Returns(true);
            scheduler.SelectedTask = task.Object;
            var vm = new WorkSurfaceContextViewModel(new EventAggregator(), new WorkSurfaceKey(), scheduler, new Mock<IPopupController>().Object, (a, b) => { });
            environmentRepository.Setup(repo => repo.All()).Returns(new List<IEnvironmentModel>());
            mvm.Items.Add(vm);
            Assert.IsTrue(mvm.OnStudioClosing());

        }

        #endregion

        [TestMethod]
        [TestCategory("MainViewModel_HandleDeployResourcesMessage")]
        [Description("Handle DeployResourcesMessage must open the deploy tab and select the resource in the view.")]
        [Owner("Trevor Williams-Ros")]
        public void MainViewModel_HandleDeployResourcesMessage_PublishesSelectItemInDeployMessage()
        {
            SelectItemInDeployMessage actual = null;
            var eventAggregator = new Mock<IEventAggregator>();
            eventAggregator.Setup(e => e.Publish(It.IsAny<object>())).Callback((object msg) => actual = msg as SelectItemInDeployMessage).Verifiable();
            #region Setup ImportService - GRRR!
            SetupDefaultMef();
            #endregion

            var resourceID = Guid.NewGuid();
            var environmentID = Guid.NewGuid();
            var envRepo = new Mock<IEnvironmentRepository>();
            envRepo.Setup(e => e.All()).Returns(new List<IEnvironmentModel>());
            Mock<IResourceRepository> mockResourceRepository = new Mock<IResourceRepository>();
            Mock<IContextualResourceModel> mockResourceModel = new Mock<IContextualResourceModel>();
            mockResourceModel.Setup(model => model.ID).Returns(resourceID);
            mockResourceRepository.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false)).Returns(mockResourceModel.Object);
            Mock<IEnvironmentModel> mockEnvironmentModel = new Mock<IEnvironmentModel>();
            mockEnvironmentModel.Setup(model => model.ID).Returns(environmentID);
            mockEnvironmentModel.Setup(model => model.ResourceRepository).Returns(mockResourceRepository.Object);
            mockEnvironmentModel.Setup(model => model.AuthorizationService).Returns(new Mock<IAuthorizationService>().Object);
            var environmentModel = mockEnvironmentModel.Object;
            mockResourceModel.Setup(model => model.Environment).Returns(environmentModel);
            envRepo.Setup(e => e.Source).Returns(environmentModel);
            envRepo.Setup(e => e.FindSingle(It.IsAny<Expression<Func<IEnvironmentModel, bool>>>())).Returns(environmentModel);
            envRepo.Setup(e => e.Source.IsConnected).Returns(false);
            envRepo.Setup(e => e.Source.Connection.IsConnected).Returns(false);
            var vm = new MainViewModel(eventAggregator.Object, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object, envRepo.Object, new Mock<IVersionChecker>().Object, false, new Mock<IBrowserPopupController>().Object);
            vm.AddDeployResourcesWorkSurface(mockResourceModel.Object);
            var expected = new Mock<IExplorerItemModel>();
            expected.Setup(model => model.ResourceId).Returns(resourceID);
            expected.Setup(model => model.EnvironmentId).Returns(environmentModel.ID);
            var deployMessage = new DeployResourcesMessage(expected.Object);
            vm.Handle(deployMessage);
            eventAggregator.Verify(e => e.Publish(It.IsAny<object>()), "MainViewModel Handle DeployResourcesMessage did not publish message with the selected view model.");
            Assert.IsNotNull(actual, "MainViewModel Handle DeployResourcesMessage did not publish message with the selected view model.");
            Assert.AreEqual(expected.Object.ResourceId, actual.ResourceID, "MainViewModel Handle DeployResourcesMessage did not publish message with the selected display name.");
            Assert.AreEqual(expected.Object.EnvironmentId, actual.EnvironmentID, "MainViewModel Handle DeployResourcesMessage did not publish message with the selected environment.");
        }

        [TestMethod]
        [TestCategory("MainViewModel_AddDeployResourceWorksurface")]
        [Owner("Trevor Williams-Ros")]
        public void MainViewModel_AddDeployResourceWorksurface_PublishesSelectItemInDeployMessage()
        {
            SelectItemInDeployMessage actual = null;
            var eventAggregator = new Mock<IEventAggregator>();
            eventAggregator.Setup(e => e.Publish(It.IsAny<object>())).Callback((object msg) => actual = msg as SelectItemInDeployMessage).Verifiable();
            #region Setup ImportService - GRRR!
            SetupDefaultMef();
            #endregion

            var resourceID = Guid.NewGuid();
            var environmentID = Guid.NewGuid();
            var envRepo = new Mock<IEnvironmentRepository>();
            envRepo.Setup(e => e.All()).Returns(new List<IEnvironmentModel>());
            Mock<IResourceRepository> mockResourceRepository = new Mock<IResourceRepository>();
            Mock<IContextualResourceModel> mockResourceModel = new Mock<IContextualResourceModel>();
            mockResourceModel.Setup(model => model.ID).Returns(resourceID);
            mockResourceRepository.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false)).Returns(mockResourceModel.Object);
            Mock<IEnvironmentModel> mockEnvironmentModel = new Mock<IEnvironmentModel>();
            mockEnvironmentModel.Setup(model => model.ID).Returns(environmentID);
            mockEnvironmentModel.Setup(model => model.ResourceRepository).Returns(mockResourceRepository.Object);
            var environmentModel = mockEnvironmentModel.Object;
            mockResourceModel.Setup(model => model.Environment).Returns(environmentModel);
            envRepo.Setup(e => e.Source).Returns(environmentModel);
            envRepo.Setup(e => e.FindSingle(It.IsAny<Expression<Func<IEnvironmentModel, bool>>>())).Returns(environmentModel);
            envRepo.Setup(e => e.Source.IsConnected).Returns(false);
            envRepo.Setup(e => e.Source.Connection.IsConnected).Returns(false);
            var vm = new MainViewModel(eventAggregator.Object, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object, envRepo.Object, new Mock<IVersionChecker>().Object, false, new Mock<IBrowserPopupController>().Object);
            vm.AddDeployResourcesWorkSurface(mockResourceModel.Object);
            var expected = new Mock<IExplorerItemModel>();
            expected.Setup(model => model.ResourceId).Returns(resourceID);
            expected.Setup(model => model.EnvironmentId).Returns(environmentModel.ID);
            vm.AddDeployResourcesWorkSurface(mockResourceModel.Object);
            eventAggregator.Verify(e => e.Publish(It.IsAny<object>()), "MainViewModel Handle DeployResourcesMessage did not publish message with the selected view model.");
            Assert.IsNotNull(actual, "MainViewModel Handle DeployResourcesMessage did not publish message with the selected view model.");
            Assert.AreEqual(expected.Object.ResourceId, actual.ResourceID, "MainViewModel Handle DeployResourcesMessage did not publish message with the selected display name.");
            Assert.AreEqual(expected.Object.EnvironmentId, actual.EnvironmentID, "MainViewModel Handle DeployResourcesMessage did not publish message with the selected environment.");
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("MainViewModel_Handle_DeleteFolderMessage")]
        public void MainViewModel_Handle_DeleteFolderMessage_ShouldShowMessage()
        {
            //------------Setup for test--------------------------
            CreateFullExportsAndVm();
            bool _actionCalled = false;
            PopupController.Setup(controller => controller.Show(It.IsAny<string>(), It.IsAny<string>(), MessageBoxButton.YesNo, MessageBoxImage.Warning, null)).Returns(MessageBoxResult.Yes);
            //------------Execute Test---------------------------
            MainViewModel.Handle(new DeleteFolderMessage("MyFolder", () =>
            {
                _actionCalled = true;
            }));
            //------------Assert Results-------------------------
            Assert.IsTrue(_actionCalled);
            PopupController.Verify(controller => controller.Show(It.IsAny<string>(), It.IsAny<string>(), MessageBoxButton.YesNo, MessageBoxImage.Warning, null), Times.Once());
        }

        public static ExecuteMessage MakeMsg(string msg)
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
            Verify_HandleAddWorkSurfaceMessage_ShowDebugWindowOnLoad(true);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("MainViewModel_HandleAddWorkSurfaceMessage")]
        public void MainViewModel_HandleAddWorkSurfaceMessage_ShowDebugWindowOnLoadIsFalse_DoesNotExecuteDebugCommand()
        {
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

            var vm = new MainViewModel(eventAggregator.Object, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object, envRepo.Object, new Mock<IVersionChecker>().Object, false, new Mock<IBrowserPopupController>().Object);

            var workSurfaceContextViewModel = new Mock<WorkSurfaceContextViewModel>(eventAggregator.Object, new WorkSurfaceKey(), new Mock<IWorkSurfaceViewModel>().Object, new Mock<IPopupController>().Object, new Action<IContextualResourceModel, bool>(
                (a, b) => { }));
            workSurfaceContextViewModel.Setup(v => v.Debug()).Verifiable();

            vm.ActiveItem = workSurfaceContextViewModel.Object;

            //------------Execute Test---------------------------
            vm.Handle(new AddWorkSurfaceMessage(new Mock<IWorkSurfaceObject>().Object) { ShowDebugWindowOnLoad = showDebugWindowOnLoad });

            //------------Assert Results-------------------------
            workSurfaceContextViewModel.Verify(v => v.Debug(), showDebugWindowOnLoad ? Times.Once() : Times.Never());

        }


        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("MainViewModel_AddWorkSurfaceContext")]
        public void MainViewModel_AddWorkSurfaceContext_AddTheSameResourceACoupleOfTimes_ReloadResourceIsWillBeCalledOnce()
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
            environmentRepository.Setup(c => c.ReadSession()).Returns(new[] { Guid.NewGuid() });
            environmentRepository.Setup(c => c.All()).Returns(new[] { environmentModel.Object });
            new Thread(() =>
            {
                var vm = new MainViewModel(new Mock<IEventAggregator>().Object,
                                     AsyncWorkerTests.CreateSynchronousAsyncWorker().Object,
                                     environmentRepository.Object,
                                     new Mock<IVersionChecker>().Object,
                                     false,
                                     new Mock<IBrowserPopupController>().Object,
                                     new Mock<IPopupController>().Object,
                                     new Mock<IWindowManager>().Object,
                                     new Mock<IWebController>().Object,
                                     new Mock<IFeedbackInvoker>().Object);
                int callCount = 0;
                vm.GetWorkSurfaceContextViewModel = (r, c) =>
                {
                    callCount++;
                    return new Mock<IWorkSurfaceContextViewModel>().Object;
                };

                var workspaceItemRepository = new Mock<IWorkspaceItemRepository>();
                workspaceItemRepository.SetupGet(p => p.WorkspaceItems).Returns(new List<IWorkspaceItem>());

                vm.GetWorkspaceItemRepository = () => workspaceItemRepository.Object;

                var tasks = new List<Task>
                {
                    Task.Factory.StartNew(() => vm.AddWorkSurfaceContext(resourceModel.Object)),
                    Task.Factory.StartNew(() => vm.AddWorkSurfaceContext(resourceModel.Object)),
                    Task.Factory.StartNew(() => vm.AddWorkSurfaceContext(resourceModel.Object)),
                    Task.Factory.StartNew(() => vm.AddWorkSurfaceContext(resourceModel.Object)), 
                    Task.Factory.StartNew(() => vm.AddWorkSurfaceContext(resourceModel.Object)),
                    Task.Factory.StartNew(() => vm.AddWorkSurfaceContext(resourceModel.Object)),
                    Task.Factory.StartNew(() => vm.AddWorkSurfaceContext(resourceModel.Object))
                };

                Task.WaitAll(tasks.ToArray());


                Assert.AreEqual(1, callCount, callCount.ToString(CultureInfo.InvariantCulture));
            });

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
            environmentRepository.Setup(c => c.ReadSession()).Returns(new[] { Guid.NewGuid() });
            environmentRepository.Setup(c => c.All()).Returns(new[] { environmentModel.Object });
            new Thread(() =>
            {
                var vm = new MainViewModel(new Mock<IEventAggregator>().Object,
                                     AsyncWorkerTests.CreateSynchronousAsyncWorker().Object,
                                     environmentRepository.Object,
                                     new Mock<IVersionChecker>().Object,
                                     false,
                                     new Mock<IBrowserPopupController>().Object,
                                     new Mock<IPopupController>().Object,
                                     new Mock<IWindowManager>().Object,
                                     new Mock<IWebController>().Object,
                                     new Mock<IFeedbackInvoker>().Object);

                var workspaceItemRepository = new Mock<IWorkspaceItemRepository>();
                workspaceItemRepository.SetupGet(p => p.WorkspaceItems).Returns(new List<IWorkspaceItem>());

                vm.GetWorkspaceItemRepository = () => workspaceItemRepository.Object;

                // will force a flip ;)
                resourceModel.Object.IsWorkflowSaved = false;

                var tasks = new List<Task>
                {
                    Task.Factory.StartNew(() => vm.AddWorkSurfaceContext(resourceModel.Object))
                };

                Task.WaitAll(tasks.ToArray());

                Assert.IsTrue(resourceModel.Object.IsWorkflowSaved);
            });

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
            environmentRepository.Setup(c => c.ReadSession()).Returns(new[] { Guid.NewGuid() });
            environmentRepository.Setup(c => c.All()).Returns(new[] { environmentModel.Object });
            new Thread(() =>
            {
                var vm = new MainViewModel(new Mock<IEventAggregator>().Object,
                                     AsyncWorkerTests.CreateSynchronousAsyncWorker().Object,
                                     environmentRepository.Object,
                                     new Mock<IVersionChecker>().Object,
                                     false,
                                     new Mock<IBrowserPopupController>().Object,
                                     new Mock<IPopupController>().Object,
                                     new Mock<IWindowManager>().Object,
                                     new Mock<IWebController>().Object,
                                     new Mock<IFeedbackInvoker>().Object);

                var workspaceItemRepository = new Mock<IWorkspaceItemRepository>();
                workspaceItemRepository.SetupGet(p => p.WorkspaceItems).Returns(new List<IWorkspaceItem>());

                vm.GetWorkspaceItemRepository = () => workspaceItemRepository.Object;

                var tasks = new List<Task>
                {
                    Task.Factory.StartNew(() => vm.AddWorkSurfaceContextFromWorkspace(resourceModel.Object))
                };

                Task.WaitAll(tasks.ToArray());

                Assert.IsFalse(resourceModel.Object.IsWorkflowSaved);
            });

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

            var viewModel = new MainViewModelMock(eventPublisher.Object, asyncWorker.Object, environmentRepository.Object, versionChecker.Object, new Mock<IStudioResourceRepository>().Object, new Mock<IConnectControlSingleton>().Object, new Mock<IConnectControlViewModel>().Object, false, browserPopupController.Object) { IsBusyDownloadingInstaller = null };

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
            var viewModel = new MainViewModelMock(eventPublisher.Object, asyncWorker.Object, environmentRepository.Object, versionChecker.Object, new Mock<IStudioResourceRepository>().Object, new Mock<IConnectControlSingleton>().Object, new Mock<IConnectControlViewModel>().Object, false, browserPopupController.Object) { IsBusyDownloadingInstaller = null };
            Mock<IPopupController> mockPopupController = new Mock<IPopupController>();
            mockPopupController.Setup(controller => controller.Show("Some message", "Some heading", MessageBoxButton.OK, MessageBoxImage.Warning, "")).Verifiable();
            viewModel.PopupProvider = mockPopupController.Object;
            //------------Execute Test---------------------------
            viewModel.Handle(new DisplayMessageBoxMessage("Some heading", "Some message", MessageBoxImage.Warning));
            //------------Assert Results-------------------------
            mockPopupController.Verify(controller => controller.Show("Some message", "Some heading", MessageBoxButton.OK, MessageBoxImage.Warning, ""));
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
            //environmentRepository.Setup(c => c.ReadSession()).Returns(new[] { Guid.NewGuid() });
            environmentRepository.Setup(c => c.All()).Returns(new[] { localhost.Object });
            environmentRepository.Setup(c => c.Source).Returns(localhost.Object);
            var eventPublisher = new Mock<IEventAggregator>();
            var asyncWorker = AsyncWorkerTests.CreateSynchronousAsyncWorker();
            var versionChecker = new Mock<IVersionChecker>();
            var browserPopupController = new Mock<IBrowserPopupController>();
            var viewModel = new MainViewModelMock(eventPublisher.Object, asyncWorker.Object, environmentRepository.Object, versionChecker.Object, new Mock<IStudioResourceRepository>().Object, new Mock<IConnectControlSingleton>().Object, new Mock<IConnectControlViewModel>().Object, false, browserPopupController.Object) { IsBusyDownloadingInstaller = () => false };
            //------------Execute Test---------------------------
            var isDownloading = viewModel.IsDownloading();
            //------------Assert Results-------------------------
            Assert.IsFalse(isDownloading);
        }

        [TestCategory("MainViewModel_IsDownloading")]
        [Owner("Tshepo Ntlhokoa")]
        public void MainViewModel_IsDownloading_IsBusyDownloadingInstallerReturnsTrue_True()
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
            var viewModel = new MainViewModelMock(eventPublisher.Object, asyncWorker.Object, environmentRepository.Object, versionChecker.Object, new Mock<IStudioResourceRepository>().Object, new Mock<IConnectControlSingleton>().Object, new Mock<IConnectControlViewModel>().Object, false, browserPopupController.Object) { IsBusyDownloadingInstaller = () => false };
            //------------Execute Test---------------------------
            var isDownloading = viewModel.IsDownloading();
            //------------Assert Results-------------------------
            Assert.IsTrue(isDownloading);
        }

        [TestCategory("MainViewModel_SetActiveEnvironment")]
        [Owner("Tshepo Ntlhokoa")]
        public void MainViewModel_SetActiveEnvironment_ActiveEnvironmentIsUpdated()
        {
            //------------Setup for test--------------------------
            var localhost = new Mock<IEnvironmentModel>();
            localhost.Setup(e => e.ID).Returns(Guid.Empty);
            localhost.Setup(e => e.IsConnected).Returns(true);
            var environmentRepository = new Mock<IEnvironmentRepository>();
            environmentRepository.Setup(c => c.All()).Returns(new[] { localhost.Object });
            environmentRepository.Setup(c => c.Source).Returns(localhost.Object);
            var eventPublisher = new Mock<IEventAggregator>();
            var asyncWorker = AsyncWorkerTests.CreateSynchronousAsyncWorker();
            var versionChecker = new Mock<IVersionChecker>();
            var browserPopupController = new Mock<IBrowserPopupController>();
            var viewModel = new MainViewModelMock(eventPublisher.Object, asyncWorker.Object, environmentRepository.Object, versionChecker.Object, new Mock<IStudioResourceRepository>().Object, new Mock<IConnectControlSingleton>().Object, new Mock<IConnectControlViewModel>().Object, false, browserPopupController.Object) { IsBusyDownloadingInstaller = () => false };
            //------------Execute Test---------------------------
            var newEnvironment = new Mock<IEnvironmentModel>();
            viewModel.SetActiveEnvironment(newEnvironment.Object);
            //------------Assert Results-------------------------
            Assert.AreEqual(newEnvironment, viewModel.ActiveEnvironment);
        }
    }

    public class SchedulerViewModelForTesting : SchedulerViewModel
    {
        public SchedulerViewModelForTesting()
        {

        }
        public SchedulerViewModelForTesting(IEventAggregator eventPublisher, DirectoryObjectPickerDialog directoryObjectPicker, IPopupController popupController, IAsyncWorker asyncWorker)
            : base(eventPublisher, directoryObjectPicker, popupController, asyncWorker, new Mock<IConnectControlViewModel>().Object)
        {

        }

        public override bool DoDeactivate()
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

        public SettingsViewModelForTest(IEventAggregator eventPublisher, IPopupController popupController,
                                       IAsyncWorker asyncWorker, IWin32Window parentWindow)
            : base(eventPublisher, popupController, asyncWorker, parentWindow, new Mock<IConnectControlViewModel>().Object)
        {
        }


        public override bool DoDeactivate()
        {
            return RetValue;
        }

        public bool RetValue { get; set; }
    }
}
