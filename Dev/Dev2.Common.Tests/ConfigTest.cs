using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Common.Wrappers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.IO;

namespace Dev2.Common.Tests
{
    [TestClass]
    public class ConfigTest
    {
        class MockConfigurationManager : IConfigurationManager
        {
            readonly Dictionary<string, string> _store = new Dictionary<string, string>();
            public string this[string settingName]
            {
                get
                {
                    return _store.ContainsKey(settingName) ? _store[settingName] : null;
                }
                set
                {
                    _store[settingName] = value;
                }
            }
        }

        [TestMethod]
        public void Constructor_Setup_ServerSettings_NotNull_Expected()
        {
            var mockConfigurationManager = new Mock<IConfigurationManager>();
            Config.ConfigureSettings(mockConfigurationManager.Object);
            const string expectedPath = @"C:\ProgramData\Warewolf\Audits";
            Assert.IsNotNull(Config.Server);
            Assert.AreEqual(expectedPath, Config.Server.AuditFilePath);
        }

        [TestMethod]
        public void Update_AppConfig_AuditFilePath_Default_Not_Expected()
        {
            const string expectedPath2 = "SomeOtherPath";

            var mockConfig = new MockConfigurationManager();
            Config.ConfigureSettings(mockConfig);

            Config.Server.AuditFilePath = expectedPath2;
            Assert.AreEqual(expectedPath2, Config.Server.AuditFilePath);
        }

        [TestMethod]
        public void ServerSettingsData_Equals_Valid_Expected()
        {
            var expectedServerSettingsData = new ServerSettingsData
            {
                WebServerPort = 3142,
                WebServerSslPort = 3143,
                SslCertificateName = "SslCertificateName",
                CollectUsageStats = true,
                DaysToKeepTempFiles = 2,
                AuditFilePath = "AuditFilePath"
            };

            var serverSettingsData = new ServerSettingsData
            {
                WebServerPort = 3142,
                WebServerSslPort = 3143,
                SslCertificateName = "SslCertificateName",
                CollectUsageStats = true,
                DaysToKeepTempFiles = 2,
                AuditFilePath = "AuditFilePath"
            };

            Assert.IsTrue(serverSettingsData.Equals(expectedServerSettingsData));
        }

        [TestMethod]
        public void Get_AppConfig_Configuration()
        {
            const string expectedPath = @"C:\ProgramData\Warewolf\Audits";
            var mockConfig = new MockConfigurationManager();
            Config.ConfigureSettings(mockConfig);

            var settings = Config.Server.Get();
            Assert.AreEqual(8, settings.GetType().GetProperties().Length);

            Assert.AreEqual((ushort)0, settings.WebServerPort);
            Assert.AreEqual((ushort)0, settings.WebServerSslPort);
            Assert.AreEqual(null, settings.SslCertificateName);
            Assert.AreEqual(false, settings.CollectUsageStats);
            Assert.AreEqual(0, settings.DaysToKeepTempFiles);
            Assert.AreEqual(expectedPath, settings.AuditFilePath);
            Assert.AreEqual(true, settings.EnableDetailedLogging);
            Assert.AreEqual(200, settings.LogFlushInterval);
        }

        [TestMethod]
        public void GetServerSettings_Constants()
        {
            Assert.AreEqual(@"C:\ProgramData\Warewolf\Audits", Config.Server.DefaultAuditPath);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("Logging Paths")]
        public void ServerSettings_SaveIfNotExists()
        {
            var mockConfigurationManager = new Mock<IConfigurationManager>();
            Config.ConfigureSettings(mockConfigurationManager.Object);
            //arrange
            ServerSettings serverSettings;

            var sourceFilePath = Config.Server.AuditFilePath;

            var source = Path.Combine(sourceFilePath, "auditDB.db");

            var mockIFile = new Mock<IFile>();
            mockIFile.Setup(o => o.Exists(It.IsAny<string>())).Returns(false).Verifiable();

            var mockDirectory = new Mock<IDirectory>();

            serverSettings = new ServerSettings(mockConfigurationManager.Object, mockIFile.Object, mockDirectory.Object);

            //act
            serverSettings.SaveIfNotExists();

            //assert
            mockIFile.Verify();
            mockDirectory.Verify();
            mockConfigurationManager.Verify();

        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory("Logging Paths")]
        public void ServerSettings_SaveLoggingPath()
        {
            var newAuditsFilePath = "falsepath7";

            var mockConfigurationManager = new Mock<IConfigurationManager>();
            mockConfigurationManager.SetupSet(o => o["AuditFilePath"] = newAuditsFilePath).Verifiable();
            Config.ConfigureSettings(mockConfigurationManager.Object);
            //arrange
            ServerSettings serverSettings;

            var sourceFilePath = Config.Server.AuditFilePath;

            var source = Path.Combine(sourceFilePath, "auditDB.db");
            var destination = Path.Combine(newAuditsFilePath, "auditDB.db");

            var mockIFile = new Mock<IFile>();
            mockIFile.Setup(o => o.Exists(It.IsAny<string>())).Returns(true).Verifiable();
            mockIFile.Setup(c=> c.Copy(It.IsAny<string>() , It.IsAny<string>())).Verifiable();

            var mockDirectory = new Mock<IDirectory>();
            mockDirectory.Setup(d => d.CreateIfNotExists(It.IsAny<string>())).Returns(newAuditsFilePath).Verifiable();

            serverSettings = new ServerSettings(mockConfigurationManager.Object, mockIFile.Object, mockDirectory.Object);

            //act
            var actual = serverSettings.SaveLoggingPath(sourceFilePath);
            Assert.IsFalse(actual);

            actual = serverSettings.SaveLoggingPath(newAuditsFilePath);
            //assert
            Assert.IsTrue(actual);
            mockIFile.Verify();
            mockDirectory.Verify();
            mockConfigurationManager.Verify();
        }
    }
}
