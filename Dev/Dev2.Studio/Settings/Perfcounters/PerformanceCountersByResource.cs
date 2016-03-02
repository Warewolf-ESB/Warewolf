using System;
using Dev2.Common.Interfaces;

namespace Dev2.Settings.Perfcounters
{
    public class PerformanceCountersByResource : IPerformanceCountersByResource
    {
        private bool _isdeleted;
        public  Guid ResourceId { get;  set; }
        public string CounterName { get; set; }
        public bool RequestPerSecond { get; set; }
        public bool TotalErrors { get; set; }
        public bool AverageExecutionTime { get; set; }
        public bool ConcurrentRequests { get; set; }
        public bool IsDeleted
        {
            get
            {
                return _isdeleted;
            }
        }
    }
}