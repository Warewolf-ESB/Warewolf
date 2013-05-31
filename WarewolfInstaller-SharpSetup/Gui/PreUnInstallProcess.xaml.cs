using System;
using System.ServiceProcess;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
using SharpSetup.UI.Wpf.Forms.Modern;

namespace Gui
{
    /// <summary>
    /// Interaction logic for PreInstallProcess.xaml
    /// </summary>
    public partial class PreUnInstallProcess : ModernInfoStep
    {
        public PreUnInstallProcess()
        {
            InitializeComponent();
        }

        private bool RemoveService()
        {
            bool result = false;

            try
            {
                ServiceInstaller installer = new ServiceInstaller();

                installer.ServiceName = InstallVariables.ServerService;
                installer.Uninstall(null);

                result = true;
            }
            catch (Exception e)
            {
                // just being safe ;)
            }

            return result;
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
                        // The pre-uninstall process has finished.
                        if (sc.Status == ServiceControllerStatus.Stopped)
                        {
                            // cool beans remove the service ;)
                            if (RemoveService())
                            {
                                PreInstallMsg.Text = "SUCCESS: Server instance stopped amd removed";
                                preInstallStatusImg.Visibility = Visibility.Visible;
                                btnRerun.Visibility = Visibility.Collapsed;    
                            }                            
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
                        // cool beans remove the service ;)
                        if (RemoveService())
                        {
                            PreInstallMsg.Text = "SUCCESS: Server instance stopped and removed";
                            preInstallStatusImg.Visibility = Visibility.Visible;
                            btnRerun.Visibility = Visibility.Collapsed;
                        }
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
                catch (InvalidOperationException ioe)
                {
                    PreInstallMsg.Text = "FAILURE : Cannot stop server instance";
                    preInstallStatusImg.Source =
                        new BitmapImage(new Uri("pack://application:,,,/Resourcefiles/cross.png",
                                                UriKind.RelativeOrAbsolute));
                    preInstallStatusImg.Visibility = Visibility.Visible;
                    CanGoNext = false;
                    btnRerun.Visibility = Visibility.Visible;
                }
                catch (Exception e1)
                {
                    // service is not present ;)
                    btnRerun.Visibility = Visibility.Collapsed;
                    PreInstallMsg.Text = "SUCCESS : The pre-install process has finished";
                    preInstallStatusImg.Visibility = Visibility.Visible;
                }

        }
    }
}
