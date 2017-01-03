using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using Dev2.Common;
using Dev2.Common.Interfaces.Scheduler.Interfaces;
using Dev2.Communication;
using Dev2.Dialogs;
using Dev2.Scheduler;
using Dev2.Services.Security;
using Dev2.Studio.Enums;
using Dev2.TaskScheduler.Wrappers;
using Microsoft.Win32.TaskScheduler;
using Warewolf.Studio.Resources.Languages;
using Warewolf.Studio.ViewModels;

namespace Dev2.Settings.Scheduler
{
    internal class SchedulerTaskManager
    {
        private readonly SchedulerViewModel _schedulerViewModel;
        private int _newTaskCounter = 1;
        private IResourcePickerDialog _currentResourcePicker;
        private TriggerEditDialog _triggerEditDialog;
        readonly Dev2JsonSerializer _ser = new Dev2JsonSerializer();
        private readonly Task<IResourcePickerDialog> _resourcePickerDialogTask;
        private EnvironmentViewModel _source;

        internal SchedulerTaskManager(SchedulerViewModel schedulerViewModel, Task<IResourcePickerDialog> getResourcePicker)
        {
            _schedulerViewModel = schedulerViewModel;
            _resourcePickerDialogTask = getResourcePicker;
            var taskServiceConvertorFactory = new TaskServiceConvertorFactory();
            SchedulerFactory = new ClientSchedulerFactory(new Dev2TaskService(taskServiceConvertorFactory), taskServiceConvertorFactory);
        }

        private IResourcePickerDialog CreateResourcePickerDialog()
        {
            var res = new ResourcePickerDialog(enDsfActivityType.All, _source);
            ResourcePickerDialog.CreateAsync(enDsfActivityType.Workflow, _source).ContinueWith(a => _currentResourcePicker = a.Result);
            return res;
        }

        private Task<IResourcePickerDialog> GetResourcePickerDialog => _resourcePickerDialogTask ?? ResourcePickerDialog.CreateAsync(enDsfActivityType.Workflow, _source);

        public IResourcePickerDialog CurrentResourcePickerDialog
        {
            private get
            {
                return _currentResourcePicker ?? CreateResourcePickerDialog();
            }
            set
            {
                _currentResourcePicker = value;
                _schedulerViewModel.NotifyOfPropertyChange(() => CurrentResourcePickerDialog);
            }
        }
        protected IClientSchedulerFactory SchedulerFactory { get; }
        public TriggerEditDialog TriggerEditDialog
        {
            get
            {
                return _triggerEditDialog;
            }
            private set
            {
                _triggerEditDialog = value;
                _schedulerViewModel.NotifyOfPropertyChange(() => TriggerEditDialog);
            }
        }
        public EnvironmentViewModel Source
        {
            set
            {
                _source = value;
            }
        }

        [ExcludeFromCodeCoverage]
        protected virtual IScheduleTrigger ShowEditTriggerDialog()
        {
            var tmpTrigger = _schedulerViewModel.SelectedTask.Trigger.Trigger.Instance;
            if (TriggerEditDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (!TriggerEquals(TriggerEditDialog.Trigger, tmpTrigger))
                {
                    return SchedulerFactory.CreateTrigger(TaskState.Disabled, new Dev2Trigger(null, TriggerEditDialog.Trigger));
                }
            }
            return null;
        }

        [ExcludeFromCodeCoverage]
        protected virtual void GetCredentials(IScheduledResource scheduledResource)
        {
            var cancelled = false;
            while ((String.IsNullOrEmpty(_schedulerViewModel.AccountName) || String.IsNullOrEmpty(_schedulerViewModel.Password)) && !cancelled)
            {
                NetworkCredential cred;
                CredentialUI.GetCredentialsVistaAndUp(scheduledResource.Name, out cred);
                if (cred == null)
                {
                    cancelled = true;
                }
                else
                {
                    _schedulerViewModel.AccountName = cred.UserName;
                    _schedulerViewModel.Password = cred.Password;
                }
            }
        }

        public bool CanSelectWorkflow()
        {
            return CurrentResourcePickerDialog != null;
        }

