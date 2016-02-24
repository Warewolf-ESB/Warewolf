using System;
using System.Collections.Generic;
using System.Diagnostics;
using Dev2.Common;
using Dev2.Common.Interfaces.Monitoring;

namespace Dev2.PerformanceCounters
{
    public class WarewolfAverageExecutionTimePerformanceCounter : IPerformanceCounter
    {

        private PerformanceCounter _counter;
        private PerformanceCounter _baseCounter;
        private bool _started;
        private readonly WarewolfPerfCounterType _perfCounterType;

        public WarewolfAverageExecutionTimePerformanceCounter()
        {
            _started = false;
            IsActive = true;
            _perfCounterType = WarewolfPerfCounterType.AverageExecutionTime;
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
            try
            {
                Setup();
                if (IsActive)
                {
                    _counter.Increment();
                    _baseCounter.Increment();
                }
            }
            catch (Exception exception)
            {
                Dev2Logger.Error(exception);
            }
        }

        public void IncrementBy(long ticks)
        {
            try
            {
                Setup();
                if (IsActive)
                {
                    _counter.IncrementBy(ticks);
                    _baseCounter.Increment();
                }
            }
            catch (Exception exception)
            {
                Dev2Logger.Error(exception);
            }
        }

        private void Setup()
        {
            if (!_started)
            {
                _counter = new PerformanceCounter(GlobalConstants.Warewolf, Name,GlobalConstants.GlobalCounterName)
                {
                    MachineName = ".",
                    ReadOnly = false,
                

                };
                _baseCounter = new PerformanceCounter(GlobalConstants.Warewolf, "average time per operation base", GlobalConstants.GlobalCounterName)
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
            Setup();
            if (IsActive)
            {
                try
                {
                    _counter.Decrement();
                    _baseCounter.Decrement();
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
                return "Average workflow execution time";
            }
        }

        #endregion
    }
}