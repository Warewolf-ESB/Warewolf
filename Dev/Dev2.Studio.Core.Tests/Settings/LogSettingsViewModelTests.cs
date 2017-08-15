using System;
using System.IO;
using Dev2.Common.Interfaces.Help;
using Dev2.Services.Security;
using Dev2.Settings.Logging;
using Dev2.Studio.Interfaces;
using log4net.Config;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;


namespace Dev2.Core.Tests.Settings
{
    [TestClass]
    public class LogSettingsViewModelTests
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("LogSettingsViewModel_Constructor")]
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
        [TestCategory("LogSettingsViewModel_Constructor")]
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
        [TestCategory("LogSettingsViewModel_ServerLogLevel")]
        public void LogSettingsViewModel_ServerLogLevel_Set_PropertyChangeFired()
        {
            //------------Setup for test--------------------------
            var logSettingsViewModel = CreateLogSettingViewModel();
            bool hasPropertyChanged = false;
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
            Assert.AreEqual(LogLevel.FATAL,logSettingsViewModel.ServerEventLogLevel);
            Assert.IsTrue(hasPropertyChanged);
            Assert.IsTrue(logSettingsViewModel.IsDirty);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("LogSettingsViewModel_Handle")]
        public void LogSettingsViewModel_UpdateHelp_ShouldCallToHelpViewMode()
        {
            //------------Setup for test--------------------------      
            var mockMainViewModel = new Mock<IShellViewModel>();
            var mockHelpViewModel = new Mock<IHelpWindowViewModel>();
            mockHelpViewModel.Setup(model => model.UpdateHelpText(It.IsAny<string>())).Verifiable();
            mockMainViewModel.Setup(model => model.HelpViewModel).Returns(mockHelpViewModel.Object);
            CustomContainer.Register(mockMainViewModel.Object);
            var viewModel = CreateLogSettingViewModel();
            //------------Execute Test---------------------------
            viewModel.UpdateHelpDescriptor("help");
            //------------Assert Results-------------------------
            mockHelpViewModel.Verify(model => model.UpdateHelpText(It.IsAny<string>()), Times.Once());
            viewModel.CloseHelpCommand.Execute(null);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("LogSettingsViewModel_StudioLogLevel")]
        public void LogSettingsViewModel_StudioLogLevel_Set_PropertyChangeFired()
        {
            //------------Setup for test--------------------------
            var logSettingsViewModel = CreateLogSettingViewModel();
            bool hasPropertyChanged = false;
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
        [TestCategory("LogSettingsViewModel_ServerLogMaxSize")]
        public void LogSettingsViewModel_ServerLogMaxSize_SetInt_PropertyChangeFired()
        {
            //------------Setup for test--------------------------
            var logSettingsViewModel = CreateLogSettingViewModel();
            bool hasPropertyChanged = false;
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
        [TestCategory("LogSettingsViewModel_ServerLogMaxSize")]
        public void LogSettingsViewModel_ServerLogMaxSize_SetNonInt_PropertyChangeNotFired()
        {
            //------------Setup for test--------------------------
            var logSettingsViewModel = CreateLogSettingViewModel();
            bool hasPropertyChanged = false;
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
        [TestCategory("LogSettingsViewModel_StudioLogMaxSize")]
        public void LogSettingsViewModel_StudioLogMaxSize_SetInt_PropertyChangeFired()
        {
            //------------Setup for test--------------------------
            var logSettingsViewModel = CreateLogSettingViewModel();
            bool hasPropertyChanged = false;
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
        [TestCategory("LogSettingsViewModel_StudioFileLogLevel")]
        public void LogSettingsViewModel_StudioFileLogLevel_Construct_IsDebug()
        {
            //------------Setup for test--------------------------
            var logSettingsViewModel = CreateLogSettingViewModel();

            //------------Execute Test---------------------------
            
            //------------Assert Results-------------------------
            Assert.AreEqual(LogLevel.DEBUG, logSettingsViewModel.StudioFileLogLevel);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("LogSettingsViewModel_StudioFileLogLevel")]
        public void LogSettingsViewModel_StudioFileLogLevel_SetLevel_IsInfo()
        {
            //------------Setup for test--------------------------
            var logSettingsViewModel = CreateLogSettingViewModel();

            //------------Execute Test---------------------------
            logSettingsViewModel.StudioFileLogLevel = LogLevel.INFO;

            //------------Assert Results-------------------------
            Assert.AreEqual(LogLevel.INFO, logSettingsViewModel.StudioFileLogLevel);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("LogSettingsViewModel_ServerFileLogLevel")]
        public void LogSettingsViewModel_ServerFileLogLevel_Construct_IsTrace()
        {
            //------------Setup for test--------------------------
            var logSettingsViewModel = CreateLogSettingViewModel();

            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.AreEqual(LogLevel.TRACE, logSettingsViewModel.ServerFileLogLevel);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("LogSettingsViewModel_ServerFileLogLevel")]
        public void LogSettingsViewModel_ServerFileLogLevel_SetLevel_IsInfo()
        {
            //------------Setup for test--------------------------
            var logSettingsViewModel = CreateLogSettingViewModel();

            //------------Execute Test---------------------------
            logSettingsViewModel.ServerFileLogLevel = LogLevel.INFO;

            //------------Assert Results-------------------------
            Assert.AreEqual(LogLevel.INFO, logSettingsViewModel.ServerFileLogLevel);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("LogSettingsViewModel_CanEdit")]
        public void LogSettingsViewModel_CanEdit_Construct_IsFalse()
        {
            //------------Setup for test--------------------------
            var logSettingsViewModel = CreateLogSettingViewModel();

            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.IsFalse(logSettingsViewModel.CanEditLogSettings);
            Assert.IsFalse(logSettingsViewModel.CanEditStudioLogSettings);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("LogSettingsViewModel_CanEdit")]
        public void LogSettingsViewModel_CanEdit_Construct_IsTrue()
        {
            //------------Setup for test--------------------------
            var logSettingsViewModel = CreateLogSettingViewModel();

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
        [TestCategory("LogSettingsViewModel_StudioLogMaxSize")]
        public void LogSettingsViewModel_StudioLogMaxSize_SetNonInt_PropertyChangeNotFired()
        {
            //------------Setup for test--------------------------
            var logSettingsViewModel = CreateLogSettingViewModel();
            bool hasPropertyChanged = false;
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

        static LogSettingsViewModel CreateLogSettingViewModel()
        {
            XmlConfigurator.ConfigureAndWatch(new FileInfo("Settings.config"));
            var loggingSettingsTo = new LoggingSettingsTo { FileLoggerLogSize = 50, FileLoggerLogLevel = "TRACE" };
            var logSettingsViewModel = new LogSettingsViewModel(loggingSettingsTo, new Mock<IServer>().Object);
            return logSettingsViewModel;
        }
    }
}
