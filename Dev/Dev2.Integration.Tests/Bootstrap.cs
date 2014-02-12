using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Integration.Tests
{
    /// <summary>
    /// Used to bootstrap the server for integration test runs ;)
    /// </summary>
    [TestClass]
    public class Bootstrap
    {
        private static Process _serverProc;
        private const string _serverName = "Warewolf Server.exe";


        private static readonly object _tumbler = new object();

        /// <summary>
        /// Inits the specified text CTX.
        /// </summary>
        /// <param name="textCtx">The text CTX.</param>
        [AssemblyInitialize]
        public static void Init(TestContext textCtx)
        {
            if(textCtx.Properties["ControllerName"] == null || textCtx.Properties["ControllerName"].Equals("localhost:6901")) return;

            lock(_tumbler)
            {
                if(File.Exists("C:\\Users\\IntegrationTester\\Desktop\\uitest.log"))
                {
                    File.Delete("C:\\Users\\IntegrationTester\\Desktop\\uitest.log");
                }

                var assembly = Assembly.GetExecutingAssembly();
                var loc = assembly.Location;

                // ReSharper disable AssignNullToNotNullAttribute
                var serverLoc = Path.Combine(Path.GetDirectoryName(loc), _serverName);
                // ReSharper restore AssignNullToNotNullAttribute

                //var args = "/endpointAddress=http://localhost:4315/dsf /nettcpaddress=net.tcp://localhost:73/dsf /webserverport=2234 /webserversslport=2236 /managementEndpointAddress=net.tcp://localhost:5421/dsfManager";

                const string args = "-t";

                ProcessStartInfo startInfo = new ProcessStartInfo { CreateNoWindow = false, UseShellExecute = true, FileName = serverLoc, Arguments = args };
                //startInfo.RedirectStandardOutput = true;
                //startInfo.WindowStyle = ProcessWindowStyle.Hidden;

                var started = false;
                var startCnt = 0;

                while(!started && startCnt < 5)
                {
                    try
                    {
                        _serverProc = Process.Start(startInfo);

                        // Wait for server to start
                        Thread.Sleep(30000); // wait up to 30 seconds for server to start ;)
                        if(_serverProc != null && !_serverProc.HasExited)
                        {
                            started = true;
                        }
                    }
                    catch(Exception e)
                    {
                        try
                        {
                            File.WriteAllText("C:\\Users\\IntegrationTester\\Desktop\\uitest.log", @"Exception : " + e.Message + @" " + serverLoc);
                        }
                        // ReSharper disable EmptyGeneralCatchClause
                        catch
                        // ReSharper restore EmptyGeneralCatchClause
                        {
                        }

                        // most likely a server is already running, kill it and try again ;)
                        startCnt++;
                    }
                }
            }
        }

        /// <summary>
        /// Teardowns this instance.
        /// </summary>
        [AssemblyCleanup]
        public static void Teardown()
        {
            if(_serverProc != null)
            {
                _serverProc.Kill();
            }
        }
    }
}
