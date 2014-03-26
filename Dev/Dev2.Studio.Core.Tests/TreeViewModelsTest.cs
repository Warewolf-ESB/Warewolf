#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Caliburn.Micro;
using Dev2.Activities;
using Dev2.Communication;
using Dev2.Composition;
using Dev2.Core.Tests.ProperMoqs;
using Dev2.Core.Tests.Utils;
using Dev2.Core.Tests.ViewModelTests.ViewModelMocks;
using Dev2.Network;
using Dev2.Providers.Errors;
using Dev2.Providers.Events;
using Dev2.Services.Events;
using Dev2.Services.Security;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Core.ViewModels.Navigation;
using Dev2.Studio.ViewModels.Navigation;
using Dev2.Studio.ViewModels.Workflow;
using Dev2.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Unlimited.Applications.BusinessDesignStudio.Activities;

#endregion

namespace Dev2.Core.Tests
{
    /// <summary>
    ///     Summary description for Base
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class TreeViewModelsTest
    {
        IEnvironmentConnection _testConnection;

        #region Variables

        Mock<IEventAggregator> _eventAggregator;
        CategoryTreeViewModel _categoryVm;
        CategoryTreeViewModel _categoryVm2;
        EnvironmentTreeViewModel _environmentVm;
        Mock<IEnvironmentModel> _mockEnvironmentModel;
        Mock<IContextualResourceModel> _mockResourceModel;
        Mock<IContextualResourceModel> _mockResourceModel2;
        ResourceTreeViewModel _resourceVm;
        ResourceTreeViewModel _resourceVm2;
        RootTreeViewModel _rootVm;
        ServiceTypeTreeViewModel _serviceTypeVm;
        ServiceTypeTreeViewModel _serviceTypeVm2;

        private static readonly object TestGuard = new object();

        #endregion

        /// <summary>
        ///     Gets or sets the result context which provides
        ///     information about and functionality for the current result run.
        /// </summary>
        public TestContext TestContext { get; set; }

        #region Initialize

        [TestInitialize]
        public void MyTestInitialize()
        {
            // Monitor.Enter(TestGuard);

            _eventAggregator = new Mock<IEventAggregator>();
            EventPublishers.Aggregator = _eventAggregator.Object;
            _eventAggregator.Setup(e => e.Publish(It.IsAny<object>())).Verifiable();

            _testConnection = new ServerProxy(new Uri("http://127.0.0.1:77/dsf"));

            //            ImportService.CurrentContext =
            //                CompositionInitializer.InializeWithEventAggregator();


            _mockEnvironmentModel = new Mock<IEnvironmentModel>();
            _mockEnvironmentModel.SetupGet(x => x.Connection.AppServerUri).Returns(new Uri("http://127.0.0.1/"));
            _mockEnvironmentModel.Setup(e => e.Connection).Returns(_testConnection);
            _mockEnvironmentModel.Setup(model => model.Equals(It.IsAny<IEnvironmentModel>())).Returns(true);

            _mockResourceModel = new Mock<IContextualResourceModel>();
            _mockResourceModel.Setup(r => r.ResourceType).Returns(ResourceType.WorkflowService);
            _mockResourceModel.Setup(r => r.Category).Returns("Testing");
            _mockResourceModel.Setup(r => r.ResourceName).Returns("Mock");
            _mockResourceModel.Setup(r => r.Environment).Returns(_mockEnvironmentModel.Object);

            _mockResourceModel2 = new Mock<IContextualResourceModel>();
            _mockResourceModel2.Setup(r => r.ResourceType).Returns(ResourceType.Service);
            _mockResourceModel2.Setup(r => r.Category).Returns("Testing2");
            _mockResourceModel2.Setup(r => r.ResourceName).Returns("Mock2");
            _mockResourceModel2.Setup(r => r.Environment).Returns(_mockEnvironmentModel.Object);

            _rootVm = new RootTreeViewModel(_eventAggregator.Object);
            _environmentVm = new EnvironmentTreeViewModel(_eventAggregator.Object, _rootVm, _mockEnvironmentModel.Object, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object);
            _serviceTypeVm = new ServiceTypeTreeViewModel(_eventAggregator.Object, _environmentVm, ResourceType.WorkflowService);
            _serviceTypeVm2 = new ServiceTypeTreeViewModel(_eventAggregator.Object, _environmentVm, ResourceType.Service);

            _categoryVm = new CategoryTreeViewModel(_eventAggregator.Object, _serviceTypeVm, _mockResourceModel.Object.Category, _mockResourceModel.Object.ResourceType);
            _categoryVm2 = new CategoryTreeViewModel(_eventAggregator.Object, _serviceTypeVm2, _mockResourceModel2.Object.Category, _mockResourceModel2.Object.ResourceType);

            _resourceVm = new ResourceTreeViewModel(_eventAggregator.Object, _categoryVm, _mockResourceModel.Object);
            _resourceVm2 = new ResourceTreeViewModel(_eventAggregator.Object, _categoryVm2, _mockResourceModel2.Object);
        }

        [TestCleanup]
        public void TestCleanUp()
        {
            //Monitor.Exit(TestGuard);
        }

        #endregion

        #region Root

        [TestMethod]
        public void CheckRoot_Expected_AllChildrenChecked()
        {
            _resourceVm.IsChecked = false;
            _resourceVm2.IsChecked = false;
            _rootVm.IsChecked = true;

            Assert.IsTrue(_rootVm.IsChecked == true);
            Assert.IsTrue(_environmentVm.IsChecked == true);
            Assert.IsTrue(_serviceTypeVm.IsChecked == true);
            Assert.IsTrue(_serviceTypeVm2.IsChecked == true);
            Assert.IsTrue(_categoryVm.IsChecked == true);
            Assert.IsTrue(_categoryVm2.IsChecked == true);
            Assert.IsTrue(_resourceVm.IsChecked == true);
            Assert.IsTrue(_resourceVm2.IsChecked == true);
        }

        [TestMethod]
        public void UncheckRoot_Expected_AllChildrenUnChecked()
        {
            _resourceVm.IsChecked = true;
            _resourceVm2.IsChecked = true;
            _rootVm.IsChecked = false;

            Assert.IsTrue(_rootVm.IsChecked == false);
            Assert.IsTrue(_environmentVm.IsChecked == false);
            Assert.IsTrue(_serviceTypeVm.IsChecked == false);
            Assert.IsTrue(_serviceTypeVm2.IsChecked == false);
            Assert.IsTrue(_categoryVm.IsChecked == false);
            Assert.IsTrue(_categoryVm2.IsChecked == false);
            Assert.IsTrue(_resourceVm.IsChecked == false);
            Assert.IsTrue(_resourceVm2.IsChecked == false);
        }

        [TestMethod]
        public void OneCheckedChildAndOneUnCheckedChild_Expected_PartiallyCheckedRoot()
        {
            _resourceVm.IsChecked = true;
            _resourceVm2.IsChecked = false;

            Assert.IsTrue(_rootVm.IsChecked == null);
            Assert.IsTrue(_environmentVm.IsChecked == null);
            Assert.IsTrue(_serviceTypeVm.IsChecked == true);
            Assert.IsTrue(_serviceTypeVm2.IsChecked == false);
            Assert.IsTrue(_categoryVm.IsChecked == true);
            Assert.IsTrue(_categoryVm2.IsChecked == false);
            Assert.IsTrue(_resourceVm.IsChecked == true);
            Assert.IsTrue(_resourceVm2.IsChecked == false);
        }

        [TestMethod]
        public void RootNodeFindChildByEnvironment_Expected_RightEnvironmentNode()
        {
            var environment = _mockEnvironmentModel.Object;
            var child = _rootVm.FindChild(environment);
            Assert.IsTrue(Equals(child, _environmentVm));
        }

        [TestMethod]
        public void RootNodeFindChildByResourceModel_Expected_RightChildNode()
        {
            var child = _rootVm.FindChild(_mockResourceModel.Object);
            Assert.IsTrue(ReferenceEquals(child, _resourceVm));
        }

        [TestMethod]
        public void TestGetChildCountFromRoot_Expected_RecursiveTotal()
        {
            var childCount = _rootVm.ChildrenCount;
            Assert.IsTrue(childCount == 2);
        }

        [TestMethod]
        public void TestGetChildCount_WherePredicateIsNull_Expected_AllChildren()
        {
            var childCount = _rootVm.GetChildren(null).ToList().Count;
            Assert.IsTrue(childCount == 7);
        }

        [TestMethod]
        public void TestGetChildren_Expected_FirstChildMatchingPredicate()
        {
            var child = _rootVm.GetChildren(c => c.DisplayName == "Mock").ToList();
            Assert.IsTrue(child.Count == 1);
            Assert.IsTrue(child.First().DisplayName == "Mock");
        }

        [TestMethod]
        public void TestFiler_Were_FiterIsSet_Expected_CheckedNodesAndParentCategoriesArentFiltered_()
        {
            var eventAggregator = new Mock<IEventAggregator>().Object;
            ITreeNode parent = new CategoryTreeViewModel(eventAggregator, null, "More", ResourceType.WorkflowService);

            var checkedNonMatchingNode = new ResourceTreeViewModel(new Mock<IEventAggregator>().Object, parent, Dev2MockFactory.SetupResourceModelMock(ResourceType.WorkflowService, "cake").Object);

            checkedNonMatchingNode.IsChecked = true;

            var nonMatchingNode = new ResourceTreeViewModel(new Mock<IEventAggregator>().Object, parent, Dev2MockFactory.SetupResourceModelMock(ResourceType.WorkflowService, "cake1111").Object);

            var matchingNode = new ResourceTreeViewModel(new Mock<IEventAggregator>().Object, parent, Dev2MockFactory.SetupResourceModelMock(ResourceType.WorkflowService, "Match").Object);

            parent.FilterText = ("Match");

            Assert.IsTrue(nonMatchingNode.IsFiltered);
            Assert.IsFalse(checkedNonMatchingNode.IsFiltered);
            Assert.IsFalse(matchingNode.IsFiltered);
            Assert.IsFalse(parent.IsFiltered);
        }

        [TestMethod]
        public void TestFiler_Were_AllNodesFiltered_Expected_CheckedNodesAndParentCategoriestFiltered_()
        {
            var eventAggregator = new Mock<IEventAggregator>().Object;
            var rootVM = new RootTreeViewModel(eventAggregator);
            var environmentVM = new EnvironmentTreeViewModel(eventAggregator, rootVM, _mockEnvironmentModel.Object, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object);
            var serviceTypeVM = new ServiceTypeTreeViewModel(eventAggregator, environmentVM, ResourceType.WorkflowService);
            var categoryVM = new CategoryTreeViewModel(eventAggregator, serviceTypeVM, _mockResourceModel.Object.Category, _mockResourceModel.Object.ResourceType);
            var resource1 = Dev2MockFactory.SetupResourceModelMock(ResourceType.WorkflowService, "cake1").Object;
            var resource2 = Dev2MockFactory.SetupResourceModelMock(ResourceType.WorkflowService, "cake2").Object;

            var resourceVM1 = new ResourceTreeViewModel(eventAggregator, categoryVM, resource1);
            var resourceVM2 = new ResourceTreeViewModel(eventAggregator, categoryVM, resource2);

            resourceVM1.IsChecked = false;
            resourceVM2.IsChecked = false;

            categoryVM.FilterText = ("Match");

            Assert.IsTrue(resourceVM1.IsFiltered);
            Assert.IsTrue(resourceVM2.IsFiltered);
            Assert.IsTrue(categoryVM.IsFiltered);
        }

        [TestMethod]
        public void TestFilter_Were_AllNodesCheckedAndFilter_Expected_CheckedNodesAndParentCategoriestNotFiltered()
        {
            var eventAggregator = new Mock<IEventAggregator>().Object;
            var rootVM = new RootTreeViewModel(eventAggregator);
            var environmentVM = new EnvironmentTreeViewModel(eventAggregator, rootVM, _mockEnvironmentModel.Object, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object);
            var serviceTypeVM = new ServiceTypeTreeViewModel(eventAggregator, environmentVM, ResourceType.WorkflowService);
            var categoryVM = new CategoryTreeViewModel(eventAggregator, serviceTypeVM, _mockResourceModel.Object.Category, _mockResourceModel.Object.ResourceType);

            var resource1 = Dev2MockFactory.SetupResourceModelMock(ResourceType.WorkflowService, "cake1").Object;
            var resource2 = Dev2MockFactory.SetupResourceModelMock(ResourceType.WorkflowService, "cake2").Object;

            var resourceVM1 = new ResourceTreeViewModel(eventAggregator, categoryVM, resource1);
            var resourceVM2 = new ResourceTreeViewModel(eventAggregator, categoryVM, resource2);

            resourceVM1.IsChecked = false;
            resourceVM2.IsChecked = false;

            categoryVM.FilterText = "cake";

            Assert.IsFalse(resourceVM1.IsFiltered);
            Assert.IsFalse(resourceVM2.IsFiltered);
            Assert.IsFalse(categoryVM.IsFiltered);
        }

        #endregion Root

        #region Environment

        [TestMethod]
        public void EnvironmentNodeCanConnectWhenCanStudioExecuteFalseAndConnected()
        {
            _mockEnvironmentModel.Setup(e => e.CanStudioExecute).Returns(false);
            _mockEnvironmentModel.Setup(e => e.IsConnected).Returns(true);
            Assert.IsTrue(_environmentVm.CanConnect);
        }

        [TestMethod]
        public void EnvironmentNodeCanConnectWhenNotConnected()
        {
            _mockEnvironmentModel.Setup(e => e.IsConnected).Returns(false);
            Assert.IsTrue(_environmentVm.CanConnect);
        }

        [TestMethod]
        public void EnvironmentNodeConnectCommadsSetsCanStudioExecute()
        {
            //------Test Setup---------
            _mockEnvironmentModel.Setup(e => e.IsConnected).Returns(true);
            _mockEnvironmentModel.Setup(e => e.CanStudioExecute)
                .Returns(false)
                .Verifiable();
            _mockEnvironmentModel.Setup(e => e.ForceLoadResources()).Verifiable();
            _mockEnvironmentModel.Setup(e => e.Connect()).Verifiable();
            _mockEnvironmentModel.Setup(e => e.IsConnected).Returns(false);
            var navigationVM = new Mock<INavigationContext>();
            navigationVM.Setup(vm => vm.LoadEnvironmentResources(It.IsAny<IEnvironmentModel>())).Verifiable();
            _rootVm.Parent = navigationVM.Object;

            //------Test Execution---------
            _environmentVm.ConnectCommand.Execute(null);

            //------Assertion--------
            _mockEnvironmentModel.Verify(e => e.ForceLoadResources(), Times.Once());
            _mockEnvironmentModel.Verify(e => e.Connect(), Times.Once());
            navigationVM.Verify(vm => vm.LoadEnvironmentResources(It.IsAny<IEnvironmentModel>()), Times.Once());

        }

        [TestMethod]
        public void EnvironmentNodeExpectHasNewResourceCommand()
        {
            Assert.IsNotNull(_environmentVm.NewResourceCommand);
        }

        [TestMethod]
        public void EnvironmentNodeFindChildByResourceType_Expected_RightServiceTypeNode()
        {
            var child = _environmentVm.FindChild(ResourceType.WorkflowService);
            Assert.IsTrue(ReferenceEquals(child, _serviceTypeVm));
        }

        [TestMethod]
        public void EnvironmentNodeCantDisconnectLocalHost()
        {
            _mockEnvironmentModel.SetupGet(c => c.IsConnected).Returns(true);
            _mockEnvironmentModel.SetupGet(c => c.Connection).Returns(_testConnection);
            _mockEnvironmentModel.SetupGet(c => c.Name).Returns(StringResources.DefaultEnvironmentName);
            Assert.IsTrue(_environmentVm.CanDisconnect == false);
        }

        [TestMethod]
        public void EnvironmentNodeCanDisconnectOtherHosts()
        {
            _mockEnvironmentModel.SetupGet(c => c.IsConnected).Returns(true);
            _mockEnvironmentModel.SetupGet(c => c.Connection).Returns(_testConnection);
            _mockEnvironmentModel.SetupGet(c => c.Name).Returns("Mock");
            Assert.IsTrue(_environmentVm.CanDisconnect == true);
        }

        [TestMethod]
        public void EnvironmentNodeConnectCommand_Expected_EnvironmentModelConnectMethodExecuted()
        {
            _mockEnvironmentModel.Setup(c => c.Connect()).Verifiable();
            _mockEnvironmentModel.SetupGet(c => c.IsConnected).Returns(false);

            var navigationVM = new Mock<INavigationContext>();
            navigationVM.Setup(vm => vm.LoadEnvironmentResources(It.IsAny<IEnvironmentModel>())).Verifiable();
            _rootVm.Parent = navigationVM.Object;

            var cmd = _environmentVm.ConnectCommand;
            cmd.Execute(null);

            _mockEnvironmentModel.Verify(c => c.Connect(), Times.Once());
        }

        [TestMethod]
        public void EnvironmentNodeDisconnectCommand_Expected_EnvironmentModelDisconnectMethodExecuted()
        {
            _mockEnvironmentModel.Setup(c => c.Disconnect()).Verifiable();
            _mockEnvironmentModel.SetupGet(c => c.Connection).Returns(_testConnection);
            _mockEnvironmentModel.SetupGet(c => c.IsConnected).Returns(true);
            var cmd = _environmentVm.DisconnectCommand;
            cmd.Execute(null);

            _mockEnvironmentModel.Verify(c => c.Disconnect(), Times.Once());
        }

        [TestMethod]
        public void EnvironmentNodeRemoveCommand_Expected_MediatorRemoveServerFromExplorerMessage()
        {
            _environmentVm.RemoveCommand.Execute(null);
            _eventAggregator.Verify(e => e.Publish(It.IsAny<RemoveEnvironmentMessage>()), Times.Once());
        }

        [TestMethod]
        public void EnvironmentNodeISNotIHandleUpdateActiveEnvironmentMessage()
        {
            //Do not select the active environment in the tree,
            Assert.IsNotInstanceOfType(_environmentVm, typeof(IHandle<UpdateActiveEnvironmentMessage>));
        }



        #endregion Environment

        #region ServicType

        [TestMethod]
        public void ServiceNodeEnvironmentModel_Expect_ParentEnvironmentModel()
        {
            var model = _serviceTypeVm.EnvironmentModel;
            var model2 = _environmentVm.EnvironmentModel;
            Assert.IsTrue(ReferenceEquals(model, model2));
        }

        [TestMethod]
        public void ServiceTypeNodeFindChildByString_Expected_RightCategoryNode()
        {
            var child = _serviceTypeVm.FindChild("Testing");
            Assert.IsTrue(ReferenceEquals(child, _categoryVm));
        }

        [TestMethod]
        public void TestGetChildCountFromService_Expected_RecursiveTotal()
        {
            var childCount = _serviceTypeVm.ChildrenCount;
            Assert.IsTrue(childCount == 1);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("ServiceTypeTreeViewModel_Add")]
        public void ServiceTypeTreeViewModel_Add_UnassignedResource_InsertedAtTop()
        {
            var eventAggregator = new Mock<IEventAggregator>().Object;
            var serviceTypeTreeViewModel = new ServiceTypeTreeViewModel(eventAggregator, null, ResourceType.WorkflowService);
            var existingChildren = new ObservableCollection<ITreeNode>
            {
                new CategoryTreeViewModel(eventAggregator, null, "aaa", ResourceType.WorkflowService),
                new CategoryTreeViewModel(eventAggregator, null, "zzz", ResourceType.WorkflowService)
            };
            serviceTypeTreeViewModel.Children = existingChildren;

            //------------Execute Test---------------------------
            serviceTypeTreeViewModel.Add(new CategoryTreeViewModel(eventAggregator, null, StringResources.Navigation_Category_Unassigned, ResourceType.WorkflowService));

            // Assert InsertedAtTop
            Assert.AreEqual(StringResources.Navigation_Category_Unassigned, serviceTypeTreeViewModel.Children[0].DisplayName);
        }

        #endregion ServiceType

        #region Category

        [TestMethod]
        public void AddChildExpectChildAdded()
        {
            var mockResource3 = new Mock<IContextualResourceModel>();
            mockResource3.Setup(r => r.ResourceType).Returns(ResourceType.Service);
            mockResource3.Setup(r => r.Category).Returns("Testing3");
            mockResource3.Setup(r => r.ResourceName).Returns("Mock3");
            mockResource3.Setup(r => r.Environment.Connection.ServerEvents).Returns(new Mock<IEventPublisher>().Object);

            var count = _categoryVm2.ChildrenCount;

            var toAdd = new ResourceTreeViewModel(new Mock<IEventAggregator>().Object, null, mockResource3.Object);

            _categoryVm2.Add(toAdd);

            Assert.IsTrue(_categoryVm2.ChildrenCount == count + 1);
            Assert.IsTrue(ReferenceEquals(toAdd.TreeParent, _categoryVm2));
        }

        [TestMethod]
        public void RemoveChildExpectChildRemoved()
        {
            var count = _categoryVm2.ChildrenCount;
            var toRemove = _categoryVm2.GetChildren(c => true).FirstOrDefault();
            _categoryVm2.Remove(toRemove);
            Assert.IsTrue(_categoryVm2.ChildrenCount == count - 1);
            Assert.IsTrue(toRemove.TreeParent == null);
        }

        [TestMethod]
        public void CategoryNodeEnvironmentModel_Expect_ParentEnvironmentModel()
        {
            var model = _categoryVm.EnvironmentModel;
            var model2 = _environmentVm.EnvironmentModel;
            Assert.IsTrue(ReferenceEquals(model, model2));
        }

        [TestMethod]
        public void CategoryNodeFindChildByName_Expected_RightChildNode()
        {
            var child = _categoryVm.FindChild("Mock");
            Assert.IsTrue(ReferenceEquals(child, _resourceVm));
        }

        [TestMethod]
        public void TestGetChildCountFromCategory_Expected_RecursiveTotal()
        {
            var childCount = _categoryVm.ChildrenCount;
            Assert.IsTrue(childCount == 1);
        }

        [TestMethod]
        [TestCategory("CategoryTreeViewModelUnitTest")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.Description("Test for CategoryTreeViewModel DisplayName: Displayname property is set to a valid resource category name ('new_category.var') and RenameCategory is expected to be called")]
        [Owner("Ashley Lewis")]
        // ReSharper disable InconsistentNaming
        public void CategoryTreeView_CategoryTreeViewModelUnitTest_DisplayNameBinding_RenameCategoryIsCalledOnce()
        // ReSharper restore InconsistentNaming
        {
            //MEF!!!
            ImportService.CurrentContext = CompositionInitializer.InitializeForMeflessBaseViewModel();

            var eventAggregator = new Mock<IEventAggregator>().Object;

            //Initialization
            var mockResourceRepo = new Mock<IResourceRepository>();
            mockResourceRepo.Setup(c => c.RenameCategory(It.IsAny<string>(), It.IsAny<string>(), ResourceType.WorkflowService)).Verifiable();

            var parent = new Mock<ServiceTypeTreeViewModel>(eventAggregator, null, ResourceType.WorkflowService);
            parent.Setup(model => model.EnvironmentModel.ResourceRepository).Returns(mockResourceRepo.Object);
            parent.Setup(model => model.Children).Returns(new ObservableCollection<ITreeNode>());

            //Execute
            var vm = new CategoryTreeViewModelMock(new Mock<IEventAggregator>().Object, parent.Object, "Test Category", ResourceType.WorkflowService) { TreeParent = parent.Object, DisplayName = "Renamed Test Category" };


            Assert.AreEqual(1, vm.RenameCategoryHitCount, "Rename category not called after display name change");
        }

        [TestMethod]
        [TestCategory("CategoryTreeViewModelUnitTest")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.Description("Test for CategoryTreeViewModel's RenameCategory function: ResourceRepository's RenameCategory function is expected to be called")]
        [Owner("Ashley Lewis")]
        // ReSharper disable InconsistentNaming
        public void CategoryTreeView_CategoryTreeViewModelUnitTest_RenameCategory_ResourceRepoRenameCategoryCalledAndReparentCalled()
        // ReSharper restore InconsistentNaming
        {
            //MEF!!!
            ImportService.CurrentContext = CompositionInitializer.InitializeForMeflessBaseViewModel();

            var eventAggregator = new Mock<IEventAggregator>().Object;

            //Initialization
            var mockResourceRepo = new Mock<IResourceRepository>();
            mockResourceRepo.Setup(c => c.RenameCategory(It.IsAny<string>(), It.IsAny<string>(), ResourceType.WorkflowService)).Verifiable();

            var parent = new Mock<ServiceTypeTreeViewModel>(eventAggregator, null, ResourceType.WorkflowService);
            parent.Setup(model => model.EnvironmentModel.ResourceRepository).Returns(mockResourceRepo.Object);
            parent.Setup(model => model.Children).Returns(new ObservableCollection<ITreeNode>());

            //Execute
            var vm = new CategoryTreeViewModelMock(eventAggregator, parent.Object, "Test Category", ResourceType.WorkflowService) { TreeParent = parent.Object, DisplayName = "Renamed Test Category" };

            //Assert
            mockResourceRepo.Verify(c => c.RenameCategory(It.IsAny<string>(), It.IsAny<string>(), ResourceType.WorkflowService), Times.Once(), "ResourceReposities RenameCategory function was not called after category tree view model rename resource was called");
            Assert.AreEqual(1, vm.ReparentHitCount, "Reparent not called during rename category");
        }

        #endregion Category

        #region Resource

        [TestMethod]
        public void TestGetChildCountFromResource_Expected_CountOfOne()
        {
            var childCount = _resourceVm.ChildrenCount;
            Assert.IsTrue(childCount == 1);
        }

        [TestMethod]
        public void ResourceNodeEnvironmentModel_Expect_ParentEnvironmentModel()
        {
            var model = _resourceVm.EnvironmentModel;
            var model2 = _environmentVm.EnvironmentModel;
            Assert.IsTrue(ReferenceEquals(model, model2));
        }

        [TestMethod]
        public void ResourceNodeDebugCommand_Expected_MediatorDebugResourceMessage()
        {
            _resourceVm.DebugCommand.Execute(null);
            _eventAggregator.Verify(e => e.Publish(It.Is<DebugResourceMessage>
              (t => t.Resource == _mockResourceModel.Object)), Times.Once());
        }

        [TestMethod]
        public void ResourceNodeDeleteCommand_With_Expected_EventAggregatorDeleteResourceMessage()
        {
            _resourceVm.DeleteCommand.Execute(null);
            _eventAggregator.Verify(e => e.Publish(It.IsAny<DeleteResourcesMessage>()), Times.Once());
        }


        [TestMethod]
        public void ResourceNodeManualEditCommand_With_Source_Expected_MediatorShowEditResourceWizardMessage()
        {
            _mockResourceModel.SetupGet(m => m.ResourceType).Returns(ResourceType.Source);
            _resourceVm.ManualEditCommand.Execute(null);
            _eventAggregator.Verify(e => e.Publish(It.Is<ShowEditResourceWizardMessage>
             (t => t.ResourceModel == _mockResourceModel.Object)), Times.Once());
        }

        [TestMethod]
        public void ResourceNodeManualEditCommand_With_WorkflowService_Expected_MediatorAddWorkflowDesignerMessage()
        {
            _mockResourceModel.SetupGet(m => m.ResourceType).Returns(ResourceType.WorkflowService);
            _resourceVm.ManualEditCommand.Execute(null);
            _eventAggregator.Verify(e => e.Publish(It.Is<AddWorkSurfaceMessage>
            (t => t.WorkSurfaceObject == _mockResourceModel.Object)), Times.Once());
        }

        [TestMethod]
        public void ResourceNodeManualEditCommand_With_Service_Expected_MediatorShowEditResourceWizardMessage()
        {
            _mockResourceModel.SetupGet(m => m.ResourceType).Returns(ResourceType.Service);
            _resourceVm.ManualEditCommand.Execute(null);
            _eventAggregator.Verify(e => e.Publish(It.Is<ShowEditResourceWizardMessage>
                    (t => t.ResourceModel == _mockResourceModel.Object)), Times.Once());
        }

        [TestMethod]
        public void ResourceNodeEditCommand_With_Source_Expected_MediatorShowEditResourceWizardMessage()
        {

            _mockResourceModel.SetupGet(m => m.ResourceType).Returns(ResourceType.Source);
            _resourceVm.EditCommand.Execute(null);
            _eventAggregator.Verify(e => e.Publish(It.Is<ShowEditResourceWizardMessage>
                   (t => t.ResourceModel == _mockResourceModel.Object)), Times.Once());
        }

        [TestMethod]
        public void ResourceNodeEditCommand_With_WorkflowService_Expected_MediatorAddWorkflowDesignerMessage()
        {

            _mockResourceModel.SetupGet(m => m.ResourceType).Returns(ResourceType.WorkflowService);
            _resourceVm.EditCommand.Execute(null);
            _eventAggregator.Verify(e => e.Publish(It.Is<AddWorkSurfaceMessage>
                  (t => t.WorkSurfaceObject == _mockResourceModel.Object)), Times.Once());
        }

        [TestMethod]
        public void ResourceNodeEditCommand_With_Service_Expected_MediatorShowEditResourceWizardMessage()
        {

            _mockResourceModel.SetupGet(m => m.ResourceType).Returns(ResourceType.Service);
            _resourceVm.EditCommand.Execute(null);
            _eventAggregator.Verify(e => e.Publish(It.Is<ShowEditResourceWizardMessage>
                 (t => t.ResourceModel == _mockResourceModel.Object)), Times.Once());
        }

        [TestMethod]
        public void ResourceNodeShowPropertiesCommand_Expected_MediatorShowEditResourceWizardMessage()
        {

            _mockResourceModel.SetupGet(m => m.ResourceType).Returns(ResourceType.Service);
            _resourceVm.ShowPropertiesCommand.Execute(null);
            _eventAggregator.Verify(e => e.Publish(It.Is<ShowEditResourceWizardMessage>
                 (t => t.ResourceModel == _mockResourceModel.Object)), Times.Once());

            _mockResourceModel.SetupGet(m => m.ResourceType).Returns(ResourceType.WorkflowService);
            _resourceVm.ShowPropertiesCommand.Execute(null);

            _eventAggregator.Verify(e => e.Publish(It.Is<ShowEditResourceWizardMessage>
                 (t => t.ResourceModel == _mockResourceModel.Object)), Times.AtLeastOnce());
            _mockResourceModel.SetupGet(m => m.ResourceType).Returns(ResourceType.Source);
            _resourceVm.ShowPropertiesCommand.Execute(null);

            _eventAggregator.Verify(e => e.Publish(It.Is<ShowEditResourceWizardMessage>
                 (t => t.ResourceModel == _mockResourceModel.Object)), Times.AtLeastOnce());
        }

        [TestMethod]
        public void ResourceNodeShowDependenciesCommand_Expected_EventAggregatorShowDependencyGraphMessage()
        {
            _resourceVm.ShowDependenciesCommand.Execute(null);

            _eventAggregator.Verify(e => e.Publish(It.Is<ShowDependenciesMessage>
                (t => t.ResourceModel == _mockResourceModel.Object)), Times.Once());
        }

        [TestMethod]
        public void ResourceNodeBuildCommand_Expected_EditCommandExecuted_And_MediatorSaveResourceMessage()
        {
            _resourceVm.BuildCommand.Execute(null);
            _eventAggregator.Verify(e => e.Publish(It.Is<SaveResourceMessage>
                 (t => t.Resource == _mockResourceModel.Object)), Times.Once());
        }

        [TestMethod]
        public void EditCommand_GivenAResourceOfTypeWorkflowService_Expected_AddWorkflowDesignerMediatorMessage()
        {
            _mockResourceModel.Setup(r => r.ResourceType).Returns(ResourceType.WorkflowService);
            _resourceVm.EditCommand.Execute(_mockResourceModel);
            _eventAggregator.Verify(e => e.Publish(It.Is<AddWorkSurfaceMessage>
                (t => t.WorkSurfaceObject == _mockResourceModel.Object)), Times.Once());
        }


        [TestMethod]
        public void EditCommand_GivenAResourceOfTypeSource_Expected_ShowEditResourceWizardMediatorMessage()
        {
            _mockResourceModel.Setup(r => r.ResourceType).Returns(ResourceType.Source);
            _resourceVm.EditCommand.Execute(_mockResourceModel);
            _eventAggregator.Verify(e => e.Publish(It.Is<ShowEditResourceWizardMessage>
                (t => t.ResourceModel == _mockResourceModel.Object)), Times.Once());
        }

        [TestMethod]
        public void EditCommand_GivenAResourceOfTypeService_Expected_ShowEditResourceWizardMediatorMessage()
        {
            _mockResourceModel.Setup(r => r.ResourceType).Returns(ResourceType.Service);
            _resourceVm.EditCommand.Execute(_mockResourceModel);
            _eventAggregator.Verify(e => e.Publish(It.Is<ShowEditResourceWizardMessage>
                (t => t.ResourceModel == _mockResourceModel.Object)), Times.Once());
        }

        //
        //Juries - Bug 8427
        //
        [TestMethod]
        public void ResourceParentCheckedDoesNotCheckFilteredChildren()
        {
            var mockResource3 = new Mock<IContextualResourceModel>();
            mockResource3.Setup(r => r.ResourceType).Returns(ResourceType.Service);
            mockResource3.Setup(r => r.Category).Returns("Testing3");
            mockResource3.Setup(r => r.ResourceName).Returns("Mock3");
            mockResource3.Setup(r => r.Environment.Connection.ServerEvents).Returns(new Mock<IEventPublisher>().Object);

            var toAdd = new ResourceTreeViewModel(new Mock<IEventAggregator>().Object, _categoryVm2, mockResource3.Object);

            _categoryVm2.IsChecked = true;
            Assert.IsTrue(_categoryVm2.Children.Count(c => c.IsChecked == true) == 2);

            _categoryVm2.IsChecked = false;
            _rootVm.FilterText = "Mock3";
            _categoryVm2.IsChecked = true;

            Assert.IsTrue(_categoryVm2.Children.Count(c => c.IsChecked == true) == 1);
        }

        //
        //Juries - Bug 8427
        //
        [TestMethod]
        public void FilterChangedResultingInItemNotFilteredUpdatesParentState()
        {
            var mockResource3 = new Mock<IContextualResourceModel>();
            mockResource3.Setup(r => r.ResourceType).Returns(ResourceType.Service);
            mockResource3.Setup(r => r.Category).Returns("Testing2");
            mockResource3.Setup(r => r.ResourceName).Returns("Mock3");
            mockResource3.Setup(r => r.Environment.Connection.ServerEvents).Returns(new Mock<IEventPublisher>().Object);

            var toAdd = new ResourceTreeViewModel(new Mock<IEventAggregator>().Object, _categoryVm2, mockResource3.Object);

            Thread.Sleep(100);

            Assert.IsTrue(_categoryVm2.ChildrenCount == 2);

            toAdd.IsChecked = true;
            _rootVm.FilterText = "Mock3";
            _rootVm.NotifyOfFilterPropertyChanged(false);

            Thread.Sleep(100);

            Assert.IsTrue(_categoryVm2.IsChecked == true);

            _rootVm.FilterText = "";
            _rootVm.NotifyOfFilterPropertyChanged(false);

            Thread.Sleep(100);

            Assert.IsTrue(_categoryVm2.IsChecked == null);
        }

        [TestMethod]
        [TestCategory("ResourceTreeViewModelUnitTest")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.Description("Test for implementation of 'IDataErrorInfo': The IDataErrorInfo error message is set and then recovered from a resource tree view model")]
        [Owner("Ashley Lewis")]
        // ReSharper disable InconsistentNaming
        public void ResourceTreeViewModel_ResourceTreeViewModelUnitTest_IDataErrorInfoImplimentation_ReturnsItsErrorMessage()
        // ReSharper restore InconsistentNaming
        {
            //init
            var mockResource3 = new Mock<IContextualResourceModel>();
            mockResource3.Setup(r => r.Environment.Connection.ServerEvents).Returns(new Mock<IEventPublisher>().Object);

            var vm = new ResourceTreeViewModel(new Mock<IEventAggregator>().Object, _categoryVm, mockResource3.Object);
            var testError = new ErrorInfo { Message = "Test IDataErrorInfo Message" };
            vm.DataContext = new ResourceModel(ResourceModelTest.CreateMockEnvironment().Object);
            vm.DataContext.AddError(testError);

            var displayName = vm["DisplayName"];
            //assert on execute
            Assert.AreEqual("Test IDataErrorInfo Message", vm["DisplayName"], "ResourceTreeViewModel was not able to return its error message correctly");
        }

        [TestMethod]
        [TestCategory("ResourceTreeViewModelUnitTest")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.Description("Test for implementation of 'OnDesignValidationReceived': OnDesignValidationReceived is called and a new error message is expected to be added to data context")]
        [Owner("Ashley Lewis")]
        // ReSharper disable InconsistentNaming
        public void ResourceTreeViewModel_ResourceTreeViewModelUnitTest_OnDesignValidationReceived_ErrorNotAddedToDataContext()
        // ReSharper restore InconsistentNaming
        {
            //init
            var mockResource3 = new Mock<IContextualResourceModel>();
            mockResource3.Setup(r => r.Environment.Connection.ServerEvents).Returns(new Mock<IEventPublisher>().Object);

            var vm = new MockResourceTreeViewModel(new Mock<IEventAggregator>().Object, _categoryVm, mockResource3.Object) { DataContext = new TestResourceModel() };
            var memo = new DesignValidationMemo();
            var testError = new ErrorInfo { Message = "Test Error Message" };
            memo.Errors.Add(testError);
            vm.TestDesignValidationReceived(memo);

            //assert on execute
            Assert.AreEqual(0, vm.DataContext.Errors.Count, "OnDesignValidationReceived added error to datacontext");
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("ResourceTreeViewModel_RemoveNode")]
        public void ResourceTreeViewModel_RemoveNode_OnlyResourceInItsType_CategoryNodeAndTypeNodeRemoved()
        {
            var eventAggregator = new Mock<IEventAggregator>().Object;

            // ReSharper disable ObjectCreationAsStatement
            var vmRoot = new RootTreeViewModel(eventAggregator);
            var envModel = new Mock<IEnvironmentModel>();
            envModel.Setup(e => e.Connection).Returns(new Mock<IEnvironmentConnection>().Object);

            new EnvironmentTreeViewModel(eventAggregator, vmRoot, envModel.Object, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object);
            new ServiceTypeTreeViewModel(eventAggregator, vmRoot.Children[0], ResourceType.WorkflowService);
            new CategoryTreeViewModel(eventAggregator, vmRoot.Children[0].Children[0], "TESTCATEGORY", ResourceType.WorkflowService);

            var mockResource3 = new Mock<IContextualResourceModel>();
            mockResource3.Setup(r => r.Environment.Connection.ServerEvents).Returns(new Mock<IEventPublisher>().Object);

            var danglingNode = new ResourceTreeViewModel(eventAggregator, vmRoot.Children[0].Children[0].Children[0], mockResource3.Object);
            // ReSharper restore ObjectCreationAsStatement

            //------------Execute Test---------------------------
            danglingNode.TreeParent.Remove(danglingNode);

            // Assert Category Node And Type Node Removed
            Assert.AreEqual(0, vmRoot.Children[0].Children.Count, "Dangling nodes below environment not removed");
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("ResourceTreeViewModel_Rename")]
        public void ResourceTreeViewModel_Rename_ResourceWithTheSameNameAlreadyExistsResourceNotRenamed()
        {
            var oldResourceID = Guid.NewGuid();
            const string newResourceName = "NameOfExistingResource";

            var eventAggregator = new Mock<IEventAggregator>().Object;

            //Mock resource repository
            var mockedResourceRepo = new Mock<IResourceRepository>();
            var mockedEnvironment = new Mock<IEnvironmentModel>();
            var mockedResourceModel = new Mock<IContextualResourceModel>();
            var allExistingResources = new Collection<IResourceModel>();
            mockedResourceModel.Setup(res => res.ID).Returns(oldResourceID);
            mockedResourceModel.Setup(r => r.Environment.Connection.ServerEvents).Returns(new Mock<IEventPublisher>().Object);

            allExistingResources.Add(new ResourceModel(mockedEnvironment.Object) { ResourceName = newResourceName });
            mockedResourceRepo.Setup(repo => repo.Rename(oldResourceID.ToString(), newResourceName)).Verifiable();
            mockedResourceRepo.Setup(repo => repo.All()).Returns(allExistingResources);
            mockedEnvironment.Setup(env => env.ResourceRepository).Returns(mockedResourceRepo.Object);
            mockedEnvironment.Setup(e => e.Connection).Returns(new Mock<IEnvironmentConnection>().Object);

            //Isolate rename resource unit
            var treeParent = new EnvironmentTreeViewModel(eventAggregator, null, mockedEnvironment.Object, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object);
            var resourcetreeviewmodel = new MockResourceTreeViewModel(eventAggregator, treeParent, mockedResourceModel.Object);

            //------------Execute Test---------------------------
            resourcetreeviewmodel.TestRename(newResourceName);

            // Assert Dialog Shown
            mockedResourceRepo.Verify(repo => repo.Rename(oldResourceID.ToString(), newResourceName), Times.Never(), "Resource repository rename resource was called dispite a resource with the same name already there");
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("ResourceTreeViewModel_Rename")]
        public void ResourceTreeViewModel_Rename_WithADash_ResourceRepositoryRenameCalled()
        {
            var oldResourceID = Guid.NewGuid();
            const string newResourceName = "new-display-name-with-dashes";

            //Mock resource repository
            var mockedResourceRepo = new Mock<IResourceRepository>();
            var mockedEnvironment = new Mock<IEnvironmentModel>();
            var mockedResourceModel = new Mock<IContextualResourceModel>();
            mockedResourceModel.Setup(res => res.ID).Returns(oldResourceID);
            mockedResourceModel.Setup(r => r.Environment.Connection.ServerEvents).Returns(new Mock<IEventPublisher>().Object);

            mockedResourceRepo.Setup(repo => repo.All()).Returns(new Collection<IResourceModel>());
            mockedResourceRepo.Setup(repo => repo.Rename(oldResourceID.ToString(), newResourceName)).Verifiable();
            mockedEnvironment.Setup(env => env.ResourceRepository).Returns(mockedResourceRepo.Object);
            mockedEnvironment.Setup(e => e.Connection).Returns(new Mock<IEnvironmentConnection>().Object);

            var eventAggregator = new Mock<IEventAggregator>().Object;

            //Isolate rename resource unit
            var treeParent = new EnvironmentTreeViewModel(eventAggregator, null, mockedEnvironment.Object, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object);
            var resourcetreeviewmodel = new MockResourceTreeViewModel(eventAggregator, treeParent, mockedResourceModel.Object);

            //------------Execute Test---------------------------
            resourcetreeviewmodel.TestRename(newResourceName);

            // Assert Resource Repository Rename Called
            mockedResourceRepo.Verify(repo => repo.Rename(oldResourceID.ToString(), newResourceName), Times.Once(), "Resource repository rename resource was not called with the correct params"); ;
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("ResourceTreeViewModel_Rename")]
        public void ResourceTreeViewModel_Rename_ResourceRenamedBackToOldName_ResourceRenamed()
        {
            var oldResourceID = Guid.NewGuid();
            const string oldResourceName = "OldName";
            const string newResourceName = "NewName";

            //Mock resource repository
            var mockedResourceRepo = new Mock<IResourceRepository>();
            var mockedEnvironment = new Mock<IEnvironmentModel>();
            var mockedResourceModel = new Mock<IContextualResourceModel>();
            mockedResourceModel.Setup(res => res.ID).Returns(oldResourceID);
            mockedResourceModel.Setup(res => res.ResourceName).Returns(oldResourceName);
            mockedResourceModel.Setup(r => r.Environment.Connection.ServerEvents).Returns(new Mock<IEventPublisher>().Object);

            mockedResourceRepo.Setup(repo => repo.All()).Returns(new Collection<IResourceModel>());
            mockedResourceRepo.Setup(repo => repo.Rename(oldResourceID.ToString(), newResourceName)).Verifiable();
            mockedResourceRepo.Setup(repo => repo.Rename(oldResourceID.ToString(), oldResourceName)).Verifiable();
            mockedEnvironment.Setup(env => env.ResourceRepository).Returns(mockedResourceRepo.Object);
            mockedEnvironment.Setup(e => e.Connection).Returns(new Mock<IEnvironmentConnection>().Object);

            var eventAggregator = new Mock<IEventAggregator>().Object;

            //Isolate rename resource unit
            var treeParent = new EnvironmentTreeViewModel(eventAggregator, null, mockedEnvironment.Object, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object);
            var resourcetreeviewmodel = new MockResourceTreeViewModel(eventAggregator, treeParent, mockedResourceModel.Object);

            //------------Execute Test---------------------------
            resourcetreeviewmodel.TestRename(newResourceName);
            resourcetreeviewmodel.TestRename(oldResourceName);

            // Assert resource renamed both times
            mockedResourceRepo.Verify(repo => repo.Rename(oldResourceID.ToString(), newResourceName), Times.Once(), "Resource repository rename resource was not called with the correct params");
            mockedResourceRepo.Verify(repo => repo.Rename(oldResourceID.ToString(), oldResourceName), Times.Once(), "Resource repository rename resource was not called with the correct params");
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("ResourceTreeViewModel_SetIsChecked")]
        public void ResourceTreeViewModel_SetIsChecked_SettingIsCheckToFalseFromTrue_IsOverwriteGetsSetToFalseOnResource()
        {
            //------------Setup for test--------------------------
            var resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(r => r.ServerResourceType).Returns("Workflow");
            resourceModel.Setup(r => r.Environment.Connection.ServerEvents).Returns(new Mock<IEventPublisher>().Object);
            var cvm = new CategoryTreeViewModel(new Mock<IEventAggregator>().Object, null, "test", ResourceType.WorkflowService);
            var rvm = new ResourceTreeViewModel(new Mock<IEventAggregator>().Object, null, resourceModel.Object);
            cvm.Children.Add(rvm);
            rvm.TreeParent = cvm;
            rvm.IsOverwrite = true;
            cvm.IsOverwrite = true;
            rvm.IsChecked = true;
            Assert.AreEqual(true, rvm.IsOverwrite);
            Assert.AreEqual(true, cvm.IsOverwrite);
            //------------Execute Test---------------------------
            rvm.IsChecked = false;
            //------------Assert Results-------------------------
            Assert.AreEqual(false, rvm.IsOverwrite);
            Assert.AreEqual(false, cvm.IsOverwrite);
        }

        #endregion Resource

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ResourceTreeViewModel_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        // ReSharper disable InconsistentNaming
        public void ResourceTreeViewModel_Constructor_NullEventAggregator_ThrowsException()
        // ReSharper restore InconsistentNaming
        {
            var tvm = new ResourceTreeViewModel(null, null, new Mock<IContextualResourceModel>().Object);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ResourceTreeViewModel_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        // ReSharper disable InconsistentNaming
        public void ResourceTreeViewModel_Constructor_NullDataContext_ThrowsException()
        // ReSharper restore InconsistentNaming
        {
            var tvm = new ResourceTreeViewModel(new Mock<IEventAggregator>().Object, null, null);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ResourceTreeViewModel_Constructor")]
        public void ResourceTreeViewModel_Constructor_ServerResourceTypeIsDbService_ActivityFullNameIsDsfDatabaseActivity()
        {
            ResourceTreeViewModel_Verify_ActivityFullName("DbService", typeof(DsfDatabaseActivity));
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ResourceTreeViewModel_Constructor")]
        public void ResourceTreeViewModel_Constructor_ServerResourceTypeIsPluginService_ActivityFullNameIsDsfPluginActivity()
        {
            ResourceTreeViewModel_Verify_ActivityFullName("PluginService", typeof(DsfPluginActivity));
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ResourceTreeViewModel_Constructor")]
        public void ResourceTreeViewModel_Constructor_ServerResourceTypeIsEmptyOrNull_ActivityFullNameIsDsfActivity()
        {
            ResourceTreeViewModel_Verify_ActivityFullName("", typeof(DsfActivity));
            ResourceTreeViewModel_Verify_ActivityFullName(null, typeof(DsfActivity));
        }

        void ResourceTreeViewModel_Verify_ActivityFullName(string serverResourceType, Type expectedType)
        {
            //------------Setup for test--------------------------
            var resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(r => r.ServerResourceType).Returns(serverResourceType);
            resourceModel.Setup(r => r.Environment.Connection.ServerEvents).Returns(new Mock<IEventPublisher>().Object);

            //------------Execute Test---------------------------
            var tvm = new ResourceTreeViewModel(new Mock<IEventAggregator>().Object, null, resourceModel.Object);

            //------------Assert Results-------------------------
            Assert.AreEqual(expectedType.AssemblyQualifiedName, tvm.ActivityFullName);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ResourceTreeViewModel_Constructor")]
        public void ResourceTreeViewModel_Constructor_SubscribesToDesignValidationService()
        {
            //------------Setup for test--------------------------
            var eventPublisher = new EventPublisher();

            var resourceID = Guid.NewGuid();
            var resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(r => r.ID).Returns(resourceID);
            resourceModel.Setup(r => r.Environment.Connection.ServerEvents).Returns(eventPublisher);

            //------------Execute Test---------------------------
            var tvm = new MockResourceTreeViewModel(new Mock<IEventAggregator>().Object, null, resourceModel.Object);

            eventPublisher.Publish(new DesignValidationMemo { InstanceID = resourceID });

            //------------Assert Results-------------------------
            Assert.AreEqual(1, tvm.OnDesignValidationReceivedHitCount);

            tvm.Dispose();
        }

        [TestMethod]
        [TestCategory("TreeViewModelFactory_Create")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.Description("TreeViewModelFactory must assign a AssemblyQualifiedName based on the ServerResourceType to the ResourceTreeViewModel.")]
        [Owner("Trevor Williams-Ros")]
        public void TreeViewModelFactory_UnitTest_AssemblyQualifiedNameMatchesServerResourceType_DbService()
        {
            var mockEventPublisher = new Mock<IEventPublisher>();
            mockEventPublisher.Setup(publisher => publisher.GetEvent<DesignValidationMemo>()).Returns(new Mock<IObservable<DesignValidationMemo>>().Object);
            var mockEnvironmenConnection = new Mock<IEnvironmentConnection>();
            mockEnvironmenConnection.Setup(connection => connection.ServerEvents).Returns(mockEventPublisher.Object);
            var mockEnvironment = new Mock<IEnvironmentModel>();
            mockEnvironment.Setup(model => model.Connection).Returns(mockEnvironmenConnection.Object);
            var dbServiceModel = new Mock<IContextualResourceModel>();
            dbServiceModel.Setup(m => m.ResourceType).Returns(ResourceType.Service);
            dbServiceModel.Setup(m => m.ServerResourceType).Returns("DbService");
            dbServiceModel.Setup(m => m.Environment).Returns(mockEnvironment.Object);
            dbServiceModel.Setup(m => m.ID).Returns(Guid.NewGuid);

            var eventAggregator = new Mock<IEventAggregator>().Object;
            var dbServiceTvm = new ResourceTreeViewModel(eventAggregator, null, dbServiceModel.Object);
            Assert.AreEqual(typeof(DsfDatabaseActivity).AssemblyQualifiedName, dbServiceTvm.ActivityFullName, "TreeViewModelFactory.Create did not assign database activity type correctly");

            var otherModel = new Mock<IContextualResourceModel>();
            otherModel.Setup(m => m.ResourceType).Returns(ResourceType.WorkflowService);
            otherModel.Setup(m => m.ServerResourceType).Returns("xxx");
            otherModel.Setup(m => m.Environment).Returns(mockEnvironment.Object);
            otherModel.Setup(m => m.ID).Returns(Guid.NewGuid);

            var otherTvm = new ResourceTreeViewModel(eventAggregator, null, otherModel.Object);
            Assert.AreEqual(typeof(DsfActivity).AssemblyQualifiedName, otherTvm.ActivityFullName, "TreeViewModelFactory.Create did not assign DSF activity type correctly");

        }

        [TestMethod]
        [TestCategory("TreeViewModelFactory_Create")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.Description("TreeViewModelFactory must assign a AssemblyQualifiedName based on the ServerResourceType to the ResourceTreeViewModel.")]
        [Owner("Huggs")]
        public void TreeViewModelFactory_UnitTest_AssemblyQualifiedNameMatchesServerResourceType_PluginService()
        {
            var mockEventPublisher = new Mock<IEventPublisher>();
            mockEventPublisher.Setup(publisher => publisher.GetEvent<DesignValidationMemo>()).Returns(new Mock<IObservable<DesignValidationMemo>>().Object);
            var mockEnvironmenConnection = new Mock<IEnvironmentConnection>();
            mockEnvironmenConnection.Setup(connection => connection.ServerEvents).Returns(mockEventPublisher.Object);
            var mockEnvironment = new Mock<IEnvironmentModel>();
            mockEnvironment.Setup(model => model.Connection).Returns(mockEnvironmenConnection.Object);
            var dbServiceModel = new Mock<IContextualResourceModel>();
            dbServiceModel.Setup(m => m.ResourceType).Returns(ResourceType.Service);
            dbServiceModel.Setup(m => m.ServerResourceType).Returns("PluginService");
            dbServiceModel.Setup(m => m.Environment).Returns(mockEnvironment.Object);
            dbServiceModel.Setup(m => m.ID).Returns(Guid.NewGuid);

            var eventAggregator = new Mock<IEventAggregator>().Object;
            var dbServiceTvm = new ResourceTreeViewModel(eventAggregator, null, dbServiceModel.Object);
            Assert.AreEqual(typeof(DsfPluginActivity).AssemblyQualifiedName, dbServiceTvm.ActivityFullName, "TreeViewModelFactory.Create did not assign database activity type correctly");

            var otherModel = new Mock<IContextualResourceModel>();
            otherModel.Setup(m => m.ResourceType).Returns(ResourceType.WorkflowService);
            otherModel.Setup(m => m.ServerResourceType).Returns("xxx");
            otherModel.Setup(m => m.Environment).Returns(mockEnvironment.Object);
            otherModel.Setup(m => m.ID).Returns(Guid.NewGuid);

            var otherTvm = new ResourceTreeViewModel(eventAggregator, null, otherModel.Object);
            Assert.AreEqual(typeof(DsfActivity).AssemblyQualifiedName, otherTvm.ActivityFullName, "TreeViewModelFactory.Create did not assign DSF activity type correctly");

        }



        [TestMethod]
        [Microsoft.VisualStudio.TestTools.UnitTesting.Description("EnvironmentTreeViewModel must attach NetworkStateChanged event handler to raise IsConnected property changed event.")]
        [TestCategory("EnvironmentTreeViewModel_EnvironmentModel")]
        [Owner("Trevor Williams-Ros")]
        // ReSharper disable InconsistentNaming
        public void EnvironmentTreeViewModel_UnitTest_EnvironmentModelNetworkStateChanged_RaisesIsConnectedPropertyChanged()
        // ReSharper restore InconsistentNaming
        {
            var envModel = new Mock<IEnvironmentModel>();
            envModel.Setup(e => e.Connection).Returns(new Mock<IEnvironmentConnection>().Object);

            var envTreeViewModel = new EnvironmentTreeViewModel(new Mock<IEventAggregator>().Object, _rootVm, envModel.Object, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object);
            envTreeViewModel.PropertyChanged += (sender, args) =>
            {
                Assert.AreEqual("IsConnected", args.PropertyName, "EnvironmentTreeViewModel did not raise IsConnected property changed event.");
            };
            envModel.Raise(c => c.IsConnectedChanged += null, new ConnectedEventArgs());

        }


        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ResourceTreeViewModel_CanProperties")]
        public void ResourceTreeViewModel_CanProperties_ValidResourceTypesAndIsAuthorized_True()
        {
            foreach(var resourceType in GetResourceTypes(true))
            {
                foreach(ResourceTreeViewModelProperty property in Enum.GetValues(typeof(ResourceTreeViewModelProperty)))
                {
                    Verify_ResourceTreeViewModel_CanProperties(property, resourceType, isAuthorized: true, expected: true);
                }
            }
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ResourceTreeViewModel_CanProperties")]
        public void ResourceTreeViewModel_CanProperties_ValidResourceTypesAndIsNotAuthorized_False()
        {
            foreach(var resourceType in GetResourceTypes(true))
            {
                foreach(ResourceTreeViewModelProperty property in Enum.GetValues(typeof(ResourceTreeViewModelProperty)))
                {
                    Verify_ResourceTreeViewModel_CanProperties(property, resourceType, isAuthorized: false, expected: false);
                }
            }
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ResourceTreeViewModel_CanProperties")]
        public void ResourceTreeViewModel_CanProperties_InvalidResourceTypesAndIsAuthorized_False()
        {
            foreach(var resourceType in GetResourceTypes(false))
            {
                foreach(ResourceTreeViewModelProperty property in Enum.GetValues(typeof(ResourceTreeViewModelProperty)))
                {
                    Verify_ResourceTreeViewModel_CanProperties(property, resourceType, isAuthorized: true, expected: false);
                }
            }
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ResourceTreeViewModel_CanProperties")]
        public void ResourceTreeViewModel_CanProperties_InvalidResourceTypesAndIsNotAuthorized_False()
        {
            foreach(var resourceType in GetResourceTypes(false))
            {
                foreach(ResourceTreeViewModelProperty property in Enum.GetValues(typeof(ResourceTreeViewModelProperty)))
                {
                    Verify_ResourceTreeViewModel_CanProperties(property, resourceType, isAuthorized: false, expected: false);
                }
            }
        }

        static void Verify_ResourceTreeViewModel_CanProperties(ResourceTreeViewModelProperty property, ResourceType resourceType, bool isAuthorized, bool expected)
        {
            //------------Setup for test--------------------------

            var resourceID = Guid.NewGuid();
            var resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(r => r.Environment.Connection.ServerEvents).Returns(new Mock<IEventPublisher>().Object);
            resourceModel.Setup(r => r.ID).Returns(resourceID);
            resourceModel.Setup(r => r.ResourceType).Returns(resourceType);
            resourceModel.Setup(r => r.IsAuthorized(AuthorizationContext.Contribute)).Returns(isAuthorized);
            resourceModel.Setup(r => r.IsAuthorized(AuthorizationContext.DeployFrom)).Returns(isAuthorized);
            resourceModel.Setup(r => r.IsAuthorized(AuthorizationContext.View)).Returns(isAuthorized);

            var tvm = new MockResourceTreeViewModel(new Mock<IEventAggregator>().Object, null, resourceModel.Object);

            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            switch(property)
            {
                case ResourceTreeViewModelProperty.CanDelete:
                    Assert.AreEqual(expected, tvm.CanDelete);
                    break;
                case ResourceTreeViewModelProperty.CanEdit:
                    Assert.AreEqual(expected, tvm.CanEdit);
                    break;
                case ResourceTreeViewModelProperty.CanManualEdit:
                    Assert.AreEqual(expected, tvm.CanManualEdit);
                    break;
                case ResourceTreeViewModelProperty.CanRename:
                    Assert.AreEqual(expected, tvm.CanRename);
                    break;
                case ResourceTreeViewModelProperty.CanDuplicate:
                    Assert.AreEqual(expected, tvm.CanDuplicate);
                    break;
                case ResourceTreeViewModelProperty.CanMoveRename:
                    Assert.AreEqual(expected, tvm.CanMoveRename);
                    break;
                case ResourceTreeViewModelProperty.CanDeploy:
                    Assert.AreEqual(expected, tvm.CanDeploy);
                    break;
                case ResourceTreeViewModelProperty.CanShowDependencies:
                    Assert.AreEqual(expected, tvm.CanShowDependencies);
                    break;
            }

            tvm.Dispose();
        }

        static IEnumerable<ResourceType> GetResourceTypes(bool isValidType)
        {
            return Enum.GetValues(typeof(ResourceType)).Cast<ResourceType>().Where(r => isValidType
                ? r == ResourceType.WorkflowService || r == ResourceType.Service || r == ResourceType.Source
                : !(r == ResourceType.WorkflowService || r == ResourceType.Service || r == ResourceType.Source));
        }

        enum ResourceTreeViewModelProperty
        {
            CanDelete,
            CanEdit,
            CanManualEdit,
            CanRename,
            CanDuplicate,
            CanMoveRename,
            CanDeploy,
            CanShowDependencies
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("CategoryTreeViewModel_CanProperties")]
        public void CategoryTreeViewModel_CanProperties_InvalidResourceTypesAndIsNotAuthorized_False()
        {
            foreach(var resourceType in GetResourceTypes(false))
            {
                foreach(CategoryTreeViewModelProperty property in Enum.GetValues(typeof(CategoryTreeViewModelProperty)))
                {
                    Verify_CategoryTreeViewModel_CanProperties(property, resourceType, isAuthorized: false, expected: false);
                }
            }
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("CategoryTreeViewModel_CanProperties")]
        public void CategoryTreeViewModel_CanProperties_InvalidResourceTypesAndIsAuthorized_False()
        {
            foreach(var resourceType in GetResourceTypes(false))
            {
                foreach(CategoryTreeViewModelProperty property in Enum.GetValues(typeof(CategoryTreeViewModelProperty)))
                {
                    Verify_CategoryTreeViewModel_CanProperties(property, resourceType, isAuthorized: true, expected: false);
                }
            }
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("CategoryTreeViewModel_CanProperties")]
        public void CategoryTreeViewModel_CanProperties_ValidResourceTypesAndIsAuthorized_True()
        {
            foreach(var resourceType in GetResourceTypes(true))
            {
                foreach(CategoryTreeViewModelProperty property in Enum.GetValues(typeof(CategoryTreeViewModelProperty)))
                {
                    Verify_CategoryTreeViewModel_CanProperties(property, resourceType, isAuthorized: true, expected: true);
                }
            }
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("CategoryTreeViewModel_CanProperties")]
        public void CategoryTreeViewModel_CanProperties_ValidResourceTypesAndIsNotAuthorized_False()
        {
            foreach(var resourceType in GetResourceTypes(true))
            {
                foreach(CategoryTreeViewModelProperty property in Enum.GetValues(typeof(CategoryTreeViewModelProperty)))
                {
                    Verify_CategoryTreeViewModel_CanProperties(property, resourceType, isAuthorized: false, expected: false);
                }
            }
        }

        static void Verify_CategoryTreeViewModel_CanProperties(CategoryTreeViewModelProperty property, ResourceType resourceType, bool isAuthorized, bool expected)
        {
            //------------Setup for test--------------------------
            var environmentModel = new Mock<IEnvironmentModel>();
            environmentModel.Setup(e => e.IsAuthorizedDeployFrom).Returns(isAuthorized);

            var displayName = string.Format("{0}s", resourceType == ResourceType.WorkflowService ? "Workflow" : resourceType.ToString()).ToUpper();

            var parent = new Mock<ITreeNode>();
            parent.Setup(p => p.EnvironmentModel).Returns(environmentModel.Object);
            parent.Setup(p => p.DisplayName).Returns(displayName);

            var tvm = new CategoryTreeViewModel(new Mock<IEventAggregator>().Object, null, "test", resourceType);
            tvm.TreeParent = parent.Object;

            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            switch(property)
            {
                case CategoryTreeViewModelProperty.CanDeploy:
                    Assert.AreEqual(expected, tvm.CanDeploy, string.Format("{0} {1}({2}) WHEN IsAuthorized={3}", property, resourceType, displayName, isAuthorized));
                    break;
            }

            tvm.Dispose();
        }

        enum CategoryTreeViewModelProperty
        {
            CanDeploy,
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ServiceTypeTreeViewModel_CanProperties")]
        public void ServiceTypeTreeViewModel_CanProperties_InvalidResourceTypesAndIsNotAuthorized_False()
        {
            foreach(var resourceType in GetResourceTypes(false))
            {
                foreach(ServiceTypeTreeViewModelProperty property in Enum.GetValues(typeof(ServiceTypeTreeViewModelProperty)))
                {
                    Verify_ServiceTypeTreeViewModel_CanProperties(property, resourceType, isAuthorized: false, expected: false);
                }
            }
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ServiceTypeTreeViewModel_CanProperties")]
        public void ServiceTypeTreeViewModel_CanProperties_InvalidResourceTypesAndIsAuthorized_False()
        {
            foreach(var resourceType in GetResourceTypes(false))
            {
                foreach(ServiceTypeTreeViewModelProperty property in Enum.GetValues(typeof(ServiceTypeTreeViewModelProperty)))
                {
                    Verify_ServiceTypeTreeViewModel_CanProperties(property, resourceType, isAuthorized: true, expected: false);
                }
            }
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ServiceTypeTreeViewModel_CanProperties")]
        public void ServiceTypeTreeViewModel_CanProperties_ValidResourceTypesAndIsAuthorized_True()
        {
            foreach(var resourceType in GetResourceTypes(true))
            {
                foreach(ServiceTypeTreeViewModelProperty property in Enum.GetValues(typeof(ServiceTypeTreeViewModelProperty)))
                {
                    Verify_ServiceTypeTreeViewModel_CanProperties(property, resourceType, isAuthorized: true, expected: true);
                }
            }
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ServiceTypeTreeViewModel_CanProperties")]
        public void ServiceTypeTreeViewModel_CanProperties_ValidResourceTypesAndIsNotAuthorized_False()
        {
            foreach(var resourceType in GetResourceTypes(true))
            {
                foreach(ServiceTypeTreeViewModelProperty property in Enum.GetValues(typeof(ServiceTypeTreeViewModelProperty)))
                {
                    Verify_ServiceTypeTreeViewModel_CanProperties(property, resourceType, isAuthorized: false, expected: false);
                }
            }
        }

        static void Verify_ServiceTypeTreeViewModel_CanProperties(ServiceTypeTreeViewModelProperty property, ResourceType resourceType, bool isAuthorized, bool expected)
        {
            //------------Setup for test--------------------------
            var environmentModel = new Mock<IEnvironmentModel>();
            environmentModel.Setup(e => e.IsAuthorizedDeployFrom).Returns(isAuthorized);

            var parent = new Mock<ITreeNode>();
            parent.Setup(p => p.EnvironmentModel).Returns(environmentModel.Object);

            var tvm = new ServiceTypeTreeViewModel(new Mock<IEventAggregator>().Object, null, resourceType);
            tvm.TreeParent = parent.Object;

            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            switch(property)
            {
                case ServiceTypeTreeViewModelProperty.CanDeploy:
                    Assert.AreEqual(expected, tvm.CanDeploy, string.Format("{0} {1} WHEN IsAuthorized={2}", property, resourceType, isAuthorized));
                    break;
            }

            tvm.Dispose();
        }

        enum ServiceTypeTreeViewModelProperty
        {
            CanDeploy,
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("EnvironmentTreeViewModel_CanProperties")]
        public void EnvironmentTreeViewModel_CanProperties_IsAuthorized_True()
        {
            foreach(EnvironmentTreeViewModelProperty property in Enum.GetValues(typeof(EnvironmentTreeViewModelProperty)))
            {
                Verify_EnvironmentTreeViewModel_CanProperties(property, isAuthorized: true, expected: true);
            }
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("EnvironmentTreeViewModel_CanProperties")]
        public void EnvironmentTreeViewModel_CanProperties_IsNotAuthorized_False()
        {
            foreach(EnvironmentTreeViewModelProperty property in Enum.GetValues(typeof(EnvironmentTreeViewModelProperty)))
            {
                Verify_EnvironmentTreeViewModel_CanProperties(property, isAuthorized: false, expected: false);
            }
        }

        static void Verify_EnvironmentTreeViewModel_CanProperties(EnvironmentTreeViewModelProperty property, bool isAuthorized, bool expected)
        {
            //------------Setup for test--------------------------
            var environmentModel = new Mock<IEnvironmentModel>();
            environmentModel.Setup(e => e.IsAuthorizedDeployFrom).Returns(isAuthorized);
            environmentModel.Setup(e => e.IsConnected).Returns(true);
            environmentModel.Setup(e => e.Connection).Returns(new Mock<IEnvironmentConnection>().Object);

            var parent = new Mock<ITreeNode>();
            parent.Setup(p => p.EnvironmentModel).Returns(environmentModel.Object);

            var tvm = new EnvironmentTreeViewModel(new Mock<IEventAggregator>().Object, null, environmentModel.Object, new Mock<IAsyncWorker>().Object);

            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            switch(property)
            {
                case EnvironmentTreeViewModelProperty.CanDeploy:
                    Assert.AreEqual(expected, tvm.CanDeploy, string.Format("{0} WHEN IsAuthorized={1}", property, isAuthorized));
                    break;
            }

            tvm.Dispose();
        }

        enum EnvironmentTreeViewModelProperty
        {
            CanDeploy,
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ResourceTreeViewModel_UserPermissions")]
        public void ResourceTreeViewModel_UserPermissions_DataContextPropertyChanged_UpdatesTreeViewModelUserPermissions()
        {
            //------------Setup for test--------------------------
            const Permissions PrePermissions = Permissions.View;
            const Permissions PostPermissions = Permissions.Execute;

            var resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(r => r.Environment.Connection.ServerEvents).Returns(new Mock<IEventPublisher>().Object);
            resourceModel.Setup(r => r.ID).Returns(Guid.NewGuid());
            resourceModel.Setup(r => r.UserPermissions).Returns(PrePermissions);

            var tvm = new MockResourceTreeViewModel(new Mock<IEventAggregator>().Object, null, resourceModel.Object);

            //------------Assert Pre-conditions-------------------------
            Assert.IsTrue(tvm.UserPermissions.HasValue);
            Assert.AreEqual(PrePermissions, tvm.UserPermissions.Value);

            //------------Execute Test---------------------------
            resourceModel.Setup(r => r.UserPermissions).Returns(PostPermissions);
            resourceModel.Raise(r => r.PropertyChanged += null, new PropertyChangedEventArgs("UserPermissions"));

            //------------Assert Results-------------------------
            Assert.IsTrue(tvm.UserPermissions.HasValue);
            Assert.AreEqual(PostPermissions, tvm.UserPermissions.Value);

            tvm.Dispose();
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("CategoryTreeViewModel_UserPermissions")]
        public void CategoryTreeViewModel_UserPermissions_IsNull()
        {
            //------------Setup for test--------------------------


            //------------Execute Test---------------------------
            var tvm = new CategoryTreeViewModel(new Mock<IEventAggregator>().Object, null, "test", ResourceType.WorkflowService);

            //------------Assert Results-------------------------
            Assert.IsFalse(tvm.UserPermissions.HasValue);

            tvm.Dispose();
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ServiceTypeTreeViewModel_UserPermissions")]
        public void ServiceTypeTreeViewModel_UserPermissions_IsNull()
        {
            //------------Setup for test--------------------------


            //------------Execute Test---------------------------
            var tvm = new ServiceTypeTreeViewModel(new Mock<IEventAggregator>().Object, null, ResourceType.WorkflowService);

            //------------Assert Results-------------------------
            Assert.IsFalse(tvm.UserPermissions.HasValue);

            tvm.Dispose();
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("EnvironmentTreeViewModel_UserPermissions")]
        public void EnvironmentTreeViewModel_UserPermissions_IsNull()
        {
            //------------Setup for test--------------------------
            var environmentModel = new Mock<IEnvironmentModel>();
            environmentModel.Setup(e => e.IsConnected).Returns(true);
            environmentModel.Setup(e => e.Connection).Returns(new Mock<IEnvironmentConnection>().Object);


            //------------Execute Test---------------------------
            var tvm = new EnvironmentTreeViewModel(new Mock<IEventAggregator>().Object, null, environmentModel.Object, new Mock<IAsyncWorker>().Object);

            //------------Assert Results-------------------------
            Assert.IsFalse(tvm.UserPermissions.HasValue);

            tvm.Dispose();
        }



        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ResourceTreeViewModel_ExecuteCommand")]
        public void ResourceTreeViewModel_ExecuteCommand_UserPermissionsHasExecuteFlag_ShowsViewInBrowserDialog()
        {
            //------------Setup for test--------------------------
            var eventAggregator = new Mock<IEventAggregator>();
            eventAggregator.Setup(e => e.Publish(It.IsAny<AddWorkSurfaceMessage>())).Verifiable();

            var resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(r => r.Environment.Connection.ServerEvents).Returns(new Mock<IEventPublisher>().Object);
            resourceModel.Setup(r => r.ID).Returns(Guid.NewGuid());
            resourceModel.Setup(r => r.UserPermissions).Returns(Permissions.Execute);

#pragma warning disable 219
            WorkflowInputDataViewModel workflowInputDataViewModel = null;
#pragma warning restore 219

            var windowManager = new Mock<IWindowManager>();
            windowManager.Setup(w => w.ShowDialog(It.IsAny<object>(), It.IsAny<object>(), It.IsAny<IDictionary<string, object>>()))
                .Callback((object rootModel, object context, IDictionary<string, object> settings) => workflowInputDataViewModel = (WorkflowInputDataViewModel)rootModel)
                .Returns(true)
                .Verifiable();

            var tvm = new MockResourceTreeViewModel(eventAggregator.Object, null, resourceModel.Object) { WindowManager = windowManager.Object };

            Assert.IsFalse(tvm.ShowDebugWindowOnLoad);

            //------------Execute Test---------------------------
            tvm.ExecuteCommand.Execute(null);

            //------------Assert Results-------------------------
            Assert.IsFalse(tvm.ShowDebugWindowOnLoad);
            eventAggregator.Verify(e => e.Publish(It.IsAny<AddWorkSurfaceMessage>()), Times.Once());

            tvm.Dispose();
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ResourceTreeViewModel_ExecuteCommand")]
        public void ResourceTreeViewModel_ExecuteCommand_UserPermissionsHasContributeFlag_InvokesEdit()
        {
            Verify_ResourceTreeViewModel_ExecuteCommand_InvokesEdit(ResourceType.WorkflowService);
            Verify_ResourceTreeViewModel_ExecuteCommand_InvokesEdit(ResourceType.Source);
            Verify_ResourceTreeViewModel_ExecuteCommand_InvokesEdit(ResourceType.Service);
        }

        void Verify_ResourceTreeViewModel_ExecuteCommand_InvokesEdit(ResourceType resourceType)
        {
            //------------Setup for test--------------------------
            ShowEditResourceWizardMessage editMessage = null;
            AddWorkSurfaceMessage addMessage = null;

            var eventAggregator = new Mock<IEventAggregator>();
            if(resourceType == ResourceType.WorkflowService)
            {
                eventAggregator.Setup(e => e.Publish(It.IsAny<AddWorkSurfaceMessage>())).Callback((object msg) => addMessage = (AddWorkSurfaceMessage)msg).Verifiable();
            }
            else
            {
                eventAggregator.Setup(e => e.Publish(It.IsAny<ShowEditResourceWizardMessage>())).Callback((object msg) => editMessage = (ShowEditResourceWizardMessage)msg).Verifiable();
            }

            var resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(r => r.Environment.Connection.ServerEvents).Returns(new Mock<IEventPublisher>().Object);
            resourceModel.Setup(r => r.ID).Returns(Guid.NewGuid());
            resourceModel.Setup(r => r.UserPermissions).Returns(Permissions.Contribute);
            resourceModel.Setup(r => r.ResourceType).Returns(resourceType);

            var tvm = new MockResourceTreeViewModel(eventAggregator.Object, null, resourceModel.Object);
            Assert.IsFalse(tvm.ShowDebugWindowOnLoad);

            //------------Execute Test---------------------------
            tvm.ExecuteCommand.Execute(null);

            //------------Assert Results-------------------------
            Assert.IsFalse(tvm.ShowDebugWindowOnLoad);
            if(resourceType == ResourceType.WorkflowService)
            {
                eventAggregator.Verify(e => e.Publish(It.IsAny<AddWorkSurfaceMessage>()));
                Assert.IsNotNull(addMessage);
                Assert.IsTrue(addMessage.ShowDebugWindowOnLoad);
            }
            else
            {
                eventAggregator.Verify(e => e.Publish(It.IsAny<ShowEditResourceWizardMessage>()));
                Assert.IsNotNull(editMessage);
                Assert.AreSame(resourceModel.Object, editMessage.ResourceModel);
            }


            tvm.Dispose();
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ResourceTreeViewModel_CanExecute")]
        public void ResourceTreeViewModel_CanExecute_DataContextIsNotNull_True()
        {
            //------------Setup for test--------------------------
            var eventAggregator = new Mock<IEventAggregator>();

            var resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(r => r.Environment.Connection.ServerEvents).Returns(new Mock<IEventPublisher>().Object);
            resourceModel.Setup(r => r.ID).Returns(Guid.NewGuid());
            resourceModel.Setup(r => r.UserPermissions).Returns(Permissions.Contribute);
            resourceModel.Setup(r => r.ResourceType).Returns(ResourceType.WorkflowService);

            var tvm = new MockResourceTreeViewModel(eventAggregator.Object, null, resourceModel.Object);

            //------------Execute Test---------------------------
            var result = tvm.ExecuteCommand.CanExecute(null);

            //------------Assert Results-------------------------
            Assert.IsTrue(result);

            tvm.Dispose();
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ResourceTreeViewModel_CanExecute")]
        public void ResourceTreeViewModel_CanExecute_DataContextIsNull_False()
        {
            //------------Setup for test--------------------------
            var eventAggregator = new Mock<IEventAggregator>();

            var resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(r => r.Environment.Connection.ServerEvents).Returns(new Mock<IEventPublisher>().Object);
            resourceModel.Setup(r => r.ID).Returns(Guid.NewGuid());
            resourceModel.Setup(r => r.UserPermissions).Returns(Permissions.Contribute);
            resourceModel.Setup(r => r.ResourceType).Returns(ResourceType.WorkflowService);

            var tvm = new MockResourceTreeViewModel(eventAggregator.Object, null, resourceModel.Object) { DataContext = null };

            //------------Execute Test---------------------------
            var result = tvm.ExecuteCommand.CanExecute(null);

            //------------Assert Results-------------------------
            Assert.IsFalse(result);

            tvm.Dispose();
        }
    }
}