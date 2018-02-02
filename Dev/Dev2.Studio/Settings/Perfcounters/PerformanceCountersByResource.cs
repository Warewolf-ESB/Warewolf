using System;
using System.Windows.Input;
using Dev2.Common.Interfaces;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Microsoft.Practices.Prism.Mvvm;

namespace Dev2.Settings.Perfcounters
{
    public class PerformanceCountersByResource : BindableBase,IPerformanceCountersByResource
    {
        bool _isDeleted;
        Guid _resourceId;
        string _counterName;
        bool _requestPerSecond;
        bool _totalErrors;
        bool _averageExecutionTime;
        bool _concurrentRequests;
        ICommand _removeRow;
        bool _isNew;

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

        public ICommand RemoveRow => _removeRow ??
                       (_removeRow =
                       new RelayCommand(o =>
                       {
                           IsDeleted = !IsDeleted;
                       }, o => CanRemove));

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