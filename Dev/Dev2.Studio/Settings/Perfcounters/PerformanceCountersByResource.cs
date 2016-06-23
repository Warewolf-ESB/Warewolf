using System;
using System.Windows.Input;
using Dev2.Common.Interfaces;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Microsoft.Practices.Prism.Mvvm;

namespace Dev2.Settings.Perfcounters
{
    public class PerformanceCountersByResource : BindableBase,IPerformanceCountersByResource
    {
        private bool _isDeleted;
        private Guid _resourceId;
        private string _counterName;
        private bool _requestPerSecond;
        private bool _totalErrors;
        private bool _averageExecutionTime;
        private bool _concurrentRequests;
        private ICommand _removeRow;
        private bool _isNew;

        public Guid ResourceId
        {
            get
            {
                return _resourceId;
            }
            set
            {
                _resourceId = value;
                OnPropertyChanged(()=>ResourceId);
            }
        }
        public string CounterName
        {
            get
            {
                return _counterName;
            }
            set
            {
                _counterName = value;
                OnPropertyChanged(()=>CounterName);
                OnPropertyChanged(()=>CanRemove);
                ((RelayCommand)RemoveRow).RaiseCanExecuteChanged();
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

        public bool IsDeleted
        {
            get
            {
                return _isDeleted;
            }
            set
            {
                _isDeleted = value;
                OnPropertyChanged(()=>IsDeleted);
            }
        }

        public ICommand RemoveRow
        {
            get
            {
                return _removeRow ??
                       (_removeRow =
                       new RelayCommand(o =>
                       {
                           IsDeleted = !IsDeleted;
                       }, o => CanRemove));
            }
        }

        public bool CanRemove => !string.IsNullOrEmpty(CounterName);

        public bool IsNew
        {
            get
            {
                return _isNew;
            }
            set
            {
                _isNew = value;
                OnPropertyChanged(()=>IsNew);
            }
        }
    }
}