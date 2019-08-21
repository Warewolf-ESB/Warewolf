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

            SaveCommand = new RelayCommand(o => SaveTriggers(), o => IsDirty);

            ToEnvironmentModel = toEnvironmentModel ?? (a => a.ToEnvironmentModel());
            CurrentEnvironment = ToEnvironmentModel?.Invoke(server);
            LoadTasks();
            DisplayName = StringResources.TriggersHeader + " - " + Server.DisplayName;
        }

        public string ResourceType => StringResources.TriggersHeader;

        public string QueueEventsHeader => QueueEventsViewModel != null && QueueEventsViewModel.IsDirty ? StringResources.QueueEventsHeader + " *" : StringResources.QueueEventsHeader;

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
                if (CurrentEnvironment.IsConnected)
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

        void SetDisplayName()
        {
            if (IsDirty)
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
                IsLoading = false;
                SchedulerViewModel = CreateSchedulerViewModel();
                QueueEventsViewModel = CreateQueueEventsViewModel();

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
            //TODO: I will come back to this, want to get the rest through code review
            //if (SchedulerViewModel != null)
            //{
            //    isDirtyProperty.AddValueChanged(SchedulerViewModel, OnIsDirtyPropertyChanged);
            //    SchedulerViewModel.PropertyChanged += (sender, args) =>
            //    {
            //        if (args.PropertyName == nameof(IsDirty))
            //        {
            //            OnIsDirtyPropertyChanged(null, new EventArgs());
            //        }
            //    };
            //}
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
            if (SchedulerViewModel != null)
            {
                IsDirty = SchedulerViewModel.IsDirty;
            }
            if (QueueEventsViewModel != null)
            {
                IsDirty = QueueEventsViewModel.IsDirty;
            }
            NotifyOfPropertyChange(() => QueueEventsHeader);
            NotifyOfPropertyChange(() => SchedulerHeader);
            ClearErrors();
        }

        private QueueEventsViewModel CreateQueueEventsViewModel()
        {
            var queueEventsViewModel = new QueueEventsViewModel(CurrentEnvironment);
            return queueEventsViewModel;
        }

        private static SchedulerViewModel CreateSchedulerViewModel()
        {
            var schedulerViewModel = new SchedulerViewModel();
            return schedulerViewModel;
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
                    //TODO: Call Scheduler Save
                    IsDirty = false;
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
