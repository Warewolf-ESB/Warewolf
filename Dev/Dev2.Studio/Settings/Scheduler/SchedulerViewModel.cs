/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Caliburn.Micro;
using CubicOrange.Windows.Forms.ActiveDirectory;
using Dev2.Activities.Designers2.Core;
using Dev2.Activities.Designers2.Core.Help;
using Dev2.Common;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data.TO;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Common.Interfaces.Scheduler.Interfaces;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Common.Interfaces.Threading;
using Dev2.Communication;
using Dev2.Data.TO;
using Dev2.Diagnostics;
using Dev2.Dialogs;
using Dev2.Interfaces;
using Dev2.Providers.Events;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Services.Events;
using Dev2.Studio.Controller;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.Models;
using Dev2.Studio.ViewModels.Diagnostics;
using Dev2.Studio.ViewModels.WorkSurface;
using Dev2.Threading;
using Warewolf.Studio.AntiCorruptionLayer;
using Warewolf.Studio.Resources.Languages;
using Warewolf.Studio.ViewModels;
// ReSharper disable NonLocalizedString

namespace Dev2.Settings.Scheduler
{
    public class SchedulerViewModel : BaseWorkSurfaceViewModel, IHelpSource, IStudioTab
    {
        private ICommand _saveCommand;
        private ICommand _newCommand;
        private ICommand _deleteCommand;
        private ICommand _editTriggerCommand;
        private ICommand _addWorkflowCommand;
        readonly Dev2JsonSerializer _ser = new Dev2JsonSerializer();
        private IScheduledResource _selectedTask;
        private readonly IPopupController _popupController;
        private readonly IAsyncWorker _asyncWorker;

        private IResourceHistory _selectedHistory;
        private IList<IResourceHistory> _history;
        private TabItem _activeItem;
        private bool _isProgressBarVisible;
        private bool _isHistoryTabVisible;
        private string _helpText;
        private bool _isLoading;
        private string _connectionError;
        private bool _hasConnectionError;
        private IEnvironmentModel _currentEnvironment;
        private IScheduledResourceModel _scheduledResourceModel;
        private Func<IServer, IEnvironmentModel> _toEnvironmentModel;
        private bool _errorShown;
        private DebugOutputViewModel _debugOutputViewModel;
        private string _displayName;

        // ReSharper disable once MemberCanBeProtected.Global
        public SchedulerViewModel()
            : this(EventPublishers.Aggregator, new DirectoryObjectPickerDialog(), new PopupController(), new AsyncWorker(), CustomContainer.Get<IShellViewModel>().ActiveServer, null)
        {
        }

        public SchedulerViewModel(Func<IServer, IEnvironmentModel> toEnvironmentModel)
            : this(EventPublishers.Aggregator, new DirectoryObjectPickerDialog(), new PopupController(), new AsyncWorker(), CustomContainer.Get<IShellViewModel>().ActiveServer, toEnvironmentModel)
        {
        }

        public SchedulerViewModel(IEventAggregator eventPublisher, DirectoryObjectPickerDialog directoryObjectPicker, IPopupController popupController, IAsyncWorker asyncWorker, IServer server, Func<IServer, IEnvironmentModel> toEnvironmentModel, Task<IResourcePickerDialog> getResourcePicker = null)
            : base(eventPublisher)
        {
            SchedulerTaskManager = new SchedulerTaskManager(this, getResourcePicker);
            VerifyArgument.IsNotNull("directoryObjectPicker", directoryObjectPicker);
            DirectoryObjectPickerDialog directoryObjectPicker1 = directoryObjectPicker;

            VerifyArgument.IsNotNull("popupController", popupController);
            _popupController = popupController;

            VerifyArgument.IsNotNull("asyncWorker", asyncWorker);
            _asyncWorker = asyncWorker;
            _toEnvironmentModel = toEnvironmentModel ?? (a => a.ToEnvironmentModel());
            Errors = new ErrorResultTO();
            IsLoading = false;
            directoryObjectPicker1.AllowedObjectTypes = ObjectTypes.Users;
            directoryObjectPicker1.DefaultObjectTypes = ObjectTypes.Users;
            directoryObjectPicker1.AllowedLocations = Locations.All;
            directoryObjectPicker1.DefaultLocations = Locations.JoinedDomain;
            directoryObjectPicker1.MultiSelect = false;
            directoryObjectPicker1.TargetComputer = string.Empty;
            directoryObjectPicker1.ShowAdvancedView = false;

            InitializeHelp();
            DebugOutputViewModel = new DebugOutputViewModel(new EventPublisher(), EnvironmentRepository.Instance, new DebugOutputFilterStrategy());

            Server = server;
            SetupServer(server);
            SetDisplayName(false);
        }

