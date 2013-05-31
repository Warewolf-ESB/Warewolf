using System;
using System.ComponentModel;
using System.Linq;
using System.ServiceProcess;
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
            btnRerun.Click += new RoutedEventHandler((o, e) => { plPost.Start(); });
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
            ServiceController sc = new ServiceController("Warewolf Server");

            if (e.Id == "svrService")
            {

                // attempts to install service ;)

                // TODO : Gain access to warewolf exe ;)

                //put your custom test logic here
                //e.Status = PrerequisiteCheckStatus.Ok;
                //e.Message = "tst1 passed";
            }else if (e.Id == "svrStart")
            {
                 // now try and start the service ;)
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
            }
            
        }
    }
}
