using System.Runtime.CompilerServices;
using System.Windows.Input;
using Caliburn.Micro;
using Dev2.Services.Events;
using Dev2.Studio.Core.Controller;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.ViewModels.WorkSurface;

namespace Dev2.Settings
{
    public class SettingsViewModel : BaseWorkSurfaceViewModel
    {
        bool _isLoading;
        bool _selectionChanging;

        bool _showLogging;
        bool _showSecurity = true;

        IPopupController _popupController;

        public SettingsViewModel()
            : this(EventPublishers.Aggregator)
        {
        }

        public SettingsViewModel(IEventAggregator eventPublisher)
            : base(eventPublisher)
        {
            ServerChangedCommand = new RelayCommand(OnServerChanged, o => true);
        }

        public ICommand ServerChangedCommand { get; private set; }

        public IEnvironmentModel CurrentEnvironment { get; private set; }

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

            IsLoading = true;

            try
            {
                CurrentEnvironment = server.Environment;
                LoadSettings();
            }
            finally
            {
                IsLoading = false;
            }
        }

        void LoadSettings()
        {
            //CurrentEnvironment.Connection.ExecuteCommand()
        }

        void SaveSettings()
        {
        }
    }
}
