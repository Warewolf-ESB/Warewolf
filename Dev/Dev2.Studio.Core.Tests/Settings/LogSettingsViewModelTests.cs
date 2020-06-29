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
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Help;
using Dev2.Common.Interfaces.Resources;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Communication;
using Dev2.Data.Interfaces.Enums;
using Dev2.Data.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Services.Security;
using Dev2.Settings.Logging;
using Dev2.Studio.Interfaces;
using log4net.Config;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Configuration;
using Warewolf.Data;
using Warewolf.Security.Encryption;
using Warewolf.UnitTestAttributes;

namespace Dev2.Core.Tests.Settings
{
    [TestClass]
    [TestCategory("Studio Settings Core")]
    public class LogSettingsViewModelTests
    {
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(LogSettingsViewModel))]
        public void LogSettingsViewModel_Constructor_Equals_Null_ReturnFalse()
        {
            //------------Setup for test--------------------------
            var viewModel = CreateLogSettingViewModel("AuditingSettingsData");
            var logSettingsViewModel2 = CreateLogSettingViewModel("AuditingSettingsData");
            logSettingsViewModel2.SetItem(viewModel);

            //------------Execute Test---------------------------
            var expected = viewModel.Equals(logSettingsViewModel2);
            //------------Assert Results-------------------------
            Assert.IsFalse(expected);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(LogSettingsViewModel))]
        public void LogSettingsViewModel_Constructor_Equals_NotNull_ReturnTrue()
        {
            //------------Setup for test--------------------------
            var viewModel = CreateLogSettingViewModel("AuditingSettingsData");
            var logSettingsViewModel2 = CreateLogSettingViewModel("AuditingSettingsData");
            logSettingsViewModel2.SetItem(viewModel);
            logSettingsViewModel2.ResourceSourceId = viewModel.ResourceSourceId;

            //------------Execute Test---------------------------
            var expected = viewModel.Equals(logSettingsViewModel2);
            //------------Assert Results-------------------------
            Assert.IsTrue(expected);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(LogSettingsViewModel))]
        [ExpectedException(typeof(ArgumentNullException))]
        public void LogSettingsViewModel_Constructor_NullValueLoggingSettingTo_ExceptionThrown()
        {
            //------------Setup for test--------------------------


            //------------Execute Test---------------------------
            new LogSettingsViewModel(null, new Mock<IServer>().Object);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(LogSettingsViewModel))]
        [ExpectedException(typeof(ArgumentNullException))]
        public void LogSettingsViewModel_Constructor_NullValueEnvironment_ExceptionThrown()
        {
            //------------Setup for test--------------------------


            //------------Execute Test---------------------------
            new LogSettingsViewModel(new LoggingSettingsTo(), null);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(LogSettingsViewModel))]
        public void LogSettingsViewModel_ServerLogLevel_Set_PropertyChangeFired()
        {
            //------------Setup for test--------------------------
            var logSettingsViewModel = CreateLogSettingViewModel("AuditingSettingsData");
            var hasPropertyChanged = false;
            logSettingsViewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "ServerEventLogLevel")
                {
                    hasPropertyChanged = true;
                }
            };

            //------------Execute Test---------------------------
            logSettingsViewModel.ServerEventLogLevel = LogLevel.FATAL;
            //------------Assert Results-------------------------
            Assert.AreEqual(LogLevel.FATAL, logSettingsViewModel.ServerEventLogLevel);
            Assert.AreEqual(LogLevel.FATAL, logSettingsViewModel.ExecutionLogLevel);
            Assert.IsTrue(hasPropertyChanged);
            Assert.IsTrue(logSettingsViewModel.IsDirty);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(LogSettingsViewModel))]
        [DoNotParallelize]
        public void LogSettingsViewModel_UpdateHelp_ShouldCallToHelpViewMode()
        {
            //------------Setup for test--------------------------
            var mockMainViewModel = new Mock<IShellViewModel>();
            var mockHelpViewModel = new Mock<IHelpWindowViewModel>();
            mockHelpViewModel.Setup(model => model.UpdateHelpText(It.IsAny<string>())).Verifiable();
            mockMainViewModel.Setup(model => model.HelpViewModel).Returns(mockHelpViewModel.Object);
            CustomContainer.Register(mockMainViewModel.Object);
            CustomContainer.Register(new Mock<IPopupController>().Object);
            var viewModel = CreateLogSettingViewModel("AuditingSettingsData");
            //------------Execute Test---------------------------
            viewModel.UpdateHelpDescriptor("help");
            //------------Assert Results-------------------------
            mockHelpViewModel.Verify(model => model.UpdateHelpText(It.IsAny<string>()), Times.Once());
            viewModel.CloseHelpCommand.Execute(null);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(LogSettingsViewModel))]
        public void LogSettingsViewModel_SelectedLoggingType_ShouldSelectLoggingType()
        {
            //------------Setup for test--------------------------
            var mockMainViewModel = new Mock<IShellViewModel>();
            CustomContainer.Register(mockMainViewModel.Object);
            var viewModel = CreateLogSettingViewModel("AuditingSettingsData");
            //------------Execute Test---------------------------
            viewModel.SelectedLoggingType = "Fatal: Only log events that are fatal";
            //------------Assert Results-------------------------
            Assert.AreEqual("Fatal: Only log events that are fatal", viewModel.SelectedLoggingType);
            Assert.AreEqual(LogLevel.FATAL, viewModel.ServerEventLogLevel);
            Assert.AreEqual(LogLevel.FATAL, viewModel.ExecutionLogLevel);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(LogSettingsViewModel))]
        public void LogSettingsViewModel_GetServerLogFileCommand_CanExecute()
        {
            //------------Setup for test--------------------------
            var mockMainViewModel = new Mock<IShellViewModel>();
            CustomContainer.Register(mockMainViewModel.Object);
            var viewModel = CreateLogSettingViewModel("AuditingSettingsData");
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            var canExecute = viewModel.GetServerLogFileCommand.CanExecute(null);
            Assert.IsTrue(canExecute);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(LogSettingsViewModel))]
        public void LogSettingsViewModel_GetStudioLogFileCommand_CanExecute()
        {
            //------------Setup for test--------------------------
            var mockMainViewModel = new Mock<IShellViewModel>();
            CustomContainer.Register(mockMainViewModel.Object);
            var viewModel = CreateLogSettingViewModel("AuditingSettingsData");
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            var canExecute = viewModel.GetStudioLogFileCommand.CanExecute(null);
            Assert.IsTrue(canExecute);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(LogSettingsViewModel))]
        public void LogSettingsViewModel_StudioLogLevel_Set_PropertyChangeFired()
        {
            //------------Setup for test--------------------------
            var logSettingsViewModel = CreateLogSettingViewModel("AuditingSettingsData");
            var hasPropertyChanged = false;
            logSettingsViewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "StudioEventLogLevel")
                {
                    hasPropertyChanged = true;
                }
            };

            //------------Execute Test---------------------------
            logSettingsViewModel.StudioEventLogLevel = LogLevel.INFO;

            //------------Assert Results-------------------------
            Assert.AreEqual(LogLevel.INFO, logSettingsViewModel.StudioEventLogLevel);
            Assert.IsTrue(hasPropertyChanged);
            Assert.IsTrue(logSettingsViewModel.IsDirty);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(LogSettingsViewModel))]
        public void LogSettingsViewModel_ServerLogMaxSize_SetInt_PropertyChangeFired()
        {
            //------------Setup for test--------------------------
            var logSettingsViewModel = CreateLogSettingViewModel("AuditingSettingsData");
            var hasPropertyChanged = false;
            logSettingsViewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "ServerLogMaxSize")
                {
                    hasPropertyChanged = true;
                }
            };

            //------------Execute Test---------------------------
            logSettingsViewModel.ServerLogMaxSize = "20";
            //------------Assert Results-------------------------
            Assert.AreEqual("20", logSettingsViewModel.ServerLogMaxSize);
            Assert.IsTrue(hasPropertyChanged);
            Assert.IsTrue(logSettingsViewModel.IsDirty);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(LogSettingsViewModel))]
        public void LogSettingsViewModel_ServerLogMaxSize_SetNonInt_PropertyChangeNotFired()
        {
            //------------Setup for test--------------------------
            var logSettingsViewModel = CreateLogSettingViewModel("AuditingSettingsData");
            var hasPropertyChanged = false;
            logSettingsViewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "ServerLogMaxSize")
                {
                    hasPropertyChanged = true;
                }
            };

            //------------Execute Test---------------------------
            logSettingsViewModel.ServerLogMaxSize = "aa";
            //------------Assert Results-------------------------
            Assert.AreEqual("50", logSettingsViewModel.ServerLogMaxSize);
            Assert.IsFalse(hasPropertyChanged);
            Assert.IsFalse(logSettingsViewModel.IsDirty);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(LogSettingsViewModel))]
        public void LogSettingsViewModel_StudioLogMaxSize_SetInt_PropertyChangeFired()
        {
            //------------Setup for test--------------------------
            var logSettingsViewModel = CreateLogSettingViewModel("AuditingSettingsData");
            var hasPropertyChanged = false;
            logSettingsViewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "StudioLogMaxSize")
                {
                    hasPropertyChanged = true;
                }
            };

            //------------Execute Test---------------------------
            logSettingsViewModel.StudioLogMaxSize = "20";
            //------------Assert Results-------------------------
            Assert.AreEqual("20", logSettingsViewModel.StudioLogMaxSize);
            Assert.IsTrue(hasPropertyChanged);
            Assert.IsTrue(logSettingsViewModel.IsDirty);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(LogSettingsViewModel))]
        public void LogSettingsViewModel_StudioFileLogLevel_Construct_IsDebug()
        {
            //------------Setup for test--------------------------
            var logSettingsViewModel = CreateLogSettingViewModel("AuditingSettingsData");

            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.AreEqual(LogLevel.DEBUG, logSettingsViewModel.StudioFileLogLevel);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(LogSettingsViewModel))]
        public void LogSettingsViewModel_StudioFileLogLevel_SetLevel_IsInfo()
        {
            //------------Setup for test--------------------------
            var logSettingsViewModel = CreateLogSettingViewModel("AuditingSettingsData");

            //------------Execute Test---------------------------
            logSettingsViewModel.StudioFileLogLevel = LogLevel.INFO;

            //------------Assert Results-------------------------
            Assert.AreEqual(LogLevel.INFO, logSettingsViewModel.StudioFileLogLevel);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(LogSettingsViewModel))]
        public void LogSettingsViewModel_CanEdit_Construct_IsFalse()
        {
            //------------Setup for test--------------------------
            var logSettingsViewModel = CreateLogSettingViewModel("AuditingSettingsData");

            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.IsFalse(logSettingsViewModel.CanEditLogSettings);
            Assert.IsFalse(logSettingsViewModel.CanEditStudioLogSettings);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(LogSettingsViewModel))]
        public void LogSettingsViewModel_IsLegacy_Construct_IsTrue()
        {
            //------------Setup for test--------------------------
            var logSettingsViewModel = CreateLogSettingViewModel("LegacySettingsData");

            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.IsTrue(logSettingsViewModel.IsLegacy);
            Assert.AreEqual(LogLevel.DEBUG, logSettingsViewModel.ExecutionLogLevel);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(LogSettingsViewModel))]
        public void LogSettingsViewModel_IsLegacy_Construct_IsFalse()
        {
            //------------Setup for test--------------------------
            var logSettingsViewModel = CreateLogSettingViewModel("AuditingSettingsData");

            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.IsFalse(logSettingsViewModel.IsLegacy);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(LogSettingsViewModel))]
        public void LogSettingsViewModel_CanEdit_Construct_IsTrue()
        {
            //------------Setup for test--------------------------
            var logSettingsViewModel = CreateLogSettingViewModel("AuditingSettingsData");

            //------------Execute Test---------------------------
            var env = new Mock<IServer>();
            env.Setup(a => a.IsLocalHost).Returns(true);
            env.Setup(a => a.IsConnected).Returns(true);
            logSettingsViewModel.CurrentEnvironment = env.Object;

            //------------Assert Results-------------------------
            Assert.IsTrue(logSettingsViewModel.CanEditLogSettings);
            Assert.IsTrue(logSettingsViewModel.CanEditStudioLogSettings);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(LogSettingsViewModel))]
        public void LogSettingsViewModel_StudioLogMaxSize_SetNonInt_PropertyChangeNotFired()
        {
            //------------Setup for test--------------------------
            var logSettingsViewModel = CreateLogSettingViewModel("LegacySettingsData");
            var hasPropertyChanged = false;
            logSettingsViewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "StudioLogMaxSize")
                {
                    hasPropertyChanged = true;
                }
            };
            //---------------Assert Precondition----------------
            Assert.IsFalse(logSettingsViewModel.IsDirty);
            //------------Execute Test---------------------------
            logSettingsViewModel.StudioLogMaxSize = "aa";
            //------------Assert Results-------------------------
            Assert.IsFalse(hasPropertyChanged);
            Assert.IsFalse(logSettingsViewModel.IsDirty);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(LogSettingsViewModel))]
        public void LogSettingsViewModel_AuditsFilePath_PropertyChangeFired()
        {
            //------------Setup for test--------------------------
            var logSettingsViewModel = CreateLogSettingViewModel("LegacySettingsData");
            var hasPropertyChanged = false;
            logSettingsViewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "AuditFilePath")
                {
                    hasPropertyChanged = true;
                }
            };
            //---------------Assert Precondition----------------
            Assert.IsFalse(logSettingsViewModel.IsDirty);
            //------------Execute Test---------------------------
            logSettingsViewModel.AuditFilePath = @"C:\ProgramData\Warewolf\Audits";
            //------------Assert Results-------------------------
            Assert.AreEqual(@"C:\ProgramData\Warewolf\Audits", logSettingsViewModel.AuditFilePath);
            Assert.IsTrue(hasPropertyChanged);
            Assert.IsTrue(logSettingsViewModel.IsDirty);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(LogSettingsViewModel))]
        public void LogSettingsViewModel_Save_LegacySettingsData()
        {
            var _resourceRepo = new Mock<IResourceRepository>();
            //------------Setup for test--------------------------
            var logSettingsViewModel = CreateLogSettingViewModel("LegacySettingsData", _resourceRepo);
            var loggingSettingsTo = new LoggingSettingsTo {FileLoggerLogSize = 50, FileLoggerLogLevel = "TRACE", EventLogLoggerLogLevel = "DEBUG"};
            //------------Execute Test---------------------------
            CustomContainer.Register(new Mock<IPopupController>().Object);
            var mockResource = new Mock<IResource>();
            mockResource.Setup(o => o.ResourceName).Returns("Default");
            mockResource.Setup(o => o.ResourceID).Returns(Guid.Empty);
            logSettingsViewModel.SelectedAuditingSource = mockResource.Object;
            logSettingsViewModel.AuditFilePath = @"C:\ProgramData\Warewolf\Audits";
            //------------Assert Results-------------------------
            logSettingsViewModel.Save(loggingSettingsTo);
            _resourceRepo.Verify(o => o.SaveAuditingSettings(It.IsAny<IServer>(), It.IsAny<LegacySettingsData>()), Times.Once);
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(LogSettingsViewModel))]
        public void LogSettingsViewModel_Save_ChangeSink()
        {
            var _resourceRepo = new Mock<IResourceRepository>();
            //------------Setup for test--------------------------
            var logSettingsViewModel = CreateLogSettingViewModel("LegacySettingsData", _resourceRepo);
            var loggingSettingsTo = new LoggingSettingsTo { FileLoggerLogSize = 50, FileLoggerLogLevel = "TRACE", EventLogLoggerLogLevel = "DEBUG" };
            //------------Execute Test---------------------------
            CustomContainer.Register(new Mock<IPopupController>().Object);
            var mockResource = new Mock<IResource>();
            mockResource.Setup(o => o.ResourceName).Returns("Default");
            mockResource.Setup(o => o.ResourceID).Returns(Guid.NewGuid());
            logSettingsViewModel.SelectedAuditingSource = mockResource.Object;
            //------------Assert Results-------------------------
            logSettingsViewModel.Save(loggingSettingsTo);
            _resourceRepo.Verify(o => o.SaveAuditingSettings(It.IsAny<IServer>(), It.IsAny<AuditingSettingsData>()), Times.Once);
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(LogSettingsViewModel))]
        public void LogSettingsViewModel_Save_AuditingSettingsData()
        {
            var _resourceRepo = new Mock<IResourceRepository>();
            //------------Setup for test--------------------------
            var logSettingsViewModel = CreateLogSettingViewModel("AuditingSettingsData", _resourceRepo);
            var loggingSettingsTo = new LoggingSettingsTo {FileLoggerLogSize = 50, FileLoggerLogLevel = "TRACE", EventLogLoggerLogLevel = "DEBUG"};
            //------------Execute Test---------------------------
            var mockResource = new Mock<IResource>();
            mockResource.Setup(o => o.ResourceName).Returns("Default");
            mockResource.Setup(o => o.ResourceID).Returns(Guid.NewGuid());
            logSettingsViewModel.SelectedAuditingSource = mockResource.Object;
            logSettingsViewModel.ExecutionLogLevel = LogLevel.TRACE;
            logSettingsViewModel.EncryptDataSource = false;
            CustomContainer.Register(new Mock<IPopupController>().Object);
            //------------Assert Results-------------------------
            logSettingsViewModel.Save(loggingSettingsTo);
            _resourceRepo.Verify(o => o.SaveAuditingSettings(It.IsAny<IServer>(), It.IsAny<AuditingSettingsData>()), Times.Once);
        }

        static LogSettingsViewModel CreateLogSettingViewModel(string sink, Mock<IResourceRepository> _resourceRepo = null)
        {
            XmlConfigurator.ConfigureAndWatch(new FileInfo("Settings.config"));
            var loggingSettingsTo = new LoggingSettingsTo {FileLoggerLogSize = 50, FileLoggerLogLevel = "TRACE", EventLogLoggerLogLevel = "DEBUG"};

            var env = new Mock<IServer>();
            if (_resourceRepo is null)
            {
                _resourceRepo = new Mock<IResourceRepository>();
            }

            var expectedServerSettingsData = new ServerSettingsData
            {
                Sink = sink,
                ExecutionLogLevel = LogLevel.DEBUG.ToString()
            };
            _resourceRepo.Setup(res => res.GetServerSettings(env.Object)).Returns(expectedServerSettingsData);
            var selectedAuditingSourceId = Guid.NewGuid();
            if (sink == "LegacySettingsData")
            {
                var legacySettingsData = new LegacySettingsData() {AuditFilePath = "somePath"};
                _resourceRepo.Setup(res => res.GetAuditingSettings<LegacySettingsData>(env.Object)).Returns(legacySettingsData);
                _resourceRepo.Setup(res => res.SaveAuditingSettings(env.Object, legacySettingsData)).Verifiable();
            }
            else
            {
                var dependency = new Depends(Depends.ContainerType.AnonymousElasticsearch);
                var hostName = "http://" + dependency.Container.IP;
                var elasticsearchSource = new ElasticsearchSource
                {
                    AuthenticationType = AuthenticationType.Anonymous,
                    Port = dependency.Container.Port,
                    HostName = hostName,
                    SearchIndex = "warewolflogstests"
                };
                var serializer = new Dev2JsonSerializer();
                var payload = serializer.Serialize(elasticsearchSource);
                var encryptedPayload = DpapiWrapper.Encrypt(payload);
                var auditingSettingsData = new AuditingSettingsData
                {
                    Endpoint = "ws://127.0.0.1:5000/ws",
                    EncryptDataSource = true,
                    LoggingDataSource = new NamedGuidWithEncryptedPayload
                    {
                        Name = "Auditing Data Source",
                        Value = selectedAuditingSourceId,
                        Payload = encryptedPayload
                    },
                };
                _resourceRepo.Setup(res => res.GetAuditingSettings<AuditingSettingsData>(env.Object)).Returns(auditingSettingsData);
                _resourceRepo.Setup(res => res.SaveAuditingSettings(env.Object, auditingSettingsData)).Verifiable();
            }

            IResource mockAuditingSource = new ElasticsearchSource
            {
                ResourceID = selectedAuditingSourceId,
                ResourceName = "Auditing Data Source"
            };
            var expectedList = new List<IResource>();
            expectedList.Add(mockAuditingSource);

            _resourceRepo.Setup(resourceRepository => resourceRepository.FindResourcesByType<IAuditingSource>(env.Object)).Returns(expectedList);

            env.Setup(a => a.ResourceRepository).Returns(_resourceRepo.Object);
            var logSettingsViewModel = new LogSettingsViewModel(loggingSettingsTo, env.Object);
            return logSettingsViewModel;
        }
    }
}