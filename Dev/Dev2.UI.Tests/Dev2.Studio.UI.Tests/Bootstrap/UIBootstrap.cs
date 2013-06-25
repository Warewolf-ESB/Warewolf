using System;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Threading;
using Dev2.Common;
using Dev2.Studio.Utils;
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
        private const string _serverName = "Warewolf Server.exe";
        private const string _serverProcName = "Warewolf Server";
        private const string _studioName = "Warewolf Studio.exe";
        private const string _studioProcName = "Warewolf Studio";

        private const string _exeRoot = @"C:\CodedUI";

        private static object _tumbler = new object();

        //var args = "/endpointAddress=http://localhost:4315/dsf /nettcpaddress=net.tcp://localhost:73/dsf /webserverport=2234 /webserversslport=2236 /managementEndpointAddress=net.tcp://localhost:5421/dsfManager";

        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            // If the destination directory doesn't exist, create it. 
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            // If copying subdirectories, copy them and their contents to new location. 
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }

        /// <summary>
        /// Brittle setup for CodedUI, if exe naming or folder structure changes this needs to be updated!!!
        /// </summary>
        /// <param name="textCtx">The text CTX.</param>
        [AssemblyInitialize()]
        public static void Init(TestContext textCtx)
        {
            lock (_tumbler)
            {
                // TODO : Webs??

                // DeploymentDirectory

                #region Setup - Configs and DLLs ;)

                // Included here to force dll copy for deploy
                WebServer ws;
                WorkflowDesignerUtils wdu;

                // get executing root ;)
                string loc = textCtx.DeploymentDirectory;

                if (!string.IsNullOrEmpty(loc))
                {
                    var svrConfig = "Warewolf Server.exe.config";
                    var stdConfig = "Warewolf Studio.exe.config";
                    var secConfig = "Warewolf Server.exe.secureconfig";
                    var svrConfigLoc = loc + @"\..\..\..\Dev2.Server\" + svrConfig;
                    var studioConfigLoc = loc + @"\..\..\..\Dev2.Studio\" + stdConfig;
                    var secureConfigLoc = loc + @"\..\..\..\Dev2.Server\" + secConfig;

                    // cp server config
                    try
                    {
                        var loc1 = Path.Combine(loc, svrConfig);
                        File.Copy(svrConfigLoc, loc1);
                    }
                    catch (Exception e)
                    {
                        Logger.LogError(e);
                    }

                    // cp studio config
                    try
                    {
                        var loc1 = Path.Combine(loc, stdConfig);
                        File.Copy(studioConfigLoc, loc1);
                    }
                    catch (Exception e)
                    {
                        Logger.LogError(e);
                    }

                    // cp secureconfig
                    try
                    {
                        var loc1 = Path.Combine(loc, secConfig);
                        File.Copy(secureConfigLoc, loc1);
                    }
                    catch (Exception e)
                    {
                        Logger.LogError(e);
                    }

                }

                #endregion

                // Now move to a nice and safe staging location ;)

                #region Stage It

                #region Dir Create
                // remove it
                try
                {
                    Directory.Delete(_exeRoot, true);
                }
                catch (Exception e)
                {
                    Logger.LogError(e);
                }

                // create it
                try
                {
                    Directory.CreateDirectory(_exeRoot);
                }
                catch (Exception e)
                {
                    Logger.LogError(e);
                }

                // create locale folder
                try
                {
                    Directory.CreateDirectory(_exeRoot + @"\locales");
                }
                catch (Exception e)
                {
                    Logger.LogError(e);
                }

                // create help folder
                try
                {
                    Directory.CreateDirectory(_exeRoot + @"\Help");
                }
                catch (Exception e)
                {
                    Logger.LogError(e);
                }


                #endregion

                #region File Stage
                // stage it ;)
                foreach (var file in Directory.GetFiles(loc))
                {
                    try
                    {
                        var fName = Path.GetFileName(file);
                        var tLoc = Path.Combine(_exeRoot, fName);

                        File.Copy(file, tLoc);
                    }
                    catch (Exception e)
                    {
                        Logger.LogError(e);
                    }
                }

                // stage Webs ;)
                var webDir = _exeRoot + @"\Webs\wwwroot";
                if (Directory.Exists(webDir))
                {
                    Directory.Delete(webDir, true);
                }

                DirectoryCopy((loc + @"\..\..\..\Dev2.Server\bin\Debug\Webs\wwwroot"), webDir, true);

                // now stage binaries data 
                foreach (var file in Directory.GetFiles(loc + @"\..\..\..\Binaries"))
                {
                    try
                    {
                        var fName = Path.GetFileName(file);
                        var tLoc = Path.Combine(_exeRoot, fName);

                        File.Copy(file, tLoc);
                    }
                    catch (Exception e)
                    {
                        Logger.LogError(e);
                    }
                }

                // now stage help data 
                foreach (var file in Directory.GetFiles(loc + @"\..\..\..\Dev2.Studio\Help"))
                {
                    try
                    {
                        var fName = Path.GetFileName(file);
                        var tLoc = Path.Combine(_exeRoot, "Help", fName);

                        File.Copy(file, tLoc);
                    }
                    catch (Exception e)
                    {
                        Logger.LogError(e);
                    }
                }


                // C:\Development\Dev\Binaries\CefSharp

                // now stage the CefSharp junk in the correct location ;)
                foreach (var file in Directory.GetFiles(loc + @"\..\..\..\Binaries\CefSharp"))
                {
                    try
                    {
                        var fName = Path.GetFileName(file);
                        File.Copy(file, Path.Combine(_exeRoot, fName));
                    }
                    catch (Exception e)
                    {
                        Logger.LogError(e);
                    }
                }

                // now stage the CefSharp locales in the correct location ;)
                foreach (var file in Directory.GetFiles(loc + @"\..\..\..\Binaries\CefSharp\locales"))
                {
                    try
                    {
                        var fName = Path.GetFileName(file);
                        File.Copy(file, Path.Combine(_exeRoot, "locales", fName));
                    }
                    catch (Exception e)
                    {
                        Logger.LogError(e);
                    }
                }

                #endregion

                #endregion

                Logger.LogMessage("Base Loc -> " + loc);

                var serverLoc = Path.Combine(_exeRoot, _serverName);
                var studioLoc = Path.Combine(_exeRoot, _studioName);

                Logger.LogMessage("Server Loc -> " + serverLoc);
                Logger.LogMessage("Studio Loc -> " + studioLoc);
                Logger.LogMessage("App Server Path -> " + EnvironmentVariables.ApplicationPath);

                var args = "-t";

                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.CreateNoWindow = false;
                startInfo.UseShellExecute = true;
                startInfo.FileName = serverLoc;
                //startInfo.RedirectStandardOutput = true;
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.Arguments = args;

                // Setup studio proc info
                ProcessStartInfo studioInfo = new ProcessStartInfo();
                studioInfo.CreateNoWindow = false;
                studioInfo.UseShellExecute = true;
                studioInfo.FileName = studioLoc;
                studioInfo.WindowStyle = ProcessWindowStyle.Maximized;


                var started = false;
                var studioStart = false;
                var startCnt = 0;

                // term any existing server processes ;)
                TerminateProcess(_serverProcName);
                // term any existing studio processes ;)
                TerminateProcess(_studioProcName);

                while (!started && !studioStart && startCnt < 5)
                {
                    try 
                    {
                        if (!started)
                        {
                            _serverProc = Process.Start(startInfo);
                        }
                        if (!studioStart)
                        {
                            _studioProc = Process.Start(studioInfo);
                        }

                        // Wait for server to start
                        Thread.Sleep(10000); // wait up to 15 seconds for server to start ;)
                        if (!_serverProc.HasExited)
                        {
                            started = true;
                            Logger.LogMessage("** Server Started for CodedUI Test Run");
                        }
                        if (!_studioProc.HasExited)
                        {
                            studioStart = true;
                            Logger.LogMessage("** Studio Started for CodedUI Test Run");
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.LogMessage("Exception : " + e.Message);

                        // most likely a server is already running, kill it and try again ;)
                        startCnt++;

                    }
                    finally
                    {
                        if (!started && !studioStart)
                        {
                            Logger.LogMessage("** Server Failed to Start for CodedUI Test Run");
                            // term any existing server processes ;)
                            TerminateProcess(_serverProcName);
                            Logger.LogMessage("** Studio Failed to Start for CodedUI Test Run");
                            // term any existing server processes ;)
                            TerminateProcess(_studioProcName);
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
            if (_serverProc != null)
            {
                _serverProc.Kill();
                Logger.LogMessage("Server Terminated");
            }

            if (_studioProc != null)
            {
                _studioProc.Kill();
                Logger.LogMessage("Studio Terminated");
            }
        }


        private static void TerminateProcess(string procName)
        {
            Logger.LogMessage("** Kill Process LIKE { " + procName + " }");
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
                    Logger.LogMessage("No processes");
                }
                else
                {

                    foreach (ManagementObject process in processes)
                    {
                        //print process properties

                        process.Get();
                        PropertyDataCollection processProperties = process.Properties;

                        var pid = processProperties["ProcessID"].Value.ToString();

                        Logger.LogMessage("Killed Process { " + pid + " }");

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
