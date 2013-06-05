using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
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

        private bool _serviceInstalled = false;
        private bool _serviceInstallException = false;

        public PostInstallProcess()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Performs the custom operation ;)
        /// 
        /// Change Log : 
        /// + Release 0.2.13.1 - Swap old secure config name to new
        /// 
        /// </summary>
        private void CustomOperation()
        {
            // Swap config files around
            var oldConfig = InstallVariables.InstallRoot + @"\Dev2.Server.exe.secureconfig";
            var newConfig = InstallVariables.InstallRoot + @"\Warewolf Server.exe.secureconfig";

            if (File.Exists(oldConfig))
            {
                File.Move(oldConfig, newConfig);
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
        /// Sets the success messasge.
        /// </summary>
        /// <param name="msg">The MSG.</param>
        private void SetSuccessMessasge(string msg)
        {
            PostInstallMsg.Text = msg;
            postInstallStatusImg.Visibility = Visibility.Visible;
            postInstallStatusImg.Source =
                new BitmapImage(new Uri("pack://application:,,,/Resourcefiles/tick.png",
                                        UriKind.RelativeOrAbsolute));
            CanGoNext = true;
            btnRerun.Visibility = Visibility.Hidden;
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
            postInstallStatusImg.Visibility = Visibility.Hidden;
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
                new BitmapImage(new Uri("pack://application:,,,/Resourcefiles/cross.png",
                                        UriKind.RelativeOrAbsolute));
            postInstallStatusImg.Visibility = Visibility.Visible;
            CanGoNext = false;
            btnRerun.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Installs the service.
        /// </summary>
        /// <param name="installRoot">The install root.</param>
        private void InstallService(string installRoot)
        {
            ServiceController sc = new ServiceController(InstallVariables.ServerService);
            // Gain access to warewolf exe location ;)
            var serverInstallLocation = Path.Combine(installRoot, "Server", InstallVariables.ServerService + ".exe");

            // TODO : Remove after r 0.2.13.1
            // Perform any post install custom operation ;)
            CustomOperation();

            try
            {
                ProcessStartInfo psi = new ProcessStartInfo();

                psi.FileName = serverInstallLocation;
                psi.Arguments = "-i"; // install flag
                psi.WindowStyle = ProcessWindowStyle.Hidden;
                psi.UseShellExecute = true;

                Process p = Process.Start(psi);

                // Fails to work correctly
                //p.WaitForExit(10000); // wait up to 10 seconds for process exit ;)

                int cnt = 0;
                while (cnt < InstallVariables.DefaultWaitInSeconds && !p.HasExited)
                {
                    Thread.Sleep(1000);
                    cnt++;
                }

                // now try and start the service ;)
                if (sc.Status == ServiceControllerStatus.Stopped)
                {
                    sc.Start();
                    // wait start ;)
                    sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(InstallVariables.DefaultWaitInSeconds));
                    
                    if (sc.Status == ServiceControllerStatus.Running)
                    {
                        _serviceInstalled = true;
                    }
                    else
                    {
                        // wait a bit more ;)
                        cnt = 0;
                        while (cnt < InstallVariables.DefaultWaitInSeconds && !_serviceInstalled)
                        {
                            Thread.Sleep(1000);
                            if (sc.Status == ServiceControllerStatus.Running)
                            {
                                _serviceInstalled = true;
                            }
                            cnt++;
                        }
                    }

                }
                else if (sc.Status == ServiceControllerStatus.Running)
                {
                    _serviceInstalled = true;
                }
                else
                {
                    // wait some more ;)
                    sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(InstallVariables.DefaultWaitInSeconds));

                    if (sc.Status == ServiceControllerStatus.Running)
                    {
                        _serviceInstalled = true;
                    }
                }
            }
            catch(Exception e)
            {
                try
                {
                    // maybe it is already installed, just try and start it ;)
                    sc.Start();
                    sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(InstallVariables.DefaultWaitInSeconds));
                    if (sc.Status == ServiceControllerStatus.Running)
                    {
                        _serviceInstalled = true;
                    }
                    else
                    {
                        _serviceInstallException = true;
                    }
                }
                catch
                {
                    _serviceInstallException = true;
                    //MessageBox.Show(e.Message);
                }

                MessageBox.Show(e.Message);
            }
            finally
            {
                sc.Dispose();    
            }

        }

        /// <summary>
        /// Handles the Entered event of the PostInstallStep control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SharpSetup.UI.Wpf.Base.ChangeStepRoutedEventArgs"/> instance containing the event data.</param>
        private void PostInstallStep_Entered(object sender, SharpSetup.UI.Wpf.Base.ChangeStepRoutedEventArgs e)
        {

            CanGoNext = false;
            postInstallStatusImg.Visibility = Visibility.Hidden;
            btnRerun.Visibility = Visibility.Hidden;
            // Setup a cancel action ;)
            Cancel += delegate(object o, ChangeStepRoutedEventArgs args)
            {
                SetCleanupMessage();
                var trans = new PreUnInstallProcess();
                
                if (!trans.Rollback())
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

            if (!string.IsNullOrEmpty(InstallVariables.InstallRoot))
            {

                // Get the BackgroundWorker that raised this event.
                BackgroundWorker worker = new BackgroundWorker();
                worker.DoWork += delegate
                {
                    InstallService(InstallVariables.InstallRoot);
                };

                worker.RunWorkerCompleted += delegate
                {

                    if (_serviceInstalled && !_serviceInstallException)
                    {
                        SetSuccessMessasge("Started server service");
                    }
                    else if (!_serviceInstalled && _serviceInstallException)
                    {
                        SetFailureMessage("Cannot install server as service");
                    }
                    else if (!_serviceInstalled && !_serviceInstallException)
                    {
                        SetFailureMessage("Cannot start server service");
                    }
                };

                worker.RunWorkerAsync();
            }
            else
            {
                SetFailureMessage("Installer cannot resolve server install location");
            }
        }
    }
}
