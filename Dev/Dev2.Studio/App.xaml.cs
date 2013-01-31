using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Dev2.Studio.Core.AppResources.Browsers;
using Dev2.Studio.Factory;
using System.Windows;
using System.Windows.Threading;

namespace Dev2.Studio
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        public App()
        {
            //Bug 8403
            var currentProc = Process.GetCurrentProcess().ProcessName;
            //if(currentProc.Contains(".vshost")) currentProc = currentProc.Remove(Process.GetCurrentProcess().ProcessName.IndexOf(".vshost"));
            var studioProcesses = Process.GetProcessesByName(currentProc);
           // if (Process.GetProcessesByName(currentProc + ".vshost").Length > 0) studioProcesses.ToObservableCollection().Add(Process.GetProcessesByName(currentProc+".vshost")[0]);
            if (studioProcesses.Length > 1)
            {
                SetForegroundWindow(studioProcesses[0].MainWindowHandle);
                SetForegroundWindow(studioProcesses[1].MainWindowHandle);
                Current.Shutdown();
            }
            else
            {
                InitializeComponent();
            }
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
            Browser.Shutdown();
            base.OnExit(e);
        }

        private void OnApplicationDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            try
        {
            ExceptionFactory.CreateViewModel(e.Exception).Show();
                e.Handled = true;
            //TODO Log
            }
            catch (Exception)
            {
                MessageBox.Show(
                    "An unexpected unrecoverable exception has been encountered. The application will now shut down;");
                Current.Shutdown();
            }
        }
    }
}
