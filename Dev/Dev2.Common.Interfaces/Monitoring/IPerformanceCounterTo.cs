using System.Collections.Generic;

namespace Dev2.Common.Interfaces.Monitoring
{
    public interface IPerformanceCounterTo
    {
        IList<IPerformanceCounter> NativeCounters { get; set; }

        IList<IResourcePerformanceCounter> ResourceCounters { get; set; } 
    }
}