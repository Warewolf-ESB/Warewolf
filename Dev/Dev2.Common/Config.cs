#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2022 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SQLite;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Common.Wrappers;
using Dev2.Data.Interfaces.Enums;
using Warewolf.Configuration;
using Warewolf.Data;
using Warewolf.VirtualFileSystem;

namespace Dev2.Common
{
    [ExcludeFromCodeCoverage]
    public class Config
    {
        private static string _appDataPath;
        private static string _userDataPath;

        public static string AppDataPath => _appDataPath ?? (_appDataPath = GetDirectory(GlobalConstants.ServerPathKey, Environment.SpecialFolder.CommonApplicationData));
        public static string UserDataPath => _userDataPath ?? (_userDataPath = GetDirectory(GlobalConstants.UserPathKey, Environment.SpecialFolder.LocalApplicationData));

        private static string GetDirectory(string key, Environment.SpecialFolder defaultPath)
        {
            // First try environment variable; skip Windows-style paths when running on Linux
            string path = Environment.GetEnvironmentVariable(key);

            // Fall back to ConfigurationManager.AppSettings.
            // This can throw in hosts that lack a classic app-config system (e.g. an Azure
            // Functions isolated-worker process), so treat any failure as "no value" and let
            // the remaining fallback tiers below handle it instead of crashing this static
            // initializer (Config's static fields are constructed eagerly).
            if (string.IsNullOrEmpty(path) || !Path.IsPathFullyQualified(path))
            {
                try
                {
                    path = ConfigurationManager.AppSettings[key];
                }
                catch (Exception)
                {
                    path = null;
                }
            }

            // Fall back to special folder path
            if (string.IsNullOrEmpty(path) || !Path.IsPathFullyQualified(path))
            {
                path = Environment.GetFolderPath(defaultPath, Environment.SpecialFolderOption.Create);
            }

            // Ultimate fallback: temp directory (always writable, e.g. in CI containers)
            if (string.IsNullOrEmpty(path) || !Path.IsPathFullyQualified(path))
            {
                path = Path.GetTempPath();
            }

            return Path.Combine(path, GlobalConstants.Warewolf);
        }

        public static ServerSettings Server = new ServerSettings();
        public static StudioSettings Studio = new StudioSettings();
        public static AuditingSettings Auditing = new AuditingSettings();
        public static LegacySettings Legacy = new LegacySettings();
        public static PersistenceSettings Persistence = new PersistenceSettings();
        public static ChatbotSettings Chatbot = new ChatbotSettings();
    }
    public class PersistenceSettings : ConfigSettingsBase<PersistenceSettingsData>
    {
        public static string DefaultDashboardPort => "5001";
        public static string DefaultDashboardHostname => "http://localhost";
        public static string DefaultDashboardName = "hangfire";
        public static string DefaultServerName = "";

        public PersistenceSettings()
            : this(SettingsPath, new FileWrapper(), new DirectoryWrapper())
        {
        }

        public PersistenceSettings(string settingsPath, IFile file, IDirectory directoryWrapper)
            : base(settingsPath, file, directoryWrapper)
        {
        }

        public static string SettingsPath => Path.Combine(Config.AppDataPath, "Server Settings", "persistencesettings.json");

        public PersistenceSettingsData Get()
        {
            var result = new PersistenceSettingsData();
            foreach (var prop in typeof(PersistenceSettingsData).GetProperties())
            {
                var thisProp = this.GetType().GetProperty(prop.Name);
                var value = thisProp.GetValue(this);
                prop.SetValue(result, value);
            }

            return result;
        }
        public bool EncryptDataSource
        {
            get => _settings?.EncryptDataSource ?? false;
            set
            {
                _settings.EncryptDataSource = value;
                Save();
            }
        }
        public NamedGuidWithEncryptedPayload PersistenceDataSource
        {
            get => _settings.PersistenceDataSource ?? new NamedGuidWithEncryptedPayload();
            set
            {
                _settings.PersistenceDataSource = value;
                Save();
            }
        }
        public bool Enable
        {
            get => _settings?.Enable ?? false;
            set
            {
                _settings.Enable = value;
                Save();
            }
        }
        public string PersistenceScheduler
        {
            get => _settings.PersistenceScheduler;
            set
            {
                _settings.PersistenceScheduler = value;
                Save();
            }
        }
        public bool PrepareSchemaIfNecessary
        {
            get => _settings?.PrepareSchemaIfNecessary ?? true;
            set
            {
                _settings.PrepareSchemaIfNecessary = value;
                Save();
            }
        }

