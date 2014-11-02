/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
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

// ReSharper disable CheckNamespace

namespace Dev2.Common
// ReSharper restore CheckNamespace
{
    /// <summary>
    ///     Environment Variables to be used in the Server
    /// </summary>
    public static class EnvironmentVariables
    {
        private static string _appPath;
        private static string _rootPath;
        private static readonly Guid _remoteID = Guid.NewGuid();

        /// <summary>
        ///     Gets the application path.
        /// </summary>
        /// <value>
        ///     The application path.
        /// </value>
        public static string ApplicationPath
        {
            get
            {
                if (String.IsNullOrEmpty(_appPath))
                {
                    try
                    {
                        Assembly assembly = Assembly.GetExecutingAssembly();
                        string loc = assembly.Location;
                        _appPath = Path.GetDirectoryName(loc);
                    }
                    catch (Exception e)
                    {
                        Dev2Logger.Log.Info("ApplicationPath Error -> " + e.Message);
                        _appPath = Directory.GetCurrentDirectory(); // fail safe ;)
                    }
                }

                return _appPath;
            }
        }

        public static string ResourcePath
        {
            get { return Path.Combine(ApplicationPath, "Resources"); }
        }

        /// <summary>
        ///     Gets the workspace path.
        /// </summary>
        /// <value>
        ///     The workspace path.
        /// </value>
        public static string WorkspacePath
        {
            get { return Path.Combine(ApplicationPath, "Workspaces"); }
        }

        public static bool IsServerOnline { get; set; }

        /// <summary>
        ///     Gets the root persistence path.
        /// </summary>
        /// <value>
        ///     The root persistence path.
        /// </value>
        public static string RootPersistencePath
        {
            get
            {
                return _rootPath ??
                       (_rootPath =
                           Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                               @"Warewolf"));
            }
        }


        /// <summary>
        ///     Gets the remote invoke ID.
        /// </summary>
        /// <value>
        ///     The remote invoke ID.
        /// </value>
        public static Guid RemoteInvokeID
        {
            get { return _remoteID; }
        }

        public static string WebServerUri { get; set; }

        /// <summary>
        ///     Gets the workspace path.
        /// </summary>
        /// <param name="workspaceID">The workspace ID.</param>
        /// <returns></returns>
        public static string GetWorkspacePath(Guid workspaceID)
        {
            return workspaceID == Guid.Empty
                ? Path.Combine(ApplicationPath, "Resources")
                : Path.Combine(Path.Combine(WorkspacePath, workspaceID.ToString()), "Resources");
        }

        /// <summary>
        ///     Gets the encoding for character maps.
        /// </summary>
        /// <param></param>
        /// <returns name="Encoding"></returns>
        public struct CharacterMap
        {
            public static Encoding DefaultEncoding = Encoding.ASCII;
            public static int LettersStartNumber = 97;
            public static int LettersLength = 26;
        }
    }
}