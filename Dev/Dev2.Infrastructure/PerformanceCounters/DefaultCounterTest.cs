using System;
using System.Collections.Generic;
using System.Diagnostics;
using Dev2.Common;
using Dev2.Common.Interfaces.Monitoring;

namespace Dev2.PerformanceCounters
{
    public  class DefaultCounterTest :IResourcePerformanceCounter
    {
        
        
        private PerformanceCounter _counter;
        private bool _started;
        private readonly WarewolfPerfCounterType _perfCounterType;
        private readonly Guid _resourceId;

        public DefaultCounterTest(Guid resourceId, string name,WarewolfPerfCounterType type)
        {
            _resourceId = resourceId;
            CategoryInstanceName = name;
            _started = false;
            IsActive = true;
            _perfCounterType = type;
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
                CounterType = _perfCounterType.ToSystemType()
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
                _counter = new PerformanceCounter("Warewolf", Name,CategoryInstanceName)
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

        #region Implementation of IResourcePerformanceCounter

        public Guid ResourceId
        {
            get
            {
                return _resourceId;
            }
        }
        public string CategoryInstanceName { get; private set; }

        #endregion
    }
}