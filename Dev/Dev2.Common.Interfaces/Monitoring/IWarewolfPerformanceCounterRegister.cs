using System.Collections.Generic;

namespace Dev2.Common.Interfaces.Monitoring
{
    public interface IWarewolfPerformanceCounterRegister
    {
        IList<IPerformanceCounter> Counters { get; set; }
        IList<IPerformanceCounter> DefaultCounters { get; set; }
        void RegisterCountersOnMachine(IList<IPerformanceCounter> counters, string Category);


    }
}