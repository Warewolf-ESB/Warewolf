#pragma warning disable
 /*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Common.Wrappers;
using log4net.Config;
using System;
using System.Configuration;
using System.IO;
using System.Security.Principal;

namespace Dev2
{
    public interface IServerEnvironmentPreparer : IDisposable
    {
        void PrepareEnvironment();
    }
    public class ServerEnvironmentPreparer : IServerEnvironmentPreparer
    {
        readonly IFile _fileWrapper;
        readonly IDirectory _directoryWrapper;
        readonly ITempFileDeleter _tempFileDeleter;

        public ServerEnvironmentPreparer()
            :this(new FileWrapper(), new DirectoryWrapper())
        {
        }
        private ServerEnvironmentPreparer(IFile fileWrapper, IDirectory directoryWrapper)
            :this(new TempFileDeleter(directoryWrapper, new TimerWrapperFactory()), fileWrapper, directoryWrapper)
        {
        }

        public ServerEnvironmentPreparer(ITempFileDeleter tempFileDeleter, IFile fileWrapper, IDirectory directoryWrapper)
        {
            _tempFileDeleter = tempFileDeleter;
            _fileWrapper = fileWrapper;
            _directoryWrapper = directoryWrapper;
        }

        public void PrepareEnvironment()
        {
            var settingsConfigFile = EnvironmentVariables.ServerLogSettingsFile;
            InitializeSettingsFiles(settingsConfigFile);

            PrepareLogging(settingsConfigFile);

            Common.Utilities.ServerUser = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            SetupTempCleanupSetting();

            Config.Server.SaveIfNotExists();
            Config.Auditing.SaveIfNotExists();
            Config.Legacy.SaveIfNotExists();
        }

        protected void PrepareLogging(string settingsConfigFile)
        {
            try
            {

                Dev2Logger.AddEventLogging(settingsConfigFile, "Warewolf Server");
                Dev2Logger.UpdateFileLoggerToProgramData(settingsConfigFile);
                XmlConfigurator.ConfigureAndWatch(new FileInfo(settingsConfigFile));
            }
            catch (Exception e)
            {
                Dev2Logger.Error("Error in startup.", e, GlobalConstants.WarewolfError);
            }
        }

        protected void InitializeSettingsFiles(string settingsConfigFile)
        {
            if (!_directoryWrapper.Exists(EnvironmentVariables.ServerSettingsFolder))
            {
                _directoryWrapper.CreateDirectory(EnvironmentVariables.ServerSettingsFolder);
            }

            if (_fileWrapper.Exists("Settings.config") && !_fileWrapper.Exists(EnvironmentVariables.ServerLogSettingsFile))
            {
                    _fileWrapper.Copy("Settings.config", EnvironmentVariables.ServerLogSettingsFile);
            }
            if (_fileWrapper.Exists("secure.config") && !_fileWrapper.Exists(EnvironmentVariables.ServerSecuritySettingsFile))
            {
                    _fileWrapper.Copy("secure.config", EnvironmentVariables.ServerSecuritySettingsFile);
            }
            if (!_fileWrapper.Exists(settingsConfigFile))
            {
                _fileWrapper.WriteAllText(settingsConfigFile, GlobalConstants.DefaultServerLogFileConfig);
            }
        }

        protected void SetupTempCleanupSetting()
        {
            var daysToKeepTempFilesValue = ConfigurationManager.AppSettings.Get("DaysToKeepTempFiles");
            if (!string.IsNullOrEmpty(daysToKeepTempFilesValue) && int.TryParse(daysToKeepTempFilesValue, out int daysToKeepTempFiles) && daysToKeepTempFiles > 0)
            {
                _tempFileDeleter.DaysToKeepTempFiles = daysToKeepTempFiles;
                _tempFileDeleter.Start();
            }
        }

        public void Dispose()
        {
            if (_tempFileDeleter is null)
            {
                return;
            }
            _tempFileDeleter.Dispose();
        }
    }

}
