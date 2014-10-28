
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
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
using System.Windows.Input;
using Caliburn.Micro;
using Dev2.Common;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.CustomControls.Connections;
using Dev2.Instrumentation;
using Dev2.Interfaces;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Services.Events;
using Dev2.Services.Security;
using Dev2.Settings.Logging;
using Dev2.Settings.Security;
using Dev2.Studio.Controller;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.ViewModels.WorkSurface;
using Dev2.Threading;

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
        LoggingViewModel _loggingViewModel;
        IConnectControlViewModel _connectControlViewModel;

        public SettingsViewModel()
            : this(EventPublishers.Aggregator, new PopupController(), new AsyncWorker(), (IWin32Window)System.Windows.Application.Current.MainWindow, null)
        {
        }

        public SettingsViewModel(IEventAggregator eventPublisher, IPopupController popupController, IAsyncWorker asyncWorker, IWin32Window parentWindow, IConnectControlViewModel connectControlViewModel)
            : base(eventPublisher)
        {
            Settings = new Data.Settings.Settings();
            VerifyArgument.IsNotNull("popupController", popupController);
            _popupController = popupController;
            VerifyArgument.IsNotNull("asyncWorker", asyncWorker);
            _asyncWorker = asyncWorker;
            VerifyArgument.IsNotNull("parentWindow", parentWindow);
            _parentWindow = parentWindow;

            SaveCommand = new RelayCommand(o => SaveSettings(), o => IsDirty);
            ServerChangedCommand = new DelegateCommand(OnServerChanged);

            ConnectControlViewModel = connectControlViewModel ?? new ConnectControlViewModel(OnServerChanged, "Server:", false);
        }

        public RelayCommand SaveCommand { get; private set; }

        public ICommand ServerChangedCommand { get; private set; }

        public IEnvironmentModel CurrentEnvironment { get; set; }

        public bool IsSavedSuccessVisible { get { return !HasErrors && !IsDirty && IsSaved; } }

        public bool IsErrorsVisible { get { return HasErrors || (IsDirty && !IsSaved); } }

        public bool HasErrors
        {
            get { return _hasErrors; }
            set
            {
                if(value.Equals(_hasErrors))
                {
                    return;
                }
                _hasErrors = value;
                NotifyOfPropertyChange(() => HasErrors);
                NotifyOfPropertyChange(() => IsSavedSuccessVisible);
                NotifyOfPropertyChange(() => IsErrorsVisible);
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

        public string Errors
        {
            get { return _errors; }
            set
            {
                if(value == _errors)
                {
                    return;
                }
                _errors = value;
                NotifyOfPropertyChange(() => Errors);
            }
        }

        public bool IsSaved
        {
            get { return _isSaved; }
            set
            {
                if(value.Equals(_isSaved))
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
            get { return _isDirty; }
            set
            {
                if(value.Equals(_isDirty))
                {
                    return;
                }
                _isDirty = value;
                NotifyOfPropertyChange(() => IsDirty);
                NotifyOfPropertyChange(() => IsSavedSuccessVisible);
                NotifyOfPropertyChange(() => IsErrorsVisible);
                SaveCommand.RaiseCanExecuteChanged();
            }
        }

        public bool IsLoading
        {
            get { return _isLoading; }
            set
            {
                if(value.Equals(_isLoading))
                {
                    return;
                }
                _isLoading = value;
                NotifyOfPropertyChange(() => IsLoading);
            }
        }

        public bool ShowLogging
        {
            get { return _showLogging; }
            set
            {
                if(value.Equals(_showLogging))
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
            get { return _showSecurity; }
            set
            {
                if(value.Equals(_showSecurity))
                {
                    return;
                }
                _showSecurity = value;
                OnSelectionChanged();
                NotifyOfPropertyChange(() => ShowSecurity);
            }
        }

        public Data.Settings.Settings Settings { get; private set; }

        public SecurityViewModel SecurityViewModel
        {
            get { return _securityViewModel; }
            private set
            {
                if(Equals(value, _securityViewModel))
                {
                    return;
                }
                _securityViewModel = value;
                NotifyOfPropertyChange(() => SecurityViewModel);
            }
        }

        public LoggingViewModel LoggingViewModel
        {
            get { return _loggingViewModel; }
            private set
            {
                if(Equals(value, _loggingViewModel))
                {
                    return;
                }
                _loggingViewModel = value;
                NotifyOfPropertyChange(() => LoggingViewModel);
            }
        }
        public string SecurityHeader
        {
            get
            {
                return IsDirty ? "Security *" : "Security";
            }
        }

        void OnSelectionChanged([CallerMemberName] string propertyName = null)
        {
            if(_selectionChanging)
            {
                return;
            }

            _selectionChanging = true;
            switch(propertyName)
            {
                case "ShowLogging":
                    ShowSecurity = !ShowLogging;
                    break;

                case "ShowSecurity":
                    // TODO: Remove this when logging is enabled!
                    if(!ShowSecurity)
                    {
                        ShowSecurity = true;
                    }
                    // TODO: Add this when logging is enabled!
                    //ShowLogging = !ShowSecurity;
                    break;
            }
            _selectionChanging = false;
        }

        void OnServerChanged(object obj)
        {
            var server = obj as IEnvironmentModel;

            if(server == null)
            {
                return;
            }

            if(SecurityViewModel != null)
            {
                if(!DoDeactivate())
                {
                    return;
                }
            }

            CurrentEnvironment = server;
            LoadSettings();
        }

        void LoadSettings()
        {
            ClearErrors();
            IsSaved = false;
            IsDirty = false;
            IsLoading = true;

            _asyncWorker.Start(() =>
            {
                Settings = CurrentEnvironment.IsConnected ? ReadSettings() : new Data.Settings.Settings { Security = new SecuritySettingsTO() };

            }, () =>
            {
                IsLoading = false;
                SecurityViewModel = CreateSecurityViewModel();
                LoggingViewModel = CreateLoggingViewModel();

                AddPropertyChangedHandlers();

                if(Settings.HasError)
                {
                    ShowError("Load Error", Settings.Error);
                }
            });
        }

        protected virtual SecurityViewModel CreateSecurityViewModel()
        {
            return new SecurityViewModel(Settings.Security, _parentWindow, CurrentEnvironment);
        }

        protected virtual LoggingViewModel CreateLoggingViewModel()
        {
            return new LoggingViewModel();
        }

        void AddPropertyChangedHandlers()
        {
            var isDirtyProperty = DependencyPropertyDescriptor.FromProperty(SettingsItemViewModel.IsDirtyProperty, typeof(SettingsItemViewModel));
            isDirtyProperty.AddValueChanged(LoggingViewModel, OnIsDirtyPropertyChanged);
            SecurityViewModel.PropertyChanged += (sender, args) =>
            {
                if(args.PropertyName == "IsDirty")
                {
                    OnIsDirtyPropertyChanged(null, new EventArgs());
                }
            };
        }

        void OnIsDirtyPropertyChanged(object sender, EventArgs eventArgs)
        {
            IsDirty = SecurityViewModel.IsDirty;
            NotifyOfPropertyChange(() => SecurityHeader);
            ClearErrors();
        }

        void ResetIsDirtyForChildren()
        {
            if(SecurityViewModel != null)
            {
                SecurityViewModel.IsDirty = false;
                NotifyOfPropertyChange(() => SecurityHeader);
            }
        }

        #region Overrides of SimpleBaseViewModel

        #region Overrides of Screen

        public virtual bool DoDeactivate()
        {
            var messageBoxResult = GetSaveResult();
            if(messageBoxResult == MessageBoxResult.Cancel || messageBoxResult == MessageBoxResult.None)
            {
                return false;
            }
            if(messageBoxResult == MessageBoxResult.Yes)
            {
                return SaveSettings();
            }

            if(messageBoxResult == MessageBoxResult.No)
            {
                IsDirty = false;
                ResetIsDirtyForChildren();
            }

            return true;
        }

        MessageBoxResult GetSaveResult()
        {
            if(_popupController != null && SecurityViewModel.IsDirty)
            {
                return _popupController.ShowSettingsCloseConfirmation();
            }
            return !SecurityViewModel.IsDirty ? MessageBoxResult.No : MessageBoxResult.None;
        }

        #endregion


        #endregion

        /// <summary>
        /// Saves the settings.
        /// </summary>
        /// <returns></returns>
        bool SaveSettings()
        {
            if(CurrentEnvironment.IsConnected)
            {
                if(CurrentEnvironment.AuthorizationService.IsAuthorized(AuthorizationContext.Administrator, null))
                {
                    Tracker.TrackEvent(TrackerEventGroup.Settings, TrackerEventName.SaveClicked);
                    // Need to reset sub view models so that selecting something in them fires our OnIsDirtyPropertyChanged()

                    ClearErrors();
                    if(SecurityViewModel.HasDuplicateResourcePermissions())
                    {
                        IsSaved = false;
                        IsDirty = true;
                        ShowError(StringResources.SaveSettingErrorPrefix, StringResources.SaveSettingsDuplicateResourcePermissions);
                        return false;
                    }

                    if(SecurityViewModel.HasDuplicateServerPermissions())
                    {
                        IsSaved = false;
                        IsDirty = true;
                        ShowError(StringResources.SaveSettingErrorPrefix, StringResources.SaveSettingsDuplicateServerPermissions);
                        return false;
                    }
                    SecurityViewModel.Save(Settings.Security);

                    var isWritten = WriteSettings();
                    if(isWritten)
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

                ShowError(StringResources.SaveSettingErrorPrefix, StringResources.SaveSettingsPermissionsErrorMsg);
                return false;
            }
            ShowError(StringResources.SaveSettingErrorPrefix, StringResources.SaveSettingsNotReachableErrorMsg);
            return false;
        }

        bool WriteSettings()
        {
            var payload = CurrentEnvironment.ResourceRepository.WriteSettings(CurrentEnvironment, Settings);
            if(payload == null)
            {
                ShowError("Network Error", string.Format(GlobalConstants.NetworkCommunicationErrorTextFormat, "WriteSettings"));
                return false;
            }
            if(payload.HasError)
            {
                ShowError("Save Error", payload.Message.ToString());
                return false;
            }
            return true;
        }

        Data.Settings.Settings ReadSettings()
        {
            var payload = CurrentEnvironment.ResourceRepository.ReadSettings(CurrentEnvironment);
            if(payload == null)
            {
                ShowError("Network Error", string.Format(GlobalConstants.NetworkCommunicationErrorTextFormat, "ReadSettings"));
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
    }
}

