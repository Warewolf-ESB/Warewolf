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

        static object _lock = new object();
        Mock<IEnvironmentModel> mockEnvironmentModel;
        Mock<IEnvironmentModel> disconnectedMockEnvironmentModel;
        Mock<IContextualResourceModel> mockResourceModel;
        Mock<IContextualResourceModel> mockResourceModel1;
        Mock<IContextualResourceModel> mockResourceModel2;
        NavigationViewModel vm;

        #endregion Test Variables

        #region Refresh Test Variables

        Mock<IEnvironmentModel> reMockEnvironmentModel;
        Mock<IEnvironmentModel> reMockEnvironmentModel1;
        Mock<IResourceRepository> reMockResourceRepository;

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
            //Monitor.Enter(_lock);

        }

        [TestCleanup]
        public void TestCleanUp()
        {
            //Monitor.Exit(_lock);
        }

        #endregion

        #region Loading Resources

        [TestMethod]
        [TestCategory("NavigationViewModel_LoadResourcesAsync")]
        [Description("Async load calls OnResourcesLoaded event")]
        [Owner("Ashley Lewis")]
        // ReSharper disable InconsistentNaming
        public void NavigationViewModel_UnitTest_LoadResourcesAsync_InvokesEventHandler()
        // ReSharper restore InconsistentNaming
        {
            //isolate unit
            Init();

            vm.LoadResourcesCompleted += (sender, args) => Assert.AreEqual(1, 1, "Navigation view model did not execute LoadResourcesCompleted event after async load");


            //test unit
            vm.LoadEnvironmentResources(mockEnvironmentModel.Object);

            //test result
        }

        #endregion

        #region Updating Resources

        void Init()
        {

            mockEnvironmentModel = new Mock<IEnvironmentModel>();
            mockEnvironmentModel.SetupGet(x => x.Connection.AppServerUri).Returns(new Uri("http://127.0.0.1/"));
            mockEnvironmentModel.SetupGet(x => x.Connection.AppServerUri).Returns(new Uri("http://127.0.0.1/"));
            mockEnvironmentModel.SetupGet(x => x.IsConnected).Returns(true);

            // setup env repo
            var repo = new Mock<IEnvironmentRepository>();
            repo.Setup(l => l.Load()).Verifiable();

            IList<IEnvironmentModel> models = new List<IEnvironmentModel>();
            repo.Setup(l => l.All()).Returns(models);

            // ImportService.CurrentContext = CompositionInitializer.InitializeNavigationViewModelTests(repo);

            mockResourceModel = new Mock<IContextualResourceModel>();
            mockResourceModel.Setup(r => r.ResourceType).Returns(ResourceType.WorkflowService);
            mockResourceModel.Setup(r => r.Category).Returns("Testing");
            mockResourceModel.Setup(r => r.ResourceName).Returns("Mock");
            mockResourceModel.Setup(r => r.Environment).Returns(mockEnvironmentModel.Object);

            mockResourceModel1 = new Mock<IContextualResourceModel>();
            mockResourceModel1.Setup(r => r.ResourceType).Returns(ResourceType.WorkflowService);
            mockResourceModel1.Setup(r => r.Category).Returns("Testing2");
            mockResourceModel1.Setup(r => r.ResourceName).Returns("Mock1");
            mockResourceModel1.Setup(r => r.Environment).Returns(mockEnvironmentModel.Object);

            mockResourceModel2 = new Mock<IContextualResourceModel>();
            mockResourceModel2.Setup(r => r.ResourceType).Returns(ResourceType.Service);
            mockResourceModel2.Setup(r => r.Category).Returns("Testing2");
            mockResourceModel2.Setup(r => r.ResourceName).Returns("Mock2");
            mockResourceModel2.Setup(r => r.Environment).Returns(mockEnvironmentModel.Object);

            var mockResourceRepository = new Mock<IResourceRepository>();
            mockResourceRepository.Setup(r => r.All()).Returns(
                new Collection<IResourceModel>
                {
                    mockResourceModel.Object,
                    mockResourceModel1.Object,
                    mockResourceModel2.Object
                });

            mockEnvironmentModel.SetupGet(x => x.ResourceRepository).Returns(mockResourceRepository.Object);
            mockEnvironmentModel.Setup(x => x.LoadResources());

            vm = CreateViewModel();
            vm.AddEnvironment(mockEnvironmentModel.Object);
        }

        [TestMethod]
        public void UpdateResourceMessage_WhenResourceExistsOldCategory_Expects_ResourceChanged()
        {
            // Kick the test off on the dispatcher worker thread synchronously which will block until the work is competed

            //var reset = new AutoResetEvent(false);
            UpdateResourceMessage updatemsg;
            ITreeNode updatedResource = null;
            //            ThreadExecuter.RunCodeAsSTA(reset,
            //                () =>
            //                {
            //                  
            //                }
            //                );
            //            reset.WaitOne();
            Init(false, true);
            ITreeNode oldResoure = vm.Root.FindChild(mockResourceModel.Object);

            //Assert.AreEqual("Mock", oldResoure.DisplayName);

            mockResourceModel.Setup(r => r.ResourceName).Returns("MockChangedName");
            updatemsg = new UpdateResourceMessage(mockResourceModel.Object);

            vm.Handle(updatemsg);
            updatedResource = vm.Root.FindChild(mockResourceModel.Object);
            Assert.AreEqual("MockChangedName", updatedResource.DisplayName);
        }

        [TestMethod]
        public void UpdateResourceMessage_WhenResourceUnchanged_Expects_UnchangedResource()
        {
            ITreeNode updatedResource = null;

            //            var reset = new AutoResetEvent(false);
            //            ThreadExecuter.RunCodeAsSTA(reset,
            //                () =>
            //                {
            //                   
            //                });
            //            reset.WaitOne();
            Init(false, true);
            ITreeNode oldResoure = vm.Root.FindChild(mockResourceModel.Object);

            Assert.AreEqual("Mock", oldResoure.DisplayName);

            var updatemsg = new UpdateResourceMessage(mockResourceModel.Object);

            vm.Handle(updatemsg);
            updatedResource = vm.Root.FindChild(mockResourceModel.Object);
            Assert.AreEqual("Mock", updatedResource.DisplayName);
        }

        [TestMethod]
        public void UpdateResourceMessage_WhenResourceExistsChangedCategory_Expects_CategoryChanged()
        {
            ITreeNode updatedCategory = null;
            ITreeNode resourceVM = null;

            Init(false, true);
            ITreeNode serviceTypeVM = vm.Root.FindChild(ResourceType.WorkflowService);
            ITreeNode oldCategory = serviceTypeVM.FindChild("Testing");
            resourceVM = oldCategory.FindChild(mockResourceModel.Object);

            Assert.IsTrue(ReferenceEquals(oldCategory, resourceVM.TreeParent));
            Assert.IsTrue(String.Compare(oldCategory.DisplayName, "Testing",
                StringComparison.InvariantCultureIgnoreCase) ==
                            0);

            mockResourceModel.Setup(r => r.Category).Returns("Testing2");

            var updatemsg = new UpdateResourceMessage(mockResourceModel.Object);

            vm.Handle(updatemsg);

            updatedCategory = serviceTypeVM.FindChild("Testing2");
            resourceVM = updatedCategory.FindChild(mockResourceModel.Object);
            Assert.IsTrue(ReferenceEquals(updatedCategory, resourceVM.TreeParent));
            Assert.IsTrue(String.Compare(updatedCategory.DisplayName, "Testing2",
                StringComparison.InvariantCultureIgnoreCase) ==
                          0);
            Assert.AreEqual(updatedCategory.ChildrenCount, 2);
        }

        [TestMethod]
        public void UpdateResourceMessage_WhenNewCategory_Expects_CategoryAdded()
        {
            ITreeNode updatedCategory = null;

            Init(false, true);
            ITreeNode serviceTypeVM = vm.Root.FindChild(ResourceType.WorkflowService);

            mockResourceModel.Setup(r => r.Category).Returns("Testing5");

            var updatemsg = new UpdateResourceMessage(mockResourceModel.Object);

            vm.Handle(updatemsg);

            var tryGetNewCategory = vm.Root.Children[0].Children[0].Children.FirstOrDefault(child => child.DisplayName.ToUpper() == mockResourceModel.Object.Category.ToUpper());

            updatedCategory = serviceTypeVM.FindChild("Testing5");

            Assert.IsTrue(ReferenceEquals(updatedCategory, tryGetNewCategory));
            Assert.IsTrue(String.Compare(updatedCategory.DisplayName, "Testing5",
                StringComparison.InvariantCultureIgnoreCase) ==
                          0);
            Assert.AreEqual(updatedCategory.ChildrenCount, 1);
        }

        [TestMethod]
        public void UpdateResourceMessage_WhenOldCategoryBecomesEmpty_Expects_CategoryDeleted()
        {
            ITreeNode updatedCategory = null;
            Init(false, true);
            ITreeNode serviceTypeVM = vm.Root.FindChild(ResourceType.WorkflowService);

            mockResourceModel.Setup(r => r.Category).Returns("Testing5");

            var updatemsg = new UpdateResourceMessage(mockResourceModel.Object);

            vm.Handle(updatemsg);

            updatedCategory = serviceTypeVM.FindChild("Testing");
            Assert.IsNull(updatedCategory);
            Assert.IsTrue(vm.Root.ChildrenCount == 3);
        }

        [TestMethod]
        public void UpdateResourceMessage_WhenOldCategoryNotEmpty_Expects_CategoryNotDeleted()
        {
            ITreeNode updatedCategory = null;

            Init(false, true);
            ITreeNode serviceTypeVM = vm.Root.FindChild(ResourceType.WorkflowService);

            mockResourceModel.Setup(r => r.Category).Returns("Testing2");

            var updatemsg = new UpdateResourceMessage(mockResourceModel.Object);
            vm.Handle(updatemsg);

            mockResourceModel.Setup(r => r.Category).Returns("Testing3");

            updatemsg = new UpdateResourceMessage(mockResourceModel.Object);

            vm.Handle(updatemsg);

            updatedCategory = serviceTypeVM.FindChild("Testing2");

            Assert.IsTrue(updatedCategory.ChildrenCount == 1);
        }

        [TestMethod]
        public void UpdateResourceMessage_WhenNewResource_Expects_ResourceAdded()
        {
            ITreeNode categoryVM = null;
            ITreeNode resourceVM = null;
            Init(false, true);
            var newResource = new Mock<IContextualResourceModel>();
            newResource.Setup(r => r.ResourceType)
                        .Returns(ResourceType.WorkflowService);
            newResource.Setup(r => r.Category).Returns("Testing");
            newResource.Setup(r => r.ResourceName).Returns("Cake");
            newResource.Setup(r => r.Environment)
                        .Returns(mockEnvironmentModel.Object);

            var updatemsg = new UpdateResourceMessage(newResource.Object);
            vm.Handle(updatemsg);

            ITreeNode serviceTypeVM = vm.Root.FindChild(ResourceType.WorkflowService);
            categoryVM = serviceTypeVM.FindChild("Testing");
            resourceVM = vm.Root.FindChild(newResource.Object);
            Assert.IsTrue(resourceVM.DisplayName == "Cake");
            Assert.IsTrue(categoryVM.ChildrenCount == 2);
            Assert.IsTrue(vm.Root.ChildrenCount == 4);
        }

        [TestMethod]
        public void UpdateResourceMessage_WhenNewResourceAndNewCategory_Expects_CategoryAdded()
        {
            ITreeNode categoryVM = null;
            //            var reset = new AutoResetEvent(false);
            //            ThreadExecuter.RunCodeAsSTA(reset,
            //                () =>
            //                {
            //                    
            //
            //                });
            //            reset.WaitOne();
            Init(false, true);
            var newResource = new Mock<IContextualResourceModel>();
            newResource.Setup(r => r.ResourceType)
                       .Returns(ResourceType.WorkflowService);
            newResource.Setup(r => r.Category).Returns("Testing4");
            newResource.Setup(r => r.ResourceName).Returns("Cake");
            newResource.Setup(r => r.Environment)
                       .Returns(mockEnvironmentModel.Object);

            var updatemsg = new UpdateResourceMessage(newResource.Object);
            vm.Handle(updatemsg);

            ITreeNode serviceTypeVM = vm.Root.FindChild(ResourceType.WorkflowService);
            categoryVM = serviceTypeVM.FindChild("Testing4");
            Assert.IsTrue(String.Compare(categoryVM.DisplayName, "Testing4",
                StringComparison.InvariantCultureIgnoreCase) ==
                          0);

            Assert.IsTrue(categoryVM.ChildrenCount == 1);
            Assert.IsTrue(vm.Root.ChildrenCount == 4);
        }

        [TestMethod]
        public void AddEnvironmentWithShouldLoadResourcesTrueExpectedRootAndChildrenCreated()
        {
            ITreeNode workflowServiceTypeVM = null;
            ITreeNode serviceTypeVM = null;
            ITreeNode categoryVM = null;
            ITreeNode resourceVM = null;

            //            var reset = new AutoResetEvent(false);
            //            ThreadExecuter.RunCodeAsSTA(reset,
            //                () =>
            //                {
            //                   
            //                });
            //            reset.WaitOne();
            Init(false, true);
            workflowServiceTypeVM =
                vm.Root.FindChild(ResourceType.WorkflowService);
            serviceTypeVM = vm.Root.FindChild(ResourceType.Service);
            categoryVM = workflowServiceTypeVM.FindChild("Testing");
            resourceVM = categoryVM.FindChild(mockResourceModel.Object);
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
            Assert.AreEqual(0, vm.Root.Children.Count());
        }

        [TestMethod]
        public void AddEnvironmentWithShouldLoadResourcesTrueExpectedEnvironmentAdded()
        {
            //            var reset = new AutoResetEvent(false);
            //            ThreadExecuter.RunCodeAsSTA(reset,
            //                () =>
            //                {
            //                    
            //                });
            //            reset.WaitOne();
            Init(false, true);
            Assert.AreEqual(1, vm.Root.Children.Count());
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
            new EnvironmentTreeViewModel(eventAggregator, vm.Root, environment.Object, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object);
            var newResource = new Mock<IContextualResourceModel>();
            newResource.Setup(res => res.Category).Returns("Expected Category");
            newResource.Setup(res => res.ResourceName).Returns("Expected Resource Name");
            newResource.Setup(res => res.Environment).Returns(environment.Object);
            new ResourceTreeViewModel(eventAggregator, null, newResource.Object);
            // ReSharper restore ObjectCreationAsStatement

            //------------Execute Test---------------------------
            vm.UpdateResource(newResource.Object);

            // Assert Category Node And Type Node Removed
            Assert.AreEqual("WORKFLOWS", vm.Root.Children[0].Children[0].DisplayName, "New service type not created for new resource");
            Assert.AreEqual("EXPECTED CATEGORY", vm.Root.Children[0].Children[0].Children[0].DisplayName, "New category not created for new resource");
            Assert.AreEqual("Expected Resource Name", vm.Root.Children[0].Children[0].Children[0].Children[0].DisplayName, "New resource not added to the tree");
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

            new EnvironmentTreeViewModel(eventAggregator, vm.Root, localEnvironment.Object, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object);
            new ServiceTypeTreeViewModel(eventAggregator, vm.Root.Children[0], ResourceType.WorkflowService);
            new CategoryTreeViewModel(eventAggregator, vm.Root.Children[0].Children[0], expectedCategoryName, ResourceType.WorkflowService);
            vm.Root.Children[0].Children[0].Children[0].Children.Add(localResource.Object);

            //------------Execute Test---------------------------
            vm.UpdateResource(remoteResource.Object);

            // Assert Does Not Update For Delete Remote Resource
            Assert.IsNotNull(vm.Root.Children[0].Children[0].Children[0].Children.FirstOrDefault(child => child.DisplayName == localResource.Object.DisplayName));
        }

        #endregion Updating Resources

        #region Filtering

        [TestMethod]
        public void FilteredNavigationViewModel_AfterReload_Expects_FilteredItemsInTreeWithIsFilteredTrue()
        {
            ITreeNode resourceVM = null;

            //            var reset = new AutoResetEvent(false);
            //            ThreadExecuter.RunCodeAsSTA(reset,
            //                () =>
            //                {
            //                   
            //
            //                });
            //            reset.WaitOne();

            Init(false, true);
            resourceVM = vm.Root.FindChild(mockResourceModel.Object);

            Assert.AreEqual(3, vm.Root.ChildrenCount);
            Assert.IsTrue(resourceVM.IsFiltered == false);

            vm.UpdateSearchFilter("Mock2");
            vm.Root.NotifyOfFilterPropertyChanged(false);

            //Thread.Sleep(100);

            Assert.AreEqual(1, vm.Root.ChildrenCount);
            Assert.IsTrue(resourceVM.IsFiltered);

            vm.LoadEnvironmentResources(mockEnvironmentModel.Object);
            vm.Root.NotifyOfFilterPropertyChanged(false);

            //Thread.Sleep(100);

            resourceVM = vm.Root.FindChild(mockResourceModel.Object);

            // Thread.Sleep(100);

            Assert.AreEqual(1, vm.Root.ChildrenCount);
            Assert.IsTrue(resourceVM.IsFiltered);

            vm.UpdateSearchFilter("Mock");

            // Thread.Sleep(100);

            Assert.AreEqual(3, vm.Root.ChildrenCount);
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
                    ITreeNode resourceVM = vm.Root.FindChild(mockResourceModel.Object);
                    ITreeNode resourceVM2_1 = vm.Root.FindChild(mockResourceModel1.Object);
                    resourceVM2_2 = vm.Root.FindChild(mockResourceModel2.Object);

                    Assert.IsTrue(resourceVM.IsFiltered == false);
                    Assert.IsTrue(resourceVM2_1.IsFiltered == false);
                    Assert.IsTrue(resourceVM2_2.IsFiltered == false);
                    Assert.IsTrue(resourceVM.TreeParent.IsExpanded == false);
                    Assert.IsTrue(resourceVM2_1.TreeParent.IsExpanded == false);
                    Assert.IsTrue(resourceVM2_2.TreeParent.IsExpanded == false);

                    vm.UpdateSearchFilter("Mock2");
                    vm.Root.NotifyOfFilterPropertyChanged(false);

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
                    resourceVM = vm.Root.FindChild(mockResourceModel.Object);
                    ITreeNode resourceVM2_1 = vm.Root.FindChild(mockResourceModel1.Object);
                    ITreeNode resourceVM2_2 = vm.Root.FindChild(mockResourceModel2.Object);

                    Assert.IsTrue(resourceVM.IsFiltered == false);
                    Assert.IsTrue(resourceVM2_1.IsFiltered == false);
                    Assert.IsTrue(resourceVM2_2.IsFiltered == false);
                    Assert.IsTrue(resourceVM.TreeParent.IsExpanded == false);
                    Assert.IsTrue(resourceVM2_1.TreeParent.IsExpanded == false);
                    Assert.IsTrue(resourceVM2_2.TreeParent.IsExpanded == false);

                    vm.UpdateSearchFilter("Mock2");
                    vm.Root.NotifyOfFilterPropertyChanged(false);

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
                    resourceVM = vm.Root.FindChild(mockResourceModel.Object);
                    resourceVM2_1 = vm.Root.FindChild(mockResourceModel1.Object);
                    resourceVM2_2 = vm.Root.FindChild(mockResourceModel2.Object);

                    Assert.IsTrue(resourceVM.IsFiltered == false);
                    Assert.IsTrue(resourceVM2_1.IsFiltered == false);
                    Assert.IsTrue(resourceVM2_2.IsFiltered == false);
                    Assert.IsTrue(resourceVM.TreeParent.IsExpanded == false);
                    Assert.IsTrue(resourceVM2_1.TreeParent.IsExpanded == false);
                    Assert.IsTrue(resourceVM2_2.TreeParent.IsExpanded == false);

                    vm.UpdateSearchFilter("Mock2");

                    vm.UpdateSearchFilter("");
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
                    vm.UpdateSearchFilter("ZD");
                });
            reset.WaitOne();

            Assert.IsTrue(vm.Root.IsFiltered == false);
            Assert.IsTrue(vm.Root.IsExpanded);
            Assert.IsTrue(vm.Root.Children.ToList().All(c => c.IsFiltered == false));
        }

        [TestMethod]
        public void NavigationViewModel_WhereNoContent_Expects_RootAndEnvironmentNotFiltered()
        {
            var reset = new AutoResetEvent(false);
            ThreadExecuter.RunCodeAsSTA(reset,
                () =>
                {
                    Init(false, true);
                    ITreeNode resourceVM = vm.Root.FindChild(mockResourceModel.Object);
                    ITreeNode resourceVM2_1 = vm.Root.FindChild(mockResourceModel1.Object);
                    ITreeNode resourceVM2_2 = vm.Root.FindChild(mockResourceModel2.Object);

                    resourceVM.TreeParent.Remove(resourceVM);
                    resourceVM2_1.TreeParent.Remove(resourceVM2_1);
                    resourceVM2_2.TreeParent.Remove(resourceVM2_2);

                    vm.UpdateSearchFilter("");
                });
            reset.WaitOne();

            Assert.IsTrue(vm.Root.ChildrenCount == 0);
            Assert.IsTrue(vm.Root.IsFiltered == false);
            Assert.IsTrue(vm.Root.IsExpanded);
            Assert.IsTrue(vm.Root.Children.ToList().All(c => c.IsFiltered == false));
            Assert.IsTrue(vm.Root.Children.ToList().All(c => c.ChildrenCount == 0));
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
                    vm.UpdateSearchFilter("zd");
                    vm.Root.NotifyOfFilterPropertyChanged(false);

                    resourceVM = vm.Root.FindChild(mockResourceModel.Object);
                    resourceVM2_1 = vm.Root.FindChild(mockResourceModel1.Object);
                    resourceVM2_2 = vm.Root.FindChild(mockResourceModel2.Object);

                });
            reset.WaitOne();

            Assert.IsTrue(resourceVM.TreeParent.IsFiltered);
            Assert.IsTrue(resourceVM2_1.TreeParent.IsFiltered);
            Assert.IsTrue(resourceVM2_2.TreeParent.IsFiltered);
        }

        [TestMethod]
        public void Filter_Expects_FilteredCategories_WithResources_IsNotFiltered_AndExpanded()
        {
            ITreeNode nonMatchingNode1 = null;
            ITreeNode matchingNode = null;

            var reset = new AutoResetEvent(false);
            ThreadExecuter.RunCodeAsSTA(reset,
                () =>
                {
                    Init(false, true);
                    nonMatchingNode1 = vm.Root.FindChild(mockResourceModel.Object);
                    matchingNode = vm.Root.FindChild(mockResourceModel2.Object);

                    vm.UpdateSearchFilter("Mock2");

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
                        vm.Root.FindChild(mockResourceModel.Object).TreeParent;
                    matchingCategory =
                        vm.Root.FindChild(mockResourceModel1.Object).TreeParent;

                    vm.UpdateSearchFilter("Testing2");

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
            //            var reset = new AutoResetEvent(false);
            //            ThreadExecuter.RunCodeAsSTA(reset,
            //                () =>
            //                {
            //                    
            //
            //                });
            //            reset.WaitOne();

            Init(false, true);
            // FromCurrentSynchronizationContext will now resolve to the dispatcher thread here
            mockEnvironmentModel.SetupGet(c => c.IsConnected).Returns(true);
            mockEnvironmentModel.SetupGet(c => c.Name).Returns("Mock");

            Assert.IsTrue(vm.Environments.Count == 1);
            Assert.IsTrue(vm.Root.ChildrenCount > 0);

            vm.RemoveEnvironment(mockEnvironmentModel.Object);
            Assert.IsTrue(vm.Environments.Count == 0);
            Assert.IsTrue(vm.Root.ChildrenCount == 0);

        }

        [TestMethod]
        public void NoExceptionWhenEnvironmentNotInEnvironmentList()
        {
            var reset = new AutoResetEvent(false);
            ThreadExecuter.RunCodeAsSTA(reset,
                () =>
                {
                    Init(false, true);
                    // FromCurrentSynchronizationContext will now resolve to the dispatcher thread here
                    mockEnvironmentModel.SetupGet(c => c.IsConnected).Returns(true);
                    mockEnvironmentModel.SetupGet(c => c.Name).Returns("Mock");

                    Assert.IsTrue(vm.Environments.Count == 1);
                    Assert.IsTrue(vm.Root.ChildrenCount > 0);

                    var nonAddedMock = GetMockEnvironment();
                    nonAddedMock.Setup(m => m.ID).Returns(Guid.NewGuid);

                    vm.RemoveEnvironment(nonAddedMock.Object);

                });
            reset.WaitOne();

            Assert.IsTrue(vm.Environments.Count == 1);
            Assert.IsTrue(vm.Root.ChildrenCount > 0);

        }

        #endregion remove

        #region Refresh Environments Tests

        void RefreshTestsSetup()
        {
            Guid firstEnvId = Guid.NewGuid();
            reMockEnvironmentModel = GetMockEnvironment();
            reMockEnvironmentModel.Setup(x => x.ID).Returns(firstEnvId);
            reMockEnvironmentModel.Setup(x => x.IsConnected).Returns(true);

            Guid secondEnvId = Guid.NewGuid();
            reMockEnvironmentModel1 = GetMockEnvironment();
            reMockEnvironmentModel1.Setup(x => x.ID).Returns(secondEnvId);
            reMockEnvironmentModel1.Setup(x => x.IsConnected).Returns(false);

            // setup env repo
            var repo = new Mock<IEnvironmentRepository>();
            repo.Setup(l => l.Load()).Verifiable();

            IList<IEnvironmentModel> models = new List<IEnvironmentModel>();
            repo.Setup(l => l.All()).Returns(models);

            // ImportService.CurrentContext = CompositionInitializer.InitializeNavigationViewModelTests(repo);

            mockResourceModel = new Mock<IContextualResourceModel>();
            mockResourceModel.Setup(r => r.ResourceType).Returns(ResourceType.WorkflowService);
            mockResourceModel.Setup(r => r.Category).Returns("Testing");
            mockResourceModel.Setup(r => r.ResourceName).Returns("Mock");
            mockResourceModel.Setup(r => r.Environment).Returns(reMockEnvironmentModel.Object);

            mockResourceModel1 = new Mock<IContextualResourceModel>();
            mockResourceModel1.Setup(r => r.ResourceType).Returns(ResourceType.WorkflowService);
            mockResourceModel1.Setup(r => r.Category).Returns("Testing2");
            mockResourceModel1.Setup(r => r.ResourceName).Returns("Mock1");
            mockResourceModel1.Setup(r => r.Environment).Returns(reMockEnvironmentModel.Object);

            mockResourceModel2 = new Mock<IContextualResourceModel>();
            mockResourceModel2.Setup(r => r.ResourceType).Returns(ResourceType.Service);
            mockResourceModel2.Setup(r => r.Category).Returns("Testing2");
            mockResourceModel2.Setup(r => r.ResourceName).Returns("Mock2");
            mockResourceModel2.Setup(r => r.Environment).Returns(reMockEnvironmentModel.Object);

            reMockResourceRepository = new Mock<IResourceRepository>();
            reMockResourceRepository.Setup(r => r.All()).Returns(
                new Collection<IResourceModel>
                {
                    mockResourceModel.Object,
                    mockResourceModel1.Object,
                    mockResourceModel2.Object
                });
            reMockResourceRepository.Setup(x => x.UpdateWorkspace(It.IsAny<IList<IWorkspaceItem>>())).Verifiable();

            reMockEnvironmentModel.SetupGet(x => x.ResourceRepository).Returns(reMockResourceRepository.Object);
            reMockEnvironmentModel.Setup(x => x.LoadResources());

            reMockEnvironmentModel1.SetupGet(x => x.ResourceRepository).Returns(reMockResourceRepository.Object);
            reMockEnvironmentModel1.Setup(x => x.LoadResources());

            vm = CreateViewModel();
            vm.AddEnvironment(reMockEnvironmentModel.Object);
            vm.AddEnvironment(reMockEnvironmentModel1.Object);
        }

        [TestMethod]
        public void RefreshSingleEnvironmentTestExpectedRefreshOfOneEnvironmentOnly()
        {
            RefreshTestsSetup();

            var eventAggregator = new Mock<IEventAggregator>().Object;

            var rootTreeViewModel = new RootTreeViewModel(eventAggregator);
            rootTreeViewModel.Parent = vm;

            var envConn = new Mock<IEnvironmentConnection>();
            envConn.Setup(e => e.IsConnected).Returns(true);
            envConn.Setup(e => e.ServerEvents).Returns(new Mock<IEventPublisher>().Object);
            envConn.Setup(e => e.AppServerUri).Returns(new Uri("http://127.0.0.1/"));

            var envModel = new EnvironmentModel(new Mock<IEventAggregator>().Object, Guid.NewGuid(), envConn.Object, reMockResourceRepository.Object);
            var environmentTreeViewModel = new EnvironmentTreeViewModel(eventAggregator, rootTreeViewModel, envModel, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object);
            environmentTreeViewModel.RefreshCommand.Execute(null);

            reMockResourceRepository.Verify(x => x.UpdateWorkspace(It.IsAny<IList<IWorkspaceItem>>()), Times.Exactly(1));

        }

        [TestMethod]
        public void RefreshAllEnvironmentTestExpectedSecondEnvironmentStillNotConnected()
        {
            RefreshTestsSetup();
            vm.UpdateWorkspaces();
            Assert.IsTrue(vm.Environments[0].IsConnected);
            Assert.IsTrue(!vm.Environments[1].IsConnected);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("NavigationViewModel")]
        // ReSharper disable InconsistentNaming
        public void RefreshEnvironment_UnitTest_WhereTreeIsPopulatedAlready_ExpectedSameObjectsInTree()
        // ReSharper restore InconsistentNaming
        {
            RefreshTestsSetup();
            reMockEnvironmentModel.Setup(c => c.CanStudioExecute).Returns(true);
            vm.LoadEnvironmentResources(reMockEnvironmentModel.Object);
            ResourceTreeViewModel expected = vm.Root.Children[0].Children[0].Children[0].Children[0] as ResourceTreeViewModel;
            vm.LoadEnvironmentResources(reMockEnvironmentModel.Object);
            ResourceTreeViewModel actual = vm.Root.Children[0].Children[0].Children[0].Children[0] as ResourceTreeViewModel;
            Assert.AreEqual(expected, actual, "The objects are not the same object");
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("NavigationViewModel")]
        // ReSharper disable InconsistentNaming
        public void RefreshEnvironment_UnitTest_WhereOneExtraResource_ExpectedOneExtraResourceInTree()
        // ReSharper restore InconsistentNaming
        {
            RefreshTestsSetup();
            reMockEnvironmentModel.Setup(c => c.CanStudioExecute).Returns(true);
            vm.LoadEnvironmentResources(reMockEnvironmentModel.Object);
            int expected = vm.Root.Children[0].Children[0].Children.Count;

            Mock<IContextualResourceModel> mockResourceModel3 = new Mock<IContextualResourceModel>();
            mockResourceModel3.Setup(r => r.ResourceType).Returns(ResourceType.WorkflowService);
            mockResourceModel3.Setup(r => r.Category).Returns("Testing3");
            mockResourceModel3.Setup(r => r.ResourceName).Returns("Mock3");
            mockResourceModel3.Setup(r => r.Environment).Returns(reMockEnvironmentModel.Object);

            reMockResourceRepository = new Mock<IResourceRepository>();
            reMockResourceRepository.Setup(r => r.All()).Returns(
                new Collection<IResourceModel>
                {
                    mockResourceModel.Object,
                    mockResourceModel1.Object,
                    mockResourceModel2.Object,
                    mockResourceModel3.Object
                });
            reMockEnvironmentModel.Setup(c => c.ResourceRepository).Returns(reMockResourceRepository.Object);
            vm.LoadEnvironmentResources(reMockEnvironmentModel.Object);
            int actual = vm.Root.Children[0].Children[0].Children.Count;
            Assert.IsTrue(actual == expected + 1, "The resource was not added to the tree");
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("NavigationViewModel")]
        // ReSharper disable InconsistentNaming
        public void RefreshEnvironment_UnitTest_WhereOneLessResource_ExpectedOneLessResourceInTree()
        // ReSharper restore InconsistentNaming
        {
            RefreshTestsSetup();
            reMockEnvironmentModel.Setup(c => c.CanStudioExecute).Returns(true);
            vm.LoadEnvironmentResources(reMockEnvironmentModel.Object);

            reMockResourceRepository = new Mock<IResourceRepository>();
            reMockResourceRepository.Setup(r => r.All()).Returns(
                new Collection<IResourceModel>
                {
                    mockResourceModel.Object
                });
            reMockEnvironmentModel.Setup(c => c.ResourceRepository).Returns(reMockResourceRepository.Object);
            vm.LoadEnvironmentResources(reMockEnvironmentModel.Object);

            Assert.IsTrue(vm.Root.Children[0].Children[0].Children[0].Children.Count == 0, "The resource was not removed to the tree");
        }

        [TestMethod]
        [TestCategory("NavigationViewModel")]
        [Description("Refresh all environments connects to localhost")]
        [Owner("Ashley Lewis")]
        // ReSharper disable InconsistentNaming
        public void NavigationViewModel_UpdateWorkspaces_DisconnectedLocalhostServer_LocalhostServerConnected()
        // ReSharper restore InconsistentNaming
        {
            //isolate UpdateWorkspaces as a functional unit
            RefreshTestsSetup();
            reMockEnvironmentModel.Setup(c => c.CanStudioExecute).Returns(true);

            //run unit
            vm.UpdateWorkspaces();

            //assert both localhost servers are connected
            Assert.IsTrue(vm.Root.Children[0].Children[0].IsConnected, "Refresh does not autoconnect to localhost");
            Assert.IsTrue(vm.Root.Children[0].Children[1].IsConnected, "Refresh does not autoconnect to localhost");
        }

        #endregion

        #region Disconnect

        [TestMethod]
        public void EnvironmentNodeDisconnect_Expect_NodeRemovedFromRoot()
        {
            Init(false, true);
            // FromCurrentSynchronizationContext will now resolve to the dispatcher thread here
            mockEnvironmentModel.SetupGet(c => c.IsConnected).Returns(true);
            mockEnvironmentModel.SetupGet(c => c.Name).Returns("Mock");

            Assert.IsTrue(vm.Root.ChildrenCount != 0);

            var message =
                new EnvironmentDisconnectedMessage(mockEnvironmentModel.Object);
            vm.Handle(message);
            Assert.IsTrue(vm.Root.ChildrenCount == 0);
        }

        [TestMethod]
        public void EnvironmentNodeDisconnectExpectSetIsRefreshingToFalse()
        {
            var reset = new AutoResetEvent(false);
            ThreadExecuter.RunCodeAsSTA(reset,
                () =>
                {
                    Init(false, true);
                    // FromCurrentSynchronizationContext will now resolve to the dispatcher thread here
                    mockEnvironmentModel.SetupGet(c => c.IsConnected).Returns(true);
                    mockEnvironmentModel.SetupGet(c => c.Name).Returns("Mock");

                    Assert.IsTrue(vm.Root.ChildrenCount != 0);

                    var message =
                        new EnvironmentDisconnectedMessage(mockEnvironmentModel.Object);
                    vm.Handle(message);
                    Assert.IsFalse(vm.IsRefreshing);
                });
            reset.WaitOne();

            Assert.IsTrue(vm.Root.ChildrenCount == 0);
        }

        #endregion

        #region Activity Drop From ToolBox Tests

        [TestMethod]
        public void CreateNavigationViewModelWithIsActivityDropTrueAndTypeWorkflowExpectedOnlyWorkflowsToBeInTree()
        {
            Init(false, true);
            vm = CreateViewModel(true, enDsfActivityType.Workflow);
            vm.AddEnvironment(mockEnvironmentModel.Object, new Mock<IDesignValidationService>().Object);
            Assert.AreEqual(1, vm.Root.Children[0].Children.Count);
            Assert.AreEqual("WORKFLOWS", vm.Root.Children[0].Children[0].DisplayName);
        }

        [TestMethod]
        public void CreateNavigationViewModelWithIsActivityDropTrueAndTypeServiceExpectedOnlyServicesToBeInTree()
        {
            Init(false, true);
            vm = CreateViewModel(true, enDsfActivityType.Service);
            vm.AddEnvironment(mockEnvironmentModel.Object, new Mock<IDesignValidationService>().Object);
            Assert.AreEqual(1, vm.Root.Children[0].Children.Count);
            Assert.AreEqual("SERVICES", vm.Root.Children[0].Children[0].DisplayName);
        }

        [TestMethod]
        public void CreateNavigationViewModelWithIsActivityDropTrueAndTypeSourceExpectedOnlySourcesToBeInTree()
        {
            Init(false, true);
            vm = CreateViewModel(true, enDsfActivityType.Source);
            vm.AddEnvironment(mockEnvironmentModel.Object, new Mock<IDesignValidationService>().Object);
            Assert.AreEqual(1, vm.Root.Children[0].Children.Count);
            Assert.AreEqual("SOURCES", vm.Root.Children[0].Children[0].DisplayName);
        }

        [TestMethod]
        public void CreateNavigationViewModelWithIsActivityDropTrueAndTypeAllExpectedAllItemsToBeInTree()
        {
            Init(false, true);
            vm = CreateViewModel(true, enDsfActivityType.All);
            vm.AddEnvironment(mockEnvironmentModel.Object, new Mock<IDesignValidationService>().Object);
            Assert.AreEqual(3, vm.Root.Children[0].Children.Count);
            Assert.AreEqual("WORKFLOWS", vm.Root.Children[0].Children[0].DisplayName);
            Assert.AreEqual("SERVICES", vm.Root.Children[0].Children[1].DisplayName);
            Assert.AreEqual("SOURCES", vm.Root.Children[0].Children[2].DisplayName);
        }

        [TestMethod]
        public void CreateNavigationViewModelWithIsActivityDropTrueAndTypeWorkflowExpectedWorkflowsWithNoChilrenToBeInTree()
        {
            Init(true, true);
            vm = CreateViewModel(true, enDsfActivityType.Workflow);
            vm.AddEnvironment(mockEnvironmentModel.Object, new Mock<IDesignValidationService>().Object);
            Assert.AreEqual(1, vm.Root.Children[0].Children.Count);
            Assert.AreEqual(0, vm.Root.Children[0].Children[0].Children[1].Children[0].Children.Count);
        }

        //2013.05.30: Ashley Lewis for bugs 9444+9445 - Dont show services from disconnected environments
        [TestMethod]
        public void CreateNavigationViewModelWithIsActivityDropTrueExpectedBothEnvironmentsVisibleOnlyResourcesForConnectedEnvironmentsToBeInTree()
        {
            InitTwoEnvironments(false, true);
            vm = CreateViewModel(true);
            vm.AddEnvironment(mockEnvironmentModel.Object, new Mock<IDesignValidationService>().Object);
            vm.AddEnvironment(disconnectedMockEnvironmentModel.Object, new Mock<IDesignValidationService>().Object);
            Assert.AreEqual(2, vm.Root.Children.Count, "Disconnected environment not added to NavigationViewModel");
            Assert.AreEqual(3, vm.Root.Children[0].Children.Count, "Resources for connected environment not visible in NavigationViewModel");
            Assert.AreEqual(0, vm.Root.Children[1].Children.Count, "Resources for disconnected environment are visible in NavigationViewModel");
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

            var nvm = new NavigationViewModel(eventPublisher.Object, null, null, null);
        }

        [TestMethod]
        [TestCategory("NavigationViewModel_LoadEnvironmentResources")]
        [Description("LoadEnvironmentResources must load resources asynchronously.")]
        [Owner("Trevor Williams-Ros")]
        public void NavigationViewModel_UnitTest_LoadEnvironmentResources_InvokesAsyncWorker()
        {
            //            ImportService.CurrentContext = new ImportServiceContext();
            //            ImportService.Initialize(new List<ComposablePartCatalog>
            //            {
            //                new FullTestAggregateCatalog()
            //            });
            //            ImportService.AddExportedValueToContainer(new Mock<IWizardEngine>().Object);

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
            var resourceVm = vm.Root.FindChild(mockResourceModel.Object);
            vm.BringItemIntoView(mockResourceModel.Object);
            Assert.IsTrue(resourceVm.IsSelected);
            Assert.IsTrue(resourceVm.TreeParent.IsExpanded);
        }


        [TestMethod]
        public void NavigationViewModel_BringItemIntoViewWithNull_Expects_DoesNothing()
        {
            Init(false, true);
            var resourceVm = vm.Root.FindChild(mockResourceModel.Object);
            vm.BringItemIntoView(null);
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

            vm = CreateViewModel();
            vm.AddEnvironment(mockEnvironmentModel.Object, new Mock<IDesignValidationService>().Object);
        }

        void SetupMockEnvironment(bool shouldLoadResources)
        {
            mockEnvironmentModel = GetMockEnvironment();
            mockEnvironmentModel.SetupGet(x => x.CanStudioExecute).Returns(shouldLoadResources);
        }

        public static Mock<IEnvironmentModel> GetMockEnvironment()
        {
            var eventPublisher = new Mock<IEventPublisher>();

            var designValidationEvents = new Mock<IObservable<DesignValidationMemo>>();
            eventPublisher.Setup(p => p.GetEvent<DesignValidationMemo>()).Returns(designValidationEvents.Object);

            var mock = new Mock<IEnvironmentModel>();
            // http://127.0.0.1/
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

            vm = CreateViewModel();
            vm.AddEnvironment(mockEnvironmentModel.Object);
            vm.AddEnvironment(disconnectedMockEnvironmentModel.Object);
        }

        void SetupTwoMockEnvironments(bool shouldLoadResources)
        {
            mockEnvironmentModel = GetMockEnvironment();
            mockEnvironmentModel.SetupGet(x => x.CanStudioExecute).Returns(shouldLoadResources);
            disconnectedMockEnvironmentModel = GetSecondMockEnvironment();
            disconnectedMockEnvironmentModel.SetupGet(x => x.CanStudioExecute).Returns(shouldLoadResources);
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

            mockResourceModel = new Mock<IContextualResourceModel>();
            mockResourceModel.Setup(r => r.ResourceType).Returns(ResourceType.WorkflowService);
            mockResourceModel.Setup(r => r.Category).Returns("Testing");
            mockResourceModel.Setup(r => r.ResourceName).Returns("Mock");
            mockResourceModel.Setup(r => r.Environment).Returns(mockEnvironmentModel.Object);

            mockResourceModel1 = new Mock<IContextualResourceModel>();
            mockResourceModel1.Setup(r => r.ResourceType).Returns(ResourceType.WorkflowService);
            mockResourceModel1.Setup(r => r.Category).Returns("Testing2");
            mockResourceModel1.Setup(r => r.ResourceName).Returns("Mock1");
            mockResourceModel1.Setup(r => r.Environment).Returns(mockEnvironmentModel.Object);
            if(addWizardChildToResource)
            {
                mockResourceModel11 = new Mock<IContextualResourceModel>();
                mockResourceModel11.Setup(r => r.ResourceType).Returns(ResourceType.WorkflowService);
                mockResourceModel11.Setup(r => r.Category).Returns("Testing");
                mockResourceModel11.Setup(r => r.ResourceName).Returns("Mock1.wiz");
                mockResourceModel11.Setup(r => r.Environment).Returns(mockEnvironmentModel.Object);
            }

            mockResourceModel2 = new Mock<IContextualResourceModel>();
            mockResourceModel2.Setup(r => r.ResourceType).Returns(ResourceType.Service);
            mockResourceModel2.Setup(r => r.Category).Returns("Testing2");
            mockResourceModel2.Setup(r => r.ResourceName).Returns("Mock2");
            mockResourceModel2.Setup(r => r.Environment).Returns(mockEnvironmentModel.Object);

            Collection<IResourceModel> mockResources = new Collection<IResourceModel>
            {
                mockResourceModel.Object,
                mockResourceModel1.Object,
                mockResourceModel2.Object
            };
            if(addWizardChildToResource)
            {
                mockResources.Add(mockResourceModel11.Object);
            }


            mockResourceRepository = new Mock<IResourceRepository>();
            mockResourceRepository.Setup(r => r.All()).Returns(mockResources);

            mockEnvironmentModel.SetupGet(x => x.ResourceRepository).Returns(mockResourceRepository.Object);
            mockEnvironmentModel.Setup(x => x.LoadResources());

            //disconnectedMockEnvironmentModel.SetupGet(x => x.ResourceRepository).Returns(mockResourceRepository.Object);
            //disconnectedMockEnvironmentModel.Setup(x => x.LoadResources());
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


        //[TestMethod]
        //[Owner("Trevor Williams-Ros")]
        //[TestCategory("NavigationViewModel_UpdateResource")]
        //[Ignore]
        //public void NavigationViewModel_UpdateResource_ServiceTypeFound_TreeViewModelItemWithActivityFullName()
        //{
        //    //------------Setup for test--------------------------

        //    var eventPublisher = new Mock<IEventAggregator>();
        //    var environmentRepository = new Mock<IEnvironmentRepository>();
        //    var asyncWorker = AsyncWorkerTests.CreateSynchronousAsyncWorker();

        //    var nvm = new NavigationViewModel(eventPublisher.Object, asyncWorker.Object, null, environmentRepository.Object);

        //    var env = new Mock<IEnvironmentModel>();
        //    env.Setup(e => e.Connection.Alias).Returns("Expected Environment");
        //    env.Setup(e => e.Connection.AppServerUri).Returns(new Uri("http://10.0.0.1"));
        //    env.Setup(e => e.Connection.ServerEvents).Returns(new Mock<IEventPublisher>().Object);

        //    var newResource = new Mock<IContextualResourceModel>();
        //    newResource.Setup(res => res.Category).Returns("Expected Category");
        //    newResource.Setup(res => res.ResourceName).Returns("Expected Resource Name");
        //    newResource.Setup(res => res.Environment).Returns(env.Object);

        //    //nvm.Root.Children.Add(new CategoryTreeViewModel("Workflows", ResourceType.WorkflowService, nvm.Root));

        //    new EnvironmentTreeViewModel(eventPublisher.Object, vm.Root, env.Object, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object);

        //    //------------Execute Test---------------------------
        //    nvm.UpdateResource(newResource.Object);

        //    //------------Assert Results-------------------------
        //    var workflowsNode = nvm.Root.Children[0];
        //    var categoryNode = workflowsNode.Children[0];
        //    var resourceNode = (ResourceTreeViewModel)categoryNode.Children[0];
        //    Assert.IsNotNull(resourceNode.ActivityFullName);
        //}
    }
}