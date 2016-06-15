/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
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
using System.Windows.Forms;
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
using Dev2.DataList.Contract;
using Dev2.Dialogs;
using Dev2.Interfaces;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Scheduler;
using Dev2.Services.Events;
using Dev2.Services.Security;
using Dev2.Studio.Controller;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Enums;
using Dev2.Studio.ViewModels.WorkSurface;
using Dev2.TaskScheduler.Wrappers;
using Dev2.Threading;
using Microsoft.Win32.TaskScheduler;
using Warewolf.Studio.AntiCorruptionLayer;
using Warewolf.Studio.Resources.Languages;
using Warewolf.Studio.ViewModels;

namespace Dev2.Settings.Scheduler
{
    public class SchedulerViewModel : BaseWorkSurfaceViewModel, IHelpSource, IStudioTab
    {
        #region Fields

        const string DuplicateNameErrorMessage = "There is already a task with the same name";
        const string LoginErrorMessage = "Error while saving: Logon failure: unknown user name or bad password";
        const string NotConnectedErrorMessage = "Error while saving: Server unreachable.";
        const string BlankWorkflowNameErrorMessage = "Please select a workflow to schedule";
        const string BlankNameErrorMessage = "The name can not be blank";
        const string SaveErrorPrefix = "Error while saving:";
        const string NewTaskName = "New Task";
        IResourcePickerDialog _currentResourcePicker;
        int _newTaskCounter = 1;
        ICommand _saveCommand;
        ICommand _newCommand;
        ICommand _closeCommand;
        ICommand _deleteCommand;
        ICommand _editTriggerCommand;
        ICommand _addWorkflowCommand;

        IClientSchedulerFactory _schedulerFactory;
        IScheduledResource _selectedTask;
        TriggerEditDialog _triggerEditDialog;
        readonly IPopupController _popupController;
        readonly IAsyncWorker _asyncWorker;

        IResourceHistory _selectedHistory;
        IList<IResourceHistory> _history;
        TabItem _activeItem;
        bool _isProgressBarVisible;
        bool _isHistoryTabVisible;
        string _helpText;
        bool _isLoading;
        string _connectionError;
        bool _hasConnectionError;
        IEnvironmentModel _currentEnvironment;
        IConnectControlViewModel _connectControlViewModel;
        Task<IResourcePickerDialog> _task;
        IScheduledResourceModel _scheduledResourceModel;
        private Func<IServer, IEnvironmentModel> _toEnvironmentModel;
        private bool _errorShown;

        #endregion

        #region Ctor

        public SchedulerViewModel()
            : this(EventPublishers.Aggregator, new DirectoryObjectPickerDialog(), new PopupController(), new AsyncWorker(), CustomContainer.Get<IShellViewModel>().ActiveServer, null)
        {
        }

        public SchedulerViewModel(Func<IServer, IEnvironmentModel> toEnvironmentModel)
            : this(EventPublishers.Aggregator, new DirectoryObjectPickerDialog(), new PopupController(), new AsyncWorker(), CustomContainer.Get<IShellViewModel>().ActiveServer, toEnvironmentModel)
        {
        }
        public SchedulerViewModel(IEventAggregator eventPublisher, DirectoryObjectPickerDialog directoryObjectPicker, IPopupController popupController, IAsyncWorker asyncWorker, IServer server, Func<IServer, IEnvironmentModel> toEnvironmentModel)
            : base(eventPublisher)
        {
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

            var taskServiceConvertorFactory = new TaskServiceConvertorFactory();
            SchedulerFactory = new ClientSchedulerFactory(new Dev2TaskService(taskServiceConvertorFactory), taskServiceConvertorFactory);
            IShellViewModel vm = CustomContainer.Get<IShellViewModel>();
            CreateEnvironmentFromServer(vm.ActiveServer, vm);
            Server = server;
            server.NetworkStateChanged += server_NetworkStateChanged;
            SetupServer(server);
        }

        #region Overrides of Screen

        public override string DisplayName
        {
            get
            {
                if (Server != null)
                {
                    return "Scheduler - " + Server.ResourceName;
                }
                return "Scheduler";
            }
            set
            {

            }
        }

