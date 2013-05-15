#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Dev2.Composition;
using Dev2.Studio;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.ViewModels.Navigation;
using Dev2.Studio.Enums;
using Dev2.Studio.ViewModels.Navigation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

#endregion

namespace Dev2.Core.Tests
{
    /// <summary>
    ///     Summary description for NavigationViewModelTest
    /// </summary>
    [TestClass]
    public class NavigationViewModelTest
    {
        #region Test Variables

        private object _lock = new object();
        private Mock<IEnvironmentModel> mockEnvironmentModel;
        private Mock<IContextualResourceModel> mockResourceModel;
        private Mock<IContextualResourceModel> mockResourceModel1;
        private Mock<IContextualResourceModel> mockResourceModel2;
        private NavigationViewModel vm;

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
        public void MyTestInitialize()
        {
            Monitor.Enter(_lock);
            
        }

        [TestCleanup]
        public void TestCleanUp()
        {
            Monitor.Exit(_lock);
        }

        #endregion

        #region Updating Resources

        private void Init()
        {
            //if (Application.Current == null)
            //{
            //    App app = new App();
            //}

            mockEnvironmentModel = new Mock<IEnvironmentModel>();
            mockEnvironmentModel.SetupGet(x => x.Connection.AppServerUri).Returns(new Uri("http://127.0.0.1/"));
            mockEnvironmentModel.SetupGet(x => x.Connection.AppServerUri).Returns(new Uri("http://127.0.0.1/"));
            mockEnvironmentModel.SetupGet(x => x.IsConnected).Returns(true);

            // setup env repo
            var repo = new Mock<IEnvironmentRepository>();
            repo.Setup(l => l.Load()).Verifiable();

            IList<IEnvironmentModel> models = new List<IEnvironmentModel>();
            repo.Setup(l => l.All()).Returns(models);

            ImportService.CurrentContext = CompositionInitializer.InitializeNavigationViewModelTests(repo);

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

            vm = new NavigationViewModel(false);
            vm.AddEnvironment(mockEnvironmentModel.Object);
        }

        [TestMethod]
        public void UpdateResourceMessage_WhenResourceExistsOldCategory_Expects_ResourceChanged()
        {
            // Kick the test off on the dispatcher worker thread synchronously which will block until the work is competed

            var reset = new AutoResetEvent(false);
            UpdateResourceMessage updatemsg;
            ITreeNode updatedResource = null;
            ThreadExecuter.RunCodeAsSTA(reset,
                                        () =>
                                            {
                                                Init(false, true);
                                                ITreeNode oldResoure = vm.Root.FindChild(mockResourceModel.Object);

                                                //Assert.AreEqual("Mock", oldResoure.DisplayName);

                                                mockResourceModel.Setup(r => r.ResourceName).Returns("MockChangedName");
                                                updatemsg = new UpdateResourceMessage(mockResourceModel.Object);

                                                vm.Handle(updatemsg);
                                                updatedResource = vm.Root.FindChild(mockResourceModel.Object);
                                            }
                );

            reset.WaitOne();
            Assert.AreEqual("MockChangedName", updatedResource.DisplayName);
        }

        [TestMethod]
        public void UpdateResourceMessage_WhenResourceUnchanged_Expects_UnchangedResource()
        {
            ITreeNode updatedResource = null;

            var reset = new AutoResetEvent(false);
            ThreadExecuter.RunCodeAsSTA(reset,
                                        () =>
                                            {
                                                Init(false, true);
                                                ITreeNode oldResoure = vm.Root.FindChild(mockResourceModel.Object);

                                                Assert.AreEqual("Mock", oldResoure.DisplayName);

                                                var updatemsg = new UpdateResourceMessage(mockResourceModel.Object);

                                                vm.Handle(updatemsg);
                                                updatedResource = vm.Root.FindChild(mockResourceModel.Object);
                                            });
            reset.WaitOne();

            Assert.AreEqual("Mock", updatedResource.DisplayName);
        }

