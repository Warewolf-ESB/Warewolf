using Dev2.Common.Interfaces.Monitoring;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Dev2.PerformanceCounters
{
    class PerformanceCounterCategoryWrapper : IPerformanceCounterCategory
    {
        private PerformanceCounterCategory _category;

        public bool CounterExists(string name) => _category.CounterExists(name);
        public void Create(string category, string categoryHelp, IEnumerable<IPerformanceCounter> counters) {
            var counterCreationDataCollection = new CounterCreationDataCollection();
            foreach (var counterl in counters)
            {
                foreach (var counterData in counterl.CreationData())
                {
                    var systemCounterData = new CounterCreationData(
                        counterData.CounterName,
                        counterData.CounterHelp,
                        CounterTypeAsSystemDiagnosticsType(counterData.CounterType)
                    );

                    counterCreationDataCollection.Add(systemCounterData);
                }

            }
            PerformanceCounterCategory.Create(category, categoryHelp, PerformanceCounterCategoryType.MultiInstance, counterCreationDataCollection);
        }

        public void Delete(string category) => PerformanceCounterCategory.Delete(category);
        public bool Exists(string category) => PerformanceCounterCategory.Exists(category);
        public IPerformanceCounterCategory New(string category)
        {
            var result = new PerformanceCounterCategoryWrapper
            {
                _category = new PerformanceCounterCategory(category)
            };
            return result;
        }

        public static System.Diagnostics.PerformanceCounterType CounterTypeAsSystemDiagnosticsType(Common.Interfaces.Monitoring.PerformanceCounterType counterType)
        {
            switch (counterType)
            {
                case Common.Interfaces.Monitoring.PerformanceCounterType.AverageBase:
                    return System.Diagnostics.PerformanceCounterType.AverageBase;
                case Common.Interfaces.Monitoring.PerformanceCounterType.AverageTimer32:
                    return System.Diagnostics.PerformanceCounterType.AverageTimer32;
                case Common.Interfaces.Monitoring.PerformanceCounterType.NumberOfItems32:
                    return System.Diagnostics.PerformanceCounterType.NumberOfItems32;
                case Common.Interfaces.Monitoring.PerformanceCounterType.RateOfCountsPerSecond32:
                    return System.Diagnostics.PerformanceCounterType.RateOfCountsPerSecond32;
                default:
                    throw new Exception($"unhandled PerformanceCounterType: {counterType}");
            }
        }

    }
}
