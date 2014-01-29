using System;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Reflection;
using System.Threading;
using Dev2.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Integration.Tests
{
    /// <summary>
    /// Used to bootstrap the server for integration test runs ;)
    /// </summary>
    [TestClass()]
    public class Bootstrap
    {
        private static Process _serverProc;
        private const string _serverName = "Warewolf Server.exe";
        private const string _serverProcName = "Warewolf Server";


        private static object _tumbler = new object();

        /// <summary>
        /// Inits the specified text CTX.
        /// </summary>
        /// <param name="textCtx">The text CTX.</param>
        [AssemblyInitialize()]
        public static void Init(TestContext textCtx)
        {
            if(textCtx.Properties["ControllerName"].Equals("localhost:6901")) return;

            lock(_tumbler)
            {
                if(File.Exists("C:\\Users\\IntegrationTester\\Desktop\\integrationtest.log"))
                {
                    File.Delete("C:\\Users\\IntegrationTester\\Desktop\\integrationtest.log");
                }

                var assembly = Assembly.GetExecutingAssembly();
                var loc = assembly.Location;

                var serverLoc = Path.Combine(Path.GetDirectoryName(loc), _serverName);

                //var args = "/endpointAddress=http://localhost:4315/dsf /nettcpaddress=net.tcp://localhost:73/dsf /webserverport=2234 /webserversslport=2236 /managementEndpointAddress=net.tcp://localhost:5421/dsfManager";

                ServerLogger.LogMessage("Server Loc -> " + serverLoc);
                ServerLogger.LogMessage("App Server Path -> " + EnvironmentVariables.ApplicationPath);

                var args = "-t";

                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.CreateNoWindow = false;
                startInfo.UseShellExecute = true;
                startInfo.FileName = serverLoc;
                //startInfo.RedirectStandardOutput = true;
                //startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.Arguments = args;

                var started = false;
                var startCnt = 0;

                // term any existing server processes ;)
                TerminateProcess(_serverProcName);

                while(!started && startCnt < 5)
                {
                    try
                    {
                        _serverProc = Process.Start(startInfo);

                        // Wait for server to start
                        Thread.Sleep(30000); // wait up to 30 seconds for server to start ;)
                        if(!_serverProc.HasExited)
                        {
                            started = true;
                            ServerLogger.LogMessage("** Server Started for Integration Test Run");
                        }
                    }
                    catch(Exception e)
                    {
                        ServerLogger.LogMessage("Exception : " + e.Message);

                        try
                        {
                            File.WriteAllText("C:\\Users\\IntegrationTester\\Desktop\\integrationtest.log", "Exception : " + e.Message + " " + serverLoc);
                        }
                        // ReSharper disable once EmptyGeneralCatchClause
                        catch
                        {
                        }

                        // most likely a server is already running, kill it and try again ;)
                        startCnt++;


                    }
                    finally
                    {
                        if(!started)
                        {
                            ServerLogger.LogMessage("** Server Failed to Start for Integration Test Run");
                            // term any existing server processes ;)
                            TerminateProcess(_serverProcName);
                        }
                    }
                }
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
                ServerLogger.LogMessage("Server Terminated");
            }
        }


        private static void TerminateProcess(string procName)
        {
            ServerLogger.LogMessage("** Kill Process LIKE { " + procName + " }");
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
                }
                else
                {

                    foreach(ManagementObject process in processes)
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
