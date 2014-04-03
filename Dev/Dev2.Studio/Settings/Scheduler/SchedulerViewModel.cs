using Caliburn.Micro;
using CubicOrange.Windows.Forms.ActiveDirectory;
using Dev2.Activities.Designers2.Core;
using Dev2.Activities.Designers2.Core.Help;
using Dev2.AppResources.Enums;
using Dev2.Common.ExtMethods;
using Dev2.DataList.Contract;
using Dev2.Diagnostics;
using Dev2.Dialogs;
using Dev2.Messages;
using Dev2.Scheduler;
using Dev2.Scheduler.Interfaces;
using Dev2.Services.Events;
using Dev2.Services.Security;
using Dev2.Studio.Controller;
using Dev2.Studio.Core.Controller;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.Enums;
using Dev2.Studio.ViewModels.WorkSurface;
using Dev2.TaskScheduler.Wrappers;
using Dev2.Threading;
using Microsoft.Win32.TaskScheduler;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;

namespace Dev2.Settings.Scheduler
{
    public class SchedulerViewModel : BaseWorkSurfaceViewModel, IHandle<ServerSelectionChangedMessage>, IHandle<SelectedServerConnectedMessage>, IHelpSource
    {
        #region Fields

        const string NumberOfHistoryErrorMessage = "Number of History Records to Keep must be a whole number";
        const string DuplicateNameErrorMessage = "There is already a task with the same name";
        const string LoginErrorMessage = "Error while saving: Logon failure: unknown user name or bad password";
        const string BlankWorkflowNameErrorMessage = "Please select a workflow to schedule";
        const string BlankNameErrorMessage = "The name can not be blank";
        const string NewTaskName = "New Task";
        IResourcePickerDialog _resourcePicker;
        int _newTaskCounter = 1;
        ICommand _saveCommand;
        ICommand _newCommand;
        ICommand _deleteCommand;
        ICommand _editTriggerCommand;
        ICommand _addWorkflowCommand;

        IClientSchedulerFactory _schedulerFactory;
        IScheduledResource _selectedTask;
        TriggerEditDialog _triggerEditDialog;
        readonly IPopupController _popupController;
        readonly IAsyncWorker _asyncWorker;
        bool _isSaveEnabled;

        IResourceHistory _selectedHistory;
        IList<IResourceHistory> _history;
        TabItem _activeItem;
        bool _isProgressBarVisible;
        bool _isHistoryTabVisible;
        string _helpText;
        bool _isLoading;
        string _connectionError;
        bool _hasConnectionError;

        #endregion

        #region Ctor

        public SchedulerViewModel()
            : this(EventPublishers.Aggregator, new DirectoryObjectPickerDialog(), new PopupController(), new AsyncWorker())
        {
        }


        public SchedulerViewModel(IEventAggregator eventPublisher, DirectoryObjectPickerDialog directoryObjectPicker, IPopupController popupController, IAsyncWorker asyncWorker)
            : base(eventPublisher)
        {
            VerifyArgument.IsNotNull("directoryObjectPicker", directoryObjectPicker);
            DirectoryObjectPickerDialog directoryObjectPicker1 = directoryObjectPicker;

            VerifyArgument.IsNotNull("popupController", popupController);
            _popupController = popupController;
            _asyncWorker = asyncWorker;

            IsLoading = false;
            CloseHelpCommand = new RelayCommand(o => CloseHelp(), o => true);
            directoryObjectPicker1.AllowedObjectTypes = ObjectTypes.Users;
            directoryObjectPicker1.DefaultObjectTypes = ObjectTypes.Users;
            directoryObjectPicker1.AllowedLocations = Locations.All;
            directoryObjectPicker1.DefaultLocations = Locations.JoinedDomain;
            directoryObjectPicker1.MultiSelect = false;
            directoryObjectPicker1.TargetComputer = string.Empty;
            directoryObjectPicker1.ShowAdvancedView = false;

            InitializeHelp();

            IsSaveEnabled = true;
            var taskServiceConvertorFactory = new TaskServiceConvertorFactory();
            SchedulerFactory = new ClientSchedulerFactory(new Dev2TaskService(taskServiceConvertorFactory), taskServiceConvertorFactory);
            ServerChangedCommand = new RelayCommand(OnServerChanged, o => true);
        }

