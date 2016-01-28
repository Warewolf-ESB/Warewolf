using System.Collections.Generic;
using System.Diagnostics;

namespace Dev2.Common.Interfaces.Monitoring
{
    public interface IPerformanceCounter
    {
        void Increment();
        void IncrementBy(long ticks);
        void Decrement();
        string Category { get;}
        string Name { get; }
        WarewolfPerfCounterType PerfCounterType { get; }
        IList<CounterCreationData> CreationData();
        bool IsActive { get; set; }
    }

    public enum WarewolfPerfCounterType
    {
        ConcurrentRequests,
        RequestsPerSecond,
        AverageExecutionTime,
        ExecutionErrors,
        ServicesNotFound,
        NotAuthorisedErrors,
        WorkflowExecutionTime
    }
}
