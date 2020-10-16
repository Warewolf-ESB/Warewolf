/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Forms;
using Caliburn.Micro;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Triggers.Scheduler;
using Dev2.Common.Interfaces.Enums;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Common.Interfaces.Threading;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Services.Events;
using Dev2.Services.Security;
using Dev2.Settings.Logging;
using Dev2.Settings.Perfcounters;
using Dev2.Settings.Persistence;
using Dev2.Settings.Security;
using Dev2.Studio.Controller;
using Dev2.Studio.ViewModels.WorkSurface;
using Dev2.Threading;
using Dev2.Studio.Core;
using Dev2.Studio.Interfaces;

namespace Dev2.Settings
{
    public class SettingsViewModel : BaseWorkSurfaceViewModel, IStudioTab
    {
        bool _isLoading;
        bool _isDirty;
        bool _selectionChanging;
        bool _hasErrors;
        string _errors;
        bool _isSaved;

        bool _showLogging;
        bool _showSecurity = true;

        readonly IPopupController _popupController;
        readonly IAsyncWorker _asyncWorker;
        readonly IWin32Window _parentWindow;

        SecurityViewModel _securityViewModel;
        LogSettingsViewModel _logSettingsViewModel;
        PersistenceSettingsViewModel _persistenceSettingsViewModel;
        IServer _currentEnvironment;
        Func<IServer, IServer> _toEnvironmentModel;
        PerfcounterViewModel _perfmonViewModel;
        string _displayName;
        private bool _showPersistence;


        // ReSharper disable once MemberCanBeProtected.Global
        public SettingsViewModel()
            : this(EventPublishers.Aggregator, new PopupController(), new AsyncWorker(), (IWin32Window) System.Windows.Application.Current.MainWindow, CustomContainer.Get<IShellViewModel>().ActiveServer, null)
        {
        }

        public SettingsViewModel(IEventAggregator eventPublisher, IPopupController popupController, IAsyncWorker asyncWorker, IWin32Window parentWindow, IServer server, Func<IServer, IServer> toEnvironmentModel)
            : base(eventPublisher)
        {
            Server = server;
            Server.NetworkStateChanged += ServerNetworkStateChanged;
            Settings = new Data.Settings.Settings();
            VerifyArgument.IsNotNull(nameof(popupController), popupController);
            _popupController = popupController;
            VerifyArgument.IsNotNull(nameof(asyncWorker), asyncWorker);
            _asyncWorker = asyncWorker;
            VerifyArgument.IsNotNull(nameof(parentWindow), parentWindow);
            _parentWindow = parentWindow;

            SaveCommand = new RelayCommand(o => SaveSettings(), o => IsDirty);

            ToEnvironmentModel = toEnvironmentModel ?? (a => a.ToEnvironmentModel());
            CurrentEnvironment = ToEnvironmentModel?.Invoke(server);

            LoadSettings();
            // ReSharper disable once VirtualMemberCallInContructor
            DisplayName = StringResources.SettingsTitle + " - " + Server.DisplayName;
        }

        protected override void OnDispose()
        {
            Server.NetworkStateChanged -= ServerNetworkStateChanged;
            base.OnDispose();
        }

        public override string DisplayName
        {
            get => _displayName;
            set
            {
                _displayName = value;
                NotifyOfPropertyChange(() => DisplayName);
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
                LoadSettings();
            }

            if (args.State == ConnectionNetworkState.Disconnected)
            {
                LoadSettings();
            }
        }

        public IServer Server { get; set; }

        public RelayCommand SaveCommand { get; private set; }

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

