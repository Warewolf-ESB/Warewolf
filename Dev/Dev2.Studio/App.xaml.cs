using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Caliburn.Micro;
using Dev2.Diagnostics;
using Dev2.Studio.Core.AppResources.Browsers;
using Dev2.Studio.Core.Utils;
using Dev2.Studio.Diagnostics;

namespace Dev2.Studio
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : IApp
    {
        private Mutex _processGuard = null;
        private AppExceptionHandler _appExceptionHandler;
        private AppExceptionPopupController _appExceptionPopupController;
        private bool _hasShutdownStarted;

        public App()
        {
            _hasShutdownStarted = false;
            ShouldRestart = false;
            InitializeComponent();
        }

#if DEBUG
        public static bool IsAutomationMode
        {
            get
            {
                return true;
            }
        }
#else

        public static bool IsAutomationMode
        {
            get
            {
                return false;
            }
        }
#endif


        protected override void OnStartup(StartupEventArgs e)
        {
            bool createdNew;
            Mutex localprocessGuard;
            if(e.Args.Length > 0)
            {
                localprocessGuard = new Mutex(true, e.Args[0], out createdNew);
            }
            else
            {
                localprocessGuard = new Mutex(true, "Warewolf Studio", out createdNew);
            }

            if(createdNew)
            {
                _processGuard = localprocessGuard;
            }
            else
            {
                Environment.Exit(Environment.ExitCode);
            }

            Browser.Startup();

            new Bootstrapper().Start();

            //2013.07.01: Ashley Lewis for bug 9817 - initialize app exception handler framework
            _appExceptionPopupController = new AppExceptionPopupController();
            _appExceptionHandler = new AppExceptionHandler(_appExceptionPopupController, this, ServerUtil.GetLocalhostServer().Environment, new EventAggregator());

            base.OnStartup(e);
        }

        public bool ShouldRestart { get; set; }

        public bool HasShutdownStarted
        {
            get
            {
                return Dispatcher.CurrentDispatcher.HasShutdownStarted || Dispatcher.CurrentDispatcher.HasShutdownFinished || _hasShutdownStarted;
            }
            set
            {
                _hasShutdownStarted = value;
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            HasShutdownStarted = true;
            DebugDispatcher.Instance.Shutdown();
            Browser.Shutdown();
            base.OnExit(e);
            if(ShouldRestart)
            {
                Task.Run(() => Process.Start(ResourceAssembly.Location, Guid.NewGuid().ToString()));
            }
            Environment.Exit(0);
        }

        private void OnApplicationDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = HasShutdownStarted || _appExceptionHandler.Handle(e.Exception);
        }
    }
}
