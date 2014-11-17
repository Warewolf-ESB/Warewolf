using System;
using System.Globalization;
using System.Windows.Input;
using Dev2.Common;
using Dev2.Common.ExtMethods;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Services.Security;
using Dev2.Studio.Core.Interfaces;

namespace Dev2.Settings.Logging
{
    public enum LogLevel
    {
        Fatal,
        Error,
        Warn,
        Info,
        Debug,
        Trace
    }
    public class LogSettingsViewModel : SettingsItemViewModel, ILogSettings
    {
        private string _serverLogMaxSize;
        private string _studioLogMaxSize;
        LogLevel _serverLogLevel;
        LogLevel _studioLogLevel;

        public LogSettingsViewModel(LoggingSettingsTo logging, IEnvironmentModel currentEnvironment)
        {
            if (logging == null) throw new ArgumentNullException("logging");
            if (currentEnvironment == null) throw new ArgumentNullException("currentEnvironment");
            GetServerLogFileCommand = new DelegateCommand(o => {});
            GetStudioLogFileCommand = new DelegateCommand(o => {});
            LogLevel serverLogLevel;
            if (Enum.TryParse(logging.LogLevel,out serverLogLevel))
            {
                ServerLogLevel = serverLogLevel;
            }
            ServerLogMaxSize = logging.LogSize.ToString(CultureInfo.InvariantCulture);

            LogLevel studioLogLevel;
            if (Enum.TryParse(Dev2Logger.GetLogLevel(), out studioLogLevel))
            {
                StudioLogLevel = studioLogLevel;
            }
            StudioLogMaxSize = Dev2Logger.GetLogMaxSize().ToString(CultureInfo.InvariantCulture);
        }

        public virtual void Save(LoggingSettingsTo logSettings)
        {
            logSettings.LogLevel = ServerLogLevel.ToString();
            logSettings.LogSize = int.Parse(ServerLogMaxSize);
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