        public override bool HasVariables => false;
        public override bool HasDebugOutput => true;

        public override string DisplayName
        {
            get
            {
                return _displayName;                
            }
            set
            {
                _displayName = value;
                OnPropertyChanged("DisplayName");
            }
        }

        public Func<IServer, IEnvironmentModel> ToEnvironmentModel
        {
            private get
            {
                return _toEnvironmentModel ?? (a => a.ToEnvironmentModel());
            }
            set
            {
                _toEnvironmentModel = value;
            }
        }

        public bool HasConnectionError
        {
            get
            {
                return _hasConnectionError;
            }
            set
            {
                _hasConnectionError = value;
                NotifyOfPropertyChange(() => HasConnectionError);
            }
        }

        public void SetConnectionError()
        {
            ConnectionError = Core.SchedulerConnectionError;
            HasConnectionError = true;
        }

        public void CreateNewTask()
        {
            SchedulerTaskManager.CreateNewTask();
        }

        public string ConnectionError
        {
            get
            {
                return _connectionError;
            }
            set
            {
                _connectionError = value;
                NotifyOfPropertyChange(() => ConnectionError);
            }
        }

        public bool IsLoading
        {
            get
            {
                return _isLoading;
            }
            private set
            {
                _isLoading = value;
                NotifyOfPropertyChange(() => IsLoading);
            }
        }

        public ActivityDesignerToggle HelpToggle { get; private set; }

        public IScheduleTrigger Trigger
        {
            get
            {
                return SelectedTask.Trigger;
            }
            set
            {
                if (SelectedTask != null)
                {
                    SelectedTask.Trigger = value;
                    NotifyOfPropertyChange(() => IsDirty);
                    NotifyOfPropertyChange(() => Trigger);
                }
            }
        }

        public SchedulerStatus Status
        {
            get
            {
                return SelectedTask?.Status ?? SchedulerStatus.Enabled;
            }
            set
            {
                if (SelectedTask != null)
                {
                    if (value == SelectedTask.Status)
                    {
                        return;
                    }
                    SelectedTask.Status = value;
                    NotifyOfPropertyChange(() => IsDirty);
                    NotifyOfPropertyChange(() => Status);
                }
            }
        }

        public string WorkflowName
        {
            get
            {
                return SelectedTask != null ? SelectedTask.WorkflowName : string.Empty;
            }
            set
            {
                if (SelectedTask != null && !SelectedTask.IsNewItem)
                {
                    if (string.IsNullOrEmpty(value))
                    {
                        ShowError(Core.SchedulerBlankWorkflowNameErrorMessage);
                    }
                    else
                    {
                        ClearError(Core.SchedulerBlankWorkflowNameErrorMessage);
                    }

                    if (value == SelectedTask.WorkflowName)
                    {
                        return;
                    }

                    SelectedTask.WorkflowName = value;
                    NotifyOfPropertyChange(() => IsDirty);
                    NotifyOfPropertyChange(() => WorkflowName);
                }
            }
        }

