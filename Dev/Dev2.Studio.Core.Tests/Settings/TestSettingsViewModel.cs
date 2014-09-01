using Caliburn.Micro;
using CubicOrange.Windows.Forms.ActiveDirectory;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.CustomControls.Connections;
using Dev2.Dialogs;
using Dev2.Settings;
using Dev2.Settings.Security;
using Dev2.Studio.Core.Interfaces;
using Dev2.Threading;
using Moq;
using System.Windows.Forms;

namespace Dev2.Core.Tests.Settings
{
    public class TestSettingsViewModel : SettingsViewModel
    {
        public TestSettingsViewModel()
        {
        }

        public TestSettingsViewModel(IEventAggregator eventPublisher, IPopupController popupController, IAsyncWorker asyncWorker, IWin32Window parentWindow)
            : base(eventPublisher, popupController, asyncWorker, parentWindow, new Mock<IConnectControlViewModel>().Object)
        {
        }

        public int ShowErrorHitCount { get; private set; }
        protected override void ShowError(string header, string description)
        {
            ShowErrorHitCount++;
            base.ShowError(header, description);
        }

        public SecurityViewModel TheSecurityViewModel { get; set; }
        protected override SecurityViewModel CreateSecurityViewModel()
        {
            return TheSecurityViewModel ?? new SecurityViewModel(Settings.Security, new Mock<IResourcePickerDialog>().Object, new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IEnvironmentModel>().Object);
        }

        public void CallDeactivate()
        {
            DoDeactivate();
        }
    }
}
