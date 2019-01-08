/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Monitoring;
using Dev2.Common.Interfaces.Scheduler.Interfaces;
using Dev2.Common.Wrappers;
using Dev2.Data;
using Dev2.Diagnostics.Debug;
using Dev2.Diagnostics.Logging;
using Dev2.PerformanceCounters.Management;
using Dev2.Runtime;
using Dev2.Runtime.ESB.Execution;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Security;
using Dev2.Runtime.WebServer;
using Dev2.Services.Security.MoqInstallerActions;
using Dev2.Workspaces;
using WarewolfCOMIPC.Client;

namespace Dev2
{
    public interface IServerLifecycleManager : IDisposable
    {
        bool InteractiveMode { get; set; }

        void Run(IEnumerable<IServerLifecycleWorker> initWorkers);
        void Stop(bool didBreak, int result);
    }

    public sealed class ServerLifecycleManager : IServerLifecycleManager, IWriter
    {
        public bool InteractiveMode { get; set; } = true;
        IServerEnvironmentPreparer _serverEnvironmentPreparer;

        bool _isDisposed;

        public IWebServerConfiguration WebServerConfiguration { get; private set; }

        Timer _timer;
        IStartWebServer _startWebServer;
        readonly IStartTimer _pulseLogger; // need to keep reference to avoid collection of timer
        readonly IStartTimer _pulseTracker; // need to keep reference to avoid collection of timer
        IpcClient _ipcIpcClient;


        public ServerLifecycleManager(IServerEnvironmentPreparer serverEnvironmentPreparer)
        {
            _serverEnvironmentPreparer = serverEnvironmentPreparer;
            _pulseLogger = new PulseLogger(60000).Start();
            _pulseTracker = new PulseTracker(TimeSpan.FromDays(1).TotalMilliseconds).Start();
            _serverEnvironmentPreparer.PrepareEnvironment();
            _startWebServer = new StartWebServer(this, WebServerStartup.Start);
        }

        public void Run(IEnumerable<IServerLifecycleWorker> initWorkers)
        {
            // ** Perform Moq Installer Actions For Development ( DEBUG config ) **
#if DEBUG
            try
            {
                var miq = MoqInstallerActionFactory.CreateInstallerActions();
                miq.ExecuteMoqInstallerActions();
            }
            catch (Exception e)
            {
                Dev2Logger.Warn("Mocking installer actions for DEBUG config failed to create Warewolf Administrators group and/or to add current user to it [ " + e.Message + " ]", GlobalConstants.WarewolfWarn);
            }
#endif

            try
            {
                foreach (var worker in initWorkers)
                {
                    worker.Execute();
                }

                LoadHostSecurityProvider();
                LoadPerformanceCounters();
                CheckExampleResources();
                MigrateOldTests();
                var webServerConfig = new WebServerConfiguration(this, new FileWrapper());
                webServerConfig.Execute();
                new LoadRuntimeConfigurations(this).Execute();                
                OpenCOMStream();
                var catalog = LoadResourceCatalog();
                _timer = new Timer(PerformTimerActions, null, 1000, GlobalConstants.NetworkComputerNameQueryFreq);
                new LogFlusherWorker(new LogManagerImplementation(), this).Execute();
                LoadServerWorkspace();
                LoadActivityCache(catalog);
                _startWebServer.Execute(webServerConfig, new PauseHelper());
#if DEBUG
                SetAsStarted();
#endif
                LoadTestCatalog();
                if (InteractiveMode)
                {
                    WaitForUserExit();
                }
            }
            catch (Exception e)
            {
#pragma warning disable S2228 // Console logging should not be used
#pragma warning disable S2228 // Console logging should not be used
                Console.WriteLine(e);
#pragma warning restore S2228 // Console logging should not be used
#pragma warning restore S2228 // Console logging should not be used
                Dev2Logger.Error("Error Starting Server", e, GlobalConstants.WarewolfError);
                Stop(true, 0);
            }
        }

        void OpenCOMStream()
        {
            Write("Opening named pipe client stream for COM IPC... ");
            _ipcIpcClient = IpcClient.GetIPCExecutor();
            WriteLine("done.");
        }

        void PerformTimerActions(object state)
        {
            GetComputerNames.GetComputerNamesList();
        }

        void PreloadReferences()
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

        public void Stop(bool didBreak, int result)
        {
            if (!didBreak)
            {
                Dispose();
            }

            Write($"Exiting with exitcode {result}");
        }

        void WaitForUserExit()
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

        internal void CleanupServer()
        {
            try
            {
                if (_startWebServer != null)
                {
                    _startWebServer.Dispose();
                    _startWebServer = null;
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
                Dev2Logger.Error("Dev2.ServerLifecycleManager", ex, GlobalConstants.WarewolfError);
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

        ResourceCatalog LoadResourceCatalog()
        {
            MigrateOldResources();
            ValidateResourceFolder();
            Write("Loading resource catalog...  ");
            var catalog = ResourceCatalog.Instance;
            MethodsToBeDepricated();
            WriteLine("done.");
            return catalog;
        }

        static void MethodsToBeDepricated()
        {
            ResourceCatalog.Instance.CleanUpOldVersionControlStructure();
        }

        void LoadTestCatalog()
        {

            Write("Loading test catalog...  ");
            TestCatalog.Instance.Load();
            WriteLine("done.");
        }

        void LoadActivityCache(ResourceCatalog catalog)
        {
            PreloadReferences();
            Write("Loading resource activity cache...  ");
            catalog.LoadServerActivityCache();
            WriteLine("done.");
        }
        public static IDirectoryHelper DirectoryHelperInstance()
        {
            return new DirectoryHelper();
        }
        static void MigrateOldResources()
        {
            var serverBinResources = Path.Combine(EnvironmentVariables.ApplicationPath, "Resources");
            if (!Directory.Exists(EnvironmentVariables.ResourcePath) && Directory.Exists(serverBinResources))
            {
                var dir = DirectoryHelperInstance();
                dir.Copy(serverBinResources, EnvironmentVariables.ResourcePath, true);
                dir.CleanUp(serverBinResources);
            }
        }

        static void CheckExampleResources()
        {
            var serverReleaseResources = Path.Combine(EnvironmentVariables.ApplicationPath, "Resources");
            if (Directory.Exists(EnvironmentVariables.ResourcePath) && Directory.Exists(serverReleaseResources))
            {
                ResourceCatalog.Instance.LoadExamplesViaBuilder(serverReleaseResources);
            }
        }

        static void MigrateOldTests()
        {
            var serverBinTests = Path.Combine(EnvironmentVariables.ApplicationPath, "Tests");
            if (!Directory.Exists(EnvironmentVariables.TestPath) && Directory.Exists(serverBinTests))
            {
                var dir = DirectoryHelperInstance();
                dir.Copy(serverBinTests, EnvironmentVariables.TestPath, true);
                dir.CleanUp(serverBinTests);
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

       

        void LoadServerWorkspace()
        {

            Write("Loading server workspace...  ");

            var instance = WorkspaceRepository.Instance;
            if (instance != null)
            {
                WriteLine("done.");
            }
        }

        void LoadHostSecurityProvider()
        {
            Write("Loading security provider...  ");
            var instance = HostSecurityProvider.Instance;
            if (instance != null)
            {
                WriteLine("done.");
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
        
    }
    
}

