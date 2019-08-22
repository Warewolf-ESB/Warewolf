/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Activities.Designers2.Core;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Data.TO;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.Queue;
using Dev2.Common.Interfaces.Resources;
using Dev2.Common.Interfaces.Threading;
using Dev2.Common.Serializers;
using Dev2.Data.TO;
using Dev2.Dialogs;
using Dev2.Studio.Enums;
using Dev2.Runtime.Triggers;
using Dev2.Studio.Interfaces;
using Dev2.Threading;
using Microsoft.Practices.Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using Warewolf.Options;
using Warewolf.Studio.Resources.Languages;
using Warewolf.Studio.ViewModels;
using Warewolf.Trigger;
using Warewolf.UI;
using Dev2.Common.Interfaces.Studio.Controller;
using System.Windows;
using System.Collections.ObjectModel;
using Dev2.Common.Common;

namespace Dev2.Triggers.QueueEvents
{
    public class QueueEventsViewModel : TasksItemViewModel, IUpdatesHelp
    {
        ICommand _newCommand;
        ICommand _deleteCommand;
        IResource _selectedQueueSource;
        IResource _selectedDeadLetterQueueSource;
        string _selectedQueueEvent;
        string _queueName;
        string _deadLetterQueue;
        string _workflowName;
        int _concurrency;
        private readonly IServer _server;
        private readonly IResourceRepository _resourceRepository;
        readonly IExternalProcessExecutor _externalProcessExecutor;
        private IList<INameValue> _queueNames;

        private IList<INameValue> _deadLetterQueues;
        private ICollection<IServiceInput> _inputs;
        private string _pasteResponse;
        private ICommand _queueStatsCommand;
        private bool _isVerifying;
        private bool _verifyFailed;
        private bool _verifyPassed;
        private bool _verifyResultsAvailable;
        private bool _isVerifyResultsEmptyRows;
        private bool _isHistoryExpanded;
        private string _verifyResults;
        IList<IExecutionHistory> _history;
        readonly IAsyncWorker _asyncWorker;
        private readonly EnvironmentViewModel _source;
        IResourcePickerDialog _currentResourcePicker;
        string _connectionError;
        bool _hasConnectionError;
        bool _isProgressBarVisible;
        bool _isHistoryTabVisible;
        ITriggerQueueResourceModel _queueResourceModel;
        TriggerQueueView _selectedQueue;
        bool _errorShown;
        readonly Dev2JsonSerializer _ser = new Dev2JsonSerializer();
        bool _isDirty;
        private bool _enabled;
        TabItem _activeItem;
        private List<OptionView> _options;
        private List<OptionView> _deadLetterOptions;
        private ObservableCollection<TriggerQueueView> _queues;

        public IPopupController PopupController { get; }

        public QueueEventsViewModel(IServer server)
            : this(server, new ExternalProcessExecutor(), new SynchronousAsyncWorker(), null)
        {

        }

        public QueueEventsViewModel(IServer server, IExternalProcessExecutor externalProcessExecutor, IAsyncWorker asyncWorker, IResourcePickerDialog resourcePickerDialog)
        {
            VerifyArgument.IsNotNull(nameof(asyncWorker), asyncWorker);
            _server = server;
            _resourceRepository = server.ResourceRepository;
            _externalProcessExecutor = externalProcessExecutor;
            Inputs = new List<IServiceInput>();
            VerifyCommand = new DelegateCommand(ExecuteVerify);
            AddWorkflowCommand = new DelegateCommand(OpenResourcePicker);
            IsVerifying = false;
            _source = new EnvironmentViewModel(server, CustomContainer.Get<IShellViewModel>(), true);
            _currentResourcePicker = resourcePickerDialog ?? CreateResourcePickerDialog();
            Errors = new ErrorResultTO();
            _asyncWorker = asyncWorker;
            InitializeHelp();
            Options = new List<OptionView>();
            DeadLetterOptions = new List<OptionView>();
            PopupController = CustomContainer.Get<IPopupController>();
            Queues = new ObservableCollection<TriggerQueueView>();
            AddDummyTriggerQueueView();
        }

