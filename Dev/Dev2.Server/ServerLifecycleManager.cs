// ReSharper disable RedundantUsingDirective
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
using System.Diagnostics;
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
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Security;
using Dev2.Runtime.WebServer;
using Dev2.Services.Security.MoqInstallerActions;
using Dev2.Workspaces;
using log4net.Config;
using WarewolfCOMIPC.Client;

// ReSharper disable AssignNullToNotNullAttribute
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable NonLocalizedString
// ReSharper disable CatchAllClause
// ReSharper disable CatchAllClause
// ReSharper disable ThrowFromCatchWithNoInnerException
// ReSharper disable ThrowingSystemException

// ReSharper disable InconsistentNaming
namespace Dev2
{
    /// <summary>
    /// PBI 5278
    /// Application Server Life-cycle Manager
    /// Facilitates start-up, execution and tear-down of the application server.
    /// </summary>
    sealed class ServerLifecycleManager : IDisposable
    {

        static ServerLifecycleManager _singleton;

        /// <summary>
        /// Entry Point for application server.
        /// </summary>
        /// <param name="arguments">Command line arguments passed to executable.</param>
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
            var result = 0;

            var options = new CommandLineParameters();
            var parser = new CommandLineParser(new CommandLineParserSettings(Console.Error));
            if (!parser.ParseArguments(arguments, options))
            {
                return 80;
            }

            var commandLineParameterProcessed = false;
            if (options.Install)
            {
                Dev2Logger.Info("Starting Install");
                commandLineParameterProcessed = true;

                if (!EnsureRunningAsAdministrator(arguments))
                {
                    Dev2Logger.Info("Cannot install because the server is not running as an admin user");
                    return result;
                }

                if (!WindowsServiceManager.Install())
                {
                    result = 81;
                    Dev2Logger.Info("Install Success Result is 81");
                }
            }

            if (options.StartService)
            {
                Dev2Logger.Info("Starting Service");
                commandLineParameterProcessed = true;

                if (!EnsureRunningAsAdministrator(arguments))
                {
                    Dev2Logger.Info("Cannot start because the server is not running as an admin user");
                    return result;
                }

                if (!WindowsServiceManager.StartService(null))
                {
                    Dev2Logger.Info("Starting Service success. result 83");
                    result = 83;
                }
            }

            if (options.StopService)
            {
                Dev2Logger.Info("Stopping Service");
                commandLineParameterProcessed = true;

                if (!EnsureRunningAsAdministrator(arguments))
                {
                    Dev2Logger.Info("Cannot stop because the server is not running as an admin user");
                    return result;
                }

                if (!WindowsServiceManager.StopService(null))
                {
                    Dev2Logger.Info("Stopping Service success. result 84");
                    result = 84;
                }
            }

            if (options.Uninstall)
            {
                Dev2Logger.Info("Uninstall Service");
                commandLineParameterProcessed = true;

                if (!EnsureRunningAsAdministrator(arguments))
                {
                    Dev2Logger.Info("Cannot uninstall because the server is not running as an admin user");
                    return result;
                }

                if (!WindowsServiceManager.Uninstall())
                {
                    Dev2Logger.Info("Uninstall Service success. result 92");
                    result = 82;
                }
            }

