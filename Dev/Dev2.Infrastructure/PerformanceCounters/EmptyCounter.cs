using System;
using System.Collections.Generic;
using System.Diagnostics;
using Dev2.Common.Interfaces.Monitoring;

namespace Dev2.PerformanceCounters
{
    public class EmptyCounter : IResourcePerformanceCounter
    {
        #region Implementation of IPerformanceCounter

        public void Increment()
        {
        }

        public void IncrementBy(long ticks)
        {
        }

        public void Decrement()
        {
        }

        public string Category { get;  set; }
        public string Name { get;  set; }
        public WarewolfPerfCounterType PerfCounterType { get;  set; }

        public IList<CounterCreationData> CreationData()
        {
            return null;
        }

        public bool IsActive { get; set; }

        #endregion

        #region Implementation of IResourcePerformanceCounter

        public Guid ResourceId { get;  set; }
        public string CategoryInstanceName { get;  set; }

        #endregion
    }
}