using System;
using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Gui
{
    /// <summary>
    /// Interaction logic for PreInstallProcess.xaml
    /// </summary>
    public partial class PreUnInstallProcess
    {
        private bool _serviceRemoved;

        public PreUnInstallProcess()
        {
            InitializeComponent();
        }

        public bool Rollback()
        {
            try
            {
                ServiceController sc = new ServiceController(InstallVariables.ServerService);

                if(sc.Status == ServiceControllerStatus.Running)
                {
                    sc.Stop();
                    sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(InstallVariables.DefaultWaitInSeconds)); // wait ;)
                    // The pre-uninstall process has finished.
                    if(sc.Status == ServiceControllerStatus.Stopped)
                    {
                        RemoveService();
                    }
                }
                else if(sc.Status == ServiceControllerStatus.Stopped)
                {
                    RemoveService();
                }

                sc.Dispose();

            }
            catch
            {
                // Best effort ;)
            }

            return _serviceRemoved;
        }

        /// <summary>
        /// Removes the service.
        /// </summary>
        private void RemoveService()
        {

            _serviceRemoved = false;

            try
            {
                ServiceInstaller installer = new ServiceInstaller();
                InstallContext context = new InstallContext("<<log file path>>", null);
                installer.Context = context;
                installer.ServiceName = InstallVariables.ServerService;
                // ReSharper disable AssignNullToNotNullAttribute
                installer.Uninstall(null);
                // ReSharper restore AssignNullToNotNullAttribute

                _serviceRemoved = true;

                installer.Dispose();

                // close the ports we opened ;)
                ClosePorts();
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch(Exception)
            // ReSharper restore EmptyGeneralCatchClause
            {
                // just being safe ;)

            }
        }

        private void ClosePorts()
        {
            var args = new[] { "http delete urlacl url={http://*:3142}/", "http delete urlacl url={https://*:3143}/" };

            //var args = string.Format("http add urlacl url={0}/ user=\\Everyone", url);
            try
            {
                foreach(var arg in args)
                {
                    ProcessHost.Invoke(null, "netsh.exe", arg);
                }

            }
            catch(Exception e)
            {

            }
        }

        /// <summary>
        /// Sets the success messasge.
        /// </summary>
        private void SetSuccessMessasge(string msg)
        {
            PreUnInstallMsg.Text = msg;
            preUnInstallStatusImg.Visibility = Visibility.Visible;
            btnRerun.Visibility = Visibility.Collapsed;
            preUnInstallStatusImg.Source =
                new BitmapImage(new Uri("pack://application:,,,/Resourcefiles/tick.png",
                                        UriKind.RelativeOrAbsolute));
            CanGoNext = true;
        }



        /// <summary>
        /// Sets the failure message.
        /// </summary>
        private void SetFailureMessage()
        {
            PreUnInstallMsg.Text = "Cannot remove server instance";
            preUnInstallStatusImg.Source =
                new BitmapImage(new Uri("pack://application:,,,/Resourcefiles/cross.png",
                                        UriKind.RelativeOrAbsolute));
            preUnInstallStatusImg.Visibility = Visibility.Visible;
            btnRerun.Visibility = Visibility.Visible;
            CanGoNext = false;
        }

        /// <summary>
        /// Handles the OnClick event of the BtnRerun control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void BtnRerun_OnClick(object sender, RoutedEventArgs e)
        {
            PreUnInstallStep_Entered(sender, null);
        }

        /// <summary>
        /// Handles the Entered event of the PreUnInstallStep control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SharpSetup.UI.Wpf.Base.ChangeStepRoutedEventArgs"/> instance containing the event data.</param>
        public void PreUnInstallStep_Entered(object sender, SharpSetup.UI.Wpf.Base.ChangeStepRoutedEventArgs e)
        {
            try
            {
                ServiceController sc = new ServiceController(InstallVariables.ServerService);

                if(sc.Status == ServiceControllerStatus.Running)
                {
                    sc.Stop();
                    sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(InstallVariables.DefaultWaitInSeconds)); // wait ;)
                    // The pre-uninstall process has finished.
                    if(sc.Status == ServiceControllerStatus.Stopped)
                    {

                        btnRerun.Visibility = Visibility.Hidden;
                        preUnInstallStatusImg.Visibility = Visibility.Hidden;

                        // Get the BackgroundWorker that raised this event.
                        BackgroundWorker worker = new BackgroundWorker();
                        worker.DoWork += delegate
                        {

                            RemoveService();
                        };

                        worker.RunWorkerCompleted += delegate
                        {
                            if(_serviceRemoved)
                            {
                                SetSuccessMessasge("Server instance removed");
                            }
                            else
                            {
                                SetFailureMessage();
                            }
                        };

                        worker.RunWorkerAsync();
                    }
                    else
                    {
                        SetFailureMessage();
                    }
                }
                else if(sc.Status == ServiceControllerStatus.Stopped)
                {

                    BackgroundWorker worker = new BackgroundWorker();

                    btnRerun.Visibility = Visibility.Hidden;
                    preUnInstallStatusImg.Visibility = Visibility.Hidden;

                    worker.DoWork += delegate
                    {

                        RemoveService();
                    };

                    worker.RunWorkerCompleted += delegate
                    {
                        if(_serviceRemoved)
                        {
                            SetSuccessMessasge("Server instance removed");
                        }
                        else
                        {
                            SetFailureMessage();
                        }
                    };

                    worker.RunWorkerAsync();
                }
                else
                {
                    SetFailureMessage();
                }
                sc.Dispose();
            }
            catch(InvalidOperationException ioe)
            {
                // magic string stating that service is not present ;)
                if(ioe.Message.IndexOf(InstallVariables.ServerService + " was not found on computer", StringComparison.Ordinal) > 0)
                {
                    SetSuccessMessasge("Scan for server services complete");
                }
                else
                {
                    SetFailureMessage();
                }
            }
            catch(Exception)
            {
                SetFailureMessage();
            }
        }

        void BtnRemoveAllTraces(object sender, RoutedEventArgs e)
        {
            InstallVariables.RemoveAllItems = !(InstallVariables.RemoveAllItems);
        }
    }
}
