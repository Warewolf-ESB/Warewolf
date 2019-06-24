using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Common.Wrappers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        public void ServerSettings_GivenDefaults_ExpectJsonDefaults()
        {
            var data = JsonConvert.SerializeObject(new ServerSettings());
            var expectedData = "{\"AuditFilePath\":\"C:\\\\ProgramData\\\\Warewolf\\\\Audits\",\"EnableDetailedLogging\":true,\"WebServerPort\":0,\"WebServerSslPort\":0,\"SslCertificateName\":null,\"CollectUsageStats\":false,\"DaysToKeepTempFiles\":0,\"LogFlushInterval\":200}";
            Assert.AreEqual(expectedData, data);
        }

        [TestMethod]
        public void ServerSettings_GivenDefaults_ExpectDefault_AuditFilePath()
        {
            var settingsPath = "";
            var fileWrapper = new Mock<IFile>();
            fileWrapper.Setup(o => o.ReadAllText(It.IsAny<string>())).Returns("{}");
            var directoryWrapper = new Mock<IDirectory>();
            var serverConfig = new ServerSettings(settingsPath, fileWrapper.Object, directoryWrapper.Object);

            const string expectedPath = @"C:\ProgramData\Warewolf\Audits";
            Assert.AreEqual(expectedPath, serverConfig.AuditFilePath);
        }

        [TestMethod]
        public void ServerSettings_Update_AppConfig_AuditFilePath_Default_Not_Expected()
        {
            var settingsPath = Path.GetTempFileName();
            var fileWrapper = new Mock<IFile>();
            fileWrapper.Setup(o => o.ReadAllText(It.IsAny<string>())).Returns("{}");
            fileWrapper.Setup(o => o.Exists(settingsPath)).Returns(true);

            const string expectedPath2 = "SomeOtherPath";
            bool expectedPathWritten = false;
            fileWrapper.Setup(o => o.WriteAllText(settingsPath, It.IsAny<string>())).Callback<string,string>((path, data) => {
                var parsedJson = (JObject)JsonConvert.DeserializeObject(data);
                if (parsedJson["AuditFilePath"].ToString() == expectedPath2)
                {
                    expectedPathWritten = true;
                }
            });

            var directoryWrapper = new Mock<IDirectory>();
            var serverConfig = new ServerSettings(settingsPath, fileWrapper.Object, directoryWrapper.Object);


            serverConfig.AuditFilePath = expectedPath2;
            Assert.AreEqual(expectedPath2, serverConfig.AuditFilePath);

            fileWrapper.Verify(o => o.Exists(settingsPath), Times.Once);
            fileWrapper.Verify(o => o.ReadAllText(settingsPath), Times.Once);
            fileWrapper.Verify(o => o.WriteAllText(settingsPath, It.IsAny<string>()), Times.Once);

            var expectedSaveDirectory = Path.GetDirectoryName(settingsPath);
            directoryWrapper.Verify(o => o.CreateIfNotExists(expectedSaveDirectory), Times.Once);

            Assert.IsTrue(expectedPathWritten);
        }

        [TestMethod]
        public void ServerSettings_ServerSettingsData_Equals_Valid_Expected()
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
        public void ServerSettings_Get_AppConfig_Configuration()
        {
            const string expectedPath = @"C:\ProgramData\Warewolf\Audits";
            var settingsPath = "";
            var fileWrapper = new Mock<IFile>();
            fileWrapper.Setup(o => o.ReadAllText(It.IsAny<string>())).Returns("{}");
            var directoryWrapper = new Mock<IDirectory>();
            var serverConfig = new ServerSettings(settingsPath, fileWrapper.Object, directoryWrapper.Object);

            var settings = serverConfig.Get();
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
        public void ServerSettings_GetServerSettings_Constants()
        {
            Assert.AreEqual(@"C:\ProgramData\Warewolf\Audits", ServerSettings.DefaultAuditPath);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("Logging Paths")]
        public void ServerSettings_SaveIfNotExists()
        {
            var settingsPath = Path.GetTempFileName();
            var mockIFile = new Mock<IFile>();
            mockIFile.Setup(o => o.WriteAllText(settingsPath, It.IsAny<string>())).Verifiable();

            var mockDirectory = new Mock<IDirectory>();

            var serverSettings = new ServerSettings(settingsPath, mockIFile.Object, mockDirectory.Object);

            //act
            serverSettings.SaveIfNotExists();

            //assert
            mockIFile.Verify();
            mockDirectory.Verify();
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory("Logging Paths")]
        public void ServerSettings_SaveLoggingPath()
        {
            var newAuditsFilePath = "falsepath7";

            var settingsPath = Path.GetTempFileName();

            var sourceFilePath = Config.Server.AuditFilePath;


            var mockIFile = new Mock<IFile>();
            mockIFile.Setup(o => o.Exists(It.IsAny<string>())).Returns(true).Verifiable();
            mockIFile.Setup(o => o.ReadAllText(It.IsAny<string>())).Returns("{}");
            mockIFile.Setup(c=> c.Copy(It.IsAny<string>() , It.IsAny<string>())).Verifiable();

            var mockDirectory = new Mock<IDirectory>();
            mockDirectory.Setup(d => d.CreateIfNotExists(It.IsAny<string>())).Returns(newAuditsFilePath).Verifiable();

            var serverSettings = new ServerSettings(settingsPath, mockIFile.Object, mockDirectory.Object);

            //act
            var actual = serverSettings.SaveLoggingPath(sourceFilePath);
            Assert.IsFalse(actual);

            actual = serverSettings.SaveLoggingPath(newAuditsFilePath);
            //assert
            Assert.IsTrue(actual);
            mockIFile.Verify();
            mockDirectory.Verify();
            mockIFile.Verify(o => o.WriteAllText(settingsPath, It.IsAny<string>()), Times.Once);
        }
    }
}
