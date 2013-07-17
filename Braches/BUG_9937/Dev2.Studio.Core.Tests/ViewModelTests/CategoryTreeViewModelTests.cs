using System.Collections.ObjectModel;
using Dev2.Composition;
using Dev2.Core.Tests.ViewModelTests.ViewModelMocks;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.ViewModels.Navigation;
using Dev2.Studio.ViewModels.Navigation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;

namespace Dev2.Core.Tests.ViewModelTests
{
    [TestClass]
    public class CategoryTreeViewModelTests
    {
        [TestMethod]
        [TestCategory("CategoryTreeViewModelUnitTest")]
        [Description("Test for CategoryTreeViewModel DisplayName: Displayname property is set to a valid resource category name ('new_category.var') and RenameCategory is expected to be called")]
        [Owner("Ashley Lewis")]
    // ReSharper disable InconsistentNaming
        public void CategoryTreeView_CategoryTreeViewModelUnitTest_DisplayNameBinding_RenameCategoryIsCalledOnce()
    // ReSharper restore InconsistentNaming
        {
            //MEF!!!
            ImportService.CurrentContext = CompositionInitializer.InitializeForMeflessBaseViewModel();

            // ReSharper disable ObjectCreationAsStatement
            var vm = new MockCategoryTreeViewModelRenameCategory("Test Category", ResourceType.WorkflowService, null) { DisplayName = "Renamed Test Category" };
            // ReSharper restore ObjectCreationAsStatement

            Assert.AreEqual(1, vm.RenameCategoryHitCount, "Rename category not called after display name change");
        }

        [TestMethod]
        [TestCategory("CategoryTreeViewModelUnitTest")]
        [Description("Test for CategoryTreeViewModel's RenameCategory function: ResourceRepository's RenameCategory function is expected to be called")]
        [Owner("Ashley Lewis")]
        // ReSharper disable InconsistentNaming
        public void CategoryTreeView_CategoryTreeViewModelUnitTest_RenameCategory_ResourceRepoRenameCategoryCalledAndReparentCalled()
        // ReSharper restore InconsistentNaming
        {
            //MEF!!!
            ImportService.CurrentContext = CompositionInitializer.InitializeForMeflessBaseViewModel();

            //Initialization
            var mockResourceRepo = new Mock<IResourceRepository>();
            mockResourceRepo.Setup(c=>c.RenameCategory(It.IsAny<string>(), It.IsAny<string>(), ResourceType.WorkflowService)).Verifiable();
            var parent = new Mock<ServiceTypeTreeViewModel>(ResourceType.WorkflowService, null);
            parent.Setup(model => model.EnvironmentModel.ResourceRepository).Returns(mockResourceRepo.Object);
            parent.Object.Children = new ObservableCollection<ITreeNode>();
            

            //Execute
            // ReSharper disable ObjectCreationAsStatement
            var vm = new MockCategoryTreeViewModelReparent("Test Category", ResourceType.WorkflowService, parent.Object) { TreeParent = parent.Object, DisplayName = "Renamed Test Category" };
            // ReSharper restore ObjectCreationAsStatement

            //Assert
            mockResourceRepo.Verify(c => c.RenameCategory(It.IsAny<string>(), It.IsAny<string>(), ResourceType.WorkflowService), Times.Once(), "ResourceReposities RenameCategory function was not called after category tree view model rename resource was called");
            Assert.AreEqual(1, vm.ReparentHitCount, "Reparent not called during rename category");
        }
    }
}
