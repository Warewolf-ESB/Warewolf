using System;
using System.Collections.Generic;
using System.Diagnostics;
using Dev2.Common;
using Dev2.Common.Interfaces.Monitoring;

namespace Dev2.PerformanceCounters
{
    public class WarewolfRequestsPerSecondPerformanceCounterByResource : IResourcePerformanceCounter
    {

        private PerformanceCounter _counter;
        private Stopwatch _stopwatch;

        // ReSharper disable once InconsistentNaming
        private const WarewolfPerfCounterType _perfCounterType = WarewolfPerfCounterType.RequestsPerSecond;
        public WarewolfRequestsPerSecondPerformanceCounterByResource(Guid resourceId, string categoryInstanceName)
        {
            ResourceId = resourceId;
            CategoryInstanceName = categoryInstanceName;
            IsActive = true;
        }

        private void Setup()
        {


            if (_counter == null)
            {
                _stopwatch = new Stopwatch();
                _stopwatch.Start();
                _counter = new PerformanceCounter(GlobalConstants.Warewolf, Name, CategoryInstanceName)
                {
                    MachineName = ".",
                    ReadOnly = false,
                    InstanceLifetime = PerformanceCounterInstanceLifetime.Global
              
                };
            }
        }

        #region Implementation of IPerformanceCounter

        public void Increment()
        {
            if (IsActive)
                try
                {
                    Setup();
                    _counter.Increment();
                }

                catch (Exception err)
                {

                    Dev2Logger.Error(err);
                }
        }

        public void IncrementBy(long ticks)
        {
            if (IsActive)
                try
                {
                    Setup();

                    _counter.IncrementBy(ticks);
                }

                catch (Exception err)
                {

                    Dev2Logger.Error(err);
                }
        }

        public void Decrement()
        {
            if (IsActive)
            {
                Setup();
                try
                {
                    _counter.Decrement();
                }
                catch (Exception err)
                {

                    Dev2Logger.Error(err);
                }
            }
        }

        public string Category
        {
            get
            {
                return "Warewolf";
            }
        }
        public string Name
        {
            get
            {
                return "Request Per Second";
            }
        }
        public WarewolfPerfCounterType PerfCounterType
        {
            get
            {
                return _perfCounterType;
            }
        }

        public IList<CounterCreationData> CreationData()
        {

            CounterCreationData totalOps = new CounterCreationData
            {
                CounterName = Name,
                CounterHelp = Name,
                CounterType = PerformanceCounterType.RateOfCountsPerSecond32
            };
            return new[] { totalOps };
        }

        public bool IsActive { get; set; }

        #endregion

        #region Implementation of IResourcePerformanceCounter

        public Guid ResourceId { get; private set; }
        public string CategoryInstanceName { get; private set; }

        #endregion
    }
}