        #endregion
        public Func<IServer, IEnvironmentModel> ToEnvironmentModel
        {
            get
            {
                return _toEnvironmentModel ?? (a => a.ToEnvironmentModel());
            }
            set
            {
                _toEnvironmentModel = value;
            }
        }

        void server_NetworkStateChanged(INetworkStateChangedEventArgs args, IServer server)
        {

        }

        void CreateEnvironmentFromServer(IServer server, IShellViewModel shellViewModel)
        {
            if (server != null && server.UpdateRepository != null)
            {
                server.UpdateRepository.ItemSaved += Refresh;
            }
            // ReSharper disable once ObjectCreationAsStatement
            new EnvironmentViewModel(server, shellViewModel);
        }

        #endregion

        #region Properties

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

        public IResourcePickerDialog CurrentResourcePickerDialog
        {
            get
            {
                return _currentResourcePicker;
            }
            set
            {
                _currentResourcePicker = value;
                NotifyOfPropertyChange(() => CurrentResourcePickerDialog);
            }
        }

        public bool IsLoading
        {
            get
            {
                return _isLoading;
            }
            set
            {
                _isLoading = value;
                NotifyOfPropertyChange(() => IsLoading);
            }
        }

        public IConnectControlViewModel ConnectControlViewModel
        {
            get
            {
                return _connectControlViewModel;
            }
            set
            {
                if (Equals(value, _connectControlViewModel))
                {
                    return;
                }
                _connectControlViewModel = value;
                NotifyOfPropertyChange(() => ConnectControlViewModel);
            }
        }

        public ActivityDesignerToggle HelpToggle { get; private set; }

