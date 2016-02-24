using System;

namespace Dev2.Common.Interfaces.Monitoring
{
    public interface IWarewolfPerformanceCounterLocater
    {
        IPerformanceCounter GetCounter(string name);
        IPerformanceCounter GetCounter(WarewolfPerfCounterType type);

        IPerformanceCounter GetCounter(Guid resourceId, WarewolfPerfCounterType type);
    }

    public interface IPerformanceCounterFactory
    {
        IResourcePerformanceCounter CreateCounter(Guid resourceId, WarewolfPerfCounterType type, string name);

        void RemoverCounter(Guid resourceId, WarewolfPerfCounterType type, string name);
    }


}