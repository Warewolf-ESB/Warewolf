#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Linq;
using Newtonsoft.Json;
using Warewolf.Esb;
using Warewolf.VirtualFileSystem;

namespace Warewolf.Configuration
{
    public class ConfigSettingsBase<T> where T : class, IHasChanged, new()
    {
        private readonly string _settingsPath;
        protected readonly IDirectoryBase _directoryWrapper;
        protected readonly IFileBase _fileWrapper;
        private readonly IClusterDispatcher _clusterDispatcher;
        protected T _settings { get; private set; } = new T();

        protected ConfigSettingsBase(string settingsPath, IFileBase file, IDirectoryBase directoryWrapper, IClusterDispatcher clusterDispatcher)
        {
            _settingsPath = settingsPath;
            _directoryWrapper = directoryWrapper;
            _fileWrapper = file;
            _clusterDispatcher = clusterDispatcher;

            Load();
        }

        private void Load()
        {
            if (!_fileWrapper.Exists(_settingsPath))
            {
                return;
            }
            var text = _fileWrapper.ReadAllText(_settingsPath);
            _settings = JsonConvert.DeserializeObject<T>(text);
            _settings.HasChanged = false;
        }
        protected void Save()
        {
            _directoryWrapper.CreateIfNotExists(System.IO.Path.GetDirectoryName(_settingsPath));
            var text = JsonConvert.SerializeObject(this);
            var changed = true;
            _fileWrapper.WriteAllText(_settingsPath, text);

            if (_settings.HasChanged)
            {
                var clusterDispatcher = _clusterDispatcher;
                if (clusterDispatcher is null)
                {
                    clusterDispatcher = CustomContainer.Get<IClusterDispatcher>();
                }
                clusterDispatcher.Write(this._settings);
                _settings.HasChanged = false;
            }
        }
        
        /// <summary>
        /// Gets a copy of the data in this instance as a T
        /// without saving changes
        /// </summary>
        /// <returns></returns>
        public T Get()
        {
            var result = new T();
            foreach (var prop in typeof(T).GetProperties())
            {
                if (prop.CustomAttributes.FirstOrDefault()?.AttributeType == typeof(JsonIgnoreAttribute))
                {
                    continue;
                }

                var thisProp = this.GetType().GetProperty(prop.Name);
                if (thisProp is null)
                {
                    throw new Exception($"config field {this._settings.GetType().Name}.{prop.Name} missing from {GetType().Name}");
                }
                var value = thisProp.GetValue(this);
                prop.SetValue(result, value);
            }
            return result;
        }

        public void SaveIfNotExists()
        {
            if (!_fileWrapper.Exists(_settingsPath))
            {
                Save();
            }
        }
    }
}