        public bool UseAsServer
        {
            get => _settings?.UseAsServer ?? true;
            set
            {
                _settings.UseAsServer = value;
                Save();
            }
        }

        public string DashboardHostname
        {
            get => _settings.DashboardHostname ?? DefaultDashboardHostname;
            set
            {
                _settings.DashboardHostname = value;
                Save();
            }
        }
        public string DashboardPort
        {
            get => _settings.DashboardPort ?? DefaultDashboardPort;
            set
            {
                _settings.DashboardPort = value;
                Save();
            }
        }
        public string DashboardName
        {
            get => _settings.DashboardName ?? DefaultDashboardName;
            set
            {
                _settings.DashboardName = value;
                Save();
            }
        }
        public string ServerName
        {
            get => _settings.ServerName ?? DefaultServerName;
            set
            {
                _settings.ServerName = value;
                Save();
            }
        }

    }
    public class ServerSettings : ConfigSettingsBase<ServerSettingsData>
    {
        public static string SettingsPath => Path.Combine(Config.AppDataPath, "Server Settings", "serverSettings.json");
        public static string DefaultSink => nameof(LegacySettingsData);

        public bool EnableDetailedLogging
        {
            get => _settings.EnableDetailedLogging ?? true;
            set => _settings.EnableDetailedLogging = value;
        }
        public string ExecutionLogLevel
        {
            get => _settings.ExecutionLogLevel ?? LogLevel.DEBUG.ToString();
            set
            {
                _settings.ExecutionLogLevel = value;
            }
        }

        public string Sink
        {
            get => GetSink();
            set
            {
                _settings.Sink = value;
            }
        }

        private string GetSink()
        {
            if (_settings.Sink != null)
            {
                {
                    return _settings.Sink;
                }
            }
            return DefaultSink;
        }

        [Obsolete("AuditFilePath is deprecated. It will be deleted in future releases.")]
        public string AuditFilePath => LegacySettings.DefaultAuditPath;

        public bool IncludeEnvironmentVariable
        {
            get => _settings.IncludeEnvironmentVariable;
            set
            {
                _settings.IncludeEnvironmentVariable = value;
            }
        }

        public ushort WebServerPort => _settings.WebServerPort ?? 0;
        public ushort WebServerSslPort => _settings.WebServerSslPort ?? 0;
        public string SslCertificateName => _settings.SslCertificateName;
        public bool CollectUsageStats => _settings.CollectUsageStats ?? false;
        public int DaysToKeepTempFiles => _settings.DaysToKeepTempFiles ?? 0;
        public int LogFlushInterval => _settings.LogFlushInterval ?? 200;
        public bool EnablePerformanceCounters => _settings.EnablePerformanceCounters ?? false;

        public ServerSettings()
            : this(SettingsPath, new FileWrapper(), new DirectoryWrapper())
        { }
        public ServerSettings(string settingsPath, IFile fileWrapper, IDirectory directoryWrapper)
            : base(settingsPath, fileWrapper, directoryWrapper)
        {
        }

        public ServerSettingsData Get()
        {
            var result = new ServerSettingsData();
            foreach (var prop in typeof(ServerSettingsData).GetProperties())
            {
                var thisProp = this.GetType().GetProperty(prop.Name);
                var value = thisProp.GetValue(this);
                prop.SetValue(result, value);
            }

            return result;
        }
    }

    public class StudioSettings : ConfigSettingsBase<StudioSettingsData>
    {
        public StudioSettings()
            : this(SettingsPath, new FileWrapper(), new DirectoryWrapper())
        {
        }

        protected StudioSettings(string settingsPath, IFile file, IDirectory directoryWrapper)
            : base(settingsPath, file, directoryWrapper)
        {
        }

        public static string SettingsPath => Path.Combine(Config.UserDataPath, "Studio", "studio_settings.json");

        public int ConnectTimeout => _settings.ConnectTimeout ?? 10000;

        public StudioSettingsData Get()
        {
            var result = new StudioSettingsData();
            foreach (var prop in typeof(StudioSettingsData).GetProperties())
            {
                var thisProp = this.GetType().GetProperty(prop.Name);
                var value = thisProp.GetValue(this);
                prop.SetValue(result, value);
            }

            return result;
        }
    }

