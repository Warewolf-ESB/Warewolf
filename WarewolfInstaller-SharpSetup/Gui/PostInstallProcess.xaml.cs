using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
using Gui.Utility;
using Ionic.Zip;
using SharpSetup.Base;
using SharpSetup.UI.Wpf.Base;
using Path = System.IO.Path;

namespace Gui
{
    /// <summary>
    /// Interaction logic for PreInstallProcess.xaml
    /// </summary>
    public partial class PostInstallProcess
    {

        private bool _serviceInstalled;
        private bool _serviceInstallException;

        public PostInstallProcess(int stepNumber, List<string> listOfStepNames)
        {
            InitializeComponent();
            DataContext = new InfoStepDataContext(stepNumber, listOfStepNames);
        }

        /// <summary>
        /// Performs the custom operation when installing ;)
        /// 
        /// Change Log : 
        /// + Release 0.2.13.1 - Swap old secure config name to new
        /// + Release 0.4.1.2 - Install Examples
        /// 
        /// </summary>
        private void CustomOperation()
        {
            SwapConfigs();
            InstallExamples();
        }

        private void InstallExamples()
        {
            var stream = ResourceExtractor.Fetch("Samples.zip");

            if(stream == null)
            {
                return;
            }

            using(stream)
            {
                // Extract zip and unpack ;)
                using(var zp = ZipFile.Read(stream))
                {
                    foreach(var ze in zp)
                    {
                        ze.Extract(InstallVariables.InstallRoot, ExtractExistingFileAction.OverwriteSilently);
                    }
                }
            }
        }

