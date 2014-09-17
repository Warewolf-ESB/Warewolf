using Caliburn.Micro;
using Dev2.AppResources.Repositories;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Common.Interfaces.Infrastructure.Events;
using Dev2.Communication;
using Dev2.ConnectionHelpers;
using Dev2.Core.Tests.Environments;
using Dev2.Core.Tests.Utils;
using Dev2.Models;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Enums;
using Dev2.Studio.ViewModels.Navigation;
using Dev2.Util;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Windows.Threading;

// ReSharper disable  InconsistentNaming
namespace Dev2.Core.Tests
{
    /// <summary>
    ///     Summary description for NavigationViewModelTest
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class NavigationViewModelTest
    {
        #region Test Variables

        Mock<IEnvironmentModel> _mockEnvironmentModel;
        Mock<IContextualResourceModel> _mockResourceModel;
        Mock<IContextualResourceModel> _mockResourceModel1;
        Mock<IContextualResourceModel> _mockResourceModel2;
        Mock<IContextualResourceModel> _mockResourceSourceModel;
        readonly Action<System.Action, DispatcherPriority> _Invoke = ((a, b) => { });
        #endregion Test Variables

        #region Refresh Test Variables

        Mock<IEnvironmentModel> _reMockEnvironmentModel;
        Mock<IEnvironmentModel> _reMockEnvironmentModel1;
        Mock<IResourceRepository> _reMockResourceRepository;

        #endregion

        #region Test Context

        /// <summary>
        ///     Gets or sets the result context which provides
        ///     information about and functionality for the current result run.
        /// </summary>
        public TestContext TestContext { get; set; }

        #endregion

        #region Test Setup and Cleanup

        [TestInitialize]
        public void TestInitialize()
        {
            AppSettings.LocalHost = "http://localhost:3142";
            _mockEnvironmentModel = null;
            _mockResourceModel = null;
            _mockResourceModel1 = null;
            _mockResourceModel2 = null;
            _mockResourceSourceModel = null;
        }

        [TestCleanup]
        public void TestCleanUp()
        {
            _mockEnvironmentModel = null;
            _mockResourceModel = null;
            _mockResourceModel1 = null;
            _mockResourceModel2 = null;
            _mockResourceSourceModel = null;
        }

        #endregion

        #region Updating Resources

        // ReSharper disable UnusedMember.Local
        NavigationViewModel Init(Mock<IConnectControlSingleton> connectControlASingleton)
        // ReSharper restore UnusedMember.Local
        {

            _mockEnvironmentModel = new Mock<IEnvironmentModel>();
            _mockEnvironmentModel.SetupGet(x => x.Connection.AppServerUri).Returns(new Uri("http://127.0.0.1/"));
            _mockEnvironmentModel.SetupGet(x => x.Connection.AppServerUri).Returns(new Uri("http://127.0.0.1/"));
            _mockEnvironmentModel.SetupGet(x => x.IsConnected).Returns(true);

            // setup env repo
            var repo = new Mock<IEnvironmentRepository>();
            repo.Setup(l => l.Load()).Verifiable();

            IList<IEnvironmentModel> models = new List<IEnvironmentModel>();
            repo.Setup(l => l.All()).Returns(models);

            _mockResourceModel = new Mock<IContextualResourceModel>();
            _mockResourceModel.Setup(r => r.ResourceType).Returns(ResourceType.WorkflowService);
            _mockResourceModel.Setup(r => r.Category).Returns("Testing");
            _mockResourceModel.Setup(r => r.ResourceName).Returns("Mock");
            _mockResourceModel.Setup(r => r.Environment).Returns(_mockEnvironmentModel.Object);

            _mockResourceModel1 = new Mock<IContextualResourceModel>();
            _mockResourceModel1.Setup(r => r.ResourceType).Returns(ResourceType.WorkflowService);
            _mockResourceModel1.Setup(r => r.Category).Returns("Testing2");
            _mockResourceModel1.Setup(r => r.ResourceName).Returns("Mock1");
            _mockResourceModel1.Setup(r => r.Environment).Returns(_mockEnvironmentModel.Object);

            _mockResourceModel2 = new Mock<IContextualResourceModel>();
            _mockResourceModel2.Setup(r => r.ResourceType).Returns(ResourceType.Service);
            _mockResourceModel2.Setup(r => r.Category).Returns("Testing2");
            _mockResourceModel2.Setup(r => r.ResourceName).Returns("Mock2");
            _mockResourceModel2.Setup(r => r.Environment).Returns(_mockEnvironmentModel.Object);

            var mockResourceRepository = new Mock<IResourceRepository>();
            mockResourceRepository.Setup(r => r.All()).Returns(
                new Collection<IResourceModel>
                {
                    _mockResourceModel.Object,
                    _mockResourceModel1.Object,
                    _mockResourceModel2.Object
                });

            _mockEnvironmentModel.SetupGet(x => x.ResourceRepository).Returns(mockResourceRepository.Object);
            _mockEnvironmentModel.Setup(x => x.LoadResources());

            var viewModel = CreateViewModel(GetEnvironmentRepository(_mockEnvironmentModel), mockResourceRepository, connectControlASingleton: connectControlASingleton);
            viewModel.AddEnvironment(_mockEnvironmentModel.Object);
            return viewModel;
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("NavigationViewModel_UpdateWorkspaces")]
        public void NavigationViewModel_UpdateWorkspaces_WhenSomeFoldersAreExpanded_FoldersAreStillExpandedAfter()
        {
            //Arrange
            Mock<IEnvironmentModel> mockEnvironment = EnviromentRepositoryTest.CreateMockEnvironment();
            mockEnvironment.Setup(model => model.CanStudioExecute).Returns(true);
            mockEnvironment.Setup(model => model.ID).Returns(Guid.Empty);
            var environmentRepository = GetEnvironmentRepository(mockEnvironment);
            const string displayName = "localhost (https://localhost:3242/)";

            ObservableCollection<IExplorerItemModel> explorerItemModels = new ObservableCollection<IExplorerItemModel>();
            ExplorerItemModel server = new ExplorerItemModel { ResourceType = Common.Interfaces.Data.ResourceType.Server, DisplayName = displayName, IsExplorerExpanded = true, EnvironmentId = Guid.Empty };
            ExplorerItemModel folder1 = new ExplorerItemModel { ResourceType = Common.Interfaces.Data.ResourceType.Folder, DisplayName = "Folder1", ResourcePath = "Folder1", IsExplorerExpanded = true, EnvironmentId = Guid.Empty };
            ExplorerItemModel resource1 = new ExplorerItemModel { ResourceType = Common.Interfaces.Data.ResourceType.WorkflowService, DisplayName = "Resource1", ResourcePath = "Resource1", IsExplorerExpanded = false, EnvironmentId = Guid.Empty };
            ExplorerItemModel resource2 = new ExplorerItemModel { ResourceType = Common.Interfaces.Data.ResourceType.WorkflowService, DisplayName = "Resource2", ResourcePath = "Resource2", IsExplorerExpanded = false, IsExplorerSelected = true, EnvironmentId = Guid.Empty };
            ExplorerItemModel folder2 = new ExplorerItemModel { ResourceType = Common.Interfaces.Data.ResourceType.Folder, DisplayName = "Folder2", ResourcePath = "Folder2", IsExplorerExpanded = false, EnvironmentId = Guid.Empty };

            folder1.Children.Add(resource1);
            folder1.Children.Add(resource2);
            server.Children.Add(folder1);
            server.Children.Add(folder2);
            explorerItemModels.Add(server);

            var mockStudioRepo = new Mock<IStudioResourceRepository>();
            mockStudioRepo.SetupGet(p => p.ExplorerItemModels).Returns(explorerItemModels);

            var viewmodel = new NavigationViewModel(new Mock<IEventAggregator>().Object, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object, null, environmentRepository, mockStudioRepo.Object, new Mock<IConnectControlSingleton>().Object, () => { }) { SelectedItem = resource2 };

            viewmodel.Environments.Add(mockEnvironment.Object);
            viewmodel.ExplorerItemModels = explorerItemModels;
            var connectControlSingletonMock = new Mock<IConnectControlSingleton>();
            //Act
            viewmodel.UpdateWorkspaces(connectControlSingletonMock.Object);
            //Assert
            Assert.IsNotNull(viewmodel.ExplorerItemModels);
            Assert.AreEqual(1, viewmodel.ExplorerItemModels.Count);
            Assert.AreEqual(displayName, viewmodel.ExplorerItemModels[0].DisplayName);
            Assert.AreEqual(true, viewmodel.ExplorerItemModels[0].Children[0].IsExplorerExpanded);
            Assert.AreEqual(false, viewmodel.ExplorerItemModels[0].Children[1].IsExplorerExpanded);
            Assert.AreEqual(false, viewmodel.ExplorerItemModels[0].Children[0].Children[0].IsExplorerSelected);
            Assert.AreEqual(true, viewmodel.ExplorerItemModels[0].Children[0].Children[1].IsExplorerSelected);
        }

        private static IEnvironmentRepository GetEnvironmentRepository(Mock<IEnvironmentModel> mockEnvironment)
        {
            var repo = new TestLoadEnvironmentRespository(mockEnvironment.Object) { IsLoaded = true };
            return repo;
        }


        [TestMethod]
        public void AddEnvironmentWithShouldLoadResourcesTrueExpectedRootAndChildrenCreated()
        {
            var viewModel = Init(false, true);
            var explorerItemModels = viewModel.ExplorerItemModels;
            Assert.IsNotNull(explorerItemModels);
            Assert.AreEqual(1, explorerItemModels.Count);
            Assert.AreEqual(2, explorerItemModels[0].Children.Count);
        }


        #endregion Updating Resources

        #region Filtering

        [TestMethod]
        public void FilteredNavigationViewModel_AfterReload_Expects_FilteredItemsInTreeWithIsFilteredTrue()
        {
            var viewModel = Init(false, true,false);
            var resourceVM = viewModel.FindChild(_mockResourceModel.Object);

            Assert.IsNotNull(resourceVM);

            Assert.AreEqual(4, viewModel.ExplorerItemModels[0].ChildrenCount);

            viewModel.UpdateSearchFilter("Mock2");
            resourceVM = viewModel.FindChild(_mockResourceModel.Object);
            Assert.AreEqual(1, viewModel.ExplorerItemModels[0].ChildrenCount);
            Assert.IsNull(resourceVM);

            resourceVM = viewModel.FindChild(_mockResourceModel.Object);

            Assert.AreEqual(1, viewModel.ExplorerItemModels[0].ChildrenCount);
            Assert.IsNull(resourceVM);
            viewModel.UpdateSearchFilter("Mock");
            resourceVM = viewModel.FindChild(_mockResourceModel.Object);
            Assert.AreEqual(4, viewModel.ExplorerItemModels[0].ChildrenCount);
            Assert.IsNotNull(resourceVM);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("NavigationViewModel_Filter")]
        public void NavigationViewModel_Filter_GivenFunction_ShouldFilter()
        {
            //------------Setup for test--------------------------
            var viewModel = Init(false, true);
            //------------Preconditions---------------------------
            Assert.AreEqual(4, viewModel.ExplorerItemModels[0].ChildrenCount);
            //------------Execute Test---------------------------
            viewModel.Filter(model => model.ResourceType == Common.Interfaces.Data.ResourceType.WorkflowService);
            //------------Assert Results-------------------------
            Assert.AreEqual(2, viewModel.ExplorerItemModels[0].ChildrenCount);
            Assert.IsTrue(HasFolder(viewModel.ExplorerItemModels[0].Children));
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("NavigationViewModel_Filter")]
        public void NavigationViewModel_Filter_GivenFunctionAndActivityTypeIsWorkflow_ShouldFilter()
        {
            //------------Setup for test--------------------------
            var viewModel = Init(false, true);
            viewModel.DsfActivityType = enDsfActivityType.Workflow;
            //------------Preconditions---------------------------
            Assert.AreEqual(4, viewModel.ExplorerItemModels[0].ChildrenCount);
            //------------Execute Test---------------------------
            viewModel.Filter(model => model.ResourceType == Common.Interfaces.Data.ResourceType.WorkflowService);
            //------------Assert Results-------------------------
            Assert.IsTrue(HasFolder(viewModel.ExplorerItemModels[0].Children));
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("NavigationViewModel_Filter")]
        public void NavigationViewModel_Filter_GivenFunctionAndActivityTypeIsWorkflow_ShouldFilterIfUsingDialogFilter()
        {
            //------------Setup for test--------------------------
            var viewModel = Init(false, true);
            viewModel.DsfActivityType = enDsfActivityType.Workflow;
            //------------Preconditions---------------------------
            Assert.AreEqual(4, viewModel.ExplorerItemModels[0].ChildrenCount);
            //------------Execute Test---------------------------
            viewModel.Filter(model => model.ResourceType == Common.Interfaces.Data.ResourceType.WorkflowService || model.ResourceType == Common.Interfaces.Data.ResourceType.Folder,false,true);
            //------------Assert Results-------------------------
            Assert.IsTrue(HasFolder(viewModel.ExplorerItemModels[0].Children));
        }


        private bool HasFolder(IEnumerable<IExplorerItemModel> children)
        {
            if(children.Any(explorerItemModel => explorerItemModel.ResourceType == Common.Interfaces.Data.ResourceType.Folder))
            {
                return true;
            }

            return false;
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("NavigationViewModel_Filter")]
        public void NavigationViewModel_Filter_GivenNull_ShouldOriginalCollection()
        {
            //------------Setup for test--------------------------
            var viewModel = Init(false, true,false);
            //------------Preconditions---------------------------
            Assert.AreEqual(4, viewModel.ExplorerItemModels[0].ChildrenCount);
            viewModel.Filter(model => model.ResourceType == Common.Interfaces.Data.ResourceType.WorkflowService);
            Assert.AreEqual(2, viewModel.ExplorerItemModels[0].ChildrenCount);
            //------------Execute Test---------------------------
            viewModel.Filter(null);
            //------------Assert Results-------------------------
            Assert.AreEqual(4, viewModel.ExplorerItemModels[0].ChildrenCount);
        }

        [TestMethod]
        public void FilteredNavigationViewModel_WhereFilterReset_Expects_OriginalExpandState()
        {
            IExplorerItemModel resourceVM = null;

            var reset = new AutoResetEvent(false);
            ThreadExecuter.RunCodeAsSTA(reset,
                () =>
                {
                    var viewModel = Init(false, true);
                    resourceVM = viewModel.FindChild(_mockResourceModel.Object);
                    IExplorerItemModel resourceVM2_1 = viewModel.FindChild(_mockResourceModel1.Object);
                    IExplorerItemModel resourceVM2_2 = viewModel.FindChild(_mockResourceModel2.Object);

                    Assert.IsTrue(resourceVM.Parent.IsExplorerExpanded == false);
                    Assert.IsTrue(resourceVM2_1.Parent.IsExplorerExpanded == false);
                    Assert.IsTrue(resourceVM2_2.Parent.IsExplorerExpanded == false);

                    viewModel.UpdateSearchFilter("Mock2");

                    viewModel.UpdateSearchFilter("");
                });
            reset.WaitOne();

            Assert.IsTrue(resourceVM.Parent.IsExplorerExpanded == false);
        }

        #endregion Filtering

        #region Rename

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("NavigationViewModel_RenameCommand")]
        public void NavigationViewModel_RenameCommand_RenameKeyPressed_RenameCommandCalledOnSelectedItem()
        {
            //------------Setup for test--------------------------
            Mock<IResourceRepository> mockResourceRepository;
            SetupWithOutViewModel(false, true, out mockResourceRepository);
            var viewModel = CreateViewModel(GetEnvironmentRepository(_mockEnvironmentModel), mockResourceRepository, true, enDsfActivityType.Workflow);
            viewModel.SelectedItem = new ExplorerItemModel { DisplayName = "test1" };
            //------------Execute Test---------------------------
            Assert.IsFalse(viewModel.SelectedItem.IsRenaming);
            viewModel.RenameCommand.Execute(null);
            //------------Assert Results-------------------------
            Assert.IsTrue(viewModel.SelectedItem.IsRenaming);
        }

