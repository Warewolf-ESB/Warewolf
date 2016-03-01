using System;

namespace Dev2.Common.Interfaces
{
    public interface IPerformanceCountersByResource
    {
        Guid ResourceId { get; set; }
        string CounterName { get; set; }
        bool RequestPerSecond { get; set; }
        bool TotalErrors { get; set; }
        bool AverageExecutionTime { get; set; }
        bool ConcurrentRequests { get; set; }
    }
}