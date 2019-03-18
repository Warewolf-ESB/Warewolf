#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
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

        public bool IsNewItem => true;

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

        public bool Equals(IScheduledResource other) => false;

        #endregion
    }
}