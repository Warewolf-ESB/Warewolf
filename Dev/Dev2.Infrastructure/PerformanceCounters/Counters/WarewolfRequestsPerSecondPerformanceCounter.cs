﻿using System.Collections.Generic;
using System.Diagnostics;
using Dev2.Common;
using Dev2.Common.Interfaces.Monitoring;
using System;

namespace Dev2.PerformanceCounters.Counters
{
    public class WarewolfRequestsPerSecondPerformanceCounter : IPerformanceCounter, IDisposable
    {
        PerformanceCounter _counter;
        Stopwatch _stopwatch;

        const WarewolfPerfCounterType _perfCounterType = WarewolfPerfCounterType.RequestsPerSecond;
        public WarewolfRequestsPerSecondPerformanceCounter()
        {
            IsActive = true;
        }

        public void Setup()
        {
            if (_counter == null)
            {
                _stopwatch = new Stopwatch();
                _stopwatch.Start();
                _counter = new PerformanceCounter(GlobalConstants.Warewolf, Name, GlobalConstants.GlobalCounterName)
                {
                    MachineName = ".",
                    ReadOnly = false,
                    InstanceLifetime = PerformanceCounterInstanceLifetime.Global
              
                };
            }
        }

        public void Reset()
        {
            if (_counter != null)
            {
                _counter.RawValue = 0;
            }
        }

        #region Implementation of IPerformanceCounter

        public void Increment()
        {
            if (IsActive)
            {
                Setup();
                _counter.Increment();
            }
      
        }

        public void IncrementBy(long ticks)
        {
            if (IsActive)
            {
                _counter.IncrementBy(ticks);
            }
        }

        public void Decrement()
        {
            if (IsActive)
            {
           
                    _counter.Decrement();

            }
        }

        public string Category => "Warewolf";

        public string Name => "Request Per Second";
        public WarewolfPerfCounterType PerfCounterType => _perfCounterType;

        public IList<CounterCreationData> CreationData()
        {

            var totalOps = new CounterCreationData
            {
                CounterName = Name,
                CounterHelp = Name,
                CounterType = PerformanceCounterType.RateOfCountsPerSecond32
            };
            return new[] { totalOps };
        }

        public void Dispose()
        {
            _counter.Dispose();
        }

        public bool IsActive { get; set; }

        #endregion
    }
}