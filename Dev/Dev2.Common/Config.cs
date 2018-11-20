using System;
using System.Data.SQLite;
using System.IO;
using System.Threading;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Common.Wrappers;

namespace Dev2.Common
{
    public class Config
    {
        public static readonly string AppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData, Environment.SpecialFolderOption.Create), "Warewolf");

        private readonly object _configurationLock = new object();
        private readonly static Config _settings = new Config();

        public static ServerSettings Server = new ServerSettings();
        private void InnerConfigureSettings(IConfigurationManager m)
        {
            lock (_configurationLock)
            {
                Server = new ServerSettings(m);
            }
        }
        public static void ConfigureSettings(IConfigurationManager m)
        {
            _settings.InnerConfigureSettings(m);
        }
    }

    public class ServerSettings
    {
        const int DELETE_TRIES_SLEEP = 50;
        const int DELETE_TRIES_MAX = 50;

        public string SettingsPath => Path.Combine(Config.AppDataPath, "Server Settings", "serverSettings.json");
        public string DefaultAuditPath => Path.Combine(Config.AppDataPath, @"Audits");
        public string AuditFilePath
        {
            get => manager[nameof(AuditFilePath)] ?? DefaultAuditPath;
            set
            {
                manager[nameof(AuditFilePath)] = value;
            }
        }
        public bool EnableDetailedLogging => StringToBool(manager[nameof(EnableDetailedLogging)], true);

        public ushort WebServerPort         => StringToUShort(manager[nameof(WebServerPort)], 0);
        public ushort WebServerSslPort      => StringToUShort(manager[nameof(WebServerSslPort)], 0);
        public string SslCertificateName    => manager[nameof(SslCertificateName)];
        public bool   CollectUsageStats     => StringToBool(manager[nameof(CollectUsageStats)], false);
        public int    DaysToKeepTempFiles   => StringToInt(manager[nameof(DaysToKeepTempFiles)], 0);

        public int LogFlushInterval => StringToInt(manager[nameof(LogFlushInterval)], 200);

        private static bool StringToBool(string value, bool defaultValue) {
            if (value is null)
            {
                return defaultValue;
            }
            return value.Trim().ToLower() == "true";
        }
        private static ushort StringToUShort(string value, ushort defaultValue)
        {
            if (value is null)
            {
                return defaultValue;
            }
            return ushort.Parse(value);
        }
        private static int StringToInt(string value, int defaultValue)
        {
            if (value is null)
            {
                return defaultValue;
            }
            return int.Parse(value);
        }

        readonly IConfigurationManager manager;

        public ServerSettings() : this(new ConfigurationManagerWrapper())
        {
        }
        public ServerSettings(IConfigurationManager manager)
        {
            this.manager = manager;
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

        public bool SaveLoggingPath(string auditsFilePath)
        {
            var sourceFilePath = Config.Server.AuditFilePath;
            IFile _fileWrapper = new FileWrapper();
            var directoryWrapper = new DirectoryWrapper();

            if (sourceFilePath != auditsFilePath)
            {
                var source = Path.Combine(sourceFilePath, "auditDB.db");
                if (_fileWrapper.Exists(source))
                {
                    var destination = Path.Combine(auditsFilePath, "auditDB.db");
                    directoryWrapper.CreateIfNotExists(auditsFilePath);

                    try
                    {
                        OnLogFlushPauseRequested?.Invoke();

                        _fileWrapper.Copy(source, destination);
                        Config.Server.AuditFilePath = auditsFilePath;
                        TryDeleteOldLogFile(_fileWrapper, source);
                    }
                    finally
                    {
                        OnLogFlushResumeRequested?.Invoke();
                    }
                    
                    return true;
                }
            }

            return false;
        }

        private void TryDeleteOldLogFile(IFile _fileWrapper, string source)
        {
            new Thread(() =>
            {
                int tries = 0;
                while (_fileWrapper.Exists(source))
                {
                    try
                    {
                        SQLiteConnection.ClearAllPools();
                        GC.Collect();
                        
                        _fileWrapper.Delete(source);
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

        public delegate void VoidEventHandler();
        public event VoidEventHandler OnLogFlushPauseRequested;
        public event VoidEventHandler OnLogFlushResumeRequested;
    }
}
