using System.Collections.Generic;
using System.Diagnostics;
using Dev2.Common.Interfaces.Monitoring;

namespace Dev2.Diagnostics.PerformanceCounters
{
    public class WarewolfServicesNotFoundCounter:IPerformanceCounter
    {
       
        private PerformanceCounter _counter;
        private bool _started;
        private readonly WarewolfPerfCounterType _perfCounterType;

        public WarewolfServicesNotFoundCounter()
        {
            _started = false;
            IsActive = true;
            _perfCounterType = WarewolfPerfCounterType.ServicesNotFound;
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
            return new []{ totalOps};
        }

        public bool IsActive { get; set; }

        #region Implementation of IPerformanceCounter

        public void Increment()
        {
            Setup();
            if (IsActive)
                _counter.Increment();
        }

        public void IncrementBy(long ticks)
        {
            Setup();
            _counter.IncrementBy(ticks);
        }

        private void Setup()
        {
            if (!_started)
            {
                _counter = new PerformanceCounter("Warewolf", Name)
                {
                    MachineName = ".",
                    ReadOnly = false
                };
                _started = true;
            }
        }

        public void Decrement()
        {
            Setup();
            if(IsActive)

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
                return "Count of requests for workflows which don’t exist";
            }
        }

        #endregion
    }
}