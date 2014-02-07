using System;
using System.Diagnostics;
using System.IO;
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
        private const int ServerTimeOut = 30000;
        private const int StudioTimeOut = 30000;
        private const string ChangesetIDPathFileName = "ForChangesetID.txt";//For getting the changeset ID
        private const string LocalBuildRunDirectory = "C:\\TestDeploy\\";//Local run directory
        private const string RemoteBuildDirectory = "\\\\rsaklfsvrtfsbld\\Automated Builds\\TestRunStaging\\";//Where the zipped build has been staged


        private static object _tumbler = new object();

        /// <summary>
        /// Inits the specified test CTX.
        /// </summary>
        /// <param name="textCtx">The test CTX.</param>
        [AssemblyInitialize()]
        public static void Init(TestContext textCtx)
        {
            if(textCtx.Properties["ControllerName"] == null || textCtx.Properties["ControllerName"].ToString() == "localhost:6901") return;

            lock(_tumbler)
            {
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
    }
}
