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
        public string AuditPath => Path.Combine(Config.AppDataPath, @"Audits");

        readonly IConfigurationManager manager;

        public ServerSettings() : this(new ConfigurationManagerWrapper())
        {
        }
        public ServerSettings(IConfigurationManager manager)
        {
            this.manager = manager;
        }

        public string this[string settingName]
        {
            get
            {
                var defaultValue = GetDefault(settingName, default(string));
                return manager[settingName, defaultValue];
            }
            set => manager[settingName, null] = value;
        }

        public ServerSettingsData Get()
        {
            var result = new ServerSettingsData();
            foreach (var prop in typeof(ServerSettingsData).GetProperties())
            {
                var value = this[prop.Name];
                if (value is null)
                {
                    prop.SetValue(result, GetDefault(prop.Name, value));
                    continue;
                }
                switch (Type.GetTypeCode(prop.PropertyType))
                {
                    case TypeCode.UInt16:
                        prop.SetValue(result, ushort.Parse(value));
                        break;
                    case TypeCode.String:
                        prop.SetValue(result, value);
                        break;
                    case TypeCode.Int32:
                        prop.SetValue(result, int.Parse(value));
                        break;
                    case TypeCode.Boolean:
                        prop.SetValue(result, bool.Parse(value));
                        break;
                    default:
                        throw new Exception($"unhandled setting type: {prop.PropertyType.Name}");
                }
            }
            return result;
        }

        static T GetDefault<T>(string key, T value)
        {
            if (Type.GetTypeCode(typeof(T)) == TypeCode.String && key == "AuditFilePath" && string.IsNullOrWhiteSpace(value as string))
            {
                return (T)(object)(Config.Server.AuditPath);
            }
            return  value;
        }
    }
}
