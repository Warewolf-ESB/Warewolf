using System;
using System.Windows.Input;
using Dev2.Common.Interfaces.Data.TO;
using Dev2.Common.Interfaces.Scheduler.Interfaces;
using Microsoft.Practices.Prism.Commands;

namespace Dev2.Scheduler
{
    public class DummyResource : IScheduledResource, INewScheduledResource
    {
        bool _isDirty;
        string _name;
        string _oldName;
        SchedulerStatus _status;
        DateTime _nextRunDate;
        IScheduleTrigger _trigger;
        int _numberOfHistoryToKeep;
        string _workflowName;
        Guid _resourceId;
        bool _runAsapIfScheduleMissed;
        bool _allowMultipleIstances;
        string _userName;
        string _password;
        IErrorResultTO _errors;
        bool _isNew;
        readonly
#pragma warning disable 414
        bool _isNewItem;
#pragma warning restore 414
        ICommand _newCommand;

        public DummyResource(Action model)
        {
            NameForDisplay = "'";
            _isNewItem = true;
            _newCommand = new DelegateCommand(model);
        }

        #region Implementation of IScheduledResource
        
        public bool IsDirty
        {
            get
            {
                return _isDirty;
            }
            set
            {
                _isDirty = value;
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
            }
        }

        public Guid ResourceId
        {
            get
            {
                return _resourceId;
            }
            set
            {
                _resourceId = value;
            }
        }

        public bool RunAsapIfScheduleMissed
        {
            get
            {
                return _runAsapIfScheduleMissed;
            }
            set
            {
                _runAsapIfScheduleMissed = value;
            }
        }
        public bool AllowMultipleIstances
        {
            get
            {
                return _allowMultipleIstances;
            }
            set
            {
                _allowMultipleIstances = value;
            }
        }

        public string UserName
        {
            get
            {
                return _userName;
            }
            set
            {
                _userName = value;
            }
        }
        public string Password
        {
            get
            {
                return _password;
            }
            set
            {
                _password = value;
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
                _isNew = value;
            }
        }

        public bool IsNewItem
        {
            get
            {
                return true;
            }
        }

        public string NameForDisplay { get; private set; }

        public void SetItem(IScheduledResource item)
        {
        }

        #endregion

        #region Implementation of INewScheduledResource

        public ICommand NewCommand
        {
            get
            {
                return _newCommand;
            }
            set
            {
                _newCommand = value;
            }
        }

        #endregion

        #region Implementation of IEquatable<IScheduledResource>

        public bool Equals(IScheduledResource other)
        {
            return false;
        }

        #endregion
    }
}