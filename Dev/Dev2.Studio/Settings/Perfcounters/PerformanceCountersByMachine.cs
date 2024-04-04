using Dev2.Common.Interfaces;
#if !NETFRAMEWORK
using Dev2.Common;
#else
using Microsoft.Practices.Prism.Mvvm;
#endif

namespace Dev2.Settings.Perfcounters
{
#if !NETFRAMEWORK
    public class PerformanceCountersByMachine : BindableBase2,IPerformanceCountersByMachine
#else
    public class PerformanceCountersByMachine : BindableBase,IPerformanceCountersByMachine
#endif
    {
        string _machineName;
        bool _requestPerSecond;
        bool _totalErrors;
        bool _averageExecutionTime;
        bool _concurrentRequests;
        bool _workFlowsNotFound;
        bool _notAuthorisedErrors;
        public string MachineName
        {
            get
            {
                return _machineName;
            }
            set
            {
                _machineName = value;
                OnPropertyChanged(()=>MachineName);
            }
        }
        public bool RequestPerSecond
        {
            get
            {
                return _requestPerSecond;
            }
            set
            {
                _requestPerSecond = value;
                OnPropertyChanged(()=>RequestPerSecond);
            }
        }
        public bool TotalErrors
        {
            get
            {
                return _totalErrors;
            }
            set
            {
                _totalErrors = value;
                OnPropertyChanged(()=>TotalErrors);
            }
        }
        public bool AverageExecutionTime
        {
            get
            {
                return _averageExecutionTime;
            }
            set
            {
                _averageExecutionTime = value;
                OnPropertyChanged(()=>AverageExecutionTime);
            }
        }
        public bool ConcurrentRequests
        {
            get
            {
                return _concurrentRequests;
            }
            set
            {
                _concurrentRequests = value;
                OnPropertyChanged(()=>ConcurrentRequests);
            }
        }
        public bool WorkFlowsNotFound
        {
            get
            {
                return _workFlowsNotFound;
            }
            set
            {
                _workFlowsNotFound = value;
                OnPropertyChanged(()=>WorkFlowsNotFound);
            }
        }
        public bool NotAuthorisedErrors
        {
            get
            {
                return _notAuthorisedErrors;
            }
            set
            {
                _notAuthorisedErrors = value;
                OnPropertyChanged(()=>NotAuthorisedErrors);
            }
        }
    }
}