        public string Name
        {
            get
            {
                return SelectedTask != null ? SelectedTask.Name : string.Empty;
            }
            set
            {
                if (SelectedTask == null || SelectedTask.Name == value)
                {
                    return;
                }
                if (string.IsNullOrEmpty(value))
                {
                    ShowError(Core.SchedulerBlankNameErrorMessage);
                }
                else
                {
                    ClearError(Core.SchedulerBlankNameErrorMessage);
                }

                if (TaskList.Any(c => c.Name == value))
                {
                    foreach (var resource in TaskList.Where(a => a.Name == value))
                    {
                        resource.Errors.AddError(Core.SchedulerDuplicateNameErrorMessage);
                    }
                    ShowError(Core.SchedulerDuplicateNameErrorMessage);
                }
                else
                {
                    ClearError(Core.SchedulerDuplicateNameErrorMessage);
                }
                SelectedTask.Name = value;
                NotifyOfPropertyChange(() => IsDirty);
                NotifyOfPropertyChange(() => Name);
            }
        }

        public bool RunAsapIfScheduleMissed
        {
            get
            {
                return SelectedTask != null && SelectedTask.RunAsapIfScheduleMissed;
            }
            set
            {
                if (SelectedTask != null)
                {
                    if (value == SelectedTask.RunAsapIfScheduleMissed)
                    {
                        return;
                    }
                    SelectedTask.RunAsapIfScheduleMissed = value;
                    NotifyOfPropertyChange(() => IsDirty);
                    NotifyOfPropertyChange(() => RunAsapIfScheduleMissed);
                }
            }
        }

        public string NumberOfRecordsToKeep
        {
            get
            {
                return SelectedTask?.NumberOfHistoryToKeep.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
            }
            set
            {
                if (SelectedTask != null)
                {
                    if (value == SelectedTask.NumberOfHistoryToKeep.ToString(CultureInfo.InvariantCulture))
                    {
                        return;
                    }
                    if (string.IsNullOrEmpty(value))
                    {
                        SelectedTask.NumberOfHistoryToKeep = 0;
                        NotifyOfPropertyChange(() => IsDirty);
                    }
                    else
                    {
                        int val;
                        if (value.IsWholeNumber(out val))
                        {
                            SelectedTask.NumberOfHistoryToKeep = val;
                            NotifyOfPropertyChange(() => IsDirty);
                        }
                    }
                    NotifyOfPropertyChange(() => NumberOfRecordsToKeep);
                }
            }
        }

        public IList<IResourceHistory> History
        {
            get
            {
                if (ScheduledResourceModel != null && SelectedTask != null && _history == null && !SelectedTask.IsNewItem)
                {
                    _asyncWorker.Start(() =>
                    {
                        IsHistoryTabVisible = false;
                        IsProgressBarVisible = true;
                        _history = ScheduledResourceModel.CreateHistory(SelectedTask).ToList();
                    }
                   , () =>
                   {
                       IsHistoryTabVisible = true;
                       IsProgressBarVisible = false;
                       NotifyOfPropertyChange(() => History);
                   });
                }
                var history = _history;
                _history = null;
                return history ?? new List<IResourceHistory>();
            }
        }

        public IResourceHistory SelectedHistory
        {
            get
            {
                return _selectedHistory;
            }
            set
            {
                if (null == value)
                {
                    EventPublisher.Publish(new DebugOutputMessage(new List<IDebugState>()));
                    return;
                }
                _selectedHistory = value;
                DebugOutputViewModel.Clear();
                if (value.DebugOutput != null)
                {
                    foreach (var debugState in value.DebugOutput)
                    {
                        if (debugState != null)
                        {
                            debugState.StateType = StateType.Clear;
                            debugState.SessionID = DebugOutputViewModel.SessionID;
                            DebugOutputViewModel.Append(debugState);
                        }
                    }
                }
                NotifyOfPropertyChange(() => SelectedHistory);
            }
        }

        public ObservableCollection<IScheduledResource> TaskList => ScheduledResourceModel != null ? ScheduledResourceModel.ScheduledResources : new ObservableCollection<IScheduledResource>();

        public string TriggerText => SelectedTask?.Trigger.Trigger.Instance.ToString() ?? string.Empty;

