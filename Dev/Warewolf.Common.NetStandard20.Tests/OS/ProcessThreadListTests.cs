/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.OS;

namespace Warewolf.OS.Tests
{
    [TestClass]
    public class ProcessThreadListTests
    {
        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(MessageToInputsMapper))]
        public void ProcessThreadList_GivenConfig_ExpectNeedsUpdate()
        {
            var mockConfig = new Mock<IJobConfig>();
            var expectedConfig = mockConfig.Object;
            var list = new ProcessThreadListForTesting(expectedConfig, null);

            Assert.AreEqual(expectedConfig, list.Config);
            Assert.IsTrue(list.NeedUpdate);

            var newConfig = new Mock<IJobConfig>().Object;
            list.UpdateConfig(newConfig);
            Assert.AreEqual(newConfig, list.Config);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(MessageToInputsMapper))]
        public void ProcessThreadList_GivenConfigHighConcurrency_ExpectLogicalProcessorCount()
        {
            var mockConfig = new Mock<IJobConfig>();
            var mockProcessFactory = new Mock<ITestProcessFactory>();
            mockProcessFactory.Setup(o => o.New()).Returns(new Mock<IProcessThread>().Object);
            mockConfig.Setup(o => o.Concurrency).Returns(102);
            var expectedConfig = mockConfig.Object;
            var list = new ProcessThreadListForTesting(expectedConfig, mockProcessFactory.Object);

            Assert.AreEqual(expectedConfig, list.Config);
            Assert.IsTrue(list.NeedUpdate);

            list.Monitor();
            mockProcessFactory.Verify(o => o.New(), Times.Exactly(Environment.ProcessorCount));
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(MessageToInputsMapper))]
        public void ProcessThreadList_GivenDefaultConfig_ExpectNoWorker()
        {
            var mockConfig = new Mock<IJobConfig>();
            var mockProcessFactory = new Mock<ITestProcessFactory>();

            var mockProcessThread = CreateMockProcessThread();

            var processThread = mockProcessThread.Object;
            mockProcessFactory.Setup(o => o.New()).Returns(mockProcessThread.Object);

            var expectedConfig = mockConfig.Object;
            var list = new ProcessThreadListForTesting(expectedConfig, mockProcessFactory.Object);

            list.Monitor();

            Assert.IsTrue(list.NeedUpdate);
            Assert.IsFalse(processThread.IsAlive);
            mockProcessThread.Verify(o => o.Start(), Times.Never);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(MessageToInputsMapper))]
        public void ProcessThreadList_GivenWorkerDies_ExpectOnProcessDiedCalled()
        {
            var mockConfig = new Mock<IJobConfig>();
            var mockProcessFactory = new Mock<ITestProcessFactory>();
            var process = new ProcessThreadForTesting(mockConfig.Object);
            mockProcessFactory.Setup(o => o.New()).Returns(process);
            mockConfig.Setup(o => o.Concurrency).Returns(102);
            var expectedConfig = mockConfig.Object;
            var list = new ProcessThreadListForTesting(expectedConfig, mockProcessFactory.Object);
            var lists_OnProcessDiedCalled = false;
            list.OnProcessDied += (config) => lists_OnProcessDiedCalled = true;

            Assert.AreEqual(expectedConfig, list.Config);
            Assert.IsTrue(list.NeedUpdate);

            list.Monitor();
            mockProcessFactory.Verify(o => o.New(), Times.Exactly(Environment.ProcessorCount));

            process.ForceProcessDiedEvent();

            Assert.IsTrue(lists_OnProcessDiedCalled);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(MessageToInputsMapper))]
        public void ProcessThreadList_GivenConfigConcurrent3_ExpectThreeWorkers()
        {
            var mockConfig = new Mock<IJobConfig>();
            mockConfig.Setup(o => o.Concurrency).Returns(3);
            var mockProcessFactory = new Mock<ITestProcessFactory>();

            var mockProcessThread1 = CreateMockProcessThread();
            var mockProcessThread2 = CreateMockProcessThread();
            var mockProcessThread3 = CreateMockProcessThread();

            var processThread1 = mockProcessThread1.Object;
            var processThread2 = mockProcessThread2.Object;
            var processThread3 = mockProcessThread3.Object;
            mockProcessFactory.SetupSequence(o => o.New())
                .Returns(mockProcessThread1.Object)
                .Returns(mockProcessThread2.Object)
                .Returns(mockProcessThread3.Object);

            var expectedConfig = mockConfig.Object;
            var list = new ProcessThreadListForTesting(expectedConfig, mockProcessFactory.Object);

            list.Monitor();

            Assert.IsFalse(list.NeedUpdate);
            Assert.IsTrue(processThread1.IsAlive);
            Assert.IsTrue(processThread2.IsAlive);
            Assert.IsTrue(processThread3.IsAlive);
            mockProcessThread1.Verify(o => o.Start(), Times.Once);
            mockProcessThread2.Verify(o => o.Start(), Times.Once);
            mockProcessThread3.Verify(o => o.Start(), Times.Once);

            foreach (var process in list)
            {
                Assert.IsTrue(process.IsAlive);
            }
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(MessageToInputsMapper))]
        public void ProcessThreadList_GivenKillCalled_ExpectAllWorkersKilled()
        {
            var mockConfig = new Mock<IJobConfig>();
            mockConfig.Setup(o => o.Concurrency).Returns(3);
            var mockProcessFactory = new Mock<ITestProcessFactory>();

            var mockProcessThread1 = CreateMockProcessThread();
            var mockProcessThread2 = CreateMockProcessThread();
            var mockProcessThread3 = CreateMockProcessThread();

            var processThread1 = mockProcessThread1.Object;
            var processThread2 = mockProcessThread2.Object;
            var processThread3 = mockProcessThread3.Object;
            mockProcessFactory.SetupSequence(o => o.New())
                .Returns(mockProcessThread1.Object)
                .Returns(mockProcessThread2.Object)
                .Returns(mockProcessThread3.Object);

            var expectedConfig = mockConfig.Object;
            var list = new ProcessThreadListForTesting(expectedConfig, mockProcessFactory.Object);

            list.Monitor();

            Assert.IsFalse(list.NeedUpdate);
            Assert.IsTrue(processThread1.IsAlive);
            Assert.IsTrue(processThread2.IsAlive);
            Assert.IsTrue(processThread3.IsAlive);
            mockProcessThread1.Verify(o => o.Start(), Times.Once);
            mockProcessThread2.Verify(o => o.Start(), Times.Once);
            mockProcessThread3.Verify(o => o.Start(), Times.Once);

            list.Kill();

            mockProcessThread1.Verify(o => o.Kill(), Times.Once);
            mockProcessThread2.Verify(o => o.Kill(), Times.Once);
            mockProcessThread3.Verify(o => o.Kill(), Times.Once);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(MessageToInputsMapper))]
        public void ProcessThreadList_GivenInvalidConfig_ExpectThreadListStopMonitoring()
        {
            var mockConfig = new Mock<IJobConfig>();
            mockConfig.Setup(o => o.Concurrency).Returns(3);
            var mockProcessFactory = new Mock<ITestProcessFactory>();

            var mockProcessThread1 = CreateMockProcessThread();
            var mockProcessThread2 = CreateMockProcessThread();
            var mockProcessThread3 = CreateMockProcessThread();

            var processThread1 = mockProcessThread1.Object;
            var processThread2 = mockProcessThread2.Object;
            var processThread3 = mockProcessThread3.Object;
            mockProcessFactory.SetupSequence(o => o.New())
                .Returns(mockProcessThread1.Object)
                .Returns(mockProcessThread2.Object)
                .Returns(mockProcessThread3.Object);

            var expectedConfig = mockConfig.Object;
            var list = new ProcessThreadListForTesting(expectedConfig, mockProcessFactory.Object);

            list.UpdateConfig(null);
            list.Monitor();

            Assert.IsTrue(list.NeedUpdate);
            Assert.IsFalse(processThread1.IsAlive);
            Assert.IsFalse (processThread2.IsAlive);
            Assert.IsFalse(processThread3.IsAlive);
            mockProcessThread1.Verify(o => o.Start(), Times.Never);
            mockProcessThread2.Verify(o => o.Start(), Times.Never);
            mockProcessThread3.Verify(o => o.Start(), Times.Never);
        }

        private static Mock<IProcessThread> CreateMockProcessThread()
        {
            var mockProcessThread = new Mock<IProcessThread>();
            var startCalled = false;
            mockProcessThread.Setup(o => o.IsAlive).Returns(() => startCalled);
            mockProcessThread.Setup(o => o.Start()).Callback(() => startCalled = true);
            return mockProcessThread;
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(MessageToInputsMapper))]
        public void ProcessThreadList_GivenConfigConcurrentChanges_ExpectWorkerCountChanges()
        {
            var mockConfig = new Mock<IJobConfig>();
            mockConfig.Setup(o => o.Concurrency).Returns(3);
            var mockProcessFactory = new Mock<ITestProcessFactory>();

            var mockProcessThread1 = CreateMockProcessThread();
            var mockProcessThread2 = CreateMockProcessThread();
            var mockProcessThread3 = CreateMockProcessThread();

            var processThread1 = mockProcessThread1.Object;
            var processThread2 = mockProcessThread2.Object;
            var processThread3 = mockProcessThread3.Object;
            mockProcessFactory.SetupSequence(o => o.New())
                .Returns(mockProcessThread1.Object)
                .Returns(mockProcessThread2.Object)
                .Returns(mockProcessThread3.Object);

            var expectedConfig = mockConfig.Object;
            var list = new ProcessThreadListForTesting(expectedConfig, mockProcessFactory.Object);

            list.Monitor();

            Assert.IsFalse(list.NeedUpdate);
            Assert.IsTrue(processThread1.IsAlive);
            Assert.IsTrue(processThread2.IsAlive);
            Assert.IsTrue(processThread3.IsAlive);
            mockProcessThread1.Verify(o => o.Start(), Times.Once);
            mockProcessThread2.Verify(o => o.Start(), Times.Once);
            mockProcessThread3.Verify(o => o.Start(), Times.Once);

            mockConfig.Setup(o => o.Concurrency).Returns(2);
            list.UpdateConfig(mockConfig.Object);

            Assert.IsTrue(list.NeedUpdate);
            list.Monitor();
            mockProcessThread3.Verify(o => o.Kill(), Times.Once);
        }


        internal class ProcessThreadForTesting : IProcessThread
        {
            private IJobConfig _config;

            public ProcessThreadForTesting(IJobConfig config)
            {
                _config = config;
            }

            public bool IsAlive { get; }

            public event ProcessDiedEvent OnProcessDied;

            public void ForceProcessDiedEvent()
            {
                OnProcessDied(_config);
            }

            public void Kill() { }
            public void Start() { }
        }
    }


    public interface ITestProcessFactory
    {
        IProcessThread New();
    }
    internal class ProcessThreadListForTesting : ProcessThreadList
    {
        ITestProcessFactory _testProcessFactory;
        public ProcessThreadListForTesting(IJobConfig config, ITestProcessFactory testProcessFactory)
            : base(config)
        {
            _testProcessFactory = testProcessFactory;
        }

        protected override IProcessThread GetProcessThread() => _testProcessFactory.New();
    }
}
