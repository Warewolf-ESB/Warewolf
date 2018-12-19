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
            var mockLogManager = new Mock<ILogManager>();
            mockLogManager.Setup(a => a.FlushLogs()).Callback(() => Console.WriteLine("FlushLogs Invoked")).Verifiable();
            //----------------------Act-------------------
            var worker = new LogFlusherWorker(mockLogManager.Object)
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
            var mockLogManager = new Mock<ILogManager>();
            mockLogManager.Setup(a => a.FlushLogs()).Verifiable();
            //----------------------Act-------------------
            var worker = new LogFlusherWorker(mockLogManager.Object)
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
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory("LogFlusherWorker")]
        public void LogFlusherWorker_PerformLogFlushTimerResume()
        {
            //----------------------Arrange-------------------
            var mockLogManager = new Mock<ILogManager>();
            mockLogManager.Setup(a => a.FlushLogs()).Verifiable();
            //----------------------Act-------------------
            var worker = new LogFlusherWorker(mockLogManager.Object)
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