        private void SwapConfigs()
        {
            // Swap config files around
            var oldConfig = InstallVariables.InstallRoot + @"Server\Dev2.Server.exe.secureconfig";
            var newConfig = InstallVariables.InstallRoot + @"Server\Warewolf Server.exe.secureconfig";

            try
            {
                if(File.Exists(oldConfig))
                {
                    if(File.Exists(newConfig))
                    {
                        var newLoc = newConfig + ".new";
                        File.Move(newConfig, newLoc);
                    }

                    File.Move(oldConfig, newConfig);
                }
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch
            // ReSharper restore EmptyGeneralCatchClause
            {
                // Just making sure ;)
            }
        }

        /// <summary>
        /// Handles the OnClick event of the BtnRerun control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void BtnRerun_OnClick(object sender, RoutedEventArgs e)
        {
            PostInstallStep_Entered(sender, null);
        }

        /// <summary>
        /// Sets the success message.
        /// </summary>
        /// <param name="msg">The MSG.</param>
        private void SetSuccessMessasge(string msg)
        {
            PostInstallMsg.Text = msg;
            postInstallStatusImg.Visibility = Visibility.Visible;
            postInstallStatusImg.Source =
                new BitmapImage(new Uri("pack://application:,,,/Resourcefiles/Images/tick.png",
                                        UriKind.RelativeOrAbsolute));
            postInstallStatusCircularProgressBar.Visibility = Visibility.Hidden;
            CanGoNext = true;
            btnRerun.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Shows the cancel error.
        /// </summary>
        private void ShowCancelError()
        {
            MessageBox.Show("Failed to rollback installer progress", "Error");
        }

        /// <summary>
        /// Sets the cleanup message.
        /// </summary>
        private void SetCleanupMessage()
        {
            PostInstallMsg.Text = InstallVariables.RollbackMessage;
            postInstallStatusImg.Visibility = Visibility.Collapsed;
            postInstallStatusCircularProgressBar.Visibility = Visibility.Visible;
            CanGoNext = false;
            btnRerun.Visibility = Visibility.Hidden;
        }


        /// <summary>
        /// Sets the failure message.
        /// </summary>
        /// <param name="msg">The MSG.</param>
        private void SetFailureMessage(string msg)
        {
            PostInstallMsg.Text = msg;
            postInstallStatusImg.Source =
                new BitmapImage(new Uri("pack://application:,,,/Resourcefiles/Images/cross.png",
                                        UriKind.RelativeOrAbsolute));
            postInstallStatusImg.Visibility = Visibility.Visible;
            postInstallStatusCircularProgressBar.Visibility = Visibility.Collapsed;
            CanGoNext = false;
            btnRerun.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Starts the service.
        /// </summary>
        private void StartService(string serverInstallLocation)
        {
            try
            {
                ServiceController sc = new ServiceController(InstallVariables.ServerService);

                ProcessStartInfo psi = new ProcessStartInfo { FileName = serverInstallLocation, Arguments = "-i", WindowStyle = ProcessWindowStyle.Hidden, UseShellExecute = true };

                Process p = Process.Start(psi);

                int cnt = 0;
                while(p != null && (cnt < InstallVariables.DefaultWaitInSeconds && !p.HasExited))
                {
                    Thread.Sleep(1000);
                    cnt++;
                }

                // now try and start the service ;)
                if(sc.Status == ServiceControllerStatus.Stopped)
                {
                    sc.Start();
                    // wait start ;)
                    sc.WaitForStatus(ServiceControllerStatus.Running,
                                     TimeSpan.FromSeconds(InstallVariables.DefaultWaitInSeconds));

                    if(sc.Status == ServiceControllerStatus.Running)
                    {
                        _serviceInstalled = true;
                    }
                    else
                    {
                        // wait a bit more ;)
                        cnt = 0;
                        while(cnt < InstallVariables.DefaultWaitInSeconds && !_serviceInstalled)
                        {
                            Thread.Sleep(1000);
                            if(sc.Status == ServiceControllerStatus.Running)
                            {
                                _serviceInstalled = true;
                            }
                            cnt++;
                        }
                    }
                }
                else if(sc.Status == ServiceControllerStatus.Running)
                {
                    _serviceInstalled = true;
                }
                else
                {
                    // wait some more ;)
                    sc.WaitForStatus(ServiceControllerStatus.Running,
                                     TimeSpan.FromSeconds(InstallVariables.DefaultWaitInSeconds));

                    if(sc.Status == ServiceControllerStatus.Running)
                    {
                        _serviceInstalled = true;
                    }
                }

                sc.Dispose();
            }
            catch(Exception e)
            {
                try
                {
                    ServiceController sc = new ServiceController(InstallVariables.ServerService);
                    // maybe it is already installed, just try and start it ;)
                    sc.Start();
                    sc.WaitForStatus(ServiceControllerStatus.Running,
                                     TimeSpan.FromSeconds(InstallVariables.DefaultWaitInSeconds));
                    if(sc.Status == ServiceControllerStatus.Running)
                    {
                        _serviceInstalled = true;
                    }
                    else
                    {
                        _serviceInstallException = true;
                    }
                    sc.Dispose();
                }
                catch
                {
                    _serviceInstallException = true;
                    MessageBox.Show(e.Message);
                }

                MessageBox.Show(e.Message);
            }
        }

        /// <summary>
        /// Restarts the service.
        /// </summary>
        private void RestartService()
        {
            ServiceController sc = new ServiceController(InstallVariables.ServerService);


            if(sc.Status == ServiceControllerStatus.Running)
            {
                sc.Stop();
                sc.WaitForStatus(ServiceControllerStatus.Stopped,
                                 TimeSpan.FromSeconds(InstallVariables.DefaultWaitInSeconds)); // wait ;)

            }

            try
            {

                // maybe it is already installed, just try and start it ;)
                sc.Start();
                sc.WaitForStatus(ServiceControllerStatus.Running,
                                 TimeSpan.FromSeconds(InstallVariables.DefaultWaitInSeconds));
                if(sc.Status == ServiceControllerStatus.Running)
                {
                    _serviceInstalled = true;
                }
                else
                {
                    _serviceInstallException = true;
                }
                sc.Dispose();
            }
            catch(Exception e2)
            {
                _serviceInstallException = true;
                MessageBox.Show(e2.Message);
            }
        }

        /// <summary>
        /// Installs the service.
        /// </summary>
        /// <param name="installRoot">The install root.</param>
        private void InstallService(string installRoot)
        {
            // Gain access to warewolf exe location ;)
            var serverInstallLocation = Path.Combine(installRoot, "Server", InstallVariables.ServerService + ".exe");

            // TODO : Remove after r 0.2.13.1
            try
            {
                // Perform any post install custom operation ;)
                CustomOperation();
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch(Exception) { }
            // ReSharper restore EmptyGeneralCatchClause

            // We need to stop and restart the server - V 0.2.15.2 ;)
            StartService(serverInstallLocation);
            RestartService();

            // clean up any log files and junk ;)
            var serverRoot = Path.Combine(installRoot, "Server");
            CleanupOperation(serverRoot);

        }

        private static string outputData = string.Empty;
        private bool VerifyPortsAreOpen()
        {
            Process p = new Process { StartInfo = { FileName = @"C:\Windows\system32\netsh.exe", Arguments = "http show urlacl", RedirectStandardOutput = true, UseShellExecute = false, WindowStyle = ProcessWindowStyle.Hidden } };
            p.OutputDataReceived += OutputHandler;
            p.BeginOutputReadLine();
            p.Start();

            p.WaitForExit(30);

            // Now scan outputData for magical strings ;)
            if(outputData.IndexOf("http://*:3142/", StringComparison.Ordinal) >= 0)
            {
                if(outputData.IndexOf("https://*:3143/", StringComparison.Ordinal) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        private static void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            // Collect the sort command output. 
            var data = outLine.Data;
            if(!String.IsNullOrEmpty(data))
            {
                outputData += data;
            }
        }

        /// <summary>
        /// Cleanups the operation.
        /// </summary>
        private void CleanupOperation(string installLocation)
        {
            // two install log files
            var path = Path.Combine(installLocation, "Warewolf Server.InstallLog");
            var path2 = Path.Combine(installLocation, "Warewolf Server.InstallState");

            var paths = new[] { path, path2 };

            foreach(var p in paths)
            {
                try
                {
                    File.Delete(p);
                }
                // ReSharper disable EmptyGeneralCatchClause
                catch { }
                // ReSharper restore EmptyGeneralCatchClause
            }

        }

        /// <summary>
        /// Handles the Entered event of the PostInstallStep control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SharpSetup.UI.Wpf.Base.ChangeStepRoutedEventArgs"/> instance containing the event data.</param>
        private void PostInstallStep_Entered(object sender, ChangeStepRoutedEventArgs e)
        {

            CanGoNext = false;
            postInstallStatusImg.Visibility = Visibility.Collapsed;
            postInstallStatusCircularProgressBar.Visibility = Visibility.Visible;
            btnRerun.Visibility = Visibility.Hidden;
            // Setup a cancel action ;)
            Cancel += delegate
            {
                SetCleanupMessage();
                List<string> listOfStepNames = new List<string> { "License Agreement", "Pre UnInstall", "UnInstall", "Installation", "Post Install", "Finish" };
                var trans = new PreUnInstallProcess(2, listOfStepNames);

                if(!trans.Rollback())
                {
                    ShowCancelError();
                }
                else
                {
                    // Now uninstall?!
                    MsiConnection.Instance.Uninstall();
                    SetSuccessMessasge("Rollback complete");
                }
            };

            // attempts to install service ;)
            if(!string.IsNullOrEmpty(InstallVariables.InstallRoot))
            {

                // Get the BackgroundWorker that raised this event.
                BackgroundWorker worker = new BackgroundWorker();
                bool portAreOpen = false;
                worker.DoWork += delegate
                {
                    InstallService(InstallVariables.InstallRoot);
                    //portAreOpen = VerifyPortsAreOpen();
                };

                worker.RunWorkerCompleted += delegate
                {

                    if(_serviceInstalled && !_serviceInstallException)
                    {
                        // verify ports opened
                        //if(!portAreOpen)
                        //{
                            //SetFailureMessage("Failed to open Firewall port 3142 and 3142");
                        //}
                        //else
                        //{
                            SetSuccessMessasge("Started server service");
                        //}
                    }
                    else if(!_serviceInstalled && _serviceInstallException)
                    {
                        SetFailureMessage("Cannot install server as service");
                    }
                    else if(!_serviceInstalled && !_serviceInstallException)
                    {
                        SetFailureMessage("Cannot start server service");
                    }

                    InstallVariables.IsInstallMode = true;
                };

                try
                {
                    worker.RunWorkerAsync();
                }
                catch(Exception e1)
                {
                    MessageBox.Show(e1.Message);
                }
            }
            else
            {
                SetFailureMessage("Installer cannot resolve server install location");
            }
        }
    }
}
