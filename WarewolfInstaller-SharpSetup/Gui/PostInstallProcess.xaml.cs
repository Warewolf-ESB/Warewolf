using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SharpSetup.Base;
using SharpSetup.UI.Wpf.Forms.Modern;
using Path = System.IO.Path;

namespace Gui
{
    /// <summary>
    /// Interaction logic for PreInstallProcess.xaml
    /// </summary>
    public partial class PostInstallProcess : ModernInfoStep
    {
        public PostInstallProcess()
        {
            InitializeComponent();
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
                                PostInstallMsg.Text = "SUCCESS : Installed and started server service";
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
                            PostInstallMsg.Text = "SUCCESS : Installed and started server service";
                            postInstallStatusImg.Visibility = Visibility.Visible;
                            CanGoNext = true;
                            btnRerun.Visibility = Visibility.Hidden;
                        }
                    }
                    catch (Exception e1)
                    {
                        // Just here to make things more stable ;)
                    }
                }
                catch (Exception e1)
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

                //try
                //{
                //    ServiceController sc = new ServiceController("Warewolf Server");

                //    if (sc.Status == ServiceControllerStatus.Running)
                //    {
                //        sc.Stop();
                //        // The pre-install process has finished.
                //        if (sc.Status == ServiceControllerStatus.Stopped)
                //        {
                //            PreInstallMsg.Text = "SUCCESS: Server instance stopped";
                //            preInstallStatusImg.Visibility = Visibility.Visible;
                //            btnRerun.Visibility = Visibility.Collapsed;
                //        }
                //        else
                //        {
                //            PreInstallMsg.Text = "FAILURE : Cannot stop server instance";
                //            preInstallStatusImg.Source =
                //                new BitmapImage(new Uri("pack://application:,,,/Resourcefiles/cross.png",
                //                                        UriKind.RelativeOrAbsolute));
                //            preInstallStatusImg.Visibility = Visibility.Visible;
                //            CanGoNext = false;
                //            btnRerun.Visibility = Visibility.Visible;
                //        }
                //    }
                //    else if (sc.Status == ServiceControllerStatus.Stopped)
                //    {
                //        PreInstallMsg.Text = "SUCCESS: Server instance stopped";
                //        preInstallStatusImg.Visibility = Visibility.Visible;
                //        btnRerun.Visibility = Visibility.Collapsed;
                //    }
                //    else
                //    {
                //        PreInstallMsg.Text = "FAILURE : Cannot stop server instance";
                //        preInstallStatusImg.Source =
                //            new BitmapImage(new Uri("pack://application:,,,/Resourcefiles/cross.png",
                //                                    UriKind.RelativeOrAbsolute));
                //        preInstallStatusImg.Visibility = Visibility.Visible;
                //        CanGoNext = false;
                //        btnRerun.Visibility = Visibility.Visible;
                //    }
                //}
                //catch (InvalidOperationException ioe)
                //{
                //    PreInstallMsg.Text = "FAILURE : Cannot stop server instance";
                //    preInstallStatusImg.Source =
                //        new BitmapImage(new Uri("pack://application:,,,/Resourcefiles/cross.png",
                //                                UriKind.RelativeOrAbsolute));
                //    preInstallStatusImg.Visibility = Visibility.Visible;
                //    CanGoNext = false;
                //    btnRerun.Visibility = Visibility.Visible;
                //}
                //catch (Exception e1)
                //{
                //    // service is not present ;)
                //    btnRerun.Visibility = Visibility.Collapsed;
                //    PreInstallMsg.Text = "SUCCESS : The pre-install process has finished";
                //    preInstallStatusImg.Visibility = Visibility.Visible;
                //}

        }
    }
}
