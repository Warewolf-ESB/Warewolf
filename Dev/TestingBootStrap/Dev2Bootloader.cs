using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Dev2.Common;

namespace Dev2.Integration.Tests
{
    public static class Dev2Bootloader
    {

        private static Process _serverProc;
        private const string _serverName = "Dev2.Server.exe";

        public static void StartServer(string rootLocation)
        {
            var serverLoc = Path.Combine(Path.GetDirectoryName(rootLocation), _serverName);

            //var args = "/endpointAddress=http://localhost:4315/dsf /nettcpaddress=net.tcp://localhost:73/dsf /webserverport=2234 /webserversslport=2236 /managementEndpointAddress=net.tcp://localhost:5421/dsfManager";

            DateTime now = DateTime.Now;

            ServerLogger.LogMessage("Server Loc -> " + serverLoc);
            ServerLogger.LogMessage("App Server Path -> " + EnvironmentVariables.ApplicationPath);

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

                    ServerLogger.LogMessage("Server Started for Integration Test Run");

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
        }

        public static void StopServer()
        {
            if (_serverProc != null)
            {
                _serverProc.Kill();
                ServerLogger.LogMessage("Server Terminated");
            }
        }
    }
}
