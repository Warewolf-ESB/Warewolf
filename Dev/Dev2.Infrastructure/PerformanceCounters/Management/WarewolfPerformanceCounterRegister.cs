#pragma warning disable
﻿using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Common;
using Dev2.Common.Interfaces.Monitoring;


namespace Dev2.PerformanceCounters.Management
{
    public class WarewolfPerformanceCounterRegister : IWarewolfPerformanceCounterRegister
    {
        readonly IPerformanceCounterCategory _performanceCounterCategory;

        public WarewolfPerformanceCounterRegister(IList<IPerformanceCounter> counters, IList<IResourcePerformanceCounter> resourcePerformanceCounters)
            : this(new PerformanceCounterCategoryWrapper(), counters, resourcePerformanceCounters)
        {
        }
        public WarewolfPerformanceCounterRegister(IPerformanceCounterCategory performanceCounterCategory, IList<IPerformanceCounter> counters, IList<IResourcePerformanceCounter> resourcePerformanceCounters)
        {
            _performanceCounterCategory = performanceCounterCategory;

            RegisterCountersOnMachine(counters, GlobalConstants.Warewolf);
            RegisterCountersOnMachine(resourcePerformanceCounters.Cast<IPerformanceCounter>().ToList(), GlobalConstants.WarewolfServices);
            Counters = counters;
            ResourceCounters = resourcePerformanceCounters;
        }

        public IList<IResourcePerformanceCounter> ResourceCounters { get; set; }

        public IList<IPerformanceCounter> Counters { get; set; }
        public IList<IPerformanceCounter> DefaultCounters { get; set; } // TODO: remove me?

        public void RegisterCountersOnMachine(IList<IPerformanceCounter> counters,string Category)
        {
            try
            {
                if (!_performanceCounterCategory.Exists(Category))
                {
                    CreateAllCounters(counters, Category);
                }
                else
                {
                    var cat = _performanceCounterCategory.New(Category);
                    if (!counters.All(a => cat.CounterExists(a.Name)))
                    {
                        _performanceCounterCategory.Delete(Category);
                        CreateAllCounters(counters, Category);

                    }
                }
            }
            catch (Exception e)
            {
                Dev2Logger.Error(e, GlobalConstants.WarewolfError);
            }

          

        }



        void CreateAllCounters(IEnumerable<IPerformanceCounter> counters, string category)
        {
            _performanceCounterCategory.Create(category, "Warewolf Performance Counters", counters);
        }
    }
}
