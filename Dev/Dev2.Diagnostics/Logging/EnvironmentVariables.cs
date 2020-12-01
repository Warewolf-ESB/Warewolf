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

using System;
using System.IO;
using System.Text;

namespace Dev2.Common
{
    public static class EnvironmentVariables
    {
        static readonly string DataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData, Environment.SpecialFolderOption.Create), "Warewolf");

        private static string _applicationPath;
        public static string ApplicationPath {
            get => _applicationPath;
            set
            {
                if (_applicationPath is null)
                {
                    _applicationPath = value;
                }
            }
        }

        public static string ResourcePath
        {
            get
            {
                var resourcePath = Path.Combine(AppDataPath, "Resources");
                return resourcePath;
            }
        }

        public static string VersionsPath
        {
            get
            {
                var versionsPath = Path.Combine(AppDataPath, "VersionControl");
                if (!Directory.Exists(versionsPath))
                {
                    Directory.CreateDirectory(versionsPath);
                }
                return versionsPath;
            }
        }
        public static string TestPath
        {
            get
            {
                var resourcePath = Path.Combine(AppDataPath, "Tests");
                return resourcePath;
            }
        }

        public static string TestCoveragePath
        {
            get
            {
                var resourcePath = Path.Combine(AppDataPath, "CoverageReports");
                return resourcePath;
            }
        }

        public static string TriggersPath
        {
            get
            {
                var resourcePath = Path.Combine(AppDataPath, "Triggers");
                return resourcePath;
            }
        }

        public static string QueueTriggersPath
        {
            get
            {
                var resourcePath = Path.Combine(TriggersPath, "Queue");
                return resourcePath;
            }
        }
        public static string DetailLogPath
        {
            get
            {
                var resourcePath = Path.Combine(AppDataPath, "DetailedLogs");
                if (!Directory.Exists(resourcePath))
                {
                    Directory.CreateDirectory(resourcePath);
                }
                return resourcePath;
            }
        }

        public static string DetailedLogsArchives
        {
            get
            {
                var resourcePath = Path.Combine(AppDataPath, DetailLogPath, "Archives");
                if (!Directory.Exists(resourcePath))
                {
                    Directory.CreateDirectory(resourcePath);
                }
                return resourcePath;
            }
        }
        public static string WorkflowDetailLogPath(Guid Id, string name)
        {
            var wfDetailedLogPath = Path.Combine($"{DetailLogPath}", string.Format("{0}_{1}", Id, name));
            if (!Directory.Exists(wfDetailedLogPath))
            {
                Directory.CreateDirectory(wfDetailedLogPath);
            }
            return wfDetailedLogPath;
        }
        public static string WorkflowDetailLogArchivePath(Guid Id, string name)
        {
            return Path.Combine($"{DetailedLogsArchives}", string.Format("{0}_{1}.zip", Id, string.IsNullOrEmpty(name) ? "" : name));
        }
        public static string AppDataPath
        {
            get
            {
                if (!Directory.Exists(DataPath))
                {
                    Directory.CreateDirectory(DataPath);
                }
                return DataPath;
            }
        }

        public static string ServerSettingsFolder
        {
            get
            {
                var serverSettingsFolder = Path.Combine(AppDataPath, "Server Settings");
                if (!Directory.Exists(serverSettingsFolder))
                {
                    Directory.CreateDirectory(serverSettingsFolder);
                }
                return serverSettingsFolder;
            }
        }

        public static string ServerLogSettingsFile
        {
            get
            {
                var serverLogSettings = Path.Combine(ServerSettingsFolder, "Settings.config");
                return serverLogSettings;
            }
        }

        public static string ServerPerfmonSettingsFile
        {
            get
            {
                var serverLogSettings = Path.Combine(ServerSettingsFolder, "Perfmon.config");
                return serverLogSettings;
            }
        }
        public static string ServerResourcePerfmonSettingsFile
        {
            get
            {
                var serverLogSettings = Path.Combine(ServerSettingsFolder, "ResourcesPerfmon.config");
                return serverLogSettings;
            }
        }

        public static string ServerSecuritySettingsFile
        {
            get
            {
                var severSecurityFile = Path.Combine(ServerSettingsFolder, "secure.config");
                return severSecurityFile;
            }
        }

        public static string ServerLogFile => Path.Combine(AppDataPath, "Server Log", "warewolf-Server.log");

        public static string WorkspacePath
        {
            get
            {
                var workspacePath = Path.Combine(AppDataPath, "Workspaces");
                if (!Directory.Exists(workspacePath))
                {
                    Directory.CreateDirectory(workspacePath);
                }
                return workspacePath;
            }
        }

        public static string DebugItemTempPath
        {
            get
            {
                var tempPath = Path.Combine(GlobalConstants.TempLocation, "Warewolf", "Debug");
                if (!Directory.Exists(tempPath))
                {
                    Directory.CreateDirectory(tempPath);
                }
                return tempPath;
            }
        }

        public static string GetWorkspacePath(Guid workspaceID) => workspaceID == Guid.Empty
                       ? Path.Combine(AppDataPath, "Resources")
                       : Path.Combine(Path.Combine(WorkspacePath, workspaceID.ToString()), "Resources");

        public static bool IsServerOnline { get; set; }

        static string _rootPath;

        public static string RootPersistencePath => _rootPath ?? (_rootPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Warewolf"));

        public struct CharacterMap
        {
            public static readonly Encoding DefaultEncoding = Encoding.ASCII;
            public static readonly int LettersStartNumber = 97;
            public static readonly int LettersLength = 26;
        }

        static readonly Guid RemoteID = Guid.NewGuid();
        
        public static Guid RemoteInvokeID => RemoteID;

        public static string WebServerUri { get; set; }
        public static string PublicWebServerUri => DnsName + ":" + Port + "/";
        public static string OpenAPiVersion => "3.0.1";
        public static string DnsName { get; set; }
        public static int Port { get; set; }
    }
}
