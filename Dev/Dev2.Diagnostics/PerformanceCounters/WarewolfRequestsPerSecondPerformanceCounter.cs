using System.Collections.Generic;
using System.Diagnostics;
using Dev2.Common.Interfaces.Monitoring;

namespace Dev2.Diagnostics.PerformanceCounters
{
    public class WarewolfRequestsPerSecondPerformanceCounter:IPerformanceCounter
    {
       
        private PerformanceCounter _counter;
        private Stopwatch _stopwatch;

        // ReSharper disable once InconsistentNaming
        private const WarewolfPerfCounterType _perfCounterType = WarewolfPerfCounterType.RequestsPerSecond;

        private void Setup()
        {

            if (_counter == null)
            {
                _stopwatch = new Stopwatch();
                _stopwatch.Start();
                _counter = new PerformanceCounter("Warewolf", Name)
                {
                    MachineName = ".",
                    ReadOnly = false
                };
            }
        }

        #region Implementation of IPerformanceCounter

        public void Increment()
        {
            Setup();
            _counter.Increment();
        }

        public void IncrementBy(long ticks)
        {
            Setup();

            _counter.IncrementBy(ticks);
        }

        public void Decrement()
        {
            Setup();
            _counter.Decrement();
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
    }
}