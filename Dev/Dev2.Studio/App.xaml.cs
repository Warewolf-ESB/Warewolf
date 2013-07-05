using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
        private static readonly string RootPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        private const string NewPath = @"Warewolf\";
        private static readonly string FullNewPath = Path.Combine(RootPath, NewPath);
        private const string OldPath = @"Dev2\";
        private static readonly string FullOldPath = Path.Combine(RootPath, OldPath);

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
            MigrateExistingTempData();

            Browser.Startup();

            new Bootstrapper().Start();

            base.OnStartup(e);
        }

        private static void MigrateExistingTempData()
        {
            if(!Directory.Exists(FullOldPath))
            {
                return;//no old data to migrate
            }

            if(!Directory.Exists(FullNewPath))
            {
                Directory.CreateDirectory(FullNewPath);//create new path if doesnt exist
            }

            DirectoryCopy(FullOldPath, FullNewPath);//perform recursive file copy
        }

        public static void DirectoryCopy(string sourceDirName, string destDirName)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            // If the destination directory doesn't exist, create it. 
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            // If copying subdirectories, copy them and their contents to new location. 
            foreach (DirectoryInfo subdir in dirs)
            {
                string temppath = Path.Combine(destDirName, subdir.Name);
                DirectoryCopy(subdir.FullName, temppath);
            }
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

                File.WriteAllText("StudioError.txt", e.Exception.Message);
                File.WriteAllText("StudioError.txt", e.Exception.StackTrace);
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
    }
}
