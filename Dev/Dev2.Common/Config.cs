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
using Warewolf.Configuration;
using Warewolf.Data;
using Warewolf.Esb;
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
        public static ClusterSettings Cluster = new ClusterSettings();
        public static AuditingSettings Auditing = new AuditingSettings();
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
        public bool EnableDetailedLogging { get => _settings.EnableDetailedLogging ?? false; set => _settings.EnableDetailedLogging = value; }
        public ushort WebServerPort => _settings.WebServerPort ?? 0;
        public ushort WebServerSslPort => _settings.WebServerSslPort ?? 0;
        public string SslCertificateName => _settings.SslCertificateName;
        public bool CollectUsageStats => _settings.CollectUsageStats ?? false;
        public int DaysToKeepTempFiles => _settings.DaysToKeepTempFiles ?? 0;
        public int LogFlushInterval => _settings.LogFlushInterval ?? 200;

        public ServerSettings()
            : this(SettingsPath, new FileWrapper(), new DirectoryWrapper(), CustomContainer.Get<IClusterDispatcher>())
        { }
        public ServerSettings(string settingsPath, IFile fileWrapper, IDirectory directoryWrapper, IClusterDispatcher clusterDispatcher)
            : base(settingsPath, fileWrapper, directoryWrapper, clusterDispatcher)
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

        public delegate void VoidEventHandler();
        public event VoidEventHandler OnLogFlushPauseRequested;
        public event VoidEventHandler OnLogFlushResumeRequested;
    }

    public class StudioSettings : ConfigSettingsBase<StudioSettingsData>
    {
        public StudioSettings()
            : this(SettingsPath, new FileWrapper(), new DirectoryWrapper(), CustomContainer.Get<IClusterDispatcher>())
        {
        }
        protected StudioSettings(string settingsPath, IFile file, IDirectory directoryWrapper, IClusterDispatcher clusterDispatcher)
            : base(settingsPath, file, directoryWrapper, clusterDispatcher)
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


    public class AuditingSettings : ConfigSettingsBase<AuditingSettingsData>
    {
        public static string SettingsPath => Path.Combine(Config.AppDataPath, "Server Settings", "auditingSettings.json");

        public AuditingSettings()
            : this(SettingsPath, new FileWrapper(), new DirectoryWrapper(), CustomContainer.Get<IClusterDispatcher>())
        {
        }

        protected AuditingSettings(string settingsPath, IFileBase file, IDirectoryBase directoryWrapper, IClusterDispatcher clusterDispatcher)
            : base(settingsPath, file, directoryWrapper, clusterDispatcher)
        {
        }

        public string Endpoint => _settings.Endpoint;
    }

   public class ClusterSettings : ConfigSettingsBase<ClusterSettingsData>
    {
        public static string SettingsPath => Path.Combine(Config.AppDataPath, "Server Settings", "clusterSettings.json");
        public ClusterSettings()
            : this(SettingsPath, new FileWrapper(), new DirectoryWrapper(), CustomContainer.Get<IClusterDispatcher>())
        {
        }

        public ClusterSettings(string settingsPath, IFile file, IDirectory directoryWrapper, IClusterDispatcher clusterDispatcher)
            : base(settingsPath, file, directoryWrapper, clusterDispatcher)
        {
        }

        public string Key
        {
            get
            {
                if (_settings.Key is null)
                {
                    _settings.Key = Guid.NewGuid().ToString();
                    Save();
                }
                
                return _settings.Key;
            }
        }

        public string LeaderServerKey
        {
            get => _settings.LeaderServerKey;
            set
            {
                _settings.LeaderServerKey = value;
                Save();
            }
        }

        public NamedGuid LeaderServerResource
        {
            get => _settings.LeaderServerResource ?? new NamedGuid();
            set
            {
                _settings.LeaderServerResource = value;
                Save();
            }
        }


        public ClusterSettingsData Get()
        {
            var result = new ClusterSettingsData();
            foreach (var prop in typeof(ClusterSettingsData).GetProperties())
            {
                var thisProp = this.GetType().GetProperty(prop.Name);
                var value = thisProp.GetValue(this);
                prop.SetValue(result, value);
            }
            return result;
        }
    }
}
