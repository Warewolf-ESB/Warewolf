using System;

namespace Dev2.Common.Interfaces.Monitoring
{
    public interface IWarewolfPerformanceCounterLocater
    {
        IPerformanceCounter GetCounter(string name);
        IPerformanceCounter GetCounter(WarewolfPerfCounterType type);
        IResourcePerformanceCounter GetCounter(Guid resourceId,string name);

    }

    public interface IPerformanceCounterFactory
    {
        IResourcePerformanceCounter CreateCounter(Guid resourceId, WarewolfPerfCounterType type, string name);
    }


}