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
using System.Linq;
using System.Windows.Input;
using Dev2;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Data.TO;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.Resources;
using Dev2.Data;
using Dev2.Studio.Interfaces;
using Dev2.Triggers;
using Microsoft.Practices.Prism.Commands;
using Warewolf.Core;
using Warewolf.Options;
using Warewolf.UI;

namespace Warewolf.Trigger
{
    public class TriggerQueueView : BindableBase
    {
        private string _triggerQueueName;
        IResource _selectedQueueSource;
        private Guid _queueSourceId;
        private IList<INameValue> _queueNames;
        private string _queueName;
        private string _workflowName;
        private int _concurrency;
        private string _userName;
        private string _password;
        private List<OptionView> _options;
        private IResource _selectedDeadLetterQueueSource;
        private Guid _queueSinkId;
        private IList<INameValue> _deadLetterQueues;
        private string _deadLetterQueue;
        private List<OptionView> _deadLetterOptions;
        private ICollection<IServiceInput> _inputs;

        private bool _isDirty;
        private string _oldQueueName;
        private bool _enabled;
        private IErrorResultTO _errors;
        private bool _isNewQueue;
        private bool _newQueue;
        private string _nameForDisplay;
        private TriggerQueueView _item;
        private bool _isValidatingIsDirty;
        private IResourceRepository _resourceRepository;
        private IServer _server;

        private bool _isVerifying;
        private bool _verifyFailed;
        private bool _verifyPassed;
        private string _verifyResults;
        private bool _verifyResultsAvailable;
        private bool _isVerifyResultsEmptyRows;

        private DataListModel _dataList;
        private DataListConversionUtils _dataListConversionUtils;
        private IContextualResourceModel _contextualResourceModel;

