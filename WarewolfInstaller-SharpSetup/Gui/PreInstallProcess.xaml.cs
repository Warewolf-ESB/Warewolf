using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Windows;
using System.Windows.Media.Imaging;
using Gui.Utility;
using Microsoft.Deployment.WindowsInstaller;
using SharpSetup.UI.Wpf.Base;

namespace Gui
{
    /// <summary>
    /// Interaction logic for PreInstallProcess.xaml
    /// </summary>
    public partial class PreInstallProcess
    {
        private bool _vcInstalled;

        public PreInstallProcess(int stepNumber, List<string> listOfStepNames)
        {
            InitializeComponent();
            // enable shortcut install
            InstallVariables.InstallShortcuts = true;
            cbInstallShortcuts.Focus();
            DataContext = new InfoStepDataContext(stepNumber, listOfStepNames);
        }

        /// <summary>
        /// Executes the process.
        /// </summary>
        public void ExecuteProcess()
        {
            PreInstallStep_Entered(null, null);
        }

        /// <summary>
        /// Handles the OnClick event of the BtnRerun control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void BtnRerun_OnClick(object sender, RoutedEventArgs e)
        {
            PreInstallStep_Entered(sender, null);
        }


        /// <summary>
        /// Sets the success message.
        /// </summary>
        /// <param name="msg">The MSG.</param>
        private void SetSuccessMessasgeForDependencies(string msg)
        {
            PreInstallMsgVCPlusPlus.Text = msg;
            preInstallStatusImgVCPlusPlus.Visibility = Visibility.Visible;
            postInstallStatusCircularProgressBarVCPlusPlus.Visibility = Visibility.Collapsed;
            preInstallStatusImgVCPlusPlus.Source =
                new BitmapImage(new Uri("pack://application:,,,/Resourcefiles/Images/tick.png",
                                        UriKind.RelativeOrAbsolute));
            btnRerun.Visibility = Visibility.Collapsed;

        }


        /// <summary>
        /// Sets the success message.
        /// </summary>
        /// <param name="msg">The MSG.</param>
        private void SetSuccessMessasge(string msg)
        {
            PreInstallMsg.Text = msg;
            preInstallStatusImg.Visibility = Visibility.Visible;
            postInstallStatusCircularProgressBar.Visibility = Visibility.Collapsed;
            preInstallStatusImg.Source =
                new BitmapImage(new Uri("pack://application:,,,/Resourcefiles/Images/tick.png",
                                        UriKind.RelativeOrAbsolute));
            btnRerun.Visibility = Visibility.Collapsed;
        }

