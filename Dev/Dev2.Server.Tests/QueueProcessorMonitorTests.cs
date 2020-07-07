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
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Warewolf.OS;
using Warewolf.Triggers;

namespace Dev2.Server.Tests
{
    [TestClass]
    public class QueueProcessorMonitorTests
    {
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(QueueWorkerMonitor))]
        public void QueueProcessorMonitor_Start_Success()
        {
            //----------------------------Arrange-----------------------------
            var mockProcessFactory = new Mock<IProcessFactory>();
            var mockQueueConfigLoader = new Mock<IQueueConfigLoader>();
            var mockProcess = new Mock<IProcess>();
            var mockTriggerCatalog = new Mock<ITriggersCatalog>();
            var mockChildProcessTracker = new Mock<IChildProcessTracker>();
            var mockTriggerObject = new Mock<ITrigger>();
            mockTriggerObject.Setup(o => o.Concurrency).Returns(1);
            mockQueueConfigLoader.Setup(o => o.Configs).Returns(new List<ITrigger> { mockTriggerObject.Object });

            var pass = true;

            mockProcessFactory.Setup(o => o.Start(It.IsAny<ProcessStartInfo>()))
                                .Callback<ProcessStartInfo>((startInfo) => { if (pass) pass = (GlobalConstants.QueueWorkerExe == startInfo.FileName); })
                                .Returns(mockProcess.Object);

            //----------------------------Act---------------------------------
            var processMonitor = new QueueWorkerMonitor(
                mockProcessFactory.Object,
                mockQueueConfigLoader.Object,
                mockTriggerCatalog.Object,
                mockChildProcessTracker.Object);

            mockProcess.SetupSequence(o => o.WaitForExit(1000))
                        .Returns(() => { Thread.Sleep(1000); return false; })
                        .Returns(false).Returns(true)
                        .Returns(() => { Thread.Sleep(1000); return false; })
                        .Returns(false).Returns(() => { processMonitor.Shutdown(); return true; });

            new Thread(() => processMonitor.Start()).Start();
            Thread.Sleep(5000);
            //----------------------------Assert------------------------------
            mockProcess.Verify(o => o.WaitForExit(1000), Times.Exactly(6));
            mockProcessFactory.Verify(o => o.Start(It.IsAny<ProcessStartInfo>()), Times.Exactly(2));

            Assert.IsTrue(pass, "Queue worker exe incorrect");
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(QueueWorkerMonitor))]
        public void QueueProcessorMonitor_DoesNotStartAsConcurrencyDefaultIsZero_Success()
        {
            //----------------------------Arrange-----------------------------
            var mockProcessFactory = new Mock<IProcessFactory>();
            var mockQueueConfigLoader = new Mock<IQueueConfigLoader>();
            var mockProcess = new Mock<IProcess>();
            var mockTriggerCatalog = new Mock<ITriggersCatalog>();
            var mockChildProcessTracker = new Mock<IChildProcessTracker>();
            var mockTriggerObject = new Mock<ITrigger>();
           
            mockQueueConfigLoader.Setup(o => o.Configs).Returns(new List<ITrigger> { mockTriggerObject.Object });

            var pass = true;

            mockProcessFactory.Setup(o => o.Start(It.IsAny<ProcessStartInfo>()))
                                .Callback<ProcessStartInfo>((startInfo) => { if (pass) pass = (GlobalConstants.QueueWorkerExe == startInfo.FileName); })
                                .Returns(mockProcess.Object);

            //----------------------------Act---------------------------------
            var processMonitor = new QueueWorkerMonitor(
                mockProcessFactory.Object,
                mockQueueConfigLoader.Object,
                mockTriggerCatalog.Object,
                mockChildProcessTracker.Object);

            mockProcess.SetupSequence(o => o.WaitForExit(1000))
                        .Returns(() => { Thread.Sleep(1000); return false; })
                        .Returns(false).Returns(true)
                        .Returns(() => { Thread.Sleep(1000); return false; })
                        .Returns(false).Returns(() => { processMonitor.Shutdown(); return true; });

            new Thread(() => processMonitor.Start()).Start();
            Thread.Sleep(5000);
            //----------------------------Assert------------------------------
            mockProcess.Verify(o => o.WaitForExit(1000), Times.Exactly(0));
            mockProcessFactory.Verify(o => o.Start(It.IsAny<ProcessStartInfo>()), Times.Exactly(0));

            Assert.IsTrue(pass, "Queue worker exe incorrect");
        }
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(QueueWorkerMonitor))]
        public void QueueProcessorMonitor_WorkerCreated()
        {
            var mockProcessFactory = new Mock<IProcessFactory>();
            var mockQueueConfigLoader = new Mock<IQueueConfigLoader>();
            var mockChildProcessTracker = new Mock<IChildProcessTracker>();

            var triggersCatalogForTesting = new TriggersCatalogForTesting();

            _ = new QueueWorkerMonitor(mockProcessFactory.Object, mockQueueConfigLoader.Object, triggersCatalogForTesting, mockChildProcessTracker.Object);

            triggersCatalogForTesting.CallOnChanged(Guid.NewGuid());
            triggersCatalogForTesting.CallOnDeleted(Guid.NewGuid());
            triggersCatalogForTesting.CallOnCreated(Guid.NewGuid());

        }
    }

    public class TriggersCatalogForTesting : ITriggersCatalog
    {
        public event TriggerChangeEvent OnChanged;
        public event TriggerChangeEvent OnDeleted;
        public event TriggerChangeEvent OnCreated;

        public void CallOnChanged(Guid guid)
        {
            OnChanged?.Invoke(guid);
        }
        public void CallOnDeleted(Guid guid)
        {
            OnDeleted?.Invoke(guid);
        }
        public void CallOnCreated(Guid guid)
        {
            OnCreated?.Invoke(guid);
        }

        public IList<ITriggerQueue> Queues { get; set; }

        public void DeleteTriggerQueue(ITriggerQueue triggerQueue)
        {

        }

        public void Load()
        {

        }

        public ITriggerQueue LoadQueueTriggerFromFile(string filename)
        {
            return null;
        }

        public void SaveTriggerQueue(ITriggerQueue triggerQueue)
        {

        }

        public string PathFromResourceId(string triggerId)
        {
            throw new NotImplementedException();
        }
    }
}
