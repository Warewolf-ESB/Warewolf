using Dev2.Common.Interfaces.Wrappers;
using System.Configuration;

namespace Dev2.Common.Wrappers
{
    class ConfigurationManagerWrapper : IConfigurationManager
    {
        readonly Configuration conf;
        private readonly object _settingLock = new object();
        public ConfigurationManagerWrapper()
        {
            conf = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        }
        public string this[string settingName, string defaultValue = null]
        {
            get
            {
                lock (_settingLock)
                {
                    return conf.AppSettings.Settings[settingName]?.Value ?? defaultValue;
                }
            }
            set
            {
                lock (_settingLock)
                {
                    var setting = conf.AppSettings.Settings[settingName];
                    if (setting is null)
                    {
                        conf.AppSettings.Settings.Add(settingName, value);
                    }
                    else
                    {
                        conf.AppSettings.Settings[settingName].Value = value;
                    }
                    conf.Save(ConfigurationSaveMode.Modified);
                }
            }
        }
    }
}