        [TestMethod]
        public void UpdateResourceMessage_WhenResourceExistsChangedCategory_Expects_CategoryChanged()
        {
            ITreeNode updatedCategory = null;
            ITreeNode resourceVM = null;

            var reset = new AutoResetEvent(false);
            ThreadExecuter.RunCodeAsSTA(reset,
                                        () =>
                                            {
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

                                            });
            reset.WaitOne();

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
            ITreeNode resourceVM = null;

            var reset = new AutoResetEvent(false);
            ThreadExecuter.RunCodeAsSTA(reset,
                                        () =>
                                            {
                                                Init(false, true);
                                                ITreeNode serviceTypeVM = vm.Root.FindChild(ResourceType.WorkflowService);
                                                ITreeNode oldCategory = serviceTypeVM.FindChild("Testing");
                                                resourceVM = oldCategory.FindChild(mockResourceModel.Object);

                                                mockResourceModel.Setup(r => r.Category).Returns("Testing5");

                                                var updatemsg = new UpdateResourceMessage(mockResourceModel.Object);

                                                vm.Handle(updatemsg);

                                                updatedCategory = serviceTypeVM.FindChild("Testing5");

                                            });
            reset.WaitOne();

            Assert.IsTrue(ReferenceEquals(updatedCategory, resourceVM.TreeParent));
            Assert.IsTrue(String.Compare(updatedCategory.DisplayName, "Testing5",
                                         StringComparison.InvariantCultureIgnoreCase) ==
                          0);
            Assert.AreEqual(updatedCategory.ChildrenCount, 1);
        }

        [TestMethod]
        public void UpdateResourceMessage_WhenOldCategoryBecomesEmpty_Expects_CategoryDeleted()
        {
            ITreeNode updatedCategory = null;
            var reset = new AutoResetEvent(false);
            ThreadExecuter.RunCodeAsSTA(reset,
                                        () =>
                                            {
                                                Init(false, true);
                                                ITreeNode serviceTypeVM = vm.Root.FindChild(ResourceType.WorkflowService);

                                                mockResourceModel.Setup(r => r.Category).Returns("Testing5");

                                                var updatemsg = new UpdateResourceMessage(mockResourceModel.Object);

                                                vm.Handle(updatemsg);

                                                updatedCategory = serviceTypeVM.FindChild("Testing");

                                            });
            reset.WaitOne();

            Assert.IsNull(updatedCategory);
            Assert.IsTrue(vm.Root.ChildrenCount == 3);
        }

        [TestMethod]
        public void UpdateResourceMessage_WhenOldCategoryNotEmpty_Expects_CategoryNotDeleted()
        {
            ITreeNode updatedCategory = null;
            var reset = new AutoResetEvent(false);
            ThreadExecuter.RunCodeAsSTA(reset,
                                        () =>
                                            {
                                                Init(false, true);
                                                ITreeNode serviceTypeVM = vm.Root.FindChild(ResourceType.WorkflowService);

                                                mockResourceModel.Setup(r => r.Category).Returns("Testing2");

                                                var updatemsg = new UpdateResourceMessage(mockResourceModel.Object);
                                                vm.Handle(updatemsg);

                                                mockResourceModel.Setup(r => r.Category).Returns("Testing3");

                                                updatemsg = new UpdateResourceMessage(mockResourceModel.Object);
                                                vm.Handle(updatemsg);

                                                updatedCategory = serviceTypeVM.FindChild("Testing2");

                                            });
            reset.WaitOne();

            Assert.IsTrue(updatedCategory.ChildrenCount == 1);
        }

        [TestMethod]
        public void UpdateResourceMessage_WhenNewResource_Expects_ResourceAdded()
        {
            ITreeNode categoryVM = null;
            ITreeNode resourceVM = null;
            var reset = new AutoResetEvent(false);
            ThreadExecuter.RunCodeAsSTA(reset,
                                        () =>
                                            {
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

                                            });
            reset.WaitOne();

            Assert.IsTrue(resourceVM.DisplayName == "Cake");
            Assert.IsTrue(categoryVM.ChildrenCount == 2);
            Assert.IsTrue(vm.Root.ChildrenCount == 4);
        }

