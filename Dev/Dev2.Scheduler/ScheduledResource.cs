/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.ComponentModel;
using System.Linq;
using Dev2.Common.Interfaces.Data.TO;
using Dev2.Common.Interfaces.Scheduler.Interfaces;
using Dev2.DataList.Contract;

namespace Dev2.Scheduler
{
    public class ScheduledResource : IScheduledResource, INotifyPropertyChanged
    {
        private bool _allowMultipleIstances;
        private IErrorResultTO _errors;
        private bool _isDirty;
        private string _name;
        private DateTime _nextRunDate;
        private int _numberOfHistoryToKeep;
        private string _oldName;
        private string _password;
        private bool _runAsapIfScheduleMissed;
        private SchedulerStatus _status;
        private IScheduleTrigger _trigger;
        private string _userName;
        private string _workflowName;

        public ScheduledResource(string name, SchedulerStatus status, DateTime nextRunDate, IScheduleTrigger trigger,
            string workflowName)
        {
            string[] history = name.Split(new[] {'~'});

            WorkflowName = workflowName;
            Trigger = trigger;

            NextRunDate = nextRunDate;
            Status = status;
            Name = history.First();
            if (history.Length == 2)
                NumberOfHistoryToKeep = int.Parse(history[1]);
            IsDirty = false;
            _errors = new ErrorResultTO();
        }

        public bool IsDirty
        {
            get { return _isDirty; }
            set
            {
                _isDirty = value;
                OnPropertyChanged("IsDirty");
            }
        }

        public string OldName
        {
            get { return _oldName; }
            set
            {
                _oldName = value;
                OnPropertyChanged("OldName");
            }
        }

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged("Name");
            }
        }

        public SchedulerStatus Status
        {
            get { return _status; }
            set
            {
                _status = value;
                OnPropertyChanged("Status");
            }
        }

        public DateTime NextRunDate
        {
            get { return _nextRunDate; }
            set
            {
                _nextRunDate = value;
                OnPropertyChanged("NextRunDate");
            }
        }

        public IScheduleTrigger Trigger
        {
            get { return _trigger; }
            set
            {
                _trigger = value;
                OnPropertyChanged("Trigger");
            }
        }

        public int NumberOfHistoryToKeep
        {
            get { return _numberOfHistoryToKeep; }
            set
            {
                _numberOfHistoryToKeep = value;
                OnPropertyChanged("NumberOfHistoryToKeep");
            }
        }

        public string WorkflowName
        {
            get { return _workflowName; }
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

        public IErrorResultTO Errors
        {
            get { return _errors; }
            set
            {
                _errors = value;
                OnPropertyChanged("Errors");
            }
        }

        public bool IsNew { get; set; }

        public Guid ResourceId { get; set; }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion INotifyPropertyChanged

        public override string ToString()
        {
            return String.Format("Name:{0} ResourceId:{1}", Name, ResourceId);
        }
    }
}