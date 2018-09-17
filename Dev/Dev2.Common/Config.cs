using System;
using System.Configuration;
using System.IO;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Common.Wrappers;

namespace Dev2.Common
{
    public class Config
    {
        static readonly string DataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData, Environment.SpecialFolderOption.Create), "Warewolf");

        public static string AddOrUpdateAppSettings1()
        {
            var conf = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var setting = conf.AppSettings.Settings["AuditFilePath"];
            var sourceFilePath = Path.Combine(AppDataPath, "Audits");

            if (setting != null && !string.IsNullOrEmpty(setting.Value))
            {
                sourceFilePath = setting.Value;
            }
            else
            {
                if (setting is null)
                {
                    conf.AppSettings.Settings.Add("AuditFilePath", sourceFilePath);
                }
                else
                {
                    conf.AppSettings.Settings["AuditFilePath"].Value = sourceFilePath;
                }
                conf.Save(ConfigurationSaveMode.Modified);
            }

            return sourceFilePath;
        }

        public static string AppDataPath
        {
            get
            {
                if (!Directory.Exists(DataPath))
                {
                    Directory.CreateDirectory(DataPath);
                }
                return DataPath;
            }
        }
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
        readonly IConfigurationManager manager;

        public ServerSettings() : this(new ConfigurationManagerWrapper())
        {
        }
        public ServerSettings(IConfigurationManager manager)
        {
            this.manager = manager;
        }

        public string this[string settingName, string defaultValue = null]
        {
            get => manager[settingName, defaultValue];
            set => manager[settingName] = value;
        }
    }
}
