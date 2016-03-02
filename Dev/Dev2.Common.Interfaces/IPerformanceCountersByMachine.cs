namespace Dev2.Common.Interfaces
{
    public interface IPerformanceCountersByMachine
    {
        string MachineName { get; set; }
        bool RequestPerSecond { get; set; }
        bool TotalErrors { get; set; }
        bool AverageExecutionTime { get; set; }
        bool ConcurrentRequests { get; set; }
        bool WorkFlowsNotFound { get; set; }
        bool NotAuthorisedErrors { get; set; }
    }
}