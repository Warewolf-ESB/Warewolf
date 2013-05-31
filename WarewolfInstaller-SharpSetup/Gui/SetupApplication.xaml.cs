using System;
using System.Windows;
using SharpSetup.Base;

namespace Gui
{
    /// <summary>
    /// Interaction logic for SetupApplication.xaml
    /// </summary>
    public partial class SetupApplication
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            SetupHelper.Initialize(e.Args);
            SetupHelper.Install += SetupHelper_Install;
            //SetupHelper.SilentInstall += new EventHandler<EventArgs>(SetupHelper_SilentInstall);
            SetupHelper.StartInstallation();
        }

        /*
        void SetupHelper_SilentInstall(object sender, EventArgs e)
        {
            Shutdown();
        }
        */

        void SetupHelper_Install(object sender, EventArgs e)
        {
            MainWindow = new SetupWizard();
            MainWindow.Show();
        }
    }
}
