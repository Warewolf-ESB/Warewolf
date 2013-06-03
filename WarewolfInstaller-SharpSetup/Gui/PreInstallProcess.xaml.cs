using System;
using System.ServiceProcess;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Gui
{
    /// <summary>
    /// Interaction logic for PreInstallProcess.xaml
    /// </summary>
    public partial class PreInstallProcess
    {
        public PreInstallProcess()
        {
            InitializeComponent();
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
        /// Sets the success messasge.
        /// </summary>
        /// <param name="msg">The MSG.</param>
        private void SetSuccessMessasge(string msg)
        {
            PreInstallMsg.Text = msg;
            preInstallStatusImg.Visibility = Visibility.Visible;
            preInstallStatusImg.Source =
                new BitmapImage(new Uri("pack://application:,,,/Resourcefiles/tick.png",
                                        UriKind.RelativeOrAbsolute));
            btnRerun.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Sets the failure message.
        /// </summary>
        private void SetFailureMessage()
        {
            PreInstallMsg.Text = "Cannot stop server instance";
            preInstallStatusImg.Source =
                new BitmapImage(new Uri("pack://application:,,,/Resourcefiles/cross.png",
                                        UriKind.RelativeOrAbsolute));
            preInstallStatusImg.Visibility = Visibility.Visible;
            CanGoNext = false;
            btnRerun.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Handles the Entered event of the PreInstallStep control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SharpSetup.UI.Wpf.Base.ChangeStepRoutedEventArgs"/> instance containing the event data.</param>
        private void PreInstallStep_Entered(object sender, SharpSetup.UI.Wpf.Base.ChangeStepRoutedEventArgs e)
        {

                try
                {

                    ServiceController sc = new ServiceController(InstallVariables.ServerService);

                    if (sc.Status == ServiceControllerStatus.Running)
                    {
                        sc.Stop();
                        sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(10)); // wait 10 seconds ;)
                        // The pre-install process has finished.
                        if (sc.Status == ServiceControllerStatus.Stopped)
                        {
                            SetSuccessMessasge("Server instance stopped");
                        }
                        else
                        {
                            SetFailureMessage();
                        }
                    }
                    else if (sc.Status == ServiceControllerStatus.Stopped)
                    {
                        SetSuccessMessasge("Server instance stopped");
                    }
                    else
                    {
                        SetFailureMessage();
                    }
                    sc.Dispose();
                }
                catch (InvalidOperationException ioe)
                {
                    // magic string stating that service is not present ;)
                    if (ioe.Message.IndexOf(InstallVariables.ServerService+" was not found on computer", StringComparison.Ordinal) > 0)
                    {
                        SetSuccessMessasge("Scan for server services complete");
                    }
                    else
                    {
                        SetFailureMessage();
                    }
                    
                }
                catch (Exception)
                {
                    // Service not present ;)
                    SetSuccessMessasge("Scan for server services complete");
                }

        }
    }
}
