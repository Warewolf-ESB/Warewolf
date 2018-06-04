﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using Dev2.Common;
using Dev2.Common.Interfaces.Monitoring;

namespace Dev2.PerformanceCounters.Counters
{
    public class SafeCounter:IPerformanceCounter
    {
        readonly IPerformanceCounter _counter;

        #region Implementation of IPerformanceCounter

        public SafeCounter(IPerformanceCounter counter)
        {
            _counter = counter;
        }

        public void Increment()
        {
            try
            {
                if (_counter.IsActive)
                {
                    _counter.Setup();
                    _counter.Increment();
                }
            }
            catch(Exception e)
            {
                
               Dev2Logger.Error(e, GlobalConstants.WarewolfError);
            }
        }

        public void IncrementBy(long ticks)
        {
            try
            {
                _counter.Setup();
                _counter.IncrementBy(ticks);

            }
            catch (Exception e)
            {

                Dev2Logger.Error(e, GlobalConstants.WarewolfError);
            }
        }

        public void Decrement()
        {
            try
            {
                _counter.Setup();
                _counter.Decrement();

            }
            catch (Exception e)
            {

                Dev2Logger.Error(e, GlobalConstants.WarewolfError);
            }
        }

        public string Category => _counter.Category;
        public string Name => _counter.Name;
        public WarewolfPerfCounterType PerfCounterType => _counter.PerfCounterType;

        public IList<CounterCreationData> CreationData() => _counter.CreationData();

        public bool IsActive { get => _counter.IsActive; set => _counter.IsActive = value; }
        public IPerformanceCounter InnerCounter => _counter;

        public void Setup()
        {
            _counter.Setup();
        }

        public void Reset()
        {
            _counter.Reset();
        }
        #endregion
    }


    public static class PerfCounterExtensions
    {
        public static IPerformanceCounter ToSafe(this IPerformanceCounter counter) => new SafeCounter(counter);

        public static IPerformanceCounter FromSafe(this IPerformanceCounter counter)
        {
            var x = counter as SafeCounter;
            return x == null ? counter : x.InnerCounter;
        }
    }
}
