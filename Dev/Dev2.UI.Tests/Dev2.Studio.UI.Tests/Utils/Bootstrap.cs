using System;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Threading;
using BuildEventLogging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Studio.UI.Tests.Utils
{
    /// <summary>
    /// Used to bootstrap the server for coded UI test runs
    /// </summary>
    [TestClass]
    public class Bootstrap
    {
        private const string ServerName = "Warewolf Server.exe";
        private const string StudioName = "Warewolf Studio.exe";
        private const string ServerProcName = "Warewolf Server";
        private const string StudioProcName = "Warewolf Studio";
        private const int ServerTimeOut = 2000;
        private const int StudioTimeOut = 10000;

        public static string ServerLocation = @"C:\Builds\UITestRunWorkspace\Binaries\Warewolf Server.exe";
        public static Process ServerProc;
        public static string StudioLocation = @"C:\Builds\UITestRunWorkspace\Binaries\Warewolf Studio.exe";
        public static Process StudioProc;


        private static readonly object _tumbler = new object();
        private static TestContext testCtx;

        /// <summary>
        /// Inits the specified test CTX.
        /// </summary>
        /// <param name="textCtx">The test CTX.</param>
        [AssemblyInitialize]
        public static void AssemblyInit(TestContext textCtx)
        {
            testCtx = textCtx;
        }

        public static void Init()
        {
            lock(_tumbler)
            {
                var serverProcess = TryGetProcess(ServerProcName);
                var studioProcess = TryGetProcess(StudioProcName);

                if(File.Exists(ServerLocation) && File.Exists(StudioLocation))
                {
                    // term any existing studio processes ;)
                    KillProcess(studioProcess);
                    // term any existing server processes ;)
                    KillProcess(serverProcess);

                    StartServer();
                    StartStudio();
                    Thread.Sleep(2500);
                }
            }
        }

        static void StartStudio()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo { CreateNoWindow = false, UseShellExecute = true, FileName = StudioLocation };

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
                        //        StudioLocation = GetProcessPath(TryGetProcess(StudioProcName));
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
            const string args = "-t";

            ProcessStartInfo startInfo = new ProcessStartInfo { CreateNoWindow = false, UseShellExecute = true, Arguments = args, FileName = ServerLocation };

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
        [AssemblyCleanup]
        public static void Teardown()
        {
            if(ServerProc != null && !ServerProc.HasExited)
            {
                ServerProc.Kill();
            }

            if(File.Exists(testCtx.DeploymentDirectory + @"\" + ServerName))
            {
                //Server was deployed and started, stop it now.
                KillProcess(TryGetProcess(ServerProcName));
            }

            if(StudioProc != null && !StudioProc.HasExited)
            {
                StudioProc.Kill();
            }

            if(File.Exists(testCtx.DeploymentDirectory + @"\" + StudioName))
            {
                //Studio was deployed and started, stop it now.
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