        public bool SaveTasks()
        {
            if (_schedulerViewModel.CurrentEnvironment.IsConnected)
            {
                var authService = _schedulerViewModel.CurrentEnvironment.AuthorizationService;

                if (authService != null && authService.IsAuthorized(AuthorizationContext.Administrator, null))
                {
                    if (!ValidateSelectedTask())
                        return false;

                    GetCredentials(_schedulerViewModel.SelectedTask);
                    string errorMessage;
                    if (!_schedulerViewModel.ScheduledResourceModel.Save(_schedulerViewModel.SelectedTask, out errorMessage))
                    {
                        _schedulerViewModel.ShowSaveErrorDialog(errorMessage);
                        _schedulerViewModel.ShowError(errorMessage);
                        return false;
                    }
                    Dev2Logger.Info($"Save Schedule. Environment: {_schedulerViewModel.CurrentEnvironment.Name} Name:{(_schedulerViewModel.SelectedTask != null ? _schedulerViewModel.SelectedTask.Name : string.Empty)} ");
                    if (_schedulerViewModel.SelectedTask != null)
                    {
                        _schedulerViewModel.SelectedTask.Errors.ClearErrors();
                        _schedulerViewModel.NotifyOfPropertyChange(() => _schedulerViewModel.Error);
                        _schedulerViewModel.NotifyOfPropertyChange(() => _schedulerViewModel.Errors);
                        _schedulerViewModel.SelectedTask.OldName = _schedulerViewModel.SelectedTask.Name;
                        _schedulerViewModel.SelectedTask.IsNew = false;
                        _schedulerViewModel.Item = _ser.Deserialize<IScheduledResource>(_ser.SerializeToBuilder(_schedulerViewModel.SelectedTask));
                        _schedulerViewModel.NotifyOfPropertyChange(() => _schedulerViewModel.IsDirty);

                    }
                    _schedulerViewModel.NotifyOfPropertyChange(() => _schedulerViewModel.TaskList);
                }
                else
                {
                    _schedulerViewModel.ShowError(@"Error while saving: You don't have permission to schedule on this server. You need Administrator permission.");
                    return false;
                }
                return true;
            }
            _schedulerViewModel.ShowError(Core.SchedulerNotConnectedErrorMessage);
            return false;
        }

        private bool ValidateSelectedTask()
        {
            if(!_schedulerViewModel.SelectedTask.IsDirty) return true;
            if (_schedulerViewModel.HasErrors && !_schedulerViewModel.Error.StartsWith(Core.SchedulerSaveErrorPrefix))
            {
                _schedulerViewModel.ShowSaveErrorDialog(_schedulerViewModel.Error);
                return false;
            }

            var oldName = _schedulerViewModel.SelectedTask?.OldName;
            if(oldName == null) return true;
            if (oldName != _schedulerViewModel.SelectedTask.Name && !oldName.Contains(Core.SchedulerNewTaskName) && !_schedulerViewModel.SelectedTask.IsNew)
            {
                var showNameChangedConflict = _schedulerViewModel.PopupController.ShowNameChangedConflict(oldName, _schedulerViewModel.SelectedTask.Name);
                if (showNameChangedConflict == MessageBoxResult.Cancel || showNameChangedConflict == MessageBoxResult.None)
                {
                    return false;
                }
                if (showNameChangedConflict == MessageBoxResult.No)
                {
                    _schedulerViewModel.SelectedTask.Name = oldName;
                    _schedulerViewModel.NotifyOfPropertyChange(() => _schedulerViewModel.Name);
                }
            }
            if (_schedulerViewModel.SelectedTask.OldName != _schedulerViewModel.SelectedTask.Name && _schedulerViewModel.SelectedTask.OldName.Contains(Core.SchedulerNewTaskName))
            {
                _schedulerViewModel.SelectedTask.OldName = _schedulerViewModel.SelectedTask.Name;
            }
            return true;
        }

        private void ShowServerDisconnectedPopup()
        {
            _schedulerViewModel.PopupController?.Show(string.Format(Core.ServerDisconnected, _schedulerViewModel.CurrentEnvironment.Connection.DisplayName.Replace("(Connected)", "")) + Environment.NewLine +
                             Core.ServerReconnectForActions, Core.ServerDisconnectedHeader, MessageBoxButton.OK,
                MessageBoxImage.Error, "", false, true, false, false, false, false);
        }

