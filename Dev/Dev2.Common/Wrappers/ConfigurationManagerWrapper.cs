using Dev2.Common.Interfaces.Wrappers;
using System.Configuration;

namespace Dev2.Common.Wrappers
{
    class ConfigurationManagerWrapper : IConfigurationManager
    {
        private readonly object _settingLock = new object();
        public string this[string settingName, string defaultValue = null]
        {
            get
            {
                lock (_settingLock)
                {
                    return ConfigurationManager.AppSettings.Get(settingName) ?? defaultValue;
                }
            }
            set
            {
                lock (_settingLock)
                {
                    var setting = ConfigurationManager.AppSettings.Get(settingName);
                    if (setting is null)
                    {
                        ConfigurationManager.AppSettings.Add(settingName, value);
                    }
                    else
                    {
                        ConfigurationManager.AppSettings.Set(settingName, value);
                    }
                }
            }
        }
    }
}
