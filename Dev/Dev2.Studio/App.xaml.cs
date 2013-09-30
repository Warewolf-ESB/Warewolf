using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Dev2.Diagnostics;
using Dev2.Studio.Core.AppResources.Browsers;
using Dev2.Studio.Diagnostics;
using Dev2.Studio.ViewModels;

namespace Dev2.Studio
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : IApp
    {
        MainViewModel _mainViewModel;
        private Mutex _processGuard = null;
        private AppExceptionHandler _appExceptionHandler;
        private bool _hasShutdownStarted;

        public App()
        {
            _hasShutdownStarted = false;
            ShouldRestart = false;
            InitializeComponent();
        }

        public static bool IsAutomationMode
        {
            get
            {
#if DEBUG
                return true;
#else
                return false;
#endif
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            bool createdNew;
            var localprocessGuard = e.Args.Length > 0
                                        ? new Mutex(true, e.Args[0], out createdNew)
                                        : new Mutex(true, "Warewolf Studio", out createdNew);

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

            base.OnStartup(e);

            _mainViewModel = MainWindow.DataContext as MainViewModel;

            //2013.07.01: Ashley Lewis for bug 9817 - setup exception handler on 'this', with main window data context as the popup dialog controller
            _appExceptionHandler = new AppExceptionHandler(this, _mainViewModel);

#if ! (DEBUG)
            var versionChecker = new VersionChecker();
            versionChecker.IsLatest(new ProgressFileDownloader(MainWindow), new ProgressDialog(MainWindow));
#endif
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if(_mainViewModel != null)
            {
                _mainViewModel.PersistTabs();
            }

            HasShutdownStarted = true;
            DebugDispatcher.Instance.Shutdown();
            Browser.Shutdown();

            try
            {
                base.OnExit(e);
            }
            catch
            {
                // Best effort ;)
            }

            if(ShouldRestart)
            {
                Task.Run(() => Process.Start(ResourceAssembly.Location, Guid.NewGuid().ToString()));
            }
            Environment.Exit(0);
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

        private void OnApplicationDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = HasShutdownStarted || _appExceptionHandler.Handle(e.Exception);
        }
    }
}
