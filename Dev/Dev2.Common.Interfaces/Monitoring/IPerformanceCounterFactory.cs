using System;

namespace Dev2.Common.Interfaces.Monitoring
{
    public interface IPerformanceCounterFactory
    {
        IResourcePerformanceCounter CreateCounter(Guid resourceId, WarewolfPerfCounterType type, string name);
    }
}