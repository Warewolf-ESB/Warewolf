using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Input;
using Dev2.Common;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.CustomControls.Progress;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Services.Security;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Network;

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
    public class LogSettingsViewModel : SettingsItemViewModel, ILogSettings
    {
        public IEnvironmentModel CurrentEnvironment { get; set; }
        private string _serverLogMaxSize;
        private string _studioLogMaxSize;
        LogLevel _serverLogLevel;
        LogLevel _studioLogLevel;
        ProgressDialogViewModel _progressDialogViewModel;

        public LogSettingsViewModel(LoggingSettingsTo logging, IEnvironmentModel currentEnvironment)
        {
            if (logging == null) throw new ArgumentNullException("logging");
            if (currentEnvironment == null) throw new ArgumentNullException("currentEnvironment");
            CurrentEnvironment = currentEnvironment;
            GetServerLogFileCommand = new DelegateCommand(OpenServerLogFile);
            GetStudioLogFileCommand = new DelegateCommand(OpenStudioLogFile);
            LogLevel serverLogLevel;
            if (Enum.TryParse(logging.LogLevel,out serverLogLevel))
            {
                _serverLogLevel = serverLogLevel;
            }
            _serverLogMaxSize = logging.LogSize.ToString(CultureInfo.InvariantCulture);

            LogLevel studioLogLevel;
            if (Enum.TryParse(Dev2Logger.GetLogLevel(), out studioLogLevel))
            {
                _studioLogLevel = studioLogLevel;
            }
            _studioLogMaxSize = Dev2Logger.GetLogMaxSize().ToString(CultureInfo.InvariantCulture);
        }

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
            _progressDialogViewModel.Show();
            client.DownloadProgressChanged += DownloadProgressChanged;
            client.DownloadDataCompleted += DownloadFileCompleted;
            var managementServiceUri = WebServer.GetManagementServiceUri("FetchCurrentServerLogService", CurrentEnvironment.Connection);
            client.DownloadDataAsync(managementServiceUri);
            // Starts the download
            
            
           
        }

        void DownloadFileCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            _progressDialogViewModel.Close();
            string result = System.Text.Encoding.UTF8.GetString(e.Result);
            var tempPath = Path.GetTempPath();
            var serverLogFile = Path.Combine(tempPath, CurrentEnvironment.Connection.DisplayName + " Server Log.txt");
            File.WriteAllText(serverLogFile,result);
            Process.Start(serverLogFile);
        }

        void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            _progressDialogViewModel.StatusChanged("Server Log File",e.ProgressPercentage,e.TotalBytesToReceive);
        }

        void OpenStudioLogFile(object o)
        {
            var localAppDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var logFile = Path.Combine(localAppDataFolder, "Warewolf", "Studio Logs", "Warewolf Studio.log");
            if(File.Exists(logFile))
            {
                Process.Start(logFile);
            }
            else
            {
                CustomContainer.Get<IPopupController>().Show("Studio Log file does not exist.", "Error", MessageBoxButton.OK, MessageBoxImage.Error, "");
            }
        }

        public virtual void Save(LoggingSettingsTo logSettings)
        {
            logSettings.LogLevel = ServerLogLevel.ToString();
            logSettings.LogSize = int.Parse(ServerLogMaxSize);
            Dev2Logger.WriteLogSettings(StudioLogMaxSize,StudioLogLevel.ToString());
        }

        

        protected override void CloseHelp()
        {
            //Implement if help is done for the log settings.
        }

        public ICommand GetServerLogFileCommand { get; private set; }
        public ICommand GetStudioLogFileCommand { get; private set; }
        public LogLevel ServerLogLevel
        {
            get
            {
                return _serverLogLevel;
            }
            set
            {
                _serverLogLevel = value;
                IsDirty = true;
                OnPropertyChanged();
            }
        }
        public LogLevel StudioLogLevel
        {
            get
            {
                return _studioLogLevel;
            }
            set
            {
                _studioLogLevel = value;
                IsDirty = true;
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
                    IsDirty = true;
                }
                else
                {
                    int val;
                    if (value.IsWholeNumber(out val))
                    {
                        IsDirty = true;
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
                    IsDirty = true;
                }
                else
                {
                    int val;
                    if (value.IsWholeNumber(out val))
                    {
                        IsDirty = true;
                        _studioLogMaxSize = value;
                        OnPropertyChanged();
                    }
                }
            }
        }
    }

    public interface ILogSettings
    {
        ICommand GetServerLogFileCommand { get; }
        ICommand GetStudioLogFileCommand { get; }
        LogLevel ServerLogLevel { get; set; }
        LogLevel StudioLogLevel { get; set; }
        string StudioLogMaxSize { get; }
        string ServerLogMaxSize { get; }
    }
}
