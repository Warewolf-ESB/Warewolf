using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Dev2.Common.Interfaces.Monitoring;

namespace Dev2.Diagnostics.PerformanceCounters
{
    public class WarewolfPerformanceCounterBuilder
    {
        private IList<IPerformanceCounter> _counters;

        public  WarewolfPerformanceCounterBuilder(IList<IPerformanceCounter> counters )
        {
            CreateCounters(counters);
            _counters = counters;
        }

        public IList<IPerformanceCounter> Counters
        {
            get
            {
                return _counters;
            }
            set
            {
                _counters = value;
            }
        }

        private void CreateCounters(IList<IPerformanceCounter> counters)
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

        private static void CreateAllCounters(IList<IPerformanceCounter> counters)
        {
            CounterCreationDataCollection coll = new CounterCreationDataCollection();
            foreach(var counter in counters)
            {
                coll.Add(counter.CreationData());
            }
            PerformanceCounterCategory.Create("Warewolf", "Warewolf Performance Counters", PerformanceCounterCategoryType.SingleInstance, coll);
        }
    }
}
