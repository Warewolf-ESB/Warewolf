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
    [TestClass()]
    public class Bootstrap
    {
        private static Process _serverProc;
        private static Process _studioProc;
        private const string _serverName = "Warewolf Server.exe";
        private const string _studioName = "Warewolf Studio.exe";


        private static object _tumbler = new object();

        /// <summary>
        /// Inits the specified text CTX.
        /// </summary>
        /// <param name="textCtx">The text CTX.</param>
        [AssemblyInitialize()]
        public static void Init(TestContext textCtx)
        {
            lock(_tumbler)
            {
                if(File.Exists("C:\\Users\\IntegrationTester\\Desktop\\uitest.log"))
                {
                    File.Delete("C:\\Users\\IntegrationTester\\Desktop\\uitest.log");
                }

                string userName = string.Empty;
                var windowsIdentity = System.Security.Principal.WindowsIdentity.GetCurrent();
                if(windowsIdentity != null)
                {
                    userName = windowsIdentity.Name;
                }
                if(userName != "DEV2\\IntegrationTester")
                {
                    if(Directory.Exists("C:\\Users\\IntegrationTester"))
                    {
                        try
                        {
                            File.WriteAllText("C:\\Users\\IntegrationTester\\Desktop\\uitest.log", "Failed to bootstrap, user not recognized : " + userName);
                        }
                        // ReSharper disable once EmptyGeneralCatchClause
                        catch
                        {
                        }
                    }
                    return;
                }

                var assembly = Assembly.GetExecutingAssembly();
                var loc = assembly.Location;

                var serverLoc = Path.Combine(Path.GetDirectoryName(loc), _serverName);
                var studioLoc = Path.Combine(Path.GetDirectoryName(loc), _studioName);

                //var args = "/endpointAddress=http://localhost:4315/dsf /nettcpaddress=net.tcp://localhost:73/dsf /webserverport=2234 /webserversslport=2236 /managementEndpointAddress=net.tcp://localhost:5421/dsfManager";

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
                            File.WriteAllText("C:\\Users\\IntegrationTester\\Desktop\\uitest.log", "Exception : " + e.Message + " " + serverLoc);
                        }
                        // ReSharper disable once EmptyGeneralCatchClause
                        catch
                        {
                        }

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
                        Thread.Sleep(30000); // wait up to 30 seconds for server to start ;)
                        if(_studioProc != null && !_studioProc.HasExited)
                        {
                            started = true;
                        }
                    }
                    catch(Exception e)
                    {
                        try
                        {
                            File.WriteAllText("C:\\Users\\IntegrationTester\\Desktop\\uitest.log", "Exception : " + e.Message + " " + serverLoc);
                        }
                        // ReSharper disable once EmptyGeneralCatchClause
                        catch
                        {
                        }

                        // most likely a studio is already running, kill it and try again ;)
                        startCnt++;
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
            }
            if(_studioProc != null)
            {
                _studioProc.Kill();
            }
        }
    }
}
