#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
﻿using System;
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
                Server = new ServerSettings(m, new FileWrapper(), new DirectoryWrapper());
            }
        }
        public static void ConfigureSettings(IConfigurationManager m)
        {
            _settings.InnerConfigureSettings(m);
        }
    }

    public class ServerSettings
    {
        const int DELETE_TRIES_SLEEP = 5000;
        const int DELETE_TRIES_MAX = 30;

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
        private readonly IDirectory _directoryWrapper;
        private readonly IFile _fileWrapper;

        public ServerSettings()
            : this(new ConfigurationManagerWrapper(), new FileWrapper(), new DirectoryWrapper())
        {
        }

        public ServerSettings(IConfigurationManager manager, IFile file, IDirectory directoryWrapper)
        {
            this.manager = manager;
            _directoryWrapper = directoryWrapper;
            _fileWrapper = file;
        }

        public void SaveIfNotExists()
        {
            if (!_fileWrapper.Exists(Config.Server.SettingsPath))
            {
                Config.Server.Get().Save(_fileWrapper);
            }
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
