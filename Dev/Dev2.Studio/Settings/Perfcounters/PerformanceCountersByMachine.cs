using Dev2.Common.Interfaces;
using Microsoft.Practices.Prism.Mvvm;

namespace Dev2.Settings.Perfcounters
{
    public class PerformanceCountersByMachine : BindableBase,IPerformanceCountersByMachine
    {
        private string _machineName;
        private bool _requestPerSecond;
        private bool _totalErrors;
        private bool _averageExecutionTime;
        private bool _concurrentRequests;
        private bool _workFlowsNotFound;
        private bool _notAuthorisedErrors;
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