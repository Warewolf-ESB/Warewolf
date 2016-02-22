using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces.Monitoring;

namespace Dev2.PerformanceCounters
{
    public class WarewolfPerfCounterSetting
    {
       public string Name { get; set; }
       public Guid ResourceId { get; set; }
       public bool Enabled { get; set; }
    }

    public class  WarewolfPerfCounterSettings
    {
        public IList<IPerformanceCounter> BuildFromFile()
        {
            return null;
        }
        List<WarewolfPerfCounterSetting> Settings { get; set; }
    }
}
