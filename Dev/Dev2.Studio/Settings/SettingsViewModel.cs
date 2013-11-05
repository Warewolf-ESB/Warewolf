using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Caliburn.Micro;
using Dev2.Common;
using Dev2.Data.Settings.Security;
using Dev2.Services.Events;
using Dev2.Settings.Logging;
using Dev2.Settings.Security;
using Dev2.Studio.Controller;
using Dev2.Studio.Core.Controller;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.ViewModels.WorkSurface;
using Dev2.Threading;
using Newtonsoft.Json;
using Unlimited.Framework;

namespace Dev2.Settings
{
    public class SettingsViewModel : BaseWorkSurfaceViewModel
    {
        bool _isLoading;
        bool _isDirty;
        bool _selectionChanging;

        bool _showLogging;
        bool _showSecurity = true;

        SecurityViewModel _securityViewModel;

        readonly IPopupController _popupController;
        readonly IAsyncWorker _asyncWorker;
        LoggingViewModel _loggingViewModel;

        public SettingsViewModel()
            : this(EventPublishers.Aggregator, new PopupController(), new AsyncWorker())
        {
        }

        public SettingsViewModel(IEventAggregator eventPublisher, IPopupController popupController, IAsyncWorker asyncWorker)
            : base(eventPublisher)
        {
            VerifyArgument.IsNotNull("popupController", popupController);
            _popupController = popupController;
            VerifyArgument.IsNotNull("asyncWorker", asyncWorker);
            _asyncWorker = asyncWorker;

            SaveCommand = new RelayCommand(o => SaveSettings(), o => IsDirty);
            ServerChangedCommand = new RelayCommand(OnServerChanged, o => true);
        }

        public ICommand SaveCommand { get; private set; }

        public ICommand ServerChangedCommand { get; private set; }

        public IEnvironmentModel CurrentEnvironment { get; private set; }

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
            var server = obj as IServer;
            if(server == null || server.Environment == null)
            {
                return;
            }

            if(!server.Environment.IsConnected)
            {
                server.Environment.CanStudioExecute = false;
                _popupController.ShowNotConnected();
                return;
            }

            CurrentEnvironment = server.Environment;
            LoadSettings();
        }

        void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if(args.PropertyName == "IsDirty")
            {
                IsDirty = true;
            }
        }

        void LoadSettings()
        {
            IsDirty = false;
            IsLoading = true;

            _asyncWorker.Start(() =>
            {
                var settingsJson = ExecuteCommand(SettingsServiceAction.Read);
                if(!string.IsNullOrEmpty(settingsJson))
                {
                    Settings = JsonConvert.DeserializeObject<Data.Settings.Settings>(settingsJson);
                }
            }, () =>
            {
                SecurityViewModel = new SecurityViewModel(Settings.Security ?? new List<WindowsGroupPermission>());
                SecurityViewModel.PropertyChanged += OnViewModelPropertyChanged;

                // TODO: Read from server
                LoggingViewModel = new LoggingViewModel();

                IsLoading = false;

                if(Settings.HasError)
                {
                    ShowError("Load Error", Settings.Error);
                }
            });
        }

        void SaveSettings()
        {
            var result = ExecuteCommand(SettingsServiceAction.Write);
            if(result == null)
            {
                return;
            }
            if(result.ToLowerInvariant() != "success")
            {
                ShowError("Save Error", result);
                return;
            }
            IsDirty = false;
        }

        string ExecuteCommand(SettingsServiceAction action)
        {
            var serviceName = string.Format("Settings{0}Service", action);

            dynamic dataObj = new UnlimitedObject();
            dataObj.Service = serviceName;
            if(action == SettingsServiceAction.Write)
            {
                dataObj.Settings = Settings.ToString();
            }

            var result = CurrentEnvironment.Connection.ExecuteCommand(dataObj.XmlString, CurrentEnvironment.Connection.WorkspaceID, GlobalConstants.NullDataListID);
            if(result == null)
            {
                ShowError("Network Error", string.Format(GlobalConstants.NetworkCommunicationErrorTextFormat, serviceName));
            }
            return result;
        }

        protected virtual void ShowError(string header, string description)
        {
            throw new Exception(string.Format("{0} : {1}", header, description));
        }

        enum SettingsServiceAction
        {
            Read,
            Write
        }
    }
}
