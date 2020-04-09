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
using System.Diagnostics;
using Dev2.Common.Interfaces.Monitoring;
using Dev2.Data.TO;
using Dev2.Interfaces;
using Warewolf;

namespace Dev2.Runtime.ESB.Execution
{
    public class  PerfmonExecutionContainer:IEsbExecutionContainer
    {
        readonly IEsbExecutionContainer _container;
        readonly IPerformanceCounter _recPerSecondCounter;
        readonly IPerformanceCounter _currentConnections;
        readonly IPerformanceCounter _avgTime;
        readonly Stopwatch _stopwatch;
        readonly IPerformanceCounter _totalErrors;
        readonly IWarewolfPerformanceCounterLocater _locater;

        public  PerfmonExecutionContainer(IEsbExecutionContainer container)
        {
            VerifyArgument.IsNotNull(nameof(Container), container);
            _container = container;
            _locater = CustomContainer.Get<IWarewolfPerformanceCounterLocater>();
            _recPerSecondCounter = _locater.GetCounter("Request Per Second");
            _currentConnections = _locater.GetCounter("Concurrent requests currently executing");
            _avgTime = _locater.GetCounter("Average workflow execution time");
            _totalErrors = _locater.GetCounter("Total Errors");
            _stopwatch = new Stopwatch();
            _stopwatch.Start();
        }

        #region Implementation of IEsbExecutionContainer

        public Guid Execute(out ErrorResultTO errors, int update)
        {
            var start = _stopwatch.ElapsedTicks;
            var resourceId = GetDataObject().ResourceID;

            var errorsInstanceCounter = _locater.GetCounter(resourceId, WarewolfPerfCounterType.ExecutionErrors);
            var concurrentInstanceCounter = _locater.GetCounter(resourceId, WarewolfPerfCounterType.ConcurrentRequests);
            var avgExecutionsInstance = _locater.GetCounter(resourceId, WarewolfPerfCounterType.AverageExecutionTime);
            var reqPerSecond = _locater.GetCounter(resourceId, WarewolfPerfCounterType.RequestsPerSecond);
            var outErrors = new ErrorResultTO();
            try
            {
                _recPerSecondCounter.Increment();
                _currentConnections.Increment();
                reqPerSecond.Increment();
                concurrentInstanceCounter.Increment();
                var ret = Container.Execute(out outErrors, update);
                // BUG: why do we only report errors if Execute completes successfully?
                errors = outErrors;
                return ret;
            }
            finally 
            {
                
                _currentConnections.Decrement();
                concurrentInstanceCounter.Decrement();
                var time = _stopwatch.ElapsedTicks-start;
                _avgTime.IncrementBy(time);
                avgExecutionsInstance.IncrementBy(time);
                if(outErrors != null)
                {
                    var errorCount = outErrors.FetchErrors().Count;
                    _totalErrors.IncrementBy(errorCount);
                    errorsInstanceCounter.IncrementBy(errorCount);
                }
            }
            
        }

        public IDSFDataObject Execute(IDSFDataObject inputs, IDev2Activity activity) => Container.Execute(inputs, activity);

        public string InstanceOutputDefinition
        {
            get
            {
                return Container.InstanceOutputDefinition;
            }
            set
            {
                Container.InstanceOutputDefinition = value;
            }
        }
        public string InstanceInputDefinition
        {
            get
            {
                return Container.InstanceInputDefinition;
            }
            set
            {
                Container.InstanceInputDefinition = value;
            }
        }

        public IDSFDataObject GetDataObject() => _container.GetDataObject();

        public IEsbExecutionContainer Container => _container;

        #endregion
    }
}