using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Dev2.Common;
using Dev2.Diagnostics;
using Dev2.Studio.Core.AppResources.Browsers;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Utils;
using Dev2.Studio.Factory;
using Dev2.Studio.ViewModels;

namespace Dev2.Studio
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private static Mutex _processGuard;
        public App()
        {
            bool createdNew;
            Mutex localprocessGuard = new Mutex(true, "Warewolf Studio", out createdNew);
            if(createdNew)
            {
                _processGuard = localprocessGuard;
            }
            else
            {
                Environment.Exit(Environment.ExitCode);
            }
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
            Browser.Startup();

            new Bootstrapper().Start();

            base.OnStartup(e);
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

            Environment.Exit(0);
            };

            bw.RunWorkerAsync();

        }

        static bool IsAutoConnectHelperError(DispatcherUnhandledExceptionEventArgs e)
        {
            return e.Exception is NullReferenceException
                   && e.Exception.Source == "System.Activities.Presentation"
                   && e.Exception.StackTrace.Contains("AutoConnectHelper");
        }

        private void OnApplicationDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                // 2013.06.20 - TWR - added AutoConnectHelper by-pass because it's so annoying!
                if(IsAutoConnectHelperError(e))
                {
                    e.Handled = true;
                    return;
                }


                StudioLogger.LogMessage(e.Exception.Message);
                StudioLogger.LogMessage(e.Exception.StackTrace);

                // PBI 9598 - 2013.06.10 - TWR : added environmentModel parameter
                IServer server;
                var environmentModel = (server = ServerUtil.GetLocalhostServer()) == null ? null : server.Environment;
                ExceptionFactory.CreateViewModel(e.Exception, environmentModel).Show();
                e.Handled = true;
                //TODO Log
            }
            catch(Exception ex)
            {
                if(Current == null || Dispatcher.CurrentDispatcher.HasShutdownStarted || Dispatcher.CurrentDispatcher.HasShutdownFinished)
                {
                    // Do nothing if shutdown is in progress
                    return;
                }

                MessageBox.Show(
                    "An unexpected unrecoverable exception has been encountered. The application will now shut down.");
                StudioLogger.LogMessage(ex.Message);
                Current.Shutdown();
            }
        }
    }
}