        #endregion

        #region Properties

        public bool IsCurrentEnvironmentConnected
        {
            get
            {
                if(CurrentEnvironment != null)
                {
                    return CurrentEnvironment.IsConnected;
                }
                return false;
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

        public ActivityDesignerToggle HelpToggle { get; private set; }

        public string OldName
        {
            get
            {
                return SelectedTask != null ? SelectedTask.OldName : string.Empty;
            }
            set
            {
                if(SelectedTask != null)
                {
                    SelectedTask.OldName = value;
                }
            }
        }

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
                    int val;
                    if(value.IsWholeNumber(out val))
                    {
                        SelectedTask.NumberOfHistoryToKeep = val;
                        SelectedTask.IsDirty = true;
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
                    EventPublisher.Publish(new DebugOutputMessage(new List<DebugState>()));
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
                    NotifyOfPropertyChange(() => IsSaveEnabled);
                    NotifyOfPropertyChange(() => AccountName);
                    NotifyOfPropertyChange(() => Password);
                    NotifyOfPropertyChange(() => History);
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
                    ClearError(LoginErrorMessage);
                    SelectedTask.Password = value;
                    SelectedTask.IsDirty = true;
                    NotifyOfPropertyChange(() => Password);
                }
            }
        }

        public bool IsSaveEnabled
        {
            get
            {
                return _isSaveEnabled;
            }
            set
            {
                _isSaveEnabled = value;
                NotifyOfPropertyChange(() => IsSaveEnabled);
            }
        }

        public ICommand ServerChangedCommand { get; private set; }

        public IEnvironmentModel CurrentEnvironment { get; set; }

        public bool HasErrors
        {
            get { return Errors.HasErrors(); }
        }

