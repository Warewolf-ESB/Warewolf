﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dev2.PerformanceCounters.Counters
{
    public interface IBobsPerformanceCounter : IDisposable
    {
        long RawValue { get; set; }

        void Increment();
        void IncrementBy(long ticks);
        void Decrement();
    }
    public interface IRealPerformanceCounterFactory
    {
        IBobsPerformanceCounter New(string categoryName, string counterName, string instanceName);
    }


    public class MyPerfCounter : IDisposable
    {
        protected readonly IRealPerformanceCounterFactory _counterFactory;
        protected IBobsPerformanceCounter _counter;
        public bool IsActive { get; set; }

        public MyPerfCounter(IRealPerformanceCounterFactory counterFactory)
        {
            _counterFactory = counterFactory;
        }

        public void Dispose()
        {
            if (_counter != null)
            {
                _counter.Dispose();
            }
        }
    }

    class RealBobsPerformanceCounter : IBobsPerformanceCounter
    {
        readonly PerformanceCounter _counter;
        public RealBobsPerformanceCounter(string categoryName, string counterName, string instanceName)
        {
            _counter = new PerformanceCounter(categoryName, counterName, instanceName);
            _counter.MachineName = ".";
            _counter.ReadOnly = false;
            _counter.InstanceLifetime = PerformanceCounterInstanceLifetime.Global;
        }

        public long RawValue
        {
            get => _counter.RawValue;
            set => _counter.RawValue = value;
        }

        public void Decrement()
        {
            _counter.Decrement();
        }

        public void Dispose()
        {
            _counter.Dispose();
        }

        public void Increment()
        {
            _counter.Increment();
        }

        public void IncrementBy(long ticks)
        {
            _counter.IncrementBy(ticks);
        }
    }

    public class PerformanceCounterFactory : IRealPerformanceCounterFactory
    {
        public IBobsPerformanceCounter New(string categoryName, string counterName, string instanceName)
        {
            return new RealBobsPerformanceCounter(categoryName, counterName, instanceName);
        }
    }
}