            if (commandLineParameterProcessed)
            {
                Dev2Logger.Info("Command line processed. Returning");
                return result;
            }
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                Dev2Logger.Fatal("Server has crashed!!!", args.ExceptionObject as Exception);
            };
            if (Environment.UserInteractive || options.IntegrationTestMode)
            {
                Dev2Logger.Info("** Starting In Interactive Mode ( " + options.IntegrationTestMode + " ) **");
                using (_singleton = new ServerLifecycleManager(arguments))
                {
                    _singleton.Run(true);
                }

                _singleton = null;
            }
            else
            {
                Dev2Logger.Info("** Starting In Service Mode **");
                using (var service = new ServerLifecycleManagerService())
                {
                    ServiceBase.Run(service);
                }
            }
            return result;
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
                Dev2Logger.Info("** Service Started **");
                _singleton = new ServerLifecycleManager(null);
                _singleton.Run(false);
            }

            protected override void OnStop()
            {
                Dev2Logger.Info("** Service Stopped **");
                _singleton.Stop(false, 0);
                _singleton = null;
            }
        }

        bool _isDisposed;
        bool _isWebServerEnabled;
        bool _isWebServerSslEnabled;
        readonly string[] _arguments;
        Dev2Endpoint[] _endpoints;
        Timer _timer;
        IDisposable _owinServer;
        readonly IPulseLogger _pulseLogger;
        private int _daysToKeepTempFiles;
        private readonly PulseTracker _pulseTracker;
        private Client _ipcClient;

        /// <summary>
        /// Get a value indicating if the lifecycle manager has been disposed.
        /// </summary>
        public bool IsDisposed => _isDisposed;

        /// <summary>
        /// Gets a value indicating if the webserver is enabled.
        /// </summary>
        public bool IsWebServerEnabled => _isWebServerEnabled;

        /// <summary>
        /// Gets a Guid that represents the ID of the current server.
        /// </summary>
        public Guid ServerID => HostSecurityProvider.Instance.ServerID;

        /// <summary>
        /// Constructors an instance of the ServerLifecycleManager class, ServerLifecycleManager is essentially a singleton but implemented as an instance type
        /// to ensure proper finalization occurs.
        /// </summary>
        ServerLifecycleManager(string[] arguments)
        {
            _pulseLogger = new PulseLogger(60000);
            _pulseLogger.Start();
            _pulseTracker = new PulseTracker(TimeSpan.FromDays(1).TotalMilliseconds);
            _pulseTracker.Start();
            _arguments = arguments ?? new string[0];
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
                Dev2Logger.Error("Error in startup.", e);
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

        /// <summary>
        /// Runs the application server, and handles all initialization, execution and cleanup logic required.
        /// </summary>
        /// <exception cref="Exception"></exception>
        /// <returns></returns>
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
                _ipcClient = Client.IPCExecutor;
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
                Dev2Logger.Error("Error Starting Server", e);
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

        /// <summary>
        /// Ensures all external dependencies have been loaded, then loads all referenced assemblies by the 
        /// currently executing assembly, and recursively loads each of the referenced assemblies of the 
        /// initial dependency set until all dependencies have been loaded.
        /// </summary>
        static void PreloadReferences()
        {
            Write("Preloading assemblies...  ");
            var currentAsm = typeof(ServerLifecycleManager).Assembly;
            var inspected = new HashSet<string> { currentAsm.GetName().ToString(), "GroupControls" };
            LoadReferences(currentAsm, inspected);
            WriteLine("done.");
        }

        /// <summary>
        /// Loads the assemblies that are referenced by the input assembly, but only if that assembly has not
        /// already been inspected.
        /// </summary>
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

        static bool EnsureRunningAsAdministrator(string[] arguments)
        {

            try
            {
                if (!IsElevated())
                {
                    var startInfo = new ProcessStartInfo(Assembly.GetExecutingAssembly().Location) { Verb = "runas", Arguments = string.Join(" ", arguments) };

                    var process = new Process { StartInfo = startInfo };

                    try
                    {
                        process.Start();
                    }
                    catch (Exception e)
                    {
                        LogException(e);
                    }

                    return false;
                }
            }
            catch (Exception e)
            {
                LogException(e);
            }

            return true;
        }

        static bool IsElevated()
        {
            var currentIdentity = WindowsIdentity.GetCurrent();
            {
                var principal = new WindowsPrincipal(currentIdentity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

        internal void ReadBooleanSection(XmlNode section, string sectionName, ref bool result, ref bool setter)
        {
            if (string.Equals(section.Name, sectionName, StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrEmpty(section.InnerText))
                {
                    if (string.Equals(section.InnerText, "true", StringComparison.OrdinalIgnoreCase))
                    {
                        setter = true;
                    }
                    else if (string.Equals(section.InnerText, "false", StringComparison.OrdinalIgnoreCase))
                    {
                        setter = false;
                    }
                    else
                    {
                        Fail("Configuration error, " + sectionName);
                        result = false;
                    }
                }
                else
                {
                    Fail("Configuration error, " + sectionName);
                    result = false;
                }
            }
        }

        /// <summary>
        /// Performs all necessary initialization such that the server is in a state that allows
        /// workflow execution.
        /// </summary>
        /// <returns>false if the initialization failed, otherwise true</returns>
        void InitializeServer()
        {
            try
            {
                string webServerSslPort = null;
                var webServerPort = ParseArguments(ref webServerSslPort);

                GlobalConstants.WebServerPort = webServerPort = !string.IsNullOrEmpty(webServerPort) ? webServerPort : ConfigurationManager.AppSettings["webServerPort"];
                GlobalConstants.WebServerSslPort = webServerSslPort = !string.IsNullOrEmpty(webServerSslPort) ? webServerSslPort : ConfigurationManager.AppSettings["webServerSslPort"];

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

        private string ParseArguments(ref string webServerSslPort)
        {
            var webServerPort = "";
            var arguments = new Dictionary<string, string>();

            if (_arguments.Any())
            {
                foreach (var t in _arguments)
                {
                    var arg = t.Split('=');
                    if (arg.Length == 2)
                    {
                        arguments.Add(arg[0].Replace("/", string.Empty), arg[1]);
                    }
                }
            }

            foreach (var argument in arguments)
            {
                if (argument.Key.Equals("webServerPort", StringComparison.InvariantCultureIgnoreCase))
                {
                    webServerPort = argument.Value;
                    continue;
                }

                if (argument.Key.Equals("webServerSslPort", StringComparison.InvariantCultureIgnoreCase))
                {
                    webServerSslPort = argument.Value;
                }
            }
            return webServerPort;
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

        /// <summary>
        /// Performs all necessary cleanup such that the server is gracefully moved to a state that does not allow
        /// workflow execution.
        /// </summary>
        /// <returns>false if the cleanup failed, otherwise true</returns>
        internal void CleanupServer()
        {
            try
            {
                if (_owinServer != null)
                {
                    _owinServer.Dispose();
                    _owinServer = null;
                }
                if (_ipcClient != null)
                {
                    _ipcClient.Dispose();
                    _ipcClient = null;
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

        static void Fail(string message, string details = "")
        {
            WriteLine("Critical Failure: " + message);

            if (!string.IsNullOrEmpty(details))
            {
                WriteLine("Details");
                WriteLine("--");
                WriteLine(details);
            }

            WriteLine("");

        }

        /// <summary>
        /// Finalizer for ServerLifecycleManager called when garbage collected.
        /// </summary>
        ~ServerLifecycleManager()
        {
            Dispose(false);
        }

        /// <summary>
        /// Public facing implementation of the Dispose interface
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
                return;
            _isDisposed = true;
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///  Proper dispose pattern implementation to ensure Application Server is terminated correctly, even from finalizer.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed, otherwise false.</param>
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
                Dev2Logger.Error(err);
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
            //ServerExplorerRepository.Instance.Load(GlobalConstants.ServerWorkspaceID);
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
            catalog.LoadResourceActivityCache(GlobalConstants.ServerWorkspaceID);
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
            // ReSharper disable UnusedVariable
            var instance = WorkspaceRepository.Instance;
            // ReSharper restore UnusedVariable
            WriteLine("done.");
        }

        void LoadHostSecurityProvider()
        {
            // ReSharper disable UnusedVariable
            var instance = HostSecurityProvider.Instance;
            // ReSharper restore UnusedVariable
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
                Dev2Logger.Info(message);
            }
            else
            {
                Dev2Logger.Info(message);
            }

        }

        internal static void Write(string message)
        {
            if (Environment.UserInteractive)
            {
                Console.Write(message);
                Dev2Logger.Info(message);
            }
            else
            {
                Dev2Logger.Info(message);
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
                Dev2Logger.Error(err);
            }
        }
        static void LogException(Exception ex)
        {
            Dev2Logger.Error("Dev2.ServerLifecycleManager", ex);
        }
    }
}

