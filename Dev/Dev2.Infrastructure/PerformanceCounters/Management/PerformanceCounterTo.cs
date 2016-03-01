using System.Collections.Generic;
using System.Linq;
using Dev2.Common.Interfaces.Monitoring;

namespace Dev2.PerformanceCounters.Management
{
    public class PerformanceCounterTo : IPerformanceCounterTo
    {
        public PerformanceCounterTo(IEnumerable<IPerformanceCounter> counters, IEnumerable<IPerformanceCounter> resourceCounters)
        {
            NativeCounters = counters.ToList();
            ResourceCounters = resourceCounters.Cast<IResourcePerformanceCounter>().ToList();
        }

        #region Implementation of IPerformanceCounterTo

        public IList<IPerformanceCounter> NativeCounters { get;  set; }
        public IList<IResourcePerformanceCounter> ResourceCounters { get;  set; }

        #endregion
    }
}