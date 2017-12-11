using Dev2.Common.Interfaces;
using Microsoft.Practices.Prism.Mvvm;

namespace Dev2.Settings.Perfcounters
{
    public class PerformanceCountersByMachine : BindableBase,IPerformanceCountersByMachine
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