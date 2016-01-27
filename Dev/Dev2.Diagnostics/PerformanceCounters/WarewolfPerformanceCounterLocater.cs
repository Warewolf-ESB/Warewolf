using System.Collections.Generic;
using System.Linq;
using Dev2.Common.Interfaces.Monitoring;

namespace Dev2.Diagnostics.PerformanceCounters
{
    public class WarewolfPerformanceCounterLocater : IWarewolfPerformanceCounterLocater
    {
        private readonly IList<IPerformanceCounter> _counters;

        public WarewolfPerformanceCounterLocater(IList<IPerformanceCounter> counters)
        {
            _counters = counters;
        }

        public IPerformanceCounter GetCounter(string name)
        {
            return _counters.First(a => a.Name == name);
        }

    }
}