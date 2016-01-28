using Dev2.Common.Interfaces.Monitoring;

namespace Dev2.Diagnostics.PerformanceCounters
{
    public interface IWarewolfPerformanceCounterLocater
    {
        IPerformanceCounter GetCounter(string name);
        IPerformanceCounter GetCounter(WarewolfPerfCounterType type);
    }
}