
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime;
using System.Security.Principal;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Xml;
using CommandLine;
using Dev2.Activities;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Monitoring;
using Dev2.Common.Wrappers;
using Dev2.Data;
using Dev2.Diagnostics.Debug;
using Dev2.Diagnostics.Logging;
using Dev2.Instrumentation;
using Dev2.PerformanceCounters.Management;
using Dev2.Runtime;
using Dev2.Runtime.ESB.Execution;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Security;
using Dev2.Runtime.WebServer;
using Dev2.Services.Security.MoqInstallerActions;
using Dev2.Workspaces;
using log4net.Config;
using WarewolfCOMIPC.Client;

namespace Dev2
{
    sealed class ServerLifecycleManager : IDisposable
    {
        static ServerLifecycleManager _singleton;
        
        static int Main(string[] arguments)
        {
            try
            {
                using (new MemoryFailPoint(2048))
                {
                    return RunMain(arguments);
                }
            }
            catch (InsufficientMemoryException)
            {
                return RunMain(arguments);
            }

        }

        static int RunMain(string[] arguments)
        {
            const int Result = 0;
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                Dev2Logger.Fatal("Server has crashed!!!", args.ExceptionObject as Exception, "Warewolf Fatal");
            };
            if (Environment.UserInteractive)
            {
                Dev2Logger.Info("** Starting In Interactive Mode **", "Warewolf Info");
                using (_singleton = new ServerLifecycleManager(arguments))
                {
                    _singleton.Run(true);
                }

                _singleton = null;
            }
            else
            {
                Dev2Logger.Info("** Starting In Service Mode **", "Warewolf Info");
                using (var service = new ServerLifecycleManagerService())
                {
                    ServiceBase.Run(service);
                }
            }
            return Result;
        }

        
        public class ServerLifecycleManagerService : ServiceBase
        {
            public ServerLifecycleManagerService()
            {
                ServiceName = ServiceName;
                CanPauseAndContinue = false;
            }

            protected override void OnStart(string[] args)
            {
                Dev2Logger.Info("** Service Started **", "Warewolf Info");
                _singleton = new ServerLifecycleManager(null);
                _singleton.Run(false);
            }

            protected override void OnStop()
            {
                Dev2Logger.Info("** Service Stopped **", "Warewolf Info");
                _singleton.Stop(false, 0);
                _singleton = null;
            }
        }

        bool _isDisposed;
        bool _isWebServerEnabled;
        bool _isWebServerSslEnabled;
        Dev2Endpoint[] _endpoints;
        Timer _timer;
        IDisposable _owinServer;
        readonly IPulseLogger _pulseLogger;
        private int _daysToKeepTempFiles;
        private readonly PulseTracker _pulseTracker;
        private IpcClient _ipcIpcClient;
        
        
        ServerLifecycleManager(string[] arguments)
        {
            _pulseLogger = new PulseLogger(60000);
            _pulseLogger.Start();
            _pulseTracker = new PulseTracker(TimeSpan.FromDays(1).TotalMilliseconds);
            _pulseTracker.Start();
            SetWorkingDirectory();
            MoveSettingsFiles();
            var settingsConfigFile = EnvironmentVariables.ServerLogSettingsFile;
            if (!File.Exists(settingsConfigFile))
            {
                File.WriteAllText(settingsConfigFile, GlobalConstants.DefaultServerLogFileConfig);
            }
            try
            {
                Dev2Logger.AddEventLogging(settingsConfigFile, "Warewolf Server");
                XmlConfigurator.ConfigureAndWatch(new FileInfo(settingsConfigFile));
            }
            catch (Exception e)
            {
                Dev2Logger.Error("Error in startup.", e, "Warewolf Error");
            }
            Common.Utilities.ServerUser = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            SetupTempCleanupSetting();
        }

