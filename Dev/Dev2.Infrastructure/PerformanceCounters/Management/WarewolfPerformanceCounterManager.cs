using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Common;
using Dev2.Common.Interfaces.Monitoring;
using Dev2.PerformanceCounters.Counters;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedParameter.Local

namespace Dev2.PerformanceCounters.Management
{
    

    public class WarewolfPerformanceCounterManager : IWarewolfPerformanceCounterLocater,IPerformanceCounterFactory,IPerformanceCounterRepository
    {
        private readonly IList<IPerformanceCounter> _counters;
        private readonly IPerformanceCounterPersistence _perf;
        private readonly IList<IPerformanceCounter> _resourceCounters;

        // ReSharper disable once ParameterTypeCanBeEnumerable.Local
        public WarewolfPerformanceCounterManager(IList<IPerformanceCounter> counters, IList<IResourcePerformanceCounter> resourceCounters, IWarewolfPerformanceCounterRegister register, IPerformanceCounterPersistence perf)
        {
            _counters = counters;
            _perf = perf;
            _resourceCounters = resourceCounters.Cast<IPerformanceCounter>().ToList();
            EmptyCounter = new EmptyCounter();
        }

        public IPerformanceCounter GetCounter(string name)
        {
            return _counters.First(a => a.Name == name).ToSafe();
        }

        public IPerformanceCounter GetCounter(WarewolfPerfCounterType type)
        {
            return _counters.First(a => a.PerfCounterType == type).ToSafe();
        }



        public IPerformanceCounter GetCounter(Guid resourceId, WarewolfPerfCounterType type)
        {
            try
            {
                var returnValue = _resourceCounters.Where(a => a is IResourcePerformanceCounter).Cast<IResourcePerformanceCounter>().First(a => a.ResourceId == resourceId && a.PerfCounterType==type).ToSafe();
                return returnValue;
            }
            catch(Exception)
            {
                
                return  EmptyCounter;
            }
            
        }

        public IResourcePerformanceCounter EmptyCounter { get; set; }

        #region Implementation of IPerformanceCounterFactory

        public IResourcePerformanceCounter CreateCounter(Guid resourceId, WarewolfPerfCounterType type, string name)
        {
            if (GetCounter(resourceId, type) == EmptyCounter)
            {
                IResourcePerformanceCounter counter;
                switch (type)
                {
                    case WarewolfPerfCounterType.ExecutionErrors:
                        counter = new WarewolfNumberOfErrorsByResource(resourceId, name);
                        break;
                    case WarewolfPerfCounterType.AverageExecutionTime:
                        counter = new WarewolfAverageExecutionTimePerformanceCounterByResource(resourceId, name);
                        break;
                    case WarewolfPerfCounterType.ConcurrentRequests:
                        counter = new WarewolfCurrentExecutionsPerformanceCounterByResource(resourceId, name);
                        break;
                    case WarewolfPerfCounterType.RequestsPerSecond:
                        counter = new WarewolfRequestsPerSecondPerformanceCounterByResource(resourceId, name);
                        break;
                    default:
                        return new EmptyCounter();
                }

                _resourceCounters.Add(counter);
                _perf.Save(_resourceCounters, EnvironmentVariables.ServerResourcePerfmonSettingsFile);
                return counter;
            }
            return (IResourcePerformanceCounter)GetCounter(resourceId, type).FromSafe();
        }
        public void RemoverCounter(Guid resourceId, WarewolfPerfCounterType type, string name)
        {
            var toRemove = _resourceCounters.FirstOrDefault(a =>
            {
                var resourcePerformanceCounter = a as IResourcePerformanceCounter;
                return resourcePerformanceCounter != null && (resourcePerformanceCounter.ResourceId == resourceId && resourcePerformanceCounter.PerfCounterType == type);
            });

            if (toRemove != null)
            {
                _resourceCounters.Remove(toRemove);
                _perf.Save(_resourceCounters, EnvironmentVariables.ServerResourcePerfmonSettingsFile);
            }
       
        }
        #endregion

        #region Implementation of IPerformanceCounterRepository

        public IPerformanceCounterTo Counters { get{ return new PerformanceCounterTo(_counters,_resourceCounters);}}

        public void Save(IPerformanceCounterTo toSave)
        {
        }

        #endregion
    }
}