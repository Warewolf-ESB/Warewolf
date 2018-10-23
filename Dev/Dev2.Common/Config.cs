using System;
using System.Configuration;
using System.IO;
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
    }
}