        private static void MoveSettingsFiles()
        {
            if (File.Exists("Settings.config"))
            {
                if (!Directory.Exists(EnvironmentVariables.ServerSettingsFolder))
                {
                    Directory.CreateDirectory(EnvironmentVariables.ServerSettingsFolder);
                }
                if (!File.Exists(EnvironmentVariables.ServerLogSettingsFile))
                {
                    File.Copy("Settings.config", EnvironmentVariables.ServerLogSettingsFile);
                }
            }
            if (File.Exists("secure.config"))
            {
                if (!Directory.Exists(EnvironmentVariables.ServerSettingsFolder))
                {
                    Directory.CreateDirectory(EnvironmentVariables.ServerSettingsFolder);
                }
                if (!File.Exists(EnvironmentVariables.ServerSecuritySettingsFile))
                {
                    File.Copy("secure.config", EnvironmentVariables.ServerSecuritySettingsFile);
                }
            }
        }

        private void SetupTempCleanupSetting()
        {
            var daysToKeepTempFilesValue = ConfigurationManager.AppSettings.Get("DaysToKeepTempFiles");
            if (!string.IsNullOrEmpty(daysToKeepTempFilesValue))
            {
                int daysToKeepTempFiles;
                if (int.TryParse(daysToKeepTempFilesValue, out daysToKeepTempFiles))
                {
                    _daysToKeepTempFiles = daysToKeepTempFiles;
                }
            }
        }
        
        void Run(bool interactiveMode)
        {
            Tracker.StartServer();

            // ** Perform Moq Installer Actions For Development ( DEBUG config ) **
#if DEBUG
            try
            {
                var miq = MoqInstallerActionFactory.CreateInstallerActions();
                miq.ExecuteMoqInstallerActions();
            }
            catch (Exception e)
            {
                throw new Exception("Ensure you are running as an Administrator. Mocking installer actions for DEBUG config failed to create Warewolf Administrators group and/or to add current user to it [ " + e.Message + " ]", e);
            }
#endif

            try
            {
                SetWorkingDirectory();
                LoadHostSecurityProvider();
                MigrateOldTests();
                InitializeServer();
                LoadSettingsProvider();
                ConfigureLoggging();
                _ipcIpcClient = IpcClient.GetIPCExecutor();
                var catalog = LoadResourceCatalog();
                StartWebServer();
                _timer = new Timer(PerformTimerActions, null, 1000, GlobalConstants.NetworkComputerNameQueryFreq);
                StartPulseLogger();
                LoadPerformanceCounters();
                LoadServerWorkspace();
                LoadActivityCache(catalog);
                LoadTestCatalog();
                ServerLoop(interactiveMode);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Dev2Logger.Error("Error Starting Server", e, "Warewolf Error");
                Stop(true, 0);
            }

        }

        void StartPulseLogger()
        {
            _pulseLogger.Start();
            _pulseTracker.Start();
        }

        void PerformTimerActions(object state)
        {
            GetComputerNames.GetComputerNamesList();
            if (_daysToKeepTempFiles != 0)
            {
                DeleteTempFiles();
            }
        }
        
        static void PreloadReferences()
        {
            Write("Preloading assemblies...  ");
            var currentAsm = typeof(ServerLifecycleManager).Assembly;
            var inspected = new HashSet<string> { currentAsm.GetName().ToString(), "GroupControls" };
            LoadReferences(currentAsm, inspected);
            WriteLine("done.");
        }
        
        static void LoadReferences(Assembly asm, HashSet<string> inspected)
        {
            var allReferences = asm.GetReferencedAssemblies();

            foreach (var toLoad in allReferences)
            {
                if (!inspected.Contains(toLoad.Name))
                {
                    inspected.Add(toLoad.Name);
                    var loaded = AppDomain.CurrentDomain.Load(toLoad);
                    LoadReferences(loaded, inspected);
                }
            }
        }