        #endregion

        #region Remove

        [TestMethod]
        public void RemoveEnvironmentsWhenEnvironmentConnected()
        {
            var viewModel = Init(false, true);
            _mockEnvironmentModel.SetupGet(c => c.IsConnected).Returns(true);
            _mockEnvironmentModel.SetupGet(c => c.Name).Returns("Mock");

            Assert.IsTrue(viewModel.Environments.Count == 1);
            Assert.IsTrue(viewModel.ExplorerItemModels[0].ChildrenCount > 0);

            viewModel.RemoveEnvironment(_mockEnvironmentModel.Object);
            Assert.IsTrue(viewModel.Environments.Count == 0);
            Assert.IsTrue(viewModel.ExplorerItemModels.Count == 0);

        }

        [TestMethod]
        public void NoExceptionWhenEnvironmentNotInEnvironmentList()
        {
            var reset = new AutoResetEvent(false);
            var viewModel = Init(false, true);
            ThreadExecuter.RunCodeAsSTA(reset,
                () =>
                {
                    _mockEnvironmentModel.SetupGet(c => c.IsConnected).Returns(true);
                    _mockEnvironmentModel.SetupGet(c => c.Name).Returns("Mock");

                    Assert.IsTrue(viewModel.Environments.Count == 1);
                    Assert.IsTrue(viewModel.ExplorerItemModels[0].ChildrenCount > 0);

                    var nonAddedMock = GetMockEnvironment();
                    nonAddedMock.Setup(m => m.ID).Returns(Guid.NewGuid);

                    viewModel.RemoveEnvironment(nonAddedMock.Object);

                });
            reset.WaitOne();

            Assert.IsTrue(viewModel.Environments.Count == 1);
            Assert.IsTrue(viewModel.ExplorerItemModels[0].ChildrenCount > 0);

        }

