using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Input;
using Dev2.Common;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.CustomControls.Progress;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Services.Security;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Network;
using Dev2.Utils;
using Newtonsoft.Json;

namespace Dev2.Settings.Logging
{
    public enum LogLevel
    {
        // ReSharper disable InconsistentNaming
        OFF,
        FATAL,
        ERROR,
        WARN,
        INFO,
        DEBUG,
        TRACE
    }
    public class LogSettingsViewModel : SettingsItemViewModel, ILogSettings, IUpdatesHelp
    {
        public IEnvironmentModel CurrentEnvironment
        {
            private get
            {
                return _currentEnvironment;
            }
            set
            {
                _currentEnvironment = value;
                // ReSharper disable once ExplicitCallerInfoArgument
                OnPropertyChanged("CanEditStudioLogSettings");
                // ReSharper disable once ExplicitCallerInfoArgument
                OnPropertyChanged("CanEditLogSettings");
            }
        }
        private string _serverLogMaxSize;
        private string _studioLogMaxSize;
        private LogLevel _serverEventLogLevel;
        private LogLevel _studioEventLogLevel;
        private ProgressDialogViewModel _progressDialogViewModel;
        private string _serverLogFile;
        private IEnvironmentModel _currentEnvironment;
        private LogLevel _serverFileLogLevel;
        private LogLevel _studioFileLogLevel;
        private LogSettingsViewModel _item;

        // ReSharper disable once UnusedMember.Global
        public LogSettingsViewModel()
        {

        }

        public LogSettingsViewModel(LoggingSettingsTo logging, IEnvironmentModel currentEnvironment)
        {
            if (logging == null) throw new ArgumentNullException(nameof(logging));
            if (currentEnvironment == null) throw new ArgumentNullException(nameof(currentEnvironment));
            CurrentEnvironment = currentEnvironment;
            GetServerLogFileCommand = new DelegateCommand(OpenServerLogFile);
            GetStudioLogFileCommand = new DelegateCommand(OpenStudioLogFile);
            LogLevel serverFileLogLevel;
            LogLevel serverEventLogLevel;
            if (Enum.TryParse(logging.FileLoggerLogLevel, out serverFileLogLevel))
            {
                _serverFileLogLevel = serverFileLogLevel;
            }
            if (Enum.TryParse(logging.EventLogLoggerLogLevel, out serverEventLogLevel))
            {
                _serverEventLogLevel = serverEventLogLevel;
            }
            _serverLogMaxSize = logging.FileLoggerLogSize.ToString(CultureInfo.InvariantCulture);

            LogLevel studioFileLogLevel;
            LogLevel studioEventLogLevel;
            if (Enum.TryParse(Dev2Logger.GetFileLogLevel(), out studioFileLogLevel))
            {
                _studioFileLogLevel = studioFileLogLevel;
            }
            if (Enum.TryParse(Dev2Logger.GetEventLogLevel(), out studioEventLogLevel))
            {
                _studioEventLogLevel = studioEventLogLevel;
            }
            _studioLogMaxSize = Dev2Logger.GetLogMaxSize().ToString(CultureInfo.InvariantCulture);
        }

        [ExcludeFromCodeCoverage]
        void OpenServerLogFile(object o)
        {
            WebClient client = new WebClient { Credentials = CurrentEnvironment.Connection.HubConnection.Credentials };
            var dialog = new ProgressDialog();
            _progressDialogViewModel = new ProgressDialogViewModel(() => { dialog.Close(); }, delegate
            {
                dialog.Show();
            }, delegate
            {
                dialog.Close();
            });
            _progressDialogViewModel.StatusChanged("Server Log File", 0, 0);
            _progressDialogViewModel.SubLabel = "Preparing to download Warewolf Server log file.";
            dialog.DataContext = _progressDialogViewModel;
            _progressDialogViewModel.Show();
            client.DownloadProgressChanged += DownloadProgressChanged;
            client.DownloadFileCompleted += DownloadFileCompleted;
            var managementServiceUri = WebServer.GetInternalServiceUri("getlogfile", CurrentEnvironment.Connection);
            _serverLogFile = Path.Combine(GlobalConstants.TempLocation, CurrentEnvironment.Connection.DisplayName + " Server Log.txt");
            client.DownloadFileAsync(managementServiceUri, _serverLogFile);

        }

        [ExcludeFromCodeCoverage]
        void DownloadFileCompleted(object sender, AsyncCompletedEventArgs asyncCompletedEventArgs)
        {
            _progressDialogViewModel.Close();
            Process.Start(_serverLogFile);
        }

        [ExcludeFromCodeCoverage]
        void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            _progressDialogViewModel.SubLabel = "";
            _progressDialogViewModel.StatusChanged("Server Log File", e.ProgressPercentage, e.TotalBytesToReceive);
        }

        [ExcludeFromCodeCoverage]
        void OpenStudioLogFile(object o)
        {
            var localAppDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var logFile = Path.Combine(localAppDataFolder, "Warewolf", "Studio Logs", "Warewolf Studio.log");
            if (File.Exists(logFile))
            {
                Process.Start(logFile);
            }
            else
            {
                CustomContainer.Get<IPopupController>().Show("Studio Log file does not exist.", "Error", MessageBoxButton.OK, MessageBoxImage.Error, "", false, true, false, false, false, false);
            }
        }

