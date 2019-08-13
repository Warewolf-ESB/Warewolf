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
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Queue;
using Dev2.Common.Interfaces.Resources;
using Dev2.Common.Interfaces.Threading;
using Dev2.Common.Serializers;
using Dev2.Data.TO;
using Dev2.Studio.Interfaces;
using Dev2.Studio.ViewModels.Diagnostics;
using Dev2.Threading;
using Microsoft.Practices.Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using Warewolf.Options;
using Warewolf.Studio.Resources.Languages;
using Warewolf.UI;

namespace Dev2.Tasks.QueueEvents
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
        private IServer _server;
        private IResourceRepository _resourceRepository;
        IExternalProcessExecutor _externalProcessExecutor;
        private ObservableCollection<INameValue> _queueNames;
        private ObservableCollection<INameValue> _deadLetterQueues;
        private ICollection<IServiceInput> _inputs;
        private bool _pasteResponseVisible;
        private string _pasteResponse;
        private ICommand _queueStatsCommand;
        private bool _isTesting;
        private bool _testFailed;
        private bool _testPassed;
        private bool _testResultsAvailable;
        private bool _isTestResultsEmptyRows;
        private string _testResults;
        IExecutionHistory _selectedHistory;
        IList<IExecutionHistory> _history;
        readonly IAsyncWorker _asyncWorker;
        string _connectionError;
        bool _hasConnectionError;
        bool _isProgressBarVisible;
        bool _isHistoryTabVisible;
        IQueueResourceModel _queueResourceModel;
        IQueueResource _selectedQueue;
        DebugOutputViewModel _debugOutputViewModel;
        bool _errorShown;
        readonly Dev2JsonSerializer _ser = new Dev2JsonSerializer();
        bool _isDirty;
        TabItem _activeItem;
        private List<OptionView> _options;

        //TODO: Not sure we requre this
        //  readonly IPopupController _popupController;

        public QueueEventsViewModel(IServer server)
            : this(server, new ExternalProcessExecutor(), new SynchronousAsyncWorker())
        {

        }

        public QueueEventsViewModel(IServer server, IExternalProcessExecutor externalProcessExecutor, IAsyncWorker asyncWorker)
        {
            _server = server;
            _resourceRepository = server.ResourceRepository;
            _externalProcessExecutor = externalProcessExecutor;
            Inputs = new ObservableCollection<IServiceInput>();
            PasteResponseCommand = new DelegateCommand(ExecutePaste);
            TestCommand = new DelegateCommand(ExecuteTest);
            IsTesting = false;
            Errors = new ErrorResultTO();
            VerifyArgument.IsNotNull(nameof(asyncWorker), asyncWorker);
            _asyncWorker = asyncWorker;

            InitializeHelp();
            //TODO:Not sure if this required
            // var serverRepository = CustomContainer.Get<IServerRepository>();
            //  DebugOutputViewModel = new DebugOutputViewModel(new EventPublisher(), serverRepository, new DebugOutputFilterStrategy());

        }


        public void ExecutePaste()
        {
            PasteResponseVisible = true;
        }

        public void ExecuteTest()
        {
            TestResults = null;
            IsTesting = true;

            try
            {
                TestResults = "{some text}";

                IsTestResultsEmptyRows = TestResults == null;
                if (TestResults != null)
                {
                    TestResultsAvailable = true;
                    IsTestResultsEmptyRows = TestResults == string.Empty;
                    IsTesting = false;
                    TestPassed = true;
                    TestFailed = false;
                }
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                IsTesting = false;
                TestPassed = false;
                TestFailed = true;
            }
            PasteResponseVisible = false;
        }

        public ObservableCollection<string> QueueEvents { get; set; }
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
                    Options = new List<OptionView>
                    {
                        OptionViewFactory.New(new OptionAutocomplete { Name = "Suggestion 1" }),
                        OptionViewFactory.New(new OptionBool { Name = "Item check 1" }),
                        OptionViewFactory.New(new OptionInt { Name = "Number 1" })
                    };
                }

                OnPropertyChanged(nameof(SelectedQueueSource));
            }
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
                }

                OnPropertyChanged(nameof(SelectedDeadLetterQueueSource));
            }
        }

        private ObservableCollection<INameValue> GetQueueNamesFromSource(IResource selectedQueueSource)
        {
            var queueNames = new ObservableCollection<INameValue>();

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

        public ObservableCollection<INameValue> QueueNames
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

        public ObservableCollection<INameValue> DeadLetterQueues
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

        public ICommand PasteResponseCommand { get; private set; }

        public bool PasteResponseVisible
        {
            get => _pasteResponseVisible;
            set
            {
                _pasteResponseVisible = value;
                OnPropertyChanged(nameof(PasteResponseVisible));
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

        public bool TestResultsAvailable
        {
            get => _testResultsAvailable;
            set
            {
                _testResultsAvailable = value;
                OnPropertyChanged(nameof(TestResultsAvailable));
            }
        }

        public bool IsTestResultsEmptyRows
        {
            get => _isTestResultsEmptyRows;
            set
            {
                _isTestResultsEmptyRows = value;
                OnPropertyChanged(nameof(IsTestResultsEmptyRows));
            }
        }

        public string TestResults
        {
            get => _testResults;
            set
            {
                _testResults = value;
                if (!string.IsNullOrEmpty(_testResults))
                {
                    //Model.Response = _testResults
                }
                OnPropertyChanged(nameof(TestResults));
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

        private void CreateNewQueueEvent()
        {
            QueueEvents.Add("");
        }

        public ICommand DeleteCommand => _deleteCommand ??
                       (_deleteCommand = new DelegateCommand(DeleteQueueEvent));

        private void DeleteQueueEvent()
        {
            QueueEvents.Remove("");
        }

        public ICommand TestCommand { get; private set; }

        public bool IsTesting
        {
            get => _isTesting;
            set
            {
                _isTesting = value;
                OnPropertyChanged(nameof(IsTesting));
            }
        }

        public bool TestFailed
        {
            get => _testFailed;
            set
            {
                _testFailed = value;
                OnPropertyChanged(nameof(TestFailed));
            }
        }

        public bool TestPassed
        {
            get => _testPassed;
            set
            {
                _testPassed = value;
                OnPropertyChanged(nameof(TestPassed));
            }
        }

        public void UpdateHelpDescriptor(string helpText)
        {
            HelpText = helpText;
        }

        protected override void CloseHelp()
        {

        }

        public static bool Save()
        {
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
        public IQueueResourceModel QueueResourceModel
        {
            get => _queueResourceModel;
            set
            {
                _queueResourceModel = value;
                OnPropertyChanged(nameof(QueueResourceModel));
                OnPropertyChanged(nameof(ExecutionHistory));
            }
        }
        public IList<IExecutionHistory> History
        {
            get
            {
                if (QueueResourceModel != null && SelectedQueue != null && _history == null && !SelectedQueue.IsNewItem)
                {
                    _asyncWorker.Start(() =>
                    {
                        IsHistoryTabVisible = false;
                        IsProgressBarVisible = true;
                        _history = QueueResourceModel.CreateHistory(SelectedQueue).ToList();
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
        public IQueueResource SelectedQueue
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
                if (Equals(_selectedQueue, value) || value.IsNewItem)
                {
                    return;
                }
                _selectedQueue = value;
                Item = _ser.Deserialize<IQueueResource>(_ser.SerializeToBuilder(_selectedQueue));
                OnPropertyChanged(nameof(SelectedQueue));
                if (_selectedQueue != null)
                {
                    OnPropertyChanged(nameof(Status));
                    OnPropertyChanged(nameof(WorkflowName));
                    OnPropertyChanged(nameof(QueueName));
                    OnPropertyChanged(nameof(AccountName));
                    OnPropertyChanged(nameof(Password));
                    OnPropertyChanged(nameof(Errors));
                    OnPropertyChanged(nameof(Error));
                    OnPropertyChanged(nameof(SelectedHistory));
                    SelectedHistory = null;
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
        public DebugOutputViewModel DebugOutputViewModel
        {
            get => _debugOutputViewModel;
            set
            {
                _debugOutputViewModel = value;
                OnPropertyChanged(nameof(DebugOutputViewModel));
            }
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
                    SelectedQueue.IsDirty = dirty;
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
        public ObservableCollection<IQueueResource> ExecutionHistory => QueueResourceModel != null ? QueueResourceModel.QueueResources : new ObservableCollection<IQueueResource>();
        public IExecutionHistory SelectedHistory
        {
            get => _selectedHistory;
            set
            {
                if (null == value)
                {
                    //TODO: Not sure what we are doing here....
                    // EventPublisher.Publish(new DebugOutputMessage(new List<IDebugState>()));
                    return;
                }
                _selectedHistory = value;
                //TODO: Needs Discussion
                //DebugOutputViewModel.Clear();
                if (value.DebugOutput != null)
                {
                    foreach (var debugState in value.DebugOutput)
                    {
                        if (debugState != null)
                        {
                            debugState.StateType = StateType.Clear;
                            debugState.SessionID = DebugOutputViewModel.SessionID;
                            //TODO: Needs Discussion
                            //DebugOutputViewModel.Clear();
                            // DebugOutputViewModel.Append(debugState);
                        }
                    }
                }
                OnPropertyChanged(nameof(SelectedHistory));
            }
        }
        public IQueueResource Item { get; set; }
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
        public QueueStatus Status
        {
            get => SelectedQueue?.Status ?? QueueStatus.Enabled;
            set
            {
                if (SelectedQueue != null)
                {
                    if (value == SelectedQueue.Status)
                    {
                        return;
                    }
                    SelectedQueue.Status = value;
                    OnPropertyChanged(nameof(IsDirty));
                    OnPropertyChanged(nameof(Status));
                }
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
        public bool HasErrors => Errors.HasErrors();
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