        #endregion remove

        #region Refresh Environments Tests

        NavigationViewModel RefreshTestsSetup()
        {
            var firstEnvId = Guid.NewGuid();
            _reMockEnvironmentModel = GetMockEnvironment();
            _reMockEnvironmentModel.Setup(x => x.ID).Returns(firstEnvId);
            _reMockEnvironmentModel.Setup(x => x.IsConnected).Returns(true);

            var secondEnvId = Guid.NewGuid();
            _reMockEnvironmentModel1 = GetMockEnvironment();
            _reMockEnvironmentModel1.Setup(x => x.ID).Returns(secondEnvId);
            _reMockEnvironmentModel1.Setup(x => x.IsConnected).Returns(false);

            // setup env repo
            var repo = new Mock<IEnvironmentRepository>();
            repo.Setup(l => l.Load()).Verifiable();

            IList<IEnvironmentModel> models = new List<IEnvironmentModel>();
            repo.Setup(l => l.All()).Returns(models);

            _mockResourceModel = new Mock<IContextualResourceModel>();
            _mockResourceModel.Setup(r => r.ResourceType).Returns(ResourceType.WorkflowService);
            _mockResourceModel.Setup(r => r.Category).Returns("Testing");
            _mockResourceModel.Setup(r => r.ResourceName).Returns("Mock");
            _mockResourceModel.Setup(r => r.Environment).Returns(_reMockEnvironmentModel.Object);

            _mockResourceModel1 = new Mock<IContextualResourceModel>();
            _mockResourceModel1.Setup(r => r.ResourceType).Returns(ResourceType.WorkflowService);
            _mockResourceModel1.Setup(r => r.Category).Returns("Testing2");
            _mockResourceModel1.Setup(r => r.ResourceName).Returns("Mock1");
            _mockResourceModel1.Setup(r => r.Environment).Returns(_reMockEnvironmentModel.Object);

            _mockResourceModel2 = new Mock<IContextualResourceModel>();
            _mockResourceModel2.Setup(r => r.ResourceType).Returns(ResourceType.Service);
            _mockResourceModel2.Setup(r => r.Category).Returns("Testing2");
            _mockResourceModel2.Setup(r => r.ResourceName).Returns("Mock2");
            _mockResourceModel2.Setup(r => r.Environment).Returns(_reMockEnvironmentModel.Object);

            _reMockResourceRepository = new Mock<IResourceRepository>();
            _reMockResourceRepository.Setup(r => r.All()).Returns(
                new Collection<IResourceModel>
                {
                    _mockResourceModel.Object,
                    _mockResourceModel1.Object,
                    _mockResourceModel2.Object
                });
            _reMockResourceRepository.Setup(x => x.UpdateWorkspace(It.IsAny<IList<IWorkspaceItem>>())).Verifiable();

            _reMockEnvironmentModel.SetupGet(x => x.ResourceRepository).Returns(_reMockResourceRepository.Object);
            _reMockEnvironmentModel.Setup(x => x.LoadResources());

            _reMockEnvironmentModel1.SetupGet(x => x.ResourceRepository).Returns(_reMockResourceRepository.Object);
            _reMockEnvironmentModel1.Setup(x => x.LoadResources());

            var viewModel = CreateViewModel(GetEnvironmentRepository(_reMockEnvironmentModel), _reMockResourceRepository);
            viewModel.AddEnvironment(_reMockEnvironmentModel.Object);
            viewModel.AddEnvironment(_reMockEnvironmentModel1.Object);
            return viewModel;
        }

