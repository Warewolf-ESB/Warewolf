using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Threading;
using BuildEventLogging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Studio.UI.Tests.Utils
{
    /// <summary>
    /// Used to bootstrap the server for coded UI test runs
    /// </summary>
    [TestClass()]
    public class Bootstrap
    {
        private const string ServerName = "Warewolf Server.exe";
        private const string StudioName = "Warewolf Studio.exe";
        private const string ServerProcName = "Warewolf Server";
        private const string StudioProcName = "Warewolf Studio";
        private const int ServerTimeOut = 30000;
        private const int StudioTimeOut = 30000;
        private const string LocalBuildRunDirectory = "C:\\TestDeploy\\";//Local run directory

        public static string ServerLocation;
        public static Process ServerProc;
        public static string StudioLocation;
        public static Process StudioProc;


        private static object _tumbler = new object();
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
                var studioProcess = TryGetProcess(StudioProcName);
                if(textCtx.Properties["ControllerName"] == null || textCtx.Properties["ControllerName"].ToString() == "localhost:6901")
                {
                    //Local, assume server is running
                    ServerLocation = GetProcessPath(serverProcess);
                    StudioLocation = GetProcessPath(studioProcess);
                    //return;
                }

                var deployLocation = textCtx.DeploymentDirectory;
                ServerLocation = deployLocation + @"\" + ServerName;
                StudioLocation = deployLocation + @"\" + StudioName;
                if(File.Exists(ServerLocation) && File.Exists(StudioLocation))
                {
                    // term any existing studio processes ;)
                    KillProcess(studioProcess);

                    // term any existing server processes ;)
                    KillProcess(serverProcess);

                    StartServer();
                    StartStudio();
                }
                else
                {
                    var buildLabel = new BuildLabel(textCtx.DeploymentDirectory);
                    //Remote, assume server is running
                    if(serverProcess == null)
                    {
                        //Remote by a build agent
                        ServerLocation = LocalBuildRunDirectory + ServerName;
                        if(File.Exists(ServerLocation))
                        {
                            //Try start
                            StartServer();
                            if(buildLabel.LoggingURL != string.Empty)
                            {
                                BuildEventLogger.LogBuildEvent(buildLabel, "Error! Test pack has had to start the server for coded UI test! It should already have been started!");
                            }
                        }
                        else
                        {
                            if(buildLabel.LoggingURL != string.Empty)
                            {
                                BuildEventLogger.LogBuildEvent(buildLabel, "Error! Server is not running for coded UI test pack and no build is deployed to start!");
                            }
                            throw new Exception("Cannot run coded UI test pack because server is not running and no build is available.");
                        }
                    }
                    else
                    {
                        ServerLocation = GetProcessPath(serverProcess);
                    }

                    //Remote, assume studio is running
                    if(studioProcess == null)
                    {
                        //Remote by a build agent
                        StudioLocation = LocalBuildRunDirectory + StudioName;
                        if(File.Exists(StudioLocation))
                        {
                            //Try start
                            StartStudio();
                            if(buildLabel.LoggingURL != string.Empty)
                            {
                                BuildEventLogger.LogBuildEvent(buildLabel, "Error! Test pack has had to start the studio for coded UI test! It should already have been started!");
                            }
                        }
                        else
                        {
                            if(buildLabel.LoggingURL != string.Empty)
                            {
                                BuildEventLogger.LogBuildEvent(buildLabel, "Error! Studio is not running for coded UI test pack and no build is deployed to start!");
                            }
                            throw new Exception("Cannot run coded UI test pack because studio is not running and no build is available.");
                        }
                    }
                    else
                    {
                        if((from ManagementObject process in studioProcess select process.Properties["ExecutionState"].Value).FirstOrDefault() == null && (from ManagementObject process in studioProcess select process.Properties["Status"].Value).FirstOrDefault() == null)
                        {
                            StudioLocation = GetProcessPath(studioProcess);
                        }
                        else
                        {
                            throw new Exception("Studio state cannot be determined for Coded UI run.");
                        }
                    }
                }
            }
        }

        static void StartStudio()
        {
            var args = "-t";

            ProcessStartInfo startInfo = new ProcessStartInfo { CreateNoWindow = false, UseShellExecute = true, Arguments = args };
            startInfo.FileName = StudioLocation;
            //startInfo.RedirectStandardOutput = true;
            //startInfo.WindowStyle = ProcessWindowStyle.Hidden;

            var started = false;
            var startCnt = 0;

            while(!started && startCnt < 5)
            {
                try
                {
                    StudioProc = Process.Start(startInfo);

                    // Wait for studio to start
                    Thread.Sleep(StudioTimeOut); // wait for server to start ;)
                    if(StudioProc != null && !StudioProc.HasExited)
                    {
                        StudioLocation = GetProcessPath(TryGetProcess(StudioProcName));
                        started = true;
                    }
                }
                catch
                {
                    // most likely a studio is already running, kill it and try again ;)
                    startCnt++;
                }
            }
        }

        static void StartServer()
        {
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
            if(ServerProc != null && !ServerProc.HasExited)
            {
                ServerProc.Kill();
            }
            if(StudioProc != null && !StudioProc.HasExited)
            {
                StudioProc.Kill();
            }

            if(File.Exists(testCtx.DeploymentDirectory + @"\" + ServerName) && File.Exists(testCtx.DeploymentDirectory + @"\" + StudioName))
            {
                //Server and studio were deployed and started, stop them now.
                KillProcess(TryGetProcess(ServerProcName));
                KillProcess(TryGetProcess(StudioProcName));
            }

            var buildLabel = new BuildLabel(testCtx.DeploymentDirectory);
            //Remote by a build agent
            if(buildLabel.LoggingURL != string.Empty)
            {
                BuildEventLogger.LogBuildEvent(buildLabel, "Finished coded UI testing.");
            }
        }

        private static ManagementObjectCollection TryGetProcess(string procName)
        {
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
