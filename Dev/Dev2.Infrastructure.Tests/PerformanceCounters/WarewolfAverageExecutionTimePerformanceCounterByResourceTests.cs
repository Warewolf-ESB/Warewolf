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
    public class WarewolfAverageExecutionTimePerformanceCounterByResourceTests
    {
        const string CounterName = "Average workflow execution time";
        string _categoryInstanceName = GlobalConstants.GlobalCounterName;
        Guid _resourceGuid = Guid.NewGuid();

        [TestMethod]
        public void WarewolfAverageExecutionTimePerformanceCounterByResource_Construct()
        {
            var mockPerformanceCounterFactory = new Mock<IRealPerformanceCounterFactory>();
            var performanceCounterFactory = mockPerformanceCounterFactory.Object;
            var originalCounter = new WarewolfAverageExecutionTimePerformanceCounterByResource(_resourceGuid, _categoryInstanceName, performanceCounterFactory);
            IPerformanceCounter counter = originalCounter;

            Assert.IsTrue(counter.IsActive);
            Assert.AreEqual(WarewolfPerfCounterType.AverageExecutionTime, counter.PerfCounterType);
            Assert.AreEqual(GlobalConstants.WarewolfServices, counter.Category);
            Assert.AreEqual(CounterName, counter.Name);
            Assert.AreEqual(_resourceGuid, originalCounter.ResourceId);
        }

        [TestMethod]
        public void WarewolfAverageExecutionTimePerformanceCounterByResource_CreationData_Valid()
        {
            var mockPerformanceCounterFactory = new Mock<IRealPerformanceCounterFactory>();
            var performanceCounterFactory = mockPerformanceCounterFactory.Object;
            IPerformanceCounter counter = new WarewolfAverageExecutionTimePerformanceCounterByResource(_resourceGuid, _categoryInstanceName, performanceCounterFactory);

            var data = counter.CreationData();
            Assert.AreEqual(2, data.Count());

            var dataItems = counter.CreationData().ToList();
            Assert.AreEqual(CounterName, dataItems[0].CounterHelp);
            Assert.AreEqual(CounterName, dataItems[0].CounterName);
            Assert.AreEqual(PerformanceCounterType.AverageTimer32, dataItems[0].CounterType);

            Assert.AreEqual("Average duration per operation execution base", dataItems[1].CounterHelp);
            Assert.AreEqual("average time per operation base", dataItems[1].CounterName);
            Assert.AreEqual(PerformanceCounterType.AverageBase, dataItems[1].CounterType);
        }


        [TestMethod]
        public void WarewolfAverageExecutionTimePerformanceCounterByResource_Reset_ClearsCounter()
        {
            var mockPerformanceCounterFactory = new Mock<IRealPerformanceCounterFactory>();
            var mockCounter = new Mock<IWarewolfPerformanceCounter>();
            mockPerformanceCounterFactory.Setup(o => o.New(GlobalConstants.WarewolfServices, CounterName, GlobalConstants.GlobalCounterName)).Returns(mockCounter.Object).Verifiable();
            var performanceCounterFactory = mockPerformanceCounterFactory.Object;
            IPerformanceCounter counter = new WarewolfAverageExecutionTimePerformanceCounterByResource(_resourceGuid, _categoryInstanceName, performanceCounterFactory);
            counter.Setup();
            counter.Reset();

            mockPerformanceCounterFactory.Verify();
            mockCounter.VerifySet(o => o.RawValue = 0, Times.Once);
        }

        [TestMethod]
        public void WarewolfAverageExecutionTimePerformanceCounterByResource_Increment_CallsUnderlyingCounter()
        {
            var mockPerformanceCounterFactory = new Mock<IRealPerformanceCounterFactory>();
            var mockCounter = new Mock<IWarewolfPerformanceCounter>();
            var mockCounter2 = new Mock<IWarewolfPerformanceCounter>();
            mockPerformanceCounterFactory.Setup(o => o.New(GlobalConstants.WarewolfServices, CounterName, GlobalConstants.GlobalCounterName)).Returns(mockCounter.Object).Verifiable();
            mockPerformanceCounterFactory.Setup(o => o.New(GlobalConstants.WarewolfServices, "average time per operation base", GlobalConstants.GlobalCounterName)).Returns(mockCounter2.Object).Verifiable();

            var performanceCounterFactory = mockPerformanceCounterFactory.Object;
            IPerformanceCounter counter = new WarewolfAverageExecutionTimePerformanceCounterByResource(_resourceGuid, _categoryInstanceName, performanceCounterFactory);
            counter.Setup();
            counter.Increment();

            mockPerformanceCounterFactory.Verify();
            mockCounter.Verify(o => o.Increment(), Times.Once);
            mockCounter2.Verify(o => o.Increment(), Times.Once);
        }

        [TestMethod]
        public void WarewolfAverageExecutionTimePerformanceCounterByResource_IncrementBy_CallsUnderlyingCounter()
        {
            var mockPerformanceCounterFactory = new Mock<IRealPerformanceCounterFactory>();
            var mockCounter = new Mock<IWarewolfPerformanceCounter>();
            var mockCounter2 = new Mock<IWarewolfPerformanceCounter>();
            mockPerformanceCounterFactory.Setup(o => o.New(GlobalConstants.WarewolfServices, CounterName, GlobalConstants.GlobalCounterName)).Returns(mockCounter.Object).Verifiable();
            mockPerformanceCounterFactory.Setup(o => o.New(GlobalConstants.WarewolfServices, "average time per operation base", GlobalConstants.GlobalCounterName)).Returns(mockCounter2.Object).Verifiable();

            var performanceCounterFactory = mockPerformanceCounterFactory.Object;
            IPerformanceCounter counter = new WarewolfAverageExecutionTimePerformanceCounterByResource(_resourceGuid, _categoryInstanceName, performanceCounterFactory);
            counter.Setup();
            counter.IncrementBy(1234);

            mockPerformanceCounterFactory.Verify();
            mockCounter.Verify(o => o.IncrementBy(1234), Times.Once);
            mockCounter2.Verify(o => o.Increment(), Times.Once);
        }

        [TestMethod]
        public void WarewolfAverageExecutionTimePerformanceCounterByResource_Setup_CreatesCounter()
        {
            var mockPerformanceCounterFactory = new Mock<IRealPerformanceCounterFactory>();
            var mockCounter = new Mock<IWarewolfPerformanceCounter>();
            mockPerformanceCounterFactory.Setup(o => o.New(GlobalConstants.WarewolfServices, CounterName, GlobalConstants.GlobalCounterName)).Returns(mockCounter.Object);
            var performanceCounterFactory = mockPerformanceCounterFactory.Object;
            IPerformanceCounter counter = new WarewolfAverageExecutionTimePerformanceCounterByResource(_resourceGuid, _categoryInstanceName, performanceCounterFactory);
            counter.Setup();

            mockPerformanceCounterFactory.Verify(o => o.New(GlobalConstants.WarewolfServices, CounterName, GlobalConstants.GlobalCounterName), Times.Once);
        }

        [TestMethod]
        public void WarewolfAverageExecutionTimePerformanceCounterByResource_Decrement_CallsUnderlyingCounter()
        {
            var mockPerformanceCounterFactory = new Mock<IRealPerformanceCounterFactory>();
            var mockCounter = new Mock<IWarewolfPerformanceCounter>();
            var mockCounter2 = new Mock<IWarewolfPerformanceCounter>();
            mockCounter.SetupGet(o => o.RawValue).Returns(1);
            mockCounter2.SetupGet(o => o.RawValue).Returns(1);
            mockPerformanceCounterFactory.Setup(o => o.New(GlobalConstants.WarewolfServices, CounterName, GlobalConstants.GlobalCounterName)).Returns(mockCounter.Object).Verifiable();
            mockPerformanceCounterFactory.Setup(o => o.New(GlobalConstants.WarewolfServices, "average time per operation base", GlobalConstants.GlobalCounterName)).Returns(mockCounter2.Object).Verifiable();


            var performanceCounterFactory = mockPerformanceCounterFactory.Object;
            using (IPerformanceCounter counter = new WarewolfAverageExecutionTimePerformanceCounterByResource(_resourceGuid, _categoryInstanceName, performanceCounterFactory))
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
