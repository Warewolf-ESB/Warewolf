using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Threading;
using Ionic.Zip;
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
        private static string ChangesetID;
        private static TestContext testCtx;
        private static CredentialCache cc;
        private static string LoggingURL = string.Empty;

        private const string ServerName = "Warewolf Server.exe";
        private const string StudioName = "Warewolf Studio.exe";
        private const string ServerProcName = "Warewolf Server";
        private const string StudioProcName = "Warewolf Studio";
        private const int ServerTimeOut = 30000;
        private const int StudioTimeOut = 30000;
        private const string ChangesetIDPathFileName = "BuildID.txt";//For getting the changeset ID (.testsettings file describes its deployment, build process describes its creation)
        private const string LocalBuildRunDirectory = "C:\\TestDeploy\\";//Local run directory
        private const string RemoteBuildDirectory = "\\\\rsaklfsvrtfsbld\\Automated Builds\\TestRunStaging\\";//Where the zipped build has been staged
        private const int WebRequestTimeout = 60000;

        public static string ServerLocation;
        public static Process ServerProc;
        public static string StudioLocation;
        public static Process StudioProc;


        private static object _tumbler = new object();

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
                    ServerLocation = GetProcessPath(serverProcess);
                    StudioLocation = GetProcessPath(studioProcess);
                    return;
                }

                // term any existing studio processes ;)
                KillProcess(studioProcess);

                // term any existing server processes ;)
                KillProcess(serverProcess);

                //init logging
                cc = new CredentialCache();
                cc.Add(new Uri("http://RSAKLFSVRWRWBLD:3142/"), "NTLM", new NetworkCredential("IntegrationTester", "I73573r0", "DEV2"));

                var getChangesetIDPathFilePath = Path.Combine(Path.GetDirectoryName(textCtx.DeploymentDirectory), "Deployment", ChangesetIDPathFileName);
                ReadBuildLabel(getChangesetIDPathFilePath);

                if(LoggingURL == string.Empty)
                {
                    LogBuildEvent("Test agent " + textCtx.Properties["AgentName"] + " started downloading build for coded ui testing");
                }
                GetChangesetBuild(ChangesetID);
                if(LoggingURL == string.Empty)
                {
                    LogBuildEvent("Test agent " + textCtx.Properties["AgentName"] + " finished downloading build for coded ui testing");
                }

                var serverLoc = Path.Combine(Path.GetDirectoryName(LocalBuildRunDirectory), ChangesetID, "Binaries", ServerName);
                var studioLoc = Path.Combine(Path.GetDirectoryName(LocalBuildRunDirectory), ChangesetID, "Binaries", StudioName);

                var args = "-t";

                ProcessStartInfo startInfo = new ProcessStartInfo { CreateNoWindow = false, UseShellExecute = true, Arguments = args };
                //startInfo.RedirectStandardOutput = true;
                //startInfo.WindowStyle = ProcessWindowStyle.Hidden;

                var started = false;
                var startCnt = 0;
                startInfo.FileName = serverLoc;

                while(!started && startCnt < 5)
                {
                    try
                    {
                        _serverProc = Process.Start(startInfo);

                        // Wait for server to start
                        Thread.Sleep(ServerTimeOut); // wait for server to start ;)
                        if(_serverProc != null && !_serverProc.HasExited)
                        {
                            serverProcess = TryGetProcess(ServerProcName);
                            ServerLocation = GetProcessPath(serverProcess);
                            started = true;
                        }
                    }
                    catch
                    {
                        // most likely a server is already running, kill it and try again ;)
                        startCnt++;
                    }
                }
                if(LoggingURL == string.Empty)
                {
                    LogBuildEvent("Test agent " + textCtx.Properties["AgentName"] + " has started running server for coded ui testing");
                }

                started = false;
                startCnt = 0;
                startInfo.FileName = studioLoc;

                while(!started && startCnt < 5)
                {
                    try
                    {
                        _studioProc = Process.Start(startInfo);

                        // Wait for studio to start
                        Thread.Sleep(StudioTimeOut); // wait for server to start ;)
                        if(_studioProc != null && !_studioProc.HasExited)
                        {
                            studioProcess = TryGetProcess(StudioProcName);
                            StudioLocation = GetProcessPath(studioProcess);
                            started = true;
                        }
                    }
                    catch
                    {
                        // most likely a studio is already running, kill it and try again ;)
                        startCnt++;
                    }
                }
                if(LoggingURL == string.Empty)
                {
                    LogBuildEvent("Test agent " + textCtx.Properties["AgentName"] + " has started running studio for coded ui testing");
                }
            }
        }

        static void LogBuildEvent(string LogData)
        {
            var URL = LoggingURL + "?BuildID=" + ChangesetID + "&data=" + System.Web.HttpUtility.UrlEncode(LogData);
            var webRequest = WebRequest.Create(URL);
            webRequest.Credentials = cc;
            webRequest.Timeout = WebRequestTimeout;
            webRequest.GetResponse();
        }

        static void ReadBuildLabel(string getChangesetIDPathFilePath)
        {
            var Lines = File.ReadAllLines(getChangesetIDPathFilePath);
            if(Lines[0] != null && Lines[0].StartsWith("BuildID: ") && Lines[1] != null && Lines[1].StartsWith("LoggingURL: "))
            {
                ChangesetID = Lines[0].Replace("BuildID: ", string.Empty);
                LoggingURL = Lines[0].Replace("Logging: ", string.Empty);
            }
            else
            {
                throw new Exception("Unrecognized build label format.");
            }
        }

        static void GetChangesetBuild(string changesetID)
        {
            var remoteBuildPath = Path.Combine(Path.GetDirectoryName(RemoteBuildDirectory), changesetID + ".zip");

            using(var zippedBuild = ZipFile.Read(remoteBuildPath))
            {
                zippedBuild.ExtractAll(Path.Combine(Path.GetDirectoryName(LocalBuildRunDirectory), changesetID), ExtractExistingFileAction.OverwriteSilently);
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
            try
            {
                Directory.Delete(LocalBuildRunDirectory, true);
            }
            catch(Exception)
            {

            }
            if(LoggingURL == string.Empty)
            {
                LogBuildEvent("Test agent " + testCtx.Properties["AgentName"] + " finished coded ui testing");
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
