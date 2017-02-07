using System;
using System.Collections.Generic;
using System.Diagnostics;
using Dev2.Common;
using Dev2.Common.Interfaces.Monitoring;

namespace Dev2.PerformanceCounters.Counters
{
    public class WarewolfAverageExecutionTimePerformanceCounterByResource : IResourcePerformanceCounter
    {

        private PerformanceCounter _counter;
        private PerformanceCounter _baseCounter;
        private bool _started;
        private readonly WarewolfPerfCounterType _perfCounterType;

        public WarewolfAverageExecutionTimePerformanceCounterByResource(Guid resourceId, string categoryInstanceName)
        {
            ResourceId = resourceId;
            CategoryInstanceName = categoryInstanceName;
            _started = false;
            IsActive = true;
            _perfCounterType = WarewolfPerfCounterType.AverageExecutionTime;
        }

        public WarewolfPerfCounterType PerfCounterType => _perfCounterType;

        public IList<CounterCreationData> CreationData()
        {




            CounterCreationData totalOps = new CounterCreationData
            {
                CounterName = Name,
                CounterHelp = Name,
                CounterType = PerformanceCounterType.AverageTimer32,


            };
            CounterCreationData avgDurationBase = new CounterCreationData
            {
                CounterName = "average time per operation base",
                CounterHelp = "Average duration per operation execution base",
                CounterType = PerformanceCounterType.AverageBase
            };

            return new[] { totalOps, avgDurationBase };

        }

        public bool IsActive { get; set; }

        #region Implementation of IPerformanceCounter

        public void Increment()
        {

                if (IsActive)
                {
                    _counter.Increment();
                    _baseCounter.Increment();
                }
        }

        public void IncrementBy(long ticks)
        {

                if (IsActive)
                {
                    _counter.IncrementBy(ticks);
                    _baseCounter.Increment();
                }
     
        }

        public void Setup()
        {
            if (!_started)
            {
                _counter = new PerformanceCounter(GlobalConstants.WarewolfServices, Name,CategoryInstanceName)
                {
                    MachineName = ".",
                    ReadOnly = false,
                

                };
                _baseCounter = new PerformanceCounter(GlobalConstants.WarewolfServices, "average time per operation base", CategoryInstanceName)
                {
                    MachineName = ".",
                    ReadOnly = false,
                    InstanceLifetime = PerformanceCounterInstanceLifetime.Global

                };
                _started = true;
            }
        }

        public void Decrement()
        {
            
            if (IsActive)
            {
                    _counter.Decrement();
                    _baseCounter.Decrement();
                    
            }
        }

        public string Category => GlobalConstants.WarewolfServices;

        public string Name => "Average workflow execution time";

        public void Reset()
        {
            if (_counter != null)
            {
                _counter.RawValue = 0;
            }
            if (_baseCounter != null)
            {
                _baseCounter.RawValue = 0;
            }
        }
        #endregion

        #region Implementation of IResourcePerformanceCounter

        public Guid ResourceId { get; private set; }
        public string CategoryInstanceName { get; private set; }

        #endregion
    }
}