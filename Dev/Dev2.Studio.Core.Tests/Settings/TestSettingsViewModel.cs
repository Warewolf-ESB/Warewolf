
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Windows.Forms;
using Caliburn.Micro;
using CubicOrange.Windows.Forms.ActiveDirectory;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.CustomControls.Connections;
using Dev2.Dialogs;
using Dev2.Services.Security;
using Dev2.Settings;
using Dev2.Settings.Logging;
using Dev2.Settings.Security;
using Dev2.Studio.Core.Interfaces;
using Dev2.Threading;
using Moq;

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
        public LogSettingsViewModel TheLogSettingsViewModel { get; set; }
        protected override SecurityViewModel CreateSecurityViewModel()
        {
            return TheSecurityViewModel ?? new SecurityViewModel(Settings.Security, new Mock<IResourcePickerDialog>().Object, new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IEnvironmentModel>().Object);
        }

        protected override LogSettingsViewModel CreateLoggingViewModel()
        {
            return TheLogSettingsViewModel ?? new LogSettingsViewModel(new LoggingSettingsTo(), new Mock<IEnvironmentModel>().Object);
        }

        public void CallDeactivate()
        {
            DoDeactivate();
        }
    }
}
