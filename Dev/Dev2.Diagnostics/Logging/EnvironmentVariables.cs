
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.IO;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;

// ReSharper disable CheckNamespace
namespace Dev2.Common
// ReSharper restore CheckNamespace
{
    /// <summary>
    /// Environment Variables to be used in the Server
    /// </summary>
    public static class EnvironmentVariables
    {

        private static string _appPath;

        /// <summary>
        /// Gets the application path.
        /// </summary>
        /// <value>
        /// The application path.
        /// </value>
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
                        Dev2Logger.Info("ApplicationPath Error -> " + e.Message);
                        _appPath = Directory.GetCurrentDirectory(); // fail safe ;)
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
        private static string AppDataPath
        {
            get
            {
                var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData, Environment.SpecialFolderOption.Create),"Warewolf");
                if(!Directory.Exists(appDataPath))
                {
                    Directory.CreateDirectory(appDataPath);
                }
                var directoryInfo = new DirectoryInfo(appDataPath);
                SecurityIdentifier securityIdentifier = new SecurityIdentifier
                    (WellKnownSidType.WorldSid, null);
                bool modified;
                var directorySecurity = directoryInfo.GetAccessControl();
                AccessRule rule = new FileSystemAccessRule(
                    securityIdentifier,
                    FileSystemRights.Write |
                    FileSystemRights.ReadAndExecute |
                    FileSystemRights.Modify |
                    FileSystemRights.FullControl,
                    InheritanceFlags.ContainerInherit |
                    InheritanceFlags.ObjectInherit,
                    PropagationFlags.InheritOnly,
                    AccessControlType.Allow);
                directorySecurity.ModifyAccessRule(AccessControlModification.Add, rule, out modified);
                directoryInfo.SetAccessControl(directorySecurity);
                return appDataPath;
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

        public static string ServerSecuritySettingsFile
        {
            get
            {
                var severSecurityFile = Path.Combine(ServerSettingsFolder, "secure.config");
                return severSecurityFile;
            }
        }

        public static string ServerLogFile
        {
            get
            {
                return Path.Combine(AppDataPath, "Server Log", "warewolf-Server.log");
            }
        }

        /// <summary>
        /// Gets the workspace path.
        /// </summary>
        /// <value>
        /// The workspace path.
        /// </value>
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

        /// <summary>
        /// Gets the workspace path.
        /// </summary>
        /// <param name="workspaceID">The workspace ID.</param>
        /// <returns></returns>
        public static string GetWorkspacePath(Guid workspaceID)
        {
            return workspaceID == Guid.Empty
                       ? Path.Combine(AppDataPath, "Resources")
                       : Path.Combine( Path.Combine(WorkspacePath, workspaceID.ToString()),"Resources");
        }

        public static bool IsServerOnline { get; set; }

        private static string _rootPath;
        /// <summary>
        /// Gets the root persistence path.
        /// </summary>
        /// <value>
        /// The root persistence path.
        /// </value>
        public static string RootPersistencePath
        {
            get
            {
                return _rootPath ?? (_rootPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Warewolf"));
            }
        }


        /// <summary>
        /// Gets the encoding for character maps.
        /// </summary>
        /// <param></param>
        /// <returns name="Encoding"></returns>
        public struct CharacterMap
        {
            public static Encoding DefaultEncoding = Encoding.ASCII;
            public static int LettersStartNumber = 97;
            public static int LettersLength = 26;
        }

        private static readonly Guid RemoteID = Guid.NewGuid();
        /// <summary>
        /// Gets the remote invoke ID.
        /// </summary>
        /// <value>
        /// The remote invoke ID.
        /// </value>
        public static Guid RemoteInvokeID
        {
            get { return RemoteID; }
        }

        public static string WebServerUri { get; set; }
        public static string PublicWebServerUri
        {
            get
            {
                return DnsName + ":" + Port+"/";
            }
        }
        public static string DnsName { private get; set; }
        public static int Port { private get; set; }
    }
}
