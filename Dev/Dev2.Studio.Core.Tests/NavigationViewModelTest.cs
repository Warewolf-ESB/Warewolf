#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Dev2.Composition;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.AppResources.Repositories;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.ViewModels.Navigation;
using Dev2.Studio.Factory;
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

        Mock<IEnvironmentModel> mockEnvironmentModel;
        Mock<IContextualResourceModel> mockResourceModel;
        Mock<IContextualResourceModel> mockResourceModel1;
        Mock<IContextualResourceModel> mockResourceModel2;
        NavigationViewModel vm;

        #endregion Test Variables

        /// <summary>
        ///     Gets or sets the result context which provides
        ///     information about and functionality for the current result run.
        /// </summary>
        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void MyTestInitialize()
        {

            mockEnvironmentModel = new Mock<IEnvironmentModel>();
            mockEnvironmentModel.SetupGet(x => x.DsfAddress).Returns(new Uri("http://127.0.0.1/"));
            mockEnvironmentModel.SetupGet(x => x.IsConnected).Returns(true);

            // setup env repo
            var repo = new Mock<IFrameworkRepository<IEnvironmentModel>>();
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
            mockResourceModel1.Setup(r => r.ResourceName).Returns("Mock2");
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

            mockEnvironmentModel.SetupGet(x => x.Resources).Returns(mockResourceRepository.Object);
            mockEnvironmentModel.Setup(x => x.LoadResources());

            vm = new NavigationViewModel(false);
            vm.AddEnvironment(mockEnvironmentModel.Object);
        }

        #region Updating Resources

        [TestMethod]
        public void UpdateResourceMessage_WhenResourceExistsOldCategory_Expects_ResourceChanged()
        {
            var oldResoure = vm.Root.FindChild(mockResourceModel.Object);

            Assert.AreEqual("Mock", oldResoure.DisplayName);

            mockResourceModel.Setup(r => r.ResourceName).Returns("MockChangedName");
            var updatemsg = new UpdateResourceMessage(mockResourceModel.Object);

            vm.Handle(updatemsg);
            var updatedResource = vm.Root.FindChild(mockResourceModel.Object);
            Assert.AreEqual("MockChangedName", updatedResource.DisplayName);

        }

        [TestMethod]
        public void UpdateResourceMessage_WhenResourceUnchanged_Expects_UnchangedResource()
        {
            var oldResoure = vm.Root.FindChild(mockResourceModel.Object);

            Assert.AreEqual("Mock", oldResoure.DisplayName);

            var updatemsg = new UpdateResourceMessage(mockResourceModel.Object);

            vm.Handle(updatemsg);
            var updatedResource = vm.Root.FindChild(mockResourceModel.Object);
            Assert.AreEqual("Mock", updatedResource.DisplayName);
        }

        [TestMethod]
        public void UpdateResourceMessage_WhenResourceExistsChangedCategory_Expects_CategoryChanged()
        {
            var serviceTypeVM = vm.Root.FindChild(ResourceType.WorkflowService);
            var oldCategory = serviceTypeVM.FindChild("Testing");
            var resourceVM = oldCategory.FindChild(mockResourceModel.Object);

            Assert.IsTrue(ReferenceEquals(oldCategory, (ITreeNode)resourceVM.TreeParent));
            Assert.IsTrue(String.Compare(oldCategory.DisplayName, "Testing", 
                StringComparison.InvariantCultureIgnoreCase) == 0);

            mockResourceModel.Setup(r => r.Category).Returns("Testing2");

            var updatemsg = new UpdateResourceMessage(mockResourceModel.Object);

            vm.Handle(updatemsg);

            var updatedCategory = serviceTypeVM.FindChild("Testing2");

            Assert.IsTrue(ReferenceEquals(updatedCategory, (ITreeNode)resourceVM.TreeParent));

            Assert.IsTrue(String.Compare(updatedCategory.DisplayName, "Testing2",
                StringComparison.InvariantCultureIgnoreCase) == 0);

            Assert.AreEqual(updatedCategory.ChildrenCount, 2);
        }

        [TestMethod]
        public void UpdateResourceMessage_WhenNewCategory_Expects_CategoryAdded()
        {
            var serviceTypeVM = vm.Root.FindChild(ResourceType.WorkflowService);
            var oldCategory = serviceTypeVM.FindChild("Testing");
            var resourceVM = oldCategory.FindChild(mockResourceModel.Object);

            mockResourceModel.Setup(r => r.Category).Returns("Testing5");

            var updatemsg = new UpdateResourceMessage(mockResourceModel.Object);

            vm.Handle(updatemsg);

            var updatedCategory = serviceTypeVM.FindChild("Testing5");

            Assert.IsTrue(ReferenceEquals(updatedCategory, (ITreeNode)resourceVM.TreeParent));

            Assert.IsTrue(String.Compare(updatedCategory.DisplayName, "Testing5",
                StringComparison.InvariantCultureIgnoreCase) == 0);

            Assert.AreEqual(updatedCategory.ChildrenCount, 1);
        }

        [TestMethod]
        public void UpdateResourceMessage_WhenOldCategoryBecomesEmpty_Expects_CategoryDeleted()
        {
            var serviceTypeVM = vm.Root.FindChild(ResourceType.WorkflowService);

            mockResourceModel.Setup(r => r.Category).Returns("Testing5");

            var updatemsg = new UpdateResourceMessage(mockResourceModel.Object);

            vm.Handle(updatemsg);

            var updatedCategory = serviceTypeVM.FindChild("Testing");

            Assert.IsNull(updatedCategory);
            Assert.IsTrue(vm.Root.ChildrenCount == 3);
        }

        [TestMethod]
        public void UpdateResourceMessage_WhenOldCategoryNotEmpty_Expects_CategoryNotDeleted()
        {
            var serviceTypeVM = vm.Root.FindChild(ResourceType.WorkflowService);

            mockResourceModel.Setup(r => r.Category).Returns("Testing2");

            var updatemsg = new UpdateResourceMessage(mockResourceModel.Object);
            vm.Handle(updatemsg);

            mockResourceModel.Setup(r => r.Category).Returns("Testing3");

            updatemsg = new UpdateResourceMessage(mockResourceModel.Object);
            vm.Handle(updatemsg);

            var updatedCategory = serviceTypeVM.FindChild("Testing2");

            Assert.IsTrue(updatedCategory.ChildrenCount == 1);
        }

        [TestMethod]
        public void UpdateResourceMessage_WhenNewResource_Expects_ResourceAdded()
        {
            var newResource = new Mock<IContextualResourceModel>();
            newResource.Setup(r => r.ResourceType).Returns(ResourceType.WorkflowService);
            newResource.Setup(r => r.Category).Returns("Testing");
            newResource.Setup(r => r.ResourceName).Returns("Cake");
            newResource.Setup(r => r.Environment).Returns(mockEnvironmentModel.Object);

            var updatemsg = new UpdateResourceMessage(newResource.Object);
            vm.Handle(updatemsg);

            var serviceTypeVM = vm.Root.FindChild(ResourceType.WorkflowService);
            var categoryVM = serviceTypeVM.FindChild("Testing");
            var resourceVM = vm.Root.FindChild(newResource.Object);

            Assert.IsTrue(resourceVM.DisplayName == "Cake");
            Assert.IsTrue(categoryVM.ChildrenCount == 2);
            Assert.IsTrue(vm.Root.ChildrenCount == 4);
        }

        [TestMethod]
        public void UpdateResourceMessage_WhenNewResourceAndNewCategory_Expects_CategoryAdded()
        {
            var newResource = new Mock<IContextualResourceModel>();
            newResource.Setup(r => r.ResourceType).Returns(ResourceType.WorkflowService);
            newResource.Setup(r => r.Category).Returns("Testing4");
            newResource.Setup(r => r.ResourceName).Returns("Cake");
            newResource.Setup(r => r.Environment).Returns(mockEnvironmentModel.Object);

            var updatemsg = new UpdateResourceMessage(newResource.Object);
            vm.Handle(updatemsg);

            var serviceTypeVM = vm.Root.FindChild(ResourceType.WorkflowService);
            var categoryVM = serviceTypeVM.FindChild("Testing4");

            Assert.IsTrue(String.Compare(categoryVM.DisplayName, "Testing4",
                StringComparison.InvariantCultureIgnoreCase) == 0);

            Assert.IsTrue(categoryVM.ChildrenCount == 1);
            Assert.IsTrue(vm.Root.ChildrenCount == 4);
        }

        [TestMethod]
        public void AddEnvironment_Expected_RootAndChildrenCreated()
        {
            var workflowServiceTypeVM = vm.Root.FindChild(ResourceType.WorkflowService);
            var serviceTypeVM = vm.Root.FindChild(ResourceType.Service);
            var categoryVM = workflowServiceTypeVM.FindChild("Testing");
            var resourceVM = categoryVM.FindChild(mockResourceModel.Object);

            Assert.IsTrue(ReferenceEquals(categoryVM, (ITreeNode)resourceVM.TreeParent));
            Assert.IsTrue(String.Compare(categoryVM.DisplayName, "Testing",
                StringComparison.InvariantCultureIgnoreCase) == 0);
            Assert.IsTrue(workflowServiceTypeVM.ChildrenCount == 2);
            Assert.IsTrue(serviceTypeVM.ChildrenCount == 1);

        }

        #endregion Updating Resources

        #region Filtering

        [TestMethod]
        public void FilteredNavigationViewModel_AfterReload_Expects_FilteredItemsInTreeWithIsFilteredTrue()
        {
            var resourceVM = vm.Root.FindChild(mockResourceModel.Object);

            Assert.IsTrue(vm.Root.ChildrenCount == 3);
            Assert.IsTrue(resourceVM.IsFiltered == false);

            vm.SearchFilter = "Mock2";
            Assert.IsTrue(vm.Root.ChildrenCount == 2);
            Assert.IsTrue(resourceVM.IsFiltered == true);

            vm.LoadEnvironmentResources(mockEnvironmentModel.Object);
            resourceVM = vm.Root.FindChild(mockResourceModel.Object);
            Assert.IsTrue(vm.Root.ChildrenCount == 2);
            Assert.IsTrue(resourceVM.IsFiltered == true);


            vm.SearchFilter = "Mock";
            Assert.IsTrue(vm.Root.ChildrenCount == 3);
            Assert.IsTrue(resourceVM.IsFiltered == false);
        }

        #endregion Filtering

        #region disconnect

        [TestMethod]
        public void EnvironmentNodeDisconnect_Expect_NodeRemovedFromRoot()
        {
            mockEnvironmentModel.SetupGet(c => c.IsConnected).Returns(true);
            mockEnvironmentModel.SetupGet(c => c.Name).Returns("Mock");

            Assert.IsTrue(vm.Root.ChildrenCount != 0);

            var message = new EnvironmentDisconnectedMessage(mockEnvironmentModel.Object);
            vm.Handle(message);

            Assert.IsTrue(vm.Root.ChildrenCount == 0);
        }
        #endregion
    }
}