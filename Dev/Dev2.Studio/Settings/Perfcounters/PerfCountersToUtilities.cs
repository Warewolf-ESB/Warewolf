using System.Collections.Generic;
using System.Linq;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Monitoring;
using Dev2.PerformanceCounters.Counters;


namespace Dev2.Settings.Perfcounters
{
    public static class PerfCountersToUtilities
    {
        public  static  IList<IPerformanceCountersByMachine> GetServerCountersTo( this IPerformanceCounterTo to)
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

        public static IList<IPerformanceCountersByResource> GetResourceCountersTo(this IPerformanceCounterTo to)
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
                switch (resourcePerformanceCounter.PerfCounterType)
                {
                    case WarewolfPerfCounterType.AverageExecutionTime:
                        current.AverageExecutionTime = true;
                        break;
                    case WarewolfPerfCounterType.ExecutionErrors:
                        current.TotalErrors = true;
                        break;
                    case WarewolfPerfCounterType.ConcurrentRequests:
                        current.ConcurrentRequests = true;
                        break;
                    case WarewolfPerfCounterType.RequestsPerSecond:
                        current.RequestPerSecond = true;
                        break;
                    default:
                        break;
                }
            }

            return res;
        }

        public static IList<IResourcePerformanceCounter> GetResourceCounters(this List<IPerformanceCountersByResource> to)
        {
            var res = new List<IResourcePerformanceCounter>();
            var performanceCountersByResources = to.Where(resource => !resource.IsDeleted && !string.IsNullOrEmpty(resource.CounterName));
            foreach (var resourcePerformanceCounter in performanceCountersByResources)
            {
                if (resourcePerformanceCounter.TotalErrors)
                {
                    var counter = new WarewolfNumberOfErrorsByResource(resourcePerformanceCounter.ResourceId,resourcePerformanceCounter.CounterName);
                    res.Add(counter);
                }
                if (resourcePerformanceCounter.AverageExecutionTime)
                {
                    var counter = new WarewolfAverageExecutionTimePerformanceCounterByResource(resourcePerformanceCounter.ResourceId, resourcePerformanceCounter.CounterName);
                    res.Add(counter);
                }
                if (resourcePerformanceCounter.ConcurrentRequests)
                {
                    var counter = new WarewolfCurrentExecutionsPerformanceCounterByResource(resourcePerformanceCounter.ResourceId, resourcePerformanceCounter.CounterName);
                    res.Add(counter);
                }
                if (resourcePerformanceCounter.RequestPerSecond)
                {
                    var counter = new WarewolfRequestsPerSecondPerformanceCounterByResource(resourcePerformanceCounter.ResourceId, resourcePerformanceCounter.CounterName);
                    res.Add(counter);
                }
            }
            return res;
        }
 
        
    }
}
