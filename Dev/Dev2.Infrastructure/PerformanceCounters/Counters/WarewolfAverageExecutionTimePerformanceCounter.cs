#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using Dev2.Common;
using Dev2.Common.Interfaces.Monitoring;
using System;

namespace Dev2.PerformanceCounters.Counters
{
    public class WarewolfAverageExecutionTimePerformanceCounter : MyPerfCounter, IPerformanceCounter, IDisposable
    {
        IWarewolfPerformanceCounter _baseCounter;
        bool _started;
        readonly WarewolfPerfCounterType _perfCounterType;

        public WarewolfAverageExecutionTimePerformanceCounter(IRealPerformanceCounterFactory performanceCounterFactory)
            :base(performanceCounterFactory)
        {
            _started = false;
            IsActive = true;
            _perfCounterType = WarewolfPerfCounterType.AverageExecutionTime;
        }

        public WarewolfPerfCounterType PerfCounterType => _perfCounterType;

        public IEnumerable<(string, string, PerformanceCounterType)> CreationData()
        {

            yield return (
                Name,
                Name,
                PerformanceCounterType.AverageTimer32
            );

            yield return (
                "average time per operation base",
                "Average duration per operation execution base",
                PerformanceCounterType.AverageBase
            );
        }

        #region Implementation of IPerformanceCounter

        public void Increment()
        {
         
                if (IsActive)
                {
                    _counter.Increment();
                    _baseCounter.Increment();
                }
           
        }

        public void IncrementBy(long ticks)
        {

                if (IsActive)
                {
                    _counter.IncrementBy(ticks);
                    _baseCounter.Increment();
                }
         
        }

        public void Setup()
        {
            if (!_started)
            {
                _counter = _counterFactory.New(GlobalConstants.Warewolf, Name, GlobalConstants.GlobalCounterName);
                _baseCounter = _counterFactory.New(GlobalConstants.Warewolf, "average time per operation base", GlobalConstants.GlobalCounterName);

                _started = true;
            }
        }

        public void Reset()
        {
            if (_counter != null)
            {
                _counter.RawValue = 0;
            }
            if (_baseCounter != null)
            {
                _baseCounter.RawValue = 0;
            }
        }

        public void Decrement()
        {

            if (IsActive)
            {
 
                    _counter.Decrement();
                    _baseCounter.Decrement();

            }
        }

        new public void Dispose()
        {
            base.Dispose();
            if (_baseCounter != null)
            {
                _baseCounter.Dispose();
            }
        }

        public string Category => "Warewolf";
        public string Name => "Average workflow execution time";

        #endregion
    }
}