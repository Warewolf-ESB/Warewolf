/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

using ProcessStartInfo = System.Diagnostics.ProcessStartInfo;

namespace Warewolf.OS.Tests
{
    [TestClass]
    public class ProcessThreadTests
    {
        readonly ProcessStartInfo _startInfo = new ProcessStartInfo()
        {
            FileName = "Bob.exe",
        };

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ProcessMonitor))]
        public void ProcessThread_Constructor()
        {
            var mockConfig = new Mock<IJobConfig>();
            var mockChildProcessTracker = new Mock<IChildProcessTracker>();
            var mockProcessFactory = new Mock<IProcessFactory>();
            var expectedConfig = mockConfig.Object;
            var processThread = new ProcessThreadForTesting(_startInfo, mockChildProcessTracker.Object, mockProcessFactory.Object, expectedConfig);

            Assert.AreEqual(mockConfig.Object, processThread.Config);
            Assert.IsFalse(processThread.IsAlive);
            Assert.AreEqual(0, processThread.Pid);
        }
        
        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ProcessMonitor))]
        [Timeout(30000)]
        public void ProcessThread_Start_GivenValid_ExpectNewProcessCreated()
        {
            var mockConfig = new Mock<IJobConfig>();
            var mockChildProcessTracker = new Mock<IChildProcessTracker>();
            var mockProcessFactory = new Mock<IProcessFactory>();
            var mockProcess = new Mock<IProcess>();
            mockProcess.Setup(o => o.WaitForExit(It.IsAny<int>())).Returns(true);
            var process = mockProcess.Object;
            mockProcessFactory.Setup(o => o.Start(_startInfo)).Returns(process);
            var expectedConfig = mockConfig.Object;


            var processThread = new ProcessThreadForTesting(_startInfo, mockChildProcessTracker.Object, mockProcessFactory.Object, expectedConfig);
            var done = false;
            processThread.OnProcessDied += (config) => done = true;

            processThread.Start();


            while (!done) { }

            mockChildProcessTracker.Verify(o => o.Add(process), Times.Once);
            mockProcessFactory.Verify(o => o.Start(_startInfo), Times.Once); // also need to verify a restart
            mockProcess.Verify(o => o.WaitForExit(It.IsAny<int>()));
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ProcessMonitor))]
        public void ProcessThread_Kill_GivenUnstartedProcess_DoNotThrow()
        {
            var mockConfig = new Mock<IJobConfig>();
            var mockChildProcessTracker = new Mock<IChildProcessTracker>();
            var mockProcessFactory = new Mock<IProcessFactory>();
            var mockProcess = new Mock<IProcess>();
            mockProcess.Setup(o => o.WaitForExit(It.IsAny<int>())).Returns(true);
            var process = mockProcess.Object;
            mockProcessFactory.Setup(o => o.Start(_startInfo)).Returns(process);
            var expectedConfig = mockConfig.Object;
            var processThread = new ProcessThreadForTesting(_startInfo, mockChildProcessTracker.Object, mockProcessFactory.Object, expectedConfig);

            try
            {
                processThread.Kill();
            }
            catch
            {
                Assert.Fail("kill throws exception when it should not");
            }
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ProcessMonitor))]
        [Timeout(30000)]
        public void ProcessThread_Kill_GivenDeadProcess_DoNotThrow()
        {
            var mockConfig = new Mock<IJobConfig>();
            var mockChildProcessTracker = new Mock<IChildProcessTracker>();
            var mockProcessFactory = new Mock<IProcessFactory>();
            var mockProcess = new Mock<IProcess>();
            mockProcess.Setup(o => o.WaitForExit(It.IsAny<int>())).Returns(true);
            var process = mockProcess.Object;
            mockProcessFactory.Setup(o => o.Start(_startInfo)).Returns(process);
            var expectedConfig = mockConfig.Object;
            var processThread = new ProcessThreadForTesting(_startInfo, mockChildProcessTracker.Object, mockProcessFactory.Object, expectedConfig);
            var done = false;
            processThread.OnProcessDied += (config) => done = true;

            processThread.Start();
            
            while (!done) { }

            try
            {
                processThread.Kill();
            }
            catch
            {
                Assert.Fail("kill throws exception when it should not");
            }
        }
    }

    internal class ProcessThreadForTesting : ProcessMonitor
    {
        private readonly ProcessStartInfo _testProcessInfo;

        public ProcessThreadForTesting(ProcessStartInfo processStartInfo, IChildProcessTracker childProcessTracker, IProcessFactory processFactory, IJobConfig config)
            : base(childProcessTracker, processFactory, config)
        {
            _testProcessInfo = processStartInfo;
        }

        protected override ProcessStartInfo GetProcessStartInfo() => _testProcessInfo;
    }
}
