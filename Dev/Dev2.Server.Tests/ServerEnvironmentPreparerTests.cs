/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/
using Dev2.Common;
using Dev2.Common.Interfaces.Wrappers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace Dev2.Server.Tests
{
    [TestClass]
    public class ServerEnvironmentPreparerTests
    {
        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ServerEnvironmentPreparerTests))]
        public void ServerEnvironmentPreparer_Constructor()
        {
            using (var serverEnvironmentPreparer = new ServerEnvironmentPreparer())
            {
            }
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ServerEnvironmentPreparerTests))]
        public void ServerEnvironmentPreparer_PrepareEnvironment()
        {
            var mockTempFileDeleter = new Mock<ITempFileDeleter>();
            var mockFileWrapper = new Mock<IFile>();
            var mockDirectoryWrapper = new Mock<IDirectory>();

            var expectedSettingsFile = EnvironmentVariables.ServerLogSettingsFile;
            var expectedSecurityFile = EnvironmentVariables.ServerSecuritySettingsFile;
            var expectedSettingsFolder = EnvironmentVariables.ServerSettingsFolder;

            mockFileWrapper.Setup(o => o.Exists(It.IsAny<string>())).Returns(false);
            mockFileWrapper.Setup(o => o.Exists("Settings.config")).Returns(true);
            mockFileWrapper.Setup(o => o.Exists("secure.config")).Returns(true);

            mockDirectoryWrapper.Setup(o => o.Exists(EnvironmentVariables.DebugItemTempPath)).Returns(true);

            IEnumerable<IFileInfo> infos()
            {
                var info = new Mock<IFileInfo>();
                info.Setup(o => o.CreationTime).Returns(DateTime.Now.Subtract(new TimeSpan(36, 0, 0)));
                yield return info.Object;
            };
            mockDirectoryWrapper.Setup(o => o.GetFileInfos(EnvironmentVariables.DebugItemTempPath)).Returns(infos);

            ConfigurationManager.AppSettings.Set("DaysToKeepTempFiles", "1");
            using (var serverEnvironmentPreparer = new ServerEnvironmentPreparer(mockTempFileDeleter.Object, mockFileWrapper.Object, mockDirectoryWrapper.Object))
            {
                serverEnvironmentPreparer.PrepareEnvironment();
            }

            mockFileWrapper.Verify(o => o.Copy("Settings.config", expectedSettingsFile), Times.Once);
            mockFileWrapper.Verify(o => o.Copy("secure.config", expectedSecurityFile), Times.Once);
            mockFileWrapper.Verify(o => o.WriteAllText(expectedSettingsFile, It.IsAny<string>()), Times.Once);

            mockDirectoryWrapper.Verify(o => o.CreateDirectory(expectedSettingsFolder), Times.Once);

            mockTempFileDeleter.VerifySet(o => o.DaysToKeepTempFiles = 1, Times.Once);
            mockTempFileDeleter.Verify(o => o.Start(), Times.Once);
        }
    }
}
