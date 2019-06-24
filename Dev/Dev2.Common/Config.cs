#pragma warning disable
﻿using System;
using System.Data.SQLite;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Common.Wrappers;
using Newtonsoft.Json;

namespace Dev2.Common
{
    [ExcludeFromCodeCoverage]
    public class Config
    {
        public static readonly string AppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData, Environment.SpecialFolderOption.Create), "Warewolf");
        public static readonly string UserDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.Create), "Warewolf");

        private readonly object _configurationLock = new object();
        private readonly static Config _settings = new Config();

        public static ServerSettings Server = new ServerSettings();
        public static StudioSettings Studio = new StudioSettings();
    }


    public class ConfigSettingsBase<T> where T : class, new()
    {
        protected readonly string _settingsPath;
        protected readonly IDirectory _directoryWrapper;
        protected readonly IFile _fileWrapper;
        protected T _settings { get; private set; } = new T();

        protected ConfigSettingsBase(string settingsPath, IFile file, IDirectory directoryWrapper)
        {
            _settingsPath = settingsPath;
            _directoryWrapper = directoryWrapper;
            _fileWrapper = file;

            Load();
        }

        protected void Load()
        {
            if (_fileWrapper.Exists(_settingsPath))
            {
                var text = _fileWrapper.ReadAllText(_settingsPath);
                _settings = JsonConvert.DeserializeObject<T>(text);
            }
        }
        protected void Save()
        {
            _directoryWrapper.CreateIfNotExists(System.IO.Path.GetDirectoryName(_settingsPath));
            var text = JsonConvert.SerializeObject(this);
            _fileWrapper.WriteAllText(_settingsPath, text);
        }

        public void SaveIfNotExists()
        {
            if (!_fileWrapper.Exists(_settingsPath))
            {
                Save();
            }
        }
    }
    public class ServerSettings : ConfigSettingsBase<ServerSettingsData>
    {
        const int DELETE_TRIES_SLEEP = 5000;
        const int DELETE_TRIES_MAX = 30;

        public static string SettingsPath => Path.Combine(Config.AppDataPath, "Server Settings", "serverSettings.json");
        public static string DefaultAuditPath => Path.Combine(Config.AppDataPath, @"Audits");
        public string AuditFilePath
        {
            get => _settings.AuditFilePath ?? DefaultAuditPath;
            set
            {
                _settings.AuditFilePath = value;
                Save();
            }
        }
        public bool EnableDetailedLogging => _settings.EnableDetailedLogging ?? true;
        public ushort WebServerPort         => _settings.WebServerPort ?? 0;
        public ushort WebServerSslPort      => _settings.WebServerSslPort ?? 0;
        public string SslCertificateName    => _settings.SslCertificateName;
        public bool   CollectUsageStats     => _settings.CollectUsageStats ?? false;
        public int    DaysToKeepTempFiles   => _settings.DaysToKeepTempFiles ?? 0;
        public int LogFlushInterval => _settings.LogFlushInterval ?? 200;

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

        /*public void SaveIfNotExists()
        {
            if (!_fileWrapper.Exists(SettingsPath))
            {
                Save();
            }
        }*/

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
}