        private void DeleteTempFiles()
        {
            var tempPath = Path.Combine(GlobalConstants.TempLocation, "Warewolf", "Debug");
            DeleteTempFiles(tempPath);
            var schedulerTempPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), GlobalConstants.SchedulerDebugPath);
            DeleteTempFiles(schedulerTempPath);
        }

        private void DeleteTempFiles(string tempPath)
        {
            if (Directory.Exists(tempPath))
            {
                var dir = new DirectoryInfo(tempPath);
                var files = dir.GetFiles();
                var filesToDelete = files.Where(info =>
                {
                    var maxDaysToKeepTimeSpan = new TimeSpan(_daysToKeepTempFiles, 0, 0, 0);
                    var time = DateTime.Now.Subtract(info.CreationTime);
                    return time > maxDaysToKeepTimeSpan;
                }).ToList();

                foreach (var fileInfo in filesToDelete)
                {
                    fileInfo.Delete();
                }
            }
        }

        void Stop(bool didBreak, int result)
        {

            Tracker.Stop();

            if (!didBreak)
            {
                Dispose();
            }

            Write($"Exiting with exitcode {result}");
        }

        void ServerLoop(bool interactiveMode)
        {

            if (interactiveMode)
            {
                Write("Press <ENTER> to terminate service and/or web server if started");
                if (EnvironmentVariables.IsServerOnline)
                {
                    Console.ReadLine();
                }
                else
                {
                    Write("Failed to start Server");
                }
                Stop(false, 0);
            }
        }

        void SetWorkingDirectory()
        {
            try
            {
                
                Directory.SetCurrentDirectory(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            }
            catch (Exception e)
            {
                Fail("Unable to set working directory.", e);
            }
        }
        
        void InitializeServer()
        {
            try
            {
                string webServerSslPort;
                string webServerPort;

                GlobalConstants.WebServerPort = webServerPort = ConfigurationManager.AppSettings["webServerPort"];
                GlobalConstants.WebServerSslPort = webServerSslPort = ConfigurationManager.AppSettings["webServerSslPort"];

                _isWebServerEnabled = false;

                bool.TryParse(ConfigurationManager.AppSettings["webServerEnabled"], out _isWebServerEnabled);
                bool.TryParse(ConfigurationManager.AppSettings["webServerSslEnabled"], out _isWebServerSslEnabled);

                if (_isWebServerEnabled)
                {
                    if (string.IsNullOrEmpty(webServerPort) && _isWebServerEnabled)
                    {
                        throw new ArgumentException("Web server port not set but web server is enabled. Please set the webServerPort value in the configuration file.");
                    }

                    int realPort;

                    if (!int.TryParse(webServerPort, out realPort))
                    {
                        throw new ArgumentException("Web server port is not valid. Please set the webServerPort value in the configuration file.");
                    }

                    var endpoints = new List<Dev2Endpoint>();

                    var httpEndpoint = new IPEndPoint(IPAddress.Any, realPort);
                    var httpUrl = $"http://*:{webServerPort}/";
                    endpoints.Add(new Dev2Endpoint(httpEndpoint, httpUrl));

                    EnvironmentVariables.WebServerUri = httpUrl.Replace("*", Environment.MachineName);
                    EnableSSLForServer(webServerSslPort, endpoints);

                    _endpoints = endpoints.ToArray();
                }

            }
            catch (Exception ex)
            {
                Fail("Server initialization failed", ex);
            }
        }


        private void EnableSSLForServer(string webServerSslPort, List<Dev2Endpoint> endpoints)
        {
            if (!string.IsNullOrEmpty(webServerSslPort) && _isWebServerSslEnabled)
            {
                int realWebServerSslPort;
                int.TryParse(webServerSslPort, out realWebServerSslPort);

                var sslCertPath = ConfigurationManager.AppSettings["sslCertificateName"];

                if (!string.IsNullOrEmpty(sslCertPath))
                {
                    var httpsEndpoint = new IPEndPoint(IPAddress.Any, realWebServerSslPort);
                    var httpsUrl = $"https://*:{webServerSslPort}/";
                    var canEnableSsl = HostSecurityProvider.Instance.EnsureSsl(sslCertPath, httpsEndpoint);

                    if (canEnableSsl)
                    {
                        endpoints.Add(new Dev2Endpoint(httpsEndpoint, httpsUrl, sslCertPath));
                    }
                    else
                    {
                        WriteLine("Could not start webserver to listen for SSL traffic with cert [ " + sslCertPath + " ]");
                    }
                }
            }
        }
        
        
        internal void CleanupServer()
        {
            try
            {
                if (_owinServer != null)
                {
                    _owinServer.Dispose();
                    _owinServer = null;
                }
                if (_ipcIpcClient != null)
                {
                    _ipcIpcClient.Dispose();
                    _ipcIpcClient = null;
                }
                DebugDispatcher.Instance.Shutdown();
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }

        static void Fail(string message, Exception e)
        {
            var ex = e;
            var errors = new StringBuilder();
            while (ex != null)
            {
                errors.AppendLine(ex.Message);
                errors.AppendLine(ex.StackTrace);
                ex = ex.InnerException;
            }

            WriteLine("Critical Failure: " + message);
            WriteLine(errors.ToString());

            WriteLine("");
        }
        
        ~ServerLifecycleManager()
        {
            Dispose(false);
        }
        
        public void Dispose()
        {
            if (_isDisposed)
                return;
            _isDisposed = true;
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        void Dispose(bool disposing)
        {

            if (disposing)
            {
                CleanupServer();
            }

            if (_timer != null)
            {
                _timer.Dispose();
                _timer = null;
            }

            _owinServer = null;
        }

        void LoadPerformanceCounters()
        {
            try
            {
                var perf = new PerformanceCounterPersistence(new FileWrapper());
                var register = new WarewolfPerformanceCounterRegister(perf.LoadOrCreate(), perf.LoadOrCreateResourcesCounters(perf.DefaultResourceCounters));
                var locater = new WarewolfPerformanceCounterManager(register.Counters, register.ResourceCounters, register, perf);
                locater.CreateCounter(Guid.Parse("a64fc548-3045-407d-8603-2a7337d874a6"), WarewolfPerfCounterType.ExecutionErrors, "workflow1");
                locater.CreateCounter(Guid.Parse("a64fc548-3045-407d-8603-2a7337d874a6"), WarewolfPerfCounterType.AverageExecutionTime, "workflow1");
                locater.CreateCounter(Guid.Parse("a64fc548-3045-407d-8603-2a7337d874a6"), WarewolfPerfCounterType.ConcurrentRequests, "workflow1");
                locater.CreateCounter(Guid.Parse("a64fc548-3045-407d-8603-2a7337d874a6"), WarewolfPerfCounterType.RequestsPerSecond, "workflow1");


                CustomContainer.Register<IWarewolfPerformanceCounterLocater>(locater);
                CustomContainer.Register<IPerformanceCounterRepository>(locater);
            }
            catch (Exception err)
            {
                // ignored
                Dev2Logger.Error(err, "Warewolf Error");
            }
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        /// <author>Trevor.Williams-Ros</author>
        /// <date>2013/03/13</date>
        ResourceCatalog LoadResourceCatalog()
        {
            
            MigrateOldResources();
            ValidateResourceFolder();
            Write("Loading resource catalog...  ");
            var catalog = ResourceCatalog.Instance;
            WriteLine("done.");
            return catalog;
        }

        void LoadTestCatalog()
        {
            
            Write("Loading Test catalog...  ");
            TestCatalog.Instance.Load();
            WriteLine("done.");
        }

        private static void LoadActivityCache(ResourceCatalog catalog)
        {
            PreloadReferences();
            CustomContainer.Register<IActivityParser>(new ActivityParser());
            Write("Loading resource activity cache...  ");
            catalog.LoadServerActivityCache();
            CustomContainer.Register<IExecutionManager>(ExecutionManager.Instance);
            WriteLine("done.");            
            SetStarted();
        }

        static void MigrateOldResources()
        {
            var serverBinResources = Path.Combine(EnvironmentVariables.ApplicationPath, "Resources");
            if (!Directory.Exists(EnvironmentVariables.ResourcePath) && Directory.Exists(serverBinResources))
            {
                DirectoryHelper.Copy(serverBinResources, EnvironmentVariables.ResourcePath, true);
                DirectoryHelper.CleanUp(serverBinResources);
            }
        }

        static void MigrateOldTests()
        {
            var serverBinTests = Path.Combine(EnvironmentVariables.ApplicationPath, "Tests");
            if (!Directory.Exists(EnvironmentVariables.TestPath) && Directory.Exists(serverBinTests))
            {
                DirectoryHelper.Copy(serverBinTests, EnvironmentVariables.TestPath, true);
                DirectoryHelper.CleanUp(serverBinTests);
            }
        }

        static void ValidateResourceFolder()
        {
            var folder = EnvironmentVariables.ResourcePath;
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
        }

        /// <summary>
        /// PBI 1018 - Loads the settings provider.
        /// </summary>
        /// <author>Trevor.Williams-Ros</author>
        /// <date>2013/03/07</date>
        void LoadSettingsProvider()
        {
            Write("Loading settings provider...  ");
            Runtime.Configuration.SettingsProvider.WebServerUri = EnvironmentVariables.WebServerUri;
            WriteLine("done.");
        }

        void ConfigureLoggging()
        {
            try
            {
                Write("Configure logging...  ");

                var instance = Runtime.Configuration.SettingsProvider.Instance;
                var settings = instance.Configuration;
                WorkflowLoggger.LoggingSettings = settings.Logging;

                WriteLine("done.");
            }
            catch (Exception e)
            {
                Write("fail.");
                WriteLine(e.Message);
            }
        }

        void LoadServerWorkspace()
        {

            Write("Loading server workspace...  ");
            
            var instance = WorkspaceRepository.Instance;
            
            WriteLine("done.");
        }

        void LoadHostSecurityProvider()
        {
            
            var instance = HostSecurityProvider.Instance;
            
        }

        void StartWebServer()
        {
            if (_isWebServerEnabled || _isWebServerSslEnabled)
            {
                try
                {
                    try
                    {
                        _owinServer = WebServerStartup.Start(_endpoints);
                        EnvironmentVariables.IsServerOnline = true;
                        WriteLine("\r\nWeb Server Started");
                        foreach (var endpoint in _endpoints)
                        {
                            WriteLine($"Web server listening at {endpoint.Url}");
                        }                        
                    }
                    catch (Exception e)
                    {
                        LogException(e);
                        Fail("Webserver failed to start", e);
                        Console.ReadLine();
                    }
                }
                catch (Exception e)
                {
                    EnvironmentVariables.IsServerOnline = false;
                    Fail("Webserver failed to start", e);
                    Console.ReadLine();
                }
            }
        }

        
        internal static void WriteLine(string message)
        {
            if (Environment.UserInteractive)
            {
                Console.WriteLine(message);
                Dev2Logger.Info(message, "Warewolf Info");
            }
            else
            {
                Dev2Logger.Info(message, "Warewolf Info");
            }

        }

        
        internal static void Write(string message)
        {
            if (Environment.UserInteractive)
            {
                Console.Write(message);
                Dev2Logger.Info(message, "Warewolf Info");
            }
            else
            {
                Dev2Logger.Info(message, "Warewolf Info");
            }
        }

        static void SetStarted()
        {
            try
            {
                if (File.Exists(".\\ServerStarted"))
                {
                    File.Delete(".\\ServerStarted");
                }
                File.WriteAllText(".\\ServerStarted", DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture));
            }
            catch (Exception err)
            {
                Dev2Logger.Error(err, "Warewolf Error");
            }
        }
        static void LogException(Exception ex)
        {
            Dev2Logger.Error("Dev2.ServerLifecycleManager", ex, "Warewolf Error");
        }
    }
}

