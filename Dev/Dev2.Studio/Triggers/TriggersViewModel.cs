/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Enums;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Common.Interfaces.Threading;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Services.Events;
using Dev2.Studio.Controller;
using Dev2.Studio.Core;
using Dev2.Studio.Interfaces;
using Dev2.Studio.ViewModels.WorkSurface;
using Dev2.Triggers.QueueEvents;
using Dev2.Triggers.Scheduler;
using Dev2.Threading;
using System.Linq;

namespace Dev2.Triggers
{
    public class TriggersViewModel : BaseWorkSurfaceViewModel, IStudioTab
    {
        string _displayName;
        bool _isDirty;
        bool _hasErrors;
        string _errors;
        bool _isSaved;
        bool _isLoading;
        bool _isSchedulerSelected;
        bool _isEventsSelected;
        QueueEventsViewModel _queueEventsViewModel;
        SchedulerViewModel _schedulerViewModel;

        readonly IPopupController _popupController;
        readonly IAsyncWorker _asyncWorker;
        private ICommand _newQueueEventCommand;
        private ICommand _newScheduleCommand;
        Func<IServer, IServer> _toEnvironmentModel;
        IServer _currentEnvironment;

        [ExcludeFromCodeCoverage]
        public TriggersViewModel()
            : this(EventPublishers.Aggregator, new PopupController(), new AsyncWorker(), CustomContainer.Get<IShellViewModel>().ActiveServer, null)
        {
        }

        public TriggersViewModel(IEventAggregator eventPublisher, IPopupController popupController, IAsyncWorker asyncWorker, IServer server, Func<IServer, IServer> toEnvironmentModel)
            : base(eventPublisher)
        {
            Server = server;
            Server.NetworkStateChanged += ServerNetworkStateChanged;
            VerifyArgument.IsNotNull(nameof(popupController), popupController);
            _popupController = popupController;
            VerifyArgument.IsNotNull(nameof(asyncWorker), asyncWorker);
            _asyncWorker = asyncWorker;

            SaveCommand = new RelayCommand(o => SaveTriggers(), o =>
            {
                return IsSaveEnabled();
            });

            ToEnvironmentModel = toEnvironmentModel ?? (a => a.ToEnvironmentModel());
            CurrentEnvironment = ToEnvironmentModel?.Invoke(server);
            DisplayName = StringResources.TriggersHeader + " - " + Server.DisplayName;
            LoadTasks();
        }

        private bool IsSaveEnabled()
        {
            if (QueueEventsViewModel?.SelectedQueue != null)
            {
                return QueueEventsViewModel.SelectedQueue.IsDirty;
            }
            return false;
        }

        public string ResourceType => StringResources.TriggersHeader;

        public string QueueEventsHeader
        {
            get
            {
                var isDirty = QueueEventsViewModel != null && QueueEventsViewModel.IsDirty;
                isDirty |= IsQueuesDirty();

                var displayName = isDirty ? StringResources.QueueEventsHeader + " *" : StringResources.QueueEventsHeader;
                return displayName;
            }
        }

        public string SchedulerHeader => SchedulerViewModel != null && SchedulerViewModel.IsDirty ? StringResources.SchedulerHeader + " *" : StringResources.SchedulerHeader;

        public IServer Server { get; set; }

        public override string DisplayName
        {
            get => _displayName;
            set
            {
                _displayName = value;
                NotifyOfPropertyChange(() => DisplayName);
            }
        }

        public QueueEventsViewModel QueueEventsViewModel
        {
            get => _queueEventsViewModel;
            private set
            {
                if (Equals(value, _queueEventsViewModel))
                {
                    return;
                }
                _queueEventsViewModel = value;
                NotifyOfPropertyChange(() => QueueEventsViewModel);
            }
        }

        public SchedulerViewModel SchedulerViewModel
        {
            get => _schedulerViewModel;
            private set
            {
                if (Equals(value, _schedulerViewModel))
                {
                    return;
                }
                _schedulerViewModel = value;
                NotifyOfPropertyChange(() => SchedulerViewModel);
            }
        }
        public RelayCommand SaveCommand { get; private set; }

