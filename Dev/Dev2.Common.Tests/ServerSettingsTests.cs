/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.Wrappers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.IO;
using Warewolf.Configuration;
using Warewolf.Esb;

namespace Dev2.Common.Tests
{
    [TestClass]
    public class ServerSettingsTests
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

            var mockFileWrapper = new Mock<IFile>();
            var mockDirectoryWrapper = new Mock<IDirectory>();
            var mockClusterDispatcher = new Mock<IClusterDispatcher>();

            var settings = new ServerSettings("", mockFileWrapper.Object, mockDirectoryWrapper.Object, mockClusterDispatcher.Object);
            Assert.AreEqual(8, settings.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).Length);

            Assert.AreEqual((ushort)0, settings.WebServerPort);
            Assert.AreEqual((ushort)0, settings.WebServerSslPort);
            Assert.AreEqual(null, settings.SslCertificateName);
            Assert.AreEqual(false, settings.CollectUsageStats);
            Assert.AreEqual(0, settings.DaysToKeepTempFiles);
            Assert.AreEqual(expectedPath, settings.AuditFilePath);
            Assert.AreEqual(false, settings.EnableDetailedLogging);
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
            var mockClusterDispatcher = new Mock<IClusterDispatcher>();

            //act
            var serverSettings = new ServerSettings("some path", mockIFile.Object, mockDirectory.Object, mockClusterDispatcher.Object);
            serverSettings.SaveIfNotExists();

            //assert
            mockIFile.Verify();
            mockDirectory.Verify();
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory("Logging Paths")]
        public void ServerSettings_SaveLoggingPath_Exists()
        {
            var newAuditsFilePath = "falsepath7";

            //arrange
            ServerSettings serverSettings;

            var sourceFilePath = Config.Server.AuditFilePath;

            var mockIFile = new Mock<IFile>();
            mockIFile.Setup(o => o.Exists(It.IsAny<string>())).Returns(true).Verifiable();
            mockIFile.Setup(c => c.Copy(It.IsAny<string>(), It.IsAny<string>())).Verifiable();
            mockIFile.Setup(o => o.ReadAllText("some path")).Returns("{}");
            var mockDirectory = new Mock<IDirectory>();
            mockDirectory.Setup(d => d.CreateIfNotExists(It.IsAny<string>())).Returns(newAuditsFilePath).Verifiable();
            var mockClusterDispatcher = new Mock<IClusterDispatcher>();

            serverSettings = new ServerSettings("some path", mockIFile.Object, mockDirectory.Object, mockClusterDispatcher.Object);

            //act
            var actual = serverSettings.SaveLoggingPath(sourceFilePath);
            Assert.IsFalse(actual);

            actual = serverSettings.SaveLoggingPath(newAuditsFilePath);
            //assert
            Assert.IsTrue(actual);
            mockIFile.Verify();
            mockDirectory.Verify();
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("Logging Paths")]
        public void ServerSettings_SaveLoggingPath_DoesNot_Exist()
        {
            var newAuditsFilePath = "falsepath7";

            //arrange
            var sourceFilePath = Config.Server.AuditFilePath;

            var mockIFile = new Mock<IFile>();
            mockIFile.Setup(o => o.Exists(It.IsAny<string>())).Returns(false);
            var mockDirectory = new Mock<IDirectory>();
            var mockClusterDispatcher = new Mock<IClusterDispatcher>();

            var serverSettings = new ServerSettings("some path", mockIFile.Object, mockDirectory.Object, mockClusterDispatcher.Object);

            //act
            var actual = serverSettings.SaveLoggingPath(sourceFilePath);
            Assert.IsFalse(actual);

            actual = serverSettings.SaveLoggingPath(newAuditsFilePath);
            //assert
            Assert.IsFalse(actual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("Logging Paths")]
        public void ServerSettings_SaveLoggingPath_Get()
        {
            //arrange
            var mockIFile = new Mock<IFile>();
            mockIFile.Setup(o => o.Exists(It.IsAny<string>())).Returns(false);
            var mockDirectory = new Mock<IDirectory>();
            var mockClusterDispatcher = new Mock<IClusterDispatcher>();

            var serverSettings = new ServerSettings("some path", mockIFile.Object, mockDirectory.Object, mockClusterDispatcher.Object);

            var result = serverSettings.Get();

            Assert.IsNotNull(result);
            Assert.AreEqual("C:\\ProgramData\\Warewolf\\Audits", result.AuditFilePath);
            Assert.IsFalse(result.CollectUsageStats.Value);
            Assert.AreEqual(0, result.DaysToKeepTempFiles);
            Assert.IsFalse(result.EnableDetailedLogging.Value);
            Assert.AreEqual(200, result.LogFlushInterval);
            Assert.IsNull(result.SslCertificateName);
            Assert.AreEqual(0, result.WebServerPort.Value);
            Assert.AreEqual(0, result.WebServerSslPort.Value);
        }
    }
}
