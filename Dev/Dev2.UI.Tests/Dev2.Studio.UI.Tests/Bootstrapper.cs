using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using Dev2.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Studio.UI.Tests
{
    /// <summary>
    /// Used to bootstrap the server for integration test runs ;)
    /// </summary>
    [TestClass()]
    public class Bootstrap
    {
        private static Process _serverProc;
        private static Process _studioProc;
        private const string _serverName = "Dev2.Server.exe";
        private const string _studioName = "Dev2.Studio.exe";

        /// <summary>
        /// Inits the specified text CTX.
        /// </summary>
        /// <param name="textCtx">The text CTX.</param>
        [AssemblyInitialize()]
        public static void Init(TestContext textCtx)
        {

            var assembly = Assembly.GetExecutingAssembly();
            var loc = assembly.Location;

            var baseLoc = Path.GetDirectoryName(loc);

            var serverLoc = Path.Combine(baseLoc, _serverName);
            var studioLoc = Path.Combine(baseLoc, _studioName);

            DateTime now = DateTime.Now;

            ServerLogger.LogMessage("Server Loc -> " + serverLoc);
            ServerLogger.LogMessage("Server Loc -> " + studioLoc);
            ServerLogger.LogMessage("App Server Path -> " + EnvironmentVariables.ApplicationPath);

            #region Server Start

            var args = "-t";

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = false;
            startInfo.UseShellExecute = true;
            startInfo.FileName = serverLoc;
            //startInfo.RedirectStandardOutput = true;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.Arguments = args;

            var started = false;
            var startCnt = 0;

            while (!started && startCnt < 5)
            {
                try
                {
                    _serverProc = Process.Start(startInfo);
                    started = true;

                    // Wait for server to start
                    Thread.Sleep(10000);

                    ServerLogger.LogMessage("Server Started for UI Test Run");

                }
                catch (Exception e)
                {
                    ServerLogger.LogMessage("Exception : " + e.Message);

                    // most likely a server is already running, kill it and try again ;)
                    startCnt++;

                    // term any existing server processes ;)
                    Process[] procs = Process.GetProcessesByName(_serverName);
                    foreach (var proc in procs)
                    {
                        proc.Kill();
                    }
                }
            }

            #endregion

            #region Studio Start

            ProcessStartInfo startInfo2 = new ProcessStartInfo();
            startInfo2.CreateNoWindow = false;
            startInfo2.UseShellExecute = true;
            startInfo2.FileName = studioLoc;
            //startInfo.RedirectStandardOutput = true;
            startInfo2.WindowStyle = ProcessWindowStyle.Maximized;
            startInfo2.Arguments = args;

            started = false;
            startCnt = 0;

            while (!started && startCnt < 5)
            {
                try
                {
                    _serverProc = Process.Start(startInfo);
                    started = true;

                    // Wait for server to start
                    Thread.Sleep(10000);

                    ServerLogger.LogMessage("Studio Started for UI Test Run");

                }
                catch (Exception e)
                {
                    ServerLogger.LogMessage("Exception : " + e.Message);

                    // most likely a server is already running, kill it and try again ;)
                    startCnt++;

                    // term any existing server processes ;)
                    Process[] procs = Process.GetProcessesByName(_studioName);
                    foreach (var proc in procs)
                    {
                        proc.Kill();
                    }
                }
            }

            #endregion
        }

        /// <summary>
        /// Teardowns this instance.
        /// </summary>
        [AssemblyCleanup()]
        public static void Teardown()
        {
            if (_serverProc != null)
            {
                _serverProc.Kill();
                ServerLogger.LogMessage("UI Testing Server Terminated");
            }

            if (_studioProc != null)
            {
                _studioProc.Kill();
                ServerLogger.LogMessage("UI Testing Studio Terminated");
            }
        }
    }
}
