using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Caliburn.Micro;
using Dev2.Composition;
using Dev2.Diagnostics;
using Dev2.Studio.Core.AppResources.Browsers;
using Dev2.Studio.Core.Utils;
using Dev2.Studio.Diagnostics;
using Dev2.Studio.ViewModels;

namespace Dev2.Studio
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : IApp
    {
        private Mutex _processGuard = null;
        private AppExceptionHandler _appExceptionHandler;
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
            
            base.OnStartup(e);

            //2013.07.01: Ashley Lewis for bug 9817 - setup exception handler on 'this'
            var eventAggregator = ImportService.GetExportValue<IEventAggregator>();
            _appExceptionHandler = new AppExceptionHandler(eventAggregator, this, MainWindow.DataContext as IMainViewModel);
        }

        protected override void OnExit(ExitEventArgs e)
        {

            /*
             * 05.07.2013
             * 
             * It may look silly to background a IsBusy check but this allows the 
             * current open tabs to all persist and for the studio to terminate 
             * in a timely manor ;)
             * 
             */

            // Process saving tabs and such when exiting ;)
            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += (sender, args) =>
            {
                while (MainViewModel.IsBusy)
                {
                    Thread.Sleep(50);
                }

            };

            bw.RunWorkerAsync();

            // wait a while, 10 seconds  to save everything ;)
            int cnt = 0;
            while (cnt < 5)
            {
                Thread.Sleep(1000);
                cnt++;
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