        public IScheduledResource SelectedTask
        {
            get
            {
                return _selectedTask;
            }
            set
            {
                if (value == null)
                {
                    _selectedTask = null;
                    NotifyOfPropertyChange(() => SelectedTask);
                    return;
                }
                if (Equals(_selectedTask, value) || value.IsNewItem)
                {
                    return;
                }
                _selectedTask = value;
                Item = _ser.Deserialize<IScheduledResource>(_ser.SerializeToBuilder(_selectedTask));
                NotifyOfPropertyChange(() => SelectedTask);
                if (_selectedTask != null)
                {
                    NotifyOfPropertyChange(() => Trigger);
                    NotifyOfPropertyChange(() => Status);
                    NotifyOfPropertyChange(() => WorkflowName);
                    NotifyOfPropertyChange(() => Name);
                    NotifyOfPropertyChange(() => RunAsapIfScheduleMissed);
                    NotifyOfPropertyChange(() => NumberOfRecordsToKeep);
                    NotifyOfPropertyChange(() => TriggerText);
                    NotifyOfPropertyChange(() => AccountName);
                    NotifyOfPropertyChange(() => Password);
                    NotifyOfPropertyChange(() => Errors);
                    NotifyOfPropertyChange(() => Error);
                    NotifyOfPropertyChange(() => SelectedHistory);
                    SelectedHistory = null;
                    NotifyOfPropertyChange(() => History);
                    
                }
            }
        }
        public IScheduledResource Item { get; set; }

        public IScheduledResourceModel ScheduledResourceModel
        {
            get
            {
                return _scheduledResourceModel;
            }
            set
            {
                _scheduledResourceModel = value;
                NotifyOfPropertyChange(() => ScheduledResourceModel);
                NotifyOfPropertyChange(() => TaskList);
            }
        }

        public string AccountName
        {
            get
            {
                return SelectedTask != null ? SelectedTask.UserName : string.Empty;
            }
            set
            {
                if (SelectedTask != null)
                {
                    if (Equals(SelectedTask.UserName, value))
                    {
                        return;
                    }
                    ClearError(Core.SchedulerLoginErrorMessage);
                    SelectedTask.UserName = value;
                    NotifyOfPropertyChange(()=>IsDirty);
                    NotifyOfPropertyChange(() => AccountName);
                }
            }
        }

        public string Password
        {
            get
            {
                return SelectedTask != null ? SelectedTask.Password : string.Empty;
            }
            set
            {
                if (SelectedTask != null)
                {
                    if (Equals(SelectedTask.Password, value))
                    {
                        return;
                    }
                    ClearError(Core.SchedulerLoginErrorMessage);
                    SelectedTask.Password = value;
                    NotifyOfPropertyChange(() => IsDirty);
                    NotifyOfPropertyChange(() => Password);
                }
            }
        }

        public IEnvironmentModel CurrentEnvironment
        {
            get
            {
                return _currentEnvironment;
            }
            set
            {
                _currentEnvironment = value;
                NotifyOfPropertyChange(() => CurrentEnvironment);
            }
        }

        public bool HasErrors => Errors.HasErrors();

        public IErrorResultTO Errors
        {
            get { return _selectedTask != null ? _selectedTask.Errors : new ErrorResultTO(); }
            private set
            {
                if (_selectedTask != null)
                    _selectedTask.Errors = value;
                NotifyOfPropertyChange(() => Errors);
            }
        }

        public bool IsHistoryTabVisible
        {
            get
            {
                return _isHistoryTabVisible;
            }
            private set
            {
                _isHistoryTabVisible = value;
                NotifyOfPropertyChange(() => IsHistoryTabVisible);
            }
        }

        public bool IsProgressBarVisible
        {
            get
            {
                return _isProgressBarVisible;
            }
            set
            {
                _isProgressBarVisible = value;
                NotifyOfPropertyChange(() => IsProgressBarVisible);
            }
        }

        public string Error => HasErrors ? Errors.FetchErrors()[0] : string.Empty;

        public TabItem ActiveItem
        {
            private get
            {
                return _activeItem;
            }
            set
            {
                if (Equals(value, _activeItem))
                {
                    return;
                }
                _activeItem = value;
                if (IsHistoryTab)
                {
                    NotifyOfPropertyChange(() => History);
                }
            }
        }