        public ICommand NewQueueEventCommand => _newQueueEventCommand ??
                       (_newQueueEventCommand = new DelegateCommand(CreateNewQueueEvent));

        private void CreateNewQueueEvent(object obj)
        {
            QueueEventsViewModel.NewCommand.Execute(obj);
        }

        public ICommand NewScheduleCommand => _newScheduleCommand ??
                       (_newScheduleCommand = new DelegateCommand(CreateSchedule));

        private void CreateSchedule(object obj)
        {
            SchedulerViewModel.NewCommand.Execute(obj);
        }

        public Func<IServer, IServer> ToEnvironmentModel
        {
            get => _toEnvironmentModel ?? (a => a.ToEnvironmentModel());
            set => _toEnvironmentModel = value;
        }

        public IServer CurrentEnvironment
        {
            get => _currentEnvironment;
            set
            {
                _currentEnvironment = value;
                if (_currentEnvironment != null && _currentEnvironment.IsConnected)
                {
                    _currentEnvironment.AuthorizationService?.IsAuthorized(AuthorizationContext.Administrator, null);
                }

            }
        }

        public bool IsDirty
        {
            get => _isDirty;
            set
            {
                if (value.Equals(_isDirty))
                {
                    return;
                }
                _isDirty = value;
                NotifyOfPropertyChange(() => IsDirty);
                NotifyOfPropertyChange(() => IsSavedSuccessVisible);
                NotifyOfPropertyChange(() => IsErrorsVisible);
                SetDisplayName();
                SaveCommand.CanExecute(IsDirty);
                SaveCommand.RaiseCanExecuteChanged();
            }
        }

        public bool IsSavedSuccessVisible => !HasErrors && !IsDirty && IsSaved;

        public bool IsErrorsVisible => HasErrors || IsDirty && !IsSaved;

        public bool HasErrors
        {
            get => _hasErrors;
            set
            {
                if (value.Equals(_hasErrors))
                {
                    return;
                }
                _hasErrors = value;
                NotifyOfPropertyChange(() => HasErrors);
                NotifyOfPropertyChange(() => IsSavedSuccessVisible);
                NotifyOfPropertyChange(() => IsErrorsVisible);
            }
        }

        public string Errors
        {
            get => _errors;
            set
            {
                if (value == _errors)
                {
                    return;
                }
                _errors = value;
                NotifyOfPropertyChange(() => Errors);
            }
        }

        public bool IsSaved
        {
            get => _isSaved;
            set
            {
                if (value.Equals(_isSaved))
                {
                    return;
                }
                _isSaved = value;
                NotifyOfPropertyChange(() => IsSaved);
                NotifyOfPropertyChange(() => IsSavedSuccessVisible);
                NotifyOfPropertyChange(() => IsErrorsVisible);
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (value.Equals(_isLoading))
                {
                    return;
                }
                _isLoading = value;
                NotifyOfPropertyChange(() => IsLoading);
            }
        }

        public bool IsSchedulerSelected
        {
            get => _isSchedulerSelected;
            set
            {
                if (value.Equals(_isSchedulerSelected))
                {
                    return;
                }
                if (value)
                {
                    IsEventsSelected = false;
                }
                _isSchedulerSelected = value;
                NotifyOfPropertyChange(() => IsSchedulerSelected);
            }
        }

        public bool IsEventsSelected
        {
            get => _isEventsSelected;
            set
            {
                
                if (value.Equals(_isEventsSelected))
                {
                    return;
                }
                if (value)
                {
                    IsSchedulerSelected = false;
                }
                _isEventsSelected = value;
                NotifyOfPropertyChange(() => IsEventsSelected);
            }
        }

        void SetDisplayName()
        {
            var isDirty = IsQueuesDirty();

            if (IsDirty || isDirty)
            {
                if (!DisplayName.EndsWith(" *"))
                {
                    DisplayName += " *";
                }
            }
            else
            {
                DisplayName = _displayName.Replace("*", "").TrimEnd(' ');
            }
        }

        private bool IsQueuesDirty()
        {
            return QueueEventsViewModel?.Queues != null && QueueEventsViewModel.Queues.Any(o => o.IsDirty);
        }

