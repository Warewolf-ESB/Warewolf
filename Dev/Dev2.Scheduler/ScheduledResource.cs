/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Data.TO;
using Newtonsoft.Json;
// ReSharper disable NonLocalizedString

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
        private IErrorResultTO _errors;
        DateTime _nextRunDate;

        public ScheduledResource(string name, SchedulerStatus status, DateTime nextRunDate, IScheduleTrigger trigger, string workflowName, string resourceId)
        {

            var history = name.Split('~');

            WorkflowName = workflowName;
            Trigger = trigger;

            NextRunDate = nextRunDate;
            _status = status;
            Name = history.First();
            if(history.Length == 2)
                NumberOfHistoryToKeep = int.Parse(history[1]);
            IsDirty = false;
            _errors = new ErrorResultTO();
            if(!String.IsNullOrEmpty(resourceId) )
            {
                ResourceId = Guid.Parse(resourceId);
            }
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
                OnPropertyChanged("NameForDisplay");
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
                if (!IsDirty)
                {
                    OnPropertyChanged("NameForDisplay");
                }
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
                if (NameForDisplay != value)
                {
                    IsDirty = true;
                }
                _name = value;
                OnPropertyChanged("Name");
                OnPropertyChanged("NameForDisplay");
            }
        }
        
        public string NameForDisplay
        {
            get
            {
                if (IsDirty)
                {
                    return Name + " *";
                }
                return Name;
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
                if (Status != value)
                {
                    IsDirty = true;
                }
                _status = value;
                OnPropertyChanged("Status");
                OnPropertyChanged("StatusAlt");
            }
        }
        [JsonIgnore]
        public SchedulerStatus StatusAlt
        {
            get
            {
                return _status;
            }
            // ReSharper disable once ValueParameterNotUsed
            set
            {
            
                    IsDirty = true;
               
                _status = _status ==SchedulerStatus.Disabled?SchedulerStatus.Enabled : SchedulerStatus.Disabled;
                OnPropertyChanged("StatusAlt");
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

        public IErrorResultTO Errors
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
        public bool IsNew { get; set; }
        public bool IsNewItem { get; set; }

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

        public override string ToString()
        {
            return String.Format("Name:{0} ResourceId:{1}", Name, ResourceId);
        }
        public Guid ResourceId { get; set; }
    }
}
