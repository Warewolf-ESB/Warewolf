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
using System;
using System.IO;
using Warewolf.Configuration;

namespace Dev2.Common.Tests
{
    [TestClass]
    public class ClusterSettingsTests
    {
        [TestMethod]
        public void ClusterSettingsData_Equals_Valid_Expected()
        {
            var expectedClusterSettingsData = new ClusterSettingsData
            {
                Key = "asdfasdfsadf",
            };

            var serverSettingsData = new ClusterSettingsData
            {
                Key = "asdfasdfsadf",
            };

            Assert.IsTrue(serverSettingsData.Equals(expectedClusterSettingsData));
        }

        [TestMethod]
        public void Get_AppConfig_Configuration()
        {
            string expectedClusterKey = Guid.NewGuid().ToString(); // TODO: this should be more than just a guid. something like key+servername

            var mockFileWrapper = new Mock<IFile>();
            var mockDirectoryWrapper = new Mock<IDirectory>();

            var settings = new ClusterSettings("", mockFileWrapper.Object, mockDirectoryWrapper.Object);
            Assert.AreEqual(8, settings.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).Length);

            Assert.AreEqual(expectedClusterKey, settings.Key);
        }

        [TestMethod]
        public void GetClusterSettings_Constants()
        {
            Assert.AreEqual(@"C:\ProgramData\Warewolf\Server Settings\clusterSettings.json", ClusterSettings.SettingsPath);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("Logging Paths")]
        public void ClusterSettings_SaveIfNotExists()
        {
            var mockIFile = new Mock<IFile>();
            mockIFile.Setup(o => o.Exists(It.IsAny<string>())).Returns(false).Verifiable();
            mockIFile.Setup(o => o.WriteAllText(ClusterSettings.SettingsPath, It.IsAny<string>()));
            var mockDirectory = new Mock<IDirectory>();
            mockDirectory.Setup(o => o.CreateIfNotExists(Path.GetDirectoryName(ClusterSettings.SettingsPath))).Returns(ClusterSettings.SettingsPath);

            //act
            var serverSettings = new ClusterSettings("some path", mockIFile.Object, mockDirectory.Object);
            serverSettings.SaveIfNotExists();

            //assert
            mockIFile.Verify();
            mockDirectory.Verify();
        }
    }
}
