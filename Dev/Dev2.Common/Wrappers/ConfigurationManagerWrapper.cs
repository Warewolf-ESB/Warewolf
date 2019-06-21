/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Wrappers;
using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;

namespace Dev2.Common.Wrappers
{
    [ExcludeFromCodeCoverage]
    class ConfigurationManagerWrapper<TSettingsData> : IConfigurationManager where TSettingsData : class, new()
    {
        public delegate TSettingsData LoadDelegate();
        public delegate void SaveDelegate(TSettingsData settingsData);

        private readonly object _settingLock = new object();
        private readonly FileWrapper _fileWrapper;
        private readonly string _settingsPath;
        readonly LoadDelegate _loadDelegate;
        readonly SaveDelegate _saveDelegate;
        public ConfigurationManagerWrapper(string settingsPath, LoadDelegate loadDelegate, SaveDelegate saveDelegate)
        {
            _fileWrapper = new FileWrapper();
            _settingsPath = settingsPath;
            _loadDelegate = loadDelegate;
            _saveDelegate = saveDelegate;
        }

        volatile TSettingsData _cache;
        public string this[string settingName]
        {
            get
            {
                var cacheCopy = _cache;
                if (cacheCopy != null)
                {
                    var prop = typeof(TSettingsData).GetProperty(settingName);
                    return prop.GetValue(cacheCopy)?.ToString();
                }

                lock (_settingLock)
                {
                    TSettingsData settings = null;
                    try
                    {
                        var text = _fileWrapper.ReadAllText(_settingsPath);
                        settings = JsonConvert.DeserializeObject<TSettingsData>(text);
                    } catch {
                        settings = new TSettingsData();
                    }

                    _cache = settings;

                    var prop = typeof(TSettingsData).GetProperty(settingName);
                    return prop.GetValue(settings)?.ToString();
                }
            }
            set
            {
                lock (_settingLock)
                {
                    var settings = _loadDelegate();
                    var prop = typeof(TSettingsData).GetProperty(settingName);
                    prop.SetValue(settings, value);
                    _saveDelegate(settings);

                    _cache = null;
                }
            }
        }
    }

}
