using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Studio.UI.Tests.Utils
{
    /// <summary>
    /// Used to bootstrap the server for coded UI test runs
    /// </summary>
    [TestClass]
    public class Bootstrap
    {
        private const bool EnableLocalRestart = false;

        private const string ServerProcName = "Warewolf Server";
        private const string StudioProcName = "Warewolf Studio";
        private const string ServerExeName = ServerProcName + ".exe";
        private const string StudioExeName = StudioProcName + ".exe";
        private const int ServerTimeOut = 3000;
        private const int StudioTimeOut = 12000;

        public static string LogLocation = @"C:\UI_Test.log";
        public static string BuildDirectory = @"C:\Builds\UITestRunWorkspace";//If UI tests are going to be run with no deploy by a build agent (copy this straight out of the build agent config)

        public static string ServerLocation;
        public static string StudioLocation;
        private static string _resourceLocation;
        private static string _serverWorkspaceLocation;
        public TestContext TestContext
        {
            get;
            set;
        }

        public static int WaitMs = 5000;

        public static bool IsLocal = false;

        /// <summary>
        /// Inits the ServerLocation.
        /// </summary>
        /// <param name="testCtx">Test context.</param>
        [AssemblyInitialize]
        public static void AssemblyInit(TestContext testCtx)
        {
            IsLocal = testCtx.Properties["ControllerName"] == null || testCtx.Properties["ControllerName"].ToString() == "localhost:6901";
            ResolvePathsToTestAgainst(!IsLocal ? Path.Combine(testCtx.DeploymentDirectory, "DebugBin") : null);
        }

        private static void ResolvePathsToTestAgainst(string deployDirectory = null)
        {
            ServerLocation = GetProcessPath(TryGetProcess(ServerProcName));
            StudioLocation = GetProcessPath(TryGetProcess(StudioProcName));
            if(!File.Exists(ServerLocation) || !File.Exists(StudioLocation))
            {
                //Try build workspace directory
                ServerLocation = Path.Combine(BuildDirectory, "bin", "ServerbinDebug", ServerExeName);
                StudioLocation = Path.Combine(BuildDirectory, "bin", "StudiobinDebug", StudioExeName);
            }
            if(!File.Exists(ServerLocation) || !File.Exists(StudioLocation))
            {
                //Try debug bin or deployed resources
                ServerLocation = Path.Combine(deployDirectory ?? Path.Combine(Environment.CurrentDirectory, @"..\..\..\Dev2.Server\bin\Debug"), ServerExeName);
                StudioLocation = Path.Combine(deployDirectory ?? Path.Combine(Environment.CurrentDirectory, @"..\..\..\Dev2.Studio\bin\Debug"), StudioExeName);
            }
            if(!File.Exists(ServerLocation))
            {
                LogTestRunMessage("Could not locate server to test against.", true);
                if((ServerLocation.IndexOf("TestResults\\") >= 0) && (ServerLocation.IndexOf("Dev2.") >= 0))
                {
                    throw new FileNotFoundException("Server not found at " + (deployDirectory ?? ServerLocation.Substring(0, ServerLocation.IndexOf("TestResults\\")) + ServerLocation.Substring(ServerLocation.IndexOf("Dev2."))));
                }
                else
                {
                    throw new FileNotFoundException("Server not found at " + ServerLocation);
                }
            }
            if(!File.Exists(StudioLocation))
            {
                LogTestRunMessage("Could not locate studio to test against.", true);
                if((StudioLocation.IndexOf("TestResults\\") >= 0) && (StudioLocation.IndexOf("Dev2.") >= 0))
                {
                    throw new FileNotFoundException("Studio not found at " + (deployDirectory ?? StudioLocation.Substring(0, StudioLocation.IndexOf("TestResults\\")) + StudioLocation.Substring(StudioLocation.IndexOf("Dev2."))));
                }
                else
                {
                    throw new FileNotFoundException("Studio not found at " + StudioLocation);
                }
            }
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

            //Don't touch the server or studio for local runs on a developers desk he might be debugging!
            if(serverProc != null && studioProc != null && !EnableLocalRestart && (Environment.CurrentDirectory.Contains(BuildDirectory) && IsLocal))
            {
                return;
            }
            // term any existing studio processes ;)
            KillProcess(studioProc);
            // term any existing server processes ;)
            KillProcess(serverProc);

            // remove workspaces to avoid mutation issues
            RemoveWorkspaces();

            StartServer(ServerLocation);
            StartStudio(StudioLocation);

            Thread.Sleep(WaitMs);
        }

        /// <summary>
        /// Tear downs this instance.
        /// </summary>
        public static void Teardown()
        {
            //Don't touch the server or studio for local runs on a developers desk he might be debugging!
            if(!EnableLocalRestart && (Environment.CurrentDirectory.Contains(BuildDirectory) && IsLocal))
            {
                return;
            }

            if(File.Exists(ServerLocation) && File.Exists(StudioLocation))
            {
                //Server was deployed and started, stop it now.
                KillProcess(TryGetProcess(ServerProcName));

                //Studio was deployed and started, stop it now.
                KillProcess(TryGetProcess(StudioProcName));
            }
            else
            {
                LogTestRunMessage("Could not locate CodedUI Binaries", true);
            }

            if(!IsLocal)
            {
                // Now clean up next test run ;)
                CloseAllInstancesOfIE();
            }
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
            catch(Exception e)
            {
                LogTestRunMessage(e.Message, true);
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
                        LogTestRunMessage("Started Studio");
                    }
                }
                catch
                {
                    // most likely a studio is already running, kill it and try again ;)
                    LogTestRunMessage("Could not locate Start Studio [ " + StudioLocation + " ] Attempt Count [ " + startCnt + " ]", true);
                    startCnt++;
                }
                finally
                {
                    if(!started)
                    {
                        LogTestRunMessage("Failed To Start Studio.... Aborting", true);
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
                        throw new Exception("Failed to start server at " + location);
                    }
                }
            }
        }

        // Click here to force exit all processes 
        [AssemblyCleanup]
        public static void RunTeardown()
        {
            Teardown();
        }

        private static ManagementObjectCollection TryGetProcess(string procName)
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
            if(Directory.Exists(Path.GetDirectoryName(LogLocation)))
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
}

