using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Dev2.Common;
using Dev2.Common.Interfaces.Monitoring;
using Dev2.PerformanceCounters.Counters;
using Dev2.PerformanceCounters.Management;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Diagnostics.Test
{
    [TestClass]
    public class PerfCounterBuilderTest
    {
        const string CategoryName = "Warewolf";

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("PerformanceCounterBuilder_CtorBuildCounters")]
        public void PerformanceCounterBuilder_CtorBuildCounters_RegisterExistingCounters()
        {
            try
            {
                PerformanceCounterCategory.Delete(CategoryName);
            }
            catch
            {

            }
            var mockCounterFactory = new Mock<IRealPerformanceCounterFactory>();
            var mockCounter1 = new Mock<IWarewolfPerformanceCounter>();
            var mockCounter2 = new Mock<IWarewolfPerformanceCounter>();
            var mockCounter3 = new Mock<IWarewolfPerformanceCounter>();
            var mockCounter4 = new Mock<IWarewolfPerformanceCounter>();
            var mockCounter5 = new Mock<IWarewolfPerformanceCounter>();
            var mockCounter6 = new Mock<IWarewolfPerformanceCounter>();
            var mockCounter7 = new Mock<IWarewolfPerformanceCounter>();
            mockCounterFactory.Setup(o => o.New(GlobalConstants.Warewolf, "Concurrent requests currently executing", GlobalConstants.GlobalCounterName)).Returns(mockCounter1.Object).Verifiable();
            mockCounterFactory.Setup(o => o.New(GlobalConstants.Warewolf, "Total Errors", GlobalConstants.GlobalCounterName)).Returns(mockCounter2.Object).Verifiable();
            mockCounterFactory.Setup(o => o.New(GlobalConstants.Warewolf, "Request Per Second", GlobalConstants.GlobalCounterName)).Returns(mockCounter3.Object).Verifiable();
            mockCounterFactory.Setup(o => o.New(GlobalConstants.Warewolf, "Average workflow execution time", GlobalConstants.GlobalCounterName)).Returns(mockCounter4.Object).Verifiable();
            mockCounterFactory.Setup(o => o.New(GlobalConstants.Warewolf, "average time per operation base", GlobalConstants.GlobalCounterName)).Returns(mockCounter5.Object).Verifiable();
            mockCounterFactory.Setup(o => o.New(GlobalConstants.Warewolf, "Count of Not Authorised errors", GlobalConstants.GlobalCounterName)).Returns(mockCounter6.Object).Verifiable();
            mockCounterFactory.Setup(o => o.New(GlobalConstants.Warewolf, "Count of requests for workflows which don't exist", GlobalConstants.GlobalCounterName)).Returns(mockCounter7.Object);

            var counterFactory = mockCounterFactory.Object;
            var mockPerformanceCounterCategory = new Mock<IPerformanceCounterCategory>();

            mockPerformanceCounterCategory.Setup(o => o.Create("Warewolf", "Warewolf Performance Counters", It.IsAny<IEnumerable<IPerformanceCounter>>()))
                .Callback<string, string, IEnumerable<IPerformanceCounter>>((s1, s2, counters_arg) => Assert.AreEqual(6, counters_arg.Count())).Verifiable();

            var lst = new List<IPerformanceCounter>{
                new WarewolfCurrentExecutionsPerformanceCounter(counterFactory),
                new WarewolfNumberOfErrors(counterFactory),
                new WarewolfRequestsPerSecondPerformanceCounter(counterFactory),
                new WarewolfAverageExecutionTimePerformanceCounter(counterFactory),
                new WarewolfNumberOfAuthErrors(counterFactory),
                new WarewolfServicesNotFoundCounter(counterFactory)
            };

            new WarewolfPerformanceCounterRegister(mockPerformanceCounterCategory.Object, lst, new List<IResourcePerformanceCounter>());

            mockPerformanceCounterCategory.Verify();
            mockCounterFactory.Verify(o => o.New(GlobalConstants.Warewolf, "Concurrent requests currently executing", GlobalConstants.GlobalCounterName), Times.Never);
            mockCounterFactory.Verify(o => o.New(GlobalConstants.Warewolf, "Total Errors", GlobalConstants.GlobalCounterName), Times.Never);
            mockCounterFactory.Verify(o => o.New(GlobalConstants.Warewolf, "Request Per Second", GlobalConstants.GlobalCounterName), Times.Never);
            mockCounterFactory.Verify(o => o.New(GlobalConstants.Warewolf, "Average workflow execution time", GlobalConstants.GlobalCounterName), Times.Never);
            mockCounterFactory.Verify(o => o.New(GlobalConstants.Warewolf, "average time per operation base", GlobalConstants.GlobalCounterName), Times.Never);
            mockCounterFactory.Verify(o => o.New(GlobalConstants.Warewolf, "Count of Not Authorised errors", GlobalConstants.GlobalCounterName), Times.Never);

            mockCounter1.Verify(o => o.Increment(), Times.Never);
            mockCounter2.Verify(o => o.Increment(), Times.Never);
            mockCounter3.Verify(o => o.Increment(), Times.Never);
            mockCounter4.Verify(o => o.Increment(), Times.Never);
            mockCounter5.Verify(o => o.Increment(), Times.Never);
            mockCounter6.Verify(o => o.Increment(), Times.Never);
            mockCounter7.Verify(o => o.Increment(), Times.Never);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("PerformanceCounterBuilder_CtorBuildCounters")]
        public void PerformanceCounterBuilder_CtorBuildCounters_NewResets_ExpectNewCounters()
        {
            try
            {
                PerformanceCounterCategory.Delete(CategoryName);
            }
            catch
            {

            }
            var mockCounterFactory = new Mock<PerformanceCounters.Counters.IRealPerformanceCounterFactory>();
            var mockCounter1 = new Mock<IWarewolfPerformanceCounter>();
            var mockCounter2 = new Mock<IWarewolfPerformanceCounter>();
            var mockCounter3 = new Mock<IWarewolfPerformanceCounter>();
            var mockCounter4 = new Mock<IWarewolfPerformanceCounter>();
            var mockCounter5 = new Mock<IWarewolfPerformanceCounter>();
            var mockCounter6 = new Mock<IWarewolfPerformanceCounter>();
            mockCounterFactory.Setup(o => o.New(GlobalConstants.Warewolf, "Concurrent requests currently executing", GlobalConstants.GlobalCounterName)).Returns(mockCounter1.Object).Verifiable();
            mockCounterFactory.Setup(o => o.New(GlobalConstants.Warewolf, "Total Errors", GlobalConstants.GlobalCounterName)).Returns(mockCounter2.Object).Verifiable();
            mockCounterFactory.Setup(o => o.New(GlobalConstants.Warewolf, "Request Per Second", GlobalConstants.GlobalCounterName)).Returns(mockCounter3.Object).Verifiable();
            mockCounterFactory.Setup(o => o.New(GlobalConstants.Warewolf, "Average workflow execution time", GlobalConstants.GlobalCounterName)).Returns(mockCounter4.Object).Verifiable();
            mockCounterFactory.Setup(o => o.New(GlobalConstants.Warewolf, "average time per operation base", GlobalConstants.GlobalCounterName)).Returns(mockCounter5.Object).Verifiable();
            mockCounterFactory.Setup(o => o.New(GlobalConstants.Warewolf, "Count of Not Authorised errors", GlobalConstants.GlobalCounterName)).Returns(mockCounter6.Object).Verifiable();

            var counterFactory = mockCounterFactory.Object;

            var lst = new List<IPerformanceCounter> {
                new WarewolfCurrentExecutionsPerformanceCounter(counterFactory),
                new WarewolfNumberOfErrors(counterFactory),
                new WarewolfRequestsPerSecondPerformanceCounter(counterFactory),
                new WarewolfAverageExecutionTimePerformanceCounter(counterFactory),
                new WarewolfNumberOfAuthErrors(counterFactory)
            };

            var mockPerformanceCounterCategory = new Mock<IPerformanceCounterCategory>();

            {
                int expectedCount = 5;
                mockPerformanceCounterCategory.Setup(o => o.Create("Warewolf", "Warewolf Performance Counters", It.IsAny<IEnumerable<IPerformanceCounter>>()))
                    .Callback<string, string, IEnumerable<IPerformanceCounter>>((s1, s2, counters_arg) => Assert.AreEqual(expectedCount++, counters_arg.Count())).Verifiable();
            }

            mockPerformanceCounterCategory.Setup(o => o.Create("Warewolf Services", "Warewolf Performance Counters", It.IsAny<IEnumerable<IPerformanceCounter>>()))
                .Callback<string, string, IEnumerable<IPerformanceCounter>>((s1, s2, counters_arg) => Assert.AreEqual(0, counters_arg.Count())).Verifiable();

            var register = new WarewolfPerformanceCounterRegister(mockPerformanceCounterCategory.Object, lst, new List<IResourcePerformanceCounter>());
            foreach (var performanceCounter in register.Counters)
            {
                performanceCounter.ToSafe().Increment();
            }

            mockPerformanceCounterCategory.Verify();
            mockCounterFactory.Verify(o => o.New(GlobalConstants.Warewolf, "Concurrent requests currently executing", GlobalConstants.GlobalCounterName), Times.Once);
            mockCounterFactory.Verify(o => o.New(GlobalConstants.Warewolf, "Total Errors", GlobalConstants.GlobalCounterName), Times.Once);
            mockCounterFactory.Verify(o => o.New(GlobalConstants.Warewolf, "Request Per Second", GlobalConstants.GlobalCounterName), Times.Once);
            mockCounterFactory.Verify(o => o.New(GlobalConstants.Warewolf, "Average workflow execution time", GlobalConstants.GlobalCounterName), Times.Once);
            mockCounterFactory.Verify(o => o.New(GlobalConstants.Warewolf, "average time per operation base", GlobalConstants.GlobalCounterName), Times.Once);
            mockCounterFactory.Verify(o => o.New(GlobalConstants.Warewolf, "Count of Not Authorised errors", GlobalConstants.GlobalCounterName), Times.Once);
            mockCounter1.Verify(o => o.Increment(), Times.Once);
            mockCounter2.Verify(o => o.Increment(), Times.Once);
            mockCounter3.Verify(o => o.Increment(), Times.Once);
            mockCounter4.Verify(o => o.Increment(), Times.Once);
            mockCounter5.Verify(o => o.Increment(), Times.Once);
            mockCounter6.Verify(o => o.Increment(), Times.Once);



            var mockCounter7 = new Mock<IWarewolfPerformanceCounter>();
            mockCounterFactory.Setup(o => o.New(GlobalConstants.Warewolf, "Count of requests for workflows which don't exist", GlobalConstants.GlobalCounterName)).Returns(mockCounter7.Object);

            lst.Add(new WarewolfServicesNotFoundCounter(counterFactory));

            register = new WarewolfPerformanceCounterRegister(mockPerformanceCounterCategory.Object, lst, new List<IResourcePerformanceCounter>());

            

            foreach (var performanceCounter in register.Counters)
            {
                performanceCounter.ToSafe().Increment(); // increment causes instance to be created on windows side
            }

            mockCounterFactory.Verify(o => o.New(GlobalConstants.Warewolf, "Count of requests for workflows which don't exist", GlobalConstants.GlobalCounterName), Times.Once);

            mockCounter1.Verify(o => o.Increment(), Times.Exactly(2));
            mockCounter2.Verify(o => o.Increment(), Times.Exactly(2));
            mockCounter3.Verify(o => o.Increment(), Times.Exactly(2));
            mockCounter4.Verify(o => o.Increment(), Times.Exactly(2));
            mockCounter5.Verify(o => o.Increment(), Times.Exactly(2));
            mockCounter6.Verify(o => o.Increment(), Times.Exactly(2));
            mockCounter7.Verify(o => o.Increment(), Times.Once);
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("PerformanceCounterBuilder_CtorBuildCounters")]
        public void WarewolfPerformanceCounterRegister_RegisterNewCounters()
        {
            var mockPerformanceCounterCategory = new Mock<IPerformanceCounterCategory>();
            mockPerformanceCounterCategory.Setup(o => o.Exists(GlobalConstants.Warewolf)).Returns(true);
            mockPerformanceCounterCategory.Setup(o => o.Exists(GlobalConstants.WarewolfServices)).Returns(true);

            var mockPerformanceCounterCategoryInstance = new Mock<IPerformanceCounterCategory>();
            var performanceCounterCategory2 = mockPerformanceCounterCategoryInstance.Object;
            var performanceCounterCategory3 = mockPerformanceCounterCategoryInstance.Object;

            mockPerformanceCounterCategory.Setup(o => o.New(GlobalConstants.Warewolf)).Returns(performanceCounterCategory2);
            mockPerformanceCounterCategory.Setup(o => o.New(GlobalConstants.WarewolfServices)).Returns(performanceCounterCategory3);

            var performanceCounterFactory = mockPerformanceCounterCategory.Object;

            var counters = new List<IPerformanceCounter> {
                new Mock<IPerformanceCounter>().Object
            };
            var resourceCounters = new List<IResourcePerformanceCounter> {
                new Mock<IResourcePerformanceCounter>().Object
            };

            var warewolfPerformanceCounterRegister = new WarewolfPerformanceCounterRegister(performanceCounterFactory, counters, resourceCounters);

            mockPerformanceCounterCategory.Verify(o => o.Delete(GlobalConstants.Warewolf), Times.Once);
            mockPerformanceCounterCategory.Verify(o => o.Delete(GlobalConstants.WarewolfServices), Times.Once);
            mockPerformanceCounterCategory.Verify(o => o.Create(GlobalConstants.Warewolf, "Warewolf Performance Counters", counters), Times.Once);
            mockPerformanceCounterCategory.Verify(o => o.Create(GlobalConstants.WarewolfServices, "Warewolf Performance Counters", resourceCounters), Times.Once);
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("PerformanceCounterBuilder_CtorBuildCounters")]
        public void WarewolfPerformanceCounterRegister_RegisterCountersOnMachine_ShouldNotThrow()
        {
            var mockPerformanceCounterCategory = new Mock<IPerformanceCounterCategory>();
            mockPerformanceCounterCategory.Setup(o => o.Exists(GlobalConstants.Warewolf))
                .Callback(() => throw new System.Exception("fake exception"))
                .Returns(false);

            var performanceCounterFactory = mockPerformanceCounterCategory.Object;

            var counters = new List<IPerformanceCounter>();
            var resourceCounters = new List<IResourcePerformanceCounter>();

            var warewolfPerformanceCounterRegister = new WarewolfPerformanceCounterRegister(performanceCounterFactory, counters, resourceCounters);

            // should always reach here
            mockPerformanceCounterCategory.Verify(o => o.Exists(GlobalConstants.Warewolf), Times.Once);
        }
    }
}