        private bool IsHistoryTab
        {
            get
            {
                if (ActiveItem != null)
                {
                    return (string)ActiveItem.Header == "History";
                }
                return false;
            }
        }

        public ICommand SaveCommand
        {
            get
            {
                return _saveCommand ??
                       (_saveCommand = new DelegateCommand(param => SchedulerTaskManager.SaveTasks()));
            }
        }

        public ICommand NewCommand
        {
            get
            {
                return _newCommand ??
                       (_newCommand = new DelegateCommand(param => CreateNewTask()));
            }
        }

        public ICommand DeleteCommand
        {
            get
            {
                return _deleteCommand ??
                       (_deleteCommand = new DelegateCommand(param =>
                       {
                           var taskToBeDeleted = param as IScheduledResource;
                           if (taskToBeDeleted == null) return;
                           SelectedTask = taskToBeDeleted;
                           SchedulerTaskManager.DeleteTask();
                       }));
            }
        }

        public ICommand EditTriggerCommand
        {
            get
            {
                return _editTriggerCommand ??
                       (_editTriggerCommand = new DelegateCommand(param => SchedulerTaskManager.EditTrigger()));
            }
        }

        public ICommand AddWorkflowCommand => _addWorkflowCommand ??
                                              (_addWorkflowCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(SchedulerTaskManager.AddWorkflow, SchedulerTaskManager.CanSelectWorkflow));

        private void InitializeHelp()
        {
            HelpToggle = CreateHelpToggle();
            HelpText = Warewolf.Studio.Resources.Languages.HelpText.SchedulerSettingsHelpTextSettingsView;
        }

        private static ActivityDesignerToggle CreateHelpToggle()
        {
            var toggle = ActivityDesignerToggle.Create("ServiceHelp", "Close Help", "ServiceHelp", "Open Help", "HelpToggle");

            return toggle;
        }

        void SetupServer(IServer tmpEnv)
        {
            CurrentEnvironment = ToEnvironmentModel(tmpEnv);

            if (CurrentEnvironment?.AuthorizationService != null && CurrentEnvironment.IsConnected && tmpEnv.Permissions.Any(a => a.Administrator))
            {
                ClearConnectionError();
                var environment = CurrentEnvironment ?? EnvironmentRepository.Instance.ActiveEnvironment;

                IServer server = new Server(environment);
                if (server.Permissions == null)
                {
                    server.Permissions = new List<IWindowsGroupPermission>();
                    server.Permissions.AddRange(environment.AuthorizationService.SecurityService.Permissions);
                }
                SchedulerTaskManager.Source = new EnvironmentViewModel(server, CustomContainer.Get<IShellViewModel>(), true);

                ScheduledResourceModel = new ClientScheduledResourceModel(CurrentEnvironment, CreateNewTask);
                IsLoading = true;
                try
                {
                    var cmd = AddWorkflowCommand as Microsoft.Practices.Prism.Commands.DelegateCommand;
                    cmd?.RaiseCanExecuteChanged();

                    foreach (var scheduledResource in ScheduledResourceModel.ScheduledResources.Where(a => !a.IsNewItem))
                    {
                        scheduledResource.NextRunDate = scheduledResource.Trigger.Trigger.StartBoundary;
                        scheduledResource.OldName = scheduledResource.Name;
                    }
                    NotifyOfPropertyChange(() => TaskList);
                    if (TaskList.Count > 0)
                    {
                        SelectedTask = TaskList[0];
                    }
                    IsLoading = false;
                }
                catch (Exception ex)
                {
                    if (!_errorShown)
                    {
                        Dev2Logger.Error(ex);
                        _errorShown = true;
                    }
                }
            }
            else
            {
                ClearConnectionError();
                ClearViewModel();
            }
        }

        public void ClearConnectionError()
        {
            ConnectionError = string.Empty;
            HasConnectionError = false;
        }

