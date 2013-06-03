using System;
using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Gui
{
    /// <summary>
    /// Interaction logic for PreInstallProcess.xaml
    /// </summary>
    public partial class PreUnInstallProcess
    {

        private bool _serviceRemoved;

        public PreUnInstallProcess()
        {
            InitializeComponent();
        }

        private void RemoveService()
        {

            _serviceRemoved = false;

            try
            {
                ServiceInstaller installer = new ServiceInstaller();
                InstallContext context = new InstallContext("<<log file path>>", null);
                installer.Context = context; 
                installer.ServiceName = InstallVariables.ServerService;
                // ReSharper disable AssignNullToNotNullAttribute
                installer.Uninstall(null);
                // ReSharper restore AssignNullToNotNullAttribute

                _serviceRemoved = true;
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch (Exception)
            // ReSharper restore EmptyGeneralCatchClause
            {
                // just being safe ;)
            }
        }

        private void SetSuccessMessasge()
        {
            PreUnInstallMsg.Text = "Server instance removed";
            preUnInstallStatusImg.Visibility = Visibility.Visible;
            btnRerun.Visibility = Visibility.Collapsed;
        }

        private void SetFailureMessage()
        {
            PreUnInstallMsg.Text = "Cannot remove server instance";
            preUnInstallStatusImg.Source =
                new BitmapImage(new Uri("pack://application:,,,/Resourcefiles/cross.png",
                                        UriKind.RelativeOrAbsolute));
            preUnInstallStatusImg.Visibility = Visibility.Visible;
            CanGoNext = false;
            btnRerun.Visibility = Visibility.Visible;
        }

        private void BtnRerun_OnClick(object sender, RoutedEventArgs e)
        {
            PreUnInstallStep_Entered(sender, null);
        }
       


        private void PreUnInstallStep_Entered(object sender, SharpSetup.UI.Wpf.Base.ChangeStepRoutedEventArgs e)
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

                        btnRerun.Visibility = Visibility.Hidden;
                        preUnInstallStatusImg.Visibility = Visibility.Hidden;

                        // Get the BackgroundWorker that raised this event.
                        BackgroundWorker worker = new BackgroundWorker();
                        worker.DoWork += delegate {
                            
                            RemoveService();
                        };

                        worker.RunWorkerCompleted += delegate
                        {
                            if (_serviceRemoved)
                            {
                                SetSuccessMessasge();
                            }
                            else
                            {
                                SetFailureMessage();
                            }
                        };

                        worker.RunWorkerAsync();                           
                    }
                    else
                    {
                        SetFailureMessage();
                    }
                }
                else if (sc.Status == ServiceControllerStatus.Stopped)
                {

                    // Get the BackgroundWorker that raised this event.
                    BackgroundWorker worker = new BackgroundWorker();
                    
                    btnRerun.Visibility = Visibility.Hidden;
                    preUnInstallStatusImg.Visibility = Visibility.Hidden;

                    worker.DoWork += delegate {
                        
                        RemoveService();
                    };

                    worker.RunWorkerCompleted += delegate
                    {
                        if (_serviceRemoved)
                        {
                            SetSuccessMessasge();
                        }
                        else
                        {
                            SetFailureMessage();
                        }
                    };

                    worker.RunWorkerAsync();
                }
                else
                {
                    SetFailureMessage();
                }
                sc.Dispose();
            }
            catch (InvalidOperationException ioe)
            {
                // magic string stating that service is not present ;)
                if (ioe.Message.IndexOf(InstallVariables.ServerService + " was not found on computer",StringComparison.Ordinal) > 0)
                {
                    PreUnInstallMsg.Text = "Scan for server services complete";
                    preUnInstallStatusImg.Visibility = Visibility.Visible;
                    btnRerun.Visibility = Visibility.Collapsed;
                }
                else
                {
                    SetFailureMessage();
                }   
            }
            catch (Exception)
            {
                SetFailureMessage();
            }
        }
    }
}
