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

using System.Collections.Generic;
using Dev2.Common;
using Dev2.Common.Interfaces.Monitoring;
using System;

namespace Dev2.PerformanceCounters.Counters
{
    public class WarewolfNumberOfAuthErrors : MyPerfCounter, IPerformanceCounter
    {
        bool _started;
        readonly WarewolfPerfCounterType _perfCounterType;

        public WarewolfNumberOfAuthErrors(IRealPerformanceCounterFactory performanceCounterFactory)
            :base(performanceCounterFactory)
        {
            _started = false;
            IsActive = true;
            _perfCounterType = WarewolfPerfCounterType.NotAuthorisedErrors;
        }

        public WarewolfPerfCounterType PerfCounterType => _perfCounterType;

        public IEnumerable<(string CounterName, string CounterHelp, PerformanceCounterType CounterType)> CreationData()
        {
            yield return
            (
                Name,
                Name,
                PerformanceCounterType.NumberOfItems32
            );
        }

        #region Implementation of IPerformanceCounter

        public void Increment()
        {
            if (IsActive)
            {
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

        public void Setup()
        {
            if (!_started)
            {
                _counter = _counterFactory.New(GlobalConstants.Warewolf, Name, GlobalConstants.GlobalCounterName);
                _started = true;
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
        public string Name => "Count of Not Authorised errors";

        public void Reset()
        {
            if (_counter != null)
            {
                _counter.RawValue = 0;
            }
        }

        new public void Dispose()
        {
            if (_counter != null)
            {
                _counter.Dispose();
            }
        }
        #endregion
    }
}