        public ErrorResultTO Errors
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
                    var fetchError = Errors.FetchErrors()[0];
                    if(fetchError != LoginErrorMessage)
                    {
                        IsSaveEnabled = false;
                    }
                    return fetchError;
                }
                return string.Empty;
            }
        }

        #endregion

        #region Commands

        public ICommand SaveCommand
        {
            get
            {
                return _saveCommand ??
                       (_saveCommand = new RelayCommand(param => SaveTasks()));
            }
        }

        public ICommand CloseHelpCommand
        {
            get;
            private set;
        }

        public ICommand NewCommand
        {
            get
            {
                return _newCommand ??
                       (_newCommand = new RelayCommand(param => CreateNewTask()));
            }
        }

        public ICommand DeleteCommand
        {
            get
            {
                return _deleteCommand ??
                       (_deleteCommand = new RelayCommand(param => DeleteTask()));
            }
        }

        public ICommand EditTriggerCommand
        {
            get
            {
                return _editTriggerCommand ??
                       (_editTriggerCommand = new RelayCommand(param => EditTrigger()));
            }
        }

        public ICommand AddWorkflowCommand
        {
            get
            {
                return _addWorkflowCommand ??
                       (_addWorkflowCommand = new RelayCommand(param => AddWorkflow()));
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

        void SaveTasks()
        {
            if(SelectedTask != null && SelectedTask.IsDirty)
            {
                if(SelectedTask.OldName != SelectedTask.Name && !SelectedTask.OldName.Contains(NewTaskName))
                {
                    var showNameChangedConflict = _popupController.ShowNameChangedConflict(SelectedTask.OldName,
                                                                                           SelectedTask.Name);
                    if(showNameChangedConflict == MessageBoxResult.Cancel)
                    {
                        return;
                    }
                    if(showNameChangedConflict == MessageBoxResult.No)
                    {
                        SelectedTask.Name = SelectedTask.OldName;
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
                ShowError(errorMessage);
            }
            else
            {
                if(SelectedTask != null) SelectedTask.OldName = SelectedTask.Name;
                NotifyOfPropertyChange(() => TaskList);
            }
        }



        void CreateNewTask()
        {
            var dev2DailyTrigger = new Dev2DailyTrigger(new TaskServiceConvertorFactory(), new DailyTrigger());
            var scheduleTrigger = _schedulerFactory.CreateTrigger(TaskState.Ready, dev2DailyTrigger);
            ScheduledResource scheduledResource = new ScheduledResource(NewTaskName + _newTaskCounter, SchedulerStatus.Enabled, scheduleTrigger.Trigger.Instance.StartBoundary, scheduleTrigger, string.Empty);
            scheduledResource.IsDirty = true;
            scheduledResource.OldName = scheduledResource.Name;
            ScheduledResourceModel.ScheduledResources.Add(scheduledResource);
            _newTaskCounter++;
            NotifyOfPropertyChange(() => TaskList);
            SelectedTask = ScheduledResourceModel.ScheduledResources.Last();
            WorkflowName = string.Empty;
        }

        void DeleteTask()
        {
            if(SelectedTask != null)
            {
                if(_popupController.ShowDeleteConfirmation(SelectedTask.Name) == MessageBoxResult.Yes)
                {
                    int index = ScheduledResourceModel.ScheduledResources.IndexOf(SelectedTask);
                    int indexInFilteredList = TaskList.IndexOf(SelectedTask);
                    if(index != -1)
                    {
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
                var hasResult = _resourcePicker.ShowDialog();
                if(hasResult)
                {
                    WorkflowName = _resourcePicker.SelectedResource.ResourceName;
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
            CurrentEnvironment = obj as IEnvironmentModel;

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
            NotifyOfPropertyChange(() => IsCurrentEnvironmentConnected);
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
            Errors.ClearErrors();
        }

        void ClearError(string description)
        {
            Errors.RemoveError(description);
            if(!Errors.HasErrors())
            {
                IsSaveEnabled = true;
            }
            NotifyOfPropertyChange(() => Error);
            NotifyOfPropertyChange(() => HasErrors);
        }

        void ShowError(string description)
        {
            if(!string.IsNullOrEmpty(description))
            {
                if(!description.StartsWith("Error while saving:"))
                {
                    IsSaveEnabled = false;
                }
                Errors.AddError(description, true);
                NotifyOfPropertyChange(() => Error);
                NotifyOfPropertyChange(() => HasErrors);
            }
        }

        #endregion

        #region Public Methods

        public virtual IScheduleTrigger ShowEditTriggerDialog()
        {
            var tmpTrigger = TriggerEditDialog.Trigger;
            if(TriggerEditDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if(TriggerEditDialog.Trigger != tmpTrigger)
                {
                    return _schedulerFactory.CreateTrigger(TaskState.Disabled, new Dev2Trigger(null, TriggerEditDialog.Trigger));
                }
            }
            return null;
        }

        public virtual void GetCredentials(IScheduledResource scheduledResource)
        {
            var cancelled = false;
            while((String.IsNullOrEmpty(AccountName) || String.IsNullOrEmpty(Password)) && !cancelled)
            {
                CredentialsDialog credentialsDialog = new CredentialsDialog();
                credentialsDialog.UserName = scheduledResource.UserName;
                credentialsDialog.Options = CredentialsDialogOptions.IncorrectPassword | CredentialsDialogOptions.ExpectConfirmation;
                credentialsDialog.ValidatePassword = true;
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

        #region Implementation of IHandle<ServerSelectionChangedMessage>

        public void Handle(ServerSelectionChangedMessage message)
        {
            if(message != null)
            {
                if(message.SelectedServer == null)
                {
                    // ReSharper disable NotResolvedInText
                    throw new ArgumentNullException("message.SelectedServer");
                    // ReSharper restore NotResolvedInText
                }

                if(message.ConnectControlInstanceType == ConnectControlInstanceType.Scheduler)
                {
                    OnServerChanged(message.SelectedServer);
                }
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

        #region Implementation of IHandle<SelectedServerConnectedMessage>

        public void Handle(SelectedServerConnectedMessage message)
        {

            if(message != null)
            {
                if(message.SelectedServer == null)
                {
                    // ReSharper disable NotResolvedInText
                    throw new ArgumentNullException("message.SelectedServer");
                    // ReSharper restore NotResolvedInText
                }

                if(Equals(message.SelectedServer, CurrentEnvironment))
                {
                    OnServerChanged(message.SelectedServer);
                }
            }
        }

        #endregion
    }
}