        void ServerNetworkStateChanged(INetworkStateChangedEventArgs args, IServer server)
        {
            if (args.State == ConnectionNetworkState.Connected)
            {
                LoadTasks();
            }
            if (args.State == ConnectionNetworkState.Disconnected)
            {
                LoadTasks();
            }
        }

        void LoadTasks()
        {
            ClearErrors();
            IsSaved = false;
            IsDirty = false;
            IsLoading = true;

            _asyncWorker.Start(() =>
            {
                

            }, () =>
            {
                QueueEventsViewModel = CreateQueueEventsViewModel();
                IsLoading = false;
                AddPropertyChangedHandlers();
            });
        }

        protected void ClearErrors()
        {
            HasErrors = false;
            Errors = null;
        }

        void AddPropertyChangedHandlers()
        {
            var isDirtyProperty = DependencyPropertyDescriptor.FromProperty(TasksItemViewModel.IsDirtyProperty, typeof(TasksItemViewModel));
            if (QueueEventsViewModel != null)
            {
                isDirtyProperty.AddValueChanged(QueueEventsViewModel, OnIsDirtyPropertyChanged);
                QueueEventsViewModel.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName == nameof(IsDirty))
                    {
                        OnIsDirtyPropertyChanged(null, new EventArgs());
                    }
                };
            }     
        }

        void OnIsDirtyPropertyChanged(object sender, EventArgs eventArgs)
        {
            if (QueueEventsViewModel != null)
            {
                IsDirty = QueueEventsViewModel.IsDirty;
            }
            NotifyOfPropertyChange(() => QueueEventsHeader);
            ClearErrors();
        }

        private QueueEventsViewModel CreateQueueEventsViewModel()
        {
            var queueEventsViewModel = new QueueEventsViewModel(CurrentEnvironment);
            return queueEventsViewModel;
        }

        public bool DoDeactivate(bool showMessage)
        {
            if (showMessage)
            {
                var messageBoxResult = GetSaveResult();
                if (messageBoxResult == MessageBoxResult.Cancel || messageBoxResult == MessageBoxResult.None)
                {
                    return false;
                }
                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    return SaveTriggers();
                }

                if (messageBoxResult == MessageBoxResult.No)
                {
                    IsDirty = false;
                    ResetIsDirtyForChildren();
                }
            }
            else
            {
                return SaveTriggers();
            }

            return true;
        }

        bool SaveTriggers()
        {
            if (CurrentEnvironment.IsConnected)
            {
                //TODO: Is this a valid check for events?
                if (CurrentEnvironment.AuthorizationService.IsAuthorized(AuthorizationContext.Administrator, null))
                {
                    // Need to reset sub view models so that selecting something in them fires our OnIsDirtyPropertyChanged()
                    ClearErrors();
                    IsSaved = QueueEventsViewModel.Save();
                    if (!IsSaved)
                    {
                        IsSaved = false;
                        IsDirty = true;
                    }
                    return IsSaved;
                }
                ShowError(StringResources.SaveErrorPrefix, StringResources.SaveSettingsPermissionsErrorMsg);
                _popupController.ShowSaveSettingsPermissionsErrorMsg();
                return false;
            }
            ShowError(StringResources.SaveErrorPrefix, StringResources.SaveServerNotReachableErrorMsg);
            _popupController.ShowSaveServerNotReachableErrorMsg();
            return false;
        }

        void ResetIsDirtyForChildren()
        {
            if (SchedulerViewModel != null)
            {
                SchedulerViewModel.IsDirty = false;
                NotifyOfPropertyChange(() => SchedulerHeader);
            }
            if (QueueEventsViewModel != null)
            {
                QueueEventsViewModel.IsDirty = false;
                NotifyOfPropertyChange(() => QueueEventsHeader);
            }        
        }

        protected virtual void ShowError(string header, string description)
        {
            HasErrors = true;
            Errors = description;
        }

        MessageBoxResult GetSaveResult()
        {
            if (_popupController != null && IsDirty)
            {
                return _popupController.ShowTasksCloseConfirmation();
            }
            return !IsDirty ? MessageBoxResult.No : MessageBoxResult.None;
        }

        public void CloseView()
        {
            
        }
    }
}
