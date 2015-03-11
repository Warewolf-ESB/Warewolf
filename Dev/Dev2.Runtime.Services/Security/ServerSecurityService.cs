 
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using Dev2.Common;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Communication;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Services.Security;

namespace Dev2.Runtime.Security
{
    public class ServerSecurityService : SecurityServiceBase
    {
        public const string FileName = "secure.config";
        private bool _disposing;
        FileSystemWatcher _configWatcher = new FileSystemWatcher();

        public ServerSecurityService()
            : this(FileName)
        {
        }

        public ServerSecurityService(string fileName)
        {
            InitializeConfigWatcher(fileName);

        }

        protected override List<IWindowsGroupPermission> ReadPermissions()
        {
            var reader = new SecurityRead();
            var result = reader.Execute(null, null);
            var serializer = new Dev2JsonSerializer();
            SecuritySettingsTO securitySettingsTO = serializer.Deserialize<SecuritySettingsTO>(result);
            if (securitySettingsTO == null)
            {
                return null;
            }
            TimeOutPeriod = securitySettingsTO.CacheTimeout;
            return securitySettingsTO.WindowsGroupPermissions;
        }

        protected override void WritePermissions(List<IWindowsGroupPermission> permissions)
        {
            SecurityWrite.Write(new SecuritySettingsTO(permissions));
        }

        public void InitializeConfigWatcher(string fileName)
        {
            if(_configWatcher == null)
            {
                _configWatcher = new FileSystemWatcher();
            }
            _configWatcher.Path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            // Watch for changes in LastAccess and LastWrite times, and the renaming of files or directories. 
            _configWatcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
                                          | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            // Only watch secure.config
            _configWatcher.Filter = fileName;

            // Add event handlers.
            _configWatcher.Changed += OnFileChanged;
            _configWatcher.Created += OnFileChanged;
            _configWatcher.Deleted += OnFileChanged;
            _configWatcher.Renamed += OnFileRenamed;

            // Begin watching.
            _configWatcher.EnableRaisingEvents = true;
        }

        protected virtual void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            LogStart();
            OnFileChanged();
            LogEnd();
        }

        protected virtual void OnFileRenamed(object sender, RenamedEventArgs e)
        {
            LogStart();
            OnFileChanged();
            LogEnd();
        }

        void OnFileChanged()
        {
            try
            {
                // Disable raising events otherwise the Changed event is raised twice
                OnFileChangedEnableRaisingEvents(false);
                Read();
            }
            finally
            {
                OnFileChangedEnableRaisingEvents(true);
            }
        }

        protected virtual void OnFileChangedEnableRaisingEvents(bool enabled)
        {
            if (!_disposing)
            _configWatcher.EnableRaisingEvents = enabled;
        }

        protected override void OnDisposed()
        {
            _disposing = true;
            if (_configWatcher != null && !_isDisposed)
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

        protected override void LogStart([CallerMemberName]string methodName = null)
        {
            // ReSharper disable once ExplicitCallerInfoArgument
            Dev2Logger.Log.Info("SecurityService"+ methodName);
        }

        protected override void LogEnd([CallerMemberName]string methodName = null)
        {
            // ReSharper disable once ExplicitCallerInfoArgument
            Dev2Logger.Log.Info("SecurityService"+ methodName);
        }
    }
}
