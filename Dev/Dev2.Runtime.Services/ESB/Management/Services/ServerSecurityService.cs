using System.IO;
using System.Reflection;
using Dev2.Services.Security;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class ServerSecurityService : SecurityServiceBase
    {
        public const string FileName = "secure.config";

        FileSystemWatcher _configWatcher = new FileSystemWatcher();

        public ServerSecurityService()
        {
            InitializeConfigWatcher();
        }

        protected override string ReadPermissions()
        {
            var reader = new SecurityRead();
            return reader.Execute(null, null);
        }

        void InitializeConfigWatcher()
        {
            _configWatcher.Path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            // Watch for changes in LastAccess and LastWrite times, and the renaming of files or directories. 
            _configWatcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
                                          | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            // Only watch secure.config
            _configWatcher.Filter = FileName;

            // Add event handlers.
            _configWatcher.Changed += OnFileChanged;
            _configWatcher.Created += OnFileChanged;
            _configWatcher.Deleted += OnFileChanged;
            _configWatcher.Renamed += OnFileRenamed;

            // Begin watching.
            _configWatcher.EnableRaisingEvents = true;
        }

        void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            Read();
        }

        void OnFileRenamed(object sender, RenamedEventArgs e)
        {
            Read();
        }

        protected override void OnDisposed()
        {
            if(_configWatcher != null)
            {
                _configWatcher.EnableRaisingEvents = false;
                _configWatcher.Changed -= OnFileChanged;
                _configWatcher.Created -= OnFileChanged;
                _configWatcher.Deleted -= OnFileChanged;
                _configWatcher.Renamed -= OnFileRenamed;
                _configWatcher.Dispose();
                _configWatcher = null;
            }
        }
    }
}