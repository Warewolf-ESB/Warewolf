using System.IO;
using System.Windows;
using SharpSetup.Base;
using SharpSetup.Prerequisites.Base;

namespace Gui
{
    /// <summary>
    /// Interaction logic for InstallationStep.xaml
    /// </summary>
    public partial class InstallationStep
    {
        InstallationMode mode;
        public InstallationStep(InstallationMode mode)
        {
            InitializeComponent();
            this.mode = mode;
        }

        private void InstallationStep_Entered(object sender, SharpSetup.UI.Wpf.Base.ChangeStepRoutedEventArgs e)
        {
            try
            {
                if (mode == InstallationMode.Uninstall)
                {
                    MsiConnection.Instance.Uninstall();
                    
                    if (File.Exists(Properties.Resources.MainMsiFile))
                        MsiConnection.Instance.Open(Properties.Resources.MainMsiFile, true);
                }
                else if (mode == InstallationMode.Install)
                {
                    PrerequisiteManager.Instance.Install();

                    try
                    {
                        InstallVariables.InstallRoot = MsiConnection.Instance.GetProperty("INSTALLLOCATION");
                    }
                    catch
                    {
                        // Best effort to fetch product code, if not present we have big issues ;(
                        MessageBox.Show("Cannot locate product code to continue install.");
                        Wizard.Finish();
                    }

                    MsiConnection.Instance.Install();

                }
                else
                {
                    MessageBox.Show("Unknown mode");
                }
            }
            catch (MsiException mex)
            {
                if (mex.ErrorCode != (uint)InstallError.UserExit)
                    MessageBox.Show("Installation failed: " + mex.Message);
                Wizard.Finish();
            }

            Wizard.NextStep();
        }
    }
}
