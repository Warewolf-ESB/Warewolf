namespace Dev2.Common.Interfaces.Monitoring
{
    public enum WarewolfPerfCounterType
    {
        ConcurrentRequests,
        RequestsPerSecond,
        AverageExecutionTime,
        ExecutionErrors,
        ServicesNotFound,
        NotAuthorisedErrors,
    }

    public static class WarewolfPerfCounterTypeExtensions
    {
    }
}