        public void CreateNewTask()
        {
            if (_schedulerViewModel.CurrentEnvironment?.Connection != null && !_schedulerViewModel.CurrentEnvironment.Connection.IsConnected)
            {
                ShowServerDisconnectedPopup();
                return;
            }

            if (_schedulerViewModel.IsDirty)
            {
                _schedulerViewModel.PopupController.Show(Core.SchedulerUnsavedTaskMessage, Core.SchedulerUnsavedTaskHeader, MessageBoxButton.OK, MessageBoxImage.Error, null, false, true, false, false, false, false);
                return;
            }

            var dev2DailyTrigger = new Dev2DailyTrigger(new TaskServiceConvertorFactory(), new DailyTrigger());
            var scheduleTrigger = SchedulerFactory.CreateTrigger(TaskState.Ready, dev2DailyTrigger);
            ScheduledResource scheduledResource = new ScheduledResource(Core.SchedulerNewTaskName + _newTaskCounter, SchedulerStatus.Enabled, scheduleTrigger.Trigger.Instance.StartBoundary, scheduleTrigger, string.Empty, Guid.NewGuid().ToString());
            scheduledResource.OldName = scheduledResource.Name;
            var newres = _schedulerViewModel.ScheduledResourceModel.ScheduledResources[_schedulerViewModel.ScheduledResourceModel.ScheduledResources.Count == 1 ? 0 : _schedulerViewModel.ScheduledResourceModel.ScheduledResources.Count - 1];
            _schedulerViewModel.ScheduledResourceModel.ScheduledResources[_schedulerViewModel.ScheduledResourceModel.ScheduledResources.Count == 1 ? 0 : _schedulerViewModel.ScheduledResourceModel.ScheduledResources.Count - 1] = scheduledResource;
            _schedulerViewModel.ScheduledResourceModel.ScheduledResources.Add(newres);

            _newTaskCounter++;

            _schedulerViewModel.NotifyOfPropertyChange(() => _schedulerViewModel.TaskList);
            _schedulerViewModel.SelectedTask = _schedulerViewModel.ScheduledResourceModel.ScheduledResources[_schedulerViewModel.ScheduledResourceModel.ScheduledResources.Count == 1 ? 1 : _schedulerViewModel.ScheduledResourceModel.ScheduledResources.Count - 2];
            _schedulerViewModel.WorkflowName = string.Empty;
            _schedulerViewModel.SelectedTask.IsNew = true;
            ViewModelUtils.RaiseCanExecuteChanged(_schedulerViewModel.NewCommand);
        }

        public void DeleteTask()
        {
            if (_schedulerViewModel.SelectedTask != null && _schedulerViewModel.CurrentEnvironment != null)
            {
                if (_schedulerViewModel.CurrentEnvironment.IsConnected)
                {
                    if (_schedulerViewModel.CurrentEnvironment.AuthorizationService.IsAuthorized(AuthorizationContext.Administrator, null))
                    {
                        if (_schedulerViewModel.PopupController.ShowDeleteConfirmation(_schedulerViewModel.SelectedTask.Name) == MessageBoxResult.Yes)
                        {
                            int index = _schedulerViewModel.ScheduledResourceModel.ScheduledResources.IndexOf(_schedulerViewModel.SelectedTask);
                            int indexInFilteredList = _schedulerViewModel.TaskList.IndexOf(_schedulerViewModel.SelectedTask);
                            if (index != -1)
                            {
                                Dev2Logger.Info($"Delete Schedule Name: {_schedulerViewModel.SelectedTask.Name} Resource:{_schedulerViewModel.SelectedTask.ResourceId} Env:{_schedulerViewModel.CurrentEnvironment.Name}");

                                _schedulerViewModel.ScheduledResourceModel.DeleteSchedule(_schedulerViewModel.SelectedTask);
                                //if delete is successfull then do the code below
                                _schedulerViewModel.ScheduledResourceModel.ScheduledResources.RemoveAt(index);
                                _schedulerViewModel.NotifyOfPropertyChange(() => _schedulerViewModel.TaskList);
                                if (indexInFilteredList <= _schedulerViewModel.TaskList.Count && indexInFilteredList > 0)
                                {
                                    _schedulerViewModel.SelectedTask = _schedulerViewModel.TaskList[indexInFilteredList - 1];
                                }
                                else if (indexInFilteredList == 0 && _schedulerViewModel.TaskList.Count > 0)
                                {
                                    _schedulerViewModel.SelectedTask = _schedulerViewModel.TaskList[0];
                                }
                            }
                            _schedulerViewModel.NotifyOfPropertyChange(() => _schedulerViewModel.History);
                        }
                    }
                    else
                    {
                        _schedulerViewModel.ShowError(@"Error while saving: You don't have permission to schedule on this server. You need Administrator permission.");
                    }
                }
                else
                {
                    _schedulerViewModel.ShowError(Core.SchedulerNotConnectedErrorMessage);
                }
            }
        }

