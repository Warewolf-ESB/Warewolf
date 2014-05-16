using System;
using System.Diagnostics;
using System.IO;
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
        private const string ServerProcName = "Warewolf Server";
        private const string StudioProcName = "Warewolf Studio";
        private const int ServerTimeOut = 2000;
        private const int StudioTimeOut = 10000;

        public static string ServerLocation = @"C:\Builds\UITestRunWorkspace\Binaries\Warewolf Server.exe";
        public static Process ServerProc;
        public static string StudioLocation = @"C:\Builds\UITestRunWorkspace\Binaries\Warewolf Studio.exe";
        public static Process StudioProc;

        public static string LogLocation = @"C:\Builds\UITestRunWorkspace\UI_Test.log";

        public static string RootSourceLocation = @"C:\Builds\UITestRunWorkspace\Binaries\Sources\";
        public static string RootServiceLocation = @"C:\Builds\UITestRunWorkspace\Binaries\Services\";

        public static string ShadowSourceLocation = @"C:\Builds\UITestRunWorkspace\Sources\";
        public static string ShadowServiceLocation = @"C:\Builds\UITestRunWorkspace\Services\";

        public static string WorkspaceLocation = @"C:\Builds\UITestRunWorkspace\Binaries\Workspaces\";

        public static int WaitMS = 5000;

        public static void Init()
        {
            var serverProcess = TryGetProcess(ServerProcName);
            var studioProcess = TryGetProcess(StudioProcName);

            // Remove old log files ;)
            if(File.Exists(LogLocation))
            {
                File.Delete(LogLocation);
            }

            if(File.Exists(ServerLocation) && File.Exists(StudioLocation))
            {
                // term any existing studio processes ;)
                KillProcess(studioProcess);
                // term any existing server processes ;)
                KillProcess(serverProcess);

                RemoveWorkspaces();

                StartServer();
                StartStudio();

                Thread.Sleep(WaitMS);
            }
            else
            {
                LogTestRunMessage("Could not locate CodedUI Binaries", true);
            }


        }

        /// <summary>
        /// Tear downs this instance.
        /// </summary>
        public static void Teardown()
        {
            if(ServerProc != null && !ServerProc.HasExited)
            {
                ServerProc.Kill();
            }


            //Server was deployed and started, stop it now.
            KillProcess(TryGetProcess(ServerProcName));


            if(StudioProc != null && !StudioProc.HasExited)
            {
                StudioProc.Kill();
            }


            //Studio was deployed and started, stop it now.
            KillProcess(TryGetProcess(StudioProcName));

            // Now clean up resource for next test run ;)

        }

        /// <summary>
        /// Deletes the source.
        /// </summary>
        /// <param name="sourceName">Name of the source.</param>
        public static void DeleteSource(string sourceName)
        {
            var path = RootSourceLocation + sourceName;
            if(File.Exists(path))
            {
                File.Delete(path);
            }
        }

        /// <summary>
        /// Deletes the service.
        /// </summary>
        /// <param name="serviceName">Name of the service.</param>
        public static void DeleteService(string serviceName)
        {
            var path = RootServiceLocation + serviceName;
            if(File.Exists(path))
            {
                File.Delete(path);
            }
        }

        /// <summary>
        /// Removes the workspaces.
        /// </summary>
        static void RemoveWorkspaces()
        {
            if(Directory.Exists(WorkspaceLocation))
            {
                Directory.Delete(WorkspaceLocation);
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
                        started = true;
                        LogTestRunMessage("Started Studio");
                    }
                }
                catch
                {
                    // most likely a studio is already running, kill it and try again ;)
                    LogTestRunMessage("Could not locate Start Studio [ " + StudioLocation + " ] Attempt Count [ " + startCnt + " ]", true);
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
                        LogTestRunMessage("Started Server");
                    }
                }
                catch(Exception)
                {
                    // most likely a server is already running, kill it and try again ;)
                    LogTestRunMessage("Could not locate Start Server [ " + ServerLocation + " ] Attempt Count [ " + startCnt + " ]", true);
                    startCnt++;
                }
                finally
                {
                    if(!started)
                    {
                        LogTestRunMessage("Failed To Start Server.... Aborting", true);
                        // term any existing server processes ;)
                        KillProcess(TryGetProcess(ServerProcName));
                        throw new Exception("Failed To Start Server!!!!");
                    }
                }
            }
        }



        // here to force exit all processes 
        [AssemblyCleanup]
        public static void RunTeardown()
        {
            Teardown();
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
                catch(Exception e)
                // ReSharper restore EmptyGeneralCatchClause
                {
                    // Do nothing
                    LogTestRunMessage(e.Message, true);
                }
            }
        }

        public static void LogTestRunMessage(string msg, bool isError = false)
        {
            if(isError)
            {
                File.AppendAllText(LogLocation, "ERROR :: " + msg);
            }
            else
            {
                File.AppendAllText(LogLocation, "INFO :: " + msg);
            }

            File.AppendAllText(LogLocation, Environment.NewLine);
        }
    }
}
