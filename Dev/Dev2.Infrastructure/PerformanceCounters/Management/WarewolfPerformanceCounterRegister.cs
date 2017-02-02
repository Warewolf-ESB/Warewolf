using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Dev2.Common;
using Dev2.Common.Interfaces.Monitoring;
// ReSharper disable MemberCanBePrivate.Global

namespace Dev2.PerformanceCounters.Management
{
    public class WarewolfPerformanceCounterRegister : IWarewolfPerformanceCounterRegister
    {
        // ReSharper disable once ParameterTypeCanBeEnumerable.Local
        public  WarewolfPerformanceCounterRegister(IList<IPerformanceCounter> counters, IList<IResourcePerformanceCounter> resourcePerformanceCounters  )
        {
            RegisterCountersOnMachine(counters, GlobalConstants.Warewolf);
            RegisterCountersOnMachine(resourcePerformanceCounters.Cast<IPerformanceCounter>().ToList(), GlobalConstants.WarewolfServices);
            Counters = counters;
            ResourceCounters = resourcePerformanceCounters;
        }

        public IList<IResourcePerformanceCounter> ResourceCounters { get; set; }

        public IList<IPerformanceCounter> Counters { get; set; }
        public IList<IPerformanceCounter> DefaultCounters { get; set; }

        public void RegisterCountersOnMachine(IList<IPerformanceCounter> counters,string category)
        {
            try
            {
                if (!PerformanceCounterCategory.Exists(category))
                {
                    CreateAllCounters(counters, category);
                }
                else
                {
                    PerformanceCounterCategory cat = new PerformanceCounterCategory(category);
                    if (!counters.All(a => cat.CounterExists(a.Name)))
                    {
                        PerformanceCounterCategory.Delete(category);
                        CreateAllCounters(counters, category);

                    }
                }
            }
            catch (Exception e)
            {
                Dev2Logger.Error(e);
            }

          

        }



        private static void CreateAllCounters(IEnumerable<IPerformanceCounter> counters, string category)
        {
            CounterCreationDataCollection counterCreationDataCollection = new CounterCreationDataCollection();
            foreach(var counterl in counters)
            {
                foreach (var counter in counterl.CreationData())
                {   
                    counterCreationDataCollection.Add(counter);
                }
               
            }
            PerformanceCounterCategory.Create(category, "Warewolf Performance Counters", PerformanceCounterCategoryType.MultiInstance, counterCreationDataCollection);
        }
    }
}
