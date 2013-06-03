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
            SetupHelper.StartInstallation();    
            //if (slientMode)
            //{
            //    SetupHelper.SilentInstall += new EventHandler<EventArgs>(SetupHelper_SilentInstall);
            //}
            //else
            //{
               
            //}
        }

        /// <summary>
        /// Handles the SilentInstall event of the SetupHelper control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void SetupHelper_SilentInstall(object sender, EventArgs e)
        {
            // TODO : Something logical ;)
            Shutdown();
        }


        /// <summary>
        /// Handles the Install event of the SetupHelper control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void SetupHelper_Install(object sender, EventArgs e)
        {
            MainWindow = new SetupWizard();
            MainWindow.Show();
        }
    }
}
