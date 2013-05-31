using System;
using System.ServiceProcess;
using System.Windows;
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

        public void ExecuteProcess()
        {
            PreInstallStep_Entered(null, null);
        }

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
                            PreInstallMsg.Text = "SUCCESS: Server instance stopped";
                            preInstallStatusImg.Visibility = Visibility.Visible;
                            btnRerun.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            PreInstallMsg.Text = "FAILURE : Cannot stop server instance";
                            preInstallStatusImg.Source =
                                new BitmapImage(new Uri("pack://application:,,,/Resourcefiles/cross.png",
                                                        UriKind.RelativeOrAbsolute));
                            preInstallStatusImg.Visibility = Visibility.Visible;
                            CanGoNext = false;
                            btnRerun.Visibility = Visibility.Visible;
                        }
                    }
                    else if (sc.Status == ServiceControllerStatus.Stopped)
                    {
                        PreInstallMsg.Text = "SUCCESS: Server instance stopped";
                        preInstallStatusImg.Visibility = Visibility.Visible;
                        btnRerun.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        PreInstallMsg.Text = "FAILURE : Cannot stop server instance";
                        preInstallStatusImg.Source =
                            new BitmapImage(new Uri("pack://application:,,,/Resourcefiles/cross.png",
                                                    UriKind.RelativeOrAbsolute));
                        preInstallStatusImg.Visibility = Visibility.Visible;
                        CanGoNext = false;
                        btnRerun.Visibility = Visibility.Visible;
                    }
                    sc.Dispose();
                }
                catch (InvalidOperationException)
                {
                    PreInstallMsg.Text = "FAILURE : Cannot stop server instance";
                    preInstallStatusImg.Source =
                        new BitmapImage(new Uri("pack://application:,,,/Resourcefiles/cross.png",
                                                UriKind.RelativeOrAbsolute));
                    preInstallStatusImg.Visibility = Visibility.Visible;
                    CanGoNext = false;
                    btnRerun.Visibility = Visibility.Visible;
                }
                catch (Exception)
                {
                    // service is not present ;)
                    btnRerun.Visibility = Visibility.Collapsed;
                    PreInstallMsg.Text = "SUCCESS : The pre-install process has finished";
                    preInstallStatusImg.Visibility = Visibility.Visible;
                }

        }
    }
}
