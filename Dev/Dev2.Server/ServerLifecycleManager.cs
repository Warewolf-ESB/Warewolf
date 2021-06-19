#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Monitoring;
using Dev2.Common.Wrappers;
using Dev2.Data;
using Dev2.Diagnostics.Debug;
using Dev2.PerformanceCounters.Management;
using Dev2.Runtime;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Security;
using Dev2.Runtime.WebServer;
using WarewolfCOMIPC.Client;
using Dev2.Common.Interfaces.Wrappers;
using System.Collections.Generic;
using System.Management;
using Dev2.Runtime.Interfaces;
using Dev2.Studio.Utils;
using System.Security.Claims;
using System.Reflection;
using System.Threading.Tasks;
using Dev2.Activities;
using Warewolf.Trigger.Queue;
using Warewolf.OS;
using Warewolf;
using Warewolf.Auditing;
using Warewolf.Common.NetStandard20;
using Warewolf.Interfaces.Auditing;
using Dev2.Services.Security.MoqInstallerActions;
using Newtonsoft.Json;
using Warewolf.Usage;
using JsonSerializer = Warewolf.Streams.JsonSerializer;
using System.Diagnostics;
using Warewolf.Execution;

namespace Dev2
{
    public interface IServerLifecycleManager : IDisposable
    {
        bool InteractiveMode { get; set; }

        Task Run(IEnumerable<IServerLifecycleWorker> initWorkers);
        void Stop(bool didBreak, int result, bool mute);
    }

    public class StartupConfiguration
    {
        public IServerEnvironmentPreparer ServerEnvironmentPreparer { get; set; }
        public IIpcClient IpcClient { get; set; }
        public IAssemblyLoader AssemblyLoader { get; set; }
        public IDirectory Directory { get; set; }
        public IResourceCatalogFactory ResourceCatalogFactory { get; set; }
        public IWebServerConfiguration WebServerConfiguration { get; set; }
        public IWriter Writer { get; set; }
        public IStartWebServer StartWebServer { get; set; }
        public ISecurityIdentityFactory SecurityIdentityFactory { get; set; }
        public IProcessMonitor QueueWorkerMonitor { get; set; } = new NullProcessMonitor();
        public IProcessMonitor LoggingServiceMonitor { get; set; } = new NullProcessMonitor();
        public IProcessMonitor HangfireServerMonitor { get; set; } = new NullProcessMonitor();
        public IWebSocketPool WebSocketPool { get; set; }
        public IGetSystemInformation SystemInformationHelper { get; set; }
        public ExecutionLogger.IExecutionLoggerFactory LoggerFactory { get; set; }

        public static StartupConfiguration GetStartupConfiguration(IServerEnvironmentPreparer serverEnvironmentPreparer)
        {
            var writer = new Writer();

            var childProcessTracker = new ChildProcessTrackerWrapper();
            var processFactory = new ProcessWrapperFactory();
            return new StartupConfiguration
            {
                ServerEnvironmentPreparer = serverEnvironmentPreparer,
                IpcClient = new IpcClientImpl(new NamedPipeClientStreamWrapper(".", Guid.NewGuid().ToString(), System.IO.Pipes.PipeDirection.InOut)),
                AssemblyLoader = new AssemblyLoader(),
                Directory = new DirectoryWrapper(),
                ResourceCatalogFactory = new ResourceCatalogFactory(),
                WebServerConfiguration = new WebServerConfiguration(writer, new FileWrapper()),
                Writer = writer,
                StartWebServer = new StartWebServer(writer, WebServerStartup.Start),
                SecurityIdentityFactory = new SecurityIdentityFactoryForWindows(),
                QueueWorkerMonitor = new QueueWorkerMonitor(processFactory, new QueueWorkerConfigLoader(), TriggersCatalog.Instance, childProcessTracker),
                LoggingServiceMonitor = new LoggingServiceMonitorWithRestart(childProcessTracker, processFactory),
                HangfireServerMonitor = new HangfireServerMonitorWithRestart(childProcessTracker, processFactory),
                WebSocketPool = new WebSocketPool(),
                LoggerFactory = new ExecutionLogger.ExecutionLoggerFactory(),
                SystemInformationHelper = new GetSystemInformationHelper()
            };
        }
    }

    public sealed class ServerLifecycleManager : IServerLifecycleManager
    {
        public bool InteractiveMode { get; set; } = true;
        IServerEnvironmentPreparer _serverEnvironmentPreparer;
        private IDirectory _startUpDirectory;
        private IResourceCatalogFactory _startupResourceCatalogFactory;
        bool _isDisposed;

