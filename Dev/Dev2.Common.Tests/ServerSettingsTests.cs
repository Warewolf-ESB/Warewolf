/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Common.Interfaces.Enums;
using Warewolf.Configuration;

namespace Dev2.Common.Tests
{
    [TestClass]
    public class ServerSettingsTests
    {
        [TestMethod]
        [TestCategory(nameof(ServerSettings))]
        public void AuditingSettingsData_Constants()
        {
            Assert.AreEqual(nameof(LegacySettingsData), ServerSettings.DefaultSink);
        }
        [TestMethod]
        [TestCategory(nameof(ServerSettings))]
        public void ServerSettingsData_Equals_Valid_Expected()
        {
            var expectedServerSettingsData = new ServerSettingsData
            {
                WebServerPort = 3142,
                WebServerSslPort = 3143,
                SslCertificateName = "SslCertificateName",
                CollectUsageStats = true,
                DaysToKeepTempFiles = 2,
                AuditFilePath = "some path",
                Sink = nameof(LegacySettingsData)
            };

            var serverSettingsData = new ServerSettingsData
            {
                WebServerPort = 3142,
                WebServerSslPort = 3143,
                SslCertificateName = "SslCertificateName",
                CollectUsageStats = true,
                DaysToKeepTempFiles = 2,
                AuditFilePath = "some path",
                Sink = nameof(LegacySettingsData)
            };

            Assert.IsTrue(serverSettingsData.Equals(expectedServerSettingsData));
        }

        [TestMethod]
        [TestCategory(nameof(ServerSettings))]
        public void ServerSettingsData_Get_AppConfig_Configuration()
        {
            var mockFileWrapper = new Mock<IFile>();
            var mockDirectoryWrapper = new Mock<IDirectory>();

            var settings = new ServerSettings("", mockFileWrapper.Object, mockDirectoryWrapper.Object);
            Assert.AreEqual(9, settings.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).Length);

            Assert.AreEqual((ushort) 0, settings.WebServerPort);
            Assert.AreEqual((ushort) 0, settings.WebServerSslPort);
            Assert.AreEqual(null, settings.SslCertificateName);
            Assert.AreEqual(false, settings.CollectUsageStats);
            Assert.AreEqual(0, settings.DaysToKeepTempFiles);
            Assert.AreEqual(false, settings.EnableDetailedLogging);
            Assert.AreEqual(200, settings.LogFlushInterval);
            Assert.AreEqual("C:\\ProgramData\\Warewolf\\Audits", settings.AuditFilePath);
            Assert.AreEqual(nameof(LegacySettingsData), settings.Sink);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ServerSettings))]
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
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ServerSettings))]
        public void ServerSettingsData_SinkNotInFile_AuditFilePathIsNotNull_SetSinkEqualLegacySettingsData()
        {
            var serverSettings = Config.Server.Get();
            Assert.AreEqual(nameof(LegacySettingsData), serverSettings.Sink);
            Assert.AreEqual(LegacySettings.DefaultAuditPath, serverSettings.AuditFilePath);
        }
    }
}