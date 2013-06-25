using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Dev2.Diagnostics;
using Dev2.Studio.Core.AppResources.Browsers;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Utils;
using Dev2.Studio.Factory;

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
            //CheckForDuplicateProcess();//Bug 8403
            bool createdNew;
            Mutex localprocessGuard = new Mutex(true, "Dev2.Studio", out createdNew);
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
            DebugDispatcher.Instance.Shutdown();
            // BackgroundDispatcher.Instance.Shutdown();
            Browser.Shutdown();
            base.OnExit(e);
            Environment.Exit(0);
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
                File.WriteAllText("StudioError.txt", ex.Message);
                Current.Shutdown();
            }
        }
        private void CheckForDuplicateProcess()
        {
            //Bug 8403
            var studioProcesses = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName);
            if(studioProcesses.Length > 1)
            {
                SetForegroundWindow(studioProcesses[0].MainWindowHandle);
                SetForegroundWindow(studioProcesses[1].MainWindowHandle);
                Current.Shutdown();
            }
        }

        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);
    }
}
