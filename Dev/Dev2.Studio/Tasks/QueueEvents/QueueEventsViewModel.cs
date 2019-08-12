/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.Resources;
using Dev2.Studio.Interfaces;
using Microsoft.Practices.Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Warewolf.Options;
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
        private Guid _executionId;
        private IResourceRepository _resourceRepository;
        IExternalProcessExecutor _externalProcessExecutor;
        private ObservableCollection<INameValue> _queueNames;
        private ObservableCollection<INameValue> _deadLetterQueues;
        private ICollection<IServiceInput> _inputs;
        private bool _pasteResponseVisible;
        private string _pasteResponse;
        private ICommand _queueStatsCommand;
        private IEnumerable<dynamic> _executionEvents;
        private bool _isTesting;
        private bool _testFailed;
        private bool _testPassed;
        private bool _testResultsAvailable;
        private bool _isTestResultsEmptyRows;
        private string _testResults;
        private List<OptionView> _options;

        public QueueEventsViewModel(IServer server)
            : this(server, new ExternalProcessExecutor())
        {

        }

        public QueueEventsViewModel(IServer server, IExternalProcessExecutor externalProcessExecutor)
        {
            _server = server;
            _resourceRepository = server.ResourceRepository;
            _externalProcessExecutor = externalProcessExecutor;
            Inputs = new ObservableCollection<IServiceInput>();
            PasteResponseCommand = new DelegateCommand(ExecutePaste);
            TestCommand = new DelegateCommand(ExecuteTest);
            IsTesting = false;
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

        public string SelectedQueueEvent
        {
            get => _selectedQueueEvent;
            set
            {
                _selectedQueueEvent = value;
                OnPropertyChanged(nameof(SelectedQueueEvent));
            }
        }
      //  public IEnumerable<dynamic> ExecutionEvents => _executionEvents.ExecutionEvents(_server, _executionId);
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

            var list = _resourceRepository.FindAutocompleteOptions(_server, SelectedQueueSource);

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
    }
}