        [TestMethod]
        [TestCategory("NavigationViewModel")]
        [Description("Refresh all environments connects to localhost")]
        [Owner("Ashley Lewis")]
        public void NavigationViewModel_UpdateWorkspaces_DisconnectedLocalhostServer_LocalhostServerConnected()
        {
            //isolate UpdateWorkspaces as a functional unit
            var viewModel = RefreshTestsSetup();
            _reMockEnvironmentModel.Setup(c => c.CanStudioExecute).Returns(true);

            //run unit
            var connectControlSingletonMock = new Mock<IConnectControlSingleton>();
            viewModel.UpdateWorkspaces(connectControlSingletonMock.Object);

            //assert both localhost servers are connected
            Assert.IsTrue(viewModel.ExplorerItemModels[0].Children[0].IsConnected, "Refresh does not autoconnect to localhost");
            Assert.IsTrue(viewModel.ExplorerItemModels[0].Children[1].IsConnected, "Refresh does not autoconnect to localhost");
        }

        #endregion

        #region Activity Drop From ToolBox Tests

        [TestMethod]
        public void CreateNavigationViewModelWithIsActivityDropTrueAndTypeWorkflowExpectedOnlyWorkflowsToBeInTree()
        {
            Mock<IResourceRepository> mockResourceRepository;
            SetupWithOutViewModel(false, true, out mockResourceRepository);
            var viewModel = CreateViewModel(GetEnvironmentRepository(_mockEnvironmentModel), mockResourceRepository, true, enDsfActivityType.Workflow);
            viewModel.AddEnvironment(_mockEnvironmentModel.Object);
            Assert.AreEqual(1, viewModel.ExplorerItemModels[0].Children[0].Children.Count);
            Assert.AreEqual("Mock", viewModel.ExplorerItemModels[0].Children[0].Children[0].DisplayName);
            Assert.AreEqual(Common.Interfaces.Data.ResourceType.WorkflowService, viewModel.ExplorerItemModels[0].Children[0].Children[0].ResourceType);
        }

        [TestMethod]
        public void CreateNavigationViewModelWithIsActivityDropTrueAndTypexServiceExpectedOnlyServicesToBeInTree()
        {
            var x = Init(false, true);
            x = null;
            Mock<IResourceRepository> mockResourceRepository;
            SetupWithOutViewModel(false, true, out mockResourceRepository);
            var viewModel = CreateViewModel(GetEnvironmentRepository(_mockEnvironmentModel), mockResourceRepository, true, enDsfActivityType.Workflow);
            viewModel.AddEnvironment(_mockEnvironmentModel.Object);
            Assert.AreEqual(1, viewModel.ExplorerItemModels[0].Children[0].Children.Count);
            //Assert.AreEqual("Mock2", viewModel.ExplorerItemModels[0].Children[0].Children[0].DisplayName);
            //Assert.AreEqual(Common.Interfaces.Data.ResourceType.DbService, viewModel.ExplorerItemModels[0].Children[0].Children[0].ResourceType);
        }

