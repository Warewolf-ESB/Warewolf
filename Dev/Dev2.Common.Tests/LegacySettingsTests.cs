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
    public class LegacySettingsTests
    {
        [TestMethod]
        [TestCategory(nameof(LegacySettings))]
        public void LegacySettingsData_Constants()
        {
            Assert.AreEqual(@"C:\ProgramData\Warewolf\Audits", LegacySettings.DefaultAuditPath);
            Assert.AreEqual(@"ws://127.0.0.1:5000/ws", LegacySettings.DefaultEndpoint);
        }
       
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(LegacySettings))]
        public void LegacySettingsData_Equals_Valid_Expected()
        {
            var expectedSettingsData = new LegacySettingsData
            {
                Endpoint = "ws://127.0.0.1:5000/ws",
                AuditFilePath = @"C:\ProgramData\Warewolf\Audits"
            };

            var settingsData = new LegacySettingsData()
            {
                Endpoint = "ws://127.0.0.1:5000/ws",
                AuditFilePath = @"C:\ProgramData\Warewolf\Audits"
            };

            Assert.IsTrue(settingsData.Equals(expectedSettingsData));
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(LegacySettings))]
        public void LegacySettings_SaveIfNotExists()
        {
            var mockIFile = new Mock<IFile>();
            mockIFile.Setup(o => o.Exists(It.IsAny<string>())).Returns(false).Verifiable();
            mockIFile.Setup(o => o.WriteAllText(LegacySettings.SettingsPath, It.IsAny<string>()));
            var mockDirectory = new Mock<IDirectory>();
            mockDirectory.Setup(o => o.CreateIfNotExists(Path.GetDirectoryName(LegacySettings.SettingsPath))).Returns(LegacySettings.SettingsPath);

            //act
            var auditSettings = new LegacySettings("some path", mockIFile.Object, mockDirectory.Object);
            auditSettings.SaveIfNotExists();

            //assert
            mockIFile.Verify();
            mockDirectory.Verify();
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(LegacySettings))]
        public void LegacySettings_Get_Settings()
        {
            const string expectedEndpoint = @"ws://127.0.0.1:5000/ws";
            const string expectedPath = @"C:\ProgramData\Warewolf\Audits";

            //arrange
            var mockIFile = new Mock<IFile>();
            mockIFile.Setup(o => o.Exists(It.IsAny<string>())).Returns(false);
            var mockDirectory = new Mock<IDirectory>();

            var auditSettings = new LegacySettings("some path", mockIFile.Object, mockDirectory.Object);

            var result = auditSettings.Get();

            Assert.IsNotNull(result);
            Assert.AreEqual(expectedEndpoint, result.Endpoint);
            Assert.AreEqual(expectedPath, result.AuditFilePath);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(LegacySettings))]
        public void LegacySettings_SQlite_SaveLoggingPath_Exists()
        {
            var newAuditsFilePath = "falsepath7";

            //arrange
            var sourceFilePath = Config.Legacy.AuditFilePath;

            var mockIFile = new Mock<IFile>();
            mockIFile.Setup(o => o.Exists(It.IsAny<string>())).Returns(true).Verifiable();
            mockIFile.Setup(c => c.Copy(It.IsAny<string>(), It.IsAny<string>())).Verifiable();
            mockIFile.Setup(o => o.ReadAllText("some path")).Returns("{}");
            var mockDirectory = new Mock<IDirectory>();
            mockDirectory.Setup(d => d.CreateIfNotExists(It.IsAny<string>())).Returns(newAuditsFilePath).Verifiable();

            var auditSettings = new LegacySettings("some path", mockIFile.Object, mockDirectory.Object);

            auditSettings.AuditFilePath = "Audits File Path";
            //act
            var actual = auditSettings.SaveLoggingPath(sourceFilePath);
            Assert.IsTrue(actual);
            actual = auditSettings.SaveLoggingPath(newAuditsFilePath);
            Assert.IsTrue(actual);

            mockIFile.Verify();
            mockDirectory.Verify();
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(LegacySettings))]
        public void LegacySettings_SaveSqlite_LoggingPath_DoesNot_Exist()
        {
            var newAuditsFilePath = "falsepath7";

            //arrange
            var sourceFilePath = Config.Legacy.AuditFilePath;

            var mockIFile = new Mock<IFile>();
            mockIFile.Setup(o => o.Exists(It.IsAny<string>())).Returns(false);
            var mockDirectory = new Mock<IDirectory>();

            var auditingSettings = new LegacySettings("some path", mockIFile.Object, mockDirectory.Object);

            //act
            var actual = auditingSettings.SaveLoggingPath(sourceFilePath);
            Assert.IsFalse(actual);

            actual = auditingSettings.SaveLoggingPath(newAuditsFilePath);
            //assert
            Assert.IsFalse(actual);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(LegacySettings))]
        public void LegacySettings_SQlite_AuditFilePath_Get()
        {
            //arrange
            var mockIFile = new Mock<IFile>();
            mockIFile.Setup(o => o.Exists(It.IsAny<string>())).Returns(false);
            var mockDirectory = new Mock<IDirectory>();

            var auditingSettings = new LegacySettings("some path", mockIFile.Object, mockDirectory.Object);

            var result = auditingSettings.Get();

            Assert.IsNotNull(result);
            Assert.AreEqual("C:\\ProgramData\\Warewolf\\Audits", result.AuditFilePath);
        }
    }
}