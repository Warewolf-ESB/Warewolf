using System.Diagnostics;
using Dev2.Common.Interfaces.Monitoring;

namespace Dev2.Diagnostics.PerformanceCounters
{
    public class WarewolfRequestsPerSecondPerformanceCounter:IPerformanceCounter
    {
       
        private PerformanceCounter _counter;

        private void Setup()
        {
            if (_counter == null)
            {
                _counter = new PerformanceCounter("Warewolf", "Request Per Second")
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
            _counter.IncrementBy(1);
        }

        public void Decrement()
        {
            Setup();
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

        public CounterCreationData CreationData()
        {
            CounterCreationData totalOps = new CounterCreationData
            {
                CounterName = "Request Per Second",
                CounterHelp = "Request Per Second",
                CounterType = PerformanceCounterType.RateOfCountsPerSecond32
            };
            return totalOps;
        }

        public bool IsActive { get; set; }

        #endregion
    }
}