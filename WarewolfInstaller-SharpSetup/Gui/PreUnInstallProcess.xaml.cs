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
        public PreUnInstallProcess()
        {
            InitializeComponent();
        }

        private bool RemoveService()
        {

            bool result = false;

            try
            {
                ServiceInstaller installer = new ServiceInstaller();
                InstallContext context = new InstallContext("<<log file path>>", null);
                installer.Context = context; 
                installer.ServiceName = InstallVariables.ServerService;
                // ReSharper disable AssignNullToNotNullAttribute
                installer.Uninstall(null);
                // ReSharper restore AssignNullToNotNullAttribute

                result = true;
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch (Exception)
            // ReSharper restore EmptyGeneralCatchClause
            {
                // just being safe ;)
            }

            return result;
        }

        public void PreUnInstallStep_Repeat(object sender, MouseButtonEventArgs mouseButtonEventArgs)
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

                        // Get the BackgroundWorker that raised this event.
                        BackgroundWorker worker = new BackgroundWorker();
                        worker.DoWork += delegate(object o, DoWorkEventArgs args)
                        {
                            RemoveService();
                        };

                        worker.RunWorkerCompleted += delegate(object o, RunWorkerCompletedEventArgs args)
                        {
                            if ((bool) args.Result)
                            {
                                PreInstallMsg.Text = "Server instance removed";
                                preInstallStatusImg.Visibility = Visibility.Visible;
                                btnRerun.Visibility = Visibility.Collapsed;
                            }
                            else
                            {
                                PreInstallMsg.Text = "Cannot remove server instance";
                                preInstallStatusImg.Source =
                                    new BitmapImage(new Uri("pack://application:,,,/Resourcefiles/cross.png",
                                                            UriKind.RelativeOrAbsolute));
                                preInstallStatusImg.Visibility = Visibility.Visible;
                                CanGoNext = false;
                                btnRerun.Visibility = Visibility.Visible;
                            }
                        };

                        worker.RunWorkerAsync();                           
                    }
                    else
                    {
                        PreInstallMsg.Text = "Cannot remove server instance";
                        preInstallStatusImg.Source =
                            new BitmapImage(new Uri("pack://application:,,,/Resourcefiles/cross.png",
                                                    UriKind.RelativeOrAbsolute));
                        preInstallStatusImg.Visibility = Visibility.Visible;
                        CanGoNext = false;
                        btnRerun.Visibility = Visibility.Visible;
                    }
                }
                else if (sc.Status == ServiceControllerStatus.Stopped)
                {

                    // Get the BackgroundWorker that raised this event.
                    BackgroundWorker worker = new BackgroundWorker();
                    worker.DoWork += delegate(object o, DoWorkEventArgs args)
                    {
                        RemoveService();
                    };

                    worker.RunWorkerCompleted += delegate(object o, RunWorkerCompletedEventArgs args)
                    {
                        if ((bool)args.Result)
                        {
                            PreInstallMsg.Text = "Server instance removed";
                            preInstallStatusImg.Visibility = Visibility.Visible;
                            btnRerun.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            PreInstallMsg.Text = "Cannot remove server instance";
                            preInstallStatusImg.Source =
                                new BitmapImage(new Uri("pack://application:,,,/Resourcefiles/cross.png",
                                                        UriKind.RelativeOrAbsolute));
                            preInstallStatusImg.Visibility = Visibility.Visible;
                            CanGoNext = false;
                            btnRerun.Visibility = Visibility.Visible;
                        }
                    };

                    worker.RunWorkerAsync();
                }
                else
                {
                    PreInstallMsg.Text = "Cannot remove server instance";
                    preInstallStatusImg.Source =
                        new BitmapImage(new Uri("pack://application:,,,/Resourcefiles/cross.png",
                                                UriKind.RelativeOrAbsolute));
                    preInstallStatusImg.Visibility = Visibility.Visible;
                    CanGoNext = false;
                    btnRerun.Visibility = Visibility.Visible;
                }
                sc.Dispose();
            }
            catch (InvalidOperationException ioe)
            {
                // magic string stating that service is not present ;)
                if (ioe.Message.IndexOf(InstallVariables.ServerService + " was not found on computer",StringComparison.Ordinal) > 0)
                {
                    PreInstallMsg.Text = "Scan for server services complete";
                    preInstallStatusImg.Visibility = Visibility.Visible;
                    btnRerun.Visibility = Visibility.Collapsed;
                }
                else
                {
                    PreInstallMsg.Text = "Cannot remove server instance";
                    preInstallStatusImg.Source =
                        new BitmapImage(new Uri("pack://application:,,,/Resourcefiles/cross.png",
                                                UriKind.RelativeOrAbsolute));
                    preInstallStatusImg.Visibility = Visibility.Visible;
                    CanGoNext = false;
                    btnRerun.Visibility = Visibility.Visible;
                }   
            }
            catch (Exception)
            {
                PreInstallMsg.Text = "Cannot remove server instance";
                preInstallStatusImg.Source =
                    new BitmapImage(new Uri("pack://application:,,,/Resourcefiles/cross.png",
                                            UriKind.RelativeOrAbsolute));
                preInstallStatusImg.Visibility = Visibility.Visible;
                CanGoNext = false;
                btnRerun.Visibility = Visibility.Visible;
            }
        }
    }
}
