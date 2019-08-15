using Dev2.Common;
using Dev2.Common.Interfaces.Monitoring;
using Dev2.PerformanceCounters.Counters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Linq;

namespace Dev2.Infrastructure.Tests.PerformanceCounters
{
    [TestClass]
    public class WarewolfAverageExecutionTimePerformanceCounterTests
    {
        const string CounterName = "Average workflow execution time";
        const string CounterName2 = "average time per operation base";

        string _categoryInstanceName = GlobalConstants.GlobalCounterName;
        Guid _resourceGuid = Guid.NewGuid();

        [TestMethod]
        public void WarewolfAverageExecutionTimePerformanceCounter_Construct()
        {
            var mockPerformanceCounterFactory = new Mock<IRealPerformanceCounterFactory>();
            var performanceCounterFactory = mockPerformanceCounterFactory.Object;
            using (var originalCounter = new WarewolfAverageExecutionTimePerformanceCounter(performanceCounterFactory))
            {
                IPerformanceCounter counter = originalCounter;

                Assert.IsTrue(counter.IsActive);
                Assert.AreEqual(WarewolfPerfCounterType.AverageExecutionTime, counter.PerfCounterType);
                Assert.AreEqual(GlobalConstants.Warewolf, counter.Category);
                Assert.AreEqual(CounterName, counter.Name);
            }
        }

        [TestMethod]
        public void WarewolfAverageExecutionTimePerformanceCounter_CreationData_Valid()
        {
            var mockPerformanceCounterFactory = new Mock<IRealPerformanceCounterFactory>();
            var performanceCounterFactory = mockPerformanceCounterFactory.Object;
            IPerformanceCounter counter = new WarewolfAverageExecutionTimePerformanceCounter(performanceCounterFactory);

            var data = counter.CreationData();
            Assert.AreEqual(2, data.Count());

            var dataItems = counter.CreationData().ToList();
            Assert.AreEqual(CounterName, dataItems[0].CounterHelp);
            Assert.AreEqual(CounterName, dataItems[0].CounterName);
            Assert.AreEqual(PerformanceCounterType.AverageTimer32, dataItems[0].CounterType);

            Assert.AreEqual("Average duration per operation execution base", dataItems[1].CounterHelp);
            Assert.AreEqual(CounterName2, dataItems[1].CounterName);
            Assert.AreEqual(PerformanceCounterType.AverageBase, dataItems[1].CounterType);
        }


        [TestMethod]
        public void WarewolfAverageExecutionTimePerformanceCounter_Reset_ClearsCounter()
        {
            var mockPerformanceCounterFactory = new Mock<IRealPerformanceCounterFactory>();
            var mockCounter = new Mock<IWarewolfPerformanceCounter>();
            var mockCounter2 = new Mock<IWarewolfPerformanceCounter>();
            mockPerformanceCounterFactory.Setup(o => o.New(GlobalConstants.Warewolf, CounterName, GlobalConstants.GlobalCounterName)).Returns(mockCounter.Object).Verifiable();
            mockPerformanceCounterFactory.Setup(o => o.New(GlobalConstants.Warewolf, CounterName2, GlobalConstants.GlobalCounterName)).Returns(mockCounter2.Object).Verifiable();
            var performanceCounterFactory = mockPerformanceCounterFactory.Object;
            IPerformanceCounter counter = new WarewolfAverageExecutionTimePerformanceCounter(performanceCounterFactory);
            counter.Setup();
            counter.Reset();

            mockPerformanceCounterFactory.Verify();
            mockCounter.VerifySet(o => o.RawValue = 0, Times.Once);
            mockCounter2.VerifySet(o => o.RawValue = 0, Times.Once);
        }

        [TestMethod]
        public void WarewolfAverageExecutionTimePerformanceCounter_Increment_CallsUnderlyingCounter()
        {
            var mockPerformanceCounterFactory = new Mock<IRealPerformanceCounterFactory>();
            var mockCounter = new Mock<IWarewolfPerformanceCounter>();
            var mockCounter2 = new Mock<IWarewolfPerformanceCounter>();
            mockPerformanceCounterFactory.Setup(o => o.New(GlobalConstants.Warewolf, CounterName, GlobalConstants.GlobalCounterName)).Returns(mockCounter.Object).Verifiable();
            mockPerformanceCounterFactory.Setup(o => o.New(GlobalConstants.Warewolf, CounterName2, GlobalConstants.GlobalCounterName)).Returns(mockCounter2.Object).Verifiable();
            var performanceCounterFactory = mockPerformanceCounterFactory.Object;
            IPerformanceCounter counter = new WarewolfAverageExecutionTimePerformanceCounter(performanceCounterFactory);
            counter.Setup();
            counter.Increment();

            mockPerformanceCounterFactory.Verify();
            mockCounter.Verify(o => o.Increment(), Times.Once);
            mockCounter2.Verify(o => o.Increment(), Times.Once);
        }