        Timer _timer;
        IStartWebServer _startWebServer;
        readonly IStartTimer _pulseLogger; // need to keep reference to avoid collection of timer
        readonly IStartTimer _pulseTracker; // need to keep reference to avoid collection of timer
        readonly IStartTimer _usageLogger;
        IIpcClient _ipcClient;

        private ILoadResources _loadResources;
        private readonly IAssemblyLoader _assemblyLoader;
        private readonly IWebServerConfiguration _webServerConfiguration;
        private readonly IWriter _writer;
        private readonly IPauseHelper _pauseHelper = new PauseHelper();
        private readonly IProcessMonitor _queueProcessMonitor;
        private readonly IProcessMonitor _loggingProcessMonitor;
        private readonly IWebSocketPool _webSocketPool;
        private readonly IProcessMonitor _hangfireServerMonitor;
        private readonly ExecutionLogger.IExecutionLoggerFactory _loggerFactory;
        private readonly IGetSystemInformation _systemInformationHelper;

        public ServerLifecycleManager(IServerEnvironmentPreparer serverEnvironmentPreparer)
            : this(StartupConfiguration.GetStartupConfiguration(serverEnvironmentPreparer))
        {
        }

        public ServerLifecycleManager(StartupConfiguration startupConfiguration)
        {
            SetApplicationDirectory();
            _writer = startupConfiguration.Writer;
            _serverEnvironmentPreparer = startupConfiguration.ServerEnvironmentPreparer;
            _startUpDirectory = startupConfiguration.Directory;
            _startupResourceCatalogFactory = startupConfiguration.ResourceCatalogFactory;
            _ipcClient = startupConfiguration.IpcClient;
            _assemblyLoader = startupConfiguration.AssemblyLoader;
            _usageLogger = new UsageLogger(TimeSpan.FromHours(1).TotalMilliseconds);
            _pulseLogger = new PulseLogger(60000).Start();
            _pulseTracker = new PulseTracker(TimeSpan.FromDays(1).TotalMilliseconds).Start();
            _serverEnvironmentPreparer.PrepareEnvironment();
            _startWebServer = startupConfiguration.StartWebServer;
            _webServerConfiguration = startupConfiguration.WebServerConfiguration;

            _loggingProcessMonitor = startupConfiguration.LoggingServiceMonitor;
            _loggingProcessMonitor.OnProcessDied += (e) => _writer.WriteLine("logging service exited");

            _hangfireServerMonitor = startupConfiguration.HangfireServerMonitor;
            _hangfireServerMonitor.OnProcessDied += (e) => _writer.WriteLine("hangfire server exited");

            _queueProcessMonitor = startupConfiguration.QueueWorkerMonitor;
            _queueProcessMonitor.OnProcessDied += (config) => _writer.WriteLine($"queue process died: {config.Name}({config.Id})");

            _webSocketPool = startupConfiguration.WebSocketPool;
            _loggerFactory = startupConfiguration.LoggerFactory;
            _systemInformationHelper = startupConfiguration.SystemInformationHelper;

            SecurityIdentityFactory.Set(startupConfiguration.SecurityIdentityFactory);
        }

