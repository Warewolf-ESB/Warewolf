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
                    AddStep(new InstallationStep(InstallationMode.Install));
                    AddStep(new FinishStep());
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

                        AddStep(new LicenseStep());
                        AddStep(new PreInstallProcess());
                        AddStep(new InstallationStep(InstallationMode.Install));
                        AddStep(new PostInstallProcess());
                        AddStep(new FinishStep());
                    break;
                    case InstallationMode.Uninstall:
                        // Set install mode variable ;)
                        InstallVariables.IsInstallMode = false;

                        AddStep(new PreUnInstallProcess());
                        AddStep(new InstallationStep(InstallationMode.Uninstall));
                        AddStep(new FinishStep());
                    break;
                    case InstallationMode.Upgrade:
                        AddStep(new LicenseStep());
                        AddStep(new PreUnInstallProcess());
                        AddStep(new InstallationStep(InstallationMode.Uninstall));
                        AddStep(new InstallationStep(InstallationMode.Install));
                        AddStep(new PostInstallProcess());
                        AddStep(new FinishStep());
                    break;
                    case InstallationMode.Reinstall:
                        AddStep(new LicenseStep());
                        AddStep(new PreInstallProcess()); 
                        AddStep(new InstallationStep(InstallationMode.Install));
                        AddStep(new PostInstallProcess());
                        AddStep(new FinishStep());
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
