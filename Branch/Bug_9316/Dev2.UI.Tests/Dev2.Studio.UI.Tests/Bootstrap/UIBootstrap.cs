using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dev2.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Studio.UI.Tests.Bootstrap
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
        private const string _serverProcName = "Dev2.Server";
        private const string _studioName = "Dev2.Studio.exe";
        private const string _studioProcName = "Dev2.Studio";

        private static object _tumbler = new object();

        /// <summary>
        /// Inits the specified text CTX.
        /// </summary>
        /// <param name="textCtx">The text CTX.</param>
        [AssemblyInitialize()]
        public static void Init(TestContext textCtx)
        {
            //lock (_tumbler)
            //{
            //    var assembly = Assembly.GetExecutingAssembly();
            //    var loc = assembly.Location;

            //    var serverLoc = Path.Combine(Path.GetDirectoryName(loc), _serverName);
            //    var studioLoc = Path.Combine(Path.GetDirectoryName(loc), _studioName);

            //    //var args = "/endpointAddress=http://localhost:4315/dsf /nettcpaddress=net.tcp://localhost:73/dsf /webserverport=2234 /webserversslport=2236 /managementEndpointAddress=net.tcp://localhost:5421/dsfManager";

            //    ServerLogger.LogMessage("Server Loc -> " + serverLoc);
            //    ServerLogger.LogMessage("Studio Loc -> " + studioLoc);
            //    ServerLogger.LogMessage("App Server Path -> " + EnvironmentVariables.ApplicationPath);

            //    var args = "-t";

            //    ProcessStartInfo startInfo = new ProcessStartInfo();
            //    startInfo.CreateNoWindow = false;
            //    startInfo.UseShellExecute = true;
            //    startInfo.FileName = serverLoc;
            //    //startInfo.RedirectStandardOutput = true;
            //    startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            //    startInfo.Arguments = args;

            //    // Setup studio proc info
            //    ProcessStartInfo studioInfo = new ProcessStartInfo();
            //    studioInfo.CreateNoWindow = false;
            //    studioInfo.UseShellExecute = true;
            //    studioInfo.FileName = studioLoc;
            //    studioInfo.WindowStyle = ProcessWindowStyle.Maximized;


            //    var started = false;
            //    var studioStart = false;
            //    var startCnt = 0;

            //    // term any existing server processes ;)
            //    TerminateProcess(_serverProcName);
            //    // term any existing studio processes ;)
            //    TerminateProcess(_studioProcName);

            //    while (!started && !studioStart && startCnt < 5)
            //    {
            //        try
            //        {
            //            if (!started)
            //            {
            //                _serverProc = Process.Start(startInfo);
            //            }
            //            if (!studioStart)
            //            {
            //                _studioProc = Process.Start(studioInfo);
            //            }

            //            // Wait for server to start
            //            Thread.Sleep(10000); // wait up to 15 seconds for server to start ;)
            //            if (!_serverProc.HasExited)
            //            {
            //                started = true;
            //                ServerLogger.LogMessage("** Server Started for CodedUI Test Run");
            //            }
            //            if (!_studioProc.HasExited)
            //            {
            //                studioStart = true;
            //                ServerLogger.LogMessage("** Studio Started for CodedUI Test Run");
            //            }
            //        }
            //        catch (Exception e)
            //        {
            //            ServerLogger.LogMessage("Exception : " + e.Message);

            //            // most likely a server is already running, kill it and try again ;)
            //            startCnt++;

            //        }
            //        finally
            //        {
            //            if (!started && !studioStart)
            //            {
            //                ServerLogger.LogMessage("** Server Failed to Start for CodedUI Test Run");
            //                // term any existing server processes ;)
            //                TerminateProcess(_serverProcName);
            //                ServerLogger.LogMessage("** Studio Failed to Start for CodedUI Test Run");
            //                // term any existing server processes ;)
            //                TerminateProcess(_studioProcName);
            //            }
            //        }
            //    }
            //}
        }

        /// <summary>
        /// Teardowns this instance.
        /// </summary>
        [AssemblyCleanup()]
        public static void Teardown()
        {
            //if (_serverProc != null)
            //{
            //    _serverProc.Kill();
            //    ServerLogger.LogMessage("Server Terminated");
            //}

            //if (_studioProc != null)
            //{
            //    _studioProc.Kill();
            //    ServerLogger.LogMessage("Studio Terminated");
            //}
        }


        private static void TerminateProcess(string procName)
        {
            ServerLogger.LogMessage("** Kill Process LIKE { " + procName + " }");
            var processName = procName;
            var query = new SelectQuery(@"SELECT * FROM Win32_Process where Name LIKE '%" + processName + "%'");
            //initialize the searcher with the query it is
            //supposed to execute
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(query))
            {
                //execute the query
                ManagementObjectCollection processes = searcher.Get();
                if (processes.Count <= 0)
                {
                    ServerLogger.LogMessage("No processes");
                }
                else
                {

                    foreach (ManagementObject process in processes)
                    {
                        //print process properties

                        process.Get();
                        PropertyDataCollection processProperties = process.Properties;

                        var pid = processProperties["ProcessID"].Value.ToString();

                        ServerLogger.LogMessage("Killed Process { " + pid + " }");

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
    }
}
