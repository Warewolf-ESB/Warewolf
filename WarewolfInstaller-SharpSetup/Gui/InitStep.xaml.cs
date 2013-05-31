using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using SharpSetup.Base;
using SharpSetup.UI.Wpf.Base;
using SharpSetup.UI.Wpf.Controls;
using SharpSetup.UI.Wpf.Forms.Modern;

namespace Gui
{

    /// <summary>
    /// Interaction logic for InitStep.xaml
    /// </summary>
    public partial class InitStep : ModernActionStep
    {
        public InitStep()
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

        private void InitStep_Entered(object sender, ChangeStepRoutedEventArgs e)
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

                // Check for the service ;)
                ProcessStartInfo psi0 = new ProcessStartInfo();

                psi0.FileName = "sc.exe";
                psi0.Arguments = "query \"Warewolf Server\"";

                Process qProc = Process.Start(psi0);

                qProc.Exited += delegate(object o1, EventArgs args1)
                {
                    // all good, it is installed ;)
                    if (qProc.ExitCode == 0)
                    {
                        ProcessStartInfo psi = new ProcessStartInfo();

                        psi.FileName = "sc.exe";
                        psi.Arguments = " stop \"Warewolf Server\"";

                        Process svr = Process.Start(psi);

                        svr.ErrorDataReceived += delegate(object o, DataReceivedEventArgs args)
                        {
                            e.Status = PrerequisiteCheckStatus.Error;
                            e.Message = args.Data;
                        };

                        svr.Exited += delegate(object o, EventArgs args)
                        {
                            // all good it was successful ;)
                            if (svr.ExitCode == 0)
                            {
                                e.Status = PrerequisiteCheckStatus.Ok;
                                e.Message = "Server instances stopped";
                            }
                            else
                            {
                                e.Status = PrerequisiteCheckStatus.Error;
                                e.Message = "Failed to stop server instances";
                            }
                        };
                    }
                    else
                    {
                        // service is not present ;)
                        e.Status = PrerequisiteCheckStatus.Ok;
                        e.Message = "No server instances found";
                    }
                };

                


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
