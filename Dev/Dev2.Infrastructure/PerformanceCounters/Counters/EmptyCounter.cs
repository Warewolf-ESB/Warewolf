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

using System;
using System.Collections.Generic;
using Dev2.Common;
using Dev2.Common.Interfaces.Monitoring;


namespace Dev2.PerformanceCounters.Counters
{
    public class EmptyCounter : IResourcePerformanceCounter
    {

        public  EmptyCounter()
        {
            Name = "Empty";
            Category = GlobalConstants.Warewolf;
        }
        #region Implementation of IPerformanceCounter

        public void Increment()
        {
        }

        public void IncrementBy(long ticks)
        {
        }

        public void Decrement()
        {
        }

        public string Category { get;  set; }
        public string Name { get;  set; }
        public WarewolfPerfCounterType PerfCounterType { get;  set; }

        public IEnumerable<(string CounterName, string CounterHelp, PerformanceCounterType CounterType)> CreationData() => null;

        public bool IsActive { get; set; }

        public void Setup()
        {
        }

        #endregion
        public void Reset()
        {            
        }

        public void Dispose()
        {

        }

        #region Implementation of IResourcePerformanceCounter

        public Guid ResourceId { get;  set; }
        public string CategoryInstanceName { get;  set; }

        #endregion
    }
}