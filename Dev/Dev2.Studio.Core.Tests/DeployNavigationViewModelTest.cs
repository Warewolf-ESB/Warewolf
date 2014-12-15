
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Windows.Threading;
using Caliburn.Micro;
using Dev2.AppResources.Repositories;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Common.Interfaces.Infrastructure.Events;
using Dev2.Communication;
using Dev2.Core.Tests.Utils;
using Dev2.Models;
using Dev2.Services.Security;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
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
    [ExcludeFromCodeCoverage]
    public class DeployNavigationViewModelTest
    {
        #region Test Variables

        readonly Action<System.Action, DispatcherPriority> _Invoke = (a, b) => a();
        Mock<IEnvironmentModel> _mockEnvironmentModel;
        Mock<IContextualResourceModel> _mockResourceModel;
        Mock<IContextualResourceModel> _mockResourceModel1;
        Mock<IContextualResourceModel> _mockResourceModel2;
        Mock<IContextualResourceModel> _mockResourceSourceModel;
        Mock<IAuthorizationService> _mockAuthService;
        DeployNavigationViewModel _vm;
        bool _target = true;
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
            _vm.Filter(model => model.ResourceType == Common.Interfaces.Data.ResourceType.WorkflowService);
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
            _vm.Filter(model => model.ResourceType == Common.Interfaces.Data.ResourceType.WorkflowService);
            Assert.AreEqual(2, _vm.ExplorerItemModels[0].ChildrenCount);
            //------------Execute Test---------------------------
            _vm.Filter(null);
            //------------Assert Results-------------------------
            Assert.AreEqual(2, _vm.ExplorerItemModels[0].ChildrenCount); // filter by environment Id
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("NavigationViewModel_EnvironmentId")]
        public void NavigationViewModel_SetEnvironmentId()
        {
            //------------Setup for test--------------------------
            Init(false, true);
            _mockAuthService.Setup(a => a.IsAuthorized(AuthorizationContext.DeployTo, It.IsAny<string>())).Returns(false);
            _mockAuthService.Setup(a => a.IsAuthorized(AuthorizationContext.DeployFrom, It.IsAny<string>())).Returns(true);
            _vm.Environment = _mockEnvironmentModel.Object;

            //------------Preconditions---------------------------
            Assert.AreEqual(0, _vm.ExplorerItemModels[0].ChildrenCount);
            _mockAuthService.Verify(a => a.IsAuthorized(AuthorizationContext.DeployTo, It.IsAny<string>()));
            _mockAuthService.Verify(a => a.IsAuthorized(AuthorizationContext.DeployFrom, It.IsAny<string>()));
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("NavigationViewModel_EnvironmentId")]
        public void NavigationViewModel_SetEnvironmentIdNotTarget()
        {
            //------------Setup for test--------------------------
            _target = false;
            Init(false, true);
            _mockAuthService.Setup(a => a.IsAuthorized(AuthorizationContext.DeployTo, It.IsAny<string>())).Returns(false);
            _mockAuthService.Setup(a => a.IsAuthorized(AuthorizationContext.DeployFrom, It.IsAny<string>())).Returns(true);
            _vm.Environment = _mockEnvironmentModel.Object;

            //------------Preconditions---------------------------
            Assert.AreEqual(4, _vm.ExplorerItemModels[0].ChildrenCount);
            _mockAuthService.Verify(a => a.IsAuthorized(AuthorizationContext.DeployTo, It.IsAny<string>()));
            _mockAuthService.Verify(a => a.IsAuthorized(AuthorizationContext.DeployFrom, It.IsAny<string>()));
        }





        [TestMethod]
        public void FilteredNavigationViewModel_WhereFilterReset_Expects_OriginalExpandState()
        {
            IExplorerItemModel resourceVM = null;

            var reset = new AutoResetEvent(false);
            ThreadExecuter.RunCodeAsSTA(reset,
                () =>
                {
                    Init(false, true);
                    resourceVM = _vm.FindChild(_mockResourceModel.Object);
                    IExplorerItemModel resourceVM2_1 = _vm.FindChild(_mockResourceModel1.Object);
                    IExplorerItemModel resourceVM2_2 = _vm.FindChild(_mockResourceModel2.Object);

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
            var localhostItemModel = new ExplorerItemModel { DisplayName = "localhost", EnvironmentId = Guid.Empty, ResourceType = Common.Interfaces.Data.ResourceType.Server };

            if(resourceModels != null)
            {
                foreach(var resourceModel in resourceModels)
                {
                    var resourceItemModel = new ExplorerItemModel { ResourceId = resourceModel.ID, ResourcePath = resourceModel.Category, EnvironmentId = Guid.Empty, DisplayName = resourceModel.ResourceName };
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
                        categoryItem = new ExplorerItemModel { Parent = localhostItemModel, DisplayName = resourceModel.Category, EnvironmentId = Guid.Empty, ResourceType = Common.Interfaces.Data.ResourceType.Folder, ResourcePath = "" };
                        localhostItemModel.Children.Add(categoryItem);
                    }
                    resourceItemModel.Parent = categoryItem;
                    categoryItem.Children.Add(resourceItemModel);
                }
            }
            var studioResourceRepository = new StudioResourceRepository(localhostItemModel, _Invoke);
            var explorerResourceRepository = new Mock<IClientExplorerResourceRepository>().Object;
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
            mock.Setup(a => a.AuthorizationService).Returns(new Mock<IAuthorizationService>().Object);
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

        DeployNavigationViewModel CreateViewModel(Mock<IResourceRepository> mockResourceRepository)
        {

            return CreateViewModel(EnvironmentRepository.Instance, mockResourceRepository);
        }

        DeployNavigationViewModel CreateViewModel(IEnvironmentRepository environmentRepository, Mock<IResourceRepository> mockResourceRepository)
        {
            StudioResourceRepository studioResourceRepository = BuildExplorerItems(mockResourceRepository.Object);
            var navigationViewModel = new DeployNavigationViewModel(new Mock<IEventAggregator>().Object, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object, environmentRepository, studioResourceRepository, _target, new Mock<IConnectControlSingleton>().Object);
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
        public void DeployNavigationViewModel_RefreshCommand_CallsConnectControlRefresh()
        {
            // base case
            Init(false, true);
         
            PrivateObject pvt = new PrivateObject(_vm);
            var con = new Mock<IConnectControlSingleton>();
            pvt.SetField("_connectControlSingleton", con.Object);
            _vm.RefreshMenuCommand.Execute(null);
            con.Verify(a => a.Refresh(_vm.Environment.ID),Times.Once());


        }

        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DeployNavigationViewMode_Update")]
        [TestMethod]
        public void DeployNavigationViewMode_Update_FiltersAndCallsStudio()
        {
            // base case
            Init(false, true);
            var studio = new Mock<IStudioResourceRepository>();
            PrivateObject p = new PrivateObject(_vm, new PrivateType( typeof(NavigationViewModelBase)));
            p.SetField("_studioResourceRepository", studio.Object);
            _vm.Update();
           studio.Verify(a=>a.Load(It.IsAny<Guid>(),It.IsAny<IAsyncWorker>()));

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
