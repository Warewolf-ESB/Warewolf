/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Common.Interfaces.Threading;
using Dev2.Dialogs;
using Dev2.Services.Security;
using Dev2.Settings;
using Dev2.Settings.Logging;
using Dev2.Settings.Perfcounters;
using Dev2.Settings.Security;
using Dev2.Studio.Core.Interfaces;
using Moq;

namespace Dev2.Core.Tests.Settings
{
    public class TestSettingsViewModel : SettingsViewModel
    {
        private SecurityViewModel _theSecurityViewModel;
        private PerfcounterViewModel _thePerfcounterViewModel;

        public TestSettingsViewModel()
        {
        }

        public TestSettingsViewModel(IEventAggregator eventPublisher, IPopupController popupController, IAsyncWorker asyncWorker, IWin32Window parentWindow,Mock<IEnvironmentModel> env)
            : base(eventPublisher, popupController, asyncWorker, parentWindow, new Mock<Dev2.Common.Interfaces.IServer>().Object, a => env.Object)
        {
            
        }

        public int ShowErrorHitCount { get; private set; }
        protected override void ShowError(string header, string description)
        {
            ShowErrorHitCount++;
            base.ShowError(header, description);
        }

        public override bool HasVariables => false;
        public override bool HasDebugOutput => true;

        public SecurityViewModel TheSecurityViewModel
        {
            get
            {
                return _theSecurityViewModel;
            }
            set
            {
                _theSecurityViewModel = value;
            }
        }
        public LogSettingsViewModel TheLogSettingsViewModel { get; set; }
        protected override SecurityViewModel CreateSecurityViewModel()
        {
            return TheSecurityViewModel ?? new SecurityViewModel(Settings.Security, new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IEnvironmentModel>().Object, ()=> new Mock<IResourcePickerDialog>().Object);
        }

        protected override PerfcounterViewModel CreatePerfmonViewModel()
        {
            return ThePerfcounterViewModel ?? new PerfcounterViewModel(Settings.PerfCounters, new Mock<IEnvironmentModel>().Object, () => new Mock<IResourcePickerDialog>().Object);
        }

        public PerfcounterViewModel ThePerfcounterViewModel
        {
            get
            {
                return _thePerfcounterViewModel;
            }
            set
            {
                _thePerfcounterViewModel = value;
            }
        }

        protected override LogSettingsViewModel CreateLoggingViewModel()
        {
            return TheLogSettingsViewModel ?? new LogSettingsViewModel(new LoggingSettingsTo(), new Mock<IEnvironmentModel>().Object);
        }

        public void CallDeactivate()
        {
            DoDeactivate(true);
        }
    }
}