        [TestMethod]
        public void UpdateResourceMessage_WhenNewResourceAndNewCategory_Expects_CategoryAdded()
        {
            ITreeNode categoryVM = null;
            var reset = new AutoResetEvent(false);
            ThreadExecuter.RunCodeAsSTA(reset,
                                        () =>
                                            {
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

                                            });
            reset.WaitOne();

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

            var reset = new AutoResetEvent(false);
            ThreadExecuter.RunCodeAsSTA(reset,
                                        () =>
                                            {
                                                Init(false, true);
                                                workflowServiceTypeVM =
                                                    vm.Root.FindChild(ResourceType.WorkflowService);
                                                serviceTypeVM = vm.Root.FindChild(ResourceType.Service);
                                                categoryVM = workflowServiceTypeVM.FindChild("Testing");
                                                resourceVM = categoryVM.FindChild(mockResourceModel.Object);
                                            });
            reset.WaitOne();

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
            var reset = new AutoResetEvent(false);
            ThreadExecuter.RunCodeAsSTA(reset,
                                        () =>
                                        {
                                            Init(false, false);
                                        });
            reset.WaitOne();

            Assert.AreEqual(0,vm.Root.Children.Count());
        }

        [TestMethod]
        public void AddEnvironmentWithShouldLoadResourcesTrueExpectedEnvironmentAdded()
        {
            var reset = new AutoResetEvent(false);
            ThreadExecuter.RunCodeAsSTA(reset,
                                        () =>
                                        {
                                            Init(false, true);
                                        });
            reset.WaitOne();

            Assert.AreEqual(1, vm.Root.Children.Count());
        }

        #endregion Updating Resources

        #region Filtering

        [TestMethod]
        public void FilteredNavigationViewModel_AfterReload_Expects_FilteredItemsInTreeWithIsFilteredTrue()
        {
            ITreeNode resourceVM = null;

            var reset = new AutoResetEvent(false);
            ThreadExecuter.RunCodeAsSTA(reset,
                                        () =>
                                            {
                                                Init(false, true);
                                                 resourceVM = vm.Root.FindChild(mockResourceModel.Object);

                                                Assert.IsTrue(vm.Root.ChildrenCount == 3);
                                                Assert.IsTrue(resourceVM.IsFiltered == false);

                                                vm.UpdateSearchFilter("Mock2");
                                                vm.Root.NotifyOfFilterPropertyChanged(false);

                                                Thread.Sleep(100);

                                                Assert.IsTrue(vm.Root.ChildrenCount == 1);
                                                Assert.IsTrue(resourceVM.IsFiltered);

                                                vm.LoadEnvironmentResources(mockEnvironmentModel.Object);
                                                vm.Root.NotifyOfFilterPropertyChanged(false);

                                                Thread.Sleep(100);

                                                resourceVM = vm.Root.FindChild(mockResourceModel.Object);

                                                Thread.Sleep(100);

                                                Assert.IsTrue(vm.Root.ChildrenCount == 1);
                                                Assert.IsTrue(resourceVM.IsFiltered);

                                                vm.UpdateSearchFilter("Mock");

                                                Thread.Sleep(100);

                                            });
            reset.WaitOne();

            Assert.IsTrue(vm.Root.ChildrenCount == 3);
            Assert.IsTrue(resourceVM.IsFiltered == false);
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

            Assert.IsTrue(nonMatchingNode1.IsFiltered);
            Assert.IsTrue(matchingNode.IsFiltered == false);
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

        #region Disconnect

        [TestMethod]
        public void EnvironmentNodeDisconnect_Expect_NodeRemovedFromRoot()
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
            vm = new NavigationViewModel(false, true, enDsfActivityType.Workflow);
            vm.AddEnvironment(mockEnvironmentModel.Object);
            Assert.AreEqual(1, vm.Root.Children[0].Children.Count);
            Assert.AreEqual("WORKFLOWS", vm.Root.Children[0].Children[0].DisplayName);
        }