        public void CloseView()
        {
            Server.NetworkStateChanged -= ServerNetworkStateChanged;
            Server = null;
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

        public bool ShowLogging
        {
            get => _showLogging;
            set
            {
                if (value.Equals(_showLogging))
                {
                    return;
                }

                _showLogging = value;
                OnSelectionChanged();
                NotifyOfPropertyChange(() => ShowLogging);
            }
        }

        public bool ShowSecurity
        {
            get => _showSecurity;
            set
            {
                if (value.Equals(_showSecurity))
                {
                    return;
                }

                _showSecurity = value;
                OnSelectionChanged();
                NotifyOfPropertyChange(() => ShowSecurity);
            }
        }

        public bool ShowPersistence
        {
            get => _showPersistence;
            set
            {
                if (value.Equals(_showPersistence))
                {
                    return;
                }

                _showPersistence = value;
                OnSelectionChanged();
                NotifyOfPropertyChange(() => ShowPersistence);
            }
        }

        public Data.Settings.Settings Settings { get; private set; }

        public SecurityViewModel SecurityViewModel
        {
            get => _securityViewModel;
            private set
            {
                if (Equals(value, _securityViewModel))
                {
                    return;
                }

                _securityViewModel = value;
                NotifyOfPropertyChange(() => SecurityViewModel);
            }
        }

        public LogSettingsViewModel LogSettingsViewModel
        {
            get => _logSettingsViewModel;
            private set
            {
                if (Equals(value, _logSettingsViewModel))
                {
                    return;
                }

                _logSettingsViewModel = value;
                NotifyOfPropertyChange(() => LogSettingsViewModel);
                NotifyOfPropertyChange(() => HasLogSettings);
            }
        }

        public PersistenceSettingsViewModel PersistenceSettingsViewModel
        {
            get => _persistenceSettingsViewModel;
            private set
            {
                if (Equals(value, _persistenceSettingsViewModel))
                {
                    return;
                }

                _persistenceSettingsViewModel = value;
                NotifyOfPropertyChange(() => PersistenceSettingsViewModel);
            }
        }

        public string SecurityHeader => SecurityViewModel != null && SecurityViewModel.IsDirty ? StringResources.SettingsSecurity + " *" : StringResources.SettingsSecurity;

        public string LogHeader => LogSettingsViewModel != null && LogSettingsViewModel.IsDirty ? StringResources.SettingsLogging + " *" : StringResources.SettingsLogging;
        public string PersistenceHeader => _persistenceSettingsViewModel != null && _persistenceSettingsViewModel.IsDirty ? StringResources.SettingsPersistence + " *" : StringResources.SettingsPersistence;

        public bool HasLogSettings
        {
            get
            {
                var hasLogSettings = LogSettingsViewModel != null && CurrentEnvironment.IsConnected;
                if (!hasLogSettings)
                {
                    ShowSecurity = true;
                }

                return hasLogSettings;
            }
        }

        void OnSelectionChanged([CallerMemberName] string propertyName = null)
        {
            if (_selectionChanging)
            {
                return;
            }

            _selectionChanging = true;
            switch (propertyName)
            {
                case nameof(ShowLogging):
                    ShowLogging = Settings?.Logging != null;
                    ShowSecurity = !ShowLogging;
                    ShowPersistence= !ShowLogging;
                    break;

                case nameof(ShowSecurity):
                    ShowSecurity = true;
                    ShowLogging = !ShowSecurity;
                    ShowPersistence= !ShowSecurity;
                    break;
                case nameof(ShowPersistence):
                    ShowPersistence =  Settings?.Persistence != null;
                    ShowSecurity =!ShowPersistence;
                    ShowLogging = !ShowPersistence;
                    break;
                default:
                    break;
            }

            _selectionChanging = false;
        }

        public override bool HasVariables => false;
        public override bool HasDebugOutput => false;

        public Func<IServer, IServer> ToEnvironmentModel
        {
            get { return _toEnvironmentModel ?? (a => a.ToEnvironmentModel()); }
            set { _toEnvironmentModel = value; }
        }


        void LoadSettings()
        {
            ClearErrors();
            IsSaved = false;
            IsDirty = false;
            IsLoading = true;

            _asyncWorker.Start(() => { Settings = CurrentEnvironment.IsConnected ? ReadSettings() : new Data.Settings.Settings {Security = new SecuritySettingsTO()}; }, () =>
            {
                IsLoading = false;
                SecurityViewModel = CreateSecurityViewModel();
                LogSettingsViewModel = CreateLoggingViewModel();
                PerfmonViewModel = CreatePerfmonViewModel();
                PersistenceSettingsViewModel = CreatePersistenceViewModel();
                AddPropertyChangedHandlers();

                if (Settings.HasError)
                {
                    ShowError("Load Error", Settings.Error);
                }
            });
        }

        public PerfcounterViewModel PerfmonViewModel
        {
            get => _perfmonViewModel;
            set
            {
                _perfmonViewModel = value;
                NotifyOfPropertyChange(() => PerfmonViewModel);
            }
        }

        protected virtual SecurityViewModel CreateSecurityViewModel()
        {
            var securityViewModel = new SecurityViewModel(Settings.Security, _parentWindow, CurrentEnvironment);
            securityViewModel.SetItem(securityViewModel);
            return securityViewModel;
        }

        protected virtual PerfcounterViewModel CreatePerfmonViewModel()
        {
            var perfcounterViewModel = new PerfcounterViewModel(Settings.PerfCounters, CurrentEnvironment);
            return perfcounterViewModel;
        }

        protected virtual LogSettingsViewModel CreateLoggingViewModel()
        {
            if (Settings.Logging != null)
            {
                var logSettingsViewModel = new LogSettingsViewModel(Settings.Logging, CurrentEnvironment);
                logSettingsViewModel.SetItem(logSettingsViewModel);
                return logSettingsViewModel;
            }

            return null;
        }

        protected virtual PersistenceSettingsViewModel CreatePersistenceViewModel()
        {
            if (Settings.Persistence != null)
            {
                var persistenceSettingsViewModel = new PersistenceSettingsViewModel(CurrentEnvironment);
                persistenceSettingsViewModel.SetItem(persistenceSettingsViewModel);
                return persistenceSettingsViewModel;
            }

            return null;
        }

        void AddPropertyChangedHandlers()
        {
            var isDirtyProperty = DependencyPropertyDescriptor.FromProperty(SettingsItemViewModel.IsDirtyProperty, typeof(SettingsItemViewModel));
            if (LogSettingsViewModel != null)
            {
                isDirtyProperty.AddValueChanged(LogSettingsViewModel, OnIsDirtyPropertyChanged);
                LogSettingsViewModel.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName == nameof(IsDirty))
                    {
                        OnIsDirtyPropertyChanged(null, new EventArgs());
                    }
                };
            }

