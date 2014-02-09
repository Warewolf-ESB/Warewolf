using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
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
        private const string ServerName = "Warewolf Server.exe";
        private const string StudioName = "Warewolf Studio.exe";
        private const string ServerProcName = "Warewolf Server";
        private const string StudioProcName = "Warewolf Studio";
        private const int ServerTimeOut = 30000;
        private const int StudioTimeOut = 30000;
        private const string ChangesetIDPathFileName = "ForChangesetID.txt";//For getting the changeset ID
        private const string LocalBuildRunDirectory = "C:\\TestDeploy\\";//Local run directory
        private const string RemoteBuildDirectory = "\\\\rsaklfsvrtfsbld\\Automated Builds\\TestRunStaging\\";//Where the zipped build has been staged

        public static string ServerLocation;
        public static string StudioLocation;

        private static object _tumbler = new object();

        /// <summary>
        /// Inits the specified test CTX.
        /// </summary>
        /// <param name="textCtx">The test CTX.</param>
        [AssemblyInitialize()]
        public static void Init(TestContext textCtx)
        {
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

                var getChangesetIDPathFilePath = Path.Combine(Path.GetDirectoryName(textCtx.DeploymentDirectory), "Deployment", ChangesetIDPathFileName);
                var changesetID = GetChangestID(getChangesetIDPathFilePath);
                GetChangesetBuild(changesetID);

                var serverLoc = Path.Combine(Path.GetDirectoryName(LocalBuildRunDirectory), changesetID, "Binaries", ServerName);
                var studioLoc = Path.Combine(Path.GetDirectoryName(LocalBuildRunDirectory), changesetID, "Binaries", StudioName);

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
        }

        static string GetChangestID(string path)
        {
            var changesetID = File.ReadAllText(path);
            return changesetID;
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
            if(_serverProc != null)
            {
                _serverProc.Kill();
            }
            if(_studioProc != null)
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
