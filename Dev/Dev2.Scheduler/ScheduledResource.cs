using System;
using System.ComponentModel;
using Dev2.DataList.Contract;
using Dev2.Scheduler.Interfaces;
using System.Linq;
namespace Dev2.Scheduler
{
    public class ScheduledResource : IScheduledResource, INotifyPropertyChanged
    {
        string _name;
        string _workflowName;
        SchedulerStatus _status;

        private bool _runAsapIfScheduleMissed;
        private bool _allowMultipleIstances;
        int _numberOfHistoryToKeep;
        IScheduleTrigger _trigger;
        bool _isDirty;
        private string _userName;
        private string _password;
        string _oldName;
        private ErrorResultTO _errors;
        DateTime _nextRunDate;

        public ScheduledResource(string name, SchedulerStatus status, DateTime nextRunDate, IScheduleTrigger trigger, string workflowName)
        {

            var history = name.Split(new[] { '~' });

            WorkflowName = workflowName;
            Trigger = trigger;

            NextRunDate = nextRunDate;
            Status = status;
            Name = history.First();
            if(history.Length == 2)
                NumberOfHistoryToKeep = int.Parse(history[1]);
            IsDirty = false;
            _errors = new ErrorResultTO();
        }

        public bool IsDirty
        {
            get
            {
                return _isDirty;
            }
            set
            {
                _isDirty = value;
                OnPropertyChanged("IsDirty");
            }
        }

        public string OldName
        {
            get
            {
                return _oldName;
            }
            set
            {
                _oldName = value;
                OnPropertyChanged("OldName");
            }
        }

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                OnPropertyChanged("Name");
            }
        }

        public SchedulerStatus Status
        {
            get
            {
                return _status;
            }
            set
            {
                _status = value;
                OnPropertyChanged("Status");
            }
        }

        public DateTime NextRunDate
        {
            get
            {
                return _nextRunDate;
            }
            set
            {
                _nextRunDate = value;
                OnPropertyChanged("NextRunDate");
            }
        }

        public IScheduleTrigger Trigger
        {
            get
            {
                return _trigger;
            }
            set
            {
                _trigger = value;
                OnPropertyChanged("Trigger");
            }
        }

        public int NumberOfHistoryToKeep
        {
            get
            {
                return _numberOfHistoryToKeep;
            }
            set
            {
                _numberOfHistoryToKeep = value;
                OnPropertyChanged("NumberOfHistoryToKeep");
            }
        }

        public string WorkflowName
        {
            get
            {
                return _workflowName;
            }
            set
            {
                _workflowName = value;
                OnPropertyChanged("WorkflowName");
            }
        }

        public bool RunAsapIfScheduleMissed
        {
            get { return _runAsapIfScheduleMissed; }
            set
            {
                _runAsapIfScheduleMissed = value;
                OnPropertyChanged("RunAsapIfScheduleMissed");
            }
        }

        public bool AllowMultipleIstances
        {
            get { return _allowMultipleIstances; }
            set
            {
                _allowMultipleIstances = value;
                OnPropertyChanged("AllowMultipleIstances");
            }
        }

        public string UserName
        {
            get { return _userName; }
            set
            {
                _userName = value;
                OnPropertyChanged("UserName");
            }
        }

        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                OnPropertyChanged("Password");
            }
        }

        public ErrorResultTO Errors
        {
            get
            {
                return _errors;
            }
            set
            {
                _errors = value;
                OnPropertyChanged("Errors");
            }
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if(PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion INotifyPropertyChanged


        public Guid ResourceId { get; set; }
    }
}