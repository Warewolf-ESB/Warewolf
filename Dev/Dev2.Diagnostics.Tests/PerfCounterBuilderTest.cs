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
        public void PerformanceCounterBuilder_CtorBuildCounters_Valid_ExpectNewCounters()
        {
            try
            {
                PerformanceCounterCategory.Delete(CategoryName);
            }
            catch
            {

            }
            var counterFactory = new Mock<PerformanceCounters.Counters.IRealPerformanceCounterFactory>();
            var lst = new List<IPerformanceCounter>{
                new WarewolfCurrentExecutionsPerformanceCounter(counterFactory.Object),
                new WarewolfNumberOfErrors(counterFactory.Object),
                new WarewolfRequestsPerSecondPerformanceCounter(counterFactory.Object),
                new WarewolfAverageExecutionTimePerformanceCounter(counterFactory.Object),
                new WarewolfNumberOfAuthErrors(counterFactory.Object),
                new WarewolfServicesNotFoundCounter(counterFactory.Object)
            };

            new WarewolfPerformanceCounterRegister(lst, new List<IResourcePerformanceCounter>());
            var cat = new PerformanceCounterCategory(CategoryName);
            var counters = cat.GetCounters();
            foreach (var performanceCounter in counters)
            {
                if (performanceCounter.CounterName != "average time per operation base")
                {
                    Assert.IsTrue(lst.Any(a => a.Name == performanceCounter.CounterName), "PerformanceCounter CounterName: " + performanceCounter.CounterName + " die not match any criteria.");
                    Assert.AreEqual(CategoryName, performanceCounter.CategoryName);
                }
            }
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("PerformanceCounterBuilder_CtorBuildCounters")]

        public void PerformanceCounterBuilder_CtorBuildCounters_RebuildDoesNotReset_ExpectNewCounters()
        {
            try
            {
                PerformanceCounterCategory.Delete(CategoryName);
            }
            catch
            {

            }
            var counterFactory = new Mock<PerformanceCounters.Counters.IRealPerformanceCounterFactory>();
            var lst = new List<IPerformanceCounter> {
                new WarewolfCurrentExecutionsPerformanceCounter(counterFactory.Object),
                new WarewolfNumberOfErrors(counterFactory.Object),    
                new WarewolfRequestsPerSecondPerformanceCounter(counterFactory.Object),
                new WarewolfAverageExecutionTimePerformanceCounter(counterFactory.Object),
                new WarewolfNumberOfAuthErrors(counterFactory.Object),
                new WarewolfServicesNotFoundCounter(counterFactory.Object)
            };
            var register = new WarewolfPerformanceCounterRegister(lst, new List<IResourcePerformanceCounter>());
            foreach (var performanceCounter in register.Counters)
            {
                performanceCounter.ToSafe().Increment();
            }

            register = new WarewolfPerformanceCounterRegister(lst, new List<IResourcePerformanceCounter>());
            var cat = new PerformanceCounterCategory(CategoryName);
            var counters = cat.GetCounters(GlobalConstants.GlobalCounterName);
            foreach (var performanceCounter in counters)
            {
                if (performanceCounter.CounterName != "average time per operation base")
                {
                    Assert.AreEqual(1, performanceCounter.RawValue);
                    Assert.IsTrue(lst.Any(a => a.Name == performanceCounter.CounterName), "PerformanceCounter CounterName: " + performanceCounter.CounterName + " die not match any criteria.");
                    Assert.AreEqual(CategoryName, performanceCounter.CategoryName);
                }
            }
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
            var mockCounter1 = new Mock<IBobsPerformanceCounter>();
            var mockCounter2 = new Mock<IBobsPerformanceCounter>();
            var mockCounter3 = new Mock<IBobsPerformanceCounter>();
            var mockCounter4 = new Mock<IBobsPerformanceCounter>();
            var mockCounter5 = new Mock<IBobsPerformanceCounter>();
            var mockCounter6 = new Mock<IBobsPerformanceCounter>();
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
            mockPerformanceCounterCategory.Setup(o => o.Create("Warewolf", "Warewolf Performance Counters", It.IsAny<IEnumerable<IPerformanceCounter>>()))
                .Callback<string, string, IEnumerable<IPerformanceCounter>>((s1, s2, counters_arg) => Assert.AreEqual(5, counters_arg.Count())).Verifiable();

            mockPerformanceCounterCategory.Setup(o => o.Create("Warewolf", "Warewolf Services", It.IsAny<IEnumerable<IPerformanceCounter>>()))
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



            lst.Add(new WarewolfServicesNotFoundCounter(counterFactory));
            register = new WarewolfPerformanceCounterRegister(mockPerformanceCounterCategory.Object, lst, new List<IResourcePerformanceCounter>());

            

            foreach (var performanceCounter in register.Counters)
            {
                performanceCounter.ToSafe().Increment(); // increment causes instance to be created on windows side
            }
            var cat = new PerformanceCounterCategory(CategoryName);
            var counters = cat.GetCounters(GlobalConstants.GlobalCounterName);
            foreach (var performanceCounter in counters)
            {
                if (performanceCounter.CounterName != "average time per operation base")
                {
                    Assert.IsTrue(lst.Any(a => a.Name == performanceCounter.CounterName), "PerformanceCounter CounterName: " + performanceCounter.CounterName + " die not match any criteria.");
                    Assert.AreEqual(CategoryName, performanceCounter.CategoryName);
                }
            }
        }
    }
}
