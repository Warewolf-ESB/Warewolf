using System;
using System.ComponentModel;
using System.Windows.Input;

namespace Dev2.Common.Interfaces
{
    public interface IPerformanceCounters : INotifyPropertyChanged
    {
    }

    public interface IPerformanceCountersByResource : IPerformanceCounters
    {
        Guid ResourceId { get; set; }
        string CounterName { get; set; }
        bool RequestPerSecond { get; set; }
        bool TotalErrors { get; set; }
        bool AverageExecutionTime { get; set; }
        bool ConcurrentRequests { get; set; }
        bool IsDeleted { get; set; }
        ICommand RemoveRow { get; }
        bool CanRemove { get; }
        bool IsNew { get; set; }
    }
}