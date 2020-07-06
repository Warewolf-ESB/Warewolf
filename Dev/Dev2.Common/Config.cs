#pragma warning disable
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
        public static readonly string AppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData, Environment.SpecialFolderOption.Create), "Warewolf");
        public static readonly string UserDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.Create), "Warewolf");

        public static ServerSettings Server = new ServerSettings();
        public static StudioSettings Studio = new StudioSettings();
        public static AuditingSettings Auditing = new AuditingSettings();
        public static LegacySettings Legacy = new LegacySettings();
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
                Save();
            }
        }
        public string Sink
        {
            get => GetSink();
            set
            {
                _settings.Sink = value;
                Save();
            }
        }

        private string GetSink()
        {
            if (_settings.Sink == null && _settings.AuditFilePath != null)
            {
                return nameof(LegacySettingsData);
            }

            if (_settings.Sink != null )
            {
                {
                    return _settings.Sink;
                }
            }
            return DefaultSink;
        }

        [Obsolete("AuditFilePath is deprecated. It will be deleted in future releases.")]
        public string AuditFilePath => _settings.AuditFilePath ?? LegacySettings.DefaultAuditPath;

        public ushort WebServerPort => _settings.WebServerPort ?? 0;
        public ushort WebServerSslPort => _settings.WebServerSslPort ?? 0;
        public string SslCertificateName => _settings.SslCertificateName;
        public bool CollectUsageStats => _settings.CollectUsageStats ?? false;
        public int DaysToKeepTempFiles => _settings.DaysToKeepTempFiles ?? 0;
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
                Save();
            }
        }

        private string GetAuditFilePath()
        {
            if (_settings.AuditFilePath == null && Config.Server.AuditFilePath != null)
            {
                return Config.Server.AuditFilePath;
            }

            if (_settings.AuditFilePath != null )
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

        private void TryDeleteOldLogFile(IFileBase _fileWrapper, string source)
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
                Save();
            }
        }
        public NamedGuidWithEncryptedPayload LoggingDataSource
        {
            get => _settings.LoggingDataSource ?? new NamedGuidWithEncryptedPayload();
            set
            {
                _settings.LoggingDataSource = value;
                Save();
            }
        }

        public string Endpoint => _settings.Endpoint ?? DefaultEndpoint;
    }
}