using System;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Path = System.IO.Path;

namespace Gui
{
    /// <summary>
    /// Interaction logic for PreInstallProcess.xaml
    /// </summary>
    public partial class PostInstallProcess
    {
        public PostInstallProcess()
        {
            InitializeComponent();
        }

        public void PostInstallStep_Repeat(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            PostInstallStep_Entered(sender, null);
        }

        private void PostInstallStep_Entered(object sender, SharpSetup.UI.Wpf.Base.ChangeStepRoutedEventArgs e)
        {


            CanGoNext = false;
            postInstallStatusImg.Visibility = Visibility.Hidden;
            // attempts to install service ;)
            ServiceController sc = new ServiceController(InstallVariables.ServerService);

            var installRoot = InstallVariables.InstallRoot;
            if (!string.IsNullOrEmpty(installRoot))
            {
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

                    int cnt = 0;
                    while (!p.HasExited && cnt < 10)
                    {
                        cnt++;
                        Thread.Sleep(1000);
                    }

                    // now try and start the service ;)
                    try
                    {

                        if (sc.Status == ServiceControllerStatus.Stopped)
                        {
                            sc.Start();

                            if (sc.Status == ServiceControllerStatus.Running)
                            {
                                PostInstallMsg.Text = "SUCCESS : Started server service";
                                postInstallStatusImg.Visibility = Visibility.Visible;
                                CanGoNext = true;
                                btnRerun.Visibility = Visibility.Hidden;
                            }
                            else
                            {
                                PostInstallMsg.Text = "FAILURE : Cannot start server service";
                                postInstallStatusImg.Source =
                                    new BitmapImage(new Uri("pack://application:,,,/Resourcefiles/cross.png",
                                                            UriKind.RelativeOrAbsolute));
                                postInstallStatusImg.Visibility = Visibility.Visible;
                                CanGoNext = false;
                                btnRerun.Visibility = Visibility.Visible;
                            }
                        }else if (sc.Status == ServiceControllerStatus.Running)
                        {
                            PostInstallMsg.Text = "SUCCESS : Started server service";
                            postInstallStatusImg.Visibility = Visibility.Visible;
                            CanGoNext = true;
                            btnRerun.Visibility = Visibility.Hidden;
                        }
                    }
// ReSharper disable EmptyGeneralCatchClause
                    catch
// ReSharper restore EmptyGeneralCatchClause
                    {
                        // Just here to make things more stable ;)
                    }
                    sc.Dispose();
                }
                catch (Exception)
                {
                    PostInstallMsg.Text = "FAILURE : Cannot install server as service";
                    postInstallStatusImg.Source =
                        new BitmapImage(new Uri("pack://application:,,,/Resourcefiles/cross.png",
                                                UriKind.RelativeOrAbsolute));
                    postInstallStatusImg.Visibility = Visibility.Visible;
                    CanGoNext = false;
                    btnRerun.Visibility = Visibility.Visible;

                }

            }
            else
            {

                PostInstallMsg.Text = "FAILURE : Installer cannot resolve server install location";
                postInstallStatusImg.Source =
                    new BitmapImage(new Uri("pack://application:,,,/Resourcefiles/cross.png",
                                            UriKind.RelativeOrAbsolute));
                postInstallStatusImg.Visibility = Visibility.Visible;
                CanGoNext = false;
                btnRerun.Visibility = Visibility.Visible;
            }

        }
    }
}
