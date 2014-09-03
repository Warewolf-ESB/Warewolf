using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
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
using Dev2.Common.Interfaces.Data.TO;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Scheduler.Interfaces;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.CustomControls.Connections;
using Dev2.DataList.Contract;
using Dev2.Dialogs;
using Dev2.Interfaces;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Scheduler;
using Dev2.Services.Events;
using Dev2.Services.Security;
using Dev2.Studio.Controller;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Enums;
using Dev2.Studio.ViewModels.WorkSurface;
using Dev2.TaskScheduler.Wrappers;
using Dev2.Threading;
using Microsoft.Win32.TaskScheduler;

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
        IResourcePickerDialog _resourcePicker;
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

        #endregion

        #region Ctor

        public SchedulerViewModel()
            : this(EventPublishers.Aggregator, new DirectoryObjectPickerDialog(), new PopupController(), new AsyncWorker(), null)
        {
        }


        public SchedulerViewModel(IEventAggregator eventPublisher, DirectoryObjectPickerDialog directoryObjectPicker, IPopupController popupController, IAsyncWorker asyncWorker, IConnectControlViewModel connectControlViewModel)
            : base(eventPublisher)
        {
            VerifyArgument.IsNotNull("directoryObjectPicker", directoryObjectPicker);
            DirectoryObjectPickerDialog directoryObjectPicker1 = directoryObjectPicker;

            VerifyArgument.IsNotNull("popupController", popupController);
            _popupController = popupController;

            VerifyArgument.IsNotNull("asyncWorker", asyncWorker);
            _asyncWorker = asyncWorker;

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
            ConnectControlViewModel = connectControlViewModel ?? new ConnectControlViewModel(OnServerChanged, "Server: ", false);
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

        public IResourcePickerDialog ResourcePickerDialog
        {
            get
            {
                return _resourcePicker;
            }
            set
            {
                _resourcePicker = value;
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
                if(Equals(value, _connectControlViewModel))
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
                if(SelectedTask != null)
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
                if(SelectedTask != null)
                {
                    if(value == SelectedTask.Status)
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
                if(SelectedTask != null)
                {
                    if(string.IsNullOrEmpty(value))
                    {
                        ShowError(BlankWorkflowNameErrorMessage);
                    }
                    else
                    {
                        ClearError(BlankWorkflowNameErrorMessage);
                    }

                    if(value == SelectedTask.WorkflowName)
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
                if(SelectedTask == null || SelectedTask.Name == value)
                {
                    return;
                }
                if(string.IsNullOrEmpty(value))
                {
                    ShowError(BlankNameErrorMessage);
                }
                else
                {
                    ClearError(BlankNameErrorMessage);
                }

                if(TaskList.Any(c => c.Name == value))
                {
                    foreach(var resource in TaskList.Where(a => a.Name == value))
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
                if(SelectedTask != null)
                {
                    if(value == SelectedTask.RunAsapIfScheduleMissed)
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
                if(SelectedTask != null)
                {
                    if(value == SelectedTask.NumberOfHistoryToKeep.ToString(CultureInfo.InvariantCulture))
                    {
                        return;
                    }
                    if(string.IsNullOrEmpty(value))
                    {
                        SelectedTask.NumberOfHistoryToKeep = 0;
                        SelectedTask.IsDirty = true;
                    }
                    else
                    {
                        int val;
                        if(value.IsWholeNumber(out val))
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
                if(ScheduledResourceModel != null && SelectedTask != null && IsHistoryTab && _history == null)
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
                if(null == value)
                {
                    EventPublisher.Publish(new DebugOutputMessage(new List<IDebugState>()));
                    return;
                }

                if(Equals(value, _selectedHistory))
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
                if(ScheduledResourceModel != null)
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
                if(SelectedTask != null)
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
                if(Equals(_selectedTask, value))
                {
                    return;
                }

                _selectedTask = value;

                NotifyOfPropertyChange(() => SelectedTask);

                if(_selectedTask != null)
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
                    if(IsHistoryTab)
                    {
                        NotifyOfPropertyChange(() => History);
                    }
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
                if(Equals(_triggerEditDialog, value))
                {
                    return;
                }
                _triggerEditDialog = value;
                NotifyOfPropertyChange(() => TriggerEditDialog);
            }
        }

        public IScheduledResourceModel ScheduledResourceModel { get; set; }

        public string AccountName
        {
            get
            {
                return SelectedTask != null ? SelectedTask.UserName : string.Empty;
            }
            set
            {
                if(SelectedTask != null)
                {
                    if(Equals(SelectedTask.UserName, value))
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
                if(SelectedTask != null)
                {
                    if(Equals(SelectedTask.Password, value))
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
                if(_selectedTask != null) _selectedTask.Errors = value;
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
                if(value.Equals(_isHistoryTabVisible))
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
                if(value.Equals(_isProgressBarVisible))
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
                if(HasErrors)
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
                if(Equals(value, _activeItem))
                {
                    return;
                }
                _activeItem = value;
                if(IsHistoryTab)
                {
                    NotifyOfPropertyChange(() => History);
                }
            }
        }

        private bool IsHistoryTab
        {
            get
            {
                if(ActiveItem != null)
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
                    (_closeCommand = new DelegateCommand(o => CloseHelp()));
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
                       (_deleteCommand = new DelegateCommand(param => DeleteTask()));
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
                       (_addWorkflowCommand = new DelegateCommand(param => AddWorkflow()));
            }
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
            HelpText = HelpTextResources.SchedulerSettingsHelpTextSettingsView;
        }

        ActivityDesignerToggle CreateHelpToggle()
        {
            var toggle = ActivityDesignerToggle.Create(
                collapseImageSourceUri: "pack://application:,,,/Dev2.Activities.Designers;component/Images/ServiceHelp-32.png",
                collapseToolTip: "Close Help",
                expandImageSourceUri: "pack://application:,,,/Dev2.Activities.Designers;component/Images/ServiceHelp-32.png",
                expandToolTip: "Open Help",
                automationID: "HelpToggle"
                );

            return toggle;
        }

        bool SaveTasks()
        {
            if(CurrentEnvironment.IsConnected)
            {
                var authService = CurrentEnvironment.AuthorizationService;

                if(authService != null && authService.IsAuthorized(AuthorizationContext.Administrator, null))
                {
                    if(SelectedTask != null && SelectedTask.IsDirty)
                    {
                        if(HasErrors && !Error.StartsWith(SaveErrorPrefix))
                        {
                            ShowSaveErrorDialog(Error);
                            return false;
                        }

                        if(SelectedTask.OldName != SelectedTask.Name && !SelectedTask.OldName.Contains(NewTaskName) && !SelectedTask.IsNew)
                        {
                            var showNameChangedConflict = _popupController.ShowNameChangedConflict(SelectedTask.OldName,
                                                                                                   SelectedTask.Name);
                            if(showNameChangedConflict == MessageBoxResult.Cancel || showNameChangedConflict == MessageBoxResult.None)
                            {
                                return false;
                            }
                            if(showNameChangedConflict == MessageBoxResult.No)
                            {
                                SelectedTask.Name = SelectedTask.OldName;
                                NotifyOfPropertyChange(() => Name);
                            }

                        }
                        if(SelectedTask.OldName != SelectedTask.Name && SelectedTask.OldName.Contains(NewTaskName))
                        {
                            SelectedTask.OldName = SelectedTask.Name;
                        }
                    }

                    GetCredentials(SelectedTask);
                    string errorMessage;
                    if(!ScheduledResourceModel.Save(SelectedTask, out errorMessage))
                    {
                        ShowSaveErrorDialog(errorMessage);
                        ShowError(errorMessage);
                        return false;
                    }
                    Dev2Logger.Log.Info(String.Format("Save Schedule. Environment: {0} Name:{1} ", CurrentEnvironment.Name,SelectedTask!=null ?SelectedTask.Name:""));
                    if(SelectedTask != null)
                    {
                        SelectedTask.Errors.ClearErrors();
                        NotifyOfPropertyChange(() => Error);
                        NotifyOfPropertyChange(() => Errors);
                        SelectedTask.OldName = SelectedTask.Name;
                        SelectedTask.IsNew = false;
                    }
                    NotifyOfPropertyChange(() => TaskList);
                }
                else
                {
                    ShowError(@"Error while saving: You don't have permission to schedule on this server.
You need Administrator permission.");
                    return false;
                }
                return true;
            }
            ShowError(NotConnectedErrorMessage);
            return false;
        }



        public void CreateNewTask()
        {
            if(CurrentEnvironment != null)
            {
                Dev2Logger.Log.Info(String.Format("Delete Schedule Environment: {0} ",CurrentEnvironment.Name));
            }
            var dev2DailyTrigger = new Dev2DailyTrigger(new TaskServiceConvertorFactory(), new DailyTrigger());
            var scheduleTrigger = _schedulerFactory.CreateTrigger(TaskState.Ready, dev2DailyTrigger);
            ScheduledResource scheduledResource = new ScheduledResource(NewTaskName + _newTaskCounter, SchedulerStatus.Enabled, scheduleTrigger.Trigger.Instance.StartBoundary, scheduleTrigger, string.Empty) { IsDirty = true };
            scheduledResource.OldName = scheduledResource.Name;
            ScheduledResourceModel.ScheduledResources.Add(scheduledResource);
            _newTaskCounter++;
            NotifyOfPropertyChange(() => TaskList);
            SelectedTask = ScheduledResourceModel.ScheduledResources.Last();
            WorkflowName = string.Empty;
            SelectedTask.IsNew = true;
        }

        void DeleteTask()
        {
            if(SelectedTask != null && CurrentEnvironment != null)
            {
                if(CurrentEnvironment.IsConnected)
                {
                    if(CurrentEnvironment.AuthorizationService.IsAuthorized(AuthorizationContext.Administrator, null))
                    {
                        if(_popupController.ShowDeleteConfirmation(SelectedTask.Name) == MessageBoxResult.Yes)
                        {
                            int index = ScheduledResourceModel.ScheduledResources.IndexOf(SelectedTask);
                            int indexInFilteredList = TaskList.IndexOf(SelectedTask);
                            if(index != -1)
                            {
                                Dev2Logger.Log.Info(String.Format("Delete Schedule Name: {0} Resource:{1} Env:{2}",SelectedTask.Name,SelectedTask.ResourceId,CurrentEnvironment.Name));

                                ScheduledResourceModel.DeleteSchedule(SelectedTask);
                                //if delete is successfull then do the code below
                                ScheduledResourceModel.ScheduledResources.RemoveAt(index);
                                NotifyOfPropertyChange(() => TaskList);
                                if(indexInFilteredList <= TaskList.Count && indexInFilteredList > 0)
                                {
                                    SelectedTask = TaskList[indexInFilteredList - 1];
                                }
                                else if(indexInFilteredList == 0 && TaskList.Count > 0)
                                {
                                    SelectedTask = TaskList[0];
                                }
                            }
                            NotifyOfPropertyChange(() => History);
                        }
                    }
                    else
                    {
                        ShowError(@"Error while saving: You don't have permission to schedule on this server.
You need Administrator permission.");
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
            if(SelectedTask != null)
            {
                TriggerEditDialog = new TriggerEditDialog(SelectedTask.Trigger.Trigger.Instance, false);
                var tempTrigger = ShowEditTriggerDialog();
                if(tempTrigger != null)
                {
                    Trigger = tempTrigger;
                    SelectedTask.NextRunDate = Trigger.Trigger.StartBoundary;
                }

                NotifyOfPropertyChange(() => TriggerText);
            }
        }

        void AddWorkflow()
        {
            if(SelectedTask != null && CurrentEnvironment != null)
            {

                if(!string.IsNullOrEmpty(WorkflowName) && CurrentEnvironment.ResourceRepository != null)
                {
                    var resourceModel = CurrentEnvironment.ResourceRepository.FindSingle(c => c.ResourceName == WorkflowName);
                    if(resourceModel != null)
                    {
                        _resourcePicker.SelectedResource = resourceModel;
                    }
                }
                var hasResult = _resourcePicker.ShowDialog(CurrentEnvironment);
                if(hasResult)
                {
                    WorkflowName = _resourcePicker.SelectedResource.Category;

                    SelectedTask.ResourceId = _resourcePicker.SelectedResource.ID;
                    if(SelectedTask.Name.StartsWith("New Task"))
                    {
                        Name = _resourcePicker.SelectedResource.ResourceName;
                        NotifyOfPropertyChange(() => Name);
                    }
                    SelectedTask.IsDirty = true;
                    NotifyOfPropertyChange(() => WorkflowName);
                    NotifyOfPropertyChange(() => TaskList);
                }
            }
        }

        void OnServerChanged(object obj)
        {
            var tmpEnv = obj as IEnvironmentModel;

            if(!DoDeactivate())
            {
                return;
            }
            CurrentEnvironment = tmpEnv;

            if(CurrentEnvironment != null && CurrentEnvironment.AuthorizationService != null && CurrentEnvironment.IsConnected)
            {
                if(CurrentEnvironment.AuthorizationService.IsAuthorized(AuthorizationContext.Administrator, null))
                {
                    ClearConnectionError();
                    _resourcePicker = new ResourcePickerDialog(enDsfActivityType.Workflow, CurrentEnvironment);
                    ScheduledResourceModel = new ClientScheduledResourceModel(CurrentEnvironment);
                    IsLoading = true;
                    _asyncWorker.Start(
                        () =>
                        ScheduledResourceModel.ScheduledResources = ScheduledResourceModel.GetScheduledResources(), () =>
                    {
                        foreach(var scheduledResource in ScheduledResourceModel.ScheduledResources)
                        {
                            scheduledResource.NextRunDate = scheduledResource.Trigger.Trigger.StartBoundary;
                            scheduledResource.OldName = scheduledResource.Name;
                        }

                        NotifyOfPropertyChange(() => TaskList);
                        if(TaskList.Count > 0)
                        {
                            SelectedTask = TaskList[0];
                        }
                        IsLoading = false;
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
            ConnectionError = @"You don't have permission to schedule on this server.
You need Administrator permission.";
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
            if(ScheduledResourceModel != null)
            {
                ScheduledResourceModel.ScheduledResources = new ObservableCollection<IScheduledResource>();
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
            if(!string.IsNullOrEmpty(description))
            {
                Errors.AddError(description, true);
                NotifyOfPropertyChange(() => Error);
                NotifyOfPropertyChange(() => HasErrors);
            }
        }

        #endregion

        #region Public Methods

        public virtual bool DoDeactivate()
        {
            if(SelectedTask != null && SelectedTask.IsDirty)
            {
                MessageBoxResult showSchedulerCloseConfirmation = _popupController.ShowSchedulerCloseConfirmation();
                if(showSchedulerCloseConfirmation == MessageBoxResult.Cancel || showSchedulerCloseConfirmation == MessageBoxResult.None)
                {
                    return false;
                }
                if(showSchedulerCloseConfirmation == MessageBoxResult.No)
                {
                    return true;
                }
                return SaveTasks();
            }
            return true;
        }

        public virtual void ShowSaveErrorDialog(string error)
        {
            _popupController.ShowSaveErrorDialog(error);
        }
        [ExcludeFromCodeCoverage]
        public virtual IScheduleTrigger ShowEditTriggerDialog()
        {
            var tmpTrigger = SelectedTask.Trigger.Trigger.Instance;
            if(TriggerEditDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if(TriggerEditDialog.Trigger.ToString() != tmpTrigger.ToString())
                {
                    return _schedulerFactory.CreateTrigger(TaskState.Disabled, new Dev2Trigger(null, TriggerEditDialog.Trigger));
                }
            }
            return null;
        }

        [ExcludeFromCodeCoverage]
        public virtual void GetCredentials(IScheduledResource scheduledResource)
        {
            var cancelled = false;
            while((String.IsNullOrEmpty(AccountName) || String.IsNullOrEmpty(Password)) && !cancelled)
            {

                CredentialsDialog credentialsDialog = new CredentialsDialog { UserName = scheduledResource.UserName, Options = CredentialsDialogOptions.GenericCredentials, ValidatePassword = true };
                var dialogResult = credentialsDialog.ShowDialog();
                if(dialogResult == System.Windows.Forms.DialogResult.Cancel)
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
                if(value == _helpText)
                    return;
                _helpText = value;
                NotifyOfPropertyChange(() => HelpText);
            }
        }

        #endregion
    }
}

