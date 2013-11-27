using Caliburn.Micro;
using Dev2.Studio.Core.AppResources.Browsers;
using Dev2.Studio.Core.Helpers;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.ViewModels;
using Dev2.Threading;

namespace Dev2.Core.Tests
{
    public class MainViewModelMock : MainViewModel
    {
        public MainViewModelMock(IEventAggregator eventPublisher, IAsyncWorker asyncWorker, IEnvironmentRepository environmentRepository, IVersionChecker versionChecker, bool createDesigners = true, IBrowserPopupController browserPopupController = null)
            : base(eventPublisher, asyncWorker, environmentRepository, versionChecker, createDesigners, browserPopupController)
        {
        }

        public int AddWorkspaceItemsHitCount { get; private set; }
        protected override void AddWorkspaceItems()
        {
            AddWorkspaceItemsHitCount++;
            base.AddWorkspaceItems();
        }
    }
}
