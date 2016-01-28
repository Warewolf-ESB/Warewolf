using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Dev2.Common.Interfaces.Monitoring;
using Dev2.Diagnostics.PerformanceCounters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Diagnostics.Test
{
    [TestClass]
    public class PerfCounterBuilderTest
    {

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("PerformanceCounterBuilder_CtorBuildCounters")]
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
            var lst = new List<IPerformanceCounter>
                                                            {
                                                                new WarewolfCurrentExecutionsPerformanceCounter(),
                                                                new WarewolfNumberOfErrors(),    
                                                               
                                                                new WarewolfRequestsPerSecondPerformanceCounter(),
                                                                 new WarewolfAverageExecutionTimePerformanceCounter(),
                                                                 new WarewolfNumberOfAuthErrors(),
                                                                 new WarewolfServicesNotFoundCounter()
                                                            };
            WarewolfPerformanceCounterBuilder builder = new WarewolfPerformanceCounterBuilder(lst);
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
            var lst = new List<IPerformanceCounter>
                                                            {
                                                                new WarewolfCurrentExecutionsPerformanceCounter(),
                                                                new WarewolfNumberOfErrors(),    
                                                               
                                                                new WarewolfRequestsPerSecondPerformanceCounter(),
                                                                 new WarewolfAverageExecutionTimePerformanceCounter(),
                                                                 new WarewolfNumberOfAuthErrors(),
                                                                 new WarewolfServicesNotFoundCounter()
                                                            };
            WarewolfPerformanceCounterBuilder builder = new WarewolfPerformanceCounterBuilder(lst);
            foreach(var performanceCounter in builder.Counters)
            {
                performanceCounter.Increment();
            }

             builder = new WarewolfPerformanceCounterBuilder(lst);
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
            var lst = new List<IPerformanceCounter>
                                                            {
                                                                new WarewolfCurrentExecutionsPerformanceCounter(),
                                                                new WarewolfNumberOfErrors(),    
                                                               
                                                                new WarewolfRequestsPerSecondPerformanceCounter(),
                                                                 new WarewolfAverageExecutionTimePerformanceCounter(),
                                                                 new WarewolfNumberOfAuthErrors(),
                                                   
                                                            };

            WarewolfPerformanceCounterBuilder builder = new WarewolfPerformanceCounterBuilder(lst);
            foreach (var performanceCounter in builder.Counters)
            {
                performanceCounter.Increment();
            }
            lst.Add(new WarewolfServicesNotFoundCounter());
            builder = new WarewolfPerformanceCounterBuilder(lst);
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
