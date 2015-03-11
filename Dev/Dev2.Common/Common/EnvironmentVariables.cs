
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
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
                        ServerLogger.LogMessage("ApplicationPath Error -> " + e.Message);
                        _appPath = Directory.GetCurrentDirectory(); // fail safe ;)
                    }

                }

                return _appPath;

                // 17.04.2013
                // The code below will not work with the integration build process 
                // as it always returns a path relative to the test executor dll, not the dev2 dlls ;(
                //return Directory.GetCurrentDirectory();
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
                return Path.Combine(ApplicationPath, "Workspaces");
            }
        }

        /// <summary>
        /// Gets the workspace path.
        /// </summary>
        /// <param name="workspaceID">The workspace ID.</param>
        /// <returns></returns>
        public static string GetWorkspacePath(Guid workspaceID)
        {
            return workspaceID == GlobalConstants.ServerWorkspaceID
                       ? ApplicationPath
                       : Path.Combine(WorkspacePath, workspaceID.ToString());
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
                if (_rootPath == null)
                {
                    _rootPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Dev2");
                }

                return _rootPath;
            }
        }


        /// <summary>
        /// Gets the encoding for character maps.
        /// </summary>
        /// <param></param>
        /// <returns name="Encoding"></returns>
         public struct CharacterMap
         {
             public static Encoding DefaultEncoding = Encoding.ASCII;//TODO: { get; set; }
             public static int LettersStartNumber = 97;//TODO: { get; set; }
             //public static int NumbersStartNumber = 48;//TODO: { get; set; }
             public static int LettersLength = 26;
             //public static int NumbersLength = 10;
         }
    }
}
