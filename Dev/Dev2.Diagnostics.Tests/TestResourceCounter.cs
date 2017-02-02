using System;
using System.Collections.Generic;
using System.Diagnostics;
using Dev2.Common.Interfaces.Monitoring;

namespace Dev2.Diagnostics.Test
{
    public class TestResourceCounter : IResourcePerformanceCounter {
        #region Implementation of IPerformanceCounter

        public TestResourceCounter()
        {
            Category = "";
            Name = "bob";
            PerfCounterType = WarewolfPerfCounterType.AverageExecutionTime;
            ResourceId = Guid.NewGuid();
            CategoryInstanceName = "bobcat";
        }

        public TestResourceCounter(WarewolfPerfCounterType warewolfPerfCounterType, Guid resourceId)
        {
            Category = "";
            Name = "bob";
            PerfCounterType = warewolfPerfCounterType;
            ResourceId = resourceId;
            CategoryInstanceName = "bobcat";
        }

        public void Increment()
        {
        }

        public void IncrementBy(long ticks)
        {
        }

        public void Decrement()
        {
        }

        public string Category { get; private set; }
        public string Name { get; private set; }
        public WarewolfPerfCounterType PerfCounterType { get; private set; }

        public IList<CounterCreationData> CreationData()
        {
            return null;
        }

        public bool IsActive { get; set; }

        public void Setup()
        {
        }

        #endregion

        #region Implementation of IResourcePerformanceCounter

        public Guid ResourceId { get; private set; }
        public string CategoryInstanceName { get; private set; }

        #endregion
        public void Reset()
        {
        }
    }
}