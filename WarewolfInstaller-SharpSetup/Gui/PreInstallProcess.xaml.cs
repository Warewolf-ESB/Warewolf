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
    public partial class PreInstallProcess : ModernInfoStep
    {
        public PreInstallProcess()
        {
            InitializeComponent();
        }


        private void PreInstallStep_Entered(object sender, SharpSetup.UI.Wpf.Base.ChangeStepRoutedEventArgs e)
        {

                try
                {
                    ServiceController sc = new ServiceController("Warewolf Server");

                    if (sc.Status == ServiceControllerStatus.Running)
                    {
                        sc.Stop();
                        // The pre-install process has finished.
                        // ImageSource="pack://application:,,,/Resources/RibbonImages/CloseButton.png"
                        if (sc.Status == ServiceControllerStatus.Stopped)
                        {
                            PreInstallMsg.Text = "SUCCESS: Server instance stopped";
                            btnRerun.IsEnabled = false;
                        }
                        else
                        {
                           PreInstallMsg.Text = "FAILURE : Cannot stop server instance";
                            // TODO : Disable moving forward
                            CanGoNext = false;
                            btnRerun.IsEnabled = true;
                        }
                    }
                    else if (sc.Status == ServiceControllerStatus.Stopped)
                    {
                        PreInstallMsg.Text = "SUCCESS: Server instance stopped";
                        btnRerun.IsEnabled = false;
                    }
                }
                catch (Exception e1)
                {
                    // service is not present ;)
                    btnRerun.IsEnabled = false;
                    PreInstallMsg.Text = "SUCCESS : The pre-install process has finished";
                }

        }
    }
}