        public bool CanEditLogSettings => CurrentEnvironment.IsConnected;

        public bool CanEditStudioLogSettings => CurrentEnvironment.IsLocalHost;

        [ExcludeFromCodeCoverage]
        public virtual void Save(LoggingSettingsTo logSettings)
        {
            logSettings.FileLoggerLogLevel = ServerFileLogLevel.ToString();
            logSettings.EventLogLoggerLogLevel = ServerEventLogLevel.ToString();
            logSettings.FileLoggerLogSize = int.Parse(ServerLogMaxSize);
            var settingsConfigFile = HelperUtils.GetStudioLogSettingsConfigFile();
            Dev2Logger.WriteLogSettings(StudioLogMaxSize, StudioFileLogLevel.ToString(), StudioEventLogLevel.ToString(), settingsConfigFile, "Warewolf Studio");

            SetItem(this);
        }

        protected override void CloseHelp()
        {
            //Implement if help is done for the log settings.
        }

        [JsonIgnore]
        public LogSettingsViewModel Item
        {
            private get { return _item; }
            set
            {
                _item = value;
                OnPropertyChanged();
            }
        }

        public void SetItem(LogSettingsViewModel model)
        {
            Item = Clone(model);
        }

        public LogSettingsViewModel Clone(LogSettingsViewModel model)
        {
            var resolver = new ShouldSerializeContractResolver();
            var ser = JsonConvert.SerializeObject(model, new JsonSerializerSettings { ContractResolver = resolver });
            LogSettingsViewModel clone = JsonConvert.DeserializeObject<LogSettingsViewModel>(ser);
            return clone;
        }

        public ICommand GetServerLogFileCommand { get; }
        public ICommand GetStudioLogFileCommand { get; }
        public LogLevel ServerEventLogLevel
        {
            get
            {
                return _serverEventLogLevel;
            }
            set
            {
                _serverEventLogLevel = value;
                IsDirty = !Equals(Item);
                OnPropertyChanged();
            }
        }
        public LogLevel StudioEventLogLevel
        {
            get
            {
                return _studioEventLogLevel;
            }
            set
            {
                _studioEventLogLevel = value;
                IsDirty = !Equals(Item);
                OnPropertyChanged();
            }
        }

        public LogLevel ServerFileLogLevel
        {
            get
            {
                return _serverFileLogLevel;
            }
            set
            {
                _serverFileLogLevel = value;
                IsDirty = !Equals(Item);
                OnPropertyChanged();
            }
        }
        public LogLevel StudioFileLogLevel
        {
            get
            {
                return _studioFileLogLevel;
            }
            set
            {
                _studioFileLogLevel = value;
                IsDirty = !Equals(Item);
                OnPropertyChanged();
            }
        }

        public string ServerLogMaxSize
        {
            get { return _serverLogMaxSize; }
            set
            {
                if (string.IsNullOrEmpty(value) && string.IsNullOrEmpty(_serverLogMaxSize))
                {
                    _serverLogMaxSize = "0";
                }
                else
                {
                    int val;
                    if (value.IsWholeNumber(out val))
                    {
                        IsDirty = !Equals(Item);
                        _serverLogMaxSize = value;
                        OnPropertyChanged();
                    }
                }

            }
        }

        public string StudioLogMaxSize
        {
            get { return _studioLogMaxSize; }
            set
            {
                if (string.IsNullOrEmpty(value) && string.IsNullOrEmpty(_studioLogMaxSize))
                {
                    _studioLogMaxSize = "0";
                }
                else
                {
                    int val;
                    if (value.IsWholeNumber(out val))
                    {
                        IsDirty = !Equals(Item);
                        _studioLogMaxSize = value;
                        OnPropertyChanged();
                    }
                }
              
            }
        }

        #region Implementation of IUpdatesHelp

        public void UpdateHelpDescriptor(string helpText)
        {
            HelpText = helpText;
        }

        #endregion

        private bool Equals(LogSettingsViewModel other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            return EqualsSeq(other);
        }

        private bool EqualsSeq(LogSettingsViewModel other)
        {
            var equalsSeq = string.Equals(_serverEventLogLevel.ToString(), other._serverEventLogLevel.ToString()) &&
                            string.Equals(_studioEventLogLevel.ToString(), other._studioEventLogLevel.ToString()) &&
                            string.Equals(_serverFileLogLevel.ToString(), other._serverFileLogLevel.ToString()) &&
                            string.Equals(_studioFileLogLevel.ToString(), other._studioFileLogLevel.ToString()) &&
                            int.Parse(_serverLogMaxSize) == int.Parse(other._serverLogMaxSize) &&
                            int.Parse(_studioLogMaxSize) == int.Parse(other._studioLogMaxSize);
            return equalsSeq;
        }
    }

    public interface ILogSettings
    {
        ICommand GetServerLogFileCommand { get; }
        ICommand GetStudioLogFileCommand { get; }
        LogLevel ServerEventLogLevel { get; set; }
        LogLevel StudioEventLogLevel { get; set; }
        string StudioLogMaxSize { get; }
        string ServerLogMaxSize { get; }
        bool CanEditStudioLogSettings { get; }
        bool CanEditLogSettings { get; }
        LogLevel ServerFileLogLevel { get; }
        LogLevel StudioFileLogLevel { get; }
    }
}