        void SetupWithOutViewModel(bool addWizardChildToResource, bool shouldLoadResources, out Mock<IResourceRepository> mockResourceRepository)
        {
            SetupMockEnvironment(shouldLoadResources);
            SetUpResources(addWizardChildToResource, out mockResourceRepository);
            _mockEnvironmentModel.Setup(model => model.IsLocalHost).Returns(true);
        }

        [TestMethod]
        public void CreateNavigationViewModelWithIsActivityDropTrueAndTypeSourceExpectedOnlySourcesToBeInTree()
        {
            Mock<IResourceRepository> mockResourceRepository;
            SetupWithOutViewModel(false, true, out mockResourceRepository);
            var viewModel = CreateViewModel(GetEnvironmentRepository(_mockEnvironmentModel), mockResourceRepository, true, enDsfActivityType.Source);
            viewModel.AddEnvironment(_mockEnvironmentModel.Object);
            Assert.AreEqual(1, viewModel.ExplorerItemModels[0].Children[0].Children.Count);
            Assert.AreEqual("MockSource", viewModel.ExplorerItemModels[0].Children[0].Children[0].DisplayName);
            Assert.AreEqual(Common.Interfaces.Data.ResourceType.WebSource, viewModel.ExplorerItemModels[0].Children[0].Children[0].ResourceType);
        }

        [TestMethod]
        public void CreateNavigationViewModelWithIsActivityDropTrueAndTypeAllExpectedAllItemsToBeInTree()
        {
            var viewModel = Init(false, true);
            Assert.AreEqual(1, viewModel.ExplorerItemModels[0].Children[0].Children.Count);
            Assert.AreEqual(3, viewModel.ExplorerItemModels[0].Children[1].Children.Count);
        }

        #endregion



