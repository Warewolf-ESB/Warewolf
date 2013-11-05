using Caliburn.Micro;
using Dev2.Settings;
using Dev2.Studio.Core.Controller;
using Dev2.Threading;

namespace Dev2.Core.Tests.Settings
{
    public class TestSettingsViewModel : SettingsViewModel
    {
        public TestSettingsViewModel()
        {
        }

        public TestSettingsViewModel(IEventAggregator eventPublisher, IPopupController popupController, IAsyncWorker asyncWorker)
            : base(eventPublisher, popupController, asyncWorker)
        {
        }

        public int ShowErrorHitCount { get; private set; }
        protected override void ShowError(string header, string description)
        {
            ShowErrorHitCount++;
            base.ShowError(header, description);
        }
    }
}
