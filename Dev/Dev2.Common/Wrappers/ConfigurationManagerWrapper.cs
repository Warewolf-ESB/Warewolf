using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Wrappers;
using Newtonsoft.Json;
using System;

namespace Dev2.Common.Wrappers
{
    class ConfigurationManagerWrapper : IConfigurationManager
    {
        private readonly object _settingLock = new object();
        private readonly FileWrapper _fileWrapper;
        public ConfigurationManagerWrapper()
        {
            _fileWrapper = new FileWrapper();
        }

        volatile ServerSettingsData cache;
        public string this[string settingName]
        {
            get
            {
                var cacheCopy = cache;
                if (cacheCopy != null)
                {
                    var prop = typeof(ServerSettingsData).GetProperty(settingName);
                    return prop.GetValue(cacheCopy)?.ToString();
                }

                lock (_settingLock)
                {
                    ServerSettingsData settings = null;
                    try
                    {
                        var text = _fileWrapper.ReadAllText(Config.Server.SettingsPath);
                        settings = JsonConvert.DeserializeObject<ServerSettingsData>(text);
                    } catch {
                        settings = new ServerSettingsData();
                    }

                    cache = settings;

                    var prop = typeof(ServerSettingsData).GetProperty(settingName);
                    return prop.GetValue(settings)?.ToString();
                }
            }
            set
            {
                lock (_settingLock)
                {
                    var settings = Config.Server.Get();
                    var prop = typeof(ServerSettingsData).GetProperty(settingName);
                    prop.SetValue(settings, value);
                    settings.Save(_fileWrapper);

                    cache = null;
                }
            }
        }
    }

    static class ServerSettingsDataExtensionMethods
    {
        public static void Save(this ServerSettingsData data, IFile fileWrapper)
        {
            var json = JsonConvert.SerializeObject(data);
            fileWrapper.WriteAllText(Config.Server.SettingsPath, json);
        }
    }
}
