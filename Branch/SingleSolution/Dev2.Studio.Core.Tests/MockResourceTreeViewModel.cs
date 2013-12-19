using Caliburn.Micro;
using Dev2.Communication;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.ViewModels.Navigation;

namespace Dev2.Studio.ViewModels.Navigation
{
    public class MockResourceTreeViewModel : ResourceTreeViewModel
    {
        public MockResourceTreeViewModel(IEventAggregator eventPublisher, ITreeNode parent, IContextualResourceModel dataContext)
            : base(eventPublisher, parent, dataContext)
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

        public int OnDesignValidationReceivedHitCount { get; set; }
        protected override void OnDesignValidationReceived(DesignValidationMemo memo)
        {
            OnDesignValidationReceivedHitCount++;
            base.OnDesignValidationReceived(memo);
        }
    }
}