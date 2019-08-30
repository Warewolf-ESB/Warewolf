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
using Dev2.Common.Wrappers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Dev2.Server.Tests
{
    [TestClass]
    public class QueueProcessorMonitorTests
    {
        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(QueueWorkerMonitor))]
        public void QueueProcessorMonitor_Start_Success()
        {
            //----------------------------Arrange-----------------------------
            var mockProcessFactory = new Mock<IProcessFactory>();
            var mockQueueConfigLoader = new Mock<IQueueConfigLoader>();
            var mockProcess = new Mock<IProcess>();

            mockQueueConfigLoader.Setup(o => o.Configs).Returns(new List<string> { "test config string1" });

            var pass = true;

            mockProcessFactory.Setup(o => o.Start(It.IsAny<ProcessStartInfo>()))
                                .Callback<ProcessStartInfo>((startInfo)=> { if (pass) pass = (GlobalConstants.QueueWorkerExe == startInfo.FileName); })
                                .Returns(mockProcess.Object);

            var worker = GlobalConstants.QueueWorkerExe;

            //----------------------------Act---------------------------------
            var processMonitor = new QueueWorkerMonitor(mockProcessFactory.Object, mockQueueConfigLoader.Object, new Mock<IWriter>().Object);

            mockProcess.SetupSequence(o => o.WaitForExit(1000))
                        .Returns(()=> { Thread.Sleep(1000); return false; }).Returns(false).Returns(true)
                        .Returns(()=> { Thread.Sleep(1000); return false; }).Returns(false).Returns(()=> { processMonitor.Stop(); return true; });

            new Thread(()=> processMonitor.Start()).Start();
            Thread.Sleep(5000);
            //----------------------------Assert------------------------------
            mockProcess.Verify(o => o.WaitForExit(1000), Times.Exactly(6));
            mockProcessFactory.Verify(o => o.Start(It.IsAny<ProcessStartInfo>()), Times.Exactly(2));

            Assert.IsTrue(pass, "Queue worker exe incorrect");
        }
    }
}
