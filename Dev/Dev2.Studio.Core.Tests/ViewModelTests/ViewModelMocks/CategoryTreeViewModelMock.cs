using Caliburn.Micro;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.ViewModels.Navigation;
using Dev2.Studio.ViewModels.Navigation;

namespace Dev2.Core.Tests.ViewModelTests.ViewModelMocks
{
    public class CategoryTreeViewModelMock : CategoryTreeViewModel
    {

        public CategoryTreeViewModelMock(IEventAggregator eventPublisher, ITreeNode parent, string name, ResourceType resourceType)
            : base(eventPublisher, parent, name, resourceType)
        {
        }

        public int RenameCategoryHitCount;

        protected override void RenameCategory(string newCategory)
        {
            RenameCategoryHitCount++;
        }

        public int ReparentHitCount;

        protected override void Reparent(string parentName)
        {
            ReparentHitCount++;
        }
    }
}