        public TriggerQueueView(IServer server)
        {
            var activeServer = CustomContainer.Get<IShellViewModel>().ActiveServer;
            _server = server is null ? activeServer : server;
            _resourceRepository = _server.ResourceRepository;
            IsNewQueue = false;
            NewQueue = true;
            Options = new List<OptionView>();
            DeadLetterOptions = new List<OptionView>();
            Inputs = new List<IServiceInput>();
            VerifyCommand = new DelegateCommand(ExecuteVerify);
        }

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
        public List<IResource> QueueSources => _resourceRepository.FindResourcesByType<IQueueSource>(_server);
        public IResource SelectedQueueSource
        {
            get => _selectedQueueSource;
            set
            {
                _selectedQueueSource = value;
                if (_selectedQueueSource != null)
                {
                    QueueNames = GetQueueNamesFromSource(_selectedQueueSource);
                    Options = FindOptions(_selectedQueueSource);
                }

                RaisePropertyChanged(nameof(SelectedQueueSource));
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
        public IList<INameValue> QueueNames
        {
            get => _queueNames;
            set
            {
                _queueNames = value;
                RaisePropertyChanged(nameof(QueueNames));
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
        public List<OptionView> Options
        {
            get => _options;
            set
            {
                _options = value;
                RaisePropertyChanged(nameof(Options));
            }
        }
        public List<IResource> DeadLetterQueueSources => _resourceRepository.FindResourcesByType<IQueueSource>(_server);
        public IResource SelectedDeadLetterQueueSource
        {
            get => _selectedDeadLetterQueueSource;
            set
            {
                _selectedDeadLetterQueueSource = value;
                if (_selectedDeadLetterQueueSource != null)
                {
                    DeadLetterQueues = GetQueueNamesFromSource(_selectedDeadLetterQueueSource);
                    DeadLetterOptions = FindOptions(_selectedDeadLetterQueueSource);
                }

                RaisePropertyChanged(nameof(SelectedDeadLetterQueueSource));
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
        public IList<INameValue> DeadLetterQueues
        {
            get => _deadLetterQueues;
            set
            {
                _deadLetterQueues = value;
                RaisePropertyChanged(nameof(DeadLetterQueues));
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
        public List<OptionView> DeadLetterOptions
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

        public bool IsVerifying
        {
            get => _isVerifying;
            set
            {
                _isVerifying = value;
                RaisePropertyChanged(nameof(IsVerifying));
            }
        }

        public bool VerifyFailed
        {
            get => _verifyFailed;
            set
            {
                _verifyFailed = value;
                RaisePropertyChanged(nameof(VerifyFailed));
            }
        }

        public bool VerifyPassed
        {
            get => _verifyPassed;
            set
            {
                _verifyPassed = value;
                RaisePropertyChanged(nameof(VerifyPassed));
            }
        }

        public bool VerifyResultsAvailable
        {
            get => _verifyResultsAvailable;
            set
            {
                _verifyResultsAvailable = value;
                RaisePropertyChanged(nameof(VerifyResultsAvailable));
            }
        }

        public bool IsVerifyResultsEmptyRows
        {
            get => _isVerifyResultsEmptyRows;
            set
            {
                _isVerifyResultsEmptyRows = value;
                RaisePropertyChanged(nameof(IsVerifyResultsEmptyRows));
            }
        }

        public string VerifyResults
        {
            get => _verifyResults;
            set
            {
                _verifyResults = value;
                RaisePropertyChanged(nameof(VerifyResults));
            }
        }

        public ICommand AddWorkflowCommand { get; private set; }
        public ICommand VerifyCommand { get; private set; }

        public void ExecuteVerify()
        {
            _isVerifying = true;
            try
            {
                _dataList = new DataListModel();
                _dataList.Create(VerifyResults, _contextualResourceModel.DataList);
                var inputList = _dataListConversionUtils.GetInputs(_dataList);
                Inputs = inputList.Select(sca =>
                {
                    var serviceTestInput = new ServiceInput(sca.DisplayValue, sca.Value);
                    return (IServiceInput)serviceTestInput;

                }).ToList();
                IsVerifyResultsEmptyRows = VerifyResults == null;
                if (VerifyResults != null)
                {
                    VerifyResultsAvailable = true;
                    IsVerifyResultsEmptyRows = VerifyResults == string.Empty;
                    IsVerifying = false;
                    VerifyPassed = true;
                    VerifyFailed = false;
                }
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                IsVerifying = false;
                VerifyPassed = false;
                VerifyFailed = true;
            }
        }
        public void GetInputsFromWorkflow()
        {
            Inputs = new List<IServiceInput>();
            _contextualResourceModel = _server.ResourceRepository.LoadContextualResourceModel(ResourceId);
            _dataList = new DataListModel();
            _dataListConversionUtils = new DataListConversionUtils();
            _dataList.Create(_contextualResourceModel.DataList, _contextualResourceModel.DataList);
            var inputList = _dataListConversionUtils.GetInputs(_dataList);
            Inputs = inputList.Select(sca =>
            {
                var serviceTestInput = new ServiceInput(sca.DisplayValue, "");
                return (IServiceInput)serviceTestInput;

            }).ToList();
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

        private IList<INameValue> GetQueueNamesFromSource(IResource selectedQueueSource)
        {
            var queueNames = new List<INameValue>();

            var list = _resourceRepository.FindAutocompleteOptions(_server, selectedQueueSource);

#pragma warning disable CC0021 // Use nameof
            foreach (var item in list["QueueNames"])
#pragma warning restore CC0021 // Use nameof
            {
                var nameValue = new NameValue(item, item);
                queueNames.Add(nameValue);
            }

            return queueNames;
        }

        private List<OptionView> FindOptions(IResource selectedQueueSource)
        {
            var optionViews = new List<OptionView>();
            var options = _resourceRepository.FindOptions(_server, selectedQueueSource);
            foreach (var option in options)
            {
                var optionView = new OptionView(option);
                optionViews.Add(optionView);
            }
            return optionViews;
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
            //equals &= Options == other.Options;
            equals &= QueueSinkId == other.QueueSinkId;
            equals &= string.Equals(DeadLetterQueue, other.DeadLetterQueue);
            //equals &= DeadLetterOptions == other.DeadLetterOptions;
            equals &= Inputs == other.Inputs;

            return equals;
        }
    }
}
