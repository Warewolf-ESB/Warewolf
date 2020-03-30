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

using Newtonsoft.Json;
using Warewolf.Esb;
using Warewolf.VirtualFileSystem;

namespace Warewolf.Configuration
{
    public class ConfigSettingsBase<T> where T : class, IHasChanged, new()
    {
        protected readonly string _settingsPath;
        protected readonly IDirectoryBase _directoryWrapper;
        protected readonly IFileBase _fileWrapper;
        protected readonly IClusterDispatcher _clusterDispatcher;
        protected T _settings { get; private set; } = new T();

        protected ConfigSettingsBase(string settingsPath, IFileBase file, IDirectoryBase directoryWrapper, IClusterDispatcher clusterDispatcher)
        {
            _settingsPath = settingsPath;
            _directoryWrapper = directoryWrapper;
            _fileWrapper = file;
            _clusterDispatcher = clusterDispatcher;

            Load();
        }

        protected void Load()
        {
            if (_fileWrapper.Exists(_settingsPath))
            {
                var text = _fileWrapper.ReadAllText(_settingsPath);
                _settings = JsonConvert.DeserializeObject<T>(text);
                _settings.HasChanged = false;
            }
        }
        protected void Save()
        {
            _directoryWrapper.CreateIfNotExists(System.IO.Path.GetDirectoryName(_settingsPath));
            var text = JsonConvert.SerializeObject(this);
            var changed = true;
            _fileWrapper.WriteAllText(_settingsPath, text);

            if (_settings.HasChanged)
            {
                _clusterDispatcher.Write(this);
                _settings.HasChanged = false;
            }
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