        public void EditTrigger()
        {
            if (_schedulerViewModel.SelectedTask != null)
            {
                TriggerEditDialog = new TriggerEditDialog(_schedulerViewModel.SelectedTask.Trigger.Trigger.Instance, false);
                var tempTrigger = ShowEditTriggerDialog();
                if (tempTrigger != null)
                {
                    _schedulerViewModel.Trigger = tempTrigger;
                    _schedulerViewModel.SelectedTask.NextRunDate = _schedulerViewModel.Trigger.Trigger.StartBoundary;
                }
                _schedulerViewModel.NotifyOfPropertyChange(() => _schedulerViewModel.TriggerText);
            }
        }

        public void AddWorkflow()
        {
            if (_schedulerViewModel.SelectedTask != null && _schedulerViewModel.CurrentEnvironment != null)
            {
                if (CurrentResourcePickerDialog == null)
                {
                    if (GetResourcePickerDialog?.Status == TaskStatus.Running)
                    {
                        GetResourcePickerDialog.Wait();
                        if (!GetResourcePickerDialog.IsFaulted)
                            CurrentResourcePickerDialog = GetResourcePickerDialog.Result;
                    }
                }

                if (!string.IsNullOrEmpty(_schedulerViewModel.WorkflowName) && _schedulerViewModel.CurrentEnvironment.ResourceRepository != null)
                {
                    var resourceModel = _schedulerViewModel.CurrentEnvironment.ResourceRepository.FindSingle(c => c.ResourceName == _schedulerViewModel.WorkflowName);
                    if (resourceModel != null)
                    {
                        CurrentResourcePickerDialog?.SelectResource(resourceModel.ID);
                    }
                }
                var hasResult = CurrentResourcePickerDialog != null && CurrentResourcePickerDialog.ShowDialog(_schedulerViewModel.CurrentEnvironment);
                if (hasResult)
                {
                    var resourcePath = CurrentResourcePickerDialog.SelectedResource.ResourcePath;
                    var resourceId = CurrentResourcePickerDialog.SelectedResource.ResourceId;
                    var resourceName = CurrentResourcePickerDialog.SelectedResource.ResourceName;
                    UpdateScheduleWithResourceDetails(resourcePath, resourceId, resourceName);
                }
            }
        }

        public void UpdateScheduleWithResourceDetails(string resourcePath, Guid resourceId, string resourceName)
        {
            _schedulerViewModel.WorkflowName = resourcePath;

            _schedulerViewModel.SelectedTask.ResourceId = resourceId;
            if (_schedulerViewModel.SelectedTask.Name.StartsWith(@"New Task"))
            {
                _schedulerViewModel.Name = resourceName;
                _schedulerViewModel.NotifyOfPropertyChange(() => _schedulerViewModel.Name);
            }
            _schedulerViewModel.NotifyOfPropertyChange(() => _schedulerViewModel.WorkflowName);
            _schedulerViewModel.NotifyOfPropertyChange(() => _schedulerViewModel.TaskList);
        }

        public static bool TriggerEquals(Microsoft.Win32.TaskScheduler.Trigger a, Microsoft.Win32.TaskScheduler.Trigger b)
        {
            return a.ToString() == b.ToString() && a.StartBoundary == b.StartBoundary && a.EndBoundary == b.EndBoundary && a.ExecutionTimeLimit == b.ExecutionTimeLimit;
        }
    }
}