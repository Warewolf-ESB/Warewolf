using System;
using System.IO;
using System.Reflection;

namespace Dev2.Common
{
    /// <summary>
    /// Environment  Variables to be used in the Server
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
    }
}
