using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Threading;
using Dev2.Common;
using Ionic.Zip;
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
        private const int ServerTimeOut = 30000;
        private const string ChangesetIDPathFileName = "BuildID.txt";//For getting the changeset ID (.testsettings file describes its deployment, build process describes its creation)
        private const string LocalBuildRunDirectory = "C:\\TestDeploy\\";//Local run directory
        private const string RemoteBuildDirectory = "\\\\rsaklfsvrtfsbld\\Automated Builds\\TestRunStaging\\";//Where the zipped build has been staged
        private static string LoggingURL = string.Empty;
        private const int WebRequestTimeout = 60000;

        public static string ServerLocation;
        public static Process ServerProc;
        private static string ChangesetID;
        private static TestContext testCtx;
        private static CredentialCache cc;

        private static readonly object _tumbler = new object();

        /// <summary>
        /// Inits the specified test CTX.
        /// </summary>
        /// <param name="textCtx">The test CTX.</param>
        [AssemblyInitialize]
        public static void Init(TestContext textCtx)
        {
            testCtx = textCtx;
            lock(_tumbler)
            {
                var serverProcess = TryGetProcess(ServerProcName);
                if(textCtx.Properties["ControllerName"] == null || textCtx.Properties["ControllerName"].ToString() == "localhost:6901")
                {
                    ServerLocation = GetProcessPath(serverProcess);
                    return;
                }

                //init logging
                cc = new CredentialCache { { new Uri("http://RSAKLFSVRWRWBLD:3142/"), "NTLM", new NetworkCredential("IntegrationTester", "I73573r0", "DEV2") } };

                // term any existing server processes ;)
                KillProcess(serverProcess);

                //get binaries to test against
                var getChangesetIDPathFilePath = Path.Combine(Path.GetDirectoryName(textCtx.DeploymentDirectory), "Deployment", ChangesetIDPathFileName);
                ReadBuildLabel(getChangesetIDPathFilePath);

                if(LoggingURL == string.Empty)
                {
                    LogBuildEvent("Test agent " + textCtx.Properties["AgentName"] + " started downloading build for integration testing");
                }
                var timeBefore = DateTime.Now;
                GetChangesetBuild(ChangesetID);
                if(LoggingURL == string.Empty)
                {
                    LogBuildEvent("Test agent " + textCtx.Properties["AgentName"] + " finished downloading build for integration testing");
                }
                ServerLogger.LogMessage("Downloaded build " + ChangesetID + " in -> " + (DateTime.Now - timeBefore).Seconds + " seconds");

                ServerLocation = Path.Combine(Path.GetDirectoryName(LocalBuildRunDirectory), ChangesetID, "Binaries", ServerName);

                ServerLogger.LogMessage("Server Loc -> " + ServerLocation);
                ServerLogger.LogMessage("App Server Path -> " + EnvironmentVariables.ApplicationPath);

                const string args = "-t";

                ProcessStartInfo startInfo = new ProcessStartInfo { CreateNoWindow = false, UseShellExecute = true, FileName = ServerLocation, Arguments = args };
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
                if(LoggingURL == string.Empty)
                {
                    LogBuildEvent("Test agent " + textCtx.Properties["AgentName"] + " started running server for integration testing");
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
                LoggingURL = Lines[0].Replace("LoggingURL: ", string.Empty);
            }
            else
            {
                throw new Exception("Unrecognized build label format.");
            }
        }

        static void GetChangesetBuild(string changesetID)
        {
            // ReSharper disable AssignNullToNotNullAttribute
            var remoteBuildPath = Path.Combine(Path.GetDirectoryName(RemoteBuildDirectory), changesetID + ".zip");
            // ReSharper restore AssignNullToNotNullAttribute

            using(var zippedBuild = ZipFile.Read(remoteBuildPath))
            {
                zippedBuild.ExtractAll(Path.Combine(Path.GetDirectoryName(LocalBuildRunDirectory), changesetID), ExtractExistingFileAction.OverwriteSilently);
            }
        }

        /// <summary>
        /// Teardowns this instance.
        /// </summary>
        [AssemblyCleanup]
        public static void Teardown()
        {
            if(ServerProc != null)
            {
                ServerProc.Kill();
                ServerLogger.LogMessage("Server Terminated");
            }

            try
            {
                Directory.Delete(LocalBuildRunDirectory, true);
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch(Exception)
            // ReSharper restore EmptyGeneralCatchClause
            {

            }
            if(LoggingURL == string.Empty)
            {
                LogBuildEvent("Test agent " + testCtx.Properties["AgentName"] + " finished integration testing");
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
                // ReSharper disable EmptyGeneralCatchClause
                catch
                // ReSharper restore EmptyGeneralCatchClause
                {
                    // Do nothing
                }
            }
        }
    }
}
