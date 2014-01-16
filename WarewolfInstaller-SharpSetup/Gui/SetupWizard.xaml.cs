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
                    AddStep(new InstallationStep(InstallationMode.Install, 1, 2));
                    AddStep(new FinishStep(2, 2));
                }
                var modes = GetInstallationModes(MsiConnection.Instance.Mode);
                if(modes.Contains(InstallationMode.Downgrade))
                    AddStep(new FatalErrorStep(Properties.Resources.DowngradeNotSupported));
                else if(modes.Count > 1)
                    AddStep(new InstallationModeStep(modes));
                else if(modes.Count == 1)
                    AddStep(new WelcomeStep(modes[0]));
            }
            else if(type == LifecycleActionType.ModeSelected)
            {

                // Set install mode variable ;)
                InstallVariables.IsInstallMode = true;

                switch((InstallationMode)argument)
                {
                    case InstallationMode.Install:

                        AddStep(new LicenseStep(1, 5));
                        AddStep(new PreInstallProcess(2, 5));
                        AddStep(new InstallationStep(InstallationMode.Install, 3, 5));
                        AddStep(new PostInstallProcess(4, 5));
                        AddStep(new FinishStep(5, 5));
                        break;
                    case InstallationMode.Uninstall:
                        // Set install mode variable ;)
                        InstallVariables.IsInstallMode = false;

                        AddStep(new PreUnInstallProcess(1, 3));
                        AddStep(new InstallationStep(InstallationMode.Uninstall, 2, 3));
                        AddStep(new FinishStep(3, 3));
                        break;
                    case InstallationMode.Upgrade:
                        AddStep(new LicenseStep(1, 6));
                        AddStep(new PreUnInstallProcess(2, 6));
                        AddStep(new InstallationStep(InstallationMode.Uninstall, 3, 6));
                        AddStep(new InstallationStep(InstallationMode.Install, 4, 6));
                        AddStep(new PostInstallProcess(5, 6));
                        AddStep(new FinishStep(6, 6));
                        break;
                    case InstallationMode.Reinstall:
                        AddStep(new LicenseStep(1, 5));
                        AddStep(new PreInstallProcess(2, 5));
                        AddStep(new InstallationStep(InstallationMode.Install, 3, 5));
                        AddStep(new PostInstallProcess(4, 5));
                        AddStep(new FinishStep(5, 5));
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
