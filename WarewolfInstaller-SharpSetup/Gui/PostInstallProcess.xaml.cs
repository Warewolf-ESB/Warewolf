using System;
using System.ComponentModel;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
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
        /// Handles the OnClick event of the BtnRerun control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void BtnRerun_OnClick(object sender, RoutedEventArgs e)
        {
            PostInstallStep_Entered(sender, null);
        }

        private void SetSuccessMessasge()
        {
            PostInstallMsg.Text = "Started server service";
            postInstallStatusImg.Visibility = Visibility.Visible;
            postInstallStatusImg.Source =
                new BitmapImage(new Uri("pack://application:,,,/Resourcefiles/tick.png",
                                        UriKind.RelativeOrAbsolute));
            CanGoNext = true;
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

            ProcessStartInfo psi = new ProcessStartInfo();

            psi.FileName = serverInstallLocation;
            psi.Arguments = "-i"; // install flag
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            psi.UseShellExecute = true;

            try
            {
                Process p = Process.Start(psi);
                //p.WaitForExit(10000); // wait up to 10 seconds for process exit ;)

                int cnt = 0;
                while (cnt < 10 && !p.HasExited)
                {
                    Thread.Sleep(1000);
                    cnt++;
                }

                // now try and start the service ;)
                if (sc.Status == ServiceControllerStatus.Stopped)
                {
                    sc.Start();
                    sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(10));
                    // wait up to 10 seconds for service to start ;)

                    if (sc.Status == ServiceControllerStatus.Running)
                    {
                        _serviceInstalled = true;
                    }

                }
                else if (sc.Status == ServiceControllerStatus.Running)
                {
                    _serviceInstalled = true;
                }
            }
            catch(Exception)
            {
                try
                {
                    // maybe it is already installed, just try and start it ;)
                    sc.Start();
                    sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(10));
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
                }
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
                var trans = new PreUnInstallProcess();
                trans.Rollback();

                // TODO : Uninstall service
                // TODO : Uninstall software ;)
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
                        SetSuccessMessasge();
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
