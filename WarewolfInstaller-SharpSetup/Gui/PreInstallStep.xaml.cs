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
    /// Interaction logic for PreInstallStep.xaml
    /// </summary>
    public partial class PreInstallStep : ModernActionStep
    {
        public PreInstallStep()
        {
            InitializeComponent();
            btnRerun.Click += new RoutedEventHandler((o, e) => { plMain.Start(); });
            DependencyPropertyDescriptor.FromProperty(PrerequisiteList.StatusProperty, typeof(PrerequisiteList)).AddValueChanged(plMain, new EventHandler(statusChanged));
        }

        private void statusChanged(object sender, EventArgs e)
        {
            CanGoNext = new[] { PrerequisiteCheckStatus.Ok, PrerequisiteCheckStatus.Warning }.Contains(plMain.Status);
            lblReviewWarnings.Visibility = plMain.Status == PrerequisiteCheckStatus.Warning ? Visibility.Visible : Visibility.Collapsed;
            lblReviewErrors.Visibility = plMain.Status == PrerequisiteCheckStatus.Error ? Visibility.Visible : Visibility.Collapsed;
        }

        private void PreInstallStep_Entered(object sender, ChangeStepRoutedEventArgs e)
        {
            if (plMain.Status == PrerequisiteCheckStatus.Unknown)
                plMain.Start();
        }

        private void plMain_Check(object sender, PrerequisiteCheckEventArgs e)
        {
            if (e.Id == "svrStop")
            {
                // flag as in progress
                e.Status = PrerequisiteCheckStatus.InProgress;

                try
                {
                    ServiceController sc = new ServiceController("Warewolf Server");

                    if (sc.Status == ServiceControllerStatus.Running)
                    {
                        sc.Stop();

                        if (sc.Status == ServiceControllerStatus.Stopped)
                        {
                            e.Status = PrerequisiteCheckStatus.Ok;
                            e.Message = "Server instances stopped";

                        }
                        else
                        {
                            e.Status = PrerequisiteCheckStatus.Error;
                            e.Message = "Failed to stop server instances";
                        }
                    }
                    else if (sc.Status == ServiceControllerStatus.Stopped)
                    {
                        e.Status = PrerequisiteCheckStatus.Ok;
                        e.Message = "All instances are already stopped";
                        btnRerun.IsEnabled = false;
                    }
                }
                catch (Exception e1)
                {
                    // service is not present ;)
                    e.Status = PrerequisiteCheckStatus.Ok;
                    e.Message = "No server instances found";
                    btnRerun.IsEnabled = false;
                }

                //put your custom test logic here

            }
            /*else if (e.Id == "prereqmodules")
            {
                //put your custom test logic here
                e.Status = PrerequisiteCheckStatus.Warning;
                e.Message = "Checks from .NET prerequisite modules are not executed when running GUI without bootstrapper.";
            }*/
        }
    }
}
