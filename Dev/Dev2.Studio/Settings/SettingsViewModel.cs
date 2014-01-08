using Caliburn.Micro;
using Dev2.Common;
using Dev2.Services.Events;
using Dev2.Services.Security;
using Dev2.Settings.Logging;
using Dev2.Settings.Security;
using Dev2.Studio.Controller;
using Dev2.Studio.Core.Controller;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.ViewModels.WorkSurface;
using Dev2.Threading;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using System.Windows.Input;

namespace Dev2.Settings
{
    public class SettingsViewModel : BaseWorkSurfaceViewModel
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

        public SettingsViewModel()
            : this(EventPublishers.Aggregator, new PopupController(), new AsyncWorker(), (IWin32Window)System.Windows.Application.Current.MainWindow)
        {
        }

        public SettingsViewModel(IEventAggregator eventPublisher, IPopupController popupController, IAsyncWorker asyncWorker, IWin32Window parentWindow)
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
            ServerChangedCommand = new RelayCommand(OnServerChanged, o => true);
        }

        public ICommand SaveCommand { get; private set; }

        public ICommand ServerChangedCommand { get; private set; }

        public IEnvironmentModel CurrentEnvironment { get; private set; }

        public bool HasErrors
        {
            get { return _hasErrors; }
            private set
            {
                if(value.Equals(_hasErrors))
                {
                    return;
                }
                _hasErrors = value;
                NotifyOfPropertyChange(() => HasErrors);
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
            private set
            {
                if(value.Equals(_isSaved))
                {
                    return;
                }
                _isSaved = value;
                NotifyOfPropertyChange(() => IsSaved);
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

            if(!server.IsConnected)
            {
                server.CanStudioExecute = false;
                _popupController.ShowNotConnected();
                return;
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
                var settingsJson = ReadSettings();
                if(!string.IsNullOrEmpty(settingsJson))
                {
                    Settings = JsonConvert.DeserializeObject<Data.Settings.Settings>(settingsJson);
                }
            }, () =>
            {
                SecurityViewModel = CreateSecurityViewModel();
                var isDirtyProperty = DependencyPropertyDescriptor.FromProperty(Dev2.Settings.Security.SecurityViewModel.IsDirtyProperty, typeof(SecurityViewModel));
                isDirtyProperty.AddValueChanged(SecurityViewModel, OnIsDirtyPropertyChanged);

                // TODO: Read from server
                LoggingViewModel = new LoggingViewModel();

                IsLoading = false;

                if(Settings.HasError)
                {
                    ShowError("Load Error", Settings.Error);
                }
            });
        }

        protected virtual SecurityViewModel CreateSecurityViewModel()
        {
            return new SecurityViewModel(Settings.Security ?? new List<WindowsGroupPermission>(), _parentWindow, CurrentEnvironment);
        }

        void OnIsDirtyPropertyChanged(object sender, EventArgs eventArgs)
        {
            IsDirty = SecurityViewModel.IsDirty;
        }

        void SaveSettings()
        {
            ClearErrors();

            var errors = new List<string>();
            SecurityViewModel.Save(Settings.Security, errors);

            if(errors.Count > 0)
            {
                ShowError("Save Error", string.Join(Environment.NewLine, errors));
            }

            var result = WriteSettings();
            if(result == null)
            {
                return;
            }
            if(result.ToLowerInvariant() != "success")
            {
                ShowError("Save Error", result);
                return;
            }

            SecurityViewModel.IsDirty = false;
            IsDirty = false;
            IsSaved = true;
        }

        string WriteSettings()
        {
            var payload = CurrentEnvironment.ResourceRepository.WriteSettings("Settings", Settings.ToString(), CurrentEnvironment);
            if(payload == null || payload.Message.Length == 0)
            {
                ShowError("Network Error", string.Format(GlobalConstants.NetworkCommunicationErrorTextFormat, "SettingsWriteService"));
                throw new NullReferenceException("The Setting are null");
            }

            return payload.Message.ToString();
        }

        string ReadSettings()
        {
            var payload = CurrentEnvironment.ResourceRepository.ReadSettings("Settings", Settings.ToString(), CurrentEnvironment);
            if(payload == null || payload.Message.Length == 0)
            {
                ShowError("Network Error", string.Format(GlobalConstants.NetworkCommunicationErrorTextFormat, "SettingsReadService"));
            }

            if(payload != null)
            {
                return payload.Message.ToString();
            }
            return string.Empty;
        }


        void ClearErrors()
        {
            HasErrors = false;
            Errors = null;
        }

        protected virtual void ShowError(string header, string description)
        {
            HasErrors = true;
            Errors = description;
            //throw new Exception(string.Format("{0} : {1}", header, description));
        }
    }
}
