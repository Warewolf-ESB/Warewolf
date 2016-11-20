using System;
using System.Diagnostics;
using Dev2.Common.Interfaces.Monitoring;
using Dev2.Data.TO;
using Dev2.Interfaces;

namespace Dev2.Runtime.ESB.Execution
{
    public class  PerfmonExecutionContainer:IEsbExecutionContainer
    {
        private readonly IEsbExecutionContainer _container;
        private readonly IPerformanceCounter _recPerSecondCounter;
        private readonly IPerformanceCounter _currentConnections;
        private readonly IPerformanceCounter _avgTime;
        private readonly Stopwatch _stopwatch;
        private readonly IPerformanceCounter _totalErrors;
        private readonly IWarewolfPerformanceCounterLocater _locater;

        public  PerfmonExecutionContainer(IEsbExecutionContainer container)
        {
            VerifyArgument.IsNotNull("Container",container);
            _container = container;
            _locater = CustomContainer.Get<IWarewolfPerformanceCounterLocater>();
            _recPerSecondCounter = _locater.GetCounter("Request Per Second");
            _currentConnections = _locater.GetCounter("Concurrent requests currently executing");
            _avgTime = _locater.GetCounter("Average workflow execution time");
            _totalErrors = _locater.GetCounter("Total Errors");
            _stopwatch = new Stopwatch();
            _stopwatch.Start();
            //_counter = CustomContainer.Get<IWarewolfPerformanceCounterLocater>().GetCounter("Request Per Second");
        }

        #region Implementation of IEsbExecutionContainer

        public Guid Execute(out ErrorResultTO errors, int update)
        {
            var start = _stopwatch.ElapsedTicks;
            var errorsInstanceCounter = _locater.GetCounter(GetDataObject().ResourceID, WarewolfPerfCounterType.ExecutionErrors);
            var concurrentInstanceCounter = _locater.GetCounter(GetDataObject().ResourceID, WarewolfPerfCounterType.ConcurrentRequests);
            var avgExecutionsInstance = _locater.GetCounter(GetDataObject().ResourceID, WarewolfPerfCounterType.AverageExecutionTime);
            var reqPerSecond = _locater.GetCounter(GetDataObject().ResourceID, WarewolfPerfCounterType.RequestsPerSecond);
            ErrorResultTO outErrors = new ErrorResultTO();
            try
            {
                _recPerSecondCounter.Increment();
                _currentConnections.Increment();
                reqPerSecond.Increment();
                concurrentInstanceCounter.Increment();
                var ret = Container.Execute(out outErrors, update);
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
                    _totalErrors.IncrementBy(outErrors.FetchErrors().Count);
                    errorsInstanceCounter.IncrementBy(outErrors.FetchErrors().Count);
                    
                }
            }
            
        }

        public IDSFDataObject Execute(IDSFDataObject inputs, IDev2Activity activity)
        {
            return Container.Execute(inputs,activity);
        }

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

        public IDSFDataObject GetDataObject()
        {
            return _container.GetDataObject();
        }

        public IEsbExecutionContainer Container => _container;

        #endregion
    }
}