﻿/*
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
using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Common.Serializers;
using Dev2.Dialogs;
using Dev2.Studio.Enums;
using Dev2.Studio.Interfaces;
using Microsoft.Practices.Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Warewolf.Studio.Resources.Languages;
using Warewolf.Studio.ViewModels;
using Warewolf.Trigger.Queue;
using Warewolf.Triggers;

namespace Dev2.Triggers.QueueEvents
{
    public class QueueEventsViewModel : TasksItemViewModel, IUpdatesHelp
    {
        ICommand _newCommand;
        ICommand _deleteCommand;

        private IServer _server;
        readonly IExternalProcessExecutor _externalProcessExecutor;

        private ICommand _queueStatsCommand;

        private readonly EnvironmentViewModel _source;
        IResourcePickerDialog _currentResourcePicker;
        string _connectionError;
        bool _hasConnectionError;

        IResourceRepository _resourceRepository;
        TriggerQueueView _selectedQueue;
        readonly Dev2JsonSerializer _ser = new Dev2JsonSerializer();

        private ObservableCollection<TriggerQueueView> _queues;

        public IPopupController PopupController { get; }

        public QueueEventsViewModel(IServer server)
            : this(server, new ExternalProcessExecutor(), null)
        {

        }

        public QueueEventsViewModel(IServer server, IExternalProcessExecutor externalProcessExecutor, IResourcePickerDialog resourcePickerDialog)
        {
            _server = server;
            _resourceRepository = server.ResourceRepository;
            _externalProcessExecutor = externalProcessExecutor;

            AddWorkflowCommand = new DelegateCommand(OpenResourcePicker);

            _source = new EnvironmentViewModel(server, CustomContainer.Get<IShellViewModel>(), true);
            _currentResourcePicker = resourcePickerDialog ?? CreateResourcePickerDialog();
            InitializeHelp();
            PopupController = CustomContainer.Get<IPopupController>();

            PopulateQueues();
            AddDummyTriggerQueueView();
        }

        private void PopulateQueues()
        {
            Queues = new ObservableCollection<TriggerQueueView>();
            var queues = _resourceRepository.FetchTriggerQueues();
            foreach (var queue in queues)
            {
                var triggerQueueView = new TriggerQueueView(_server);
                triggerQueueView.ToModel(queue);
                triggerQueueView.SetItem();
                Queues.Add(triggerQueueView);
            }
        }

        private void AddDummyTriggerQueueView()
        {
            var dummyTriggerQueueView = new DummyTriggerQueueView(_server);
            Queues.Add(dummyTriggerQueueView);
        }

        private void OpenResourcePicker()
        {
            if (_currentResourcePicker.ShowDialog(_server))
            {
                var selectedResource = _currentResourcePicker.SelectedResource;
                SelectedQueue.ResourceId = selectedResource.ResourceId;
                SelectedQueue.WorkflowName = selectedResource.ResourcePath;
                SelectedQueue.GetInputsFromWorkflow();
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

        IResourcePickerDialog CreateResourcePickerDialog()
        {
            var res = new ResourcePickerDialog(enDsfActivityType.All, _source);
            ResourcePickerDialog.CreateAsync(enDsfActivityType.Workflow, _source).ContinueWith(a => _currentResourcePicker = a.Result);
            return res;
        }

        public ICommand QueueStatsCommand => _queueStatsCommand ??
            (_queueStatsCommand = new DelegateCommand(ViewQueueStats));

        private void ViewQueueStats()
        {
            _externalProcessExecutor.OpenInBrowser(new Uri("https://www.rabbitmq.com/blog/tag/statistics/"));
        }

        public ICommand NewCommand => _newCommand ??
                       (_newCommand = new DelegateCommand(CreateNewQueueEvent));

        IEnumerable<TriggerQueueView> RealQueues() => _queues.Where(model => model.GetType() != typeof(DummyTriggerQueueView)).ToObservableCollection();

        private void CreateNewQueueEvent()
        {
            SelectedQueue = null;
            if (IsDirty)
            {
                PopupController?.Show(Core.TriggerQueueSaveEditedTestsMessage, Core.TriggerQueueSaveEditedQueueHeader, MessageBoxButton.OK, MessageBoxImage.Error, null, false, true, false, false, false, false);
                return;
            }

            var queueNumber = GetNewQueueNumber("Queue");

            var queue = new TriggerQueueView(_server)
            {
                TriggerQueueName = "Queue " + (queueNumber == 0 ? 1 : queueNumber)
            };

            AddAndSelectQueue(queue);
            IsDirty = SelectedQueue.IsDirty;
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
            var triggerQueue = RealQueues().FirstOrDefault(a => a.TriggerQueueName.Contains(nameToCheck));
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
            try
            {
                if (!SelectedQueue.IsNewQueue)
                {
                    var triggerQueue = AsTriggerQueueModel();
                    _resourceRepository.DeleteQueue(triggerQueue);
                }
                
                Queues.Remove(SelectedQueue);
                SelectedQueue = null;
                IsDirty = false;
            }
            catch (Exception ex)
            {
                PopupController.Show("Delete failed: " + ex.Message, Core.TriggerQueuesSaveErrorHeader, MessageBoxButton.OK, MessageBoxImage.Error, string.Empty, false, true, false, false, false, false);
                IsDirty = true;
            }
        }

        public ICommand AddWorkflowCommand { get; private set; }

        public void UpdateHelpDescriptor(string helpText)
        {
            HelpText = helpText;
        }

        [ExcludeFromCodeCoverage]
        protected override void CloseHelp()
        {

        }

        public bool Save()
        {
            if (SelectedQueue == null)
            {
                return true;
            }
            try
            {
                if (!PassedValidation())
                {
                    return false;
                }

                var triggerQueue = AsTriggerQueueModel();

                var triggerId = _resourceRepository.SaveQueue(triggerQueue);

                SelectedQueue.TriggerId = triggerId;
                SelectedQueue.IsNewQueue = false;
                SelectedQueue.NewQueue = false;
                SelectedQueue.SetItem();
                IsDirty = SelectedQueue.IsDirty;

                return true;
            }
            catch (Exception ex)
            {
                PopupController.Show("Save failed: " + ex.Message, Core.TriggerQueuesSaveErrorHeader, MessageBoxButton.OK, MessageBoxImage.Error, string.Empty, false, true, false, false, false, false);
                return false;
            }
        }

        private bool PassedValidation()
        {
            if (SelectedQueue.QueueSourceId == Guid.Empty)
            {
                PopupController.Show(Core.TriggerQueuesSaveQueueSourceNotSelected, Core.TriggerQueuesSaveErrorHeader, MessageBoxButton.OK, MessageBoxImage.Error, string.Empty, false, true, false, false, false, false);
                return false;
            }
            if (string.IsNullOrEmpty(SelectedQueue.QueueName))
            {
                PopupController.Show(Core.TriggerQueuesSaveQueueNameEmpty, Core.TriggerQueuesSaveErrorHeader, MessageBoxButton.OK, MessageBoxImage.Error, string.Empty, false, true, false, false, false, false);
                return false;
            }
            if (string.IsNullOrEmpty(SelectedQueue.WorkflowName))
            {
                PopupController.Show(Core.TriggerQueuesSaveWorkflowNotSelected, Core.TriggerQueuesSaveErrorHeader, MessageBoxButton.OK, MessageBoxImage.Error, string.Empty, false, true, false, false, false, false);
                return false;
            }
            if (SelectedQueue.QueueSinkId == Guid.Empty)
            {
                PopupController.Show(Core.TriggerQueuesSaveQueueSinkNotSelected, Core.TriggerQueuesSaveErrorHeader, MessageBoxButton.OK, MessageBoxImage.Error, string.Empty, false, true, false, false, false, false);
                return false;
            }
            if (string.IsNullOrEmpty(SelectedQueue.DeadLetterQueue))
            {
                PopupController.Show(Core.TriggerQueuesSaveOnErrorQueueNameEmpty, Core.TriggerQueuesSaveErrorHeader, MessageBoxButton.OK, MessageBoxImage.Error, string.Empty, false, true, false, false, false, false);
                return false;
            }

            return true;
        }

        private ITriggerQueue AsTriggerQueueModel()
        {
            var triggerQueue = new TriggerQueue
            {
                TriggerId = SelectedQueue.TriggerId,
                Name = SelectedQueue.TriggerQueueName,
                QueueSourceId = SelectedQueue.QueueSourceId,
                QueueName = SelectedQueue.QueueName,
                WorkflowName = SelectedQueue.WorkflowName,
                ResourceId = SelectedQueue.ResourceId,
                Concurrency = SelectedQueue.Concurrency,
                UserName = SelectedQueue.UserName,
                Password = SelectedQueue.Password,
                Options = SelectedQueue.Options.Select(o => o.DataContext).ToArray(),
                QueueSinkId = SelectedQueue.QueueSinkId,
                DeadLetterQueue = SelectedQueue.DeadLetterQueue,
                DeadLetterOptions = SelectedQueue.DeadLetterOptions.Select(o => o.DataContext).ToArray(),
                MapEntireMessage = SelectedQueue.MapEntireMessage,
                Inputs = SelectedQueue.Inputs
            };

            return triggerQueue;
        }

        public TriggerQueueView SelectedQueue
        {
            get => _selectedQueue;
            set
            {
                if (value == null)
                {
                    if (_selectedQueue != null)
                    {
                        _selectedQueue.PropertyChanged -= UpdateParentIsDirty;
                    }
                    _selectedQueue = null;
                    OnPropertyChanged(nameof(SelectedQueue));
                    return;
                }
                if (Equals(_selectedQueue, value) || value.IsNewQueue)
                {
                    return;
                }
                if (_selectedQueue != null)
                {
                    _selectedQueue.PropertyChanged -= UpdateParentIsDirty;
                }
                _selectedQueue = value;
                _selectedQueue.PropertyChanged += UpdateParentIsDirty;
                Item = _ser.Deserialize<TriggerQueueView>(_ser.SerializeToBuilder(_selectedQueue));
                OnPropertyChanged(nameof(SelectedQueue));
                if (_selectedQueue != null)
                {
                    IsDirty = _selectedQueue.IsDirty;
                    OnPropertyChanged(nameof(SelectedQueue.History));
                    OnPropertyChanged(nameof(SelectedQueue.Inputs));
                    OnPropertyChanged(nameof(SelectedQueue.VerifyResults));
                }
            }
        }

        private void UpdateParentIsDirty(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IsDirty))
            {
                IsDirty = _queues.Any(q=>q.IsDirty);
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

        public TriggerQueueView Item { get; set; }

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
