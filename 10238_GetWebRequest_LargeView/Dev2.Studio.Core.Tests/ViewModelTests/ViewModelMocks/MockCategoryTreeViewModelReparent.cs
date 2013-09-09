using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.ViewModels.Navigation;
using Dev2.Studio.ViewModels.Navigation;

namespace Dev2.Core.Tests.ViewModelTests.ViewModelMocks
{
    class MockCategoryTreeViewModelReparent : CategoryTreeViewModel
    {
        public int ReparentHitCount;

        public MockCategoryTreeViewModelReparent(string name, ResourceType resourceType, ITreeNode parent)
            : base(name, resourceType, parent)
        {
            ReparentHitCount = 0;
        }

        protected override void Reparent(string parentName)
        {
            ReparentHitCount++;
        }
    }
}
