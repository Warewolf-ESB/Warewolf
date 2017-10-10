using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
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
                    case WarewolfPerfCounterType.AverageExecutionTime: current.AverageExecutionTime = true; break;
                    case WarewolfPerfCounterType.ExecutionErrors: current.TotalErrors = true; break;
                    case WarewolfPerfCounterType.ConcurrentRequests: current.ConcurrentRequests = true; break;
                    case WarewolfPerfCounterType.RequestsPerSecond: current.RequestPerSecond = true; break;
                    case WarewolfPerfCounterType.ServicesNotFound:
                        break;
                    case WarewolfPerfCounterType.NotAuthorisedErrors:
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

    public class CounterByResoureEqualityComparer : IEqualityComparer<IResourcePerformanceCounter>, IComparer
    {

        readonly int _direction;
        readonly string _sortMemberPath;

        public CounterByResoureEqualityComparer(ListSortDirection direction, string sortMemberPath)
        {
            VerifyArgument.IsNotNull("sortMemberPath", sortMemberPath);
            _direction = direction == ListSortDirection.Ascending ? 1 : -1;
            _sortMemberPath = sortMemberPath;
        }

        #region Implementation of IEqualityComparer<in IResourcePerformanceCounter>

        /// <summary>
        /// Determines whether the specified objects are equal.
        /// </summary>
        /// <returns>
        /// true if the specified objects are equal; otherwise, false.
        /// </returns>
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

        #region Implementation of IComparer

        /// <summary>
        /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
        /// </summary>
        /// <returns>
        /// A signed integer that indicates the relative values of <paramref name="x"/> and <paramref name="y"/>, as shown in the following table.Value Meaning Less than zero <paramref name="x"/> is less than <paramref name="y"/>. Zero <paramref name="x"/> equals <paramref name="y"/>. Greater than zero <paramref name="x"/> is greater than <paramref name="y"/>. 
        /// </returns>
        /// <param name="x">The first object to compare. </param><param name="y">The second object to compare. </param><exception cref="T:System.ArgumentException">Neither <paramref name="x"/> nor <paramref name="y"/> implements the <see cref="T:System.IComparable"/> interface.-or- <paramref name="x"/> and <paramref name="y"/> are of different types and neither one can handle comparisons with the other. </exception>
        public int Compare(object x, object y)
        {
            var px = x as IPerformanceCountersByResource;
            var py = y as IPerformanceCountersByResource;
            if (px == null || py == null)
            {
                return 1;
            }
            if (px.IsNew)
            {
                // px is greater than py
                return int.MaxValue;
            }
            if (py.IsNew)
            {
                // px is less than py
                return int.MinValue;
            }
            var result = 0;
            switch (_sortMemberPath)
            {
                case "CounterName":
                    result = String.Compare(px.CounterName, py.CounterName, StringComparison.InvariantCulture);
                    break;
                case "AverageExecutionTime":
                    result = px.AverageExecutionTime.CompareTo(py.AverageExecutionTime);
                    break;
                case "ConcurrentRequests":
                    result = px.ConcurrentRequests.CompareTo(py.ConcurrentRequests);
                    break;
                case "RequestPerSecond":
                    result = px.RequestPerSecond.CompareTo(py.RequestPerSecond);
                    break;
                case "TotalErrors":
                    result = px.TotalErrors.CompareTo(py.RequestPerSecond);
                    break;
                default:
                    break;
            }
            return _direction * result;
        }

        #endregion
    }
}
