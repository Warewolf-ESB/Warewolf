using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Security.Permissions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Wrappers;
using Dev2.CustomControls.Progress;
using Dev2.Diagnostics.Debug;
using Dev2.Instrumentation;
using Dev2.Studio.Core.AppResources.Browsers;
using Dev2.Studio.Core.Helpers;
using Dev2.Studio.Diagnostics;
using Dev2.Studio.ViewModels;
using Dev2.Util;
using Dev2.Views.Dialogs;

// ReSharper disable RedundantUsingDirective

// ReSharper restore RedundantUsingDirective

// ReSharper disable CheckNamespace
namespace Dev2.Studio
// ReSharper restore CheckNamespace
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : IApp
    {
        MainViewModel _mainViewModel;
        //This is ignored because when starting the studio twice the second one crashes without this line
        // ReSharper disable RedundantDefaultFieldInitializer
        // ReSharper disable NotAccessedField.Local
        // ReSharper disable RedundantDefaultFieldInitializer
        private Mutex _processGuard = null;
        // ReSharper restore RedundantDefaultFieldInitializer
        // ReSharper restore NotAccessedField.Local
        private AppExceptionHandler _appExceptionHandler;
        private bool _hasShutdownStarted;

        public App()
        {
            // PrincipalPolicy must be set to WindowsPrincipal to check roles.
            AppDomain.CurrentDomain.SetPrincipalPolicy(System.Security.Principal.PrincipalPolicy.WindowsPrincipal);
            _hasShutdownStarted = false;
            ShouldRestart = false;
            InitializeComponent();
            AppSettings.LocalHost = ConfigurationManager.AppSettings["LocalHostServer"];
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

        [PrincipalPermission(SecurityAction.Demand)]  // Principal must be authenticated
        protected override void OnStartup(StartupEventArgs e)
        {
            Tracker.StartStudio();
            bool createdNew;

            Task.Factory.StartNew(() =>
                {
                    var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Warewolf", "Feedback");
                    DirectoryHelper.CleanUp(path);
                    DirectoryHelper.CleanUp(Path.Combine(Path.GetTempPath(), "Warewolf", "Debug"));
                });

            // ReSharper disable once UnusedVariable
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

    

            base.OnStartup(e);

            _mainViewModel = MainWindow.DataContext as MainViewModel;
            //2013.07.01: Ashley Lewis for bug 9817 - setup exception handler on 'this', with main window data context as the popup dialog controller
            _appExceptionHandler = new AppExceptionHandler(this, _mainViewModel);

#if ! (DEBUG)
            var versionChecker = new VersionChecker();
            if(versionChecker.GetNewerVersion())
            {
                WebLatestVersionDialog dialog = new WebLatestVersionDialog();
                dialog.ShowDialog();
            }
#endif
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Tracker.Stop();

            // this is already handled ;)
            if(_mainViewModel != null)
            {
                _mainViewModel.PersistTabs(true);
            }
            ProgressFileDownloader.PerformCleanup(new DirectoryWrapper(), GlobalConstants.VersionDownloadPath, new FileWrapper());
            HasShutdownStarted = true;
            DebugDispatcher.Instance.Shutdown();
            Browser.Shutdown();
            try
            {
                base.OnExit(e);
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch
            // ReSharper restore EmptyGeneralCatchClause
            {
                // Best effort ;)
            }

            ForceShutdown();
        }

        void ForceShutdown()
        {
            if(ShouldRestart)
            {
                Task.Run(() => Process.Start(ResourceAssembly.Location, Guid.NewGuid().ToString()));
            }
            Environment.Exit(0);
        }

        #region Implementation of IApp

        #region Implementation of IApp

        public new void Shutdown()
        {
            try
            {
                base.Shutdown();
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch
            // ReSharper restore EmptyGeneralCatchClause
            {
                // Best effort ;)
            }
            ForceShutdown();
        }

        #endregion

        #endregion

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
            Tracker.TrackException(GetType().Name, "OnApplicationDispatcherUnhandledException", e.Exception);
            if(_appExceptionHandler != null)
            {
                e.Handled = HasShutdownStarted || _appExceptionHandler.Handle(e.Exception);
            }
            else
            {
                MessageBox.Show("Fatal Error : " + e.Exception);
            }
        }
    }
}