        private void ClearViewModel()
        {
            Name = string.Empty;
            WorkflowName = string.Empty;
            Status = SchedulerStatus.Enabled;
            AccountName = string.Empty;
            Password = string.Empty;
            NumberOfRecordsToKeep = string.Empty;
            Trigger = null;
            ScheduledResourceModel?.ScheduledResources.Clear();
            NotifyOfPropertyChange(() => TaskList);
            NotifyOfPropertyChange(() => History);
            Errors.ClearErrors();
        }

        public void ClearError(string description)
        {
            Errors.RemoveError(description);
            NotifyOfPropertyChange(() => Error);
            NotifyOfPropertyChange(() => HasErrors);
        }

        public void ShowError(string description)
        {
            if (!string.IsNullOrEmpty(description))
            {
                Errors.AddError(description, true);
                NotifyOfPropertyChange(() => Error);
                NotifyOfPropertyChange(() => HasErrors);
            }
        }

        public bool IsDirty
        {
            get
            {
                try
                {
                    
                    if (SelectedTask == null)
                    {
                        SetDisplayName(false);
                        return false;
                    }
                    var dirty = !SelectedTask.Equals(Item);
                    SelectedTask.IsDirty = dirty;
                    SetDisplayName(dirty);
                    return dirty;
                }
                catch (Exception ex)
                {
                    if (!_errorShown)
                    {
                        _popupController.ShowCorruptTaskResult(ex.Message);
                        Dev2Logger.Error(ex);
                        _errorShown = true;
                    }
                }
                return false;
            }
        }

        private void SetDisplayName(bool dirty)
        {
            string baseName = "Scheduler";
            if (Server != null)
            {
                baseName = baseName + " - " + Server.ResourceName;
            }
            if (dirty)
            {
                if (!baseName.EndsWith(" *"))
                {
                    DisplayName = baseName + " *";
                }
            }
            else
            {
                DisplayName = baseName;
            }
        }

        public void CloseView()
        {
        }

        public virtual bool DoDeactivate(bool showMessage)
        {
            if (showMessage)
            {
                if (SelectedTask != null && SelectedTask.IsDirty)
                {
                    MessageBoxResult showSchedulerCloseConfirmation = _popupController.ShowSchedulerCloseConfirmation();
                    switch (showSchedulerCloseConfirmation)
                    {
                        case MessageBoxResult.Cancel:
                        case MessageBoxResult.None:
                            return false;
                        case MessageBoxResult.No:
                            return true;
                    }
                    return SchedulerTaskManager.SaveTasks();
                }
            }
            if (SelectedTask != null && !showMessage)
                return SchedulerTaskManager.SaveTasks();
            return true;
        }

        protected internal virtual void ShowSaveErrorDialog(string error)
        {
            _popupController.ShowSaveErrorDialog(error);
        }

        public string HelpText
        {
            get
            {
                return _helpText;
            }
            set
            {
                if (value == _helpText)
                    return;
                _helpText = value;
                NotifyOfPropertyChange(() => HelpText);
            }
        }

        public IServer Server { private get; set; }

        public string ResourceType => "Scheduler";
        internal SchedulerTaskManager SchedulerTaskManager { get; set; }
        public IPopupController PopupController
        {
            get
            {
                return _popupController;
            }
        }

        public DebugOutputViewModel DebugOutputViewModel
        {
            get { return _debugOutputViewModel; }
            set
            {
                _debugOutputViewModel = value;
                NotifyOfPropertyChange(() => DebugOutputViewModel);
            }
        }

        public void UpdateScheduleWithResourceDetails(string resourcePath, Guid id, string resourceName)
        {
            SchedulerTaskManager.UpdateScheduleWithResourceDetails(resourcePath, id, resourceName);
        }
    }

    public static class SchedulerServerExtensions
    {
        public static IEnvironmentModel ToEnvironmentModel(this IServer server)
        {
            var resource = server as Server;
            if (resource != null)
            {
                return new EnvironmentModel(server.EnvironmentID, resource.EnvironmentConnection);
            }
            throw new Exception();
        }
    }


}

