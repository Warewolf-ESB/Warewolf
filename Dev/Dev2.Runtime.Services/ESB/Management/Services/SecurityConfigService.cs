using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Dev2.Services.Security;
using Newtonsoft.Json;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class SecurityConfigService : DisposableObject, ISecurityConfigService
    {
        public const string FileName = "secure.config";

        FileSystemWatcher _configWatcher = new FileSystemWatcher();

        public SecurityConfigService()
        {
            InitializeConfigWatcher();
            InitializePermissions();
        }

        public event EventHandler Changed;

        void InitializePermissions()
        {
            var reader = new SecurityRead();
            var json = reader.Execute(null, null);
            Permissions = JsonConvert.DeserializeObject<List<WindowsGroupPermission>>(json);
        }

        public IReadOnlyList<WindowsGroupPermission> Permissions { get; private set; }

        void InitializeConfigWatcher()
        {
            _configWatcher.Path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            // Watch for changes in LastAccess and LastWrite times, and the renaming of files or directories. 
            _configWatcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
                                          | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            // Only watch secure.config
            _configWatcher.Filter = FileName;

            // Add event handlers.
            _configWatcher.Changed += OnChanged;
            _configWatcher.Created += OnChanged;
            _configWatcher.Deleted += OnChanged;
            _configWatcher.Renamed += OnRenamed;

            // Begin watching.
            _configWatcher.EnableRaisingEvents = true;
        }

        void OnChanged(object sender, FileSystemEventArgs e)
        {
            RaiseChanged();
        }

        void OnRenamed(object sender, RenamedEventArgs e)
        {
            RaiseChanged();
        }

        void RaiseChanged()
        {
            InitializePermissions();
            if(Changed != null)
            {
                Changed(this, EventArgs.Empty);
            }
        }

        protected override void OnDisposed()
        {
            if(_configWatcher != null)
            {
                _configWatcher.EnableRaisingEvents = false;
                _configWatcher.Changed -= OnChanged;
                _configWatcher.Created -= OnChanged;
                _configWatcher.Deleted -= OnChanged;
                _configWatcher.Renamed -= OnRenamed;
                _configWatcher.Dispose();
                _configWatcher = null;
            }
            Permissions = null;
        }
    }
}