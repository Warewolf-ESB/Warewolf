using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using SharpSetup.Base;

namespace Gui
{
    /// <summary>
    /// Interaction logic for SetupApplication.xaml
    /// </summary>
    public partial class SetupApplication : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            SetupHelper.Initialize(e.Args);
            SetupHelper.Install += new EventHandler<EventArgs>(SetupHelper_Install);
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
            MainWindow = new Gui.SetupWizard();
            MainWindow.Show();
        }
    }
}
