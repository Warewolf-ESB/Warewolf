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
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Input;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Resources;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Communication;
using Dev2.CustomControls.Progress;
using Dev2.Data.Interfaces.Enums;
using Dev2.Data.ServiceModel;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Services.Security;
using Dev2.Studio.Core.Network;
using Dev2.Studio.Interfaces;
using Dev2.Utils;
using Newtonsoft.Json;
using Warewolf.Configuration;
using Warewolf.Data;
using Warewolf.Security.Encryption;
using StringExtension = Dev2.Common.ExtMethods.StringExtension;

namespace Dev2.Settings.Logging
{
    public class LogSettingsViewModel : SettingsItemViewModel, ILogSettings, IUpdatesHelp
    {
        private readonly IResourceRepository _resourceRepository;

        public IServer CurrentEnvironment
        {
            private get => _currentEnvironment;
            set
            {
                _currentEnvironment = value;

                OnPropertyChanged(nameof(CanEditStudioLogSettings));

                OnPropertyChanged(nameof(CanEditLogSettings));
            }
        }

        string _serverLogMaxSize;
        string _studioLogMaxSize;
        string _selectedLoggingType;
        LogLevel _serverEventLogLevel;
        LogLevel _studioEventLogLevel;
        LogLevel _executionLogLevel;
        private bool _encryptDataSource = true;
        ProgressDialogViewModel _progressDialogViewModel;
        string _serverLogFile;
        IServer _currentEnvironment;
        readonly LogLevel _serverFileLogLevel;
        LogLevel _studioFileLogLevel;
        LogSettingsViewModel _item;
        string _auditFilePath;
        private Guid _resourceSourceId;
        private IResource _selectedAuditingSource;
        private bool _isLegacy;

        //this is here to clone the viewmodel
        [ExcludeFromCodeCoverage]
        public LogSettingsViewModel()
        {
        }

        public LogSettingsViewModel(LoggingSettingsTo logging, IServer currentEnvironment)
        {
            if (logging == null)
            {
                throw new ArgumentNullException(nameof(logging));
            }

            CurrentEnvironment = currentEnvironment ?? throw new ArgumentNullException(nameof(currentEnvironment));
            _resourceRepository = CurrentEnvironment.ResourceRepository;
            GetServerLogFileCommand = new DelegateCommand(OpenServerLogFile);
            GetStudioLogFileCommand = new DelegateCommand(OpenStudioLogFile);
            if (Enum.TryParse(logging.FileLoggerLogLevel, out LogLevel serverFileLogLevel))
            {
                _serverFileLogLevel = serverFileLogLevel;
            }

            if (Enum.TryParse(logging.EventLogLoggerLogLevel, out LogLevel serverEventLogLevel))
            {
                _serverEventLogLevel = serverEventLogLevel;
            }

            _serverLogMaxSize = logging.FileLoggerLogSize.ToString(CultureInfo.InvariantCulture);
            if (Enum.TryParse(Dev2Logger.GetFileLogLevel(), out LogLevel studioFileLogLevel))
            {
                _studioFileLogLevel = studioFileLogLevel;
            }

            if (Enum.TryParse(Dev2Logger.GetEventLogLevel(), out LogLevel studioEventLogLevel))
            {
                _studioEventLogLevel = studioEventLogLevel;
            }

            _studioLogMaxSize = Dev2Logger.GetLogMaxSize().ToString(CultureInfo.InvariantCulture);
            var serverSettingsData = CurrentEnvironment.ResourceRepository.GetServerSettings(CurrentEnvironment);

            if (Enum.TryParse(serverSettingsData.ExecutionLogLevel, out LogLevel executionLogLevel))
            {
                _executionLogLevel = executionLogLevel;
            }

            if (serverSettingsData.Sink == "LegacySettingsData")
            {
                var legacySettingsData = CurrentEnvironment.ResourceRepository.GetAuditingSettings<LegacySettingsData>(CurrentEnvironment);
                AuditFilePath = legacySettingsData.AuditFilePath;
                var selectedAuditingSource = AuditingSources.FirstOrDefault(o => o.ResourceID == Guid.Empty);
                SelectedAuditingSource = selectedAuditingSource;
            }

            if (serverSettingsData.Sink == "AuditingSettingsData")
            {
                var auditingSettingsData = CurrentEnvironment.ResourceRepository.GetAuditingSettings<AuditingSettingsData>(CurrentEnvironment);
                var selectedAuditingSource = AuditingSources.FirstOrDefault(o => o.ResourceID == auditingSettingsData.LoggingDataSource.Value);
                SelectedAuditingSource = selectedAuditingSource;
                _encryptDataSource = auditingSettingsData.EncryptDataSource;
            }

            IsDirty = false;
        }

