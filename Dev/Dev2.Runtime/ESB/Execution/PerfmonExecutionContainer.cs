using System;
using Dev2.Common.Interfaces.Monitoring;
using Dev2.DataList.Contract;
using Dev2.Diagnostics.PerformanceCounters;
using Warewolf.Storage;

namespace Dev2.Runtime.ESB.Execution
{
    public class  PerfmonExecutionContainer:IEsbExecutionContainer
    {
        private readonly IEsbExecutionContainer _container;
        private IPerformanceCounter _counter;

        public  PerfmonExecutionContainer(IEsbExecutionContainer container)
        {
            _container = container;
            _counter = CustomContainer.Get<IWarewolfPerformanceCounterLocater>().GetCounter("Request Per Second");
        }

        #region Implementation of IEsbExecutionContainer

        public Guid Execute(out ErrorResultTO errors, int update)
        {
            try
            {
                _counter.Increment();
                return _container.Execute(out errors, update);
            }
            finally 
            {
                _counter.Decrement();
             
            }
            
        }

        public IExecutionEnvironment Execute(IDSFDataObject inputs, IDev2Activity activity)
        {
            return _container.Execute(inputs,activity);
        }

        public string InstanceOutputDefinition
        {
            get
            {
                return _container.InstanceOutputDefinition;
            }
            set
            {
                _container.InstanceOutputDefinition = value;
            }
        }
        public string InstanceInputDefinition
        {
            get
            {
                return _container.InstanceInputDefinition;
            }
            set
            {
                _container.InstanceInputDefinition = value;
            }
        }

        #endregion
    }
}