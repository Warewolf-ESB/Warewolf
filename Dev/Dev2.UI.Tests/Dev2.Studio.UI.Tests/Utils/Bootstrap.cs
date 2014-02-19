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
        private static Process _serverProc;
        private static Process _studioProc;

        private const string ServerName = "Warewolf Server.exe";
        private const string StudioName = "Warewolf Studio.exe";
        private const string ServerProcName = "Warewolf Server";
        private const string StudioProcName = "Warewolf Studio";
        private const int ServerTimeOut = 30000;
        private const int StudioTimeOut = 30000;

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
                    return;
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

                    var args = "-t";

                    ProcessStartInfo startInfo = new ProcessStartInfo { CreateNoWindow = false, UseShellExecute = true, Arguments = args };
                    //startInfo.RedirectStandardOutput = true;
                    //startInfo.WindowStyle = ProcessWindowStyle.Hidden;

                    var started = false;
                    var startCnt = 0;
                    startInfo.FileName = ServerLocation;

                    while(!started && startCnt < 5)
                    {
                        try
                        {
                            _serverProc = Process.Start(startInfo);

                            // Wait for server to start
                            Thread.Sleep(ServerTimeOut); // wait for server to start ;)
                            if(_serverProc != null && !_serverProc.HasExited)
                            {
                                ServerLocation = GetProcessPath(TryGetProcess(ServerProcName));
                                started = true;
                            }
                        }
                        catch
                        {
                            // most likely a server is already running, kill it and try again ;)
                            startCnt++;
                        }
                    }

                    started = false;
                    startCnt = 0;
                    startInfo.FileName = StudioLocation;

                    while(!started && startCnt < 5)
                    {
                        try
                        {
                            _studioProc = Process.Start(startInfo);

                            // Wait for studio to start
                            Thread.Sleep(StudioTimeOut); // wait for server to start ;)
                            if(_studioProc != null && !_studioProc.HasExited)
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
                else
                {
                    //Remote, assume server is running
                    ServerLocation = GetProcessPath(serverProcess);
                    StudioLocation = GetProcessPath(studioProcess);
                    var buildLabel = new BuildLabel(textCtx.DeploymentDirectory);
                    //Remote by a build agent
                    if(buildLabel.LoggingURL != string.Empty)
                    {
                        BuildEventLogger.LogBuildEvent(buildLabel, "Started coded UI testing.");
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
            if(_serverProc != null && !_serverProc.HasExited)
            {
                _serverProc.Kill();
            }
            if(_studioProc != null && !_studioProc.HasExited)
            {
                _studioProc.Kill();
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
