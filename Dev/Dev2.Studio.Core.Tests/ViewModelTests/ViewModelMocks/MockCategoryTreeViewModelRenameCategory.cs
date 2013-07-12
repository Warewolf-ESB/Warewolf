using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.ViewModels.Navigation;
using Dev2.Studio.ViewModels.Navigation;

namespace Dev2.Core.Tests.ViewModelTests.ViewModelMocks
{
    class MockCategoryTreeViewModelRenameCategory : CategoryTreeViewModel
    {
        public int RenameCategoryHitCount;

        public MockCategoryTreeViewModelRenameCategory(string name, ResourceType resourceType, ITreeNode parent)
            : base(name, resourceType, parent)
        {
            RenameCategoryHitCount = 0;
        }

        protected override void RenameCategory(string newCategory)
        {
            RenameCategoryHitCount++;
        }
    }
}
