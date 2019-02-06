using System;
using System.Collections.Generic;

namespace Dev2.Common.Interfaces.Monitoring
{
    public enum PerformanceCounterType
    {
        NumberOfItems32,            // PerformanceCounterType.NumberOfItems32
        AverageTimer32,             // PerformanceCounterType.AverageTimer32
        AverageBase,                // PerformanceCounterType.AverageBase
        RateOfCountsPerSecond32,    // PerformanceCounterType.RateOfCountsPerSecond32
    }

    public interface IPerformanceCounter : IDisposable
    {
        void Increment();
        void IncrementBy(long ticks);
        void Decrement();
        string Category { get;}
        string Name { get; }
        WarewolfPerfCounterType PerfCounterType { get; }
        IEnumerable<(string CounterName, string CounterHelp, PerformanceCounterType CounterType)> CreationData();
        bool IsActive { get; set; }
        void Setup();
        void Reset();
    }

    public interface IResourcePerformanceCounter : IPerformanceCounter
    {
        Guid ResourceId { get;  }
        string CategoryInstanceName { get; }
    }
}
