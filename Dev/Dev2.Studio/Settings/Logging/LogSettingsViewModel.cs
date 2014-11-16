using System;
using System.Windows.Input;
using Dev2.Common.ExtMethods;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Services.Security;
using Dev2.Studio.Core.Interfaces;

namespace Dev2.Settings.Logging
{
    public class LogSettingsViewModel : SettingsItemViewModel, ILogSettings
    {
        private readonly LoggingSettingsTo _logging;
        private readonly IEnvironmentModel _currentEnvironment;
        private bool _isServerLogFatal;
        private bool _isServerLogError;
        private bool _isServerLogWarn;
        private bool _isServerLogInfo;
        private bool _isServerLogDebug;
        private bool _isServerLogTrace;
        private bool _isStudioLogFatal;
        private bool _isStudioLogError;
        private bool _isStudioLogWarn;
        private bool _isStudioLogInfo;
        private bool _isStudioLogDebug;
        private bool _isStudioLogTrace;
        private string _serverLogMaxSize;
        private string _studioLogMaxSize;

        public LogSettingsViewModel(LoggingSettingsTo logging, IEnvironmentModel currentEnvironment)
        {
            if (logging == null) throw new ArgumentNullException("logging");
            if (currentEnvironment == null) throw new ArgumentNullException("currentEnvironment");
            _logging = logging;
            _currentEnvironment = currentEnvironment;
            GetServerLogFileCommand = new DelegateCommand(o => {});
            GetStudioLogFileCommand = new DelegateCommand(o => {});
        }

        protected override void CloseHelp()
        {
            //Implement if help is done for the log settings.
        }

        public ICommand GetServerLogFileCommand { get; private set; }
        public ICommand GetStudioLogFileCommand { get; private set; }

        public bool IsServerLogFatal
        {
            get { return _isServerLogFatal; }
            set
            {
                _isServerLogFatal = value;
                OnPropertyChanged();
            }
        }

        public bool IsServerLogError
        {
            get { return _isServerLogError; }
            set
            {
                _isServerLogError = value;
                OnPropertyChanged();
            }
        }

        public bool IsServerLogWarn
        {
            get { return _isServerLogWarn; }
            set
            {
                _isServerLogWarn = value;
                OnPropertyChanged();
            }
        }

        public bool IsServerLogInfo
        {
            get { return _isServerLogInfo; }
            set
            {
                _isServerLogInfo = value;
                OnPropertyChanged();
            }
        }

        public bool IsServerLogDebug
        {
            get { return _isServerLogDebug; }
            set
            {
                _isServerLogDebug = value;
                OnPropertyChanged();
            }
        }

        public bool IsServerLogTrace
        {
            get { return _isServerLogTrace; }
            set
            {
                _isServerLogTrace = value;
                OnPropertyChanged();
            }
        }

        public bool IsStudioLogFatal
        {
            get { return _isStudioLogFatal; }
            set
            {
                _isStudioLogFatal = value;
                OnPropertyChanged();
            }
        }

        public bool IsStudioLogError
        {
            get { return _isStudioLogError; }
            set
            {
                _isStudioLogError = value;
                OnPropertyChanged();
            }
        }

        public bool IsStudioLogWarn
        {
            get { return _isStudioLogWarn; }
            set
            {
                _isStudioLogWarn = value;
                OnPropertyChanged();
            }
        }

        public bool IsStudioLogInfo
        {
            get { return _isStudioLogInfo; }
            set
            {
                _isStudioLogInfo = value;
                OnPropertyChanged();
            }
        }

        public bool IsStudioLogDebug
        {
            get { return _isStudioLogDebug; }
            set
            {
                _isStudioLogDebug = value;
                OnPropertyChanged();
            }
        }

        public bool IsStudioLogTrace
        {
            get { return _isStudioLogTrace; }
            set
            {
                _isStudioLogTrace = value;
                OnPropertyChanged();
            }
        }

        public string ServerLogMaxSize
        {
            get { return _serverLogMaxSize; }
            set
            {
                _serverLogMaxSize = value;
                OnPropertyChanged();
            }
        }

        public string StudioLogMaxSize
        {
            get { return _studioLogMaxSize; }
            set
            {
                if (string.IsNullOrEmpty(value))
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
                    }
                }
                _studioLogMaxSize = value;
                OnPropertyChanged();
            }
        }
    }

    public interface ILogSettings
    {
        ICommand GetServerLogFileCommand { get; }
        ICommand GetStudioLogFileCommand { get; }
        bool IsServerLogFatal { get; set; }
        bool IsServerLogError { get; set; }
        bool IsServerLogWarn { get; set; }
        bool IsServerLogInfo { get; set; }
        bool IsServerLogDebug { get; set; }
        bool IsServerLogTrace { get; set; }
        
        bool IsStudioLogFatal { get; set; }
        bool IsStudioLogError { get; set; }
        bool IsStudioLogWarn { get; set; }
        bool IsStudioLogInfo { get; set; }
        bool IsStudioLogDebug { get; set; }
        bool IsStudioLogTrace { get; set; }
        string ServerLogMaxSize { get; set; }
        string StudioLogMaxSize { get; set; }
    }
}
