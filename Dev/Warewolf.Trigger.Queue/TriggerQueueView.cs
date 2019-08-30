/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Data.TO;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.Queue;
using Dev2.Common.Interfaces.Resources;
using Dev2.Common.Interfaces.Threading;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Data;
using Dev2.Studio.Core;
using Dev2.Studio.Interfaces;
using Dev2.Threading;
using Dev2.Triggers;
using Microsoft.Practices.Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Warewolf.Core;
using Warewolf.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Warewolf.Trigger.Queue
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

        private string _oldQueueName;
        private bool _enabled;
        private IErrorResultTO _errors;
        private bool _isNewQueue;
        private bool _newQueue;
        private string _nameForDisplay;
        private TriggerQueueView _item;
        private readonly IResourceRepository _resourceRepository;
        private readonly IServer _server;
        readonly IAsyncWorker _asyncWorker;

        private bool _mapEntireMessage;
        private bool _isVerifying;
        private bool _verifyFailed;
        private bool _verifyPassed;
        private string _verifyResults;
        private bool _verifyResultsAvailable;
        private bool _isVerifyResultsEmptyRows;

        private DataListModel _dataList;
        private DataListConversionUtils _dataListConversionUtils;
        private IContextualResourceModel _contextualResourceModel;
        readonly IPopupController _popupController;
        private bool _isHistoryExpanded;
        private bool _isProgressBarVisible;
        private IList<IExecutionHistory> _history;
        private bool _isDirty;

        public TriggerQueueView(IServer server)
            : this(server, new SynchronousAsyncWorker())
        {

        }

        public TriggerQueueView(IServer server, IAsyncWorker asyncWorker)
        {
            VerifyArgument.IsNotNull(nameof(asyncWorker), asyncWorker);
            var activeServer = CustomContainer.Get<IShellViewModel>().ActiveServer;
            _server = server is null ? activeServer : server;
            _resourceRepository = _server.ResourceRepository;
            _asyncWorker = asyncWorker;
            _popupController = CustomContainer.Get<IPopupController>();

            IsNewQueue = false;
            NewQueue = true;
            Options = new List<OptionView>();
            DeadLetterOptions = new List<OptionView>();
            Inputs = new List<IServiceInput>();
            VerifyCommand = new DelegateCommand(ExecuteVerify);
            MapEntireMessage = true;
        }

        public void ToModel(ITriggerQueue queue)
        {
            NewQueue = false;
            TriggerId = queue.TriggerId;
            TriggerQueueName = queue.Name;
            SelectedQueueSource = QueueSources.FirstOrDefault(o => o.ResourceID == queue.QueueSourceId);
            QueueName = queue.QueueName;
            WorkflowName = queue.WorkflowName;
            Concurrency = queue.Concurrency;
            UserName = queue.UserName;
            Password = queue.Password;
            if (queue.Options != null)
            {
                Options = FindOptions(queue.Options.ToList());
            }

            SelectedDeadLetterQueueSource = DeadLetterQueueSources.FirstOrDefault(o => o.ResourceID == queue.QueueSinkId);
            DeadLetterQueue = queue.DeadLetterQueue;
            if (queue.DeadLetterOptions != null)
            {
                DeadLetterOptions = FindOptions(queue.DeadLetterOptions.ToList());
            }

            Inputs = queue.Inputs;
        }

        public Guid TriggerId { get; set; }
        public string TriggerQueueName
        {
            get => _triggerQueueName;
            set
            {
                _triggerQueueName = value;
                RaisePropertyChanged(nameof(TriggerQueueName));
                IsDirtyPropertyChange();
            }
        }

        private void IsDirtyPropertyChange()
        {
            if (NewQueue)
            {
                IsDirty = true;
            }
            else
            {
                IsDirty = !Equals(Item);
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
                    QueueSourceId = _selectedQueueSource.ResourceID;
                    QueueNames = GetQueueNamesFromSource();

                    var options = _resourceRepository.FindOptions(_server, _selectedQueueSource);
                    Options = FindOptions(options);
                }

                RaisePropertyChanged(nameof(SelectedQueueSource));
                IsDirtyPropertyChange();
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
                IsDirtyPropertyChange();
            }
        }
        public string WorkflowName
        {
            get => _workflowName;
            set
            {
                _workflowName = value;
                RaisePropertyChanged(nameof(WorkflowName));
                IsDirtyPropertyChange();
            }
        }
        public int Concurrency
        {
            get => _concurrency;
            set
            {
                _concurrency = value;
                RaisePropertyChanged(nameof(Concurrency));
                IsDirtyPropertyChange();
            }
        }
        public string UserName
        {
            get => _userName;
            set
            {
                _userName = value;
                RaisePropertyChanged(nameof(UserName));
                IsDirtyPropertyChange();
            }
        }
        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                RaisePropertyChanged(nameof(Password));
                IsDirtyPropertyChange();
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
                    QueueSinkId = _selectedDeadLetterQueueSource.ResourceID;
                    DeadLetterQueues = GetQueueNamesFromSource();

                    var options = _resourceRepository.FindOptions(_server, _selectedDeadLetterQueueSource);
                    DeadLetterOptions = FindOptions(options);
                }

                RaisePropertyChanged(nameof(SelectedDeadLetterQueueSource));
                IsDirtyPropertyChange();
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
                IsDirtyPropertyChange();
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
                return _isDirty;
            }
            set
            {
                if (_isDirty != value)
                {
                    _isDirty = value;
                    SetDisplayName(_isDirty);
                    RaisePropertyChanged(nameof(IsDirty));
                }
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

        public bool MapEntireMessage
        {
            get => _mapEntireMessage;
            set
            {
                _mapEntireMessage = value;
                RaisePropertyChanged(nameof(MapEntireMessage));
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

        public void ShowInvalidDataPopupMessage()
        {
            _popupController.Show(StringResources.DataInput_Error,
                                  StringResources.DataInput_Error_Title,
                                  MessageBoxButton.OK, MessageBoxImage.Error, string.Empty, false, true, false, false, false, false);
        }

        public void ExecuteVerify()
        {
            _isVerifying = true;
            try
            {
                if (_verifyResults != null)
                {
                    _dataList = new DataListModel();
                    _dataList.Create(VerifyResults, _contextualResourceModel.DataList);
                    var inputList = _dataListConversionUtils.GetInputs(_dataList);
                    Inputs = inputList.Select(sca =>
                    {
                        if (sca.IsObject)
                        {
                            var value = "";
                            if (JsonConvert.DeserializeObject(_verifyResults.Replace("@", "")) is JObject obj)
                            {
                                value = obj[sca.DisplayValue.Replace("@", "")].ToString();
                            }
                            var serviceTestInput = new ServiceInput(sca.DisplayValue, value);
                            return serviceTestInput.As<IServiceInput>();
                        }
                        else
                        {
                            var serviceTestInput = new ServiceInput(sca.DisplayValue, sca.Value);
                            return serviceTestInput.As<IServiceInput>();
                        }

                    }).ToList();
                    _isVerifyResultsEmptyRows = VerifyResults == null;
                    _verifyResultsAvailable = true;
                    _isVerifyResultsEmptyRows = VerifyResults == string.Empty;
                    _isVerifying = false;
                    _verifyPassed = true;
                    _verifyFailed = false;
                }
                else
                {
                    _isVerifying = false;
                    _verifyPassed = false;
                    _verifyFailed = true;
                    ShowInvalidDataPopupMessage();
                }
            }
            catch (Exception e)
            {
                var msg = e.Message;
                _isVerifying = false;
                _verifyPassed = false;
                _verifyFailed = true;
                ShowInvalidDataPopupMessage();
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
                return serviceTestInput.As<IServiceInput>();

            }).ToList();
        }
        public TriggerQueueView Item
        {
            private get => _item;
            set
            {
                _item = value;
                RaisePropertyChanged(nameof(Item));
                var dirty = IsDirty;
                SetDisplayName(dirty);
                IsDirtyPropertyChange();
            }
        }

        private IList<INameValue> GetQueueNamesFromSource()
        {
            var queueNames = new List<INameValue>();
            return queueNames;
        }

        private List<OptionView> FindOptions(List<Options.IOption> options)
        {
            var optionViews = new List<OptionView>();
            
            foreach (var option in options)
            {
                var optionView = new OptionView(option);
                optionViews.Add(optionView);
            }
            return optionViews;
        }

        public bool IsHistoryExpanded
        {
            get => _isHistoryExpanded;
            set
            {
                _isHistoryExpanded = value;
                RaisePropertyChanged(nameof(IsHistoryExpanded));
                RaisePropertyChanged(nameof(History));
            }
        }

        public IList<IExecutionHistory> History
        {
            get
            {
                if (!IsHistoryExpanded)
                {
                    return new List<IExecutionHistory>();
                }
                if (_history == null && !IsNewQueue)
                {
                    _asyncWorker.Start(() =>
                    {
                        IsProgressBarVisible = true;
                        _history = _resourceRepository.GetTriggerQueueHistory(ResourceId);
                    }
                   , () =>
                   {
                       IsProgressBarVisible = false;
                       RaisePropertyChanged(nameof(History));
                   });
                }
                var history = _history;
                _history = null;
                return history ?? new List<IExecutionHistory>();
            }
        }

        public bool IsProgressBarVisible
        {
            get => _isProgressBarVisible;
            set
            {
                _isProgressBarVisible = value;
                RaisePropertyChanged(nameof(IsProgressBarVisible));
            }
        }

        public void SetItem(TriggerQueueView model)
        {
            Item = model.MemberwiseClone() as TriggerQueueView;
        }

        void SetDisplayName(bool isDirty)
        {
            NameForDisplay = isDirty ? TriggerQueueName + " *" : TriggerQueueName;
        }

        public bool Equals(TriggerQueueView other)
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
            equals &= string.Equals(_triggerQueueName, other._triggerQueueName);
            equals &= _queueSourceId == other._queueSourceId;
            equals &= string.Equals(_queueName, other._queueName);
            equals &= string.Equals(_workflowName, other._workflowName);
            equals &= _concurrency == other._concurrency;
            equals &= string.Equals(_userName, other._userName);
            equals &= string.Equals(_password, other._password);
            equals &= _queueSinkId == other._queueSinkId;
            equals &= string.Equals(_deadLetterQueue, other._deadLetterQueue);

            return equals;
        }
    }
}
