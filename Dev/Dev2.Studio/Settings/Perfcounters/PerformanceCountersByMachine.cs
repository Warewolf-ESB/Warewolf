using Dev2.Common.Interfaces;

namespace Dev2.Settings.Perfcounters
{
    public class PerformanceCountersByMachine : IPerformanceCountersByMachine
    {
        public string MachineName { get; set; }
        public bool RequestPerSecond { get; set; }
        public bool TotalErrors { get; set; }
        public bool AverageExecutionTime { get; set; }
        public bool ConcurrentRequests { get; set; }
        public bool WorkFlowsNotFound { get; set; }
        public bool NotAuthorisedErrors { get; set; }
    }
}