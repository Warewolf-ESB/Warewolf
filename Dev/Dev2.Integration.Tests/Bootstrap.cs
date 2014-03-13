using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Threading;
using BuildEventLogging;
using Dev2.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Integration.Tests
{
    /// <summary>
    /// Used to bootstrap the server for integration test runs
    /// </summary>
    [TestClass()]
    public class Bootstrap
    {
        private const string ServerName = "Warewolf Server.exe";
        private const string ServerProcName = "Warewolf Server";
        private const int ServerTimeOut = 5000;
        private const string LocalBuildRunDirectory = "C:\\TestDeploy\\";//Local run directory

        public static string ServerLocation;
        public static Process ServerProc;

        private static readonly object _tumbler = new object();
        private static readonly object serverLock = new object();

        private static TestContext testCtx;

        /// <summary>
        /// Inits the specified test CTX.
        /// </summary>
        /// <param name="textCtx">The test CTX.</param>
        [AssemblyInitialize()]
        public static void Init(TestContext textCtx)
        {
            testCtx = textCtx;
            lock(_tumbler)
            {
                var serverProcess = TryGetProcess(ServerProcName);
                if(testCtx.Properties["ControllerName"] == null || testCtx.Properties["ControllerName"].ToString() == "localhost:6901")
                {
                    //Local, assume server is running
                    ServerLocation = GetProcessPath(serverProcess);
                    return;
                }

                var deployLocation = testCtx.DeploymentDirectory;
                //Server was deployed
                ServerLocation = deployLocation + @"\" + ServerName;
                if(File.Exists(ServerLocation))
                {
                    // term any existing server processes ;)
                    KillProcess(serverProcess);

                    StartServer();
                }
                else
                {
                    var buildLabel = new BuildLabel(testCtx.DeploymentDirectory);
                    //Remote, assume server is running
                    if(serverProcess == null)
                    {
                        //Remote by a build agent
                        ServerLocation = LocalBuildRunDirectory + buildLabel.ChangesetID + "\\Binaries\\" + ServerName;
                        if(File.Exists(ServerLocation))
                        {
                            //Try start
                            StartServer();
                            if(buildLabel.LoggingURL != string.Empty)
                            {
                                BuildEventLogger.LogBuildEvent(buildLabel, "Error! Test pack has had to start the server for integration test! It should already have been started!");
                            }
                        }
                        else
                        {
                            if(buildLabel.LoggingURL != string.Empty)
                            {
                                BuildEventLogger.LogBuildEvent(buildLabel, "Error! Server is not running for integration test pack and no build is deployed to start!");
                            }
                            throw new Exception("Cannot run integration test pack because server is not running and no build is available.");
                        }
                    }
                    else
                    {
                        ServerLocation = GetProcessPath(serverProcess);
                    }
                }
            }
        }

        static void StartServer()
        {
            ServerLogger.LogMessage("Server Loc -> " + ServerLocation);
            ServerLogger.LogMessage("App Server Path -> " + EnvironmentVariables.ApplicationPath);

            var args = "-t";

            ProcessStartInfo startInfo = new ProcessStartInfo { CreateNoWindow = false, UseShellExecute = true, Arguments = args };
            startInfo.FileName = ServerLocation;
            //startInfo.RedirectStandardOutput = true;
            //startInfo.WindowStyle = ProcessWindowStyle.Hidden;

            var started = false;
            var startCnt = 0;

            while(!started && startCnt < 5)
            {
                try
                {
                    ServerProc = Process.Start(startInfo);

                    // Wait for server to start
                    Thread.Sleep(ServerTimeOut); // wait for server to start ;)
                    if(ServerProc != null && !ServerProc.HasExited)
                    {
                        started = true;
                        ServerLogger.LogMessage("** Server Started for Integration Test Run");
                    }
                }
                catch(Exception e)
                {
                    ServerLogger.LogMessage("Exception : " + e.Message);

                    // most likely a server is already running, kill it and try again ;)
                    startCnt++;
                }
                finally
                {
                    if(!started)
                    {
                        ServerLogger.LogMessage("** Server Failed to Start for Integration Test Run");
                        // term any existing server processes ;)
                        KillProcess(TryGetProcess(ServerProcName));
                    }
                }
            }
        }

        /// <summary>
        /// Teardowns this instance.
        /// </summary>
        [AssemblyCleanup()]
        public static void Teardown()
        {
            if(File.Exists(testCtx.DeploymentDirectory + @"\" + ServerName))
            {
                //Server was deployed and started, stop it now.
                KillProcess(TryGetProcess(ServerProcName));
            }

            var buildLabel = new BuildLabel(testCtx.DeploymentDirectory);
            //Remote by a build agent
            if(buildLabel.LoggingURL != string.Empty)
            {
                BuildEventLogger.LogBuildEvent(buildLabel, "Finished integration testing.");
            }
        }


        private static ManagementObjectCollection TryGetProcess(string procName)
        {
            ServerLogger.LogMessage("** Get Process LIKE { " + procName + " }");
            var processName = procName;
            var query = new SelectQuery(@"SELECT * FROM Win32_Process where Name LIKE '%" + processName + "%'");
            //initialize the searcher with the query it is
            //supposed to execute
            using(ManagementObjectSearcher searcher = new ManagementObjectSearcher(query))
            {
                //execute the query
                ManagementObjectCollection processes = searcher.Get();
                if(processes.Count <= 0)
                {
                    ServerLogger.LogMessage("No processes");
                    return null;
                }
                return processes;
            }
        }

        private static string GetProcessPath(ManagementObjectCollection processes)
        {
            if(processes == null || processes.Count == 0)
            {
                return null;
            }
            return (from ManagementObject process in processes select process.Properties["ExecutablePath"].Value.ToString()).FirstOrDefault();
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
                process.Get();
                var pid = process.Properties["ProcessID"].Value.ToString();

                ServerLogger.LogMessage("Killed Process { " + pid + " }");

                var proc = Process.GetProcessById(Int32.Parse(pid));

                try
                {
                    proc.Kill();
                }
                catch
                {
                    // Do nothing
                }
            }
        }
    }
}