    public class LegacySettings : ConfigSettingsBase<LegacySettingsData>
    {
        const int DELETE_TRIES_SLEEP = 5000;
        const int DELETE_TRIES_MAX = 30;

        public static string SettingsPath => Path.Combine(Config.AppDataPath, "Server Settings", "legacySettings.json");
        public static string DefaultAuditPath => Path.Combine(Config.AppDataPath, @"Audits");
        public static string DefaultEndpoint => "ws://127.0.0.1:5000/ws";

        public const long DefaultAuditLogMaxSize = 2000;

        public LegacySettings()
            : this(SettingsPath, new FileWrapper(), new DirectoryWrapper())
        {
        }

        public LegacySettings(string settingsPath, IFileBase file, IDirectoryBase directoryWrapper)
            : base(settingsPath, file, directoryWrapper)
        {
        }

        public LegacySettingsData Get()
        {
            var result = new LegacySettingsData();
            foreach (var prop in typeof(LegacySettingsData).GetProperties())
            {
                var thisProp = this.GetType().GetProperty(prop.Name);
                var value = thisProp.GetValue(this);
                prop.SetValue(result, value);
            }

            return result;
        }

        public string AuditFilePath
        {
            get => GetAuditFilePath();
            set
            {
                _settings.AuditFilePath = value;
            }
        }

        public long AuditLogMaxSize
        {
            get => (_settings.AuditLogMaxSize == 0) ? DefaultAuditLogMaxSize : _settings.AuditLogMaxSize;
            set
            {
                _settings.AuditLogMaxSize = value;
            }
        }



        private string GetAuditFilePath()
        {
            if (_settings.AuditFilePath != null)
            {
                {
                    return _settings.AuditFilePath;
                }
            }
            return DefaultAuditPath;
        }

        public string Endpoint
        {
            get => _settings.Endpoint ?? DefaultEndpoint;
        }

        public bool IncludeEnvironmentVariable
        {
            get => _settings.IncludeEnvironmentVariable;
            set
            {
                _settings.IncludeEnvironmentVariable = value;
            }
        }

        public bool SaveLoggingPath(string auditsFilePath)
        {
            var sourceFilePath = this.AuditFilePath;
            if (sourceFilePath != auditsFilePath)
            {
                var source = Path.Combine(sourceFilePath, "auditDB.db");
                if (_fileWrapper.Exists(source))
                {
                    var destination = Path.Combine(auditsFilePath, "auditDB.db");
                    _directoryWrapper.CreateIfNotExists(auditsFilePath);

                    try
                    {
                        OnLogFlushPauseRequested?.Invoke();

                        _fileWrapper.Copy(source, destination);
                        this.AuditFilePath = auditsFilePath;
                        TryDeleteOldLogFile(_fileWrapper, source);
                    }
                    finally
                    {
                        OnLogFlushResumeRequested?.Invoke();
                    }

                    return true;
                }
                else
                {
                    this.AuditFilePath = auditsFilePath;
                }
            }

            return false;
        }

        private void TryDeleteOldLogFile(IFileBase _wrapper, string source)
        {
            new Thread(() =>
            {
                int tries = 0;
                while (_wrapper.Exists(source))
                {
                    try
                    {
                        SQLiteConnection.ClearAllPools();
                        GC.Collect();

                        _wrapper.Delete(source);
                        break;
                    }
                    catch (Exception)
                    {
                        if (tries++ >= DELETE_TRIES_MAX)
                        {
                            throw;
                        }

                        // try until we delete the file at least once
                        Thread.Sleep(DELETE_TRIES_SLEEP);
                    }
                }
            }).Start();
        }

        public event VoidEventHandler OnLogFlushPauseRequested;
        public event VoidEventHandler OnLogFlushResumeRequested;

        public delegate void VoidEventHandler();
    }

    public class AuditingSettings : ConfigSettingsBase<AuditingSettingsData>
    {
        public static string SettingsPath => Path.Combine(Config.AppDataPath, "Server Settings", "auditingSettings.json");

        public static string DefaultEndpoint => "ws://127.0.0.1:5000/ws";

        public AuditingSettings()
            : this(SettingsPath, new FileWrapper(), new DirectoryWrapper())
        {
        }

        public AuditingSettings(string settingsPath, IFileBase file, IDirectoryBase directoryWrapper)
            : base(settingsPath, file, directoryWrapper)
        {
        }

