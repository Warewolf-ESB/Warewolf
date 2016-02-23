using System;
using System.Collections.Generic;
using System.Diagnostics;
using Dev2.Common;
using Dev2.Common.Interfaces.Monitoring;

namespace Dev2.PerformanceCounters
{
    public class WarewolfNumberOfErrors : IPerformanceCounter
    {

        private PerformanceCounter _counter;
        private bool _started;
        private readonly WarewolfPerfCounterType _perfCounterType;

        public WarewolfNumberOfErrors()
        {
            _started = false;
            IsActive = true;
            _perfCounterType = WarewolfPerfCounterType.ExecutionErrors;
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
                CounterType = PerformanceCounterType.NumberOfItems32
            };
            return new[] { totalOps };
        }

        public bool IsActive { get; set; }

        #region Implementation of IPerformanceCounter

        public void Increment()
        {
            try
            {
                Setup();
                if (IsActive)
                    _counter.Increment();
            }

            catch (Exception err)
            {

                Dev2Logger.Error(err);
            }
        }

        public void IncrementBy(long ticks)
        {
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

        private void Setup()
        {
            if (!_started)
            {
                _counter = new PerformanceCounter("Warewolf", Name)
                {
                    MachineName = ".",
                    ReadOnly = false,
                    InstanceName = "Default"
                };
                _started = true;
            }
        }

        public void Decrement()
        {
            Setup();
            if (IsActive)

                try
                {
                    _counter.Decrement();
                }
                catch (Exception err)
                {

                    Dev2Logger.Error(err);
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
                return "Total Errors";
            }
        }

        #endregion
    }
}