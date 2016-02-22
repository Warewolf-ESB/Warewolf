using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Dev2.Common.Interfaces.Monitoring;
using Dev2.PerformanceCounters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
// ReSharper disable RedundantAssignment
// ReSharper disable InconsistentNaming

namespace Dev2.Diagnostics.Test
{
    [TestClass]
    public class PerfCounterBuilderTest
    {
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("PerformanceCounterBuilder_CtorBuildCounters")]
        // ReSharper disable once InconsistentNaming
        public void PerformanceCounterBuilder_CtorBuildCounters_Valid_ExpectNewCounters()
        {
            try
            {
                PerformanceCounterCategory.Delete("Warewolf");
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch
            {

            }
            var lst = new List<IPerformanceCounter>{
                new WarewolfCurrentExecutionsPerformanceCounter(),
                new WarewolfNumberOfErrors(),    
                new WarewolfRequestsPerSecondPerformanceCounter(),
                new WarewolfAverageExecutionTimePerformanceCounter(),
                new WarewolfNumberOfAuthErrors(),
                new WarewolfServicesNotFoundCounter()
            };
            // ReSharper disable once ObjectCreationAsStatement
            new WarewolfPerformanceCounterRegister(lst);
            PerformanceCounterCategory cat = new PerformanceCounterCategory("Warewolf");
            var counters = cat.GetCounters();
            foreach (var performanceCounter in counters)
            {
                if (performanceCounter.CounterName != "average time per operation base")
                {
                    Assert.AreEqual(performanceCounter.RawValue, 0);
                    Assert.IsTrue(lst.Any(a => a.Name == performanceCounter.CounterName));
                    Assert.AreEqual(performanceCounter.CategoryName, "Warewolf");
                }
            }
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("PerformanceCounterBuilder_CtorBuildCounters")]
        // ReSharper disable once InconsistentNaming
        public void PerformanceCounterBuilder_CtorBuildCounters_RebuildDoesNotReset_ExpectNewCounters()
        {
            try
            {
                PerformanceCounterCategory.Delete("Warewolf");
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch
            {

            }
            var lst = new List<IPerformanceCounter> {
                new WarewolfCurrentExecutionsPerformanceCounter(),
                new WarewolfNumberOfErrors(),    
                new WarewolfRequestsPerSecondPerformanceCounter(),
                new WarewolfAverageExecutionTimePerformanceCounter(),
                new WarewolfNumberOfAuthErrors(),
                new WarewolfServicesNotFoundCounter()
            };
            WarewolfPerformanceCounterRegister register = new WarewolfPerformanceCounterRegister(lst);
            foreach (var performanceCounter in register.Counters)
            {
                performanceCounter.Increment();
            }

            register = new WarewolfPerformanceCounterRegister(lst);
            PerformanceCounterCategory cat = new PerformanceCounterCategory("Warewolf");
            var counters = cat.GetCounters();
            foreach (var performanceCounter in counters)
            {
                if (performanceCounter.CounterName != "average time per operation base")
                {
                    Assert.AreEqual(performanceCounter.RawValue, 1);
                    Assert.IsTrue(lst.Any(a => a.Name == performanceCounter.CounterName));
                    Assert.AreEqual(performanceCounter.CategoryName, "Warewolf");
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
                PerformanceCounterCategory.Delete("Warewolf");
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch
            {

            }
            var lst = new List<IPerformanceCounter> {
                new WarewolfCurrentExecutionsPerformanceCounter(),
                new WarewolfNumberOfErrors(),    
                new WarewolfRequestsPerSecondPerformanceCounter(),
                new WarewolfAverageExecutionTimePerformanceCounter(),
                new WarewolfNumberOfAuthErrors()
            };

            WarewolfPerformanceCounterRegister register = new WarewolfPerformanceCounterRegister(lst);
            foreach (var performanceCounter in register.Counters)
            {
                performanceCounter.Increment();
            }
            lst.Add(new WarewolfServicesNotFoundCounter());
            register = new WarewolfPerformanceCounterRegister(lst);
            PerformanceCounterCategory cat = new PerformanceCounterCategory("Warewolf");
            var counters = cat.GetCounters();
            foreach (var performanceCounter in counters)
            {
                if (performanceCounter.CounterName != "average time per operation base")
                {
                    Assert.AreEqual(performanceCounter.RawValue, 0);
                    Assert.IsTrue(lst.Any(a => a.Name == performanceCounter.CounterName));
                    Assert.AreEqual(performanceCounter.CategoryName, "Warewolf");
                }
            }
        }
    }
}