            if (SecurityViewModel != null)
            {
                isDirtyProperty.AddValueChanged(SecurityViewModel, OnIsDirtyPropertyChanged);
                SecurityViewModel.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName == nameof(IsDirty))
                    {
                        OnIsDirtyPropertyChanged(null, new EventArgs());
                    }
                };
            }

            if (PerfmonViewModel != null)
            {
                isDirtyProperty.AddValueChanged(PerfmonViewModel, OnIsDirtyPropertyChanged);
                PerfmonViewModel.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName == nameof(IsDirty))
                    {
                        OnIsDirtyPropertyChanged(null, new EventArgs());
                    }
                };
            }

            if (PersistenceSettingsViewModel != null)
            {
                isDirtyProperty.AddValueChanged(PersistenceSettingsViewModel, OnIsDirtyPropertyChanged);
                PersistenceSettingsViewModel.PropertyChanged += (sender, args) =>
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
            if (SecurityViewModel != null && LogSettingsViewModel != null && PersistenceSettingsViewModel != null)
            {
                IsDirty = SecurityViewModel.IsDirty || LogSettingsViewModel.IsDirty || PerfmonViewModel.IsDirty || PersistenceSettingsViewModel.IsDirty;
            }

            NotifyOfPropertyChange(() => SecurityHeader);
            NotifyOfPropertyChange(() => LogHeader);
            NotifyOfPropertyChange(() => PerfmonHeader);
            NotifyOfPropertyChange(() => PersistenceHeader);
            ClearErrors();
        }

        void ResetIsDirtyForChildren()
        {
            if (SecurityViewModel != null)
            {
                SecurityViewModel.IsDirty = false;
                NotifyOfPropertyChange(() => SecurityHeader);
            }

            if (LogSettingsViewModel != null)
            {
                LogSettingsViewModel.IsDirty = false;
                NotifyOfPropertyChange(() => LogHeader);
            }

            if (PerfmonViewModel != null)
            {
                PerfmonViewModel.IsDirty = false;
                NotifyOfPropertyChange(() => PerfmonHeader);
            }

            if (PersistenceSettingsViewModel != null)
            {
                PersistenceSettingsViewModel.IsDirty = false;
                NotifyOfPropertyChange(() => PersistenceHeader);
            }
        }

        #region Overrides of SimpleBaseViewModel

        #region Overrides of Screen

        public virtual bool DoDeactivate(bool showMessage)
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
                    return SaveSettings();
                }

                if (messageBoxResult == MessageBoxResult.No)
                {
                    IsDirty = false;
                    ResetIsDirtyForChildren();
                }
            }
            else
            {
                return SaveSettings();
            }

            return true;
        }

        MessageBoxResult GetSaveResult()
        {
            if (_popupController != null && IsDirty)
            {
                return _popupController.ShowSettingsCloseConfirmation();
            }

            return !IsDirty ? MessageBoxResult.No : MessageBoxResult.None;
        }

        #endregion

        #endregion

        /// <summary>
        /// Saves the settings.
        /// </summary>
        /// <returns></returns>
        bool SaveSettings()
        {
            if (CurrentEnvironment.IsConnected)
            {
                if (CurrentEnvironment.AuthorizationService.IsAuthorized(AuthorizationContext.Administrator, null))
                {
                    // Need to reset sub view models so that selecting something in them fires our OnIsDirtyPropertyChanged()
                    ClearErrors();
                    if (!ValidateDuplicateResourcePermissions())
                    {
                        return false;
                    }

                    if (!ValidateDuplicateServerPermissions())
                    {
                        return false;
                    }

                    if (!ValidateResourcePermissions())
                    {
                        return false;
                    }

                    SecurityViewModel.Save(Settings.Security);
                    if (!SaveLogSettingsChanges())
                    {
                        return false;
                    }

                    if (PersistenceSettingsViewModel.IsDirty)
                    {
                        PersistenceSettingsViewModel.Save(Settings.Persistence);
                    }

                    if (PerfmonViewModel.IsDirty)
                    {
                        PerfmonViewModel.Save(Settings.PerfCounters);
                    }

                    var isWritten = WriteSettings();
                    if (isWritten)
                    {
                        ResetIsDirtyForChildren();
                        IsSaved = true;
                        IsDirty = false;
                        ClearErrors();
                    }
                    else
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

        private bool SaveLogSettingsChanges()
        {
            if (LogSettingsViewModel.IsDirty)
            {
                LogSettingsViewModel.Save(Settings.Logging);
                if (!LogSettingsViewModel.HasAuditFilePathMoved)
                {
                    return false;
                }
            }

            return true;
        }

        bool ValidateDuplicateServerPermissions()
        {
            if (SecurityViewModel.HasDuplicateServerPermissions())
            {
                IsSaved = false;
                IsDirty = true;
                ShowError(StringResources.SaveErrorPrefix, StringResources.SaveSettingsDuplicateServerPermissions);
                _popupController.ShowHasDuplicateServerPermissions();
                return false;
            }

            return true;
        }

        bool ValidateResourcePermissions()
        {
            if (SecurityViewModel.HasInvalidResourcePermission())
            {
                IsSaved = false;
                IsDirty = true;
                ShowError(StringResources.SaveErrorPrefix, StringResources.SaveSettingsInvalidPermissionEntry);
                _popupController.ShowInvalidResourcePermission();
                return false;
            }

            return true;
        }

        bool ValidateDuplicateResourcePermissions()
        {
            if (SecurityViewModel.HasDuplicateResourcePermissions())
            {
                IsSaved = false;
                IsDirty = true;
                ShowError(StringResources.SaveErrorPrefix, StringResources.SaveSettingsDuplicateResourcePermissions);
                _popupController.ShowHasDuplicateResourcePermissions();
                return false;
            }

            return true;
        }

        bool WriteSettings()
        {
            var payload = CurrentEnvironment.ResourceRepository.WriteSettings(CurrentEnvironment, Settings);
            if (payload == null)
            {
                ShowError(StringResources.NetworkSettingErrorPrefix, string.Format(GlobalConstants.NetworkCommunicationErrorTextFormat, nameof(WriteSettings)));
                return false;
            }

            if (payload.HasError)
            {
                ShowError(StringResources.SaveErrorHeader, payload.Message.ToString());
                return false;
            }

            return true;
        }

        Data.Settings.Settings ReadSettings()
        {
            var payload = CurrentEnvironment.ResourceRepository.ReadSettings(CurrentEnvironment);
            if (payload == null)
            {
                ShowError(StringResources.NetworkSettingErrorPrefix, string.Format(GlobalConstants.NetworkCommunicationErrorTextFormat, nameof(ReadSettings)));
            }

            return payload;
        }

        protected void ClearErrors()
        {
            HasErrors = false;
            Errors = null;
        }

        protected virtual void ShowError(string header, string description)
        {
            HasErrors = true;
            Errors = description;
        }

        public string ResourceType => StringResources.SettingsTitle;
        public string PerfmonHeader => PerfmonViewModel != null && PerfmonViewModel.IsDirty ? StringResources.SettingsPerformanceCounters + " *" : StringResources.SettingsPerformanceCounters;
    }
}