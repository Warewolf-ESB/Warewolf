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
using System.Collections.Generic;
using Dev2.Common.Interfaces.Data.TO;
using Dev2.Common.Interfaces.DB;
using Dev2.Triggers;
using Warewolf.Options;

namespace Warewolf.Trigger
{
    public class TriggerQueueView : BindableBase
    {
        private string _triggerQueueName;
        private Guid _queueSourceId;
        private string _queueName;
        private string _workflowName;
        private int _concurrency;
        private string _userName;
        private string _password;
        private IOption[] _options;
        private Guid _queueSinkId;
        private string _deadLetterQueue;
        private IOption[] _deadLetterOptions;
        private ICollection<IServiceInput> _inputs;

        private bool _isDirty;
        private string _oldQueueName;
        private bool _enabled;
        private IErrorResultTO _errors;
        private bool _isNewQueue;
        private bool _newQueue;
        private string _nameForDisplay;
        private TriggerQueueView _item;
        bool _isValidatingIsDirty;

        public Guid TriggerId { get; set; }
        public string TriggerQueueName
        {
            get => _triggerQueueName;
            set
            {
                _triggerQueueName = value;
                RaisePropertyChanged(nameof(TriggerQueueName));
                SetDisplayName(IsDirty);
            }
        }
        public Guid QueueSourceId
        {
            get => _queueSourceId;
            set
            {
                _queueSourceId = value;
                RaisePropertyChanged(nameof(QueueSourceId));
            }
        }
        public string QueueName
        {
            get => _queueName;
            set
            {
                _queueName = value;
                RaisePropertyChanged(nameof(QueueName));
            }
        }
        public string WorkflowName
        {
            get => _workflowName;
            set
            {
                _workflowName = value;
                RaisePropertyChanged(nameof(WorkflowName));
            }
        }
        public int Concurrency
        {
            get => _concurrency;
            set
            {
                _concurrency = value;
                RaisePropertyChanged(nameof(Concurrency));
            }
        }
        public string UserName
        {
            get => _userName;
            set
            {
                _userName = value;
                RaisePropertyChanged(nameof(UserName));
            }
        }
        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                RaisePropertyChanged(nameof(Password));
            }
        }
        public IOption[] Options
        {
            get => _options;
            set
            {
                _options = value;
                RaisePropertyChanged(nameof(Options));
            }
        }
        public Guid QueueSinkId
        {
            get => _queueSinkId;
            set
            {
                _queueSinkId = value;
                RaisePropertyChanged(nameof(QueueSinkId));
            }
        }
        public string DeadLetterQueue
        {
            get => _deadLetterQueue;
            set
            {
                _deadLetterQueue = value;
                RaisePropertyChanged(nameof(DeadLetterQueue));
            }
        }
        public IOption[] DeadLetterOptions
        {
            get => _deadLetterOptions;
            set
            {
                _deadLetterOptions = value;
                RaisePropertyChanged(nameof(DeadLetterOptions));
            }
        }
        public ICollection<IServiceInput> Inputs
        {
            get => _inputs;
            set
            {
                _inputs = value;
                RaisePropertyChanged(nameof(Inputs));
            }
        }
        public Guid ResourceId { get; set; }

        public bool IsDirty
        {
            get
            {
                if (_isValidatingIsDirty)
                {
                    return false;
                }
                _isValidatingIsDirty = true;
                var _isDirty = false;
                var notEquals = !Equals(Item);
                if (NewQueue)
                {
                    _isDirty = true;
                }
                else
                {
                    if (notEquals)
                    {
                        _isDirty = true;
                    }
                }

                SetDisplayName(_isDirty);
                _isValidatingIsDirty = false;
                return _isDirty;
            }
        }
        public string OldQueueName
        {
            get => _oldQueueName;
            set
            {
                _oldQueueName = value;
                RaisePropertyChanged(nameof(OldQueueName));
            }
        }
        public bool Enabled
        {
            get => _enabled;
            set
            {
                _enabled = value;
                RaisePropertyChanged(nameof(Enabled));
            }
        }
        public IErrorResultTO Errors
        {
            get => _errors;
            set
            {
                _errors = value;
                RaisePropertyChanged(nameof(Errors));
            }
        }
        public bool IsNewQueue
        {
            get => _isNewQueue;
            set
            {
                _isNewQueue = value;
                RaisePropertyChanged(nameof(IsNewQueue));
            }
        }
        public bool NewQueue
        {
            get => _newQueue;
            set
            {
                _newQueue = value;
                RaisePropertyChanged(nameof(NewQueue));
            }
        }
        public string NameForDisplay
        {
            get => _nameForDisplay;
            set
            {
                _nameForDisplay = value;
                RaisePropertyChanged(nameof(NameForDisplay));
            }
        }

        public TriggerQueueView Item
        {
            private get => _item;
            set
            {
                _item = value;
                RaisePropertyChanged(nameof(TriggerQueueView));
                var dirty = IsDirty;
                SetDisplayName(dirty);
                RaisePropertyChanged(nameof(IsDirty));
            }
        }

        void SetDisplayName(bool isDirty)
        {
            NameForDisplay = isDirty ? TriggerQueueName + " *" : TriggerQueueName;
        }

        public bool Equals(ITriggerQueue other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            var equals = true;
            equals &= QueueSourceId == other.QueueSourceId;
            equals &= string.Equals(QueueName, other.QueueName);
            equals &= string.Equals(WorkflowName, other.WorkflowName);
            equals &= Concurrency == other.Concurrency;
            equals &= string.Equals(UserName, other.UserName);
            equals &= string.Equals(Password, other.Password);
            equals &= Options == other.Options;
            equals &= QueueSinkId == other.QueueSinkId;
            equals &= string.Equals(DeadLetterQueue, other.DeadLetterQueue);
            equals &= DeadLetterOptions == other.DeadLetterOptions;
            equals &= Inputs == other.Inputs;

            return equals;
        }
    }
}