        [TestMethod]
        public void CreateNavigationViewModelWithIsActivityDropTrueAndTypeServiceExpectedOnlyServicesToBeInTree()
        {
            Init(false, true);
            vm = new NavigationViewModel(false, true, enDsfActivityType.Service);
            vm.AddEnvironment(mockEnvironmentModel.Object);
            Assert.AreEqual(1, vm.Root.Children[0].Children.Count);
            Assert.AreEqual("SERVICES", vm.Root.Children[0].Children[0].DisplayName);
        }

        [TestMethod]
        public void CreateNavigationViewModelWithIsActivityDropTrueAndTypeSourceExpectedOnlySourcesToBeInTree()
        {
            Init(false, true);
            vm = new NavigationViewModel(false, true, enDsfActivityType.Source);
            vm.AddEnvironment(mockEnvironmentModel.Object);
            Assert.AreEqual(1, vm.Root.Children[0].Children.Count);
            Assert.AreEqual("SOURCES", vm.Root.Children[0].Children[0].DisplayName);
        }

        [TestMethod]
        public void CreateNavigationViewModelWithIsActivityDropTrueAndTypeAllExpectedAllItemsToBeInTree()
        {
            Init(false, true);
            vm = new NavigationViewModel(false, true, enDsfActivityType.All);
            vm.AddEnvironment(mockEnvironmentModel.Object);
            Assert.AreEqual(3, vm.Root.Children[0].Children.Count);
            Assert.AreEqual("WORKFLOWS", vm.Root.Children[0].Children[0].DisplayName);
            Assert.AreEqual("SERVICES", vm.Root.Children[0].Children[1].DisplayName);
            Assert.AreEqual("SOURCES", vm.Root.Children[0].Children[2].DisplayName);
        }

        [TestMethod]
        public void CreateNavigationViewModelWithIsActivityDropTrueAndTypeWorkflowExpectedWorkflowsWithNoChilrenToBeInTree()
        {
            Init(true, true);
            vm = new NavigationViewModel(false, true, enDsfActivityType.Workflow);
            vm.AddEnvironment(mockEnvironmentModel.Object);
            Assert.AreEqual(1, vm.Root.Children[0].Children.Count);
            Assert.AreEqual(0, vm.Root.Children[0].Children[0].Children[1].Children[0].Children.Count);
        }

        #endregion

        #region Private Test Methods

        private void Init(bool addWizardChildToResource, bool shouldLoadResources)
        {
            //if (Application.Current == null)
            //{
            //    App app = new App();
            //}

            SetupMockEnvironment(shouldLoadResources);

            Mock<IResourceRepository> mockResourceRepository;
            SetUpResources(addWizardChildToResource, out mockResourceRepository);

            vm = new NavigationViewModel(false);
            vm.AddEnvironment(mockEnvironmentModel.Object);
        }

        void SetupMockEnvironment(bool shouldLoadResources)
        {
            mockEnvironmentModel = new Mock<IEnvironmentModel>();
            mockEnvironmentModel.SetupGet(x => x.Connection.AppServerUri).Returns(new Uri("http://127.0.0.1/"));
            mockEnvironmentModel.SetupGet(x => x.Connection.AppServerUri).Returns(new Uri("http://127.0.0.1/"));
            mockEnvironmentModel.SetupGet(x => x.IsConnected).Returns(true);
            mockEnvironmentModel.SetupGet(x => x.ShouldLoadResources).Returns(shouldLoadResources);
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

            Collection<IResourceModel> mockResources = new Collection<IResourceModel>{
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
        }

        #endregion
    }
}