        public IClientSchedulerFactory SchedulerFactory
        {
            get { return _schedulerFactory; }
            set { _schedulerFactory = value; }
        }

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
                    SelectedTask.IsDirty = true;
                    NotifyOfPropertyChange(() => Trigger);
                }
            }
        }

        public SchedulerStatus Status
        {
            get
            {
                return SelectedTask != null ? SelectedTask.Status : SchedulerStatus.Enabled;
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
                    SelectedTask.IsDirty = true;
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
                        ShowError(BlankWorkflowNameErrorMessage);
                    }
                    else
                    {
                        ClearError(BlankWorkflowNameErrorMessage);
                    }

                    if (value == SelectedTask.WorkflowName)
                    {
                        return;
                    }

                    SelectedTask.WorkflowName = value;
                    SelectedTask.IsDirty = true;
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
                    ShowError(BlankNameErrorMessage);
                }
                else
                {
                    ClearError(BlankNameErrorMessage);
                }

                if (TaskList.Any(c => c.Name == value))
                {
                    foreach (var resource in TaskList.Where(a => a.Name == value))
                    {
                        resource.Errors.AddError(DuplicateNameErrorMessage);
                    }
                    ShowError(DuplicateNameErrorMessage);
                }
                else
                {
                    ClearError(DuplicateNameErrorMessage);
                }
                SelectedTask.Name = value;
                SelectedTask.IsDirty = true;
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
                    SelectedTask.IsDirty = true;
                    NotifyOfPropertyChange(() => RunAsapIfScheduleMissed);
                }
            }
        }

        public string NumberOfRecordsToKeep
        {
            get
            {
                return SelectedTask != null ? SelectedTask.NumberOfHistoryToKeep.ToString(CultureInfo.InvariantCulture) : string.Empty;
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
                        SelectedTask.IsDirty = true;
                    }
                    else
                    {
                        int val;
                        if (value.IsWholeNumber(out val))
                        {
                            SelectedTask.NumberOfHistoryToKeep = val;
                            SelectedTask.IsDirty = true;
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
                    _asyncWorker.Start(
                   () =>
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
                if (Equals(value, _selectedHistory))
                {
                    return;
                }
                _selectedHistory = value;
                EventPublisher.Publish(new DebugOutputMessage(value.DebugOutput));
                NotifyOfPropertyChange(() => SelectedHistory);
            }
        }

        public ObservableCollection<IScheduledResource> TaskList
        {
            get
            {

                if (ScheduledResourceModel != null)
                {
                    return ScheduledResourceModel.ScheduledResources;
                }

                return new ObservableCollection<IScheduledResource>();
            }
        }

        public string TriggerText
        {
            get
            {
                if (SelectedTask != null)
                {
                    return SelectedTask.Trigger.Trigger.Instance.ToString();
                }
                return string.Empty;
            }
        }

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

        public TriggerEditDialog TriggerEditDialog
        {
            get
            {
                return _triggerEditDialog;
            }
            set
            {
                if (Equals(_triggerEditDialog, value))
                {
                    return;
                }
                _triggerEditDialog = value;
                NotifyOfPropertyChange(() => TriggerEditDialog);
            }
        }

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
                    ClearError(LoginErrorMessage);
                    SelectedTask.UserName = value;
                    SelectedTask.IsDirty = true;
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
                    ClearError(LoginErrorMessage);
                    SelectedTask.Password = value;
                    SelectedTask.IsDirty = true;
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

        public bool HasErrors
        {
            get { return Errors.HasErrors(); }
        }

        public IErrorResultTO Errors
        {
            get { return _selectedTask != null ? _selectedTask.Errors : new ErrorResultTO(); }
            set
            {
                if (_selectedTask != null) _selectedTask.Errors = value;
                NotifyOfPropertyChange(() => Errors);
            }
        }

        public bool IsHistoryTabVisible
        {
            get
            {
                return _isHistoryTabVisible;
            }
            set
            {
                if (value.Equals(_isHistoryTabVisible))
                {
                    return;
                }
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
                if (value.Equals(_isProgressBarVisible))
                {
                    return;
                }
                _isProgressBarVisible = value;
                NotifyOfPropertyChange(() => IsProgressBarVisible);
            }
        }

        public string Error
        {
            get
            {
                if (HasErrors)
                {
                    return Errors.FetchErrors()[0];
                }
                return string.Empty;
            }
        }

        public TabItem ActiveItem
        {
            get
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

        #endregion

        #region Commands

        public ICommand SaveCommand
        {
            get
            {
                return _saveCommand ??
                       (_saveCommand = new DelegateCommand(param => SaveTasks()));
            }
        }

        public ICommand CloseHelpCommand
        {
            get
            {
                return _closeCommand ??
                    (_closeCommand = new DelegateCommand(param => CloseHelp()));
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
                           if (taskToBeDeleted != null)
                           {
                               SelectedTask = taskToBeDeleted;
                               DeleteTask();
                           }

                       }));
            }
        }

        public ICommand EditTriggerCommand
        {
            get
            {
                return _editTriggerCommand ??
                       (_editTriggerCommand = new DelegateCommand(param => EditTrigger()));
            }
        }

        public ICommand AddWorkflowCommand
        {
            get
            {
                return _addWorkflowCommand ??
                       (_addWorkflowCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(AddWorkflow, CanSelectWorkflow));
            }
        }

        bool CanSelectWorkflow()
        {
            return CurrentResourcePickerDialog != null;
        }

        #endregion

        #region Private Methods

        void CloseHelp()
        {
            HelpToggle.IsChecked = false;
        }

        void InitializeHelp()
        {
            HelpToggle = CreateHelpToggle();
            HelpText = Core.SchedulerSettingsHelpTextSettingsView;
        }

        ActivityDesignerToggle CreateHelpToggle()
        {
            var toggle = ActivityDesignerToggle.Create("ServiceHelp", "Close Help", "ServiceHelp", "Open Help", "HelpToggle");

            return toggle;
        }

        bool SaveTasks()
        {
            if (CurrentEnvironment.IsConnected)
            {
                var authService = CurrentEnvironment.AuthorizationService;

                if (authService != null && authService.IsAuthorized(AuthorizationContext.Administrator, null))
                {
                    if (SelectedTask != null && SelectedTask.OldName != null && SelectedTask.IsDirty)
                    {
                        if (HasErrors && !Error.StartsWith(SaveErrorPrefix))
                        {
                            ShowSaveErrorDialog(Error);
                            return false;
                        }

                        if (SelectedTask.OldName != SelectedTask.Name && !SelectedTask.OldName.Contains(NewTaskName) && !SelectedTask.IsNew)
                        {
                            var showNameChangedConflict = _popupController.ShowNameChangedConflict(SelectedTask.OldName,
                                                                                                   SelectedTask.Name);
                            if (showNameChangedConflict == MessageBoxResult.Cancel || showNameChangedConflict == MessageBoxResult.None)
                            {
                                return false;
                            }
                            if (showNameChangedConflict == MessageBoxResult.No)
                            {
                                SelectedTask.Name = SelectedTask.OldName;
                                NotifyOfPropertyChange(() => Name);
                            }
                        }
                        if (SelectedTask.OldName != SelectedTask.Name && SelectedTask.OldName.Contains(NewTaskName))
                        {
                            SelectedTask.OldName = SelectedTask.Name;
                        }
                    }

                    GetCredentials(SelectedTask);
                    string errorMessage;
                    if (!ScheduledResourceModel.Save(SelectedTask, out errorMessage))
                    {
                        ShowSaveErrorDialog(errorMessage);
                        ShowError(errorMessage);
                        return false;
                    }
                    Dev2Logger.Info(String.Format("Save Schedule. Environment: {0} Name:{1} ", CurrentEnvironment.Name, SelectedTask != null ? SelectedTask.Name : ""));
                    if (SelectedTask != null)
                    {
                        SelectedTask.Errors.ClearErrors();
                        NotifyOfPropertyChange(() => Error);
                        NotifyOfPropertyChange(() => Errors);
                        SelectedTask.IsDirty = false;
                        SelectedTask.OldName = SelectedTask.Name;
                        SelectedTask.IsNew = false;
                    }
                    NotifyOfPropertyChange(() => TaskList);
                }
                else
                {
                    ShowError(@"Error while saving: You don't have permission to schedule on this server. You need Administrator permission.");
                    return false;
                }
                return true;
            }
            ShowError(NotConnectedErrorMessage);
            return false;
        }

        public void CreateNewTask()
        {
            if (IsDirty)
            {
                _popupController.Show("Please save currently edited Task(s) before creating a new one.", "Save before continuing", MessageBoxButton.OK, MessageBoxImage.Error, null, false, true, false, false);
                return;
            }

            var dev2DailyTrigger = new Dev2DailyTrigger(new TaskServiceConvertorFactory(), new DailyTrigger());
            var scheduleTrigger = _schedulerFactory.CreateTrigger(TaskState.Ready, dev2DailyTrigger);
            ScheduledResource scheduledResource = new ScheduledResource(NewTaskName + _newTaskCounter, SchedulerStatus.Enabled, scheduleTrigger.Trigger.Instance.StartBoundary, scheduleTrigger, string.Empty, Guid.NewGuid().ToString()) { IsDirty = true };
            scheduledResource.OldName = scheduledResource.Name;
            var newres = ScheduledResourceModel.ScheduledResources[ScheduledResourceModel.ScheduledResources.Count == 1 ? 0 : ScheduledResourceModel.ScheduledResources.Count - 1];
            ScheduledResourceModel.ScheduledResources[ScheduledResourceModel.ScheduledResources.Count == 1 ? 0 : ScheduledResourceModel.ScheduledResources.Count - 1] = scheduledResource;
            ScheduledResourceModel.ScheduledResources.Add(newres);

            _newTaskCounter++;

            NotifyOfPropertyChange(() => TaskList);
            SelectedTask = ScheduledResourceModel.ScheduledResources[ScheduledResourceModel.ScheduledResources.Count == 1 ? 1 : ScheduledResourceModel.ScheduledResources.Count - 2];
            WorkflowName = string.Empty;
            SelectedTask.IsNew = true;
            ViewModelUtils.RaiseCanExecuteChanged(NewCommand);
        }

        void DeleteTask()
        {
            if (SelectedTask != null && CurrentEnvironment != null)
            {
                if (CurrentEnvironment.IsConnected)
                {
                    if (CurrentEnvironment.AuthorizationService.IsAuthorized(AuthorizationContext.Administrator, null))
                    {
                        if (_popupController.ShowDeleteConfirmation(SelectedTask.Name) == MessageBoxResult.Yes)
                        {
                            int index = ScheduledResourceModel.ScheduledResources.IndexOf(SelectedTask);
                            int indexInFilteredList = TaskList.IndexOf(SelectedTask);
                            if (index != -1)
                            {
                                Dev2Logger.Info(String.Format("Delete Schedule Name: {0} Resource:{1} Env:{2}", SelectedTask.Name, SelectedTask.ResourceId, CurrentEnvironment.Name));

                                ScheduledResourceModel.DeleteSchedule(SelectedTask);
                                //if delete is successfull then do the code below
                                ScheduledResourceModel.ScheduledResources.RemoveAt(index);
                                NotifyOfPropertyChange(() => TaskList);
                                if (indexInFilteredList <= TaskList.Count && indexInFilteredList > 0)
                                {
                                    SelectedTask = TaskList[indexInFilteredList - 1];
                                }
                                else if (indexInFilteredList == 0 && TaskList.Count > 0)
                                {
                                    SelectedTask = TaskList[0];
                                }
                            }
                            NotifyOfPropertyChange(() => History);
                        }
                    }
                    else
                    {
                        ShowError(@"Error while saving: You don't have permission to schedule on this server. You need Administrator permission.");
                    }
                }
                else
                {
                    ShowError(NotConnectedErrorMessage);
                }
            }
        }

        void EditTrigger()
        {
            if (SelectedTask != null)
            {
                TriggerEditDialog = new TriggerEditDialog(SelectedTask.Trigger.Trigger.Instance, false);
                var tempTrigger = ShowEditTriggerDialog();
                if (tempTrigger != null)
                {
                    Trigger = tempTrigger;
                    SelectedTask.NextRunDate = Trigger.Trigger.StartBoundary;
                }
                NotifyOfPropertyChange(() => TriggerText);
            }
        }

        void AddWorkflow()
        {
            if (SelectedTask != null && CurrentEnvironment != null)
            {
                if (_task != null && _task.Status == TaskStatus.Running)
                {
                    _task.Wait();
                    if (!_task.IsFaulted)
                        CurrentResourcePickerDialog = _task.Result;
                }

                if (!string.IsNullOrEmpty(WorkflowName) && CurrentEnvironment.ResourceRepository != null)
                {
                    var resourceModel = CurrentEnvironment.ResourceRepository.FindSingle(c => c.ResourceName == WorkflowName);
                    if (resourceModel != null)
                    {
                        CurrentResourcePickerDialog.SelectResource(resourceModel.ID);
                    }
                }
                var hasResult = CurrentResourcePickerDialog.ShowDialog(CurrentEnvironment);
                if (hasResult)
                {
                    WorkflowName = CurrentResourcePickerDialog.SelectedResource.ResourcePath;

                    SelectedTask.ResourceId = CurrentResourcePickerDialog.SelectedResource.ResourceId;
                    if (SelectedTask.Name.StartsWith("New Task"))
                    {
                        Name = CurrentResourcePickerDialog.SelectedResource.ResourceName;
                        NotifyOfPropertyChange(() => Name);
                    }
                    SelectedTask.IsDirty = true;
                    NotifyOfPropertyChange(() => WorkflowName);
                    NotifyOfPropertyChange(() => TaskList);
                }
            }
        }

        void SetupServer(IServer tmpEnv)
        {
            CurrentEnvironment = ToEnvironmentModel(tmpEnv);

            if (CurrentEnvironment != null && CurrentEnvironment.AuthorizationService != null && CurrentEnvironment.IsConnected)
            {
                if (tmpEnv.Permissions.Any(a => a.Administrator))
                {
                    ClearConnectionError();
                    var environment = CurrentEnvironment ?? EnvironmentRepository.Instance.ActiveEnvironment;

                    IServer server = new Server(environment);
                    if (server.Permissions == null)
                    {
                        server.Permissions = new List<IWindowsGroupPermission>();
                        server.Permissions.AddRange(environment.AuthorizationService.SecurityService.Permissions);
                    }
                    var env = new EnvironmentViewModel(server, CustomContainer.Get<IShellViewModel>(), true);

                    ScheduledResourceModel = new ClientScheduledResourceModel(CurrentEnvironment, CreateNewTask);
                    IsLoading = true;
#pragma warning disable 4014
                    _asyncWorker.Start(
#pragma warning restore 4014
() =>
                        {
                            if (_currentResourcePicker == null)
                            {
                                _task = ResourcePickerDialog.CreateAsync(enDsfActivityType.Workflow, env);
                                _task.Wait();
                            }
                        }, () =>
                        {
                            try
                            {
                                CurrentResourcePickerDialog = _task.Result;
                                var cmd = AddWorkflowCommand as Microsoft.Practices.Prism.Commands.DelegateCommand;
                                if (cmd != null)
                                {
                                    cmd.RaiseCanExecuteChanged();
                                }

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
                        });
                }
                else
                {
                    SetConnectionError();
                    ClearViewModel();
                }
            }
            else
            {
                ClearConnectionError();
                ClearViewModel();
            }
        }

        void SetConnectionError()
        {
            ConnectionError = @"You don't have permission to schedule on this server. You need Administrator permission.";
            HasConnectionError = true;
        }

        void ClearConnectionError()
        {
            ConnectionError = string.Empty;
            HasConnectionError = false;
        }

        void ClearViewModel()
        {
            Name = string.Empty;
            WorkflowName = string.Empty;
            Status = SchedulerStatus.Enabled;
            AccountName = string.Empty;
            Password = string.Empty;
            NumberOfRecordsToKeep = string.Empty;
            Trigger = null;
            if (ScheduledResourceModel != null)
            {
                ScheduledResourceModel.ScheduledResources.Clear();
            }
            NotifyOfPropertyChange(() => TaskList);
            NotifyOfPropertyChange(() => History);
            Errors.ClearErrors();
        }

        #endregion

        #region Public Methods

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

        #endregion
        public bool IsDirty
        {
            get
            {
                try
                {
                    if (TaskList == null || TaskList.Count == 0)
                    {
                        return false;
                    }
                    var isDirty = TaskList.Any(resource => resource.IsDirty);
                    var cnct = Server.IsConnected;
                    return isDirty && cnct;
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
        #region Public Methods

        public virtual bool DoDeactivate(bool showMessage)
        {
            if (showMessage)
            {
                if (SelectedTask != null && SelectedTask.IsDirty)
                {
                    MessageBoxResult showSchedulerCloseConfirmation = _popupController.ShowSchedulerCloseConfirmation();
                    if (showSchedulerCloseConfirmation == MessageBoxResult.Cancel || showSchedulerCloseConfirmation == MessageBoxResult.None)
                    {
                        return false;
                    }
                    if (showSchedulerCloseConfirmation == MessageBoxResult.No)
                    {
                        return true;
                    }
                    return SaveTasks();
                }
            }
            if (SelectedTask != null && !showMessage)
                return SaveTasks();
            return true;
        }


        public virtual void ShowSaveErrorDialog(string error)
        {
            _popupController.ShowSaveErrorDialog(error);
        }
        public virtual IScheduleTrigger ShowEditTriggerDialog()
        {
            var tmpTrigger = SelectedTask.Trigger.Trigger.Instance;
            if (TriggerEditDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (!TriggerEquals(TriggerEditDialog.Trigger, tmpTrigger))
                {
                    return _schedulerFactory.CreateTrigger(TaskState.Disabled, new Dev2Trigger(null, TriggerEditDialog.Trigger));
                }
            }
            return null;
        }

        public static bool TriggerEquals(Microsoft.Win32.TaskScheduler.Trigger a, Microsoft.Win32.TaskScheduler.Trigger b)
        {
            return a.ToString() == b.ToString() && a.StartBoundary == b.StartBoundary && a.EndBoundary == b.EndBoundary && a.ExecutionTimeLimit == b.ExecutionTimeLimit;
        }
        public virtual void GetCredentials(IScheduledResource scheduledResource)
        {
            var cancelled = false;
            while ((String.IsNullOrEmpty(AccountName) || String.IsNullOrEmpty(Password)) && !cancelled)
            {
                CredentialsDialog credentialsDialog = new CredentialsDialog { UserName = scheduledResource.UserName, Options = CredentialsDialogOptions.GenericCredentials, ValidatePassword = true };
                var dialogResult = credentialsDialog.ShowDialog();
                if (dialogResult == System.Windows.Forms.DialogResult.Cancel)
                {
                    cancelled = true;
                }
                AccountName = credentialsDialog.UserName;
                Password = credentialsDialog.Password;
            }
        }

        #endregion

        #region Implementation of IHelpSource

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

        #endregion

        public string ResourceType
        {
            get
            {
                return "Scheduler";
            }
        }
        public IServer Server
        {
            get;
            set;
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

