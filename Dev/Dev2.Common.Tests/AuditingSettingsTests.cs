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
    public class AuditingSettingsTests
    {
        [TestMethod]
        [TestCategory(nameof(AuditingSettingsData))]
        public void AuditingSettingsData_Constants()
        {
            Assert.AreEqual(@"ws://127.0.0.1:5000/ws", AuditingSettings.DefaultEndpoint);
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(AuditingSettings))]
        public void AuditingSettingsData_Equals_Valid_Expected()
        {
            var expectedAuditingSettingsData = new AuditingSettingsData
            {
                Endpoint = "ws://127.0.0.1:5000/ws",
                LoggingDataSource = null
            };

            var auditingSettingsData = new AuditingSettingsData()
            {
                Endpoint = "ws://127.0.0.1:5000/ws",
                LoggingDataSource = null
            };

            Assert.IsTrue(auditingSettingsData.Equals(expectedAuditingSettingsData));
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(AuditingSettings))]
        public void AuditingSettings_SaveIfNotExists()
        {
            var mockIFile = new Mock<IFile>();
            mockIFile.Setup(o => o.Exists(It.IsAny<string>())).Returns(false).Verifiable();
            mockIFile.Setup(o => o.WriteAllText(AuditingSettings.SettingsPath, It.IsAny<string>()));
            var mockDirectory = new Mock<IDirectory>();
            mockDirectory.Setup(o => o.CreateIfNotExists(Path.GetDirectoryName(AuditingSettings.SettingsPath))).Returns(AuditingSettings.SettingsPath);

            //act
            var auditSettings = new AuditingSettings("some path", mockIFile.Object, mockDirectory.Object);
            auditSettings.SaveIfNotExists();

            //assert
            mockIFile.Verify();
            mockDirectory.Verify();
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(AuditingSettings))]
        public void AuditingSettings_Get_Settings()
        {
            const string expectedEndpoint = @"ws://127.0.0.1:5000/ws";

            //arrange
            var mockIFile = new Mock<IFile>();
            mockIFile.Setup(o => o.Exists(It.IsAny<string>())).Returns(false);
            var mockDirectory = new Mock<IDirectory>();

            var auditSettings = new AuditingSettings("some path", mockIFile.Object, mockDirectory.Object);

            var result = auditSettings.Get();

            Assert.IsNotNull(result);
            Assert.AreEqual(expectedEndpoint, result.Endpoint);
        }
    }
}