        private void AddDummyTriggerQueueView()
        {
            var dummyTriggerQueueView = new DummyTriggerQueueView();
            Queues.Add(dummyTriggerQueueView);
        }

        private void OpenResourcePicker()
        {
            if (_currentResourcePicker.ShowDialog(_server))
            {
                var selectedResource = _currentResourcePicker.SelectedResource;
                WorkflowName = selectedResource.ResourcePath;
                SelectedQueue.ResourceId = selectedResource.ResourceId;
                SelectedQueue.WorkflowName = selectedResource.ResourcePath;
            }
        }

        public void ExecuteVerify()
        {
            VerifyResults = null;
            _isVerifying = true;

            try
            {
                VerifyResults = "{some text}";

                IsVerifyResultsEmptyRows = VerifyResults == null;
                if (VerifyResults != null)
                {
                    VerifyResultsAvailable = true;
                    IsVerifyResultsEmptyRows = VerifyResults == string.Empty;
                    _isVerifying = false;
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

        public ObservableCollection<TriggerQueueView> Queues
        {
            get => _queues;
            set
            {
                _queues = value;
                OnPropertyChanged(nameof(Queues));
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

                OnPropertyChanged(nameof(SelectedQueueSource));
            }
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

        public List<OptionView> Options
        {
            get => _options;
            set
            {
                _options = value;
                OnPropertyChanged(nameof(Options));
            }
        }

        public List<OptionView> DeadLetterOptions
        {
            get => _deadLetterOptions;
            set
            {
                _deadLetterOptions = value;
                OnPropertyChanged(nameof(DeadLetterOptions));
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

                OnPropertyChanged(nameof(SelectedDeadLetterQueueSource));
            }
        }

        IResourcePickerDialog CreateResourcePickerDialog()
        {
            var res = new ResourcePickerDialog(enDsfActivityType.All, _source);
            ResourcePickerDialog.CreateAsync(enDsfActivityType.Workflow, _source).ContinueWith(a => _currentResourcePicker = a.Result);
            return res;
        }

        Task<IResourcePickerDialog> GetResourcePickerDialog => ResourcePickerDialog.CreateAsync(enDsfActivityType.Workflow, _source);


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

        public IList<INameValue> QueueNames
        {
            get => _queueNames;
            set
            {
                _queueNames = value;
                OnPropertyChanged(nameof(QueueNames));
            }
        }

        public string QueueName
        {
            get => _queueName;
            set
            {
                _queueName = value;
                OnPropertyChanged(nameof(QueueName));
            }
        }

        public IList<INameValue> DeadLetterQueues
        {
            get => _deadLetterQueues;
            set
            {
                _deadLetterQueues = value;
                OnPropertyChanged(nameof(DeadLetterQueues));
            }
        }

        public string DeadLetterQueue
        {
            get => _deadLetterQueue;
            set
            {
                _deadLetterQueue = value;
                OnPropertyChanged(nameof(DeadLetterQueue));
            }
        }

        public string WorkflowName
        {
            get => _workflowName;
            set
            {
                _workflowName = value;
                OnPropertyChanged(nameof(WorkflowName));
            }
        }

        public int Concurrency
        {
            get => _concurrency;
            set
            {
                _concurrency = value;
                OnPropertyChanged(nameof(Concurrency));
            }
        }

        public ICollection<IServiceInput> Inputs
        {
            get => _inputs;
            set
            {
                _inputs = value;
                OnPropertyChanged(nameof(Inputs));
            }
        }

        public string PasteResponse
        {
            get => _pasteResponse;
            set
            {
                _pasteResponse = value;
                OnPropertyChanged(nameof(PasteResponse));
            }
        }

        public bool VerifyResultsAvailable
        {
            get => _verifyResultsAvailable;
            set
            {
                _verifyResultsAvailable = value;
                OnPropertyChanged(nameof(VerifyResultsAvailable));
            }
        }

        public bool IsVerifyResultsEmptyRows
        {
            get => _isVerifyResultsEmptyRows;
            set
            {
                _isVerifyResultsEmptyRows = value;
                OnPropertyChanged(nameof(IsVerifyResultsEmptyRows));
            }
        }

        public string VerifyResults
        {
            get => _verifyResults;
            set
            {
                _verifyResults = value;
                if (!string.IsNullOrEmpty(_verifyResults))
                {
                    //Model.Response = _verifyResults
                }
                OnPropertyChanged(nameof(VerifyResults));
            }
        }

        public ICommand QueueStatsCommand => _queueStatsCommand ??
            (_queueStatsCommand = new DelegateCommand(ViewQueueStats));

        private void ViewQueueStats()
        {
            _externalProcessExecutor.OpenInBrowser(new Uri("https://www.rabbitmq.com/blog/tag/statistics/"));
        }

        public ICommand NewCommand => _newCommand ??
                       (_newCommand = new DelegateCommand(CreateNewQueueEvent));

        IEnumerable<TriggerQueueView> RealTests() => _queues.Where(model => model.GetType() != typeof(DummyTriggerQueueView)).ToObservableCollection();

        private void CreateNewQueueEvent()
        {
            SelectedQueue = null;
            if (IsDirty)
            {
                PopupController?.Show(Core.TriggerQueueSaveEditedTestsMessage, Core.TriggerQueueSaveEditedQueueHeader, MessageBoxButton.OK, MessageBoxImage.Error, null, false, true, false, false, false, false);
                return;
            }

            var queueNumber = GetNewQueueNumber("Queue");

            var queue = new TriggerQueueView
            {
                IsNewQueue = false,
                NewQueue = true,
                TriggerQueueName = "Queue " + (queueNumber == 0 ? 1 : queueNumber)
            };

            AddAndSelectQueue(queue);
        }

        int GetNewQueueNumber(string name)
        {
            var counter = 1;
            var fullName = name + " " + counter;

            while (Contains(fullName))
            {
                counter++;
                fullName = name + " " + counter;
            }

            return counter;
        }

        bool Contains(string nameToCheck)
        {
            var triggerQueue = RealTests().FirstOrDefault(a => a.TriggerQueueName.Contains(nameToCheck));
            return triggerQueue != null;
        }

        void AddAndSelectQueue(TriggerQueueView triggerQueueView)
        {
            var index = _queues.Count - 1;
            if (index >= 0)
            {
                _queues.Insert(index, triggerQueueView);
            }
            else
            {
                _queues.Add(triggerQueueView);
            }
            SelectedQueue = triggerQueueView;
        }

        public ICommand DeleteCommand => _deleteCommand ??
                       (_deleteCommand = new DelegateCommand(DeleteQueueEvent));

        private void DeleteQueueEvent()
        {
            Queues.Remove(SelectedQueue);
        }

        public ICommand VerifyCommand { get; private set; }
        public ICommand AddWorkflowCommand { get; private set; }

        public bool IsVerifying
        {
            get => _isVerifying;
            set
            {
                _isVerifying = value;
                OnPropertyChanged(nameof(IsVerifying));
            }
        }

        public bool VerifyFailed
        {
            get => _verifyFailed;
            set
            {
                _verifyFailed = value;
                OnPropertyChanged(nameof(VerifyFailed));
            }
        }

        public bool VerifyPassed
        {
            get => _verifyPassed;
            set
            {
                _verifyPassed = value;
                OnPropertyChanged(nameof(VerifyPassed));
            }
        }

        public void UpdateHelpDescriptor(string helpText)
        {
            HelpText = helpText;
        }

        protected override void CloseHelp()
        {

        }

        public bool Save()
        {
            ITriggerQueue triggerQueue = new TriggerQueue
            {
                QueueSourceId = SelectedQueueSource.ResourceID,
                QueueName = QueueName,
                WorkflowName = WorkflowName,
                Concurrency = Concurrency,
                UserName = AccountName,
                Password = Password,
                Options = new IOption[] { },
                QueueSinkId = SelectedDeadLetterQueueSource.ResourceID,
                DeadLetterQueue = DeadLetterQueue,
                DeadLetterOptions = new IOption[] { },
                Inputs = Inputs
            };

            foreach (var option in Options)
            {
                triggerQueue.Options = new IOption[] { option.DataContext };
            }
            foreach (var option in DeadLetterOptions)
            {
                triggerQueue.DeadLetterOptions = new IOption[] { option.DataContext };
            }

            TriggersCatalog.Instance.SaveTriggerQueue(triggerQueue);

            return true;
        }

        public string SelectedQueueEvent
        {
            get => _selectedQueueEvent;
            set
            {
                _selectedQueueEvent = value;
                OnPropertyChanged(nameof(SelectedQueueEvent));
            }
        }
        public TabItem ActiveItem
        {
            private get => _activeItem;
            set
            {
                if (Equals(value, _activeItem))
                {
                    return;
                }
                _activeItem = value;
                if (IsHistoryTab)
                {
                    OnPropertyChanged(nameof(History));
                }
            }
        }
        bool IsHistoryTab
        {
            get
            {
                if (ActiveItem != null)
                {
                    return (string)ActiveItem.Header == nameof(History);
                }
                return false;
            }
        }
        public ITriggerQueueResourceModel QueueResourceModel
        {
            get => _queueResourceModel;
            set
            {
                _queueResourceModel = value;
                OnPropertyChanged(nameof(QueueResourceModel));
                OnPropertyChanged(nameof(ExecutionHistory));
            }
        }

        public bool IsHistoryExpanded
        {
            get => _isHistoryExpanded;
            set
            {
                _isHistoryExpanded = value;
                OnPropertyChanged(nameof(IsHistoryExpanded));
                OnPropertyChanged(nameof(History));
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
                if (QueueResourceModel != null && SelectedQueue != null && _history == null && !SelectedQueue.IsNewQueue)
                {
                    _asyncWorker.Start(() =>
                    {
                        IsHistoryTabVisible = false;
                        IsProgressBarVisible = true;
                        //_history = QueueResourceModel.CreateHistory(SelectedQueue).ToList()
                    }
                   , () =>
                   {

                       IsHistoryTabVisible = true;
                       IsProgressBarVisible = false;
                       OnPropertyChanged(nameof(History));
                   });
                }
                var history = _history;
                _history = null;
                return history ?? new List<IExecutionHistory>();
            }
        }
        public bool IsHistoryTabVisible
        {
            get => _isHistoryTabVisible;
            private set
            {
                _isHistoryTabVisible = value;
                OnPropertyChanged(nameof(IsHistoryTabVisible));
            }
        }
        public bool IsProgressBarVisible
        {
            get => _isProgressBarVisible;
            set
            {
                _isProgressBarVisible = value;
                OnPropertyChanged(nameof(IsProgressBarVisible));
            }
        }
        public string AccountName
        {
            get => SelectedQueue != null ? SelectedQueue.UserName : string.Empty;
            set
            {
                if (SelectedQueue != null)
                {
                    if (Equals(SelectedQueue.UserName, value))
                    {
                        return;
                    }
                    ClearError(Core.SchedulerLoginErrorMessage);
                    SelectedQueue.UserName = value;
                    OnPropertyChanged(nameof(IsDirty));
                    OnPropertyChanged(nameof(AccountName));
                }
            }
        }
        public TriggerQueueView SelectedQueue
        {
            get => _selectedQueue;
            set
            {
                if (value == null)
                {
                    _selectedQueue = null;
                    OnPropertyChanged(nameof(SelectedQueue));
                    return;
                }
                if (Equals(_selectedQueue, value) || value.IsNewQueue)
                {
                    return;
                }
                _selectedQueue = value;
                Item = _ser.Deserialize<TriggerQueueView>(_ser.SerializeToBuilder(_selectedQueue));
                OnPropertyChanged(nameof(SelectedQueue));
                if (_selectedQueue != null)
                {
                    OnPropertyChanged(nameof(Enabled));
                    OnPropertyChanged(nameof(WorkflowName));
                    OnPropertyChanged(nameof(QueueName));
                    OnPropertyChanged(nameof(AccountName));
                    OnPropertyChanged(nameof(Password));
                    OnPropertyChanged(nameof(Errors));
                    OnPropertyChanged(nameof(Error));
                    OnPropertyChanged(nameof(History));
                }
            }
        }
        void InitializeHelp()
        {
            HelpToggle = CreateHelpToggle();
            HelpText = Warewolf.Studio.Resources.Languages.HelpText.SchedulerSettingsHelpTextSettingsView;
        }
        public ActivityDesignerToggle HelpToggle { get; private set; }
        static ActivityDesignerToggle CreateHelpToggle()
        {
            var toggle = ActivityDesignerToggle.Create("ServiceHelp", "Close Help", "ServiceHelp", "Open Help", nameof(HelpToggle));

            return toggle;
        }

        public new bool IsDirty
        {
            get
            {
                try
                {
                    if (SelectedQueue == null)
                    {
                        SetQueueName(false);
                        return false;
                    }
                    var dirty = !SelectedQueue.Equals(Item);
                    //SelectedQueue.IsDirty = dirty;
                    SetQueueName(dirty);
                    return dirty;
                }
                catch (Exception ex)
                {
                    if (!_errorShown)
                    {
                        //TODO: Not sure if this is required
                        // _popupController.ShowCorruptTaskResult(ex.Message);
                        Dev2Logger.Error(ex, GlobalConstants.WarewolfError);
                        _errorShown = true;
                    }
                }
                return false;
            }
            set
            {
                if (value.Equals(_isDirty))
                {
                    return;
                }
                _isDirty = value;
                OnPropertyChanged(nameof(IsDirty));
            }
        }
        public IServer Server { private get; set; }
        void SetQueueName(bool dirty)
        {
            var baseName = "Queue";
            if (Server != null)
            {
                baseName = baseName + " - " + Server.DisplayName;
            }
            if (dirty)
            {
                if (!baseName.EndsWith(" *"))
                {
                    QueueName = baseName + " *";
                }
            }
            else
            {
                QueueName = baseName;
            }
        }
        public IList<ITriggerQueue> ExecutionHistory => QueueResourceModel != null ? QueueResourceModel.QueueResources : new List<ITriggerQueue>();

        public TriggerQueueView Item { get; set; }
        public string Password
        {
            get => SelectedQueue != null ? SelectedQueue.Password : string.Empty;
            set
            {
                if (SelectedQueue != null)
                {
                    if (Equals(SelectedQueue.Password, value))
                    {
                        return;
                    }
                    ClearError(Core.QueueEventsLoginErrorMessage);
                    SelectedQueue.Password = value;
                    OnPropertyChanged(nameof(IsDirty));
                    OnPropertyChanged(nameof(Password));
                }
            }
        }

        public void ClearError(string description)
        {
            Errors.RemoveError(description);
            OnPropertyChanged(nameof(Error));
            OnPropertyChanged(nameof(HasErrors));
        }

        public bool Enabled
        {
            get => _enabled;
            set
            {
                _enabled = value;
                OnPropertyChanged(nameof(Enabled));
                OnPropertyChanged(nameof(IsDirty));
            }
        }

        public void ShowError(string description)
        {
            if (!string.IsNullOrEmpty(description))
            {
                Errors.AddError(description, true);
                OnPropertyChanged(nameof(Error));
                OnPropertyChanged(nameof(HasErrors));
            }
        }
        public bool HasErrors => false;// Errors.HasErrors();
        public IErrorResultTO Errors
        {
            get => _selectedQueue != null ? _selectedQueue.Errors : new ErrorResultTO();
            private set
            {
                if (_selectedQueue != null)
                {
                    _selectedQueue.Errors = value;
                }

                OnPropertyChanged(nameof(Errors));
            }
        }

        public string Error => HasErrors ? Errors.FetchErrors()[0] : string.Empty;
        public void ClearConnectionError()
        {
            ConnectionError = string.Empty;
            HasConnectionError = false;
        }
        public void SetConnectionError()
        {
            ConnectionError = Core.QueueConnectionError;
            HasConnectionError = true;
        }
        public bool HasConnectionError
        {
            get => _hasConnectionError;
            set
            {
                _hasConnectionError = value;
                OnPropertyChanged(nameof(HasConnectionError));
            }
        }
        public string ConnectionError
        {
            get => _connectionError;
            set
            {
                _connectionError = value;
                OnPropertyChanged(nameof(ConnectionError));
            }
        }
    }
}