        private void SetCleanupMessage()
        {
            PreInstallMsg.Text = InstallVariables.RollbackMessage;
            preInstallStatusImg.Visibility = Visibility.Collapsed;
            postInstallStatusCircularProgressBar.Visibility = Visibility.Visible;
            btnRerun.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Sets the failure message.
        /// </summary>
        /// <param name="msg">The MSG.</param>
        private void SetFailureMessageForDependencies(string msg = null)
        {
            PreInstallMsgVCPlusPlus.Text = msg ?? "Cannot install VC++ 2008 SP1 x86";
            preInstallStatusImgVCPlusPlus.Source =
                new BitmapImage(new Uri("pack://application:,,,/Resourcefiles/Images/cross.png",
                                        UriKind.RelativeOrAbsolute));
            preInstallStatusImgVCPlusPlus.Visibility = Visibility.Visible;
            postInstallStatusCircularProgressBarVCPlusPlus.Visibility = Visibility.Collapsed;
            CanGoNext = false;
            btnRerun.Visibility = Visibility.Visible;
        }


        /// <summary>
        /// Sets the failure message.
        /// </summary>
        private void SetFailureMessage(string msg = null)
        {
            PreInstallMsg.Text = msg ?? "Cannot stop server instance";
            preInstallStatusImg.Source =
                new BitmapImage(new Uri("pack://application:,,,/Resourcefiles/Images/cross.png",
                                        UriKind.RelativeOrAbsolute));
            preInstallStatusImg.Visibility = Visibility.Visible;
            postInstallStatusCircularProgressBar.Visibility = Visibility.Collapsed;
            CanGoNext = false;
            btnRerun.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Shows the cancel error.
        /// </summary>
        private void ShowCancelError()
        {
            MessageBox.Show("Failed to rollback installer progress", "Error");
        }

        /// <summary>
        /// Handles the Entered event of the PreInstallStep control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SharpSetup.UI.Wpf.Base.ChangeStepRoutedEventArgs"/> instance containing the event data.</param>
// ReSharper disable InconsistentNaming
        public void PreInstallStep_Entered(object sender, ChangeStepRoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try
            {
                // disable the next button ;)
                CanGoNext = false;

                CheckForAndInstallDependencies();

                CanGoNext = true;

            }
            catch(Exception e1)
            {
                SetFailureMessageForDependencies(e1.Message);
            }

            StopServerService();
        }


        /// <summary>
        /// Checks for and install vc++ 2k8 sp1.
        /// </summary>
        private void CheckForAndInstallDependencies()
        {

            // Get the BackgroundWorker that raised this event.
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += delegate
            {
                // check registry key for vc++ 2k8 sp1
                if(!IsVCPlusPlus2k8Sp1Installed())
                {
                    InstallVCPlusPlus2k8Sp1();
                }
                else
                {
                    _vcInstalled = true;
                }

            };

            worker.RunWorkerCompleted += delegate
            {
                if(_vcInstalled)
                {
                    // enable the next button
                    CanGoNext = true;
                    SetSuccessMessasgeForDependencies("Dependencies installed");
                }
                else
                {
                    SetFailureMessage();
                }
            };

            worker.RunWorkerAsync();
        }

        /// <summary>
        /// Installs the vc++ 2k8 SP1.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void InstallVCPlusPlus2k8Sp1()
// ReSharper restore InconsistentNaming
        {

            var stream = ResourceExtractor.FetchDependency("vcredist_x86.exe");

            if(stream == null)
            {
                return;
            }

            var fileName = Path.GetTempFileName();
            var streamLength = stream.Length;
            var bytes = new byte[streamLength];

            using(stream)
            {
                stream.Read(bytes, 0, (int)streamLength);
                // TODO : Write stream to TMP directory and run ;)
                File.WriteAllBytes(fileName, bytes);
            }

            // now move so it has an msi name ;)
            var newName = fileName + ".exe";
            File.Move(fileName, newName);

            // Now run the installer in /q mode
            ProcessStartInfo psi = new ProcessStartInfo(newName, "/q");
            var vcProc = Process.Start(psi);

            // wait for up to a minute
            vcProc.WaitForExit(60000);

            // remove tmp file
            File.Delete(newName);

            _vcInstalled = true;

            // check that it installed
            if(vcProc.ExitCode != 0)
            {
                _vcInstalled = false;
            }
        }

        /// <summary>
        /// Determines whether vc++ 2k8 sp1 x86 is installed
        /// </summary>
        /// <returns></returns>
// ReSharper disable InconsistentNaming
        public static bool IsVCPlusPlus2k8Sp1Installed()
// ReSharper restore InconsistentNaming
        {
            ProductInstallation productInstallation = new ProductInstallation(InstallVariables.Vcplusplus2k8sp1x86Key, null, UserContexts.Machine);

            return productInstallation.IsInstalled;
        }

        /// <summary>
        /// Stops the server service.
        /// </summary>
        private void StopServerService()
        {
            // Setup a cancel action ;)
            Cancel += OnCancel;

            // Start Service
            try
            {

                ServiceController sc = new ServiceController(InstallVariables.ServerService);
                if(sc.Status == ServiceControllerStatus.Running)
                {
                    sc.Stop();
                    sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(InstallVariables.DefaultWaitInSeconds)); // wait ;)
                    // The pre-install process has finished.
                    if(sc.Status == ServiceControllerStatus.Stopped)
                    {
                        SetSuccessMessasge("Server instance stopped");
                    }
                    else
                    {
                        SetFailureMessage();
                    }
                }
                else if(sc.Status == ServiceControllerStatus.Stopped)
                {
                    SetSuccessMessasge("Server instance stopped");
                }
                else
                {
                    SetFailureMessage();
                }
                sc.Dispose();
            }
            catch(InvalidOperationException ioe)
            {
                // magic string stating that service is not present ;)
                if(ioe.Message.IndexOf(InstallVariables.ServerService + " was not found on computer", StringComparison.Ordinal) > 0)
                {
                    SetSuccessMessasge("Scan for server service complete");
                }
                else
                {
                    SetFailureMessage();
                }

            }
            catch(Exception)
            {
                // Service not present ;)
                SetSuccessMessasge("Scan for server service complete");
            }

        }

        private void OnCancel(object sender, ChangeStepRoutedEventArgs changeStepRoutedEventArgs)
        {
            SetCleanupMessage();
            try
            {
                ServiceController sc = new ServiceController(InstallVariables.ServerService);

                // Attempt to re-start the service ;)
                if(sc.Status != ServiceControllerStatus.Running)
                {
                    try
                    {
                        sc.Start();
                        // wait ;) 
                        sc.WaitForStatus(ServiceControllerStatus.Running,
                                         TimeSpan.FromSeconds(InstallVariables.DefaultWaitInSeconds));
                    }
                    catch(Exception)
                    {
                        ShowCancelError();
                    }
                }

                try
                {
                    if(sc.Status != ServiceControllerStatus.Running)
                    {
                        ShowCancelError();
                    }
                    else
                    {
                        SetSuccessMessasge("Rollback completed");
                    }
                }
                catch(InvalidOperationException ioe)
                {
                    // magic string stating that service is not present ;)
                    if(ioe.Message.IndexOf(InstallVariables.ServerService + " was not found on computer",
                                            StringComparison.Ordinal) < 0)
                    {
                        ShowCancelError();
                    }
                    else
                    {
                        SetSuccessMessasge("Rollback completed");
                    }

                }
                catch(Exception)
                {
                    // service not present ;)
                    SetSuccessMessasge("Rollback completed");
                }

                sc.Dispose();
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch(Exception)
            // ReSharper restore EmptyGeneralCatchClause
            {
                //MessageBox.Show(e.Message);
                // Avoid showing message here, cancel is just cancel ;)
            }

        }

        void BtnInstallShortcuts(object sender, RoutedEventArgs e)
        {
            InstallVariables.InstallShortcuts = !(InstallVariables.InstallShortcuts);
        }
    }
}
