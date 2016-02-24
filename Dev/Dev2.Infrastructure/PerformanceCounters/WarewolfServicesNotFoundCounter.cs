﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using Dev2.Common;
using Dev2.Common.Interfaces.Monitoring;

namespace Dev2.PerformanceCounters
{
    public class WarewolfServicesNotFoundCounter : IPerformanceCounter
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
                _counter = new PerformanceCounter(GlobalConstants.Warewolf, Name, GlobalConstants.GlobalCounterName)
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
                return "Count of requests for workflows which don’t exist";
            }
        }

        #endregion
    }
}