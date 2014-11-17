using System;
using System.IO;
using Dev2.Services.Security;
using Dev2.Settings.Logging;
using Dev2.Studio.Core.Interfaces;
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
            new LogSettingsViewModel(null, new Mock<IEnvironmentModel>().Object);
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
                if (args.PropertyName == "ServerLogLevel")
                {
                    hasPropertyChanged = true;
                }
            };

            //------------Execute Test---------------------------
            logSettingsViewModel.ServerLogLevel = LogLevel.Fatal;
            //------------Assert Results-------------------------
            Assert.AreEqual(LogLevel.Fatal,logSettingsViewModel.ServerLogLevel);
            Assert.IsTrue(hasPropertyChanged);
            Assert.IsTrue(logSettingsViewModel.IsDirty);
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
                if (args.PropertyName == "StudioLogLevel")
                {
                    hasPropertyChanged = true;
                }
            };

            //------------Execute Test---------------------------
            logSettingsViewModel.StudioLogLevel = LogLevel.Info;
            //------------Assert Results-------------------------
            Assert.AreEqual(LogLevel.Info,logSettingsViewModel.StudioLogLevel);
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
            Assert.AreEqual("0", logSettingsViewModel.ServerLogMaxSize);
            Assert.IsFalse(hasPropertyChanged);
            Assert.IsTrue(logSettingsViewModel.IsDirty);
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

            //------------Execute Test---------------------------
            logSettingsViewModel.StudioLogMaxSize = "aa";
            //------------Assert Results-------------------------
            Assert.AreEqual("0", logSettingsViewModel.StudioLogMaxSize);
            Assert.IsFalse(hasPropertyChanged);
            Assert.IsTrue(logSettingsViewModel.IsDirty);
        }

        static LogSettingsViewModel CreateLogSettingViewModel()
        {
            XmlConfigurator.ConfigureAndWatch(new FileInfo("Settings.config"));
            var loggingSettingsTo = new LoggingSettingsTo();
            loggingSettingsTo.LogSize = 50;
            loggingSettingsTo.LogLevel = "Trace";
            var logSettingsViewModel = new LogSettingsViewModel(loggingSettingsTo, new Mock<IEnvironmentModel>().Object);
            return logSettingsViewModel;
        }
    }
}
