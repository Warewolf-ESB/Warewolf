using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Common.Interfaces.Monitoring;

namespace Dev2.PerformanceCounters
{
    

    public class WarewolfPerformanceCounterLocater : IWarewolfPerformanceCounterLocater,IPerformanceCounterFactory
    {
        private readonly IList<IPerformanceCounter> _counters;
        private readonly IWarewolfPerformanceCounterRegister _register;
        private IList<IPerformanceCounter> _resourceCounters;

        public WarewolfPerformanceCounterLocater(IList<IPerformanceCounter> counters, IWarewolfPerformanceCounterRegister register)
        {
            _counters = counters;
            _register = register;
            _resourceCounters = new List<IPerformanceCounter>();
        }

        public IPerformanceCounter GetCounter(string name)
        {
            return _counters.First(a => a.Name == name);
        }

        public IPerformanceCounter GetCounter(WarewolfPerfCounterType type)
        {
            return _counters.First(a => a.PerfCounterType == type);
        }

        public IResourcePerformanceCounter GetCounter(Guid resourceId, string name)
        {
            return _counters.Where(a=> a is IResourcePerformanceCounter).Cast<IResourcePerformanceCounter>().First(a=>a.ResourceId == resourceId);
        }

        #region Implementation of IPerformanceCounterFactory

        public IResourcePerformanceCounter CreateCounter(Guid resourceId, WarewolfPerfCounterType type, string name)
        {
            IResourcePerformanceCounter counter;
            switch(type)
            {
                    case WarewolfPerfCounterType.ExecutionErrors: 
                        counter =  new DefaultCounterTest(resourceId,name,type);
                    break;
                default :
                        counter = new DefaultCounterTest(resourceId, name, type);
                    break;
            }
            _register.RegisterCounter(counter);
            _resourceCounters.Add(counter);
            return counter;
        }

        #endregion
    }
}