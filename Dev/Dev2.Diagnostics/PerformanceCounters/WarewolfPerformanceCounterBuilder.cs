using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Dev2.Common.Interfaces.Monitoring;
using System;

namespace Dev2.Diagnostics.PerformanceCounters
{
    public class WarewolfPerformanceCounterBuilder
    {
        public  WarewolfPerformanceCounterBuilder(IList<IPerformanceCounter> counters )
        {
            CreateCounters(counters);
            Counters = counters;
        }

        public IList<IPerformanceCounter> Counters { get; set; }

        private void CreateCounters(IList<IPerformanceCounter> counters)
        {
            try
            {
                if (!PerformanceCounterCategory.Exists("Warewolf"))
                {
                    CreateAllCounters(counters);
                }
                else
                {
                    PerformanceCounterCategory cat = new PerformanceCounterCategory("Warewolf");
                    if (!counters.All(a => cat.CounterExists(a.Name)))
                    {
                        PerformanceCounterCategory.Delete("Warewolf");
                        CreateAllCounters(counters);

                    }
                }
            }
            catch (Exception e)
            {
                Common.Dev2Logger.Error(e);
            }

          

        }

        private static void CreateAllCounters(IEnumerable<IPerformanceCounter> counters)
        {
            CounterCreationDataCollection coll = new CounterCreationDataCollection();
            foreach(var counterl in counters)
            {
                foreach (var counter in counterl.CreationData())
                {
                    coll.Add(counter);
                }
               
            }
            PerformanceCounterCategory.Create("Warewolf", "Warewolf Performance Counters", PerformanceCounterCategoryType.SingleInstance, coll);
        }
    }
}
