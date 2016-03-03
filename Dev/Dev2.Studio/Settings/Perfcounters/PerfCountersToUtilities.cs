using System.Collections.Generic;
using System.Linq;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Monitoring;

namespace Dev2.Settings.Perfcounters
{
    public static class PerfCountersToUtilities
    {
        public  static  IList<IPerformanceCountersByMachine> FromTo( this IPerformanceCounterTo to)
        {
            var result = new PerformanceCountersByMachine
            {
                AverageExecutionTime = to.NativeCounters.Any(a => a.PerfCounterType == WarewolfPerfCounterType.AverageExecutionTime && a.IsActive),
                ConcurrentRequests = to.NativeCounters.Any(a => a.PerfCounterType == WarewolfPerfCounterType.ConcurrentRequests && a.IsActive),
                NotAuthorisedErrors = to.NativeCounters.Any(a => a.PerfCounterType == WarewolfPerfCounterType.NotAuthorisedErrors && a.IsActive),
                RequestPerSecond = to.NativeCounters.Any(a => a.PerfCounterType == WarewolfPerfCounterType.RequestsPerSecond && a.IsActive),
                TotalErrors = to.NativeCounters.Any(a => a.PerfCounterType == WarewolfPerfCounterType.ExecutionErrors && a.IsActive),
                WorkFlowsNotFound = to.NativeCounters.Any(a => a.PerfCounterType == WarewolfPerfCounterType.ServicesNotFound && a.IsActive)
            };
            return new List<IPerformanceCountersByMachine>{ result};
        }

        public static IList<IPerformanceCountersByResource> FromTO(this IPerformanceCounterTo to)
        {

            var res = new List<IPerformanceCountersByResource>();
            var ids = to.ResourceCounters;
            foreach(var resourcePerformanceCounter in ids)
            {
                var current = res.FirstOrDefault(a => a.ResourceId == resourcePerformanceCounter.ResourceId);
                if (current == null)
                {
                    current = new PerformanceCountersByResource { ResourceId = resourcePerformanceCounter.ResourceId, CounterName = resourcePerformanceCounter.CategoryInstanceName };
                    res.Add(current);
                }
                switch(resourcePerformanceCounter.PerfCounterType)
                {
                    case WarewolfPerfCounterType.AverageExecutionTime: current.AverageExecutionTime = true; break;
                    case WarewolfPerfCounterType.ExecutionErrors: current.TotalErrors = true; break;
                    case WarewolfPerfCounterType.ConcurrentRequests: current.ConcurrentRequests = true; break;
                    case WarewolfPerfCounterType.RequestsPerSecond: current.RequestPerSecond = true; break;

                }
            }

            return res;
        }
 
        
    }

    public class CounterByResoureEqualityComparer : IEqualityComparer<IResourcePerformanceCounter>
    {
        #region Implementation of IEqualityComparer<in IResourcePerformanceCounter>

        /// <summary>
        /// Determines whether the specified objects are equal.
        /// </summary>
        /// <returns>
        /// true if the specified objects are equal; otherwise, false.
        /// </returns>
        /// <param name="x">The first object of type <paramref name="T"/> to compare.</param><param name="y">The second object of type <paramref name="T"/> to compare.</param>
        public bool Equals(IResourcePerformanceCounter x, IResourcePerformanceCounter y)
        {
            return x.ResourceId == y.ResourceId && x.PerfCounterType == y.PerfCounterType;
        }

        /// <summary>
        /// Returns a hash code for the specified object.
        /// </summary>
        /// <returns>
        /// A hash code for the specified object.
        /// </returns>
        /// <param name="obj">The <see cref="T:System.Object"/> for which a hash code is to be returned.</param><exception cref="T:System.ArgumentNullException">The type of <paramref name="obj"/> is a reference type and <paramref name="obj"/> is null.</exception>
        public int GetHashCode(IResourcePerformanceCounter obj)
        {
            return obj.ResourceId.GetHashCode() ^obj.PerfCounterType.GetHashCode()  ;
        }

        #endregion
    }
}
