using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Common;
using Dev2.Common.Interfaces.Monitoring;

namespace Dev2.PerformanceCounters
{
    

    public class WarewolfPerformanceCounterLocater : IWarewolfPerformanceCounterLocater,IPerformanceCounterFactory
    {
        private readonly IList<IPerformanceCounter> _counters;
        private readonly IWarewolfPerformanceCounterRegister _register;
        private readonly PerformanceCounterPersistence _perf;
        private readonly IList<IPerformanceCounter> _resourceCounters;

        public WarewolfPerformanceCounterLocater(IList<IPerformanceCounter> counters, IWarewolfPerformanceCounterRegister register, PerformanceCounterPersistence perf)
        {
            _counters = counters;
            _register = register;
            _perf = perf;
            _resourceCounters = new List<IPerformanceCounter>();
            EmptyCounter = new EmptyCounter();
        }

        public IPerformanceCounter GetCounter(string name)
        {
            return _counters.First(a => a.Name == name);
        }

        public IPerformanceCounter GetCounter(WarewolfPerfCounterType type)
        {
            return _counters.First(a => a.PerfCounterType == type);
        }

        public IResourcePerformanceCounter GetCounter(Guid resourceId, string name)
        {
            return _counters.Where(a=> a is IResourcePerformanceCounter).Cast<IResourcePerformanceCounter>().First(a=>a.ResourceId == resourceId && a.Name==name);
        }

        public IResourcePerformanceCounter GetCounter(Guid resourceId, WarewolfPerfCounterType type)
        {
            try
            {
                var returnValue = _resourceCounters.Where(a => a is IResourcePerformanceCounter).Cast<IResourcePerformanceCounter>().First(a => a.ResourceId == resourceId && a.PerfCounterType==type);
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
            IResourcePerformanceCounter counter;
            switch(type)
            {
                    case WarewolfPerfCounterType.ExecutionErrors: 
                        counter =  new WarewolfNumberOfErrorsByResource(resourceId,name);
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
                default :
                        counter = new EmptyCounter();
                    break;
            }
            _register.RegisterCounter(counter);
            _resourceCounters.Add(counter);
            _perf.Save(_resourceCounters, EnvironmentVariables.ServerResourcePerfmonSettingsFile);
            return counter;
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
    }
}