using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Wrappers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

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
            Assert.AreEqual(7, settings.GetType().GetProperties().Length);

            Assert.AreEqual((ushort)0, settings.WebServerPort);
            Assert.AreEqual((ushort)0, settings.WebServerSslPort);
            Assert.AreEqual(null, settings.SslCertificateName);
            Assert.AreEqual(false, settings.CollectUsageStats);
            Assert.AreEqual(0, settings.DaysToKeepTempFiles);
            Assert.AreEqual(expectedPath, settings.AuditFilePath);
            Assert.AreEqual(true, settings.EnableDetailedLogging);
        }

        [TestMethod]
        public void GetServerSettings_Constants()
        {
            Assert.AreEqual(@"C:\ProgramData\Warewolf\Audits", Config.Server.DefaultAuditPath);
        }
    }
}
