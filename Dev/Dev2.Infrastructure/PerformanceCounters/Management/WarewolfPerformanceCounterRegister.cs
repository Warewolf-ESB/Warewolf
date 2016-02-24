using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Dev2.Common.Interfaces.Monitoring;

namespace Dev2.PerformanceCounters.Management
{
    public class WarewolfPerformanceCounterRegister : IWarewolfPerformanceCounterRegister
    {
        public  WarewolfPerformanceCounterRegister(IList<IPerformanceCounter> counters )
        {
            RegisterCountersOnMachine(counters);
            Counters = counters;
        }

        public IList<IPerformanceCounter> Counters { get; set; }
        public IList<IPerformanceCounter> DefaultCounters { get; set; }

        public void RegisterCountersOnMachine(IList<IPerformanceCounter> counters)
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
            CounterCreationDataCollection counterCreationDataCollection = new CounterCreationDataCollection();
            foreach(var counterl in counters)
            {
                foreach (var counter in counterl.CreationData())
                {   
                    counterCreationDataCollection.Add(counter);
                }
               
            }
            PerformanceCounterCategory.Create("Warewolf", "Warewolf Performance Counters", PerformanceCounterCategoryType.MultiInstance, counterCreationDataCollection);
        }
    }
}
