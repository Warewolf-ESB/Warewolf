
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

// ReSharper disable once InconsistentNaming
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Studio.UI.Tests.Utils
{
    /// <summary>
    /// Used to bootstrap the server for coded UI test runs
    /// </summary>
    [TestClass]
    public class Bootstrap
    {
        public const string ServerProcName = "Warewolf Server";
        public const string StudioProcName = "Warewolf Studio";
        private const string ServerExeName = ServerProcName + ".exe";
        public const int ServerTimeOut = 10*1000;
        public const int StudioTimeOut = 60*1000;

        public static string LogLocation = @"C:\UI_Test.log";
        public static TestContext testContext;

        public static string ServerLocation;
        public static string StudioLocation;
        private static string _resourceLocation;
        private static string _serverWorkspaceLocation;

        public static bool IsLocal = false;

        /// <summary>
        /// Inits the ServerLocation.
        /// </summary>
        /// <param name="testCtx">Test context.</param>
        [AssemblyInitialize]
        public static void AssemblyInit(TestContext testCtx)
        {
            testContext = testCtx;
            IsLocal = testContext.Properties["ControllerName"] == null || testContext.Properties["ControllerName"].ToString().StartsWith("localhost");
            ResolvePathsToTestAgainst();
        }

        public static void ResolvePathsToTestAgainst()
        {
            var serverProcess = TryGetProcess(ServerProcName);
            if(serverProcess == null)
            {
                throw new Exception("No server running.");
            }
            var studioProcess = TryGetProcess(StudioProcName);
            if(studioProcess == null)
            {
                throw new Exception("No studio running.");
            }
            ServerLocation = GetProcessPath(serverProcess);
            StudioLocation = GetProcessPath(studioProcess);
            //Set resource location
            _resourceLocation = StudioLocation.Replace(ServerExeName, @"Resources\");
            //Set workspace location
            _serverWorkspaceLocation = ServerLocation.Replace(ServerExeName, @"Workspaces\");
        }

        private static string GetProcessPath(ICollection processes)
        {
            if(processes == null || processes.Count == 0)
            {
                return null;
            }
            return (from ManagementObject process in processes select (process.Properties["ExecutablePath"].Value ?? string.Empty).ToString()).FirstOrDefault();
        }

        public static void Init()
        {
            // Remove old log files ;)
            if(File.Exists(LogLocation))
            {
                File.Delete(LogLocation);
            }

            //Check for runnning server/studio
            var studioProc = TryGetProcess(StudioProcName);
            var serverProc = TryGetProcess(ServerProcName);

            //Don't touch the studio if running
            if(studioProc != null)
            {
                return;
            }
            StartStudio(StudioLocation);

            //Don't touch the server if running
            if(serverProc != null)
            {
                return;
            }
            // remove workspaces to avoid mutation issues
            RemoveWorkspaces();
            StartServer(ServerLocation);
        }

        /// <summary>
        /// Tear downs this instance.
        /// </summary>
        public static void Teardown(bool studioOnly = false)
        {
            if(File.Exists(ServerLocation) && File.Exists(StudioLocation))
            {
                if (!studioOnly)
                {
                    //Server was deployed and started, stop it now.
                    KillServer();
                }

                //Studio was deployed and started, stop it now.
                KillStudio();
            }

            if(!IsLocal)
            {
                // Now clean up next test run ;)
                CloseAllInstancesOfIE();
            }
        }

        public static void KillServer()
        {
            KillProcess(TryGetProcess(ServerProcName));
        }

        public static void KillStudio()
        {
            KillProcess(TryGetProcess(StudioProcName));
        }

        /// <summary>
        /// Deletes the source.
        /// </summary>
        /// <param name="sourceName">Name of the source.</param>
        public static void DeleteSource(string sourceName, string serviceCategory = null)
        {
            var path = _resourceLocation + (serviceCategory != null ? (serviceCategory + "\\") : string.Empty) + sourceName;
            if(File.Exists(path))
            {
                File.Delete(path);
            }
        }

        /// <summary>
        /// Deletes the service.
        /// </summary>
        /// <param name="serviceName">Name of the service.</param>
        public static void DeleteService(string serviceName, string serviceCategory = null)
        {
            var path = _resourceLocation + (serviceCategory != null ? (serviceCategory + "\\") : string.Empty) + serviceName;
            if(File.Exists(path))
            {
                File.Delete(path);
            }
        }

        private static Exception Exception(string p)
        {
            throw new NotImplementedException();
        }

        static void CloseAllInstancesOfIE()
        {
            var browsers = new[] { "iexplore" };

            foreach(var browser in browsers)
            {
                Process[] processList = Process.GetProcessesByName(browser);
                foreach(Process p in processList)
                {
                    try
                    {
                        p.Kill();
                    }
                    // ReSharper disable EmptyGeneralCatchClause
                    catch
                    // ReSharper restore EmptyGeneralCatchClause
                    {
                    }
                }
            }
        }

        /// <summary>
        /// Removes the workspaces.
        /// </summary>
        static void RemoveWorkspaces()
        {
            try
            {
                if(Directory.Exists(_serverWorkspaceLocation))
                {
                    Directory.Delete(_serverWorkspaceLocation, true);
                }
            }
            catch(Exception)
            {
                //Do nothing
            }
        }

        static void StartStudio(string location)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo { CreateNoWindow = false, UseShellExecute = true, FileName = location };

            var started = false;
            var startCnt = 0;

            while(!started && startCnt < 5)
            {
                try
                {
                    var StudioProc = Process.Start(startInfo);

                    // Wait for studio to start
                    Thread.Sleep(StudioTimeOut); // wait for server to start ;)
                    if(StudioProc != null && !StudioProc.HasExited)
                    {
                        started = true;
                    }
                }
                catch
                {
                    // most likely a studio is already running, kill it and try again ;)
                    startCnt++;
                }
                finally
                {
                    if(!started)
                    {
                        // term any existing server processes ;)
                        KillProcess(TryGetProcess(StudioProcName));
                        throw new Exception("Failed to start Studio at " + location);
                    }
                }
            }
        }

        static void StartServer(string location)
        {
            const string args = "-t";

            ProcessStartInfo startInfo = new ProcessStartInfo { CreateNoWindow = false, UseShellExecute = true, Arguments = args, FileName = location };

            var started = false;
            var startCnt = 0;

            while(!started && startCnt < 5)
            {
                try
                {
                    var ServerProc = Process.Start(startInfo);

                    // Wait for server to start
                    Thread.Sleep(ServerTimeOut); // wait for server to start ;)
                    if(ServerProc != null && !ServerProc.HasExited)
                    {
                        started = true;
                    }
                }
                catch(Exception)
                {
                    // most likely a server is already running, kill it and try again ;)
                    startCnt++;
                }
                finally
                {
                    if(!started)
                    {
                        // term any existing server processes ;)
                        KillProcess(TryGetProcess(ServerProcName));
                        throw new Exception("Failed to start server at " + location);
                    }
                }
            }
        }

        public static ManagementObjectCollection TryGetProcess(string procName)
        {
            var processName = procName;
            var query = new SelectQuery(@"SELECT * FROM Win32_Process where Name LIKE '%" + processName + ".exe'");
            //initialize the searcher with the query it is
            //supposed to execute
            using(ManagementObjectSearcher searcher = new ManagementObjectSearcher(query))
            {
                //execute the query
                ManagementObjectCollection processes = searcher.Get();
                if(processes.Count <= 0)
                {
                    return null;
                }
                return processes;
            }
        }

        static void KillProcess(ManagementObjectCollection processes)
        {
            if(processes == null)
            {
                return;
            }
            foreach(ManagementObject process in processes)
            {
                //print process properties
                try
                {
                    process.Get();
                    var pid = process.Properties["ProcessID"].Value.ToString();
                    var proc = Process.GetProcessById(Int32.Parse(pid));
                    proc.Kill();
                }
                // ReSharper disable EmptyGeneralCatchClause
                catch(Exception)
                // ReSharper restore EmptyGeneralCatchClause
                {
                    // Do nothing
                }
            }
        }
    }
}

