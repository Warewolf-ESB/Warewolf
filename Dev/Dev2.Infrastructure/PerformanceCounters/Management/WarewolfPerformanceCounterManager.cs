﻿using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Common;
using Dev2.Common.Interfaces.Monitoring;
using Dev2.PerformanceCounters.Counters;

namespace Dev2.PerformanceCounters.Management
{
    public class WarewolfPerformanceCounterManager : IWarewolfPerformanceCounterLocater, Common.Interfaces.Monitoring.IPerformanceCounterFactory, IPerformanceCounterRepository
    {
        IList<IPerformanceCounter> _counters;
        readonly IPerformanceCounterPersistence _perf;
        IList<IPerformanceCounter> _resourceCounters;
        readonly IRealPerformanceCounterFactory _counterFactory;


        public WarewolfPerformanceCounterManager(IList<IPerformanceCounter> counters, IList<IResourcePerformanceCounter> resourceCounters, IWarewolfPerformanceCounterRegister register, IPerformanceCounterPersistence perf)
            : this(counters, resourceCounters, register, perf, new PerformanceCounterFactory())
        {
        }
        public WarewolfPerformanceCounterManager(IList<IPerformanceCounter> counters, IList<IResourcePerformanceCounter> resourceCounters, IWarewolfPerformanceCounterRegister register, IPerformanceCounterPersistence perf, IRealPerformanceCounterFactory performanceCounterFactory)
        {
            _counterFactory = performanceCounterFactory;
            _counters = counters;
            _perf = perf;
            _resourceCounters = resourceCounters.Cast<IPerformanceCounter>().ToList();
            EmptyCounter = new EmptyCounter();
        }

        public IPerformanceCounter GetCounter(string name)
        {
            var counter = _counters.FirstOrDefault(a => a.Name == name);
            
            if(null != counter) return counter.ToSafe();
            
            return EmptyCounter; // safely return EmptyCounter
        } 

        public IPerformanceCounter GetCounter(WarewolfPerfCounterType type)
        {
            var counter = _counters.FirstOrDefault(a => a.PerfCounterType == type);
            
            if(null != counter) return counter.ToSafe();

            return null; // not returning EmptyCounter as this overload is used by Tests so to fail it when it is null returning null here
        } 

        public IPerformanceCounter GetCounter(Guid resourceId, WarewolfPerfCounterType type)
        {
            try
            {
                var returnValue = _resourceCounters.Where(a => a is IResourcePerformanceCounter).Cast<IResourcePerformanceCounter>().FirstOrDefault(a => a.ResourceId == resourceId && a.PerfCounterType == type);
 
                if(null != returnValue) return returnValue.ToSafe();
                
                return EmptyCounter;
            }
            catch(Exception)
            {
                
                return  EmptyCounter;
            }
            
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
        public IResourcePerformanceCounter EmptyCounter { get; set; }

        #region Implementation of IPerformanceCounterFactory

        public IResourcePerformanceCounter CreateCounter(Guid resourceId, WarewolfPerfCounterType type, string name)
        {
            var foundCounter = GetCounter(resourceId, type);
            if (foundCounter == EmptyCounter)
            {
                IResourcePerformanceCounter counter = new EmptyCounter();

                switch (type)
                {
                    case WarewolfPerfCounterType.ExecutionErrors:
                        counter = new WarewolfNumberOfErrorsByResource(resourceId, name, _counterFactory);
                        break;
                    case WarewolfPerfCounterType.AverageExecutionTime:
                        counter = new WarewolfAverageExecutionTimePerformanceCounterByResource(resourceId, name, _counterFactory);
                        break;
                    case WarewolfPerfCounterType.ConcurrentRequests:
                        counter = new WarewolfCurrentExecutionsPerformanceCounterByResource(resourceId, name, _counterFactory);
                        break;
                    case WarewolfPerfCounterType.RequestsPerSecond:
                        counter = new WarewolfRequestsPerSecondPerformanceCounterByResource(resourceId, name, _counterFactory);
                        break;
                    case WarewolfPerfCounterType.ServicesNotFound:
                    case WarewolfPerfCounterType.NotAuthorisedErrors:
                        break;
                    default:
                        return counter;
                }

                _resourceCounters.Add(counter);
                _perf.Save(_resourceCounters, EnvironmentVariables.ServerResourcePerfmonSettingsFile);
                return counter;
            }
            return (IResourcePerformanceCounter)foundCounter.FromSafe();
        }

        #endregion

        #region Implementation of IPerformanceCounterRepository

        public IPerformanceCounterTo Counters => new PerformanceCounterTo(_counters,_resourceCounters);

        public void Save(IPerformanceCounterTo toSave)
        {
            var resourceCounters = toSave.ResourceCounters;
            var nativeCounters = toSave.NativeCounters;
            _perf.Save(nativeCounters);
            _perf.Save(resourceCounters);
            _counters = nativeCounters;
            _resourceCounters = resourceCounters.Cast<IPerformanceCounter>().ToList();
        }

        public void ResetCounters()
        {
            foreach(var performanceCounter in _resourceCounters)
            {
                performanceCounter.Reset();
            }

            foreach (var performanceCounter in _counters)
            {
                performanceCounter.Reset();
            }
        }

        #endregion
    }
}