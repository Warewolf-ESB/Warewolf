/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Caliburn.Micro;
using CubicOrange.Windows.Forms.ActiveDirectory;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Resources;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Common.Interfaces.Threading;
using Dev2.Data.Interfaces.Enums;
using Dev2.Data.ServiceModel;
using Dev2.Dialogs;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Services.Security;
using Dev2.Settings;
using Dev2.Settings.Logging;
using Dev2.Settings.Perfcounters;
using Dev2.Settings.Persistence;
using Dev2.Settings.Security;
using Dev2.Studio.Interfaces;
using log4net.Config;
using Moq;
using Newtonsoft.Json;
using Warewolf.Configuration;
using Warewolf.Data;
using Warewolf.UnitTestAttributes;

namespace Dev2.Core.Tests.Settings
{
    public class TestSettingsViewModel : SettingsViewModel
    {
        SecurityViewModel _theSecurityViewModel;
        PerfcounterViewModel _thePerfcounterViewModel;

        public TestSettingsViewModel()
        {
        }

        public TestSettingsViewModel(IEventAggregator eventPublisher, IPopupController popupController, IAsyncWorker asyncWorker, IWin32Window parentWindow, Mock<IServer> env)
            : base(eventPublisher, popupController, asyncWorker, parentWindow, new Mock<IServer>().Object, a => env.Object)
        {
        }

        public int ShowErrorHitCount { get; private set; }

        protected override void ShowError(string header, string description)
        {
            ShowErrorHitCount++;
            base.ShowError(header, description);
        }

        public PersistenceSettingsViewModel ThePersistenceSettingsViewModel { get; set; }

        public SecurityViewModel TheSecurityViewModel
        {
            get { return _theSecurityViewModel; }
            set { _theSecurityViewModel = value; }
        }

        public LogSettingsViewModel TheLogSettingsViewModel { get; set; }

        protected override SecurityViewModel CreateSecurityViewModel()
        {
            return TheSecurityViewModel ?? new SecurityViewModel(Settings.Security, new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IServer>().Object, () => new Mock<IResourcePickerDialog>().Object);
        }

        protected override PerfcounterViewModel CreatePerfmonViewModel()
        {
            return ThePerfcounterViewModel ?? new PerfcounterViewModel(Settings.PerfCounters, new Mock<IServer>().Object, () => new Mock<IResourcePickerDialog>().Object);
        }

        public PerfcounterViewModel ThePerfcounterViewModel
        {
            get { return _thePerfcounterViewModel; }
            set { _thePerfcounterViewModel = value; }
        }

        protected override LogSettingsViewModel CreateLoggingViewModel()
        {
            return TheLogSettingsViewModel ?? CreateLogSettingViewModel();
        }

        static LogSettingsViewModel CreateLogSettingViewModel()
        {
            XmlConfigurator.ConfigureAndWatch(new FileInfo("Settings.config"));
            var loggingSettingsTo = new LoggingSettingsTo {FileLoggerLogSize = 50, FileLoggerLogLevel = "TRACE"};

            var _resourceRepo = new Mock<IResourceRepository>();
            var env = new Mock<IServer>();
            var expectedServerSettingsData = new ServerSettingsData
            {
                ExecutionLogLevel = LogLevel.DEBUG.ToString(),
                Sink = "AuditingSettingsData"
            };
            _resourceRepo.Setup(res => res.GetServerSettings(env.Object)).Returns(expectedServerSettingsData);
            var dependency = new Depends(Depends.ContainerType.AnonymousElasticsearch);
            var hostName = "http://" + dependency.Container.IP;
            var elasticsearchSource = new ElasticsearchSource
            {
                AuthenticationType = AuthenticationType.Anonymous,
                Port = dependency.Container.Port,
                HostName = hostName,
                SearchIndex = "warewolflogstests"
            };
            var jsonSource = JsonConvert.SerializeObject(elasticsearchSource);
            var auditingSettingsData = new AuditingSettingsData
            {
                Endpoint = "ws://127.0.0.1:5000/ws",
                EncryptDataSource = true,
                LoggingDataSource = new NamedGuidWithEncryptedPayload
                {
                    Name = "Auditing Data Source",
                    Value = Guid.Empty,
                    Payload = jsonSource
                },
            };
            _resourceRepo.Setup(res => res.GetAuditingSettings<AuditingSettingsData>(env.Object)).Returns(auditingSettingsData);
            var selectedAuditingSourceId = Guid.NewGuid();
            var mockAuditingSource = new Mock<IResource>();
            mockAuditingSource.Setup(source => source.ResourceID).Returns(selectedAuditingSourceId);
            var auditingSources = new Mock<IResource>();
            var expectedList = new List<IResource>
            {
                mockAuditingSource.Object, auditingSources.Object
            };
            _resourceRepo.Setup(resourceRepository => resourceRepository.FindResourcesByType<IAuditingSource>(env.Object)).Returns(expectedList);

            env.Setup(a => a.ResourceRepository).Returns(_resourceRepo.Object);
            var logSettingsViewModel = new LogSettingsViewModel(loggingSettingsTo, env.Object);
            return logSettingsViewModel;
        }

        protected override PersistenceSettingsViewModel CreatePersistenceViewModel()
        {
            return ThePersistenceSettingsViewModel ?? CreatePersistenceSettingsViewModel();
        }

        static PersistenceSettingsViewModel CreatePersistenceSettingsViewModel()
        {
            var resourceRepo = new Mock<IResourceRepository>();
            var env = new Mock<IServer>();
            var selectedPersistedDataSourceId = Guid.NewGuid();
            var persistenceSettingsData = new PersistenceSettingsData()
            {
                EncryptDataSource = true,
                Enable = true,
                PrepareSchemaIfNecessary = true,
                ServerName = "servername",
                DashboardHostname = "DashboardHostname",
                DashboardName = "Dashboardname",
                DashboardPort = "5001",
                PersistenceDataSource = new NamedGuidWithEncryptedPayload
                {
                    Name = "Persistence Data Source",
                    Value = selectedPersistedDataSourceId,
                    Payload = "jsonSource"
                }
            };
            resourceRepo.Setup(res => res.GetPersistenceSettings<PersistenceSettingsData>(env.Object)).Returns(persistenceSettingsData);

            var mockPersistenceDataSource = new Mock<IResource>();
            mockPersistenceDataSource.Setup(source => source.ResourceID).Returns(selectedPersistedDataSourceId);
            var persistenceDataSources = new Mock<IResource>();
            var expectedList = new List<IResource>
            {
                mockPersistenceDataSource.Object, persistenceDataSources.Object
            };
            resourceRepo.Setup(resourceRepository => resourceRepository.FindResourcesByType<IPersistenceSource>(env.Object)).Returns(expectedList);
            env.Setup(a => a.ResourceRepository).Returns(resourceRepo.Object);
            var persistenceSettingsViewModel = new PersistenceSettingsViewModel(env.Object);
            return persistenceSettingsViewModel;
        }

        public void CallDeactivate()
        {
            DoDeactivate(true);
        }
    }
}