
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading;

namespace Dev2.Server.Tests
{
    [TestClass]
    public class LogFlusherWorkerTests
    {
        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory("LogFlusherWorker")]
        public void LogFlusherWorker_Execute()
        {
            //----------------------Arrange-------------------
            var mockWriter = new Mock<IWriter>();
            var mockLogManager = new Mock<ILogManager>();
            mockLogManager.Setup(a => a.FlushLogs()).Callback(() => Console.WriteLine("FlushLogs Invoked")).Verifiable();
            //----------------------Act-------------------
            var worker = new LogFlusherWorker(mockLogManager.Object, mockWriter.Object)
            {
                TimerDueTime = 200
            };
            using (worker)
            {
                worker.Execute();
                Thread.Sleep(500);
            }
            //----------------------Assert-------------------
            mockLogManager.Verify();
            
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory("LogFlusherWorker")]
        public void LogFlusherWorker_PerformLogFlushTimerPause()
        {
            //----------------------Arrange-------------------
            var mockWriter = new Mock<IWriter>();
            var mockLogManager = new Mock<ILogManager>();
            mockLogManager.Setup(a => a.FlushLogs()).Verifiable();
            //----------------------Act-------------------
            var worker = new LogFlusherWorker(mockLogManager.Object, mockWriter.Object)
            {
                TimerDueTime = 200
            };
            using (worker)
            {
                worker.Execute();
                Thread.Sleep(220);
                worker.PerformLogFlushTimerPause();
                mockLogManager.Verify(a => a.FlushLogs(), Times.Once);

                Thread.Sleep(300);
            }
            //----------------------Assert-------------------
            mockLogManager.Verify(a => a.FlushLogs(), Times.Once);
            mockWriter.Verify(o => o.Write("Detailed Logging Enabled\n"), Times.Once);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory("LogFlusherWorker")]
        public void LogFlusherWorker_PerformLogFlushTimerResume()
        {
            //----------------------Arrange-------------------
            var mockWriter = new Mock<IWriter>();
            var mockLogManager = new Mock<ILogManager>();
            mockLogManager.Setup(a => a.FlushLogs()).Verifiable();
            //----------------------Act-------------------
            var worker = new LogFlusherWorker(mockLogManager.Object, mockWriter.Object)
            {
                TimerDueTime = 200
            };
            using (worker)
            {
                worker.Execute();
                Thread.Sleep(220);
                worker.PerformLogFlushTimerPause();
                mockLogManager.Verify(a => a.FlushLogs(), Times.Once);

                Thread.Sleep(300);
                //----------------------Assert-------------------
                mockLogManager.Verify(a => a.FlushLogs(), Times.Once);
                worker.PerformLogFlushTimerResume();
                Thread.Sleep(500);
                mockLogManager.Verify(a => a.FlushLogs(), Times.AtLeast(3));

            }

        }
    }
}
