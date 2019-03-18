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

using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace Dev2.Common
{
    public static class EnvironmentVariables
    {
        static string _appPath;
        static readonly string DataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData, Environment.SpecialFolderOption.Create), "Warewolf");

        public static string ApplicationPath
        {
            get
            {
                if (String.IsNullOrEmpty(_appPath))
                {
                    try
                    {
                        var assembly = Assembly.GetExecutingAssembly();
                        var loc = assembly.Location;
                        _appPath = Path.GetDirectoryName(loc);
                    }
                    catch (Exception e)
                    {
                        Dev2Logger.Info("ApplicationPath Error -> " + e.Message, GlobalConstants.WarewolfInfo);
                        _appPath = Directory.GetCurrentDirectory(); 
                    }

                }

                return _appPath;
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
        public static string DnsName { get; set; }
        public static int Port { get; set; }
    }
}
