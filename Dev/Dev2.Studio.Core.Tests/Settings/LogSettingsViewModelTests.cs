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
using Dev2.Common;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Help;
using Dev2.Common.Interfaces.Resources;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Services.Security;
using Dev2.Settings.Logging;
using Dev2.Studio.Interfaces;
using log4net.Config;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Configuration;
using Warewolf.Data;

namespace Dev2.Core.Tests.Settings
{
    [TestClass]
    [TestCategory("Studio Settings Core")]
    public class LogSettingsViewModelTests
    {
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
            Assert.IsTrue(hasPropertyChanged);
            Assert.IsTrue(logSettingsViewModel.IsDirty);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(LogSettingsViewModel))]
        public void LogSettingsViewModel_UpdateHelp_ShouldCallToHelpViewMode()
        {
            //------------Setup for test--------------------------
            var mockMainViewModel = new Mock<IShellViewModel>();
            var mockHelpViewModel = new Mock<IHelpWindowViewModel>();
            mockHelpViewModel.Setup(model => model.UpdateHelpText(It.IsAny<string>())).Verifiable();
            mockMainViewModel.Setup(model => model.HelpViewModel).Returns(mockHelpViewModel.Object);
            CustomContainer.Register(mockMainViewModel.Object);
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

        static LogSettingsViewModel CreateLogSettingViewModel(string sink)
        {
            XmlConfigurator.ConfigureAndWatch(new FileInfo("Settings.config"));
            var loggingSettingsTo = new LoggingSettingsTo {FileLoggerLogSize = 50, FileLoggerLogLevel = "TRACE"};

            var _resourceRepo = new Mock<IResourceRepository>();
            var env = new Mock<IServer>();

            var expectedServerSettingsData = new ServerSettingsData
            {
                Sink = sink
            };
            _resourceRepo.Setup(res => res.GetServerSettings(env.Object)).Returns(expectedServerSettingsData);
            if (sink == "LegacySettingsData")
            {
                var legacySettingsData = new LegacySettingsData() {AuditFilePath = "somePath"};
                _resourceRepo.Setup(res => res.GetAuditingSettings<LegacySettingsData>(env.Object)).Returns(legacySettingsData);
            }
            else
            {
                var auditingSettingsData = new AuditingSettingsData
                {
                    Endpoint = "ws://127.0.0.1:5000/ws",
                    LoggingDataSource = new NamedGuid {Name = "Auditing Data Source", Value = Guid.Empty,},
                };
                _resourceRepo.Setup(res => res.GetAuditingSettings<AuditingSettingsData>(env.Object)).Returns(auditingSettingsData);
            }

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
    }
}