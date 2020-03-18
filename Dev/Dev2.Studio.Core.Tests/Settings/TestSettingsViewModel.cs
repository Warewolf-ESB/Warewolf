/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.IO;
using System.Windows.Forms;
using Caliburn.Micro;
using CubicOrange.Windows.Forms.ActiveDirectory;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Common.Interfaces.Threading;
using Dev2.Dialogs;
using Dev2.Services.Security;
using Dev2.Settings;
using Dev2.Settings.Logging;
using Dev2.Settings.Perfcounters;
using Dev2.Settings.Security;
using Dev2.Studio.Interfaces;
using log4net.Config;
using Moq;
using Warewolf.Configuration;

namespace Dev2.Core.Tests.Settings
{
    public class TestSettingsViewModel : SettingsViewModel
    {
        SecurityViewModel _theSecurityViewModel;
        PerfcounterViewModel _thePerfcounterViewModel;

        public TestSettingsViewModel()
        {
        }

        public TestSettingsViewModel(IEventAggregator eventPublisher, IPopupController popupController, IAsyncWorker asyncWorker, IWin32Window parentWindow,Mock<IServer> env)
            : base(eventPublisher, popupController, asyncWorker, parentWindow, new Mock<IServer>().Object, a => env.Object)
        {
            
        }

        public int ShowErrorHitCount { get; private set; }
        protected override void ShowError(string header, string description)
        {
            ShowErrorHitCount++;
            base.ShowError(header, description);
        }

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
            return TheSecurityViewModel ?? new SecurityViewModel(Settings.Security, new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IServer>().Object, ()=> new Mock<IResourcePickerDialog>().Object);
        }

        protected override PerfcounterViewModel CreatePerfmonViewModel()
        {
            return ThePerfcounterViewModel ?? new PerfcounterViewModel(Settings.PerfCounters, new Mock<IServer>().Object, () => new Mock<IResourcePickerDialog>().Object);
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
            return TheLogSettingsViewModel ?? CreateLogSettingViewModel();
        }

        static LogSettingsViewModel CreateLogSettingViewModel()
        {
            XmlConfigurator.ConfigureAndWatch(new FileInfo("Settings.config"));
            var loggingSettingsTo = new LoggingSettingsTo { FileLoggerLogSize = 50, FileLoggerLogLevel = "TRACE" };

            var _resourceRepo = new Mock<IResourceRepository>();
            var env = new Mock<IServer>();
            var auditingSettingsData = new LegacySettingsData() { AuditFilePath = "somePath" };
            _resourceRepo.Setup(res => res.GetAuditingSettings<LegacySettingsData>(env.Object)).Returns(auditingSettingsData);
            env.Setup(a => a.ResourceRepository).Returns(_resourceRepo.Object);

            var logSettingsViewModel = new LogSettingsViewModel(loggingSettingsTo, env.Object);
            return logSettingsViewModel;
        }

        public void CallDeactivate()
        {
            DoDeactivate(true);
        }
    }
}
