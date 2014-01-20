#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Caliburn.Micro;
using Dev2.Communication;
using Dev2.Composition;
using Dev2.Core.Tests.Utils;
using Dev2.Providers.Events;
using Dev2.Services;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Core.ViewModels.Navigation;
using Dev2.Studio.Enums;
using Dev2.Studio.ViewModels.Navigation;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;


#endregion
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
        Mock<IEnvironmentModel> _disconnectedMockEnvironmentModel;
        Mock<IContextualResourceModel> _mockResourceModel;
        Mock<IContextualResourceModel> _mockResourceModel1;
        Mock<IContextualResourceModel> _mockResourceModel2;
        NavigationViewModel _vm;

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
        public void MyTestInitialize()
        {
        }

        [TestCleanup]
        public void TestCleanUp()
        {
        }

        #endregion

        #region Loading Resources

        [TestMethod]
        [TestCategory("NavigationViewModel_LoadResourcesAsync")]
        [Description("Async load calls OnResourcesLoaded event")]
        [Owner("Ashley Lewis")]
        public void NavigationViewModel_UnitTest_LoadResourcesAsync_InvokesEventHandler()
        {
            //isolate unit
            Init();

            _vm.LoadResourcesCompleted += (sender, args) => Assert.AreEqual(1, 1, "Navigation view model did not execute LoadResourcesCompleted event after async load");


            //test unit
            _vm.LoadEnvironmentResources(_mockEnvironmentModel.Object);

            //test result
        }

        #endregion

        #region Updating Resources

        void Init()
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

            _vm = CreateViewModel();
            _vm.AddEnvironment(_mockEnvironmentModel.Object);
        }

        [TestMethod]
        public void UpdateResourceMessage_WhenResourceExistsOldCategory_Expects_ResourceChanged()
        {
            Init(false, true);
            _vm.Root.FindChild(_mockResourceModel.Object);

            _mockResourceModel.Setup(r => r.ResourceName).Returns("MockChangedName");
            UpdateResourceMessage updatemsg = new UpdateResourceMessage(_mockResourceModel.Object);

            _vm.Handle(updatemsg);
            var updatedResource = _vm.Root.FindChild(_mockResourceModel.Object);
            Assert.AreEqual("MockChangedName", updatedResource.DisplayName);
        }

        [TestMethod]
        public void UpdateResourceMessage_WhenResourceUnchanged_Expects_UnchangedResource()
        {
            Init(false, true);
            var oldResoure = _vm.Root.FindChild(_mockResourceModel.Object);

            Assert.AreEqual("Mock", oldResoure.DisplayName);

            var updatemsg = new UpdateResourceMessage(_mockResourceModel.Object);

            _vm.Handle(updatemsg);
            var updatedResource = _vm.Root.FindChild(_mockResourceModel.Object);
            Assert.AreEqual("Mock", updatedResource.DisplayName);
        }

        [TestMethod]
        public void UpdateResourceMessage_WhenResourceExistsChangedCategory_Expects_CategoryChanged()
        {
            Init(false, true);
            var serviceTypeVM = _vm.Root.FindChild(ResourceType.WorkflowService);
            var oldCategory = serviceTypeVM.FindChild("Testing");
            var resourceVM = oldCategory.FindChild(_mockResourceModel.Object);

            Assert.IsTrue(ReferenceEquals(oldCategory, resourceVM.TreeParent));
            Assert.IsTrue(String.Compare(oldCategory.DisplayName, "Testing",
                StringComparison.InvariantCultureIgnoreCase) ==
                            0);

            _mockResourceModel.Setup(r => r.Category).Returns("Testing2");

            var updatemsg = new UpdateResourceMessage(_mockResourceModel.Object);

            _vm.Handle(updatemsg);

            var updatedCategory = serviceTypeVM.FindChild("Testing2");
            resourceVM = updatedCategory.FindChild(_mockResourceModel.Object);
            Assert.IsTrue(ReferenceEquals(updatedCategory, resourceVM.TreeParent));
            Assert.IsTrue(String.Compare(updatedCategory.DisplayName, "Testing2",
                StringComparison.InvariantCultureIgnoreCase) ==
                          0);
            Assert.AreEqual(updatedCategory.ChildrenCount, 2);
        }

        [TestMethod]
        public void UpdateResourceMessage_WhenNewCategory_Expects_CategoryAdded()
        {
            Init(false, true);
            var serviceTypeVM = _vm.Root.FindChild(ResourceType.WorkflowService);

            _mockResourceModel.Setup(r => r.Category).Returns("Testing5");

            var updatemsg = new UpdateResourceMessage(_mockResourceModel.Object);

            _vm.Handle(updatemsg);

            var tryGetNewCategory = _vm.Root.Children[0].Children[0].Children.FirstOrDefault(child => child.DisplayName.ToUpper() == _mockResourceModel.Object.Category.ToUpper());

            var updatedCategory = serviceTypeVM.FindChild("Testing5");
            Assert.IsNotNull(updatedCategory);
            Assert.IsTrue(ReferenceEquals(updatedCategory, tryGetNewCategory));
            Assert.IsTrue(String.Compare(updatedCategory.DisplayName, "Testing5",
                StringComparison.InvariantCultureIgnoreCase) ==
                          0);
            Assert.AreEqual(updatedCategory.ChildrenCount, 1);
        }

        [TestMethod]
        public void UpdateResourceMessage_WhenOldCategoryBecomesEmpty_Expects_CategoryDeleted()
        {
            Init(false, true);
            var serviceTypeVM = _vm.Root.FindChild(ResourceType.WorkflowService);

            _mockResourceModel.Setup(r => r.Category).Returns("Testing5");

            var updatemsg = new UpdateResourceMessage(_mockResourceModel.Object);

            _vm.Handle(updatemsg);

            var updatedCategory = serviceTypeVM.FindChild("Testing");
            Assert.IsNull(updatedCategory);
            Assert.IsTrue(_vm.Root.ChildrenCount == 3);
        }

        [TestMethod]
        public void UpdateResourceMessage_WhenOldCategoryNotEmpty_Expects_CategoryNotDeleted()
        {
            Init(false, true);
            var serviceTypeVM = _vm.Root.FindChild(ResourceType.WorkflowService);

            _mockResourceModel.Setup(r => r.Category).Returns("Testing2");

            var updatemsg = new UpdateResourceMessage(_mockResourceModel.Object);
            _vm.Handle(updatemsg);

            _mockResourceModel.Setup(r => r.Category).Returns("Testing3");

            updatemsg = new UpdateResourceMessage(_mockResourceModel.Object);

            _vm.Handle(updatemsg);

            var updatedCategory = serviceTypeVM.FindChild("Testing2");

            Assert.IsTrue(updatedCategory.ChildrenCount == 1);
        }

        [TestMethod]
        public void UpdateResourceMessage_WhenNewResource_Expects_ResourceAdded()
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

            var serviceTypeVM = _vm.Root.FindChild(ResourceType.WorkflowService);
            var categoryVM = serviceTypeVM.FindChild("Testing");
            var resourceVM = _vm.Root.FindChild(newResource.Object);
            Assert.IsTrue(resourceVM.DisplayName == "Cake");
            Assert.IsTrue(categoryVM.ChildrenCount == 2);
            Assert.IsTrue(_vm.Root.ChildrenCount == 4);
        }

        [TestMethod]
        public void UpdateResourceMessage_WhenNewResourceAndNewCategory_Expects_CategoryAdded()
        {
            Init(false, true);
            var newResource = new Mock<IContextualResourceModel>();
            newResource.Setup(r => r.ResourceType)
                       .Returns(ResourceType.WorkflowService);
            newResource.Setup(r => r.Category).Returns("Testing4");
            newResource.Setup(r => r.ResourceName).Returns("Cake");
            newResource.Setup(r => r.Environment)
                       .Returns(_mockEnvironmentModel.Object);

            var updatemsg = new UpdateResourceMessage(newResource.Object);
            _vm.Handle(updatemsg);

            var serviceTypeVM = _vm.Root.FindChild(ResourceType.WorkflowService);
            var categoryVM = serviceTypeVM.FindChild("Testing4");
            Assert.IsTrue(String.Compare(categoryVM.DisplayName, "Testing4",
                StringComparison.InvariantCultureIgnoreCase) ==
                          0);

            Assert.IsTrue(categoryVM.ChildrenCount == 1);
            Assert.IsTrue(_vm.Root.ChildrenCount == 4);
        }

        [TestMethod]
        public void AddEnvironmentWithShouldLoadResourcesTrueExpectedRootAndChildrenCreated()
        {
            Init(false, true);
            var workflowServiceTypeVM = _vm.Root.FindChild(ResourceType.WorkflowService);
            var serviceTypeVM = _vm.Root.FindChild(ResourceType.Service);
            var categoryVM = workflowServiceTypeVM.FindChild("Testing");
            var resourceVM = categoryVM.FindChild(_mockResourceModel.Object);
            Assert.IsTrue(ReferenceEquals(categoryVM, resourceVM.TreeParent));
            Assert.IsTrue(String.Compare(categoryVM.DisplayName, "Testing",
                StringComparison.InvariantCultureIgnoreCase) ==
                          0);
            Assert.IsTrue(workflowServiceTypeVM.ChildrenCount == 2);
            Assert.IsTrue(serviceTypeVM.ChildrenCount == 1);
        }

        [TestMethod]
        public void AddEnvironmentWithShouldLoadResourcesFalseExpectedNoEnvironmentAdded()
        {
            Init(false, false);
            Assert.AreEqual(0, _vm.Root.Children.Count());
        }

        [TestMethod]
        public void AddEnvironmentWithShouldLoadResourcesTrueExpectedEnvironmentAdded()
        {
            Init(false, true);
            Assert.AreEqual(1, _vm.Root.Children.Count());
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("NavigationViewModel_UpdateResource")]
        public void NavigationViewModel_UpdateResource_ServiceTypeAndCategoryNotFound_ServiceTypeAndCategoryCreatedForNewResource()
        {
            // ReSharper disable ObjectCreationAsStatement
            Init();
            var eventAggregator = new Mock<IEventAggregator>().Object;

            var environment = new Mock<IEnvironmentModel>();
            var mockConnection = new Mock<IEnvironmentConnection>();
            mockConnection.Setup(conn => conn.Alias).Returns("Expected Environment");
            mockConnection.Setup(conn => conn.ServerEvents).Returns(new Mock<IEventPublisher>().Object);
            mockConnection.Setup(conn => conn.AppServerUri).Returns(new Uri("http://10.0.0.1"));
            environment.Setup(env => env.Connection).Returns(mockConnection.Object);
            new EnvironmentTreeViewModel(eventAggregator, _vm.Root, environment.Object, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object);
            var newResource = new Mock<IContextualResourceModel>();
            newResource.Setup(res => res.Category).Returns("Expected Category");
            newResource.Setup(res => res.ResourceName).Returns("Expected Resource Name");
            newResource.Setup(res => res.Environment).Returns(environment.Object);
            new ResourceTreeViewModel(eventAggregator, null, newResource.Object);
            // ReSharper restore ObjectCreationAsStatement

            //------------Execute Test---------------------------
            _vm.UpdateResource(newResource.Object);

            // Assert Category Node And Type Node Removed
            Assert.AreEqual("WORKFLOWS", _vm.Root.Children[0].Children[0].DisplayName, "New service type not created for new resource");
            Assert.AreEqual("EXPECTED CATEGORY", _vm.Root.Children[0].Children[0].Children[0].DisplayName, "New category not created for new resource");
            Assert.AreEqual("Expected Resource Name", _vm.Root.Children[0].Children[0].Children[0].Children[0].DisplayName, "New resource not added to the tree");
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("NavigationViewModel_UpdateResource")]
        public void NavigationViewModel_UpdateResource_DeployView_DoesNotUpdateForDeleteRemoteResource()
        {
            const string expectedNodeName = "Expected Node Name";
            const string expectedCategoryName = "Expected Category";
            var expectedResourceId = Guid.NewGuid();
            var remoteServerID = Guid.NewGuid();

            //Remote resource (IContextualResourceModel)
            Init();
            var mockRemoteConnection = new Mock<IEnvironmentConnection>();
            mockRemoteConnection.Setup(conn => conn.Alias).Returns("Remote Environment");
            mockRemoteConnection.Setup(conn => conn.AppServerUri).Returns(new Uri("http://192.168.0.1"));
            var remoteEnvironment = new Mock<IEnvironmentModel>();
            remoteEnvironment.Setup(env => env.ID).Returns(remoteServerID);
            remoteEnvironment.Setup(env => env.Connection).Returns(mockRemoteConnection.Object);

            var remoteResource = new Mock<IContextualResourceModel>();
            remoteResource.Setup(res => res.DisplayName).Returns(expectedNodeName);
            remoteResource.Setup(res => res.ResourceName).Returns(expectedNodeName);
            remoteResource.Setup(res => res.ID).Returns(expectedResourceId);
            remoteResource.Setup(res => res.Environment).Returns(remoteEnvironment.Object);
            remoteResource.Setup(res => res.Category).Returns(expectedCategoryName);
            remoteResource.Setup(res => res.ServerID).Returns(remoteServerID);

            //Local resource (ITreeNode)
            var mockLocalConnection = new Mock<IEnvironmentConnection>();
            mockLocalConnection.Setup(conn => conn.Alias).Returns("Local Environment");
            mockLocalConnection.Setup(conn => conn.AppServerUri).Returns(new Uri("http://192.168.0.2"));
            var localEnvironment = new Mock<IEnvironmentModel>();
            localEnvironment.Setup(env => env.ID).Returns(Guid.NewGuid);
            localEnvironment.Setup(env => env.Connection).Returns(mockLocalConnection.Object);

            var localResource = new Mock<ITreeNode>();
            localResource.Setup(res => res.DisplayName).Returns(expectedNodeName);
            //localResource.Setup(res => res<IResourceModel>.DataContext.ID)

            var eventAggregator = new Mock<IEventAggregator>().Object;

            new EnvironmentTreeViewModel(eventAggregator, _vm.Root, localEnvironment.Object, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object);
            new ServiceTypeTreeViewModel(eventAggregator, _vm.Root.Children[0], ResourceType.WorkflowService);
            new CategoryTreeViewModel(eventAggregator, _vm.Root.Children[0].Children[0], expectedCategoryName, ResourceType.WorkflowService);
            _vm.Root.Children[0].Children[0].Children[0].Children.Add(localResource.Object);

            //------------Execute Test---------------------------
            _vm.UpdateResource(remoteResource.Object);

            // Assert Does Not Update For Delete Remote Resource
            Assert.IsNotNull(_vm.Root.Children[0].Children[0].Children[0].Children.FirstOrDefault(child => child.DisplayName == localResource.Object.DisplayName));
        }

        #endregion Updating Resources

        #region Filtering

        [TestMethod]
        public void FilteredNavigationViewModel_AfterReload_Expects_FilteredItemsInTreeWithIsFilteredTrue()
        {
            Init(false, true);
            var resourceVM = _vm.Root.FindChild(_mockResourceModel.Object);

            Assert.AreEqual(3, _vm.Root.ChildrenCount);
            Assert.IsTrue(resourceVM.IsFiltered == false);

            _vm.UpdateSearchFilter("Mock2");
            _vm.Root.NotifyOfFilterPropertyChanged(false);

            Assert.AreEqual(1, _vm.Root.ChildrenCount);
            Assert.IsTrue(resourceVM.IsFiltered);

            _vm.LoadEnvironmentResources(_mockEnvironmentModel.Object);
            _vm.Root.NotifyOfFilterPropertyChanged(false);

            resourceVM = _vm.Root.FindChild(_mockResourceModel.Object);

            Assert.AreEqual(1, _vm.Root.ChildrenCount);
            Assert.IsTrue(resourceVM.IsFiltered);

            _vm.UpdateSearchFilter("Mock");

            Assert.AreEqual(3, _vm.Root.ChildrenCount);
            Assert.IsFalse(resourceVM.IsFiltered);
        }

        [TestMethod]
        public void FilteredNavigationViewModel_WhereResourceNodeNotFiltered_Expects_CategoryExpanded()
        {
            ITreeNode resourceVM2_2 = null;

            var reset = new AutoResetEvent(false);
            ThreadExecuter.RunCodeAsSTA(reset,
                () =>
                {
                    Init(false, true);
                    var resourceVM = _vm.Root.FindChild(_mockResourceModel.Object);
                    var resourceVM2_1 = _vm.Root.FindChild(_mockResourceModel1.Object);
                    resourceVM2_2 = _vm.Root.FindChild(_mockResourceModel2.Object);

                    Assert.IsTrue(resourceVM.IsFiltered == false);
                    Assert.IsTrue(resourceVM2_1.IsFiltered == false);
                    Assert.IsTrue(resourceVM2_2.IsFiltered == false);
                    Assert.IsTrue(resourceVM.TreeParent.IsExpanded == false);
                    Assert.IsTrue(resourceVM2_1.TreeParent.IsExpanded == false);
                    Assert.IsTrue(resourceVM2_2.TreeParent.IsExpanded == false);

                    _vm.UpdateSearchFilter("Mock2");
                    _vm.Root.NotifyOfFilterPropertyChanged(false);

                    Thread.Sleep(100);

                });
            reset.WaitOne();

            Assert.IsTrue(resourceVM2_2.IsFiltered == false);
            Assert.IsTrue(resourceVM2_2.TreeParent.IsExpanded);
        }

        [TestMethod]
        public void FilteredNavigationViewModel_WhereResourceNodeFiltered_Expects_CategoryCollapsed()
        {
            ITreeNode resourceVM = null;
            var reset = new AutoResetEvent(false);
            ThreadExecuter.RunCodeAsSTA(reset,
                () =>
                {
                    Init(false, true);
                    resourceVM = _vm.Root.FindChild(_mockResourceModel.Object);
                    var resourceVM2_1 = _vm.Root.FindChild(_mockResourceModel1.Object);
                    var resourceVM2_2 = _vm.Root.FindChild(_mockResourceModel2.Object);

                    Assert.IsTrue(resourceVM.IsFiltered == false);
                    Assert.IsTrue(resourceVM2_1.IsFiltered == false);
                    Assert.IsTrue(resourceVM2_2.IsFiltered == false);
                    Assert.IsTrue(resourceVM.TreeParent.IsExpanded == false);
                    Assert.IsTrue(resourceVM2_1.TreeParent.IsExpanded == false);
                    Assert.IsTrue(resourceVM2_2.TreeParent.IsExpanded == false);

                    _vm.UpdateSearchFilter("Mock2");
                    _vm.Root.NotifyOfFilterPropertyChanged(false);

                    Thread.Sleep(100);
                });
            reset.WaitOne();

            Assert.IsTrue(resourceVM.IsFiltered);
            Assert.IsTrue(!resourceVM.TreeParent.IsExpanded);
        }

        [TestMethod]
        public void FilteredNavigationViewModel_WhereFilterReset_Expects_OriginalExpandState()
        {
            ITreeNode resourceVM = null;
            ITreeNode resourceVM2_1 = null;
            ITreeNode resourceVM2_2 = null;

            var reset = new AutoResetEvent(false);
            ThreadExecuter.RunCodeAsSTA(reset,
                () =>
                {
                    Init(false, true);
                    resourceVM = _vm.Root.FindChild(_mockResourceModel.Object);
                    resourceVM2_1 = _vm.Root.FindChild(_mockResourceModel1.Object);
                    resourceVM2_2 = _vm.Root.FindChild(_mockResourceModel2.Object);

                    Assert.IsTrue(resourceVM.IsFiltered == false);
                    Assert.IsTrue(resourceVM2_1.IsFiltered == false);
                    Assert.IsTrue(resourceVM2_2.IsFiltered == false);
                    Assert.IsTrue(resourceVM.TreeParent.IsExpanded == false);
                    Assert.IsTrue(resourceVM2_1.TreeParent.IsExpanded == false);
                    Assert.IsTrue(resourceVM2_2.TreeParent.IsExpanded == false);

                    _vm.UpdateSearchFilter("Mock2");

                    _vm.UpdateSearchFilter("");
                });
            reset.WaitOne();

            Assert.IsTrue(resourceVM.IsFiltered == false);
            Assert.IsTrue(resourceVM2_1.IsFiltered == false);
            Assert.IsTrue(resourceVM2_2.IsFiltered == false);
            Assert.IsTrue(resourceVM.TreeParent.IsExpanded == false);
            Assert.IsTrue(resourceVM2_1.TreeParent.IsExpanded == false);
            Assert.IsTrue(resourceVM2_2.TreeParent.IsExpanded == false);
        }

        [TestMethod]
        public void FilteredNavigationViewModel_WhereNoContent_Expects_RootAndEnvironmentNotFiltered()
        {
            var reset = new AutoResetEvent(false);
            ThreadExecuter.RunCodeAsSTA(reset,
                () =>
                {
                    Init(false, true);
                    _vm.UpdateSearchFilter("ZD");
                });
            reset.WaitOne();

            Assert.IsTrue(_vm.Root.IsFiltered == false);
            Assert.IsTrue(_vm.Root.IsExpanded);
            Assert.IsTrue(_vm.Root.Children.ToList().All(c => c.IsFiltered == false));
        }

        [TestMethod]
        public void NavigationViewModel_WhereNoContent_Expects_RootAndEnvironmentNotFiltered()
        {
            var reset = new AutoResetEvent(false);
            ThreadExecuter.RunCodeAsSTA(reset,
                () =>
                {
                    Init(false, true);
                    ITreeNode resourceVM = _vm.Root.FindChild(_mockResourceModel.Object);
                    ITreeNode resourceVM2_1 = _vm.Root.FindChild(_mockResourceModel1.Object);
                    ITreeNode resourceVM2_2 = _vm.Root.FindChild(_mockResourceModel2.Object);

                    resourceVM.TreeParent.Remove(resourceVM);
                    resourceVM2_1.TreeParent.Remove(resourceVM2_1);
                    resourceVM2_2.TreeParent.Remove(resourceVM2_2);

                    _vm.UpdateSearchFilter("");
                });
            reset.WaitOne();

            Assert.IsTrue(_vm.Root.ChildrenCount == 0);
            Assert.IsTrue(_vm.Root.IsFiltered == false);
            Assert.IsTrue(_vm.Root.IsExpanded);
            Assert.IsTrue(_vm.Root.Children.ToList().All(c => c.IsFiltered == false));
            Assert.IsTrue(_vm.Root.Children.ToList().All(c => c.ChildrenCount == 0));
        }

        [TestMethod]
        public void Filter_Expects_FilteredCategories_WithNoResources_IsFiltered()
        {
            ITreeNode resourceVM = null;
            ITreeNode resourceVM2_1 = null;
            ITreeNode resourceVM2_2 = null;

            var reset = new AutoResetEvent(false);
            ThreadExecuter.RunCodeAsSTA(reset,
                () =>
                {
                    Init(false, true);
                    _vm.UpdateSearchFilter("zd");
                    _vm.Root.NotifyOfFilterPropertyChanged(false);

                    resourceVM = _vm.Root.FindChild(_mockResourceModel.Object);
                    resourceVM2_1 = _vm.Root.FindChild(_mockResourceModel1.Object);
                    resourceVM2_2 = _vm.Root.FindChild(_mockResourceModel2.Object);

                });
            reset.WaitOne();

            Assert.IsTrue(resourceVM.TreeParent.IsFiltered);
            Assert.IsTrue(resourceVM2_1.TreeParent.IsFiltered);
            Assert.IsTrue(resourceVM2_2.TreeParent.IsFiltered);
        }

        [TestMethod]
        public void Filter_Expects_FilteredCategories_WithResources_IsNotFiltered_AndExpanded()
        {
            ITreeNode matchingNode = null;

            var reset = new AutoResetEvent(false);
            ThreadExecuter.RunCodeAsSTA(reset,
                () =>
                {
                    Init(false, true);
                    _vm.Root.FindChild(_mockResourceModel.Object);
                    matchingNode = _vm.Root.FindChild(_mockResourceModel2.Object);

                    _vm.UpdateSearchFilter("Mock2");

                    Thread.Sleep(100);

                });
            reset.WaitOne();


            Assert.IsFalse(matchingNode.IsFiltered);
            Assert.IsTrue(matchingNode.TreeParent.IsExpanded);
        }

        [TestMethod]
        public void Filter_Expects_UnFilteredCategories_NotFiltered_AndExpanded()
        {
            ITreeNode nonMatchingCategory = null;
            ITreeNode matchingCategory = null;

            var reset = new AutoResetEvent(false);
            ThreadExecuter.RunCodeAsSTA(reset,
                () =>
                {
                    Init(false, true);
                    nonMatchingCategory =
                        _vm.Root.FindChild(_mockResourceModel.Object).TreeParent;
                    matchingCategory =
                        _vm.Root.FindChild(_mockResourceModel1.Object).TreeParent;

                    _vm.UpdateSearchFilter("Testing2");

                    Thread.Sleep(100);

                });
            reset.WaitOne();

            Assert.IsTrue(nonMatchingCategory.IsFiltered);
            Assert.IsTrue(matchingCategory.IsFiltered == false);
            Assert.IsTrue(matchingCategory.IsExpanded);
        }

        #endregion Filtering

        #region remove

        [TestMethod]
        public void RemoveEnvironmentsWhenEnvironmentConnected()
        {
            Init(false, true);
            _mockEnvironmentModel.SetupGet(c => c.IsConnected).Returns(true);
            _mockEnvironmentModel.SetupGet(c => c.Name).Returns("Mock");

            Assert.IsTrue(_vm.Environments.Count == 1);
            Assert.IsTrue(_vm.Root.ChildrenCount > 0);

            _vm.RemoveEnvironment(_mockEnvironmentModel.Object);
            Assert.IsTrue(_vm.Environments.Count == 0);
            Assert.IsTrue(_vm.Root.ChildrenCount == 0);

        }

        [TestMethod]
        public void NoExceptionWhenEnvironmentNotInEnvironmentList()
        {
            var reset = new AutoResetEvent(false);
            ThreadExecuter.RunCodeAsSTA(reset,
                () =>
                {
                    Init(false, true);
                    _mockEnvironmentModel.SetupGet(c => c.IsConnected).Returns(true);
                    _mockEnvironmentModel.SetupGet(c => c.Name).Returns("Mock");

                    Assert.IsTrue(_vm.Environments.Count == 1);
                    Assert.IsTrue(_vm.Root.ChildrenCount > 0);

                    var nonAddedMock = GetMockEnvironment();
                    nonAddedMock.Setup(m => m.ID).Returns(Guid.NewGuid);

                    _vm.RemoveEnvironment(nonAddedMock.Object);

                });
            reset.WaitOne();

            Assert.IsTrue(_vm.Environments.Count == 1);
            Assert.IsTrue(_vm.Root.ChildrenCount > 0);

        }

        #endregion remove

        #region Refresh Environments Tests

        void RefreshTestsSetup()
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

            _vm = CreateViewModel();
            _vm.AddEnvironment(_reMockEnvironmentModel.Object);
            _vm.AddEnvironment(_reMockEnvironmentModel1.Object);
        }

        [TestMethod]
        public void RefreshSingleEnvironmentTestExpectedRefreshOfOneEnvironmentOnly()
        {
            RefreshTestsSetup();

            var eventAggregator = new Mock<IEventAggregator>().Object;

            var rootTreeViewModel = new RootTreeViewModel(eventAggregator);
            rootTreeViewModel.Parent = _vm;

            var envConn = new Mock<IEnvironmentConnection>();
            envConn.Setup(e => e.IsConnected).Returns(true);
            envConn.Setup(e => e.ServerEvents).Returns(new Mock<IEventPublisher>().Object);
            envConn.Setup(e => e.AppServerUri).Returns(new Uri("http://127.0.0.1/"));

            var envModel = new EnvironmentModel(new Mock<IEventAggregator>().Object, Guid.NewGuid(), envConn.Object, _reMockResourceRepository.Object);
            var environmentTreeViewModel = new EnvironmentTreeViewModel(eventAggregator, rootTreeViewModel, envModel, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object);
            environmentTreeViewModel.RefreshCommand.Execute(null);

            _reMockResourceRepository.Verify(x => x.UpdateWorkspace(It.IsAny<IList<IWorkspaceItem>>()), Times.Exactly(1));

        }

        [TestMethod]
        public void RefreshAllEnvironmentTestExpectedSecondEnvironmentStillNotConnected()
        {
            RefreshTestsSetup();
            _vm.UpdateWorkspaces();
            Assert.IsTrue(_vm.Environments[0].IsConnected);
            Assert.IsTrue(!_vm.Environments[1].IsConnected);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("NavigationViewModel")]
        public void RefreshEnvironment_UnitTest_WhereTreeIsPopulatedAlready_ExpectedSameObjectsInTree()
        {
            RefreshTestsSetup();
            _reMockEnvironmentModel.Setup(c => c.CanStudioExecute).Returns(true);
            _vm.LoadEnvironmentResources(_reMockEnvironmentModel.Object);
            ResourceTreeViewModel expected = _vm.Root.Children[0].Children[0].Children[0].Children[0] as ResourceTreeViewModel;
            _vm.LoadEnvironmentResources(_reMockEnvironmentModel.Object);
            ResourceTreeViewModel actual = _vm.Root.Children[0].Children[0].Children[0].Children[0] as ResourceTreeViewModel;
            Assert.AreEqual(expected, actual, "The objects are not the same object");
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("NavigationViewModel")]
        public void RefreshEnvironment_UnitTest_WhereOneExtraResource_ExpectedOneExtraResourceInTree()
        {
            RefreshTestsSetup();
            _reMockEnvironmentModel.Setup(c => c.CanStudioExecute).Returns(true);
            _vm.LoadEnvironmentResources(_reMockEnvironmentModel.Object);
            var expected = _vm.Root.Children[0].Children[0].Children.Count;

            Mock<IContextualResourceModel> mockResourceModel3 = new Mock<IContextualResourceModel>();
            mockResourceModel3.Setup(r => r.ResourceType).Returns(ResourceType.WorkflowService);
            mockResourceModel3.Setup(r => r.Category).Returns("Testing3");
            mockResourceModel3.Setup(r => r.ResourceName).Returns("Mock3");
            mockResourceModel3.Setup(r => r.Environment).Returns(_reMockEnvironmentModel.Object);

            _reMockResourceRepository = new Mock<IResourceRepository>();
            _reMockResourceRepository.Setup(r => r.All()).Returns(
                new Collection<IResourceModel>
                {
                    _mockResourceModel.Object,
                    _mockResourceModel1.Object,
                    _mockResourceModel2.Object,
                    mockResourceModel3.Object
                });
            _reMockEnvironmentModel.Setup(c => c.ResourceRepository).Returns(_reMockResourceRepository.Object);
            _vm.LoadEnvironmentResources(_reMockEnvironmentModel.Object);
            var actual = _vm.Root.Children[0].Children[0].Children.Count;
            Assert.IsTrue(actual == expected + 1, "The resource was not added to the tree");
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("NavigationViewModel")]
        public void RefreshEnvironment_UnitTest_WhereOneLessResource_ExpectedOneLessResourceInTree()
        {
            RefreshTestsSetup();
            _reMockEnvironmentModel.Setup(c => c.CanStudioExecute).Returns(true);
            _vm.LoadEnvironmentResources(_reMockEnvironmentModel.Object);

            _reMockResourceRepository = new Mock<IResourceRepository>();
            _reMockResourceRepository.Setup(r => r.All()).Returns(
                new Collection<IResourceModel>
                {
                    _mockResourceModel.Object
                });
            _reMockEnvironmentModel.Setup(c => c.ResourceRepository).Returns(_reMockResourceRepository.Object);
            _vm.LoadEnvironmentResources(_reMockEnvironmentModel.Object);

            Assert.IsTrue(_vm.Root.Children[0].Children[0].Children[0].Children.Count == 0, "The resource was not removed to the tree");
        }

        [TestMethod]
        [TestCategory("NavigationViewModel")]
        [Description("Refresh all environments connects to localhost")]
        [Owner("Ashley Lewis")]
        public void NavigationViewModel_UpdateWorkspaces_DisconnectedLocalhostServer_LocalhostServerConnected()
        {
            //isolate UpdateWorkspaces as a functional unit
            RefreshTestsSetup();
            _reMockEnvironmentModel.Setup(c => c.CanStudioExecute).Returns(true);

            //run unit
            _vm.UpdateWorkspaces();

            //assert both localhost servers are connected
            Assert.IsTrue(_vm.Root.Children[0].Children[0].IsConnected, "Refresh does not autoconnect to localhost");
            Assert.IsTrue(_vm.Root.Children[0].Children[1].IsConnected, "Refresh does not autoconnect to localhost");
        }

        #endregion

        #region Disconnect

        [TestMethod]
        public void EnvironmentNodeDisconnect_Expect_NodeRemovedFromRoot()
        {
            Init(false, true);
            _mockEnvironmentModel.SetupGet(c => c.IsConnected).Returns(true);
            _mockEnvironmentModel.SetupGet(c => c.Name).Returns("Mock");

            Assert.IsTrue(_vm.Root.ChildrenCount != 0);

            var message =
                new EnvironmentDisconnectedMessage(_mockEnvironmentModel.Object);
            _vm.Handle(message);
            Assert.IsTrue(_vm.Root.ChildrenCount == 0);
        }

        [TestMethod]
        public void EnvironmentNodeDisconnectExpectSetIsRefreshingToFalse()
        {
            var reset = new AutoResetEvent(false);
            ThreadExecuter.RunCodeAsSTA(reset,
                () =>
                {
                    Init(false, true);
                    _mockEnvironmentModel.SetupGet(c => c.IsConnected).Returns(true);
                    _mockEnvironmentModel.SetupGet(c => c.Name).Returns("Mock");

                    Assert.IsTrue(_vm.Root.ChildrenCount != 0);

                    var message =
                        new EnvironmentDisconnectedMessage(_mockEnvironmentModel.Object);
                    _vm.Handle(message);
                    Assert.IsFalse(_vm.IsRefreshing);
                });
            reset.WaitOne();

            Assert.IsTrue(_vm.Root.ChildrenCount == 0);
        }

        #endregion

        #region Activity Drop From ToolBox Tests

        [TestMethod]
        public void CreateNavigationViewModelWithIsActivityDropTrueAndTypeWorkflowExpectedOnlyWorkflowsToBeInTree()
        {
            Init(false, true);
            _vm = CreateViewModel(true, enDsfActivityType.Workflow);
            _vm.AddEnvironment(_mockEnvironmentModel.Object);
            Assert.AreEqual(1, _vm.Root.Children[0].Children.Count);
            Assert.AreEqual("WORKFLOWS", _vm.Root.Children[0].Children[0].DisplayName);
        }

        [TestMethod]
        public void CreateNavigationViewModelWithIsActivityDropTrueAndTypeServiceExpectedOnlyServicesToBeInTree()
        {
            Init(false, true);
            _vm = CreateViewModel(true, enDsfActivityType.Service);
            _vm.AddEnvironment(_mockEnvironmentModel.Object);
            Assert.AreEqual(1, _vm.Root.Children[0].Children.Count);
            Assert.AreEqual("SERVICES", _vm.Root.Children[0].Children[0].DisplayName);
        }

        [TestMethod]
        public void CreateNavigationViewModelWithIsActivityDropTrueAndTypeSourceExpectedOnlySourcesToBeInTree()
        {
            Init(false, true);
            _vm = CreateViewModel(true, enDsfActivityType.Source);
            _vm.AddEnvironment(_mockEnvironmentModel.Object);
            Assert.AreEqual(1, _vm.Root.Children[0].Children.Count);
            Assert.AreEqual("SOURCES", _vm.Root.Children[0].Children[0].DisplayName);
        }

        [TestMethod]
        public void CreateNavigationViewModelWithIsActivityDropTrueAndTypeAllExpectedAllItemsToBeInTree()
        {
            Init(false, true);
            _vm = CreateViewModel(true, enDsfActivityType.All);
            _vm.AddEnvironment(_mockEnvironmentModel.Object);
            Assert.AreEqual(3, _vm.Root.Children[0].Children.Count);
            Assert.AreEqual("WORKFLOWS", _vm.Root.Children[0].Children[0].DisplayName);
            Assert.AreEqual("SERVICES", _vm.Root.Children[0].Children[1].DisplayName);
            Assert.AreEqual("SOURCES", _vm.Root.Children[0].Children[2].DisplayName);
        }

        [TestMethod]
        public void CreateNavigationViewModelWithIsActivityDropTrueAndTypeWorkflowExpectedWorkflowsWithNoChilrenToBeInTree()
        {
            Init(true, true);
            _vm = CreateViewModel(true, enDsfActivityType.Workflow);
            _vm.AddEnvironment(_mockEnvironmentModel.Object);
            Assert.AreEqual(1, _vm.Root.Children[0].Children.Count);
            Assert.AreEqual(0, _vm.Root.Children[0].Children[0].Children[1].Children[0].Children.Count);
        }

        //2013.05.30: Ashley Lewis for bugs 9444+9445 - Dont show services from disconnected environments
        [TestMethod]
        public void CreateNavigationViewModelWithIsActivityDropTrueExpectedBothEnvironmentsVisibleOnlyResourcesForConnectedEnvironmentsToBeInTree()
        {
            InitTwoEnvironments(false, true);
            _vm = CreateViewModel(true);
            _vm.AddEnvironment(_mockEnvironmentModel.Object);
            _vm.AddEnvironment(_disconnectedMockEnvironmentModel.Object);
            Assert.AreEqual(2, _vm.Root.Children.Count, "Disconnected environment not added to NavigationViewModel");
            Assert.AreEqual(3, _vm.Root.Children[0].Children.Count, "Resources for connected environment not visible in NavigationViewModel");
            Assert.AreEqual(0, _vm.Root.Children[1].Children.Count, "Resources for disconnected environment are visible in NavigationViewModel");
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
            newResource.Setup(r => r.ResourceName).Returns("Cake");
            newResource.Setup(r => r.Environment)
                        .Returns(_mockEnvironmentModel.Object);

            var updatemsg = new UpdateResourceMessage(newResource.Object);
            _vm.Handle(updatemsg);

            Assert.IsTrue(_vm.SetNodeOverwrite(newResource.Object, true));
        }

        [TestMethod]
        [TestCategory("NavigationViewModel_Constructor")]
        [Description("Constructor must not allow null AsyncWorker.")]
        [Owner("Trevor Williams-Ros")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NavigationViewModel_UnitTest_ConstructorWithNullAsyncWorker_ThrowsArgumentNullException()
        {
            var eventPublisher = new Mock<IEventAggregator>();

            new NavigationViewModel(eventPublisher.Object, null, null, null);
        }

        [TestMethod]
        [TestCategory("NavigationViewModel_LoadEnvironmentResources")]
        [Description("LoadEnvironmentResources must load resources asynchronously.")]
        [Owner("Trevor Williams-Ros")]
        public void NavigationViewModel_UnitTest_LoadEnvironmentResources_InvokesAsyncWorker()
        {
            var eventPublisher = new Mock<IEventAggregator>();
            var environmentRepository = new Mock<IEnvironmentRepository>();
            var asyncWorker = AsyncWorkerTests.CreateVerifiableAsyncWorker();

            var nvm = new NavigationViewModel(eventPublisher.Object, asyncWorker.Object, null, environmentRepository.Object);

            var environmentModel = new Mock<IEnvironmentModel>();

            nvm.LoadEnvironmentResources(environmentModel.Object);

            asyncWorker.Verify(w => w.Start(It.IsAny<System.Action>(), It.IsAny<System.Action>()), "LoadEnvironmentResources did not load resources asynchronously.");
        }

        [TestMethod]
        public void NavigationViewModel_BringItemIntoView_Expects_SetTreeNodeIsSelectedTrueParentOpen()
        {
            Init(false, true);
            var resourceVm = _vm.Root.FindChild(_mockResourceModel.Object);
            _vm.BringItemIntoView(_mockResourceModel.Object);
            Assert.IsTrue(resourceVm.IsSelected);
            Assert.IsTrue(resourceVm.TreeParent.IsExpanded);
        }


        [TestMethod]
        public void NavigationViewModel_BringItemIntoViewWithNull_Expects_DoesNothing()
        {
            Init(false, true);
            var resourceVm = _vm.Root.FindChild(_mockResourceModel.Object);
            _vm.BringItemIntoView(null);
            Assert.IsFalse(resourceVm.IsSelected);
            Assert.IsFalse(resourceVm.TreeParent.IsExpanded);
        }

        [TestMethod]
        [TestCategory("NavigationViewModel_AddEnvironment")]
        [Description("NavigationViewModel AddEnvironment starts localhost environment connection's auto-connect heartbeat if not connected.")]
        [Owner("Trevor Williams-Ros")]
        public void NavigationViewModel_UnitTest_AddEnvironmentWithDisconnectedLocalhost_InitiatesAutoConnect()
        {
            var localhostConnection = new Mock<IEnvironmentConnection>();
            localhostConnection.Setup(e => e.StartAutoConnect()).Verifiable();

            var localhost = new Mock<IEnvironmentModel>();
            localhost.Setup(e => e.ID).Returns(Guid.Empty);
            localhost.Setup(e => e.IsConnected).Returns(false);
            localhost.Setup(e => e.CanStudioExecute).Returns(false);
            localhost.Setup(e => e.Connection).Returns(localhostConnection.Object);

            var envRepo = new Mock<IEnvironmentRepository>();
            envRepo.Setup(e => e.All()).Returns(new List<IEnvironmentModel>());
            envRepo.Setup(e => e.Source).Returns(localhost.Object);

            ImportService.CurrentContext = CompositionInitializer.InitializeNavigationViewModelTests(envRepo);

            var viewModel = CreateViewModel(envRepo.Object);
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

            ImportService.CurrentContext = CompositionInitializer.InitializeNavigationViewModelTests(envRepo);

            var viewModel = CreateViewModel(envRepo.Object);

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

            ImportService.CurrentContext = CompositionInitializer.InitializeNavigationViewModelTests(envRepo);

            var viewModel = new NavigationViewModel(publisher.Object, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object, null, envRepo.Object);
            foreach(var env in envList)
            {
                viewModel.AddEnvironment(env);
            }

            //------------Execute Test---------------------------
            viewModel.RemoveEnvironment(toBeRemoved.Object);

            //------------Assert Results-------------------------
            publisher.Verify(p => p.Publish(It.IsAny<SetActiveEnvironmentMessage>()));
            Assert.IsTrue(viewModel.Root.Children[0].IsSelected);
        }


        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("EnvironmentTreeViewModel_Disconnect")]
        public void EnvironmentTreeViewModel_Disconnect_InvokesNavigationViewModelSelectLocalhost()
        {
            //------------Setup for test--------------------------
            var publisher = new Mock<IEventAggregator>();
            publisher.Setup(p => p.Publish(It.IsAny<SetActiveEnvironmentMessage>())).Verifiable();

            var localhost = new Mock<IEnvironmentModel>();
            localhost.Setup(e => e.ID).Returns(Guid.Empty);
            localhost.Setup(e => e.Name).Returns("localhost");
            localhost.Setup(e => e.IsConnected).Returns(true);
            localhost.SetupGet(x => x.Connection.AppServerUri).Returns(new Uri("http://127.0.0.1/"));

            localhost.Setup(e => e.CanStudioExecute).Returns(true);

            var envModel = new Mock<IEnvironmentModel>();
            envModel.Setup(e => e.ID).Returns(Guid.NewGuid());
            envModel.Setup(e => e.Name).Returns("Other Server");
            envModel.Setup(e => e.IsConnected).Returns(true);
            envModel.Setup(e => e.CanStudioExecute).Returns(true);
            envModel.SetupGet(x => x.Connection.AppServerUri).Returns(new Uri("http://127.0.0.2/"));

            var envList = new List<IEnvironmentModel> { localhost.Object, envModel.Object };
            var envRepo = new Mock<IEnvironmentRepository>();
            envRepo.Setup(e => e.All()).Returns(envList);
            envRepo.Setup(e => e.Source).Returns(localhost.Object);

            ImportService.CurrentContext = CompositionInitializer.InitializeNavigationViewModelTests(envRepo);

            var viewModel = new NavigationViewModel(publisher.Object, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object, null, envRepo.Object);
            foreach(var env in envList)
            {
                viewModel.AddEnvironment(env);
            }

            var rootTreeViewModel = new RootTreeViewModel(publisher.Object) { Parent = viewModel };

            //------------Execute Test---------------------------


            var environmentTreeViewModel = new EnvironmentTreeViewModel(publisher.Object, rootTreeViewModel, envModel.Object, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object);
            environmentTreeViewModel.DisconnectCommand.Execute(null);

            //------------Assert Results-------------------------
            publisher.Verify(p => p.Publish(It.IsAny<SetActiveEnvironmentMessage>()));
            Assert.IsTrue(viewModel.Root.Children[0].IsSelected);
        }

        #region Private Test Methods

        void Init(bool addWizardChildToResource, bool shouldLoadResources)
        {
            SetupMockEnvironment(shouldLoadResources);

            Mock<IResourceRepository> mockResourceRepository;
            SetUpResources(addWizardChildToResource, out mockResourceRepository);

            _vm = CreateViewModel();
            _vm.AddEnvironment(_mockEnvironmentModel.Object);
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
            mock.SetupGet(x => x.Connection.AppServerUri).Returns(new Uri("http://localhost:3142/dsf"));
            mock.SetupGet(x => x.IsConnected).Returns(true);
            mock.Setup(x => x.Connection.ServerEvents).Returns(eventPublisher.Object);
            return mock;
        }

        void InitTwoEnvironments(bool addWizardChildToResource, bool shouldLoadResources)
        {
            SetupTwoMockEnvironments(shouldLoadResources);

            Mock<IResourceRepository> mockResourceRepository;
            SetUpResources(addWizardChildToResource, out mockResourceRepository);

            _vm = CreateViewModel();
            _vm.AddEnvironment(_mockEnvironmentModel.Object);
            _vm.AddEnvironment(_disconnectedMockEnvironmentModel.Object);
        }

        void SetupTwoMockEnvironments(bool shouldLoadResources)
        {
            _mockEnvironmentModel = GetMockEnvironment();
            _mockEnvironmentModel.SetupGet(x => x.CanStudioExecute).Returns(shouldLoadResources);
            _disconnectedMockEnvironmentModel = GetSecondMockEnvironment();
            _disconnectedMockEnvironmentModel.SetupGet(x => x.CanStudioExecute).Returns(shouldLoadResources);
        }

        Mock<IEnvironmentModel> GetSecondMockEnvironment()
        {
            var mock = new Mock<IEnvironmentModel>();
            mock.SetupGet(x => x.Connection.AppServerUri).Returns(new Uri("http://127.0.0.2/"));
            mock.SetupGet(x => x.ID).Returns(Guid.NewGuid);
            mock.SetupGet(x => x.IsConnected).Returns(false);
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
            _mockResourceModel.Setup(r => r.Environment).Returns(_mockEnvironmentModel.Object);

            _mockResourceModel1 = new Mock<IContextualResourceModel>();
            _mockResourceModel1.Setup(r => r.ResourceType).Returns(ResourceType.WorkflowService);
            _mockResourceModel1.Setup(r => r.Category).Returns("Testing2");
            _mockResourceModel1.Setup(r => r.ResourceName).Returns("Mock1");
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
            _mockResourceModel2.Setup(r => r.Environment).Returns(_mockEnvironmentModel.Object);

            Collection<IResourceModel> mockResources = new Collection<IResourceModel>
            {
                _mockResourceModel.Object,
                _mockResourceModel1.Object,
                _mockResourceModel2.Object
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

        static NavigationViewModel CreateViewModel(bool isFromActivityDrop = false, enDsfActivityType activityType = enDsfActivityType.All)
        {
            var importServiceContext = new ImportServiceContext();
            ImportService.CurrentContext = importServiceContext;

            ImportService.Initialize(new List<ComposablePartCatalog>());

            return CreateViewModel(EnvironmentRepository.Instance, isFromActivityDrop, activityType);
        }

        static NavigationViewModel CreateViewModel(IEnvironmentRepository environmentRepository, bool isFromActivityDrop = false, enDsfActivityType activityType = enDsfActivityType.All)
        {
            return new NavigationViewModel(new Mock<IEventAggregator>().Object, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object, null, environmentRepository, isFromActivityDrop, activityType);
        }

        #endregion


        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("NavigationViewModel_UpdateResource")]
        public void NavigationViewModel_UpdateResource_ServiceTypeFound_TreeViewModelItemWithActivityFullName()
        {
            //------------Setup for test--------------------------

            var eventPublisher = new Mock<IEventAggregator>();
            var environmentRepository = new Mock<IEnvironmentRepository>();
            var asyncWorker = AsyncWorkerTests.CreateSynchronousAsyncWorker();

            var nvm = new NavigationViewModel(eventPublisher.Object, asyncWorker.Object, null, environmentRepository.Object);

            var env = new Mock<IEnvironmentModel>();
            env.Setup(e => e.Connection.Alias).Returns("Expected Environment");
            env.Setup(e => e.Connection.AppServerUri).Returns(new Uri("http://10.0.0.1"));
            env.Setup(e => e.Connection.ServerEvents).Returns(new Mock<IEventPublisher>().Object);

            var newResource = new Mock<IContextualResourceModel>();
            newResource.Setup(res => res.Category).Returns("Expected Category");
            newResource.Setup(res => res.ResourceName).Returns("Expected Resource Name");
            newResource.Setup(res => res.Environment).Returns(env.Object);

            nvm.Root.Children.Add(new CategoryTreeViewModel(eventPublisher.Object, nvm.Root, "Workflows", ResourceType.WorkflowService));

            new EnvironmentTreeViewModel(eventPublisher.Object, nvm.Root, env.Object, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object);

            //------------Execute Test---------------------------
            nvm.UpdateResource(newResource.Object);

            //------------Assert Results-------------------------
            var workflowsNode = nvm.Root.Children[2];
            var serverNode = workflowsNode.Children[0];
            var categoryNode = serverNode.Children[0];
            var resourceNode = (ResourceTreeViewModel)categoryNode.Children[0];
            Assert.IsNotNull(resourceNode.ActivityFullName);
        }
    }
}