        public AuditingSettingsData Get()
        {
            var result = new AuditingSettingsData();
            foreach (var prop in typeof(AuditingSettingsData).GetProperties())
            {
                var thisProp = this.GetType().GetProperty(prop.Name);
                var value = thisProp.GetValue(this);
                prop.SetValue(result, value);
            }
            return result;
        }
        public bool EncryptDataSource
        {
            get => _settings.EncryptDataSource;
            set
            {
                _settings.EncryptDataSource = value;
                //Save();
            }
        }
        public NamedGuidWithEncryptedPayload LoggingDataSource
        {
            get => _settings.LoggingDataSource ?? new NamedGuidWithEncryptedPayload();
            set
            {
                _settings.LoggingDataSource = value;
                //Save();
            }
        }

        public string Endpoint => _settings.Endpoint ?? DefaultEndpoint;

        public bool IncludeEnvironmentVariable
        {
            get => _settings.IncludeEnvironmentVariable;
            set
            {
                _settings.IncludeEnvironmentVariable = value;
            }
        }
    }

    public class ChatbotSettings : ConfigSettingsBase<ChatbotSettingsData>
    {
        public static string SettingsPath => Path.Combine(Config.AppDataPath, "Server Settings", "chatbotSettings.json");

        public ChatbotSettings()
            : this(SettingsPath, new FileWrapper(), new DirectoryWrapper())
        {
        }

        public ChatbotSettings(string settingsPath, IFile file, IDirectory directoryWrapper)
            : base(settingsPath, file, directoryWrapper)
        {
        }

        public ChatbotSettingsData Get()
        {
            var result = new ChatbotSettingsData();
            foreach (var prop in typeof(ChatbotSettingsData).GetProperties())
            {
                var thisProp = this.GetType().GetProperty(prop.Name);
                var value = thisProp?.GetValue(this);
                prop.SetValue(result, value);
            }

            return result;
        }

        public NamedGuidWithEncryptedPayload ChatbotSource
        {
            get => _settings?.ChatbotSource ?? new NamedGuidWithEncryptedPayload();
            set
            {
                _settings.ChatbotSource = value;
                Save();
            }
        }

        public bool IncludeSystemLog
        {
            get => _settings?.IncludeSystemLog ?? true;
            set
            {
                _settings.IncludeSystemLog = value;
                Save();
            }
        }

        public bool LoadResourcesAsXaml
        {
            get => _settings?.LoadResourcesAsXaml ?? true;
            set
            {
                _settings.LoadResourcesAsXaml = value;
                Save();
            }
        }

        public int NumberOfLogLines
        {
            get => _settings?.NumberOfLogLines ?? 1000;
            set
            {
                _settings.NumberOfLogLines = value;
                Save();
            }
        }

        public List<Guid> SelectedResourceIds
        {
            get => _settings?.SelectedResourceIds ?? new List<Guid>();
            set
            {
                _settings.SelectedResourceIds = value ?? new List<Guid>();
                Save();
            }
        }

        public string UserMessageColor
        {
            get => _settings?.UserMessageColor ?? "#ff6600";
            set
            {
                _settings.UserMessageColor = value ?? "#ff6600";
                Save();
            }
        }

        public string UserMessageTextColor
        {
            get => _settings?.UserMessageTextColor ?? "#ffffff";
            set
            {
                _settings.UserMessageTextColor = value ?? "#ffffff";
                Save();
            }
        }

        public string BotMessageColor
        {
            get => _settings?.BotMessageColor ?? "#f8f9fa";
            set
            {
                _settings.BotMessageColor = value ?? "#f8f9fa";
                Save();
            }
        }

        public string BotMessageTextColor
        {
            get => _settings?.BotMessageTextColor ?? "#333333";
            set
            {
                _settings.BotMessageTextColor = value ?? "#333333";
                Save();
            }
        }

        public int SlidingWindowSummaryLength
        {
            get => _settings?.SlidingWindowSummaryLength ?? 200;
            set
            {
                _settings.SlidingWindowSummaryLength = value > 0 ? value : 200;
                Save();
            }
        }

        public bool EnableSlidingWindowTrimming
        {
            get => _settings?.EnableSlidingWindowTrimming ?? false;
            set
            {
                _settings.EnableSlidingWindowTrimming = value;
                Save();
            }
        }
    }
}
