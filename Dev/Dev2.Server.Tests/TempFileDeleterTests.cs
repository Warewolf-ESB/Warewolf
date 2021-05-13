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
using System.Threading;

namespace Dev2.Server.Tests
{
    [TestClass]
    public class TempFileDeleterTests
    {
        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(TempFileDeleterTests))]
        public void TempFileDeleter_Constructor()
        {
            var mockDirectoryWrapper = new Mock<IDirectory>();
            var mockTimerFactory = new Mock<ITimerFactory>();
            using (var serverEnvironmentPreparer = new TempFileDeleter(mockDirectoryWrapper.Object, mockTimerFactory.Object))
            {
            }
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(TempFileDeleterTests))]
        public void TempFileDeleter_Delete_Success()
        {
            bool wasCalled = false;
            var expectedDueTime = 1000;
            var expectedPeriod = GlobalConstants.NetworkComputerNameQueryFreq;

            var mockTimerFactory = new Mock<ITimerFactory>();
            var mockTimer = new Mock<ITimer>();
            mockTimerFactory.Setup(o => o.New(It.IsAny<TimerCallback>(), null, expectedDueTime, expectedPeriod))
                .Returns(mockTimer.Object)
                .Callback<TimerCallback, object, int, int>((callback, state, dueTime, interval) => {
                    callback?.Invoke(null);
                    wasCalled = true;
                });

            var mockFileWrapper = new Mock<IFile>();
            var mockDirectoryWrapper = new Mock<IDirectory>();

            mockFileWrapper.Setup(o => o.Exists(It.IsAny<string>())).Returns(false);
            mockFileWrapper.Setup(o => o.Exists("Settings.config")).Returns(true);
            mockFileWrapper.Setup(o => o.Exists("secure.config")).Returns(true);

            mockDirectoryWrapper.Setup(o => o.Exists("C:\\ProgramData\\Warewolf\\Temp\\Warewolf\\Debug")).Returns(true);

            var mockFileInfo_getsDeleted = new Mock<IFileInfo>();
            var mockFileInfo_neverGetsDeleted = new Mock<IFileInfo>();
            IEnumerable<IFileInfo> infos()
            {
                mockFileInfo_getsDeleted.Setup(o => o.CreationTime).Returns(DateTime.Now.Subtract(new TimeSpan(72, 0, 0)));
                yield return mockFileInfo_getsDeleted.Object;

                mockFileInfo_neverGetsDeleted.Setup(o => o.CreationTime).Returns(DateTime.Now.Subtract(new TimeSpan(36, 0, 0)));
                yield return mockFileInfo_neverGetsDeleted.Object;
            }

            mockDirectoryWrapper.Setup(o => o.GetFileInfos("C:\\ProgramData\\Warewolf\\Temp\\Warewolf\\Debug")).Returns(infos);

            ConfigurationManager.AppSettings.Set("DaysToKeepTempFiles", "1");
            using (var tempFileDeleter = new TempFileDeleter(mockDirectoryWrapper.Object, mockTimerFactory.Object))
            {
                tempFileDeleter.DaysToKeepTempFiles = 2;
                tempFileDeleter.Start();
            }

            Assert.IsTrue(wasCalled, "expect timercallback to be called");

            mockTimerFactory.Verify(o => o.New(It.IsAny<TimerCallback>(), null, expectedDueTime, expectedPeriod), Times.Once);
            mockDirectoryWrapper.Verify(o => o.Exists("C:\\ProgramData\\Warewolf\\Temp\\Warewolf\\Debug"), Times.Once);
            mockDirectoryWrapper.Verify(o => o.GetFileInfos("C:\\ProgramData\\Warewolf\\Temp\\Warewolf\\Debug"), Times.Once);

            mockDirectoryWrapper.Verify(o => o.Exists("C:\\ProgramData\\Warewolf\\DebugOutPut\\"), Times.Once);
            mockDirectoryWrapper.Verify(o => o.GetFileInfos("C:\\ProgramData\\Warewolf\\DebugOutPut\\"), Times.Never);

            mockFileInfo_getsDeleted.Verify(o => o.Delete(), Times.Once);
            mockFileInfo_neverGetsDeleted.Verify(o => o.Delete(), Times.Never);
        }
    }
}
