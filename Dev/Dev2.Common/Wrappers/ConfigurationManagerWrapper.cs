using Dev2.Common.Interfaces.Wrappers;
using System;
using System.Configuration;

namespace Dev2.Common.Wrappers
{
    class ConfigurationManagerWrapper : IConfigurationManager
    {
        private readonly object _settingLock = new object();
        public string this[params string[] args]
        {
            get
            {
                if (args.Length <= 1)
                {
                    throw new ArgumentNullException();
                }
                var settingName = args[0];
                var defaultValue = args[1];
                lock (_settingLock)
                {
                    var setting = ConfigurationManager.AppSettings.Get(settingName);
                    return string.IsNullOrWhiteSpace(setting) ? defaultValue : setting;
                }
            }
            set
            {
                if (args.Length <= 0)
                {
                    throw new ArgumentNullException();
                }
                var settingName = args[0];
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
