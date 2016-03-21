using System.Diagnostics;

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
        public  static PerformanceCounterType ToSystemType( this WarewolfPerfCounterType type)
        {
            switch(type)
            {
                case WarewolfPerfCounterType.AverageExecutionTime: return PerformanceCounterType.AverageTimer32;
                case WarewolfPerfCounterType.ConcurrentRequests: return PerformanceCounterType.NumberOfItems32;
                case WarewolfPerfCounterType.ExecutionErrors: return PerformanceCounterType.NumberOfItems32;
                case WarewolfPerfCounterType.RequestsPerSecond: return PerformanceCounterType.RateOfCountsPerSecond32;
                case WarewolfPerfCounterType.NotAuthorisedErrors: return PerformanceCounterType.NumberOfItems32;
                case WarewolfPerfCounterType.ServicesNotFound: return PerformanceCounterType.NumberOfItems32;
                default :return PerformanceCounterType.NumberOfItems32;
            } 
        }
    }
}