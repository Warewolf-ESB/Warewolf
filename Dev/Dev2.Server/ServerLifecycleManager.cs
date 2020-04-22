/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Services.Security.MoqInstallerActions;
using WarewolfCOMIPC.Client;
using Dev2.Common.Interfaces.Wrappers;
using System.Collections.Generic;
using Dev2.Runtime.Interfaces;
using Dev2.Instrumentation.Factory;
using Dev2.Instrumentation;
using Dev2.Studio.Utils;
using System.Security.Claims;
using System.Reflection;
using System.Threading.Tasks;
using Warewolf.Trigger.Queue;
using Warewolf.OS;
using Warewolf;

namespace Dev2
{
    public interface IServerLifecycleManager : IDisposable
    {
        bool InteractiveMode { get; set; }

        Task Run(IEnumerable<IServerLifecycleWorker> initWorkers);
        void Stop(bool didBreak, int result);
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
        public IClusterMonitor ClusterMonitor { get; } = new ClusterMonitor();
        public IProcessMonitor LoggingServiceMonitor { get; set; } = new NullProcessMonitor();
        public IPauseHelper PauseHelper { get; } = new PauseHelper();
        public ClusterSettings ClusterSettings { get; set; } = Config.Cluster;

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
                LoggingServiceMonitor = new LoggingServiceMonitorWithRestart(childProcessTracker, processFactory)
            };
        }
    }

    public sealed class ServerLifecycleManager : IServerLifecycleManager
    {
        public bool InteractiveMode { get; set; } = true;
        private IServerEnvironmentPreparer _serverEnvironmentPreparer;
        private readonly IDirectory _startUpDirectory;
        private readonly IResourceCatalogFactory _startupResourceCatalogFactory;
        private bool _isDisposed;

        private Timer _timer;
        private IStartWebServer _startWebServer;
        private readonly IStartTimer _pulseLogger; // need to keep reference to avoid collection of timer
        private readonly IStartTimer _pulseTracker; // need to keep reference to avoid collection of timer
        private IIpcClient _ipcClient;
        
        private ILoadResources _loadResources;
        private readonly IAssemblyLoader _assemblyLoader;
        private readonly IWebServerConfiguration _webServerConfiguration;
        private readonly IWriter _writer;
        private readonly IPauseHelper _pauseHelper = new PauseHelper();
        private readonly IProcessMonitor _queueProcessMonitor;
        private readonly IClusterMonitor _clusterMonitor;
        private readonly ClusterSettings _clusterSettings;

        public ServerLifecycleManager(IServerEnvironmentPreparer serverEnvironmentPreparer)
            :this(StartupConfiguration.GetStartupConfiguration(serverEnvironmentPreparer))
        {
        }

        public ServerLifecycleManager(StartupConfiguration startupConfiguration)
        {
            SetApplicationDirectory();
            _writer = startupConfiguration.Writer;
            StartLoggingService(startupConfiguration);

            _pauseHelper = startupConfiguration.PauseHelper;
            _serverEnvironmentPreparer = startupConfiguration.ServerEnvironmentPreparer;
            _startUpDirectory = startupConfiguration.Directory;
            _startupResourceCatalogFactory = startupConfiguration.ResourceCatalogFactory;
            _ipcClient = startupConfiguration.IpcClient;
            _assemblyLoader = startupConfiguration.AssemblyLoader;
            _pulseLogger = new PulseLogger(60000).Start();
            _pulseTracker = new PulseTracker(TimeSpan.FromDays(1).TotalMilliseconds).Start();
            _serverEnvironmentPreparer.PrepareEnvironment();
            _startWebServer = startupConfiguration.StartWebServer;
            _webServerConfiguration = startupConfiguration.WebServerConfiguration;

            _queueProcessMonitor = startupConfiguration.QueueWorkerMonitor;
            _queueProcessMonitor.OnProcessDied += (config) => _writer.WriteLine($"queue process died: {config.Name}({config.Id})");

            _clusterSettings = startupConfiguration.ClusterSettings;
            _clusterMonitor = startupConfiguration.ClusterMonitor;

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
            catch (Exception e)
            {
                Dev2Logger.Info("ApplicationPath Error -> " + e.Message, GlobalConstants.WarewolfInfo);
                EnvironmentVariables.ApplicationPath = Directory.GetCurrentDirectory();
            }
        }

        private void StartLoggingService(StartupConfiguration startupConfiguration)
        {
            _writer.Write("Starting logging service...  ");
            var monitor = startupConfiguration.LoggingServiceMonitor;
            monitor.OnProcessDied += (e) => _writer.WriteLine("logging service exited");
            monitor.Start();
            _writer.WriteLine("done.");
        }

        /// <summary>
        /// Starts up the server with relevant workers.
        /// NOTE: This must return a task as in Windows Server 2008 and Windows Server 2012 there is an issue
        /// with the WMI Performance Adapter that causes it to prevent the Warewolf Server Service to need a double restart.
        /// </summary>
        /// <param name="initWorkers">Initialization Workers</param>
        /// <returns>A Task that starts up the Warewolf Server.</returns>
        public Task Run(IEnumerable<IServerLifecycleWorker> initWorkers)
        {
            return Task.Run(LoadPerformanceCounters)
                       .ContinueWith((t) =>
            {
                void OpenCOMStream(INamedPipeClientStreamWrapper clientStreamWrapper)
                {
                    _writer.Write("Opening named pipe client stream for COM IPC... ");
                    _ipcClient = _ipcClient.GetIpcExecutor(clientStreamWrapper);
                    _writer.WriteLine("done.");
                }
//
//                 // ** Perform Moq Installer Actions For Development ( DEBUG config ) **
// #if DEBUG
//                 try
//                 {
//                     var miq = MoqInstallerActionFactory.CreateInstallerActions();
//                     miq.ExecuteMoqInstallerActions();
//                 }
//                 catch (Exception e)
//                 {
//                     Dev2Logger.Warn("Mocking installer actions for DEBUG config failed to create Warewolf Administrators group and/or to add current user to it [ " + e.Message + " ]", GlobalConstants.WarewolfWarn);
//                 }
// #endif

                try
                {
                    foreach (var worker in initWorkers)
                    {
                        worker.Execute();
                    }
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
                    StartTrackingUsage();
                    _startWebServer.Execute(webServerConfig, _pauseHelper);
                    _queueProcessMonitor.Start();
                    _clusterMonitor.Start(_clusterSettings, ResourceCatalog.Instance, _writer);
#if DEBUG
                    SetAsStarted();
#endif
                }
                catch (Exception e)
                {
#pragma warning disable S2228 // Console logging should not be used
                    Console.WriteLine(e);
#pragma warning restore S2228 // Console logging should not be used
                    Dev2Logger.Error("Error Starting Server", e, GlobalConstants.WarewolfError);
                    Stop(true, 0);
                }
            });
        }

        private void LoadTriggersCatalog()
        {
            _writer.Write("Loading triggers catalog...  ");
            _assemblyLoader.LoadAndReturn(typeof(TriggerQueue).Assembly.GetName());
            _writer.WriteLine("done.");
        }

        void StartTrackingUsage()
        {
            _writer.Write("Registering usage tracker...  ");
            CustomContainer.Register(ApplicationTrackerFactory.GetApplicationTrackerProvider());
            var applicationTracker = CustomContainer.Get<IApplicationTracker>();
            applicationTracker?.EnableApplicationTracker(VersionInfo.FetchVersionInfo(), VersionInfo.FetchInformationalVersion(), @"Warewolf" + $" ({ClaimsPrincipal.Current.Identity.Name})".ToUpperInvariant());
            applicationTracker?.TrackEvent("Server Events", "Server Startup");
            _writer.WriteLine("done.");
        }

        public void Stop(bool didBreak, int result)
        {
            if (!didBreak)
            {
                Dispose();
            }

            _writer.Write($"Exiting with exit code {result}");
        }

        private void CleanupServer()
        {
            try
            {
                _queueProcessMonitor.Shutdown();
                if (_startWebServer != null)
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

        private void Dispose(bool disposing)
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

            _pulseLogger?.Dispose();
            _pulseTracker?.Dispose();

            if (_serverEnvironmentPreparer != null)
            {
                _serverEnvironmentPreparer.Dispose();
                _serverEnvironmentPreparer = null;
            }

            _startWebServer = null;
        }

        private static void LoadPerformanceCounters()
        {
            try
            {
                var perf = new PerformanceCounterPersistence(new FileWrapper());
                var register = new WarewolfPerformanceCounterRegister(perf.LoadOrCreate(), perf.LoadOrCreateResourcesCounters(perf.DefaultResourceCounters));
                var locator = new WarewolfPerformanceCounterManager(register.Counters, register.ResourceCounters, register, perf);
                locator.CreateCounter(Guid.Parse("a64fc548-3045-407d-8603-2a7337d874a6"), WarewolfPerfCounterType.ExecutionErrors, "workflow1");
                locator.CreateCounter(Guid.Parse("a64fc548-3045-407d-8603-2a7337d874a6"), WarewolfPerfCounterType.AverageExecutionTime, "workflow1");
                locator.CreateCounter(Guid.Parse("a64fc548-3045-407d-8603-2a7337d874a6"), WarewolfPerfCounterType.ConcurrentRequests, "workflow1");
                locator.CreateCounter(Guid.Parse("a64fc548-3045-407d-8603-2a7337d874a6"), WarewolfPerfCounterType.RequestsPerSecond, "workflow1");


                CustomContainer.Register<IWarewolfPerformanceCounterLocater>(locator);
                CustomContainer.Register<IPerformanceCounterRepository>(locator);
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

