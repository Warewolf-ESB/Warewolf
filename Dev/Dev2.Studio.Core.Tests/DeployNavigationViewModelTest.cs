using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Windows.Threading;
using Caliburn.Micro;
using Dev2.AppResources.Repositories;
using Dev2.Communication;
using Dev2.Composition;
using Dev2.Core.Tests.Environments;
using Dev2.Core.Tests.Utils;
using Dev2.Interfaces;
using Dev2.Models;
using Dev2.Providers.Events;
using Dev2.Services.Security;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.ViewModels.Navigation;
using Dev2.Threading;
using Dev2.Util;
using Dev2.ViewModels.Deploy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

// ReSharper disable InconsistentNaming
namespace Dev2.Core.Tests
{
    /// <summary>
    ///     Summary description for NavigationViewModelTest
    /// </summary>
    [TestClass]    
    public class DeployNavigationViewModelTest
    {
        #region Test Variables
        Action<System.Action, DispatcherPriority> _Invoke = new Action<System.Action, DispatcherPriority>((a, b) => a());
        Mock<IEnvironmentModel> _mockEnvironmentModel;
        Mock<IContextualResourceModel> _mockResourceModel;
        Mock<IContextualResourceModel> _mockResourceModel1;
        Mock<IContextualResourceModel> _mockResourceModel2;
        Mock<IContextualResourceModel> _mockResourceSourceModel;
        Mock<IAuthorizationService> _mockAuthService;
        DeployNavigationViewModel _vm;

        #endregion Test Variables


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
            _mockAuthService = new Mock<IAuthorizationService>();
            _mockAuthService.Setup(a => a.IsAuthorized(It.IsAny<AuthorizationContext>(), It.IsAny<string>())).Returns(true);
        }

        [TestCleanup]
        public void TestCleanUp()
        {
        }

        #endregion


