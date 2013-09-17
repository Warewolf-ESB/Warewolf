using Dev2.Communication;
using Dev2.Services;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.ViewModels.Navigation;

namespace Dev2.Studio.ViewModels.Navigation
{
    public class MockResourceTreeViewModel : ResourceTreeViewModel
    {
        public MockResourceTreeViewModel(IDesignValidationService validationService, ITreeNode parent, IContextualResourceModel dataContext, string activityFullName = null)
            : base(validationService, parent, dataContext, activityFullName)
        {
        }

        public void TestDesignValidationReceived(DesignValidationMemo memo)
        {
            base.OnDesignValidationReceived(memo);
        }

        public void TestRename(string newValue)
        {
            HandleRename(newValue, null);
        }
    }
}