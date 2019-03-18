#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
 
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Communication;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Services.Security;

namespace Dev2.Runtime.Security
{
    public class ServerSecurityService : SecurityServiceBase
    {
        bool _disposing;
        FileSystemWatcher _configWatcher = new FileSystemWatcher();

        public ServerSecurityService()
            : this(EnvironmentVariables.ServerSecuritySettingsFile)
        {
        }

        public ServerSecurityService(string fileName)
        {
            InitializeConfigWatcher(fileName);

        }

        protected override List<WindowsGroupPermission> ReadPermissions()
        {
            var reader = new SecurityRead();
            var result = reader.Execute(null, null);
            var serializer = new Dev2JsonSerializer();
            var securitySettingsTO = serializer.Deserialize<SecuritySettingsTO>(result);
            TimeOutPeriod = securitySettingsTO.CacheTimeout;
            return securitySettingsTO.WindowsGroupPermissions;
        }

        protected override void WritePermissions(List<WindowsGroupPermission> permissions)
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
            {
                _configWatcher.EnableRaisingEvents = enabled;
            }
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
            Dev2Logger.Info("SecurityService"+ methodName, GlobalConstants.WarewolfInfo);
        }

        protected override void LogEnd([CallerMemberName]string methodName = null)
        {
            Dev2Logger.Info("SecurityService"+ methodName, GlobalConstants.WarewolfInfo);
        }
    }
}