        [TestMethod]
        [TestCategory("NavigationViewModel_Constructor")]
        [Description("Constructor must not allow null AsyncWorker.")]
        [Owner("Trevor Williams-Ros")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NavigationViewModel_UnitTest_ConstructorWithNullAsyncWorker_ThrowsArgumentNullException()
        {
            var eventPublisher = new Mock<IEventAggregator>();

            // ReSharper disable ObjectCreationAsStatement
            new NavigationViewModel(eventPublisher.Object, null, null, null, new Mock<IStudioResourceRepository>().Object, new Mock<IConnectControlSingleton>().Object, () => { });
            // ReSharper restore ObjectCreationAsStatement
        }

        [TestMethod]
        public void NavigationViewModel_BringItemIntoView_Expects_SetTreeNodeIsSelectedTrueParentOpen()
        {
            var viewModel = Init(false, true);
            var resourceVm = viewModel.FindChild(_mockResourceModel.Object);
            viewModel.BringItemIntoView(_mockResourceModel.Object);
            Assert.IsNotNull(resourceVm);
            Assert.IsTrue(resourceVm.IsExplorerSelected);
            Assert.IsTrue(resourceVm.Parent.IsExplorerExpanded);
        }

        [TestMethod]
        public void NavigationViewModel_BringItemIntoView_NotCorrectEnvironment_DoesNotSetTreeNodeIsSelectedTrueParentOpen()
        {
            var viewModel = Init(false, true);
            var mockEnvironment2 = GetMockEnvironment();
            mockEnvironment2.Setup(model => model.ID).Returns(Guid.NewGuid());
            viewModel.AddEnvironment(mockEnvironment2.Object);
            _mockResourceModel2.Setup(model => model.ID).Returns(_mockResourceModel.Object.ID);
            _mockResourceModel2.Setup(model => model.Environment.ID).Returns(mockEnvironment2.Object.ID);
            var resourceVm = viewModel.FindChild(_mockResourceModel2.Object);
            viewModel.BringItemIntoView(_mockResourceModel2.Object);
            Assert.IsNull(resourceVm);
        }


        [TestMethod]
        public void NavigationViewModel_BringItemIntoViewWithNull_Expects_DoesNothing()
        {
            var viewModel = Init(false, true);
            var resourceVm = viewModel.FindChild(_mockResourceModel.Object);
            viewModel.BringItemIntoView(null);
            Assert.IsNotNull(resourceVm);
            Assert.IsFalse(resourceVm.IsExplorerSelected);
            Assert.IsFalse(resourceVm.Parent.IsExplorerExpanded);
        }

        [TestMethod]
        [TestCategory("NavigationViewModel_AddEnvironment")]
        [Description("NavigationViewModel AddEnvironment starts localhost environment connection's auto-connect heartbeat if not connected.")]
        [Owner("Trevor Williams-Ros")]
        public void NavigationViewModel_UnitTest_AddEnvironmentWithDisconnectedLocalhost_InitiatesAutoConnect()
        {
            var localhostConnection = new Mock<IEnvironmentConnection>();
            localhostConnection.Setup(e => e.StartAutoConnect()).Verifiable();
            localhostConnection.Setup(e => e.WebServerUri).Returns(new Uri("https://localhost:3142/"));

            var localhost = new Mock<IEnvironmentModel>();
            localhost.Setup(e => e.ID).Returns(Guid.Empty);
            localhost.Setup(e => e.Name).Returns("localhost");
            localhost.Setup(e => e.IsConnected).Returns(false);
            localhost.Setup(e => e.CanStudioExecute).Returns(false);
            localhost.Setup(e => e.Connection).Returns(localhostConnection.Object);
            localhost.Setup(m => m.Equals(It.IsAny<IEnvironmentModel>())).Returns(true);

            var envRepo = new Mock<IEnvironmentRepository>();
            envRepo.Setup(e => e.All()).Returns(new List<IEnvironmentModel>());
            envRepo.Setup(e => e.Source).Returns(localhost.Object);


            var viewModel = CreateViewModel(envRepo.Object, new Mock<IResourceRepository>());
            viewModel.AddEnvironment(localhost.Object);

            localhostConnection.Verify(e => e.StartAutoConnect(), "AddEnvironment did not start localhost auto-connect heartbeat.");
        }


        [TestMethod]
        [TestCategory("NavigationViewModel_AddEnvironment")]
        [Description("NavigationViewModel AddEnvironment does not start non-localhost environment connection's auto-connect heartbeat if not connected.")]
        [Owner("Trevor Williams-Ros")]
        public void NavigationViewModel_UnitTest_AddEnvironmentWithDisconnectedNonLocalhost_DoesNotInitiateAutoConnect()
        {
            var localhost = new Mock<IEnvironmentModel>();
            localhost.Setup(e => e.ID).Returns(Guid.Empty);

            var envRepo = new Mock<IEnvironmentRepository>();
            envRepo.Setup(e => e.All()).Returns(new List<IEnvironmentModel>());
            envRepo.Setup(e => e.Source).Returns(localhost.Object);


            var viewModel = CreateViewModel(envRepo.Object, new Mock<IResourceRepository>());

            var otherConnection = new Mock<IEnvironmentConnection>();
            otherConnection.Setup(e => e.StartAutoConnect()).Verifiable();

            var other = new Mock<IEnvironmentModel>();
            other.Setup(e => e.ID).Returns(Guid.NewGuid());
            other.Setup(e => e.IsConnected).Returns(false);
            other.Setup(e => e.CanStudioExecute).Returns(false);
            other.Setup(e => e.Connection).Returns(otherConnection.Object);

            viewModel.AddEnvironment(other.Object);

            otherConnection.Verify(e => e.StartAutoConnect(), Times.Never(), "AddEnvironment started non-localhost auto-connect heartbeat.");
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("NavigationViewModel_Remove")]
        public void NavigationViewModel_RemoveEnvironment_InvokesSelectLocalhost()
        {
            //------------Setup for test--------------------------
            var publisher = new Mock<IEventAggregator>();
            publisher.Setup(p => p.Publish(It.IsAny<SetActiveEnvironmentMessage>())).Verifiable();

            var localhost = new Mock<IEnvironmentModel>();
            localhost.Setup(e => e.ID).Returns(Guid.Empty);
            localhost.Setup(e => e.Name).Returns("localhost");
            localhost.Setup(e => e.IsLocalHost).Returns(true);
            localhost.Setup(e => e.IsConnected).Returns(true);
            localhost.Setup(e => e.CanStudioExecute).Returns(true);
            localhost.SetupGet(x => x.Connection.AppServerUri).Returns(new Uri("http://127.0.0.1/"));

            localhost.Setup(e => e.CanStudioExecute).Returns(true);

            var toBeRemoved = new Mock<IEnvironmentModel>();
            toBeRemoved.Setup(e => e.ID).Returns(Guid.NewGuid());
            toBeRemoved.Setup(e => e.Name).Returns("Other Server");
            toBeRemoved.Setup(e => e.CanStudioExecute).Returns(true);
            toBeRemoved.SetupGet(x => x.Connection.AppServerUri).Returns(new Uri("http://127.0.0.2/"));

            var envList = new List<IEnvironmentModel> { localhost.Object, toBeRemoved.Object };
            var envRepo = new Mock<IEnvironmentRepository>();
            envRepo.Setup(e => e.All()).Returns(envList);
            envRepo.Setup(e => e.Source).Returns(localhost.Object);

            var localhostExplorerItemModel = new ExplorerItemModel { EnvironmentId = Guid.Empty, DisplayName = "localhost" };

            ExplorerItemModel anotherEnvironment = new ExplorerItemModel { DisplayName = "Other Server", EnvironmentId = toBeRemoved.Object.ID };
            var studioResourceRepository = new StudioResourceRepository(localhostExplorerItemModel, _Invoke);
            studioResourceRepository.ExplorerItemModels.Add(anotherEnvironment);
            studioResourceRepository.GetExplorerProxy = guid => new Mock<IExplorerResourceRepository>().Object;
            var viewModel = new NavigationViewModel(publisher.Object, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object, null, envRepo.Object, studioResourceRepository, new Mock<IConnectControlSingleton>().Object, () => { });


            foreach(var env in envList)
            {
                viewModel.AddEnvironment(env);
            }

            //------------Execute Test---------------------------
            viewModel.RemoveEnvironment(toBeRemoved.Object);

            //------------Assert Results-------------------------
            publisher.Verify(p => p.Publish(It.IsAny<SetActiveEnvironmentMessage>()));
            Assert.IsTrue(viewModel.ExplorerItemModels[0].IsExplorerSelected);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("NavigationViewModel_RefreshMenuCommand")]
        public void NavigationViewModel_RefreshMenuCommand_CallsSetConnectionTwiceForBusyStatusAndConnectedStatus()
        {
            //------------Setup for test--------------------------
            var hitSetConnectionStateCount = 0;
            var connectControlSingleton = new Mock<IConnectControlSingleton>();
            var expectedStates = new List<ConnectionEnumerations.ConnectedState>
                {
                    ConnectionEnumerations.ConnectedState.Busy,
                    ConnectionEnumerations.ConnectedState.Connected
                };
            connectControlSingleton.Setup(c => c.SetConnectionState(It.IsAny<Guid>(), It.IsAny<ConnectionEnumerations.ConnectedState>()))
                .Callback<Guid, ConnectionEnumerations.ConnectedState>((env, state) => Assert.AreEqual(expectedStates[hitSetConnectionStateCount++], state, "Unexpected connection state change."))
                .Verifiable();
            var vm = Init(connectControlSingleton);

            //------------Execute Test---------------------------
            vm.RefreshMenuCommand.Execute(null);

            //------------Assert Results-------------------------
            Thread.Sleep(100);
            Assert.AreEqual(2, hitSetConnectionStateCount, "Not all expected connection state changes where executed.");
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("NavigationViewModel_UpdateSearchFilter")]
        public void NavigationViewModel_UpdateSearchFilter_ResourceDoesNotExist_ExplorerModelCountIsZero()
        {
            //------------Setup for test--------------------------
            var connectControlSingleton = new Mock<IConnectControlSingleton>();
            var vm = Init(connectControlSingleton);
            var explorerBeforeCount = vm.ExplorerItemModels.Count;
            vm.DsfActivityType = enDsfActivityType.All;
            //------------Execute Test---------------------------
            vm.UpdateSearchFilter("Testing1");
            //------------Assert Results-------------------------
            Assert.AreEqual(1, explorerBeforeCount);
            Assert.AreEqual(0, vm.ExplorerItemModels.Count);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("NavigationViewModel_UpdateSearchFilter")]
        public void NavigationViewModel_UpdateSearchFilter_ResourceDoesExist_ExplorerModelCountItems()
        {
            //------------Setup for test--------------------------
            var connectControlSingleton = new Mock<IConnectControlSingleton>();
            var vm = Init(connectControlSingleton);
            var explorerBeforeCount = vm.ExplorerItemModels.Count;
            vm.DsfActivityType = enDsfActivityType.All;
            //------------Execute Test---------------------------
            vm.UpdateSearchFilter("Testing2");
            //------------Assert Results-------------------------
            Assert.AreEqual(1, explorerBeforeCount);
            Assert.AreEqual(1, vm.ExplorerItemModels.Count);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("NavigationViewModel_UpdateSearchFilter")]
        public void NavigationViewModel_UpdateSearchFilter_ResourceDoesExistWhenActivityTypeIsWorkFlows_ExplorerModelCountItems()
        {
            //------------Setup for test--------------------------
            var connectControlSingleton = new Mock<IConnectControlSingleton>();
            var vm = Init(connectControlSingleton);
            var explorerBeforeCount = vm.ExplorerItemModels.Count;
            vm.DsfActivityType = enDsfActivityType.Workflow;
            //------------Execute Test---------------------------
            vm.UpdateSearchFilter("Testing2");
            //------------Assert Results-------------------------
            Assert.AreEqual(1, explorerBeforeCount);
            Assert.AreEqual(1, vm.ExplorerItemModels.Count);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("NavigationViewModel_UpdateSearchFilter")]
        public void NavigationViewModel_UpdateSearchFilter_ResourceDoesExistWhenActivityTypeIsSource_ExplorerModelCountIsZero()
        {
            //------------Setup for test--------------------------
            var connectControlSingleton = new Mock<IConnectControlSingleton>();
            var vm = Init(connectControlSingleton);
            var explorerBeforeCount = vm.ExplorerItemModels.Count;
            vm.DsfActivityType = enDsfActivityType.Source;
            //------------Execute Test---------------------------
            vm.UpdateSearchFilter("Testing2");
            //------------Assert Results-------------------------
            Assert.AreEqual(1, explorerBeforeCount);
            Assert.AreEqual(0, vm.ExplorerItemModels.Count);
        }


        #region Private Test Methods

        NavigationViewModel Init(bool addWizardChildToResource, bool shouldLoadResources, bool mockClone = true)
        {
            SetupMockEnvironment(shouldLoadResources);

            Mock<IResourceRepository> mockResourceRepository;
            SetUpResources(addWizardChildToResource, out mockResourceRepository);


            var viewModel = CreateViewModel(GetEnvironmentRepository(_mockEnvironmentModel), mockResourceRepository,false,enDsfActivityType.All,null,mockClone);
            _mockEnvironmentModel.Setup(model => model.IsLocalHost).Returns(true);

            viewModel.AddEnvironment(_mockEnvironmentModel.Object);
            return viewModel;
        }

        IStudioResourceRepository BuildExplorerItems(IResourceRepository resourceRepository,bool mockClone = true)
        {
            var resourceModels = resourceRepository.All();
            var localhostItemModel = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object) { DisplayName = "localhost", EnvironmentId = Guid.Empty, ResourceType = Common.Interfaces.Data.ResourceType.Server };

            if(resourceModels != null)
            {
                foreach(var resourceModel in resourceModels)
                {
                    var resourceItemModel = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object) { ResourceId = resourceModel.ID, ResourcePath = resourceModel.Category, EnvironmentId = Guid.Empty, DisplayName = resourceModel.ResourceName };
                    Common.Interfaces.Data.ResourceType correctTyping = Common.Interfaces.Data.ResourceType.WorkflowService;
                    switch(resourceModel.ResourceType)
                    {
                        case ResourceType.WorkflowService:
                            correctTyping = Common.Interfaces.Data.ResourceType.WorkflowService;
                            break;
                        case ResourceType.Service:
                            correctTyping = Common.Interfaces.Data.ResourceType.DbService;
                            break;
                        case ResourceType.Source:
                            correctTyping = Common.Interfaces.Data.ResourceType.WebSource;
                            break;
                    }
                    resourceItemModel.ResourceType = correctTyping;

                    var categoryItem = localhostItemModel.Children.FirstOrDefault(model => model.DisplayName == resourceModel.Category);
                    if(categoryItem == null)
                    {
                        categoryItem = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object) { Parent = localhostItemModel, DisplayName = resourceModel.Category, EnvironmentId = Guid.Empty, ResourceType = Common.Interfaces.Data.ResourceType.Folder, ResourcePath = "" };
                        localhostItemModel.Children.Add(categoryItem);
                    }
                    resourceItemModel.Parent = categoryItem;
                    categoryItem.Children.Add(resourceItemModel);
                }
            }
            var studioResourceRepository = new StudioResourceRepository(localhostItemModel, _Invoke);
            if (mockClone)
            studioResourceRepository.ExplorerItemModelClone = a => a;
            else
            studioResourceRepository.ExplorerItemModelClone = a => a.Clone( new Mock<IConnectControlSingleton>().Object,studioResourceRepository);
            var explorerResourceRepository = new Mock<IExplorerResourceRepository>().Object;
            studioResourceRepository.GetExplorerProxy = guid => explorerResourceRepository;
            return studioResourceRepository;
        }

        void SetupMockEnvironment(bool shouldLoadResources)
        {
            _mockEnvironmentModel = GetMockEnvironment();
            _mockEnvironmentModel.SetupGet(x => x.CanStudioExecute).Returns(shouldLoadResources);
        }

        public static Mock<IEnvironmentModel> GetMockEnvironment()
        {
            var eventPublisher = new Mock<IEventPublisher>();

            var designValidationEvents = new Mock<IObservable<DesignValidationMemo>>();
            eventPublisher.Setup(p => p.GetEvent<DesignValidationMemo>()).Returns(designValidationEvents.Object);

            var mock = new Mock<IEnvironmentModel>();

            mock.Setup(m => m.Equals(It.IsAny<IEnvironmentModel>())).Returns(true);
            mock.SetupGet(x => x.Connection.AppServerUri).Returns(new Uri("http://localhost:3142/dsf"));
            mock.SetupGet(x => x.IsConnected).Returns(true);
            mock.Setup(x => x.Connection.ServerEvents).Returns(eventPublisher.Object);
            return mock;
        }

        void SetUpResources(bool addWizardChildToResource, out Mock<IResourceRepository> mockResourceRepository)
        {
            // setup env repo
            var repo = new Mock<IEnvironmentRepository>();
            repo.Setup(l => l.Load()).Verifiable();

            IList<IEnvironmentModel> models = new List<IEnvironmentModel>();
            repo.Setup(l => l.All()).Returns(models);

            Mock<IContextualResourceModel> mockResourceModel11 = null;

            _mockResourceModel = new Mock<IContextualResourceModel>();
            _mockResourceModel.Setup(r => r.ResourceType).Returns(ResourceType.WorkflowService);
            _mockResourceModel.Setup(r => r.Category).Returns("Testing");
            _mockResourceModel.Setup(r => r.ResourceName).Returns("Mock");
            _mockResourceModel.Setup(model => model.ID).Returns(Guid.NewGuid());
            _mockResourceModel.Setup(r => r.Environment).Returns(_mockEnvironmentModel.Object);

            _mockResourceModel1 = new Mock<IContextualResourceModel>();
            _mockResourceModel1.Setup(r => r.ResourceType).Returns(ResourceType.WorkflowService);
            _mockResourceModel1.Setup(r => r.Category).Returns("Testing2");
            _mockResourceModel1.Setup(r => r.ResourceName).Returns("Mock1");
            _mockResourceModel1.Setup(model => model.ID).Returns(Guid.NewGuid());
            _mockResourceModel1.Setup(r => r.Environment).Returns(_mockEnvironmentModel.Object);
            if(addWizardChildToResource)
            {
                mockResourceModel11 = new Mock<IContextualResourceModel>();
                mockResourceModel11.Setup(r => r.ResourceType).Returns(ResourceType.WorkflowService);
                mockResourceModel11.Setup(r => r.Category).Returns("Testing");
                mockResourceModel11.Setup(r => r.ResourceName).Returns("Mock1.wiz");
                mockResourceModel11.Setup(r => r.Environment).Returns(_mockEnvironmentModel.Object);
            }

            _mockResourceModel2 = new Mock<IContextualResourceModel>();
            _mockResourceModel2.Setup(r => r.ResourceType).Returns(ResourceType.Service);
            _mockResourceModel2.Setup(r => r.Category).Returns("Testing2");
            _mockResourceModel2.Setup(r => r.ResourceName).Returns("Mock2");
            _mockResourceModel2.Setup(model => model.ID).Returns(Guid.NewGuid());
            _mockResourceModel2.Setup(r => r.Environment).Returns(_mockEnvironmentModel.Object);

            _mockResourceSourceModel = new Mock<IContextualResourceModel>();
            _mockResourceSourceModel.Setup(r => r.ResourceType).Returns(ResourceType.Source);
            _mockResourceSourceModel.Setup(r => r.Category).Returns("Testing2");
            _mockResourceSourceModel.Setup(r => r.ResourceName).Returns("MockSource");
            _mockResourceSourceModel.Setup(model => model.ID).Returns(Guid.NewGuid());
            _mockResourceSourceModel.Setup(r => r.Environment).Returns(_mockEnvironmentModel.Object);

            Collection<IResourceModel> mockResources = new Collection<IResourceModel>
            {
                _mockResourceModel.Object,
                _mockResourceModel1.Object,
                _mockResourceModel2.Object,
                _mockResourceSourceModel.Object

            };
            if(addWizardChildToResource)
            {
                mockResources.Add(mockResourceModel11.Object);
            }


            mockResourceRepository = new Mock<IResourceRepository>();
            mockResourceRepository.Setup(r => r.All()).Returns(mockResources);

            _mockEnvironmentModel.SetupGet(x => x.ResourceRepository).Returns(mockResourceRepository.Object);
            _mockEnvironmentModel.Setup(x => x.LoadResources());
        }

        //NavigationViewModel CreateViewModel(Mock<IResourceRepository> mockResourceRepository, bool isFromActivityDrop = false, enDsfActivityType activityType = enDsfActivityType.All, Mock<IConnectControlSingleton> connectControlASingleton = null)
        //{
        //    var importServiceContext = new ImportServiceContext();
        //    ImportService.CurrentContext = importServiceContext;

        //    ImportService.Initialize(new List<ComposablePartCatalog>());

        //    return CreateViewModel(EnvironmentRepository.Instance, mockResourceRepository, isFromActivityDrop, activityType, connectControlASingleton);
        //}

        NavigationViewModel CreateViewModel(IEnvironmentRepository environmentRepository, Mock<IResourceRepository> mockResourceRepository, bool isFromActivityDrop = false, enDsfActivityType activityType = enDsfActivityType.All, Mock<IConnectControlSingleton> connectControlASingleton = null, bool mockClone = true)
        {
            var studioResourceRepository = BuildExplorerItems(mockResourceRepository.Object,mockClone);
            connectControlASingleton = connectControlASingleton ?? new Mock<IConnectControlSingleton>();
            var navigationViewModel = new NavigationViewModel(new Mock<IEventAggregator>().Object, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object, null, environmentRepository, studioResourceRepository, connectControlASingleton.Object, () => { }, isFromActivityDrop, activityType);
            return navigationViewModel;
        }

        #endregion
    }
}