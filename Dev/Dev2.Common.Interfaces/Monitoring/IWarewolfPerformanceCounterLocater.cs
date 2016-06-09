using System;

namespace Dev2.Common.Interfaces.Monitoring
{
    public interface IWarewolfPerformanceCounterLocater
    {
        IPerformanceCounter GetCounter(string name);

        IPerformanceCounter GetCounter(Guid resourceId, WarewolfPerfCounterType type);
    }


}