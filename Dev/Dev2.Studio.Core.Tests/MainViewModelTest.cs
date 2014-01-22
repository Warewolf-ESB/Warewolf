using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Windows;
using Caliburn.Micro;
using Dev2.Communication;
using Dev2.Composition;
using Dev2.Core.Tests.Utils;
using Dev2.Providers.Events;
using Dev2.Services.Events;
using Dev2.Services.Security;
using Dev2.Studio.AppResources.Comparers;
using Dev2.Studio.Core.AppResources.Browsers;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Controller;
using Dev2.Studio.Core.Helpers;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Interfaces.DataList;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Core.ViewModels.Navigation;
using Dev2.Studio.Core.Workspaces;
using Dev2.Studio.Factory;
using Dev2.Studio.Feedback;
using Dev2.Studio.Feedback.Actions;
using Dev2.Studio.ViewModels;
using Dev2.Studio.ViewModels.DependencyVisualization;
using Dev2.Studio.ViewModels.Help;
using Dev2.Studio.ViewModels.Navigation;
using Dev2.Studio.ViewModels.Workflow;
using Dev2.Studio.ViewModels.WorkSurface;
using Dev2.Studio.Webs;
using Dev2.Threading;
using Dev2.Util;
using Dev2.Utilities;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Action = System.Action;

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
    [ExcludeFromCodeCoverage]
    public class MainViewModelTest
    {
        #region Variables

        readonly Guid _firstResourceID = Guid.NewGuid();
        readonly Guid _secondResourceID = Guid.NewGuid();
        readonly Guid _serverID = Guid.NewGuid();
        readonly Guid _workspaceID = Guid.NewGuid();
        const string DisplayName = "test2";
        Mock<IEnvironmentConnection> _environmentConnection;
        Mock<IEnvironmentModel> _environmentModel;
        IEnvironmentRepository _environmentRepo;
        Mock<IEventAggregator> _eventAggregator;
        Mock<IFeedbackInvoker> _feedbackInvoker;
        Mock<IContextualResourceModel> _firstResource;
        MainViewModel _mainViewModel;
        Mock<IWorkspaceItemRepository> _mockWorkspaceRepo;
        public Mock<IPopupController> PopupController;
        const string ResourceName = "TestResource";
        Mock<IResourceRepository> _resourceRepo;
        Mock<IContextualResourceModel> _secondResource;
        const string ServiceDefinition = "<x/>";
        Mock<IWebController> _webController;
        Mock<IWindowManager> _windowManager;
        Mock<IAuthorizationService> _authorizationService;
        Mock<IEnvironmentModel> _activeEnvironment;

        #endregion Variables

        [TestInitialize]
        public void Initialize()
        {
            AppSettings.LocalHost = "http://localhost:3142";
        }

        [TestMethod]
        public void DeployCommandCanExecuteIrrespectiveOfEnvironments()
        {
            CreateFullExportsAndVm();
            Assert.IsTrue(_mainViewModel.DeployCommand.CanExecute(null));
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

            var actual = _mainViewModel.SettingsCommand.CanExecute(null);
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
            CompositionInitializer.InitializeForMeflessBaseViewModel();
            var eventPublisher = new Mock<IEventAggregator>();
            var environmentRepository = new Mock<IEnvironmentRepository>();
            var environmentModel = new Mock<IEnvironmentModel>();
            environmentModel.Setup(c => c.CanStudioExecute).Returns(false);
            environmentRepository.Setup(c => c.Source).Returns(environmentModel.Object);
            environmentRepository.Setup(c => c.ReadSession()).Returns(new[] { Guid.NewGuid() });
            environmentRepository.Setup(c => c.All()).Returns(new[] { environmentModel.Object });
            var versionChecker = new Mock<IVersionChecker>();
            var asyncWorker = new Mock<IAsyncWorker>();
            var mvm = new Mock<MainViewModel>(eventPublisher.Object, asyncWorker.Object, environmentRepository.Object, versionChecker.Object, false, null, null, null, null, null);
            mvm.Setup(c => c.ShowStartPage()).Verifiable();

            //construct
            var concreteMvm = mvm.Object;

            //test result
            Assert.IsNotNull(concreteMvm);
            mvm.Verify(c => c.ShowStartPage(), Times.Once());
        }

        [TestMethod]
        [TestCategory("MainViewModel_Constructor")]
        [Description("MainViewModel Constructor must invoke AddWorkspaceItems once.")]
        [Owner("Trevor Williams-Ros")]
        public void MainViewModel_UnitTest_Constructor_AddWorkspaceItems()
        {
            CompositionInitializer.InitializeForMeflessBaseViewModel();

            var localhost = new Mock<IEnvironmentModel>();
            localhost.Setup(e => e.ID).Returns(Guid.Empty);
            localhost.Setup(e => e.IsConnected).Returns(true); // so that we load resources

            var environmentRepository = new Mock<IEnvironmentRepository>();
            environmentRepository.Setup(c => c.ReadSession()).Returns(new[] { Guid.NewGuid() });
            environmentRepository.Setup(c => c.All()).Returns(new[] { localhost.Object });
            environmentRepository.Setup(c => c.Source).Returns(localhost.Object);

            var eventPublisher = new Mock<IEventAggregator>();
            var asyncWorker = AsyncWorkerTests.CreateSynchronousAsyncWorker();
            var versionChecker = new Mock<IVersionChecker>();
            var browserPopupController = new Mock<IBrowserPopupController>();

            // Create view model with connected localhost - should invoke AddWorkspaceItems
            var viewModel = new MainViewModelMock(eventPublisher.Object, asyncWorker.Object, environmentRepository.Object, versionChecker.Object, false, browserPopupController.Object);

            Assert.AreEqual(1, viewModel.AddWorkspaceItemsHitCount, "Constructor did not invoke AddWorkspaceItems.");

            // Add new server - should not invoke AddWorkspaceItems
            var newServer = new Mock<IEnvironmentModel>();
            newServer.Setup(e => e.ID).Returns(Guid.NewGuid);
            newServer.Setup(e => e.IsConnected).Returns(true); // so that we load resources

            var newServerMessage = new AddServerToExplorerMessage(newServer.Object, viewModel.ExplorerViewModel.Context);
            viewModel.ExplorerViewModel.Handle(newServerMessage);

            Assert.AreEqual(1, viewModel.AddWorkspaceItemsHitCount, "AddWorkspaceItems was invoked more than once.");
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
        [Description("Constructor must initialize navigation view model load complete event")]
        [Owner("Ashley Lewis")]
        public void MainViewModel_UnitTest_ConstructorWithNoNullParams_InitializesNavigationViewModelLoadCompleteEvent()
        {
            //main view model constructor should register event handler
            var mvm = Dev2MockFactory.MainViewModel;
            mvm.Protected().Setup("AddWorkspaceItems").Verifiable();

            //trigger event handler twice
            mvm.Object.ExplorerViewModel.NavigationViewModel.LoadEnvironmentResources(new Mock<IEnvironmentModel>().Object);
            mvm.Object.ExplorerViewModel.NavigationViewModel.LoadEnvironmentResources(new Mock<IEnvironmentModel>().Object);

            //assert handler was executed (this is actually two asserts because the handler does two things:
            //1. calls AddWorkspaceItems
            //and 2. Deregisters the LoadEnvironmentResources event (Time.Once asserts this happens))
            mvm.Protected().Verify("AddWorkspaceItems", Times.Once());
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
            var msg = new ShowDependenciesMessage(_firstResource.Object);
            _mainViewModel.Handle(msg);
            var ctx = _mainViewModel.ActiveItem;
            var vm = ctx.WorkSurfaceViewModel as DependencyVisualiserViewModel;
            Assert.IsNotNull(vm);
            Assert.IsTrue(vm.ResourceModel.Equals(_firstResource.Object));
        }

        [TestMethod]
        public void ShowDependenciesMessageExpectsNothingWithNullResource()
        {
            CreateFullExportsAndVm();
            var msg = new ShowDependenciesMessage(null);
            _mainViewModel.Handle(msg);
            Assert.IsTrue(
                _mainViewModel.Items.All(
                    i => i.WorkSurfaceKey.WorkSurfaceContext != WorkSurfaceContext.DependencyVisualiser));
        }

        #endregion

        #region Show Help Tab

        [TestMethod]
        public void ShowHelpTabMessageExpectHelpTabWithUriActive()
        {
            CreateFullExportsAndVm();
            var msg = new ShowHelpTabMessage("testuri");
            _mainViewModel.Handle(msg);
            var helpctx = _mainViewModel.ActiveItem.WorkSurfaceViewModel as HelpViewModel;
            Assert.IsNotNull(helpctx);
            Assert.IsTrue(helpctx.Uri == "testuri");
        }

        #endregion

        #region Deactivate

        [TestMethod]
        public void DeactivateWithCloseExpectBuildWithEmptyDebugWriterWriteMessage()
        {
            CreateFullExportsAndVm();
            _eventAggregator.Setup(e => e.Publish(It.IsAny<UpdateDeployMessage>()))
                .Verifiable();

            _mainViewModel.Dispose();

            _eventAggregator.Verify(e => e.Publish(It.IsAny<UpdateDeployMessage>()), Times.Exactly(1));
        }

        [TestMethod]
        public void DeactivateWithCloseAndTwoTabsExpectBuildTwiceWithEmptyDebugWriterWriteMessage()
        {
            CreateFullExportsAndVm();
            _eventAggregator.Setup(e => e.Publish(It.IsAny<UpdateDeployMessage>()))
                .Verifiable();
            AddAdditionalContext();

            _mainViewModel.Dispose();

            _eventAggregator.Verify(e => e.Publish(It.IsAny<UpdateDeployMessage>()), Times.Exactly(2));
        }

        #endregion

        #region Add Work Surface

        [TestMethod]
        public void AdditionalWorksurfaceAddedExpectsLAstAddedTOBeActive()
        {
            CreateFullExportsAndVm();
            AddAdditionalContext();
            Assert.AreEqual(3, _mainViewModel.Items.Count);
            var activeItem = _mainViewModel.ActiveItem;
            var secondKey = WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.Workflow, _secondResource.Object.ID,
                _secondResource.Object.ServerID);
            Assert.IsTrue(activeItem.WorkSurfaceKey.ResourceID.Equals(secondKey.ResourceID) && activeItem.WorkSurfaceKey.ServerID.Equals(secondKey.ServerID));
        }

        #endregion

        #region Close Context

        [TestMethod]
        public void MainViewModel_CloseWorkSurfaceContext_CloseTrueAndResourceSaved_RemoveWorkspaceItemRemoveCalledAndTabClosedMessageAndContextRemoved()
        {
            CreateFullExportsAndVm();

            Assert.AreEqual(2, _mainViewModel.Items.Count);

            _firstResource.Setup(r => r.IsWorkflowSaved).Returns(true);
            _firstResource.Setup(r => r.IsAuthorized(AuthorizationContext.Contribute)).Returns(true);
            var activetx =
                _mainViewModel.Items.ToList()
                    .First(i => i.WorkSurfaceViewModel.WorkSurfaceContext == WorkSurfaceContext.Workflow);

            _eventAggregator.Setup(e => e.Publish(It.IsAny<TabClosedMessage>()))
                .Callback<object>((o =>
                {
                    var msg = (TabClosedMessage)o;
                    Assert.IsTrue(msg.Context.Equals(activetx));
                }));

            _mainViewModel.DeactivateItem(activetx, true);
            _mockWorkspaceRepo.Verify(c => c.Remove(_firstResource.Object), Times.Once());
            Assert.IsTrue(_mainViewModel.Items.Count == 1);
            _eventAggregator.Verify(e => e.Publish(It.IsAny<TabClosedMessage>()), Times.Once());
        }

        [TestMethod]
        public void MainViewModel_CloseWorkSurfaceContext_CloseTrueAndResourceNotSavedPopupOk_RemoveWorkspaceItemCalledAndContextRemovedAndSaveResourceEventAggregatorMessage()
        {
            CreateFullExportsAndVm();

            Assert.AreEqual(2, _mainViewModel.Items.Count);
            _firstResource.Setup(r => r.IsWorkflowSaved).Returns(false);
            _firstResource.Setup(r => r.IsAuthorized(AuthorizationContext.Contribute)).Returns(true);
            PopupController.Setup(s => s.Show()).Returns(MessageBoxResult.Yes);

            var activetx =
                _mainViewModel.Items.ToList()
                    .First(i => i.WorkSurfaceViewModel.WorkSurfaceContext == WorkSurfaceContext.Workflow);

            _eventAggregator.Setup(e => e.Publish(It.IsAny<TabClosedMessage>()))
                .Callback<object>((o =>
                {
                    var msg = (TabClosedMessage)o;
                    Assert.IsTrue(msg.Context.Equals(activetx));
                }));

            _eventAggregator.Setup(e => e.Publish(It.IsAny<SaveResourceMessage>()))
                .Callback<object>((o =>
                {
                    var msg = (SaveResourceMessage)o;
                    Assert.IsTrue(msg.Resource.Equals(_firstResource.Object));
                }));

            _mainViewModel.DeactivateItem(activetx, true);
            _mockWorkspaceRepo.Verify(c => c.Remove(_firstResource.Object), Times.Once());
            Assert.IsTrue(_mainViewModel.Items.Count == 1);
            _eventAggregator.Verify(e => e.Publish(It.IsAny<TabClosedMessage>()), Times.Once());
            _eventAggregator.Verify(e => e.Publish(It.IsAny<SaveResourceMessage>()), Times.Once());
        }

        [TestMethod]
        public void MainViewModel_CloseWorkSurfaceContext_CloseTrueAndResourceNotSavedPopupNotOk_WorkspaceItemNotRemoved()
        {
            CreateFullExportsAndVm();
            Assert.AreEqual(2, _mainViewModel.Items.Count);
            _firstResource.Setup(r => r.IsWorkflowSaved).Returns(false);
            _firstResource.Setup(r => r.IsAuthorized(AuthorizationContext.Contribute)).Returns(true);

            PopupController.Setup(s => s.Show()).Returns(MessageBoxResult.No);
            var activetx =
                _mainViewModel.Items.ToList()
                    .First(i => i.WorkSurfaceViewModel.WorkSurfaceContext == WorkSurfaceContext.Workflow);
            _mainViewModel.DeactivateItem(activetx, false);
            _mockWorkspaceRepo.Verify(c => c.Remove(_firstResource.Object), Times.Never());
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("MainViewModel_ChangeActiveItem")]
        public void MainViewModel_ChangeActiveItem_WhenHasContextWithNoDataListViewModel_ClearsCollectionsOnNewItem()
        {
            //------------Setup for test--------------------------
            string errorString;
            CreateFullExportsAndVm();
            _firstResource.Setup(r => r.IsAuthorized(AuthorizationContext.Contribute)).Returns(true);
            var firstCtx = _mainViewModel.FindWorkSurfaceContextViewModel(_firstResource.Object);
            var mockDataListViewModel = new Mock<IDataListViewModel>();
            firstCtx.DataListViewModel = mockDataListViewModel.Object;
            //------------Execute Test---------------------------
            _mainViewModel.ActivateItem(firstCtx);
            //------------Assert Results-------------------------
            mockDataListViewModel.Verify(model => model.ClearCollections(), Times.Once());
            mockDataListViewModel.Verify(model => model.CreateListsOfIDataListItemModelToBindTo(out errorString), Times.Once());
        }

        [TestMethod]
        public void MainViewModel_CloseWorkSurfaceContext_CloseFalse_PreviousItemActivatedAndAllItemsPResent()
        {
            CreateFullExportsAndVm();
            AddAdditionalContext();
            Assert.AreEqual(3, _mainViewModel.Items.Count);

            _firstResource.Setup(r => r.IsAuthorized(AuthorizationContext.Contribute)).Returns(true);
            _secondResource.Setup(r => r.IsAuthorized(AuthorizationContext.Contribute)).Returns(true);

            var firstCtx = _mainViewModel.FindWorkSurfaceContextViewModel(_firstResource.Object);
            var secondCtx = _mainViewModel.FindWorkSurfaceContextViewModel(_secondResource.Object);

            _mainViewModel.ActivateItem(firstCtx);
            _mainViewModel.DeactivateItem(secondCtx, false);

            Assert.AreEqual(3, _mainViewModel.Items.Count);
            Assert.IsTrue(_mainViewModel.ActiveItem.Equals(firstCtx));
        }

        [TestMethod]
        public void MainViewModel_CloseWorkSurfaceContext_CloseTrue_PreviousItemActivatedAndOneLessItem()
        {
            CreateFullExportsAndVm();
            AddAdditionalContext();
            Assert.AreEqual(3, _mainViewModel.Items.Count);

            _firstResource.Setup(r => r.IsAuthorized(AuthorizationContext.Contribute)).Returns(true);
            _secondResource.Setup(r => r.IsAuthorized(AuthorizationContext.Contribute)).Returns(true);

            var firstCtx = _mainViewModel.FindWorkSurfaceContextViewModel(_firstResource.Object);
            var secondCtx = _mainViewModel.FindWorkSurfaceContextViewModel(_secondResource.Object);

            _mainViewModel.ActivateItem(firstCtx);
            _mainViewModel.DeactivateItem(firstCtx, true);

            Assert.AreEqual(3, _mainViewModel.Items.Count);
            Assert.IsFalse(_mainViewModel.ActiveItem.Equals(secondCtx));
        }

        [TestMethod]
        public void MainViewModel_CloseWorkSurfaceContext_CloseFalse_ContextNotRemoved()
        {
            CreateFullExportsAndVm();
            _firstResource.Setup(r => r.IsAuthorized(AuthorizationContext.Contribute)).Returns(true);
            var activetx =
                _mainViewModel.Items.ToList()
                    .First(i => i.WorkSurfaceViewModel.WorkSurfaceContext == WorkSurfaceContext.Workflow);
            _mainViewModel.DeactivateItem(activetx, false);
            _mockWorkspaceRepo.Verify(c => c.Remove(_firstResource.Object), Times.Never());
        }

        [TestMethod]
        [TestCategory("MainViewModel_CloseWorkSurfaceContext")]
        [Description("An exisiting workflow with unsaved changes that is not saved, must rollback the resource model.")]
        [Owner("Trevor Williams-Ros")]
        public void MainViewModel_CloseWorkSurfaceContext_ExistingUnsavedWorkflowNotSaved_ResourceModelRolledback()
        {
            CreateFullExportsAndVm();
            Assert.IsTrue(_mainViewModel.Items.Count == 2);
            _firstResource.Setup(r => r.IsWorkflowSaved).Returns(false);
            _firstResource.Setup(r => r.Commit()).Verifiable();
            _firstResource.Setup(r => r.Rollback()).Verifiable();
            _firstResource.Setup(r => r.IsAuthorized(AuthorizationContext.Contribute)).Returns(true);

            PopupController.Setup(s => s.Show()).Returns(MessageBoxResult.No);
            var activetx = _mainViewModel.Items.ToList().First(i => i.WorkSurfaceViewModel.WorkSurfaceContext == WorkSurfaceContext.Workflow);
            _mainViewModel.CloseWorkSurfaceContext(activetx, null);
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
            Assert.IsTrue(_mainViewModel.Items.Count == 2);
            _firstResource.Setup(r => r.IsWorkflowSaved).Returns(false);
            _firstResource.Setup(r => r.IsAuthorized(AuthorizationContext.Contribute)).Returns(true);
            _firstResource.Setup(r => r.Commit()).Verifiable();
            _firstResource.Setup(r => r.Rollback()).Verifiable();

            PopupController.Setup(s => s.Show()).Returns(MessageBoxResult.Yes);
            var activetx = _mainViewModel.Items.ToList().First(i => i.WorkSurfaceViewModel.WorkSurfaceContext == WorkSurfaceContext.Workflow);
            _mainViewModel.CloseWorkSurfaceContext(activetx, null);
            _firstResource.Verify(r => r.Commit(), Times.Once(), "ResourceModel was not committed when saved.");
            _firstResource.Verify(r => r.Rollback(), Times.Never(), "ResourceModel was rolled back when saved.");
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("MainViewModel_CloseWorkSurfaceContext")]
        public void MainViewModel_CloseWorkSurfaceContext_UnsavedWorkflowAndResourceCanSaveIsFalse_ResourceModelIsNotSaved()
        {
            //------------Setup for test--------------------------
            CreateFullExportsAndVm();
            Assert.IsTrue(_mainViewModel.Items.Count == 2);

            _firstResource.Setup(r => r.Commit()).Verifiable();
            _firstResource.Setup(r => r.Rollback()).Verifiable();
            _firstResource.Setup(r => r.IsWorkflowSaved).Returns(false);
            _firstResource.Setup(r => r.IsAuthorized(AuthorizationContext.Contribute)).Returns(false);

            _eventAggregator.Setup(e => e.Publish(It.IsAny<SaveResourceMessage>())).Verifiable();

            PopupController.Setup(s => s.Show()).Returns(MessageBoxResult.Yes);
            var activetx = _mainViewModel.Items.ToList().First(i => i.WorkSurfaceViewModel.WorkSurfaceContext == WorkSurfaceContext.Workflow);

            //------------Execute Test---------------------------
            _mainViewModel.CloseWorkSurfaceContext(activetx, null);

            //------------Assert Results-------------------------
            _eventAggregator.Verify(e => e.Publish(It.IsAny<SaveResourceMessage>()), Times.Never());
            _firstResource.Verify(r => r.Commit(), Times.Never(), "ResourceModel was committed when saved.");
            _firstResource.Verify(r => r.Rollback(), Times.Never(), "ResourceModel was rolled back when saved.");
        }


        #endregion

        #region Workspaces and init

        [TestMethod]
        public void OnImportsSatisfiedExpectsTwoItems()
        {
            CreateFullExportsAndVm();
            //One saved workspaceitem, one startpage
            Assert.AreEqual(2, _mainViewModel.Items.Count);
        }

        [TestMethod]
        public void OnImportsSatisfiedExpectsContextsAddedForSavedWorkspaces()
        {
            CreateFullExportsAndVm();
            var activetx =
                _mainViewModel.Items.ToList()
                    .First(i => i.WorkSurfaceViewModel.WorkSurfaceContext == WorkSurfaceContext.Workflow);
            var expectedKey = WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.Workflow, _firstResourceID,
                _serverID);
            Assert.IsTrue(expectedKey.ResourceID.Equals(activetx.WorkSurfaceKey.ResourceID) && expectedKey.ServerID.Equals(activetx.WorkSurfaceKey.ServerID));
        }

        [TestMethod]
        public void OnImportsSatisfiedExpectsStartpageActive()
        {
            CreateFullExportsAndVm();
            var activetx = _mainViewModel.ActiveItem;
            Assert.AreEqual(activetx.WorkSurfaceViewModel.WorkSurfaceContext, WorkSurfaceContext.Help);
            var helpvm = activetx.WorkSurfaceViewModel as HelpViewModel;
            Assert.IsNotNull(helpvm);
            Assert.AreEqual(helpvm.Uri, FileHelper.GetAppDataPath(StringResources.Uri_Studio_Homepage));
        }

        [TestMethod]
        public void OnImportsSatisfiedExpectsDisplayNameSet()
        {
            CreateFullExportsAndVm();
            _mainViewModel.OnImportsSatisfied();
            const string expected = "Warewolf ()";
            Assert.AreEqual(expected, _mainViewModel.DisplayName);
        }

        #endregion workspaces

        #region Methods used by tests

        void CreateFullExportsAndVmWithEmptyRepo()
        {
            CreateResourceRepo();
            var mockEnv = new Mock<IEnvironmentRepository>();
            mockEnv.SetupProperty(g => g.ActiveEnvironment); // Start tracking changes
            mockEnv.Setup(g => g.All()).Returns(new List<IEnvironmentModel>());
            mockEnv.Setup(c => c.ReadSession()).Returns(new[] { Guid.NewGuid() });
            mockEnv.Setup(repository => repository.Source).Returns(new Mock<IEnvironmentModel>().Object);
            var environmentRepo = mockEnv.Object;

            _eventAggregator = new Mock<IEventAggregator>();
            PopupController = new Mock<IPopupController>();
            _feedbackInvoker = new Mock<IFeedbackInvoker>();
            _webController = new Mock<IWebController>();
            _windowManager = new Mock<IWindowManager>();
            SetupDefaultMef(_feedbackInvoker);
            Mock<IAsyncWorker> asyncWorker = AsyncWorkerTests.CreateSynchronousAsyncWorker();
            Mock<IWorkspaceItemRepository> mockWorkspaceItemRepository = GetworkspaceItemRespository();
            new WorkspaceItemRepository(mockWorkspaceItemRepository.Object);
            _mainViewModel = new MainViewModel(_eventAggregator.Object, asyncWorker.Object, environmentRepo,
                new Mock<IVersionChecker>().Object, false, null, PopupController.Object,
                _windowManager.Object, _webController.Object, _feedbackInvoker.Object);
        }

        void CreateFullExportsAndVm()
        {
            CreateResourceRepo();
            var environmentRepo = GetEnvironmentRepository();
            _eventAggregator = new Mock<IEventAggregator>();
            EventPublishers.Aggregator = _eventAggregator.Object;
            PopupController = new Mock<IPopupController>();
            _feedbackInvoker = new Mock<IFeedbackInvoker>();
            _webController = new Mock<IWebController>();
            _windowManager = new Mock<IWindowManager>();
            SetupDefaultMef(_feedbackInvoker);
            Mock<IAsyncWorker> asyncWorker = AsyncWorkerTests.CreateSynchronousAsyncWorker();
            Mock<IWorkspaceItemRepository> mockWorkspaceItemRepository = GetworkspaceItemRespository();
            new WorkspaceItemRepository(mockWorkspaceItemRepository.Object);
            _mainViewModel = new MainViewModel(_eventAggregator.Object, asyncWorker.Object, environmentRepo,
                new Mock<IVersionChecker>().Object, false, null, PopupController.Object
                , _windowManager.Object, _webController.Object, _feedbackInvoker.Object);

            _activeEnvironment = new Mock<IEnvironmentModel>();
            _authorizationService = new Mock<IAuthorizationService>();
            _activeEnvironment.Setup(e => e.AuthorizationService).Returns(_authorizationService.Object);

            _mainViewModel.ActiveEnvironment = _activeEnvironment.Object;
        }

        private Mock<IContextualResourceModel> CreateResource(ResourceType resourceType)
        {
            var result = new Mock<IContextualResourceModel>();
            result.Setup(c => c.ResourceName).Returns(ResourceName);
            result.Setup(c => c.ResourceType).Returns(resourceType);
            result.Setup(c => c.DisplayName).Returns(DisplayName);
            result.Setup(c => c.WorkflowXaml).Returns(new StringBuilder(ServiceDefinition));
            result.Setup(c => c.Category).Returns("Testing");
            result.Setup(c => c.Environment).Returns(_environmentModel.Object);
            result.Setup(c => c.ServerID).Returns(_serverID);
            result.Setup(c => c.ID).Returns(_firstResourceID);
            result.Setup(c => c.UserPermissions).Returns(Permissions.Contribute);
            return result;
        }

        private IEnvironmentRepository GetEnvironmentRepository()
        {
            var models = new List<IEnvironmentModel> { _environmentModel.Object };
            var mock = new Mock<IEnvironmentRepository>();
            mock.Setup(s => s.All()).Returns(models);
            mock.Setup(c => c.ReadSession()).Returns(new[] { _environmentModel.Object.ID });
            mock.Setup(s => s.Source).Returns(_environmentModel.Object);
            _environmentRepo = mock.Object;
            return _environmentRepo;
        }

        void CreateResourceRepo()
        {
            var msg = new ExecuteMessage { HasError = false };
            msg.SetMessage("");
            _environmentModel = CreateMockEnvironment();
            _resourceRepo = new Mock<IResourceRepository>();
            _resourceRepo.Setup(r => r.FetchResourceDefinition(It.IsAny<IEnvironmentModel>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new ExecuteMessage());
            _resourceRepo.Setup(r => r.GetDependenciesXml(It.IsAny<IContextualResourceModel>(), It.IsAny<bool>())).Returns(msg);
            _firstResource = CreateResource(ResourceType.WorkflowService);
            var coll = new Collection<IResourceModel> { _firstResource.Object };
            _resourceRepo.Setup(c => c.All()).Returns(coll);
            _environmentModel.Setup(m => m.ResourceRepository).Returns(_resourceRepo.Object);
        }

        private Mock<IEnvironmentConnection> CreateMockConnection(Random rand, params string[] sources)
        {
            var connection = new Mock<IEnvironmentConnection>();
            connection.Setup(c => c.ServerID).Returns(Guid.NewGuid());
            connection.SetupGet(c => c.WorkspaceID).Returns(_workspaceID);
            connection.SetupGet(c => c.ServerID).Returns(_serverID);
            connection.Setup(c => c.AppServerUri)
                .Returns(new Uri(string.Format("http://127.0.0.{0}:{1}/dsf", rand.Next(1, 100), rand.Next(1, 100))));
            connection.Setup(c => c.WebServerUri)
                .Returns(new Uri(string.Format("http://127.0.0.{0}:{1}", rand.Next(1, 100), rand.Next(1, 100))));
            connection.Setup(c => c.IsConnected).Returns(true);
            int cnt = 0;
            connection.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(
                    () =>
                    {
                        if(cnt == 0)
                        {
                            cnt++;
                            return new StringBuilder(string.Format("<XmlData>{0}</XmlData>", string.Join("\n", sources)));
                        }

                        return new StringBuilder(JsonConvert.SerializeObject(new ExecuteMessage()));
                    }
                );
            connection.Setup(c => c.ServerEvents).Returns(new EventPublisher());
            return connection;
        }

        private Mock<IEnvironmentModel> CreateMockEnvironment(params string[] sources)
        {
            var rand = new Random();
            var connection = CreateMockConnection(rand, sources);
            var env = new Mock<IEnvironmentModel>();
            env.Setup(e => e.Connection).Returns(connection.Object);
            env.Setup(e => e.IsConnected).Returns(true);
            env.Setup(e => e.ID).Returns(Guid.NewGuid());
            env.Setup(e => e.Name).Returns(string.Format("Server_{0}", rand.Next(1, 100)));
            return env;
        }

        private Mock<IWorkspaceItemRepository> GetworkspaceItemRespository()
        {
            _mockWorkspaceRepo = new Mock<IWorkspaceItemRepository>();
            var list = new List<IWorkspaceItem>();
            var item = new Mock<IWorkspaceItem>();
            item.SetupGet(i => i.WorkspaceID).Returns(_workspaceID);
            item.SetupGet(i => i.ServerID).Returns(_serverID);
            item.SetupGet(i => i.ServiceName).Returns(ResourceName);
            list.Add(item.Object);
            _mockWorkspaceRepo.SetupGet(c => c.WorkspaceItems).Returns(list);
            _mockWorkspaceRepo.Setup(c => c.UpdateWorkspaceItem(It.IsAny<IContextualResourceModel>(), It.IsAny<bool>())).Returns(new ExecuteMessage());
            _mockWorkspaceRepo.Setup(c => c.Remove(_firstResource.Object)).Verifiable();
            return _mockWorkspaceRepo;
        }

        void AddAdditionalContext()
        {
            _secondResource = new Mock<IContextualResourceModel>();
            _secondResource.Setup(c => c.ResourceName).Returns("WhoCares");
            _secondResource.Setup(c => c.ResourceType).Returns(ResourceType.WorkflowService);
            _secondResource.Setup(c => c.WorkflowXaml).Returns(new StringBuilder());
            _secondResource.Setup(c => c.Category).Returns("Testing2");
            _secondResource.Setup(c => c.Environment).Returns(_environmentModel.Object);
            _secondResource.Setup(c => c.ServerID).Returns(_serverID);
            _secondResource.Setup(c => c.ID).Returns(_secondResourceID);
            _secondResource.Setup(c => c.UserPermissions).Returns(Permissions.Contribute);
            var msg = new AddWorkSurfaceMessage(_secondResource.Object);
            _mainViewModel.Handle(msg);
        }

        void SetupForDelete()
        {
            PopupController.Setup(c => c.Show()).Verifiable();
            PopupController.Setup(s => s.Show()).Returns(MessageBoxResult.Yes);
            _resourceRepo.Setup(c => c.HasDependencies(_firstResource.Object)).Returns(false).Verifiable();
            var succesResponse = new ExecuteMessage();

            succesResponse.SetMessage(@"<DataList>Success</DataList>");
            _resourceRepo.Setup(s => s.DeleteResource(_firstResource.Object)).Returns(succesResponse);
        }

        Mock<IEnvironmentRepository> SetupForDeleteServer()
        {
            CreateResourceRepo();
            var models = new List<IEnvironmentModel> { _environmentModel.Object };
            var mock = new Mock<IEnvironmentRepository>();
            mock.Setup(s => s.All()).Returns(models);
            mock.Setup(s => s.Get(It.IsAny<Guid>())).Returns(_environmentModel.Object);
            mock.Setup(s => s.Source).Returns(_environmentModel.Object);
            mock.Setup(s => s.ReadSession()).Returns(new[] { _environmentModel.Object.ID });
            mock.Setup(s => s.Remove(It.IsAny<IEnvironmentModel>()))
                .Callback<IEnvironmentModel>(s =>
                    Assert.AreEqual(_environmentModel.Object, s))
                .Verifiable();
            PopupController = new Mock<IPopupController>();
            _eventAggregator = new Mock<IEventAggregator>();
            _eventAggregator.Setup(e => e.Publish(It.IsAny<EnvironmentDeletedMessage>()))
                .Callback<object>(m =>
                {
                    var removeMsg = (EnvironmentDeletedMessage)m;
                    Assert.AreEqual(_environmentModel.Object, removeMsg.EnvironmentModel);
                })
                .Verifiable();
            SetupDefaultMef();
            Mock<IAsyncWorker> asyncWorker = AsyncWorkerTests.CreateSynchronousAsyncWorker();
            _mainViewModel = new MainViewModel(_eventAggregator.Object, asyncWorker.Object, mock.Object, new Mock<IVersionChecker>().Object, false);
            SetupForDelete();
            _firstResource.Setup(r => r.ResourceType).Returns(ResourceType.Source);
            _firstResource.Setup(r => r.ServerResourceType).Returns("Server");
            _firstResource.Setup(r => r.ConnectionString)
                .Returns(TestResourceStringsTest.ResourceToHydrateConnectionString1);
            _environmentConnection = new Mock<IEnvironmentConnection>();
            _environmentConnection.Setup(c => c.AppServerUri)
                .Returns(new Uri(TestResourceStringsTest.ResourceToHydrateActualAppUri));
            _environmentModel.Setup(r => r.Connection).Returns(_environmentConnection.Object);
            return mock;
        }

        void SetupDefaultMef()
        {
            SetupDefaultMef(new Mock<IFeedbackInvoker>());
        }

        static void SetupDefaultMef(Mock<IFeedbackInvoker> feedbackInvoker)
        {
            ImportService.CurrentContext = new ImportServiceContext();
            ImportService.Initialize(new List<ComposablePartCatalog>());
            ImportService.AddExportedValueToContainer(new Mock<IPopupController>().Object);
            ImportService.AddExportedValueToContainer(feedbackInvoker.Object);
            ImportService.AddExportedValueToContainer<IFeedBackRecorder>(new FeedbackRecorder());
            ImportService.AddExportedValueToContainer(new Mock<IWindowManager>().Object);
        }

        #endregion Methods used by tests

        #region Commands

        [TestMethod]
        public void DisplayAboutDialogueCommandExpectsWindowManagerShowingIDialogueViewModel()
        {
            CreateFullExportsAndVm();
            _windowManager.Setup(w => w.ShowDialog(It.IsAny<IDialogueViewModel>(), null, null)).Verifiable();
            _mainViewModel.DisplayAboutDialogueCommand.Execute(null);
            _windowManager.Verify(w => w.ShowDialog(It.IsAny<IDialogueViewModel>(), null, null), Times.Once());
        }

        [TestMethod]
        public void AddStudioShortcutsPageCommandExpectsShortKeysActive()
        {
            CreateFullExportsAndVm();
            _mainViewModel.AddStudioShortcutsPageCommand.Execute(null);
            var shortkeyUri = FileHelper.GetFullPath(StringResources.Uri_Studio_Shortcut_Keys_Document);
            var helpctx = _mainViewModel.ActiveItem.WorkSurfaceViewModel as HelpViewModel;
            Assert.IsNotNull(helpctx);
            Assert.IsTrue(helpctx.Uri == shortkeyUri);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("MainViewModel_AddLanguageHelpPageCommand")]
        public void MainViewModel_AddLanguageHelpPageCommand_LanguageHelpActive()
        {
            CreateFullExportsAndVm();
            //------------Execute Test---------------------------
            _mainViewModel.AddLanguageHelpPageCommand.Execute(null);

            // Assert LanguageHelp is active
            var languageHelpUri = FileHelper.GetFullPath(StringResources.Uri_Studio_Language_Reference_Document);
            var langHelpCtx = _mainViewModel.ActiveItem.WorkSurfaceViewModel as HelpViewModel;
            Assert.IsNotNull(langHelpCtx);
            Assert.IsTrue(langHelpCtx.Uri == languageHelpUri);
        }

        [TestMethod]
        public void NewResourceCommandExpectsWebControllerDisplayDialogue()
        {
            CreateFullExportsAndVm();
            _webController.Setup(w => w.DisplayDialogue(It.IsAny<IContextualResourceModel>(), false)).Verifiable();
            _mainViewModel.Handle(new SetActiveEnvironmentMessage(_environmentModel.Object));
            _mainViewModel.NewResourceCommand.Execute("Service");
            _webController.Verify(w => w.DisplayDialogue(It.IsAny<IContextualResourceModel>(), false), Times.Once());
        }

        [TestMethod]
        public void StartFeedbackCommandCommandExpectsFeedbackInvoked()
        {
            CreateFullExportsAndVm();
            _feedbackInvoker.Setup(
                i => i.InvokeFeedback(It.IsAny<EmailFeedbackAction>(), It.IsAny<RecorderFeedbackAction>()))
                .Verifiable();
            _mainViewModel.StartFeedbackCommand.Execute(null);
            _feedbackInvoker.Verify(
                i => i.InvokeFeedback(It.IsAny<EmailFeedbackAction>(), It.IsAny<RecorderFeedbackAction>()),
                Times.Once());
        }

        [TestMethod]
        public void StartStopRecordedFeedbackCommandExpectsFeedbackStartedWhenNotInProgress()
        {
            CreateFullExportsAndVm();
            _feedbackInvoker.Setup(i => i.InvokeFeedback(It.IsAny<RecorderFeedbackAction>())).Verifiable();
            _mainViewModel.StartStopRecordedFeedbackCommand.Execute(null);
            _feedbackInvoker.Verify(i => i.InvokeFeedback(It.IsAny<RecorderFeedbackAction>()), Times.Once());
        }

        [TestMethod]
        public void StartStopRecordedFeedbackCommandExpectsFeedbackStppedtInProgress()
        {
            CreateFullExportsAndVm();
            var mockAction = new Mock<IAsyncFeedbackAction>();
            mockAction.Setup(a => a.StartFeedback()).Verifiable();
            _feedbackInvoker.SetupGet(i => i.CurrentAction).Returns(mockAction.Object);
            _mainViewModel.StartStopRecordedFeedbackCommand.Execute(null);
            _feedbackInvoker.Verify(i => i.InvokeFeedback(It.IsAny<RecorderFeedbackAction>()), Times.Never());

            // PBI 9598 - 2013.06.10 - TWR : added null parameter
            mockAction.Verify(a => a.FinishFeedBack(It.IsAny<IEnvironmentModel>()), Times.Once());
        }

        [TestMethod]
        public void DeployAllCommandWithoutCurrentResourceExpectsDeplouViewModelActive()
        {
            CreateFullExportsAndVmWithEmptyRepo();
            _mainViewModel.Handle(new SetActiveEnvironmentMessage(_environmentModel.Object));
            _mainViewModel.DeployAllCommand.Execute(null);
            var activectx = _mainViewModel.ActiveItem;
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
            var environmentRepo = CreateMockEnvironment();
            var resourceRepo = new Mock<IResourceRepository>();
            resourceRepo.Setup(c => c.FetchResourceDefinition(It.IsAny<IEnvironmentModel>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new ExecuteMessage());
            resourceRepo.Setup(r => r.Save(It.IsAny<IResourceModel>())).Verifiable();
            environmentRepo.Setup(e => e.ResourceRepository).Returns(resourceRepo.Object);
            environmentRepo.Setup(e => e.IsConnected).Returns(true);
            _mainViewModel.ActiveEnvironment = environmentRepo.Object;
            _mainViewModel.NewResourceCommand.Execute("Workflow");
            //Assert
            resourceRepo.Verify(r => r.Save(It.IsAny<IResourceModel>()), Times.Never());
        }

        #endregion

        #region Delete

        [TestMethod]
        public void DeleteServerResourceOnLocalHostAlsoDeletesFromEnvironmentRepoAndExplorerTree()
        {
            //---------Setup------
            var mock = SetupForDeleteServer();
            _environmentModel.Setup(s => s.IsLocalHost()).Returns(true);
            //---------Execute------
            var msg = new DeleteResourcesMessage(new List<IContextualResourceModel> { _firstResource.Object }, false);
            _mainViewModel.Handle(msg);

            //---------Verify------
            mock.Verify(s => s.Remove(It.IsAny<IEnvironmentModel>()), Times.Once());
            _eventAggregator.Verify(e => e.Publish(It.IsAny<EnvironmentDeletedMessage>()), Times.Once());
        }

        [TestMethod]
        public void DeleteServerResourceOnOtherServerDoesntDeleteFromEnvironmentRepoAndExplorerTree()
        {
            //---------Setup------
            var mock = SetupForDeleteServer();
            _environmentConnection.Setup(c => c.DisplayName).Returns("NotLocalHost");
            _eventAggregator = new Mock<IEventAggregator>();
            _eventAggregator.Setup(e => e.Publish(It.IsAny<EnvironmentDeletedMessage>())).Verifiable();

            //---------Execute------
            var msg = new DeleteResourcesMessage(new List<IContextualResourceModel> { _firstResource.Object }, false);
            _mainViewModel.Handle(msg);

            //---------Verify------
            mock.Verify(s => s.Remove(It.IsAny<IEnvironmentModel>()), Times.Never());
            _eventAggregator.Verify(e => e.Publish(It.IsAny<EnvironmentDeletedMessage>()), Times.Never());
        }

        //[TestMethod]
        //[Ignore]//might cause nodes to remain in the tree even if the resource is missing from the catalog
        ////check this test again after navigation tree binds to resource catalog
        //public void DeleteResourceConfirmedWithNoResponseExpectNoMessage()
        //{
        //    CreateFullExportsAndVm();
        //    SetupForDelete();

        //    _resourceRepo.Setup(s => s.DeleteResource(_firstResource.Object)).Returns(() => MakeMsg("<Result>Failure</Result>"));

        //    var msg = new DeleteResourcesMessage(new List<IContextualResourceModel> { _firstResource.Object }, false);
        //    _mainViewModel.Handle(msg);
        //    _eventAggregator.Verify(e => e.Publish(It.IsAny<RemoveNavigationResourceMessage>()), Times.Never());
        //}

        //[TestMethod]
        //[Ignore]//might cause nodes to remain in the tree even if the resource is missing from the catalog
        ////check this test again after navigation tree binds to resource catalog
        //public void DeleteResourceConfirmedWithInvalidResponseExpectNoMessage()
        //{
        //    CreateFullExportsAndVm();
        //    SetupForDelete();
        //    var response = MakeMsg("<DataList>Invalid</DataList>");
        //    _resourceRepo.Setup(s => s.DeleteResource(_firstResource.Object)).Returns(response);

        //    var msg = new DeleteResourcesMessage(new List<IContextualResourceModel> { _firstResource.Object }, false);
        //    _mainViewModel.Handle(msg);
        //    _eventAggregator.Verify(e => e.Publish(It.IsAny<RemoveNavigationResourceMessage>()), Times.Never());
        //}

        [TestMethod]
        public void DeleteResourceConfirmedExpectRemoveNavigationResourceMessage()
        {
            CreateFullExportsAndVm();
            SetupForDelete();

            _eventAggregator.Setup(e => e.Publish(It.IsAny<RemoveNavigationResourceMessage>()))
                .Callback<object>((o =>
                {
                    var m = (RemoveNavigationResourceMessage)o;
                    Assert.IsTrue(m.ResourceModel.Equals(_firstResource.Object));
                }));

            var msg = new DeleteResourcesMessage(new List<IContextualResourceModel> { _firstResource.Object }, false);
            _mainViewModel.Handle(msg);
            _eventAggregator.Verify(e => e.Publish(It.IsAny<RemoveNavigationResourceMessage>()), Times.Once());
        }

        [TestMethod]
        public void DeleteResourceConfirmedExpectContextRemoved()
        {
            CreateFullExportsAndVm();
            SetupForDelete();
            var msg = new DeleteResourcesMessage(new List<IContextualResourceModel> { _firstResource.Object });
            _mainViewModel.Handle(msg);
            _resourceRepo.Verify(s => s.HasDependencies(_firstResource.Object), Times.Once());
        }

        [TestMethod]
        public void DeleteResourceWithConfirmExpectsDependencyServiceCalled()
        {
            CreateFullExportsAndVm();
            SetupForDelete();
            PopupController.Setup(s => s.Show()).Returns(MessageBoxResult.Yes);
            var msg = new DeleteResourcesMessage(new List<IContextualResourceModel> { _firstResource.Object });
            _mainViewModel.Handle(msg);
            _resourceRepo.Verify(s => s.HasDependencies(_firstResource.Object), Times.Once());
        }

        [TestMethod]
        public void DeleteResourceWithDeclineExpectsDependencyServiceCalled()
        {
            CreateFullExportsAndVm();
            SetupForDelete();
            PopupController.Setup(s => s.Show()).Returns(MessageBoxResult.No);
            var msg = new DeleteResourcesMessage(new List<IContextualResourceModel> { _firstResource.Object }, false);
            _mainViewModel.Handle(msg);
            _resourceRepo.Verify(s => s.HasDependencies(_firstResource.Object), Times.Never());
        }

        [TestMethod]
        public void DeleteResourceWithNullResourceExpectsNoPoupShown()
        {
            CreateFullExportsAndVm();
            SetupForDelete();
            var msg = new DeleteResourcesMessage(null, false);
            _mainViewModel.Handle(msg);
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
            var msg = new DeleteResourcesMessage(new List<IContextualResourceModel> { unassignedResource.Object }, false);

            //Run delete command
            _mainViewModel.Handle(msg);

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
            _environmentModel.Setup(s => s.IsLocalHost()).Returns(true);

            //---------Execute------
            var msg = new DeleteResourcesMessage(new List<IContextualResourceModel> { null }, false);
            _mainViewModel.Handle(msg);

            //---------Verify------
            mock.Verify(s => s.Remove(It.IsAny<IEnvironmentModel>()), Times.Never());
            _eventAggregator.Verify(e => e.Publish(It.IsAny<EnvironmentDeletedMessage>()), Times.Never());
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("MainViewModel_HandleDeleteResourceMessage")]
        public void MainViewModel_HandleDeleteResourceMessage_PublishRemoveNavigationNodeWithContext()
        {
            var mockedEnvironment = new Mock<IEnvironmentModel>();
            var mockedEnvironmentRepo = new Mock<IEnvironmentRepository>();
            var mockedResourceRepo = new Mock<IResourceRepository>();
            var mockedEventAggregator = new Mock<IEventAggregator>();
            var resourceToDelete = new Mock<IContextualResourceModel>();

            mockedEnvironment.Setup(env => env.ResourceRepository).Returns(mockedResourceRepo.Object);
            mockedEnvironmentRepo.Setup(env => env.Source).Returns(mockedEnvironment.Object);
            mockedResourceRepo.Setup(repo => repo.DeleteResource(It.IsAny<IResourceModel>()))
                .Returns(MakeMsg("<DataList>Success</DataList>"));
            RemoveNavigationResourceMessage msg = null;
            mockedEventAggregator.Setup(e => e.Publish(It.IsAny<RemoveNavigationResourceMessage>()))
                .Callback<Object>(m =>
                {
                    msg = (RemoveNavigationResourceMessage)m;
                })
                .Verifiable();
            var mainViewModel = new MainViewModel(mockedEventAggregator.Object, new Mock<IAsyncWorker>().Object, mockedEnvironmentRepo.Object, new VersionChecker());
            resourceToDelete.Setup(res => res.Environment).Returns(mockedEnvironment.Object);

            //------------Execute Test---------------------------
            mainViewModel.Handle(new DeleteResourcesMessage(new Collection<IContextualResourceModel> { resourceToDelete.Object }, false));

            // Assert Result
            Assert.IsNotNull(msg, "Remove naviagtion node message not published");
            Assert.AreEqual(mockedEnvironment.Object, msg.ResourceModel.Environment, "Message does not contain environment");

        }

        #endregion delete

        #region ShowStartPage

        // PBI 9512 - 2013.06.07 - TWR: added
        [TestMethod]
        public void MainViewModelShowStartPageExpectedGetsLatestFirst()
        {
            CreateFullExportsAndVm();
            var versionChecker = Mock.Get(_mainViewModel.Version);
            versionChecker.Setup(v => v.StartPageUri).Verifiable();
            _mainViewModel.ShowStartPage();
            versionChecker.Verify(v => v.StartPageUri);
        }

        #endregion

        #region ShowDependencies

        [TestMethod]
        public void IHandleShowDependenciesActivatesDependecies()
        {
            CreateFullExportsAndVm();
            var msg = new ShowDependenciesMessage(_firstResource.Object);
            _mainViewModel.Handle(msg);

            Assert.AreEqual(_mainViewModel.ActiveItem.WorkSurfaceKey.WorkSurfaceContext, WorkSurfaceContext.DependencyVisualiser);
        }

        [TestMethod]
        public void IHandleShowDependenciesActivatesReverseDependecies()
        {
            CreateFullExportsAndVm();
            var msg = new ShowDependenciesMessage(_firstResource.Object, true);
            _mainViewModel.Handle(msg);
            Assert.AreEqual(_mainViewModel.ActiveItem.WorkSurfaceKey.WorkSurfaceContext, WorkSurfaceContext.ReverseDependencyVisualiser);
        }

        #endregion

        #region IHandle

        [TestMethod]
        public void IHandleShowDependencies()
        {
            CreateFullExportsAndVm();
            Assert.IsInstanceOfType(_mainViewModel, typeof(IHandle<ShowDependenciesMessage>));
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

            Mock<IPopupController> mockPopUp = Dev2MockFactory.CreateIPopup(MessageBoxResult.No);
            mockPopUp.Setup(m => m.Show()).Verifiable();
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
            mockPopUp.Verify(m => m.Show(), Times.Once());
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
            var importServiceContext = new ImportServiceContext();
            ImportService.CurrentContext = importServiceContext;
            ImportService.Initialize(new List<ComposablePartCatalog>());
            ImportService.AddExportedValueToContainer(wsiRepo.Object);

            new WorkspaceItemRepository(wsiRepo.Object);
            ImportService.AddExportedValueToContainer(new Mock<IPopupController>().Object);
        }

        #endregion

        #region BrowserPopupController

        // BUG 9798 - 2013.06.25 - TWR : added
        [TestMethod]
        public void MainViewModelShowCommunityPageExpectedInvokesConstructorsBrowserPopupController()
        {
            var popupController = new Mock<IBrowserPopupController>();
            popupController.Setup(p => p.ShowPopup(It.IsAny<string>())).Verifiable();

            var envRepo = new Mock<IEnvironmentRepository>();
            envRepo.Setup(e => e.All()).Returns(new List<IEnvironmentModel>());
            envRepo.Setup(e => e.Source).Returns(new Mock<IEnvironmentModel>().Object);
            envRepo.Setup(e => e.ReadSession()).Returns(new[] { Guid.NewGuid() });
            var vm = new MainViewModel(new Mock<IEventAggregator>().Object, new Mock<IAsyncWorker>().Object, envRepo.Object, new Mock<IVersionChecker>().Object, false, popupController.Object);
            vm.ShowCommunityPage();

            popupController.Verify(p => p.ShowPopup(It.IsAny<string>()));
        }

        // BUG 9798 - 2013.06.25 - TWR : added
        [TestMethod]
        public void MainViewModelConstructorWithNullBrowserPopupControllerExpectedCreatesExternalBrowserPopupController()
        {
            var mockEventAggregator = new Mock<IEventAggregator>();
            var envRepo = new Mock<IEnvironmentRepository>();
            envRepo.Setup(e => e.All()).Returns(new List<IEnvironmentModel>());
            envRepo.Setup(e => e.Source).Returns(new Mock<IEnvironmentModel>().Object);
            var vm = new MainViewModel(mockEventAggregator.Object, new Mock<IAsyncWorker>().Object, envRepo.Object, new Mock<IVersionChecker>().Object, false);
            Assert.IsInstanceOfType(vm.BrowserPopupController, typeof(ExternalBrowserPopupController));
        }

        // BUG 9941 - 2013.07.07 - TWR : added
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
        public void SetActiveEnvironmentCallsUpdateActiveEnvironmentMessageExpectedUpdateActiveEnvironmentMessagePublished()
        {
            Mock<IEnvironmentModel> mockEnv = Dev2MockFactory.SetupEnvironmentModel();
            CreateFullExportsAndVmWithEmptyRepo();
            _mainViewModel.Handle(new SetActiveEnvironmentMessage(mockEnv.Object));
            _eventAggregator.Verify(c => c.Publish(It.IsAny<UpdateActiveEnvironmentMessage>()), Times.Once());
        }

        [TestMethod]
        [TestCategory("MainViewModel_SetActiveEnvironmentMessage")]
        [Owner("Trevor Williams-Ros")]
        public void MainViewModel_SetActiveEnvironmentMessage_SetsActiveEnvironmentOnEnvironmentRepository()
        {
            Mock<IEnvironmentModel> mockEnv = Dev2MockFactory.SetupEnvironmentModel();
            CreateFullExportsAndVmWithEmptyRepo();
            _mainViewModel.Handle(new SetActiveEnvironmentMessage(mockEnv.Object));
            Assert.AreSame(mockEnv.Object, _mainViewModel.EnvironmentRepository.ActiveEnvironment);
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
            Assert.AreEqual(AuthorizationContext.Contribute, _mainViewModel.NewResourceCommand.AuthorizationContext);
            Assert.AreEqual(AuthorizationContext.Administrator, _mainViewModel.SettingsCommand.AuthorizationContext);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("MainViewModel_AuthorizeCommands")]
        public void MainViewModel_AuthorizeCommands_ActiveEnvironmentChanged_UpdateContextInvoked()
        {
            //------------Setup for test--------------------------            
            CreateFullExportsAndVmWithEmptyRepo();

            Assert.IsNull(_mainViewModel.NewResourceCommand.AuthorizationService);
            Assert.IsNull(_mainViewModel.SettingsCommand.AuthorizationService);

            var authService = new Mock<IAuthorizationService>();

            var env = new Mock<IEnvironmentModel>();
            env.Setup(e => e.AuthorizationService).Returns(authService.Object);

            //------------Execute Test---------------------------
            _mainViewModel.ActiveEnvironment = env.Object;

            //------------Assert Results-------------------------
            Assert.AreSame(authService.Object, _mainViewModel.NewResourceCommand.AuthorizationService);
            Assert.AreSame(authService.Object, _mainViewModel.SettingsCommand.AuthorizationService);
        }

        [TestMethod]
        public void IsActiveEnvironmentConnectExpectFalseWithNullEnvironment()
        {
            CreateFullExportsAndVm();
            var actual = _mainViewModel.IsActiveEnvironmentConnected();
            Assert.IsFalse(actual);
            Assert.IsFalse(_mainViewModel.HasActiveConnection);
        }

        #endregion

        [TestMethod]
        [TestCategory("MainViewModel_ShowStartPage")]
        [Description("ShowStartPage must load start page asynchronously.")]
        [Owner("Trevor Williams-Ros")]
        public void MainViewModel_UnitTest_ShowStartPage_InvokesAsyncWorker()
        {
            SetupDefaultMef();
            var eventPublisher = new Mock<IEventAggregator>();
            var environmentRepository = new Mock<IEnvironmentRepository>();
            environmentRepository.Setup(repo => repo.Source).Returns(new Mock<IEnvironmentModel>().Object);
            var versionChecker = new Mock<IVersionChecker>();
            var asyncWorker = new Mock<IAsyncWorker>();
            asyncWorker.Setup(w => w.Start(It.IsAny<Action>(), It.IsAny<Action>())).Verifiable();
            var mvm = new MainViewModel(eventPublisher.Object, asyncWorker.Object, environmentRepository.Object, versionChecker.Object, false);
            mvm.ShowStartPage();
            asyncWorker.Verify(w => w.Start(It.IsAny<Action>(), It.IsAny<Action>()), "ShowStartPage did not load start page asynchronously.");
        }

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

            var envRepo = new Mock<IEnvironmentRepository>();
            envRepo.Setup(e => e.All()).Returns(new List<IEnvironmentModel>());
            var environmentModel = new Mock<IEnvironmentModel>().Object;
            envRepo.Setup(e => e.Source).Returns(environmentModel);
            envRepo.Setup(e => e.Source.IsConnected).Returns(false);
            envRepo.Setup(e => e.Source.Connection.IsConnected).Returns(false);
            var vm = new MainViewModel(eventAggregator.Object, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object, envRepo.Object, new Mock<IVersionChecker>().Object, false, new Mock<IBrowserPopupController>().Object);
            var expected = new Mock<AbstractTreeViewModel>(new Mock<IEventAggregator>().Object, new Mock<ITreeNode>().Object);
            expected.Setup(model => model.EnvironmentModel).Returns(environmentModel);
            var deployMessage = new DeployResourcesMessage(expected.Object);
            vm.Handle(deployMessage);
            eventAggregator.Verify(e => e.Publish(It.IsAny<object>()), "MainViewModel Handle DeployResourcesMessage did not publish message with the selected view model.");
            Assert.IsNotNull(actual, "MainViewModel Handle DeployResourcesMessage did not publish message with the selected view model.");
            Assert.AreSame(expected.Object.DisplayName, actual.DisplayName, "MainViewModel Handle DeployResourcesMessage did not publish message with the selected display name.");
            Assert.AreSame(expected.Object.EnvironmentModel, actual.Environment, "MainViewModel Handle DeployResourcesMessage did not publish message with the selected environment.");
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

            var workSurfaceContextViewModel = new Mock<WorkSurfaceContextViewModel>(eventAggregator.Object, new WorkSurfaceKey(), new Mock<IWorkSurfaceViewModel>().Object);
            workSurfaceContextViewModel.Setup(v => v.Debug()).Verifiable();

            vm.ActiveItem = workSurfaceContextViewModel.Object;

            //------------Execute Test---------------------------
            vm.Handle(new AddWorkSurfaceMessage(new Mock<IWorkSurfaceObject>().Object) { ShowDebugWindowOnLoad = showDebugWindowOnLoad });

            //------------Assert Results-------------------------
            workSurfaceContextViewModel.Verify(v => v.Debug(), showDebugWindowOnLoad ? Times.Once() : Times.Never());

        }
    }
}
