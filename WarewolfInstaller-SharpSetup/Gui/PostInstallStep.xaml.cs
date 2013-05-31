using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using System.Windows;
using SharpSetup.Base;
using SharpSetup.UI.Wpf.Base;
using SharpSetup.UI.Wpf.Controls;
using SharpSetup.UI.Wpf.Forms.Modern;

namespace Gui
{
    /// <summary>
    /// Interaction logic for PostInstallStep.xaml
    /// </summary>
    public partial class PostInstallStep : ModernActionStep
    {
        public PostInstallStep()
        {
            InitializeComponent();
            btnRerun.Click += new RoutedEventHandler((o, e) => plPost.Start());
            DependencyPropertyDescriptor.FromProperty(PrerequisiteList.StatusProperty, typeof(PrerequisiteList)).AddValueChanged(plPost, new EventHandler(statusChanged));
        }

        private void statusChanged(object sender, EventArgs e)
        {
            CanGoNext = new[] { PrerequisiteCheckStatus.Ok, PrerequisiteCheckStatus.Warning }.Contains(plPost.Status);
            lblReviewWarnings.Visibility = plPost.Status == PrerequisiteCheckStatus.Warning ? Visibility.Visible : Visibility.Collapsed;
            lblReviewErrors.Visibility = plPost.Status == PrerequisiteCheckStatus.Error ? Visibility.Visible : Visibility.Collapsed;
        }

        private void PostInstallStep_Entered(object sender, ChangeStepRoutedEventArgs e)
        {
            if (plPost.Status == PrerequisiteCheckStatus.Unknown)
                plPost.Start();
        }

        private void plPost_Check(object sender, PrerequisiteCheckEventArgs e)
        {
            ServiceController sc = new ServiceController(InstallVariables.ServerService);

            if (e.Id == "svrService")
            {

                e.Status = PrerequisiteCheckStatus.InProgress;
                // attempts to install service ;)

                var installRoot = InstallVariables.InstallRoot;
                if (!string.IsNullOrEmpty(installRoot))
                {
                    // Gain access to warewolf exe ;)

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
                        while(!p.HasExited && cnt < 10)
                        {
                            cnt++;
                            Thread.Sleep(1000);
                        }

                        e.Status = PrerequisiteCheckStatus.Ok;
                        e.Message = "Installed server as a service";   
                    }
                    catch (Exception e1)
                    {
                        e.Status = PrerequisiteCheckStatus.Error;
                        e.Message = "Failed to install server as a service";   
                    }

                }
                else
                {
                    e.Status = PrerequisiteCheckStatus.Error;
                    e.Message = "Installer cannot resolve server install location";   
                }

                
            }else if (e.Id == "svrStart")
            {
                 // now try and start the service ;)
                e.Status = PrerequisiteCheckStatus.InProgress;
                try
                {

                    if (sc.Status == ServiceControllerStatus.Stopped)
                    {
                        sc.Start();

                        if (sc.Status == ServiceControllerStatus.Running)
                        {
                            e.Status = PrerequisiteCheckStatus.Ok;
                            e.Message = "Server Service Started";
                            btnRerun.IsEnabled = false;
                        }
                        else
                        {
                            e.Status = PrerequisiteCheckStatus.Error;
                            e.Message = "Server Service Is " + sc.Status;
                        }
                    }
                }
                catch (Exception e1)
                {
                    
                }
            }
            
        }
    }
}
