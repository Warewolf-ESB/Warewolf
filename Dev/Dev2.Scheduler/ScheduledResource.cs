#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Common;

namespace Dev2.Scheduler
{
    public class ScheduledResource : IScheduledResource, INotifyPropertyChanged
    {
        string _name;
        string _workflowName;
        SchedulerStatus _status;

        bool _runAsapIfScheduleMissed;
        bool _allowMultipleIstances;
        int _numberOfHistoryToKeep;
        IScheduleTrigger _trigger;
        bool _isDirty;
        string _userName;
        string _password;
        string _oldName;
        IErrorResultTO _errors;
        DateTime _nextRunDate;
        bool _isNew;

        public ScheduledResource(string name, SchedulerStatus status, DateTime nextRunDate, IScheduleTrigger trigger, string workflowName, string resourceId)
        {

            var history = name.Split('~');

            WorkflowName = workflowName;
            Trigger = trigger;

            NextRunDate = nextRunDate;
            _status = status;
            Name = history.First();
            if(history.Length == 2)
            {
                NumberOfHistoryToKeep = int.Parse(history[1]);
            }

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
            set
            {
                IsDirty = true;
                _status = _status==SchedulerStatus.Disabled?SchedulerStatus.Enabled:SchedulerStatus.Disabled;
                OnPropertyChanged("StatusAlt");
                OnPropertyChanged("Status");
                Dev2Logger.Info("Scheduled Resource Alt Status set to " + value, GlobalConstants.WarewolfInfo);
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
        public bool IsNew
        {
            get
            {
                return _isNew;
            }
            set
            {
                IsDirty = value;
                _isNew = value;
            }
        }
        public bool IsNewItem { get; set; }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion INotifyPropertyChanged

        public bool Equals(IScheduledResource other)
        {
            if (other == null)
            {
                return false;
            }
            if (IsNew)
            {
                return false;
            }
            var nameEqual = other.Name.Equals(Name, StringComparison.CurrentCultureIgnoreCase);
            var statusEqual = other.Status == Status;
            var nextRunDateEqual = other.NextRunDate == NextRunDate;
            var triggerEqual = TriggerEqual(other.Trigger, Trigger);
            var numberOfHistoryToKeepEqual = other.NumberOfHistoryToKeep == NumberOfHistoryToKeep;
            var workflowNameEqual = other.WorkflowName.Equals(WorkflowName,StringComparison.InvariantCultureIgnoreCase);
            var runAsapIfMissedEqual = other.RunAsapIfScheduleMissed == RunAsapIfScheduleMissed;
            var allowMultipleInstancesEqual = other.AllowMultipleIstances == AllowMultipleIstances;
            var userNameEqual = !string.IsNullOrEmpty(other.UserName) && !string.IsNullOrEmpty(UserName) ? other.UserName.Equals(UserName,StringComparison.InvariantCultureIgnoreCase) : string.IsNullOrEmpty(other.UserName) && string.IsNullOrEmpty(UserName);
            return nameEqual && statusEqual && nextRunDateEqual && triggerEqual && numberOfHistoryToKeepEqual && workflowNameEqual
                    && runAsapIfMissedEqual && allowMultipleInstancesEqual && userNameEqual;
        }

        bool TriggerEqual(IScheduleTrigger otherTrigger, IScheduleTrigger trigger)
        {
            if (otherTrigger.State != trigger.State)
            {
                return false;
            }
            if (otherTrigger.Trigger == null && trigger.Trigger != null)
            {
                return false;
            }
            if (otherTrigger.Trigger != null && trigger.Trigger == null)
            {
                return false;
            }
            if (otherTrigger.Trigger != null && trigger.Trigger != null)
            {
                if (otherTrigger.Trigger.Enabled != trigger.Trigger.Enabled)
                {
                    return false;
                }
                if (otherTrigger.Trigger.EndBoundary != trigger.Trigger.EndBoundary)
                {
                    return false;
                }
                if (otherTrigger.Trigger.StartBoundary != trigger.Trigger.StartBoundary)
                {
                    return false;
                }
                if (otherTrigger.Trigger.TriggerType != trigger.Trigger.TriggerType)
                {
                    return false;
                }
                if (otherTrigger.Trigger.Repetition == null && otherTrigger.Trigger.Repetition != null)
                {
                    return false;
                }

                if (otherTrigger.Trigger.Repetition != null && otherTrigger.Trigger.Repetition == null)
                {
                    return false;
                }
                if (otherTrigger.Trigger.Repetition != null && trigger.Trigger.Repetition != null)
                {
                    if (otherTrigger.Trigger.Repetition.Duration != trigger.Trigger.Repetition.Duration)
                    {
                        return false;
                    }
                    if (otherTrigger.Trigger.Repetition.Interval != trigger.Trigger.Repetition.Interval)
                    {
                        return false;
                    }
                    if (otherTrigger.Trigger.Repetition.StopAtDurationEnd != trigger.Trigger.Repetition.StopAtDurationEnd)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public override string ToString() => String.Format("Name:{0} ResourceId:{1}", Name, ResourceId);

        public Guid ResourceId { get; set; }
    }
}
