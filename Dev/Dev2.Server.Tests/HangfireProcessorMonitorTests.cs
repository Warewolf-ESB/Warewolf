/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Diagnostics;
using System.Threading;
using Warewolf.OS;

namespace Dev2.Server.Tests
{
    [TestClass]
    [DoNotParallelize]
    public class HangfireProcessorMonitorTests
    {
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(HangfireServerMonitor))]
        public void HangfireServerMonitor_Start_Success()
        {
            //----------------------------Arrange-----------------------------
            Config.Persistence.Enable = true;
            var mockProcessFactory = new Mock<IProcessFactory>();
            var mockChildProcessTracker = new Mock<IChildProcessTracker>();
            var mockProcess = new Mock<IProcess>();
            var pass = true;
            mockProcessFactory.Setup(o => o.Start(It.IsAny<ProcessStartInfo>()))
                .Callback<ProcessStartInfo>((startInfo) =>
                {
                    if (pass) pass = (GlobalConstants.HangfireServerExe == startInfo.FileName);
                })
                .Returns(mockProcess.Object);

            //----------------------------Act---------------------------------
            var processMonitor = new HangfireServerMonitorWithRestart(
                mockChildProcessTracker.Object,
                mockProcessFactory.Object);

            new Thread(() => processMonitor.Start()).Start();
            Thread.Sleep(5000);
            //----------------------------Assert------------------------------
            mockProcessFactory.Verify(o => o.Start(It.IsAny<ProcessStartInfo>()), Times.Exactly(1));
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(HangfireServerMonitor))]
        public void HangfireServerMonitor_Start_EnableIsFalse_DoesNotStart()
        {
            //----------------------------Arrange-----------------------------
            Config.Persistence.Enable = false;
            var mockProcessFactory = new Mock<IProcessFactory>();
            var mockChildProcessTracker = new Mock<IChildProcessTracker>();
            var mockProcess = new Mock<IProcess>();
            var pass = true;
            mockProcessFactory.Setup(o => o.Start(It.IsAny<ProcessStartInfo>()))
                .Callback<ProcessStartInfo>((startInfo) =>
                {
                    if (pass) pass = (GlobalConstants.HangfireServerExe == startInfo.FileName);
                })
                .Returns(mockProcess.Object);

            //----------------------------Act---------------------------------
            var processMonitor = new HangfireServerMonitorWithRestart(
                mockChildProcessTracker.Object,
                mockProcessFactory.Object);

            new Thread(() => processMonitor.Start()).Start();
            Thread.Sleep(5000);

            //----------------------------Assert------------------------------
            mockProcessFactory.Verify(o => o.Start(It.IsAny<ProcessStartInfo>()), Times.Exactly(0));
        }
    }
}