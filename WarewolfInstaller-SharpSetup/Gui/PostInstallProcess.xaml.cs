using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
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
