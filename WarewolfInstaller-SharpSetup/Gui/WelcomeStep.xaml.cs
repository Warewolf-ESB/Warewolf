using SharpSetup.Base;
using SharpSetup.UI.Wpf.Base;
using SharpSetup.UI.Wpf.Forms.Modern;

namespace Gui
{
    /// <summary>
    /// Interaction logic for WelcomeStep.xaml
    /// </summary>
    public partial class WelcomeStep : ModernInfoStep
    {
        InstallationMode mode;
        public WelcomeStep(InstallationMode mode)
        {
            this.mode = mode;
            InitializeComponent();
            lblMode.Text = Gui.Properties.Resources.ResourceManager.GetString("WelcomeStepGreeting" + mode.ToString()) ?? lblMode.Text;
        }

        private void WelcomeStep_MoveNext(object sender, ChangeStepRoutedEventArgs e)
        {
            Wizard.LifecycleAction(LifecycleActionType.ModeSelected, mode);
        }
    }
}