        [TestMethod]
        public void WarewolfAverageExecutionTimePerformanceCounter_IncrementBy_CallsUnderlyingCounter()
        {
            var mockPerformanceCounterFactory = new Mock<IRealPerformanceCounterFactory>();
            var mockCounter = new Mock<IWarewolfPerformanceCounter>();
            var mockCounter2 = new Mock<IWarewolfPerformanceCounter>();
            mockPerformanceCounterFactory.Setup(o => o.New(GlobalConstants.Warewolf, CounterName, GlobalConstants.GlobalCounterName)).Returns(mockCounter.Object).Verifiable();
            mockPerformanceCounterFactory.Setup(o => o.New(GlobalConstants.Warewolf, CounterName2, GlobalConstants.GlobalCounterName)).Returns(mockCounter2.Object).Verifiable();
            var performanceCounterFactory = mockPerformanceCounterFactory.Object;
            IPerformanceCounter counter = new WarewolfAverageExecutionTimePerformanceCounter(performanceCounterFactory);
            counter.Setup();
            counter.IncrementBy(1234);

            mockPerformanceCounterFactory.Verify();
            mockCounter.Verify(o => o.IncrementBy(1234), Times.Once);
            mockCounter2.Verify(o => o.Increment(), Times.Once);
        }

        [TestMethod]
        public void WarewolfAverageExecutionTimePerformanceCounter_Setup_CreatesCounter()
        {
            var mockPerformanceCounterFactory = new Mock<IRealPerformanceCounterFactory>();
            var mockCounter = new Mock<IWarewolfPerformanceCounter>();
            var mockCounter2 = new Mock<IWarewolfPerformanceCounter>();
            mockPerformanceCounterFactory.Setup(o => o.New(GlobalConstants.Warewolf, CounterName, GlobalConstants.GlobalCounterName)).Returns(mockCounter.Object);
            mockPerformanceCounterFactory.Setup(o => o.New(GlobalConstants.Warewolf, CounterName2, GlobalConstants.GlobalCounterName)).Returns(mockCounter2.Object).Verifiable();
            var performanceCounterFactory = mockPerformanceCounterFactory.Object;
            IPerformanceCounter counter = new WarewolfAverageExecutionTimePerformanceCounter(performanceCounterFactory);
            counter.Setup();

            mockPerformanceCounterFactory.Verify(o => o.New(GlobalConstants.Warewolf, CounterName, GlobalConstants.GlobalCounterName), Times.Once);
            mockPerformanceCounterFactory.Verify(o => o.New(GlobalConstants.Warewolf, CounterName2, GlobalConstants.GlobalCounterName), Times.Once);
        }

        [TestMethod]
        public void WarewolfAverageExecutionTimePerformanceCounter_Decrement_CallsUnderlyingCounter()
        {
            var mockPerformanceCounterFactory = new Mock<IRealPerformanceCounterFactory>();
            var mockCounter = new Mock<IWarewolfPerformanceCounter>();
            var mockCounter2 = new Mock<IWarewolfPerformanceCounter>();
            mockCounter.SetupGet(o => o.RawValue).Returns(1);
            mockPerformanceCounterFactory.Setup(o => o.New(GlobalConstants.Warewolf, CounterName, GlobalConstants.GlobalCounterName)).Returns(mockCounter.Object).Verifiable();
            mockPerformanceCounterFactory.Setup(o => o.New(GlobalConstants.Warewolf, CounterName2, GlobalConstants.GlobalCounterName)).Returns(mockCounter2.Object).Verifiable();
            var performanceCounterFactory = mockPerformanceCounterFactory.Object;
            using (IPerformanceCounter counter = new WarewolfAverageExecutionTimePerformanceCounter(performanceCounterFactory))
            {
                counter.Setup();
                counter.Decrement();

                mockPerformanceCounterFactory.Verify();
                mockCounter.Verify(o => o.Decrement(), Times.Once);
                mockCounter2.Verify(o => o.Decrement(), Times.Once);
            }
        }
    }
}
