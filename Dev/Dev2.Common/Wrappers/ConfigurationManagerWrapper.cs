using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Wrappers;
using Newtonsoft.Json;
using System;
using System.Configuration;

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
                    ServerSettingsData settings = null;
                    try
                    {
                        var text = _fileWrapper.ReadAllText("serverSettings.json");
                        settings = JsonConvert.DeserializeObject<ServerSettingsData>(text);
                    } catch {
                        settings = new ServerSettingsData();
                    }
                    var prop = typeof(ServerSettingsData).GetProperty(settingName);
                    var setting = prop.GetValue(settings)?.ToString();
                    //var setting = ConfigurationManager.AppSettings.Get(settingName)
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
                    var settings = Config.Server.Get();
                    var prop = typeof(ServerSettingsData).GetProperty(settingName);
                    prop.SetValue(settings, value);
                    settings.Save();
                }
            }
        }
    }

    static class ServerSettingsDataExtensionMethods
    {
        public static void Save(this ServerSettingsData data)
        {
            var fileWrapper = new FileWrapper();
            var json = JsonConvert.SerializeObject(data);
            fileWrapper.WriteAllText("serverSettings.json", json);
        }
    }
}
