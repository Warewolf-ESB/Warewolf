using System.Windows;
using SharpSetup.UI.Wpf.Forms.Modern;
using SharpSetup.Base;

namespace Gui
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class SetupWizard : ModernWizard
    {
        public SetupWizard()
        {
            InitializeComponent();
        }

        InstallationModeCollection GetInstallationModes(MsiInstallationModes mode)
        {
            var modes = SetupHelper.GetStandardInstallationModes(mode);
            //uncomment this line if you want to support modification mode iff reinstallation is possible
            modes.InsertBefore(InstallationMode.Reinstall, InstallationMode.Modify);
            //uncomment this line if you don't want to support reinstallation
            //modes.Remove(InstallationMode.Reinstall);
            if (modes.Contains(SetupHelper.InstallationModeFromCommandLine))
                return new InstallationModeCollection(SetupHelper.InstallationModeFromCommandLine);
            else
                return modes;
        }

        public override void LifecycleAction(string type, object argument)
        {
            if (type == LifecycleActionType.Initialization)
            {
                AddStep(new InitializationStep());
            }
            else if (type == LifecycleActionType.ConnectionOpened)
            {
                //uncomment this line if you want to support MSI property passthrough
                SetupHelper.ApplyMsiProperties();
                if (MsiConnection.Instance.IsRestored)
                {
                    AddStep(new InstallationStep(InstallationMode.Install));
                    AddStep(new FinishStep());
                }
                var modes = GetInstallationModes(MsiConnection.Instance.Mode);
                if (modes.Contains(InstallationMode.Downgrade))
                    AddStep(new FatalErrorStep(Gui.Properties.Resources.DowngradeNotSupported));
                else if (modes.Count > 1)
                    AddStep(new InstallationModeStep(modes));
                else if (modes.Count == 1)
                    AddStep(new WelcomeStep(modes[0]));
            }
            else if (type == LifecycleActionType.ModeSelected)
            {
                switch ((InstallationMode) argument)
                {
                    case InstallationMode.Install:

                        AddStep(new LicenseStep());
                        AddStep(new PreInstallStep());
                        AddStep(new InstallationStep(InstallationMode.Install));
                        AddStep(new PostInstallStep());
                        AddStep(new FinishStep());
                    break;
                    case InstallationMode.Uninstall:
                        AddStep(new InstallationStep(InstallationMode.Uninstall));
                        AddStep(new FinishStep());
                    break;
                    case InstallationMode.Upgrade:
                        AddStep(new InstallationStep(InstallationMode.Uninstall));
                        AddStep(new InstallationStep(InstallationMode.Install));
                        AddStep(new PostInstallStep());
                        AddStep(new FinishStep());
                    break;
                    case InstallationMode.Reinstall:
                        AddStep(new PreInstallStep());
                        AddStep(new InstallationStep(InstallationMode.Install));
                        AddStep(new PostInstallStep());
                        AddStep(new FinishStep());
                    break;
                    default:
                        MessageBox.Show("Mode not supported: " + (InstallationMode) argument);
                    break;
                }
            }
            else
            {
                MessageBox.Show("Unsupported lifecycle action");
            }
            base.LifecycleAction(type, argument);
        }
    }
}