        private static void SetApplicationDirectory()
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var loc = assembly.Location;
                EnvironmentVariables.ApplicationPath = Path.GetDirectoryName(loc);
            }
            catch(Exception e)
            {
                Dev2Logger.Info("ApplicationPath Error -> " + e.Message, GlobalConstants.WarewolfInfo);
                EnvironmentVariables.ApplicationPath = Directory.GetCurrentDirectory();
            }
        }

        /// <summary>
        /// Starts up the server with relevant workers.
        /// NOTE: This must return a task as in Windows Server 2008 and Windows Server 2012 there is an issue
        /// with the WMI Performance Adapter that causes it to prevent the Warewolf Server Service to need a double restart.
        /// </summary>
        /// <param name="initWorkers">Initilization Workers</param>
        /// <returns>A Task that starts up the Warewolf Server.</returns>
        public Task Run(IEnumerable<IServerLifecycleWorker> initWorkers)
        {
            void OpenCOMStream(INamedPipeClientStreamWrapper clientStreamWrapper)
            {
                _writer.Write("Opening named pipe client stream for COM IPC... ");
                _ipcClient = _ipcClient.GetIpcExecutor(clientStreamWrapper);
                _writer.WriteLine("done.");
            }

            return Task.Run(LoadPerformanceCounters)
                .ContinueWith(
                    (t) =>
                    {
                        // ** Perform Moq Installer Actions For Development ( DEBUG config ) **
#if DEBUG
                        try
                        {
                            var miq = MoqInstallerActionFactory.CreateInstallerActions();
                            miq.ExecuteMoqInstallerActions();
                        }
                        catch(Exception e)
                        {
                            Dev2Logger.Warn("Mocking installer actions for DEBUG config failed to create Warewolf Administrators group and/or to add current user to it [ " + e.Message + " ]", GlobalConstants.WarewolfWarn);
                        }
#endif

                        try
                        {
                            foreach(var worker in initWorkers)
                            {
                                worker.Execute();
                            }

                            _loggingProcessMonitor.Start();
                            var loggingServerCheckDelay = Task.Delay(TimeSpan.FromSeconds(300));

                            _loadResources = new LoadResources("Resources", _writer, _startUpDirectory, _startupResourceCatalogFactory);
                            LoadHostSecurityProvider();
                            _loadResources.CheckExampleResources();
                            _loadResources.MigrateOldTests();
                            var webServerConfig = _webServerConfiguration;
                            webServerConfig.Execute();
                            new LoadRuntimeConfigurations(_writer).Execute();
                            OpenCOMStream(null);
                            _loadResources.LoadResourceCatalog();
                            _timer = new Timer((state) => GetComputerNames.GetComputerNamesList(), null, 1000, GlobalConstants.NetworkComputerNameQueryFreq);
                            _loadResources.LoadServerWorkspace();
                            _loadResources.LoadActivityCache(_assemblyLoader);
                            LoadTestCatalog();
                            LoadTriggersCatalog();

                            _startWebServer.Execute(webServerConfig, _pauseHelper);
                            _queueProcessMonitor.Start();

                            _hangfireServerMonitor.Start();

                            var checkLogServerConnectionTask = CheckLogServerConnection();
                            var result = Task.WaitAny(new[] { checkLogServerConnectionTask, loggingServerCheckDelay });
                            var isConnectedOkay = !checkLogServerConnectionTask.IsCanceled && !checkLogServerConnectionTask.IsFaulted && checkLogServerConnectionTask.Result == true;
                            var logServerConnectedOkayNoTimeout = result == 0 && isConnectedOkay;
                            if(!logServerConnectedOkayNoTimeout)
                            {
                                _writer.WriteLine("unable to connect to logging server");
                                if(checkLogServerConnectionTask.IsFaulted)
                                {
                                    _writer.WriteLine("error: " + checkLogServerConnectionTask.Exception?.Message);
                                }

                                Stop(false, 0, true);
                            }
                            var logger = _loggerFactory.New(new JsonSerializer(), _webSocketPool);
                            LogWarewolfVersion(logger);
#if DEBUG
                            if(EnvironmentVariables.IsServerOnline)
                            {
                                SetAsStarted();
                            }
#else
                            TrackUsage(UsageType.ServerStart,logger);
#endif
                        }
                        catch(Exception e)
                        {
#pragma warning disable S2228 // Console logging should not be used
                            Console.WriteLine(e);
#pragma warning restore S2228 // Console logging should not be used
                            Dev2Logger.Error("Error Starting Server", e, GlobalConstants.WarewolfError);
                            Stop(true, 0, false);
                        }
                    });
        }

        private void LogWarewolfVersion(IExecutionLogPublisher logger)
        {
            var wareWolfVersion = _systemInformationHelper.GetWareWolfVersion();
            logger.Info("Warewolf Server Started Version: " + wareWolfVersion);
            Dev2Logger.Info(wareWolfVersion, "Warewolf Server Version");
        }

        private Task<bool> CheckLogServerConnection()
        {
            return Task.Run(
                () =>
                {
                    var webSocketWrapper = _webSocketPool.Acquire(Config.Auditing.Endpoint);
                    return webSocketWrapper.IsOpen();
                });
        }

        private void LoadTriggersCatalog()
        {
            _writer.Write("Loading triggers catalog...  ");
            _assemblyLoader.LoadAndReturn(typeof(TriggerQueue).Assembly.GetName());
            _writer.WriteLine("done.");
        }

        int GetNumberOfCores()
        {
            var coreCount = 0;
            foreach(var item in new ManagementObjectSearcher("Select * from Win32_Processor").Get())
            {
                coreCount += int.Parse(item["NumberOfCores"].ToString());
            }

            return coreCount;
        }

        void TrackUsage(UsageType usageType, IExecutionLogPublisher logger)
        {
            if(usageType == UsageType.ServerStart)
            {
                ServerStats.SessionId = Guid.NewGuid();
            }

            bool isDebugMode = false;
#if DEBUG
            isDebugMode = true;
#endif
            var myData = new
            {
                ServerStats.SessionId,
                VersionNo = _systemInformationHelper.GetWareWolfVersion(),
                IPAddress = _systemInformationHelper.GetIPv4Adresses(),
                Environment.ProcessorCount,
                NumberOfCores = GetNumberOfCores(),
                OSType = _systemInformationHelper.GetOperatingSystemInformation(),
                MachineName = _systemInformationHelper.GetComputerName(),
                IsDebugMode = isDebugMode.ToString(),
                Region = _systemInformationHelper.GetRegionInformation(),
                Executions = ServerStats.TotalExecutions,
                Uptime = DateTime.Now - Process.GetCurrentProcess().StartTime
            };
            //TODO: Add whether running in container
            var jsonData = JsonConvert.SerializeObject(myData);
            var customerId = "Unknown";
            //TODO: Licensing: Get customer ID from the licensing

            var returnResult = UsageTracker.TrackEvent(customerId, usageType, jsonData);
            if(returnResult != UsageDataResult.ok)
            {
                ServerStats.IncrementUsageServerRetry();
                _writer.WriteLine("UsageTracker: Could not log usage.");
                var msg = "Could not log usage. Retry: " + ServerStats.UsageServerRetry + "/3. Connect to the internet to avoid Warewolf reverting to ReadOnly mode.";
                logger.Warn(msg);
                Dev2Logger.Warn(msg, "UsageTracker");
            }

            if(usageType == UsageType.ServerStart)
            {
                _usageLogger.Start();
            }
        }

        public void Stop(bool didBreak, int result, bool mute)
        {
            if(!didBreak)
            {
                Dispose();
            }

            if(!mute)
            {
                _writer.Write($"Exiting with exitcode {result}");
            }
        }

        internal void CleanupServer()
        {
            try
            {
#if !DEBUG
                var logger = _loggerFactory.New(new JsonSerializer(), _webSocketPool);
                TrackUsage(UsageType.ServerStop, logger);
#endif
                _queueProcessMonitor.Shutdown();
                _hangfireServerMonitor.Shutdown();
                if(_startWebServer != null)
                {
                    _startWebServer.Dispose();
                    _startWebServer = null;
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
                Dev2Logger.Error("Dev2.ServerLifecycleManager", ex, GlobalConstants.WarewolfError);
            }
        }

        ~ServerLifecycleManager()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

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

            if(_usageLogger != null)
            {
                _usageLogger.Dispose();
            }

            if(_pulseLogger != null)
            {
                _pulseLogger.Dispose();
            }

            if (_pulseTracker != null)
            {
                _pulseTracker.Dispose();
            }

            if (_serverEnvironmentPreparer != null)
            {
                _serverEnvironmentPreparer.Dispose();
                _serverEnvironmentPreparer = null;
            }

            _startWebServer = null;
        }

        static void LoadPerformanceCounters()
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
                Dev2Logger.Error(err, GlobalConstants.WarewolfError);
            }
        }

        void LoadTestCatalog()
        {
            _writer.Write("Loading test catalog...  ");
            TestCatalog.Instance.Load();
            _writer.WriteLine("done.");
        }


        void LoadHostSecurityProvider()
        {
            _writer.Write("Loading security provider...  ");
            var instance = HostSecurityProvider.Instance;
            //TODO: have not done coverage yet as it might change.
            GlobalConstants.LicenseCustomerId = HostSecurityProvider.Instance.CustomerId;
            GlobalConstants.SiteName =  HostSecurityProvider.Instance.ConfigSitename;
            GlobalConstants.ApiKey =  HostSecurityProvider.Instance.ConfigKey;
            if (instance != null)
            {
                _writer.WriteLine("done.");
            }
        }

#if DEBUG

        static void SetAsStarted()
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
                Dev2Logger.Error(err, GlobalConstants.WarewolfError);
            }
        }
#endif
    }

    class Writer : IWriter
    {
        public void WriteLine(string message)
        {
            if (Environment.UserInteractive)
            {
                Console.WriteLine(message);
                Dev2Logger.Info(message, GlobalConstants.WarewolfInfo);
            }
            else
            {
                Dev2Logger.Info(message, GlobalConstants.WarewolfInfo);
            }
        }

        public void Write(string message)
        {
            if (Environment.UserInteractive)
            {
                Console.Write(message);
                Dev2Logger.Info(message, GlobalConstants.WarewolfInfo);
            }
            else
            {
                Dev2Logger.Info(message, GlobalConstants.WarewolfInfo);
            }
        }


        public void Fail(string message, Exception e)
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
    }
}