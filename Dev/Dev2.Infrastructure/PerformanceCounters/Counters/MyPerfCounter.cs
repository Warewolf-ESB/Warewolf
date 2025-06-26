#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common;
using Dev2.Common.Interfaces.Monitoring;
using Dev2.PerformanceCounters.Management;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dev2.PerformanceCounters.Counters
{
    public interface IWarewolfPerformanceCounter : IDisposable
    {
        long RawValue { get; set; }

        void Increment();
        void IncrementBy(long ticks);
        void Decrement();
    }
    public interface IRealPerformanceCounterFactory
    {
        IWarewolfPerformanceCounter New(string categoryName, string counterName, string instanceName);
    }


    public class MyPerfCounter : IDisposable
    {
        protected readonly IRealPerformanceCounterFactory _counterFactory;
        protected IWarewolfPerformanceCounter _counter;
        public bool IsActive { get; set; }

        public MyPerfCounter()
            :this(new PerformanceCounterFactory())
        {
        }
        public MyPerfCounter(IRealPerformanceCounterFactory counterFactory)
        {
            if (counterFactory is null)
            {
                _counterFactory = new PerformanceCounterFactory();
            }
            else
            {
                _counterFactory = counterFactory;
            }
        }

        public void Dispose()
        {
            if (_counter != null)
            {
                _counter.Dispose();
            }
        }
    }

    class RealWarewolfPerformanceCounter : IWarewolfPerformanceCounter
    {
        readonly PerformanceCounter _counter;
        public RealWarewolfPerformanceCounter(string categoryName, string counterName, string instanceName)
		{
			Dev2Logger.Warn("Attempting to get performance counter.", "Warewolf Warn");
			try
            {
                _counter = new PerformanceCounter(categoryName, counterName, instanceName);
            }
            catch (System.InvalidOperationException ex)
			{
				var allCounters = PerformanceCounterPersistence.DefaultCounters;
				var register = new WarewolfPerformanceCounterRegister(allCounters, new List<IResourcePerformanceCounter>());
				register.RegisterCountersOnMachine(allCounters, "Warewolf");
                Dev2Logger.Warn("Failed to create performance counter. Attempting to re-register all counters.", "Warewolf Warn");
				_counter = new PerformanceCounter(categoryName, counterName, instanceName);
			}
            if (_counter == null)
            {
                throw new InvalidOperationException($"Performance counter '{counterName}' in category '{categoryName}' with instance '{instanceName}' could not be created.");
            }
            else
            {
                Dev2Logger.Warn("Got performance counter: \\" + _counter.CategoryName + "(" + _counter.InstanceName + ")\\" + _counter.CounterName, "Warewolf Warn");
            }
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
        public IWarewolfPerformanceCounter New(string categoryName, string counterName, string instanceName)
        {
            return new RealWarewolfPerformanceCounter(categoryName, counterName, instanceName);
        }
    }
}
