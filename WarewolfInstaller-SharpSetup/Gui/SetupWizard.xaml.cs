using System.Collections.Generic;
using System.Windows;
using SharpSetup.Base;

namespace Gui
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class SetupWizard
    {
        public SetupWizard()
        {
            InitializeComponent();
        }

        InstallationModeCollection GetInstallationModes(MsiInstallationModes mode)
        {
            var modes = SetupHelper.GetStandardInstallationModes(mode);
            //uncomment this line if you want to support modification mode if re installation is possible
            modes.InsertBefore(InstallationMode.Reinstall, InstallationMode.Modify);
            modes.Remove(InstallationMode.Modify);
            //uncomment this line if you don't want to support re installation
            //modes.Remove(InstallationMode.Reinstall);
            if(modes.Contains(SetupHelper.InstallationModeFromCommandLine))
            {
                return new InstallationModeCollection(SetupHelper.InstallationModeFromCommandLine);
            }

            return modes;
        }

        public override void LifecycleAction(string type, object argument)
        {
            List<string> listOfStepNames;
            listOfStepNames = new List<string> { "Welcome", "Privacy Statement", "License Agreement", "Pre Install", "Installation", "Post Install", "Finish" };
            if(type == LifecycleActionType.Initialization)
            {
                AddStep(new InitializationStep());
            }
            else if(type == LifecycleActionType.ConnectionOpened)
            {
                //uncomment this line if you want to support MSI property pass through
                SetupHelper.ApplyMsiProperties();
                if(MsiConnection.Instance.IsRestored)
                {
                    listOfStepNames = new List<string> { "License Agreement", "Pre Install" };
                    AddStep(new InstallationStep(InstallationMode.Install, 1, listOfStepNames));
                    AddStep(new FinishStep(2, listOfStepNames));
                }
                var modes = GetInstallationModes(MsiConnection.Instance.Mode);
                if(modes.Contains(InstallationMode.Downgrade))
                    AddStep(new FatalErrorStep(Properties.Resources.DowngradeNotSupported));
                else if(modes.Count > 1)
                    AddStep(new InstallationModeStep(modes));
                else if(modes.Count == 1)
                    AddStep(new WelcomeStep(modes[0], 1, listOfStepNames));
            }
            else if(type == LifecycleActionType.ModeSelected)
            {

                // Set install mode variable ;)
                InstallVariables.IsInstallMode = true;
                switch((InstallationMode)argument)
                {
                    case InstallationMode.Install:
                        AddStep(new PrivacyStatmentStep(2, listOfStepNames));
                        AddStep(new LicenseStep(3, listOfStepNames));
                        AddStep(new PreInstallProcess(4, listOfStepNames));
                        AddStep(new InstallationStep(InstallationMode.Install, 5, listOfStepNames));
                        AddStep(new PostInstallProcess(6, listOfStepNames));
                        AddStep(new FinishStep(7, listOfStepNames));
                        break;
                    case InstallationMode.Uninstall:
                        // Set install mode variable ;)
                        InstallVariables.IsInstallMode = false;
                        listOfStepNames = new List<string> { "Pre Uninstall", "Uninstall", "Finish" };
                        AddStep(new PreUnInstallProcess(1, listOfStepNames));
                        AddStep(new InstallationStep(InstallationMode.Uninstall, 2, listOfStepNames));
                        AddStep(new FinishStep(3, listOfStepNames));
                        break;
                    case InstallationMode.Upgrade:
                        listOfStepNames = new List<string> { "Privacy Statement", "License Agreement", "Pre UnInstall", "Uninstall", "Installation", "Post Install", "Finish" };
                        AddStep(new PrivacyStatmentStep(1, listOfStepNames));
                        AddStep(new LicenseStep(2, listOfStepNames));
                        AddStep(new PreUnInstallProcess(3, listOfStepNames));
                        AddStep(new InstallationStep(InstallationMode.Uninstall, 4, listOfStepNames));
                        AddStep(new InstallationStep(InstallationMode.Install, 5, listOfStepNames));
                        AddStep(new PostInstallProcess(6, listOfStepNames));
                        AddStep(new FinishStep(7, listOfStepNames));
                        break;
                    case InstallationMode.Reinstall:
                        AddStep(new PrivacyStatmentStep(2, listOfStepNames));
                        AddStep(new LicenseStep(3, listOfStepNames));
                        AddStep(new PreInstallProcess(4, listOfStepNames));
                        AddStep(new InstallationStep(InstallationMode.Install, 5, listOfStepNames));
                        AddStep(new PostInstallProcess(6, listOfStepNames));
                        AddStep(new FinishStep(7, listOfStepNames));
                        break;
                    default:
                        MessageBox.Show("Mode not supported: " + (InstallationMode)argument);
                        break;
                }
            }
            else
            {
                MessageBox.Show("Unsupported life-cycle action");
            }
            base.LifecycleAction(type, argument);
        }
    }
}
