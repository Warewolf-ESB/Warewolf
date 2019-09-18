using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Common.Wrappers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.IO;
using Warewolf.Configuration;
using Warewolf.VirtualFileSystem;

namespace Dev2.Common.Tests
{
    [TestClass]
    public class ConfigTest
    {
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

            var settings = new ServerSettings();
            Assert.AreEqual(8, settings.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).Length);

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
            Assert.AreEqual(@"C:\ProgramData\Warewolf\Audits", ServerSettings.DefaultAuditPath);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("Logging Paths")]
        public void ServerSettings_SaveIfNotExists()
        {

            var mockIFile = new Mock<IFile>();
            mockIFile.Setup(o => o.Exists(It.IsAny<string>())).Returns(false).Verifiable();
            mockIFile.Setup(o => o.WriteAllText(ServerSettings.SettingsPath, It.IsAny<string>()));
            var mockDirectory = new Mock<IDirectory>();
            mockDirectory.Setup(o => o.CreateIfNotExists(Path.GetDirectoryName(ServerSettings.SettingsPath))).Returns(ServerSettings.SettingsPath);

            //act
            var serverSettings = new ServerSettings("some path", mockIFile.Object, mockDirectory.Object);
            serverSettings.SaveIfNotExists();

            //assert
            mockIFile.Verify();
            mockDirectory.Verify();

        }/*

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory("Logging Paths")]
        public void ServerSettings_SaveLoggingPath()
        {
            var newAuditsFilePath = "falsepath7";

            mockConfigurationManager.SetupSet(o => o["AuditFilePath"] = newAuditsFilePath).Verifiable();
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
        }*/
    }
}
