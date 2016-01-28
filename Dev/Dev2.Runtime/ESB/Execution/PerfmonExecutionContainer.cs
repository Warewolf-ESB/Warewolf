using System;
using System.Diagnostics;
using Dev2.Common.Interfaces.Monitoring;
using Dev2.DataList.Contract;
using Dev2.Diagnostics.PerformanceCounters;

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

        public  PerfmonExecutionContainer(IEsbExecutionContainer container)
        {
            VerifyArgument.IsNotNull("Container",container);
            _container = container;
            _recPerSecondCounter = CustomContainer.Get<IWarewolfPerformanceCounterLocater>().GetCounter("Request Per Second");
            _currentConnections = CustomContainer.Get<IWarewolfPerformanceCounterLocater>().GetCounter("Concurrent requests currently executing");
           _avgTime =  CustomContainer.Get<IWarewolfPerformanceCounterLocater>().GetCounter("Average workflow execution time");
           _totalErrors = CustomContainer.Get<IWarewolfPerformanceCounterLocater>().GetCounter("Total Errors");
            _stopwatch = new Stopwatch();
            _stopwatch.Start();
            //_counter = CustomContainer.Get<IWarewolfPerformanceCounterLocater>().GetCounter("Request Per Second");
        }

        #region Implementation of IEsbExecutionContainer

        public Guid Execute(out ErrorResultTO errors, int update)
        {
            var start = _stopwatch.ElapsedTicks;
            ErrorResultTO outErrors = new ErrorResultTO();
            try
            {
                _recPerSecondCounter.Increment();
                _currentConnections.Increment();
               
                var ret = Container.Execute(out outErrors, update);
                errors = outErrors;
                return ret;
            }
            finally 
            {
                
                _currentConnections.Decrement();
                _avgTime.IncrementBy(_stopwatch.ElapsedTicks-start);
                if(outErrors != null)
                {
                    _totalErrors.IncrementBy(outErrors.FetchErrors().Count);
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
        public IEsbExecutionContainer Container
        {
            get
            {
                return _container;
            }
        }

        #endregion
    }
}