        #region Updating Resources

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("NavigationViewModel_LoadEnvironments")]
        public void NavigationViewModel_LoadEnvironments_WhenResourceRepositoryReturnsOnlyOneItem_ExplorerItemModelsOnlyHasOneItem()
        {
            //Arrange
            const string displayName = "localhost";
            Mock<IEnvironmentModel> mockEnvironment = EnviromentRepositoryTest.CreateMockEnvironment();
            mockEnvironment.Setup(model => model.CanStudioExecute).Returns(true);
            var environmentRepository = GetEnvironmentRepository(mockEnvironment);
            var mockStudioRepo = new Mock<IStudioResourceRepository>();

            mockStudioRepo.SetupGet(p => p.ExplorerItemModels).Returns(
                new ObservableCollection<ExplorerItemModel>
                    {
                        new ExplorerItemModel
                        {
                            ResourceType = Data.ServiceModel.ResourceType.Server,
                            DisplayName = displayName,
                            ResourceId = Guid.Empty,
                            Permissions = Permissions.Administrator,
                            EnvironmentId = Guid.Empty
                        }
                    });
            var viewmodel = new NavigationViewModel(new Mock<IEventAggregator>().Object, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object, null, environmentRepository, mockStudioRepo.Object);





            //Act
            viewmodel.LoadEnvironmentResources(mockEnvironment.Object);
            //Assert
            Assert.IsNotNull(viewmodel.ExplorerItemModels);
            Assert.AreEqual(1, viewmodel.ExplorerItemModels.Count);
            Assert.AreEqual(displayName, viewmodel.ExplorerItemModels[0].DisplayName);
            Assert.AreEqual(Permissions.Administrator, viewmodel.ExplorerItemModels[0].Permissions);
            Assert.AreEqual(Guid.Empty, viewmodel.ExplorerItemModels[0].EnvironmentId);
            Assert.AreEqual(Guid.Empty, viewmodel.ExplorerItemModels[0].ResourceId);
            Assert.AreEqual(Data.ServiceModel.ResourceType.Server, viewmodel.ExplorerItemModels[0].ResourceType);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("NavigationViewModel_LoadEnvironments")]
        public void NavigationViewModel_LoadEnvironments_WillCallConnectEnvironmentOnce()
        {
            //Arrange
            Mock<IEnvironmentModel> mockEnvironment = EnviromentRepositoryTest.CreateMockEnvironment();
            mockEnvironment.Setup(model => model.CanStudioExecute).Returns(true);
            var environmentRepository = GetEnvironmentRepository(mockEnvironment);
            var mockStudioRepo = new Mock<IStudioResourceRepository>();
            mockStudioRepo.Setup(m => m.Load(It.IsAny<Guid>(), It.IsAny<IAsyncWorker>())).Verifiable();
            var viewmodel = new NavigationViewModel(new Mock<IEventAggregator>().Object, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object, null, environmentRepository, mockStudioRepo.Object);
            //Act
            viewmodel.LoadEnvironmentResources(mockEnvironment.Object);
            //Assert
            mockStudioRepo.Verify(m => m.Load(It.IsAny<Guid>(), It.IsAny<IAsyncWorker>()), Times.Once());
        }


        private static IEnvironmentRepository GetEnvironmentRepository(Mock<IEnvironmentModel> mockEnvironment)
        {
            var repo = new TestLoadEnvironmentRespository(mockEnvironment.Object);
            repo.IsLoaded = true;
            return repo;
        }


        [TestMethod]
        public void AddEnvironmentWithShouldLoadResourcesTrueExpectedRootAndChildrenCreated()
        {
            Init(false, true);
            var explorerItemModels = _vm.ExplorerItemModels;
            Assert.IsNotNull(explorerItemModels);
            Assert.AreEqual(1, explorerItemModels.Count);
            Assert.AreEqual(2, explorerItemModels[0].Children.Count);
        }


        #endregion Updating Resources

        #region Filtering

        [TestMethod]
        public void FilteredNavigationViewModel_AfterReload_Expects_FilteredItemsInTreeWithIsFilteredTrue()
        {
            Init(false, true);
            var resourceVM = _vm.FindChild(_mockResourceModel.Object);

            Assert.IsNotNull(resourceVM);

            Assert.AreEqual(4, _vm.ExplorerItemModels[0].ChildrenCount);

            _vm.UpdateSearchFilter("Mock2");
            resourceVM = _vm.FindChild(_mockResourceModel.Object);
            Assert.AreEqual(1, _vm.ExplorerItemModels[0].ChildrenCount);
            Assert.IsNull(resourceVM);

            //_vm.LoadEnvironmentResources(_mockEnvironmentModel.Object);
            resourceVM = _vm.FindChild(_mockResourceModel.Object);

            Assert.AreEqual(1, _vm.ExplorerItemModels[0].ChildrenCount);
            Assert.IsNull(resourceVM);
            _vm.UpdateSearchFilter("Mock");
            resourceVM = _vm.FindChild(_mockResourceModel.Object);
            Assert.AreEqual(4, _vm.ExplorerItemModels[0].ChildrenCount);
            Assert.IsNotNull(resourceVM);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("NavigationViewModel_Filter")]
        public void NavigationViewModel_Filter_GivenFunction_ShouldFilter()
        {
            //------------Setup for test--------------------------
            Init(false, true);
            //------------Preconditions---------------------------
            Assert.AreEqual(4, _vm.ExplorerItemModels[0].ChildrenCount);
            //------------Execute Test---------------------------
            _vm.Filter(model => model.ResourceType == Data.ServiceModel.ResourceType.WorkflowService);
            //------------Assert Results-------------------------
            Assert.AreEqual(2, _vm.ExplorerItemModels[0].ChildrenCount);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("NavigationViewModel_Filter")]
        public void NavigationViewModel_Filter_GivenNull_ShouldOriginalCollection()
        {
            //------------Setup for test--------------------------
            Init(false, true);
            //------------Preconditions---------------------------
            Assert.AreEqual(4, _vm.ExplorerItemModels[0].ChildrenCount);
            _vm.Filter(model => model.ResourceType == Data.ServiceModel.ResourceType.WorkflowService);
            Assert.AreEqual(2, _vm.ExplorerItemModels[0].ChildrenCount);
            //------------Execute Test---------------------------
            _vm.Filter(null);
            //------------Assert Results-------------------------
            Assert.AreEqual(4, _vm.ExplorerItemModels[0].ChildrenCount);
        }



        [TestMethod]
        public void FilteredNavigationViewModel_WhereFilterReset_Expects_OriginalExpandState()
        {
            ExplorerItemModel resourceVM = null;

            var reset = new AutoResetEvent(false);
            ThreadExecuter.RunCodeAsSTA(reset,
                () =>
                {
                    Init(false, true);
                    resourceVM = _vm.FindChild(_mockResourceModel.Object);
                    ExplorerItemModel resourceVM2_1 = _vm.FindChild(_mockResourceModel1.Object);
                    ExplorerItemModel resourceVM2_2 = _vm.FindChild(_mockResourceModel2.Object);

                    Assert.IsTrue(resourceVM.Parent.IsExplorerExpanded == false);
                    Assert.IsTrue(resourceVM2_1.Parent.IsExplorerExpanded == false);
                    Assert.IsTrue(resourceVM2_2.Parent.IsExplorerExpanded == false);

                    _vm.UpdateSearchFilter("Mock2");

                    _vm.UpdateSearchFilter("");
                });
            reset.WaitOne();

            Assert.IsTrue(resourceVM.Parent.IsExplorerExpanded == false);
        }

        #endregion Filtering





        #region Refresh Environments Tests

        #endregion

        #region Disconnect

        [TestMethod]
        public void EnvironmentNodeDisconnect_Expect_NodeRemovedFromRoot()
        {
            Init(false, true);
            _mockEnvironmentModel.SetupGet(c => c.IsConnected).Returns(true);
            _mockEnvironmentModel.SetupGet(c => c.Name).Returns("Mock");

            Assert.IsTrue(_vm.ExplorerItemModels[0].ChildrenCount != 0);

            var message =
                new EnvironmentDisconnectedMessage(_mockEnvironmentModel.Object);
            _vm.Handle(message);
            Assert.IsTrue(_vm.ExplorerItemModels[0].ChildrenCount == 0);
        }

        #endregion

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("NavigationViewModel_SetNodeOverwrite")]
        public void NavigationViewModel_SetNodeOverwrite_SetToFalse_FalseReturned()
        {
            Init(false, true);
            var newResource = new Mock<IContextualResourceModel>();
            newResource.Setup(r => r.ResourceType)
                .Returns(ResourceType.WorkflowService);
            newResource.Setup(r => r.Category).Returns("Testing");
            newResource.Setup(r => r.ResourceName).Returns("Cake");
            newResource.Setup(r => r.Environment)
                .Returns(_mockEnvironmentModel.Object);

            var updatemsg = new UpdateResourceMessage(newResource.Object);
            _vm.Handle(updatemsg);

            Assert.IsFalse(_vm.SetNodeOverwrite(newResource.Object, false));
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("NavigationViewModel_SetNodeOverwrite")]
        public void NavigationViewModel_SetNodeOverwrite_SetToFalse_TrueReturned()
        {
            Init(false, true);
            var newResource = new Mock<IContextualResourceModel>();
            newResource.Setup(r => r.ResourceType)
                .Returns(ResourceType.WorkflowService);
            newResource.Setup(r => r.Category).Returns("Testing");
            newResource.Setup(r => r.ResourceName).Returns("Mock");
            newResource.Setup(model => model.ID).Returns(_mockResourceModel.Object.ID);
            newResource.Setup(r => r.Environment)
                .Returns(_mockEnvironmentModel.Object);

            Assert.IsTrue(_vm.SetNodeOverwrite(newResource.Object, true));
        }

        #region Private Test Methods

        void Init(bool addWizardChildToResource, bool shouldLoadResources)
        {
            SetupMockEnvironment(shouldLoadResources);

            Mock<IResourceRepository> mockResourceRepository;
            SetUpResources(addWizardChildToResource, out mockResourceRepository);


            _vm = CreateViewModel(mockResourceRepository);
            _mockEnvironmentModel.Setup(model => model.IsLocalHost).Returns(true);
           
            _vm.Environment = _mockEnvironmentModel.Object;
        }

        StudioResourceRepository BuildExplorerItems(IResourceRepository resourceRepository)
        {
            var resourceModels = resourceRepository.All();
            var localhostItemModel = new ExplorerItemModel { DisplayName = "localhost", EnvironmentId = Guid.Empty, ResourceType = Data.ServiceModel.ResourceType.Server };

            if(resourceModels != null)
            {
                foreach(var resourceModel in resourceModels)
                {
                    var resourceItemModel = new ExplorerItemModel { ResourceId = resourceModel.ID, ResourcePath = resourceModel.Category, EnvironmentId = Guid.Empty, DisplayName = resourceModel.ResourceName };
                    Data.ServiceModel.ResourceType correctTyping = Data.ServiceModel.ResourceType.WorkflowService;
                    switch(resourceModel.ResourceType)
                    {
                        case ResourceType.WorkflowService:
                            correctTyping = Data.ServiceModel.ResourceType.WorkflowService;
                            break;
                        case ResourceType.Service:
                            correctTyping = Data.ServiceModel.ResourceType.DbService;
                            break;
                        case ResourceType.Source:
                            correctTyping = Data.ServiceModel.ResourceType.WebSource;
                            break;
                    }
                    resourceItemModel.ResourceType = correctTyping;

                    var categoryItem = localhostItemModel.Children.FirstOrDefault(model => model.DisplayName == resourceModel.Category);
                    if(categoryItem == null)
                    {
                        categoryItem = new ExplorerItemModel();
                        categoryItem.Parent = localhostItemModel;
                        categoryItem.DisplayName = resourceModel.Category;
                        categoryItem.EnvironmentId = Guid.Empty;
                        categoryItem.ResourceType = Data.ServiceModel.ResourceType.Folder;
                        categoryItem.ResourcePath = "";
                        localhostItemModel.Children.Add(categoryItem);
                    }
                    resourceItemModel.Parent = categoryItem;
                    categoryItem.Children.Add(resourceItemModel);
                }
            }
            var studioResourceRepository = new StudioResourceRepository(localhostItemModel, _Invoke);
            var explorerResourceRepository = new Mock<IExplorerResourceRepository>().Object;
            studioResourceRepository.GetExplorerProxy = guid => explorerResourceRepository;
            return studioResourceRepository;

        }

        void SetupMockEnvironment(bool shouldLoadResources)
        {
            _mockEnvironmentModel = GetMockEnvironment();
            _mockEnvironmentModel.Setup(a => a.AuthorizationService).Returns(_mockAuthService.Object);
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
            mock.Setup(a => a.AuthorizationService).Returns( new Mock<IAuthorizationService>().Object);
            return mock;
        }

        void SetUpResources(bool addWizardChildToResource, out Mock<IResourceRepository> mockResourceRepository)
        {
            // setup env repo
            var repo = new Mock<IEnvironmentRepository>();
            repo.Setup(l => l.Load()).Verifiable();

            IList<IEnvironmentModel> models = new List<IEnvironmentModel>();
            repo.Setup(l => l.All()).Returns(models);

            ImportService.CurrentContext = CompositionInitializer.InitializeNavigationViewModelTests(repo);

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

        DeployNavigationViewModel CreateViewModel(Mock<IResourceRepository> mockResourceRepository)
        {
            var importServiceContext = new ImportServiceContext();
            ImportService.CurrentContext = importServiceContext;

            ImportService.Initialize(new List<ComposablePartCatalog>());

            return CreateViewModel(EnvironmentRepository.Instance, mockResourceRepository);
        }

        DeployNavigationViewModel CreateViewModel(IEnvironmentRepository environmentRepository, Mock<IResourceRepository> mockResourceRepository)
        {
            StudioResourceRepository studioResourceRepository = BuildExplorerItems(mockResourceRepository.Object);
            var navigationViewModel = new DeployNavigationViewModel(new Mock<IEventAggregator>().Object, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object, environmentRepository, studioResourceRepository,true);
            return navigationViewModel;
        }

        #endregion

        [Owner("Leon Rajindrapersadh")]
        [TestCategory("EnvironmentTreeViewModel_Disconnect")]
        [TestMethod]
        public void ClearConflicts_Expects_AllItemsHaveNoConflicts()
        {
            // base case
            Init(false, true);
            _vm.ExplorerItemModels[0].IsOverwrite = true;
            _vm.ExplorerItemModels[0].Children[0].IsOverwrite = true;
            _vm.ExplorerItemModels[0].IsOverwrite = true;
            _vm.ExplorerItemModels[0].Children[0].Children[0].IsOverwrite = true;
            _vm.ClearConflictingNodesNodes();
            Assert.AreEqual(false, _vm.ExplorerItemModels[0].IsOverwrite);
            Assert.AreEqual(false, _vm.ExplorerItemModels[0].Children[0].IsOverwrite);
            Assert.AreEqual(false, _vm.ExplorerItemModels[0].Children[0].Children[0].IsOverwrite);



        }
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("EnvironmentTreeViewModel_Disconnect")]
        [TestMethod]
        public void ClearConflicts_Expects_NoSideEffects()
        {


            // make sure no side effects 
            Init(false, true);
            _vm.ClearConflictingNodesNodes();
            Assert.AreEqual(false, _vm.ExplorerItemModels[0].IsOverwrite);
            Assert.AreEqual(true, _vm.ExplorerItemModels[0].Children.All(a => a.IsOverwrite == false));
            Assert.AreEqual(true, _vm.ExplorerItemModels[0].Children[0].Children.All(a => !a.IsOverwrite));

        }
    }
}