        [ExcludeFromCodeCoverage]
        void OpenServerLogFile(object o)
        {
            using (WebClient client = new WebClient {Credentials = CurrentEnvironment.Connection.HubConnection.Credentials})
            {
                var dialog = new ProgressDialog();
                _progressDialogViewModel = new ProgressDialogViewModel(() => { dialog.Close(); }, delegate { dialog.Show(); }, delegate { dialog.Close(); });
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
        static void OpenStudioLogFile(object o)
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

        public bool IsLegacy
        {
            get => _isLegacy;
            set
            {
                _isLegacy = value;
                OnPropertyChanged();
            }
        }

        public virtual void Save(LoggingSettingsTo logSettings)
        {
            logSettings.EventLogLoggerLogLevel = ServerEventLogLevel.ToString();
            logSettings.FileLoggerLogSize = int.Parse(ServerLogMaxSize);
            var settingsConfigFile = HelperUtils.GetStudioLogSettingsConfigFile();

            try
            {
                var changed = false;
                var serverSettingsData = CurrentEnvironment.ResourceRepository.GetServerSettings(CurrentEnvironment);
                var savedResourceId = Guid.Empty;
                var savedSink = serverSettingsData.Sink;
                Enum.TryParse(serverSettingsData.ExecutionLogLevel, out LogLevel savedExecutionLogLevel);
                var savedEncryptDataSource = true;
                if (savedSink == "AuditingSettingsData")
                {
                    var auditingSettingsData = CurrentEnvironment.ResourceRepository.GetAuditingSettings<AuditingSettingsData>(CurrentEnvironment);
                    savedResourceId = auditingSettingsData.LoggingDataSource.Value;
                    savedEncryptDataSource = auditingSettingsData.EncryptDataSource;
                }

                if (savedSink == "LegacySettingsData" && _selectedAuditingSource.ResourceID != Guid.Empty)
                {
                    changed = true;
                }
                if (_encryptDataSource != savedEncryptDataSource)
                {
                    changed = true;
                }
                if (_selectedAuditingSource.ResourceID != savedResourceId)
                {
                    changed = true;
                }

                //TODO: We will use the Server Log Level from the UI until we get the UI changed.
                if (_executionLogLevel != savedExecutionLogLevel)
                {
                    changed = true;
                }

                if (changed)
                {
                    var popupController = CustomContainer.Get<IPopupController>();
                    var result = popupController.ShowLoggerSourceChange(_selectedAuditingSource.ResourceName);
                    if (result == MessageBoxResult.No || result == MessageBoxResult.Cancel)
                    {
                        return;
                    }
                }

                if (_executionLogLevel != savedExecutionLogLevel)
                {
                    serverSettingsData.ExecutionLogLevel = _executionLogLevel.ToString();
                    CurrentEnvironment.ResourceRepository.SaveServerSettings(CurrentEnvironment, serverSettingsData);
                }

                if (_selectedAuditingSource.ResourceID == Guid.Empty)
                {
                    var data = new LegacySettingsData
                    {
                        AuditFilePath = AuditFilePath
                    };
                    CurrentEnvironment.ResourceRepository.SaveAuditingSettings(CurrentEnvironment, data);
                }
                else
                {
                    var source = _selectedAuditingSource as ElasticsearchSource;
                    var serializer = new Dev2JsonSerializer();
                    var payload = serializer.Serialize(source);
                    if (_encryptDataSource)
                    {
                        payload = DpapiWrapper.Encrypt(payload);
                    }

                    var data = new AuditingSettingsData
                    {
                        EncryptDataSource = _encryptDataSource,
                        LoggingDataSource = new NamedGuidWithEncryptedPayload
                        {
                            Name = _selectedAuditingSource.ResourceName,
                            Value = _selectedAuditingSource.ResourceID,
                            Payload = payload
                        }
                    };
                    CurrentEnvironment.ResourceRepository.SaveAuditingSettings(CurrentEnvironment, data);
                }
            }
            catch (Exception ex)
            {
                CustomContainer.Get<IPopupController>().Show("The file was not moved. Error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error, "", false, true, false, false, false, false);
                HasAuditFilePathMoved = false;
                return;
            }

            Dev2Logger.WriteLogSettings(StudioLogMaxSize, StudioFileLogLevel.ToString(), StudioEventLogLevel.ToString(), settingsConfigFile, "Warewolf Studio");
            SetItem(this);

            HasAuditFilePathMoved = true;
        }


        public bool HasAuditFilePathMoved { get; set; }

        [JsonIgnore]
        public LogSettingsViewModel Item
        {
            private get => _item;
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

        private static LogSettingsViewModel Clone(LogSettingsViewModel model)
        {
            var resolver = new ShouldSerializeContractResolver();
            var ser = JsonConvert.SerializeObject(model, new JsonSerializerSettings {ContractResolver = resolver});
            var clone = JsonConvert.DeserializeObject<LogSettingsViewModel>(ser);
            return clone;
        }

        public ICommand GetServerLogFileCommand { get; }
        public ICommand GetStudioLogFileCommand { get; }


        public LogLevel ExecutionLogLevel
        {
            get => _executionLogLevel;
            set
            {
                _executionLogLevel = value;
                IsDirty = !Equals(Item);
                OnPropertyChanged();
            }
        }

        public LogLevel ServerEventLogLevel
        {
            get => _serverEventLogLevel;
            set
            {
                _executionLogLevel = value;
                _serverEventLogLevel = value;
                IsDirty = !Equals(Item);
                OnPropertyChanged();
            }
        }

        public LogLevel StudioEventLogLevel
        {
            get => _studioEventLogLevel;
            set
            {
                _studioEventLogLevel = value;
                IsDirty = !Equals(Item);
                OnPropertyChanged();
            }
        }

        public LogLevel StudioFileLogLevel
        {
            get => _studioFileLogLevel;
            set
            {
                _studioFileLogLevel = value;
                IsDirty = !Equals(Item);
                OnPropertyChanged();
            }
        }

        public IEnumerable<string> LoggingTypes => Data.Interfaces.Enums.EnumHelper<LogLevel>.GetDiscriptionsAsList(typeof(LogLevel)).ToList();

        public string SelectedLoggingType
        {
            get => Data.Interfaces.Enums.EnumHelper<LogLevel>.GetEnumDescription(ServerEventLogLevel.ToString());
            set
            {
                if (string.IsNullOrEmpty(value) && string.IsNullOrEmpty(ServerEventLogLevel.ToString()))
                {
                    return;
                }

                var logLevel = LoggingTypes.Single(p => p.ToString().Contains(value));
                _selectedLoggingType = logLevel;

                var enumFromDescription = Data.Interfaces.Enums.EnumHelper<LogLevel>.GetEnumFromDescription(logLevel);
                ServerEventLogLevel = enumFromDescription;
            }
        }

        public string ServerLogMaxSize
        {
            get => _serverLogMaxSize;
            set
            {
                if (string.IsNullOrEmpty(value) && string.IsNullOrEmpty(_serverLogMaxSize))
                {
                    _serverLogMaxSize = "0";
                }
                else
                {
                    if (StringExtension.IsWholeNumber(value, out int val))
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
            get => _studioLogMaxSize;
            set
            {
                if (string.IsNullOrEmpty(value) && string.IsNullOrEmpty(_studioLogMaxSize))
                {
                    _studioLogMaxSize = "0";
                }
                else
                {
                    if (StringExtension.IsWholeNumber(value, out int val))
                    {
                        IsDirty = !Equals(Item);
                        _studioLogMaxSize = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        public bool EncryptDataSource
        {
            get => _encryptDataSource;
            set
            {
                IsDirty = !Equals(Item);
                _encryptDataSource = value;
                OnPropertyChanged();
            }
        }

        public string AuditFilePath
        {
            get => _auditFilePath;
            set
            {
                IsDirty = !Equals(Item);
                _auditFilePath = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        public IResource SelectedAuditingSource
        {
            get => _selectedAuditingSource;
            set
            {
                IsDirty = !Equals(Item);
                _selectedAuditingSource = value;
                if (_selectedAuditingSource != null)
                {
                    ResourceSourceId = _selectedAuditingSource.ResourceID;
                }

                OnPropertyChanged();
                IsLegacy = SelectedAuditingSource?.ResourceID == Guid.Empty;
            }
        }

        [JsonIgnore]
        public Guid ResourceSourceId
        {
            get => _resourceSourceId;
            set
            {
                IsDirty = !Equals(Item);
                _resourceSourceId = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore] public List<IResource> AuditingSources => LoadAuditingSources();

        private List<IResource> LoadAuditingSources()
        {
            var auditingSources = _resourceRepository.FindResourcesByType<IAuditingSource>(_currentEnvironment);
            IResource defaultSource = new SqliteDBSource
            {
                ResourceID = Guid.Empty,
                ResourceName = "Default"
            };
            auditingSources.Add(defaultSource);
            return auditingSources;
        }

        public void UpdateHelpDescriptor(string helpText)
        {
            HelpText = helpText;
        }

        public bool Equals(LogSettingsViewModel other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            return EqualsSeq(other);
        }

        bool EqualsSeq(LogSettingsViewModel other)
        {
            var equalsSeq = string.Equals(_serverEventLogLevel.ToString(), other._serverEventLogLevel.ToString());
            equalsSeq &= string.Equals(_studioEventLogLevel.ToString(), other._studioEventLogLevel.ToString());
            equalsSeq &= string.Equals(_serverFileLogLevel.ToString(), other._serverFileLogLevel.ToString());
            equalsSeq &= string.Equals(_studioFileLogLevel.ToString(), other._studioFileLogLevel.ToString());
            equalsSeq &= Equals(_selectedLoggingType, other._selectedLoggingType);
            equalsSeq &= int.Parse(_serverLogMaxSize) == int.Parse(other._serverLogMaxSize);
            equalsSeq &= int.Parse(_studioLogMaxSize) == int.Parse(other._studioLogMaxSize);
            equalsSeq &= Equals(_auditFilePath, other._auditFilePath);
            equalsSeq &= Equals(_resourceSourceId, other._resourceSourceId);
            return equalsSeq;
        }

        protected override void CloseHelp()
        {
            //Implement if help is done for the log settings.
        }
    }
}