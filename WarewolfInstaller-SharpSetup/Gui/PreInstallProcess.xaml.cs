using System;
using System.Collections.Generic;
using System.ServiceProcess;
using System.Windows;
using System.Windows.Media.Imaging;
using SharpSetup.UI.Wpf.Base;

namespace Gui
{
    /// <summary>
    /// Interaction logic for PreInstallProcess.xaml
    /// </summary>
    public partial class PreInstallProcess
    {
        public PreInstallProcess(int stepNumber, List<string> listOfStepNames)
        {
            InitializeComponent();
            // enable shortcut install
            InstallVariables.InstallShortcuts = true;
            cbInstallShortcuts.Focus();
            DataContext = new InfoStepDataContext(stepNumber, listOfStepNames);
        }

        /// <summary>
        /// Executes the process.
        /// </summary>
        public void ExecuteProcess()
        {
            PreInstallStep_Entered(null, null);
        }

        /// <summary>
        /// Handles the OnClick event of the BtnRerun control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void BtnRerun_OnClick(object sender, RoutedEventArgs e)
        {
            PreInstallStep_Entered(sender, null);
        }

        /// <summary>
        /// Sets the success message.
        /// </summary>
        /// <param name="msg">The MSG.</param>
        private void SetSuccessMessasge(string msg)
        {
            PreInstallMsg.Text = msg;
            preInstallStatusImg.Visibility = Visibility.Visible;
            postInstallStatusCircularProgressBar.Visibility = Visibility.Collapsed;
            preInstallStatusImg.Source =
                new BitmapImage(new Uri("pack://application:,,,/Resourcefiles/Images/tick.png",
                                        UriKind.RelativeOrAbsolute));
            btnRerun.Visibility = Visibility.Collapsed;
        }

        private void SetCleanupMessage()
        {
            PreInstallMsg.Text = InstallVariables.RollbackMessage;
            preInstallStatusImg.Visibility = Visibility.Collapsed;
            postInstallStatusCircularProgressBar.Visibility = Visibility.Visible;
            btnRerun.Visibility = Visibility.Collapsed;
        }


        /// <summary>
        /// Sets the failure message.
        /// </summary>
        private void SetFailureMessage(string msg = null)
        {
            PreInstallMsg.Text = msg ?? "Cannot stop server instance";
            preInstallStatusImg.Source =
                new BitmapImage(new Uri("pack://application:,,,/Resourcefiles/Images/cross.png",
                                        UriKind.RelativeOrAbsolute));
            preInstallStatusImg.Visibility = Visibility.Visible;
            postInstallStatusCircularProgressBar.Visibility = Visibility.Collapsed;
            CanGoNext = false;
            btnRerun.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Shows the cancel error.
        /// </summary>
        private void ShowCancelError()
        {
            MessageBox.Show("Failed to rollback installer progress", "Error");
        }

        /// <summary>
        /// Opens the ports.
        /// </summary>
        private void OpenPorts()
        {
            // NOTE Use : netsh.exe http show urlacl - To view urlacl rules ;)

            var args = new[] { @"http add urlacl url=http://*:3142/  user=\Everyone", @"http add urlacl url=https://*:3143/ user=\Everyone" };

            //var args = string.Format("http add urlacl url={0}/ user=\\Everyone", url);
            try
            {
                foreach(var arg in args)
                {
                    bool invoke = ProcessHost.Invoke(null, @"C:\Windows\system32\netsh.exe", arg);
                    if(!invoke)
                    {
                        SetFailureMessage(string.Format("There was an error adding url: {0}", arg));
                    }
                }

            }
            catch(Exception e)
            {
                SetFailureMessage(e.Message);
            }
        }



        /// <summary>
        /// Handles the Entered event of the PreInstallStep control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SharpSetup.UI.Wpf.Base.ChangeStepRoutedEventArgs"/> instance containing the event data.</param>
        public void PreInstallStep_Entered(object sender, ChangeStepRoutedEventArgs e)
        {

            // Setup a cancel action ;)
            Cancel += OnCancel;

            try
            {
                // Open the required ports ;)
                OpenPorts();

                ServiceController sc = new ServiceController(InstallVariables.ServerService);
                if(sc.Status == ServiceControllerStatus.Running)
                {
                    sc.Stop();
                    sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(InstallVariables.DefaultWaitInSeconds)); // wait ;)
                    // The pre-install process has finished.
                    if(sc.Status == ServiceControllerStatus.Stopped)
                    {
                        SetSuccessMessasge("Server instance stopped");
                    }
                    else
                    {
                        SetFailureMessage();
                    }
                }
                else if(sc.Status == ServiceControllerStatus.Stopped)
                {
                    SetSuccessMessasge("Server instance stopped");
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
                // Service not present ;)
                SetSuccessMessasge("Scan for server services complete");
            }

        }

        private void OnCancel(object sender, ChangeStepRoutedEventArgs changeStepRoutedEventArgs)
        {
            SetCleanupMessage();
            try
            {
                ServiceController sc = new ServiceController(InstallVariables.ServerService);

                // Attempt to re-start the service ;)
                if(sc.Status != ServiceControllerStatus.Running)
                {
                    try
                    {
                        sc.Start();
                        // wait ;) 
                        sc.WaitForStatus(ServiceControllerStatus.Running,
                                         TimeSpan.FromSeconds(InstallVariables.DefaultWaitInSeconds));
                    }
                    catch(Exception)
                    {
                        ShowCancelError();
                    }
                }

                try
                {
                    if(sc.Status != ServiceControllerStatus.Running)
                    {
                        ShowCancelError();
                    }
                    else
                    {
                        SetSuccessMessasge("Rollback completed");
                    }
                }
                catch(InvalidOperationException ioe)
                {
                    // magic string stating that service is not present ;)
                    if(ioe.Message.IndexOf(InstallVariables.ServerService + " was not found on computer",
                                            StringComparison.Ordinal) < 0)
                    {
                        ShowCancelError();
                    }
                    else
                    {
                        SetSuccessMessasge("Rollback completed");
                    }

                }
                catch(Exception)
                {
                    // service not present ;)
                    SetSuccessMessasge("Rollback completed");
                }

                sc.Dispose();
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch(Exception)
            // ReSharper restore EmptyGeneralCatchClause
            {
                //MessageBox.Show(e.Message);
                // Avoid showing message here, cancel is just cancel ;)
            }

        }

        void BtnInstallShortcuts(object sender, RoutedEventArgs e)
        {
            InstallVariables.InstallShortcuts = !(InstallVariables.InstallShortcuts);
        }
    }
}
